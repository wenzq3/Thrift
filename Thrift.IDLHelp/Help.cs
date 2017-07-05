using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Thrift.IDLHelp.IDLCreate_Name;

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
        /// 生成简单类名
        /// </summary>
        /// <param name="filePath">生成路径</param>
        /// <param name="type">服务接口类型</param>
        /// <param name="nSpace">自定义命名空间</param>
        /// <param name="serviceName">自定义服务名</param>
        /// <param name="dllName">dll名称</param>
        /// <returns></returns>
        public void Create(string filePath, Type type, string nSpace, string serviceName, string version = "")
        {
            var create = new IDLCreate_Name();
            var idlcode = create.Create(type, nSpace, serviceName);

            CreateFile( idlcode, filePath, nSpace, version);
        }

        private void CreateFile(Tuple<string, string, string> idlcode, string filePath, string nSpace, string version = "")
        {
            var cmd = new ThriftCmd();

            Directory.CreateDirectory(filePath);

            var guid = cmd.Execute(Language.CSharp, filePath, idlcode.Item3);

            string idlpath = idlcode.Item1.Replace(".", "\\");

            string codePath = Path.Combine(filePath, guid, "Code", idlpath) + @"\*.cs";

            string dllName = idlcode.Item1 + ".dll";
            string thriftdll = ThriftDLL.ResolvePath(Path.Combine(filePath, guid));
            string cscPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";

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
        }


        /// <summary>
        /// 生成完整类名
        /// </summary>
        /// <param name="filePath">生成路径</param>
        /// <param name="type">服务接口类型</param>
        /// <param name="nSpace">自定义命名空间</param>
        /// <param name="serviceName">自定义服务名</param>
        /// <param name="dllName">dll名称</param>
        /// <returns></returns>
        public void CreateFullName(string filePath, Type type, string nSpace, string serviceName, string version = "")
        {
            var create = new IDLCreate_FullName();
            var idlcode = create.Create(type, nSpace, serviceName);

            CreateFile(idlcode, filePath, nSpace, version);
        }


        /// <summary>
        /// 解析idl文件，并生成dll
        /// </summary>
        /// <param name="idlFilePath"></param>
        /// <param name="outPath"></param>
        /// <param name="nSpace"></param>
        /// <param name="version"></param>
        public void AnalyzeIDL(string idlFilePath, string outPath, string nSpace, string version = "")
        {
            var cmd = new ThriftCmd();

            Directory.CreateDirectory(outPath);

            string tempPathCode = Path.Combine(outPath, "Code");
            Directory.CreateDirectory(tempPathCode);
            string dllPathCode = Path.Combine(outPath, "Out");
            Directory.CreateDirectory(dllPathCode);

            string thriftdll = ThriftDLL.ResolvePath(outPath);

            string formattedLanguage = Formatter.FormatLanguage(Language.CSharp);
            string arguments = $"--gen \"{formattedLanguage}\" -out \"{tempPathCode}\" \"{idlFilePath}\"";

            ThriftExe.Execute(outPath, arguments);

            string codePath = Path.Combine(tempPathCode, nSpace.Replace(".", "\\")) + @"\*.cs";

            string cscPath = @"C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe";

            string dllname = Path.Combine(dllPathCode, nSpace + ".dll");
            string AssemblyInfoPath = AssemblyInfo.ResolvePath(outPath, nSpace, version);
            string dll = $"{cscPath} /target:library /out:{dllname} /reference:{thriftdll} {AssemblyInfoPath} {codePath}";

            Console.WriteLine(RunCmd(dll));
            Console.WriteLine();
        }

        private static string ReadTextFile(string filePath)
        {
            using (StreamReader sr = File.OpenText(filePath))
            {
                return sr.ReadToEnd();
            }
        }

    }
}
