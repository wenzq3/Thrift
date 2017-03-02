using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.IDLHelp
{
    public class FunInfo
    {
        public bool CanReturnNull { get; set; }
        public string ReturnType { get; set; }
        public string FunName { get; set; }

        public List<Tuple<string, string>> Parameters { get; set; }
    }

    public class TypeInfo
    {
        public string ClassName { get; set; }
        public string AssemblyName { get; set; }
        public bool IsEnum { get; set; }
    }
}
