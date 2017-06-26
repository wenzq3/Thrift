﻿using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Rabbit.Zookeeper.Implementation
{
    internal class NodeEntry
    {
        #region Field

        private readonly IZookeeperClient _client;

        /// <summary>
        /// 数据变更多播委托。
        /// </summary>
        private NodeDataChangeHandler _dataChangeHandler;

        /// <summary>
        /// 子节点变更多播委托。
        /// </summary>
        private NodeChildrenChangeHandler _childrenChangeHandler;

        /// <summary>
        /// 节点的快照。
        /// </summary>
        private NodeSnapshot _localSnapshot = default(NodeSnapshot);

        #endregion Field

        #region Property

        public string Path { get; }

        #endregion Property

        #region Constructor

        public NodeEntry(string path, IZookeeperClient client)
        {
            Path = path;
            _client = client;
        }

        #endregion Constructor

        #region Public Method

        public async Task<IEnumerable<byte>> GetDataAsync(bool watch = false)
        {
            var zookeeper = _client.ZooKeeper;
            var data = await zookeeper.getDataAsync(Path, watch);

            _localSnapshot.SetData(data?.Data);

            return data?.Data;
        }

        public async Task<IEnumerable<string>> GetChildrenAsync(bool watch = false)
        {
            var zookeeper = _client.ZooKeeper;
            var data = await zookeeper.getChildrenAsync(Path, watch);

            _localSnapshot.SetChildrens(data?.Children);

            return data?.Children;
        }

        public async Task<bool> ExistsAsync(bool watch = false)
        {
            var zookeeper = _client.ZooKeeper;
            var data = await zookeeper.existsAsync(Path, watch);

            var exists = data != null;

            _localSnapshot.SetExists(exists);

            return exists;
        }

        public async Task<string> CreateAsync(byte[] data, List<ACL> acls, CreateMode createMode)
        {
            var zooKeeper = _client.ZooKeeper;
            var path = await zooKeeper.createAsync(Path, data, acls, createMode);

            _localSnapshot.Create(createMode, data, acls);

            return path;
        }

        public Task<Stat> SetDataAsync(byte[] data, int version = -1)
        {
            var zooKeeper = _client.ZooKeeper;
            var stat = zooKeeper.setDataAsync(Path, data, version);

            _localSnapshot.Update(data, version);

            return stat;
        }

        public async Task DeleteAsync(int version = -1)
        {
            var zookeeper = _client.ZooKeeper;
            await zookeeper.deleteAsync(Path, version);

            _localSnapshot.Delete();
        }

        #region Listener

        public async Task SubscribeDataChange(NodeDataChangeHandler listener)
        {
            _dataChangeHandler += listener;

            //监控数据变化
            await WatchDataChange();
        }

        public void UnSubscribeDataChange(NodeDataChangeHandler listener)
        {
            _dataChangeHandler -= listener;
        }

        public async Task<IEnumerable<string>> SubscribeChildrenChange(NodeChildrenChangeHandler listener)
        {
            _childrenChangeHandler += listener;

            //监控子节点变化
            return await WatchChildrenChange();
        }

        public void UnSubscribeChildrenChange(NodeChildrenChangeHandler listener)
        {
            _childrenChangeHandler -= listener;
        }

        #endregion Listener

        #endregion Public Method

        #region Private Method

        /// <summary>
        /// 通知节点发生变化。
        /// </summary>
        /// <param name="watchedEvent">zookeeper sdk监听事件参数。</param>
        /// <param name="isFirstConnection">是否是zk第一次连接上服务器。</param>
        internal async Task OnChange(WatchedEvent watchedEvent, bool isFirstConnection)
        {
            //得到节点路径（如果只是状态发送变化则路径为null）
            var path = watchedEvent.getPath();
            //是否是zk连接状态变更
            var stateChanged = path == null;

            //如果只是状态变更则进行状态变更处理
            if (stateChanged)
            {
                await OnStatusChangeHandle(watchedEvent, isFirstConnection);
            }
            else if (path == Path) //如果变化的节点属于自己
            {
                var eventType = watchedEvent.get_Type();

                //是否属于数据变更
                var dataChanged = new[]
                {
                    Watcher.Event.EventType.NodeCreated,
                    Watcher.Event.EventType.NodeDataChanged,
                    Watcher.Event.EventType.NodeDeleted
                }.Contains(eventType);

                if (dataChanged)
                {
                    //如果子节点刚刚被创建并且该节点有注册子节点变更监听，则通知zk进行子节点监听（延迟监听）
                    if (eventType == Watcher.Event.EventType.NodeCreated && HasChildrenChangeHandler)
                        await _client.RetryUntilConnected(() => GetChildrenAsync(true));

                    //进行数据变更处理
                    await OnDataChangeHandle(watchedEvent);
                }
                else
                {
                    //进行子节点变更处理
                    await OnChildrenChangeHandle(watchedEvent);
                }
            }
        }

        /// <summary>
        /// 是否有数据变更处理者。
        /// </summary>
        private bool HasDataChangeHandler => HasHandler(_dataChangeHandler);

        /// <summary>
        /// 是否有子节点变更处理者。
        /// </summary>
        private bool HasChildrenChangeHandler => HasHandler(_childrenChangeHandler);

        /// <summary>
        /// 状态变更处理。
        /// </summary>
        /// <param name="watchedEvent"></param>
        /// <param name="isFirstConnection">是否是zk第一次连接上服务器。</param>
        private async Task OnStatusChangeHandle(WatchedEvent watchedEvent, bool isFirstConnection)
        {
            //第一次连接zk不进行通知
            if (isFirstConnection)
                return;

            //尝试恢复节点
            await RestoreEphemeral();

            if (HasDataChangeHandler)
                await OnDataChangeHandle(watchedEvent);
            if (HasChildrenChangeHandler)
                await OnChildrenChangeHandle(watchedEvent);
        }

        private async Task OnDataChangeHandle(WatchedEvent watchedEvent)
        {
            if (!HasDataChangeHandler)
                return;

            //获取当前节点最新数据的一个委托
            var getCurrentData = new Func<Task<IEnumerable<byte>>>(() => _client.RetryUntilConnected(async () =>
            {
                try
                {
                    return await GetDataAsync();
                }
                catch (KeeperException.NoNodeException) //节点不存在返回null
                {
                    return null;
                }
            }));

            //根据事件类型构建节点变更事件参数
            NodeDataChangeArgs args;
            switch (watchedEvent.get_Type())
            {
                case Watcher.Event.EventType.NodeCreated:
                    args = new NodeDataChangeArgs(Path, Watcher.Event.EventType.NodeCreated, await getCurrentData());
                    break;

                case Watcher.Event.EventType.NodeDeleted:
                    args = new NodeDataChangeArgs(Path, Watcher.Event.EventType.NodeDeleted, null);
                    break;

                case Watcher.Event.EventType.NodeDataChanged:
                case Watcher.Event.EventType.None: //重连时触发
                    args = new NodeDataChangeArgs(Path, Watcher.Event.EventType.NodeDataChanged, await getCurrentData());
                    break;

                default:
                    throw new NotSupportedException($"不支持的事件类型：{watchedEvent.get_Type()}");
            }

            await _dataChangeHandler(_client, args);

            //重新监听
            await WatchDataChange();
        }

        private async Task OnChildrenChangeHandle(WatchedEvent watchedEvent)
        {
            if (!HasChildrenChangeHandler)
                return;

            //获取当前节点最新的子节点信息
            var getCurrentChildrens = new Func<Task<IEnumerable<string>>>(() => _client.RetryUntilConnected(
                async () =>
                {
                    try
                    {
                        return await GetChildrenAsync();
                    }
                    catch (KeeperException.NoNodeException)
                    {
                        return null;
                    }
                }));

            //根据事件类型构建节点子节点变更事件参数
            NodeChildrenChangeArgs args;
            switch (watchedEvent.get_Type())
            {
                case Watcher.Event.EventType.NodeCreated:
                    args = new NodeChildrenChangeArgs(Path, Watcher.Event.EventType.NodeCreated,
                        await getCurrentChildrens());
                    break;

                case Watcher.Event.EventType.NodeDeleted:
                    args = new NodeChildrenChangeArgs(Path, Watcher.Event.EventType.NodeDeleted, null);
                    break;

                case Watcher.Event.EventType.NodeChildrenChanged:
                case Watcher.Event.EventType.None: //重连时触发
                    args = new NodeChildrenChangeArgs(Path, Watcher.Event.EventType.NodeChildrenChanged,
                        await getCurrentChildrens());
                    break;

                default:
                    throw new NotSupportedException($"不支持的事件类型：{watchedEvent.get_Type()}");
            }

            await _childrenChangeHandler(_client, args);

            //重新监听
            await WatchChildrenChange();
        }

        private async Task WatchDataChange()
        {
            await _client.RetryUntilConnected(() => ExistsAsync(true));
        }

        private async Task<IEnumerable<string>> WatchChildrenChange()
        {
            return await _client.RetryUntilConnected(async () =>
            {
                await ExistsAsync(true);
                try
                {
                    return await GetChildrenAsync(true);
                }
                catch (KeeperException.NoNodeException)
                {
                }
                return null;
            });
        }

        private static bool HasHandler(MulticastDelegate multicast)
        {
            return multicast != null && multicast.GetInvocationList().Any();
        }

        private async Task RestoreEphemeral()
        {
            //没有开启恢复
            if (!_client.Options.EnableEphemeralNodeRestore)
                return;

            //节点不存在
            if (!_localSnapshot.IsExist)
                return;

            //不是短暂的节点
            if (_localSnapshot.Mode != CreateMode.EPHEMERAL && _localSnapshot.Mode != CreateMode.EPHEMERAL_SEQUENTIAL)
                return;

            try
            {
                await _client.RetryUntilConnected(async () =>
                {
                    try
                    {
                        return await CreateAsync(_localSnapshot.Data?.ToArray(), _localSnapshot.Acls, _localSnapshot.Mode);
                    }
                    catch (KeeperException.NodeExistsException) //节点已经存在则忽略
                    {
                        return Path;
                    }
                });
            }
            catch (Exception exception)
            {
                Console.WriteLine($"恢复节点失败，异常：{exception.Message}");
            }
        }

        #endregion Private Method

        #region Help Type

        public struct NodeSnapshot
        {
            public bool IsExist { get; set; }
            public CreateMode Mode { get; set; }
            public IEnumerable<byte> Data { get; set; }
            public int? Version { get; set; }
            public List<ACL> Acls { get; set; }
            public IEnumerable<string> Childrens { get; set; }

            public void Create(CreateMode mode, byte[] data, List<ACL> acls)
            {
                IsExist = true;
                Mode = mode;
                Data = data;
                Version = -1;
                Acls = acls;
                Childrens = null;
            }

            public void Update(IEnumerable<byte> data, int version)
            {
                IsExist = true;
                Data = data;
                Version = version;
            }

            public void Delete()
            {
                IsExist = false;
                Mode = null;
                Data = null;
                Version = null;
                Acls = null;
                Childrens = null;
            }

            public void SetData(IEnumerable<byte> data)
            {
                IsExist = true;
                Data = data;
            }

            public void SetChildrens(IEnumerable<string> childrens)
            {
                IsExist = true;
                Childrens = childrens;
            }

            public void SetExists(bool exists)
            {
                if (!exists)
                {
                    Delete();
                    return;
                }
                IsExist = true;
            }
        }

        #endregion Help Type
    }
}