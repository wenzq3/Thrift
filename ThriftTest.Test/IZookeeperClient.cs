﻿using org.apache.zookeeper;
using org.apache.zookeeper.data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Rabbit.Zookeeper
{
    /// <summary>
    /// 一个抽象的ZooKeeper客户端。
    /// </summary>
    public interface IZookeeperClient : IDisposable
    {
        /// <summary>
        /// 具体的ZooKeeper连接。
        /// </summary>
        ZooKeeper ZooKeeper { get; }

        /// <summary>
        /// 客户端选项。
        /// </summary>
        ZookeeperClientOptions Options { get; }

        /// <summary>
        /// 等待zk连接到具体的某一个状态。
        /// </summary>
        /// <param name="states">希望达到的状态。</param>
        /// <param name="timeout">最长等待时间。</param>
        /// <returns>如果成功则返回true，否则返回false。</returns>
        bool WaitForKeeperState(Watcher.Event.KeeperState states, TimeSpan timeout);

        /// <summary>
        /// 重试直到zk连接上。
        /// </summary>
        /// <typeparam name="T">返回类型。</typeparam>
        /// <param name="callable">执行的zk操作。</param>
        /// <returns>执行结果。</returns>
        Task<T> RetryUntilConnected<T>(Func<Task<T>> callable);

        /// <summary>
        /// 获取指定节点的数据。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <returns>节点数据。</returns>
        Task<IEnumerable<byte>> GetDataAsync(string path);

        /// <summary>
        /// 获取指定节点下的所有子节点。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <returns>子节点集合。</returns>
        Task<IEnumerable<string>> GetChildrenAsync(string path);

        /// <summary>
        /// 判断节点是否存在。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <returns>如果存在则返回true，否则返回false。</returns>
        Task<bool> ExistsAsync(string path);

        /// <summary>
        /// 创建节点。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="data">节点数据。</param>
        /// <param name="acls">权限。</param>
        /// <param name="createMode">创建模式。</param>
        /// <returns>节点路径。</returns>
        /// <remarks>
        /// 因为使用序列方式创建节点zk会修改节点name，所以需要返回真正的节点路径。
        /// </remarks>
        Task<string> CreateAsync(string path, byte[] data, List<ACL> acls, CreateMode createMode);

        /// <summary>
        /// 设置节点数据。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="data">节点数据。</param>
        /// <param name="version">版本号。</param>
        /// <returns>节点状态。</returns>
        Task<Stat> SetDataAsync(string path, byte[] data, int version = -1);

        /// <summary>
        /// 删除节点。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="version">版本号。</param>
        Task DeleteAsync(string path, int version = -1);

        /// <summary>
        /// 订阅节点数据变更。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="listener">监听者。</param>
        Task SubscribeDataChange(string path, NodeDataChangeHandler listener);

        /// <summary>
        /// 取消订阅节点数据变更。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="listener">监听者。</param>
        void UnSubscribeDataChange(string path, NodeDataChangeHandler listener);

        /// <summary>
        /// 订阅连接状态变更。
        /// </summary>
        /// <param name="listener">监听者。</param>
        void SubscribeStatusChange(ConnectionStateChangeHandler listener);

        /// <summary>
        /// 取消订阅连接状态变更。
        /// </summary>
        /// <param name="listener">监听者。</param>
        void UnSubscribeStatusChange(ConnectionStateChangeHandler listener);

        /// <summary>
        /// 订阅节点子节点变更。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="listener">监听者。</param>
        Task<IEnumerable<string>> SubscribeChildrenChange(string path, NodeChildrenChangeHandler listener);

        /// <summary>
        /// 取消订阅节点子节点变更。
        /// </summary>
        /// <param name="path">节点路径。</param>
        /// <param name="listener">监听者。</param>
        void UnSubscribeChildrenChange(string path, NodeChildrenChangeHandler listener);
    }


}