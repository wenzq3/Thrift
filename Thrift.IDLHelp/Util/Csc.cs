using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.IDLHelp
{
    internal class Csc
    {
        private const string cscExe = "csc.exe";
        private const string cscuiDll = "cscui.dll";

        public static string ResolvePath(string tempPath)
        {
            ResolvePath(tempPath, cscuiDll);
            return ResolvePath(tempPath, cscExe);
        }

        private static string ResolvePath(string tempPath, string fileName)
        {
            //cscuidll
            var resource = Assembly.GetExecutingAssembly().GetManifestResourceNames().Where(x => x.EndsWith(fileName)).FirstOrDefault();

            if (resource == null)
            {
                throw new Exception("Thrift.dll resource not found!");
            }
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var tempExePath = Path.Combine(tempPath, fileName);

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
                throw new Exception("Couldn't resolve " + fileName);
            }

            return tempExePath;
        }
    }
}
