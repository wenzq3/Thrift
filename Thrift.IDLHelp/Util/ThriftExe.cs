using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Thrift.IDLHelp
{
    internal class ThriftExe
    {
        private const string thriftExeFileName = "thrift.exe";

        private static string ResolvePath(string tempPath)
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.EndsWith(thriftExeFileName)).FirstOrDefault();

            if (resource == null)
            {
                throw new Exception("thrift.exe resource not found!");
            }
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            string tempExePath = Path.Combine(tempPath, thriftExeFileName);

            if (!File.Exists(tempExePath))
            {
                using (var output = File.OpenWrite(tempExePath))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                    {
                        stream.CopyTo(output);
                    }
                }
            }

            if (!File.Exists(tempExePath))
            {
                throw new Exception("Couldn't resolve "+ thriftExeFileName);
            }

            return tempExePath;
        }

        internal static void Execute(string tempPath,string arguments)
        {
            string error = string.Empty;

            var info = new ProcessStartInfo(ResolvePath(tempPath), arguments);
            info.UseShellExecute = false;
            info.RedirectStandardError = true;

            using (var process = Process.Start(info))
            {
                process.WaitForExit(30000);
                error = process.StandardError.ReadToEnd();
            }

            if (!string.IsNullOrWhiteSpace(error))
            {
                throw new Exception(error);
            }
        }
    }
}
