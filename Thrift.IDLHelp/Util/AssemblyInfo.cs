using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.IDLHelp
{
    internal class AssemblyInfo
    {
        private const string AssemblyInfoContent = "AssemblyInfo.ini";
        private const string AssemblyInfoFileName = "AssemblyInfo.cs";

        public static string ResolvePath(string tempPath, string title, string version)
        {
            if (string.IsNullOrEmpty(version))
                version = "1.0.0.0";
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.EndsWith(AssemblyInfoContent)).FirstOrDefault();

            if (resource == null)
            {
                throw new Exception(AssemblyInfoContent + " resource not found!");
            }
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            string tempExePath = Path.Combine(tempPath, AssemblyInfoFileName);

            if (!File.Exists(tempExePath))
            {
                using (var output = File.OpenWrite(tempExePath))
                {
                    using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                    {
                        byte[] bb = new byte[stream.Length];

                        stream.Read(bb, 0, bb.Length);
                        var vs = System.Text.UTF8Encoding.UTF8.GetString(bb).Replace("{title}", title).Replace("{version}", version);

                        byte[] vsByte = UTF8Encoding.UTF8.GetBytes(vs);
                        output.Write(vsByte, 0, vsByte.Length);
                    }
                }
            }

            if (!File.Exists(tempExePath))
            {
                throw new Exception("Couldn't resolve " + AssemblyInfoContent);
            }

            return tempExePath;
        }
    }
}
