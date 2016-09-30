using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Thrift.IDLHelp.IDLCreate2;

namespace Thrift.IDLHelp
{
    public class ThriftBase
    {
        public bool ThriftSuccessed { get; set; }
        public int ThriftStatusCode { get; set; }
        public string ThriftMessage { get; set; }
    }

    public class Help
    {
        private static string RunCmd(string cmd)
        {
            var p = new Process
            {
                StartInfo =
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };


            p.Start();
            p.StandardInput.WriteLine(cmd);
            p.StandardInput.WriteLine("exit");

            var strrst = p.StandardOutput.ReadToEnd();

            p.Close();
            return strrst;
        }

        /// <summary>
        /// 生成
        /// </summary>
        /// <param name="filePath">生成路径</param>
        /// <param name="type">服务接口类型</param>
        /// <param name="nSpace">自定义命名空间</param>
        /// <param name="serviceName">自定义服务名</param>
        /// <returns></returns>
        public string Create(string filePath, Type type, string nSpace = "", string serviceName = "")
        {
            try
            {
                //var create = new IDLCreate();
                //var idlcode = create.Create(typeof(Thrift.Test.ITestService), "abc.ee");
                List<FunInfo> funs;
                var create2 = new IDLCreate2();
                var idlcode = create2.Create(type, out funs, nSpace, serviceName);

                var cmd = new ThriftCmd();

                Directory.CreateDirectory(filePath);

                var guid = cmd.Execute(Language.CSharp, filePath, idlcode.Item3);

                string idlpath = idlcode.Item1.Replace(".", "\\");

                string codePath = Path.Combine(filePath, guid, "Code", idlpath) + @"\*.cs";

                //替换thrift生成的代码 
                foreach (var code in Directory.GetFiles(Path.Combine(filePath, guid, "Code", idlpath)))
                {
                    FileStream fs = new FileStream(code, FileMode.Open);//打开文件
                    StreamReader tr = new StreamReader(fs, Encoding.Default);

                    string str = tr.ReadToEnd();
                    tr.Close();
                    fs.Close();

                    foreach (var fun in funs)
                    {
                        if (fun.CanReturnNull)
                        {
                            Regex regex = new Regex("throw new TApplicationException\\(TApplicationException.ExceptionType.MissingResult, \"" + fun.FunName + " failed: unknown result\"\\);", RegexOptions.IgnoreCase);
                            str = regex.Replace(str, "return null;");
                        }
                    }

                    fs = new FileStream(code, FileMode.Create);//创建文件，存在则覆盖
                    StreamWriter sw = new StreamWriter(fs);//写入

                    sw.Write(str);
                    sw.Close();
                    fs.Close();
                }

                string thriftdll = ThriftDLL.ResolvePath(Path.Combine(filePath, guid));
                string cscPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";
                string dllname = Path.Combine(filePath, guid, "Out", idlcode.Item2 + ".dll");
                string dll = $"{cscPath} /target:library /out:{dllname} /reference:{thriftdll} {codePath}";

                Console.WriteLine(RunCmd(dll));
                Console.WriteLine();

                return "生成成功：" + Path.Combine(filePath, guid, "Out");
            }
            catch (Exception ex)
            {
                throw new Exception("生成错误：" + ex.Message);
            }
        }
    }
}
