using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Thrift.IDLHelp
{
    internal class ThriftDLL
    {
        private const string thriftDLLFileName = "Thrift.dll";

        public static string ResolvePath(string tempPath)
        {
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.EndsWith(thriftDLLFileName)).FirstOrDefault();

            if (resource == null)
            {
                throw new Exception("Thrift.dll resource not found!");
            }
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            string tempExePath = Path.Combine(tempPath, thriftDLLFileName);

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
                throw new Exception("Couldn't resolve Thrift.dll ");
            }

            return tempExePath;
        }
    }
}
