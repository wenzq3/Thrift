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
        /// <param name="dllName">dll名称</param>
        /// <returns></returns>
        public void Create(string filePath, Type type, string nSpace = "", string serviceName = "", string version = "")
        {
            //try
            //{
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

            string dllName = idlcode.Item1 + ".dll";
            string thriftdll = ThriftDLL.ResolvePath(Path.Combine(filePath, guid));
            string cscPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";
            cscPath = @"F:\ThriftTest\csc.exe";
            cscPath = Csc.ResolvePath(Path.Combine(filePath, guid));
            string dllname = Path.Combine(filePath, guid, "Out", dllName);
            string AssemblyInfoPath = AssemblyInfo.ResolvePath(Path.Combine(filePath, guid), nSpace, version);
            string dll = $"{cscPath} /target:library /out:{dllname} /reference:{thriftdll} {AssemblyInfoPath} {codePath}";


            Console.WriteLine(RunCmd(dll));
            Console.WriteLine();



            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine("生成成功：");
            Console.WriteLine();
            Console.WriteLine(Path.Combine(filePath, guid, "Out"));
            Console.WriteLine();
            Console.ForegroundColor = currentForeColor;

            Console.WriteLine("按任意键继续...");
            Console.ReadLine();
            //     return "生成成功：" + Path.Combine(filePath, guid, "Out");
            //}
            //catch (Exception ex)
            //{
            //    throw new Exception("生成错误：" + ex.Message);
            //}
        }
    }
}
