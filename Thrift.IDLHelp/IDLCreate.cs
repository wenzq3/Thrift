using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Thrift.IDLHelp
{


    public class IDLCreate
    {
        public static Dictionary<string, string> ditModel = new Dictionary<string, string>();
        private static List<string> listModel = new List<string>();
        private static int count = 1;

        /// <summary>
        /// 生成IDL模板
        /// </summary>
        /// <param name="type"></param>
        /// <param name="Namespace">IDL命名空间</param>
        /// <param name="serviceName">IDL服务名</param>
        /// <param name="filePath"></param>
        public Tuple<string,string, string> Create(Type type, string Namespace = "", string serviceName = "", string filePath = "")
        {
            List<TypeInfo> types = new List<TypeInfo>(); //实体类型集合
            List<FunInfo> funs = new List<FunInfo>(); //方法集合

            foreach (MethodInfo m in type.GetMethods())
            {
                FunInfo fun = new FunInfo();
                funs.Add(fun);

                fun.ReturnType = m.ReturnType.ToString();
                fun.FunName = m.Name;
                fun.Parameters = new List<Tuple<string, string>>();

                string asName = m.ReturnType.Assembly.FullName.Substring(0, m.ReturnType.Assembly.FullName.IndexOf(","));

                AddType(types, m.ReturnType.FullName, asName, m.ReturnType.BaseType == typeof(System.Enum));

                foreach (var p in m.GetParameters())
                {
                    fun.Parameters.Add(Tuple.Create(p.ParameterType.ToString(), p.Name));

                    string asPName = p.ParameterType.Assembly.FullName.Substring(0, p.ParameterType.Assembly.FullName.IndexOf(","));

                    AddType(types, p.ParameterType.FullName, asPName, p.ParameterType.BaseType == typeof(System.Enum));
                }
            }

            types.Sort((x, y) => { return y.IsEnum.CompareTo(x.IsEnum); });


            //--------------------------------------------------//

            if (string.IsNullOrEmpty(Namespace))
                Namespace = type.Namespace;

            if (string.IsNullOrEmpty(serviceName))
                serviceName = type.Name;


            foreach (var itemType in types)
            {
                if (isSystemType(itemType.ClassName))
                    continue;

                Type t = Type.GetType(itemType.ClassName + "," + itemType.AssemblyName, true);

                GetModelName(t.FullName, t.Name);
            }

            System.Text.StringBuilder str = new StringBuilder();

            str.AppendLine($"namespace csharp   {Namespace}");
            str.AppendLine($"namespace java {Namespace}");
            str.AppendLine($"namespace cpp  {Namespace}");

            str.AppendLine();


            foreach (var itemType in types)
            {
                if (isSystemType(itemType.ClassName))
                    continue;

                Type t = Type.GetType(itemType.ClassName + "," + itemType.AssemblyName, true);

                if (t.BaseType == typeof(System.Enum))
                {
                    str.AppendLine($"enum {GetModelName(t.FullName, t.Name)}");
                    str.AppendLine("{");

                    foreach (var f in t.GetFields(BindingFlags.Static | BindingFlags.Public))
                    {
                        int value = Convert.ToInt32(Enum.Parse(t, f.Name));
                        str.AppendLine($"{f.Name}={value},");
                    }

                    str.AppendLine("}");
                    str.AppendLine();
                }
                else
                {
                    str.AppendLine($"struct {GetModelName(t.FullName, t.Name)}");
                    str.AppendLine("{");

                    int i = 1;
                    foreach (var p in t.GetProperties())
                        str.AppendLine($"{i++}: optional {GetThriftType(p.PropertyType.ToString())} {p.Name}");

                    str.AppendLine("}");
                    str.AppendLine();
                }
            }

            str.AppendLine($"service {serviceName}");
            str.AppendLine("{");


            foreach (FunInfo f in funs)
            {
                str.Append(GetThriftType(f.ReturnType) + " ");
                str.Append(f.FunName + "(");

                for (int i = 0; i < f.Parameters.Count; i++)
                {
                    str.Append($"{i + 1}:");
                    str.Append(GetThriftType(f.Parameters[i].Item1) + " ");
                    str.Append(f.Parameters[i].Item2);
                    if (i < f.Parameters.Count - 1)
                        str.Append(",");
                }
                str.Append(")");
                str.AppendLine();
            }

            str.AppendLine("}");

            if (!string.IsNullOrEmpty(filePath))
            {
                FileStream fs = new FileStream(filePath, FileMode.Create);
                StreamWriter sw = new StreamWriter(fs);
                sw.WriteLine(str.ToString());
                sw.Flush();
                sw.Close();
                fs.Close();
            }

            Console.WriteLine(str.ToString());

            return Tuple.Create(Namespace, serviceName, str.ToString());
        }

        #region private

        private class FunInfo
        {
            public string ReturnType { get; set; }
            public string FunName { get; set; }

            public List<Tuple<string, string>> Parameters { get; set; }
        }

        private class TypeInfo
        {
            public string ClassName { get; set; }
            public string AssemblyName { get; set; }
            public bool IsEnum { get; set; }
        }

        private string GetModelName(string fullName, string name)
        {
            if (ditModel.ContainsKey(fullName))
                return ditModel[fullName];

            if (!listModel.Contains(name))
            {
                listModel.Add(name);
                ditModel.Add(fullName, name);
                return name;
            }
            else
                return GetModelName(fullName, name + count++);
        }

        private string GetThriftType(string type)
        {
            switch (type)
            {
                case "System.Void":
                    return "oneway void";
                case "System.String":
                    return "string";
                case "System.Int16":
                    return "i16";
                case "System.Int32":
                    return "i32";
                case "System.Int64":
                    return "i64";
                case "System.SByte":
                    return "byte";
                case "System.Boolean":
                    return "bool";
                case "System.Double":
                    return "double";
                case "System.Byte[]":
                    return "binary";

                case "System.String[]":
                    return "list<string>";
                case "System.Int16[]":
                    return "list<i16>";
                case "System.Int32[]":
                    return "list<i32>";
                case "System.Int64[]":
                    return "list<i64>";
                case "System.SByte[]":
                    return "list<byte>";
                case "System.Boolean[]":
                    return "list<bool>";
                case "System.Double[]":
                    return "list<double>";
            }

            if (type.IndexOf("System.Collections.Generic.List`1") == 0)
            {
                string p = type.Substring("System.Collections.Generic.List`1[".Length, type.Length - "System.Collections.Generic.List`1[".Length - 1);
                return $"list<{GetThriftType(p)}>";
            }

            if (type.IndexOf("System.Collections.Generic.Dictionary`2") == 0)
            {
                string[] p = type.Substring("System.Collections.Generic.Dictionary`2[".Length, type.Length - "System.Collections.Generic.Dictionary`2[".Length - 1).Split(',');
                return $"map<{GetThriftType(p[0])},{GetThriftType(p[1])}>";
            }

            if (type.IndexOf("System.Collections.Generic.ISet`1") == 0)
            {
                string p = type.Substring("System.Collections.Generic.ISet`1[".Length, type.Length - "System.Collections.Generic.ISet`1[".Length - 1);
                return $"set<{GetThriftType(p)}>";
            }

            if (type.IndexOf("System.Threading.Tasks.Task`1") == 0)
            {
                string p = type.Substring("System.Threading.Tasks.Task`1[".Length, type.Length - "System.Threading.Tasks.Task`1[".Length - 1);
                return GetThriftType(p);
            }

            if (type.Contains("System.Tuple"))
                throw new Exception("生成错误，目前不支持System.Tuple 类型！");

            return type.Replace(".", "_");
        }

        private bool isSystemType(string type)
        {
            switch (type)
            {
                case "System.Void":
                    return true;
                case "System.Type":
                    return true;
                case "System.Object":
                    return true;
                case "System.String":
                    return true;
                case "System.Int16":
                    return true;
                case "System.Int32":
                    return true;
                case "System.Int64":
                    return true;
                case "System.SByte":
                    return true;
                case "System.Boolean":
                    return true;
                case "System.Double":
                    return true;
                case "System.Byte[]":
                    return true;

                case "System.Type[]":
                    return true;
                case "System.Object[]":
                    return true;
                case "System.String[]":
                    return true;
                case "System.Int16[]":
                    return true;
                case "System.Int32[]":
                    return true;
                case "System.Int64[]":
                    return true;
                case "System.SByte[]":
                    return true;
                case "System.Boolean[]":
                    return true;
                case "System.Double[]":
                    return true;
            }

            return false;
        }

        private void AddType(List<TypeInfo> types, string className, string assemblyName, bool isEnum)
        {
            if (isSystemType(className))
                return;

            if (types.Exists(x => x.ClassName == className && x.AssemblyName == assemblyName))
                return;

            className = className.Replace(", mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", "");
            while (className.IndexOf("[") == 0)
            {
                className = className.Remove(0, 1);
                className = className.Remove(className.Length - 1, 1);
            }

            if (className.IndexOf("System.Collections.Generic.List`1[") == 0)
            {
                className = className.Remove(0, "System.Collections.Generic.List`1[".Length);
                className = className.Remove(className.Length - 1, 1);

                AddType(types, className, "mscorlib", isEnum);
                return;
            }

            if (className.IndexOf("System.Collections.Generic.Dictionary`2[") == 0)
            {
                className = className.Remove(0, "System.Collections.Generic.Dictionary`2[".Length);
                className = className.Remove(className.Length - 1, 1);

                string[] pp = className.Split(new string[] { "],[" }, StringSplitOptions.RemoveEmptyEntries);

                AddType(types, pp[0].Substring(1, pp[0].Length - 1), "mscorlib", isEnum);
                AddType(types, pp[1].Substring(0, pp[1].Length - 1), "mscorlib", isEnum);

                return;
            }

            if (className.IndexOf("System.Collections.Generic.ISet`1[") == 0)
            {
                className = className.Remove(0, "System.Collections.Generic.ISet`1[".Length);
                className = className.Remove(className.Length - 1, 1);

                AddType(types, className, "mscorlib", isEnum);
                return;
            }

            if (className.IndexOf("System.Threading.Tasks.Task`1[") == 0)
            {
                className = className.Remove(0, "System.Threading.Tasks.Task`1[".Length);
                className = className.Remove(className.Length - 1, 1);

                AddType(types, className, "mscorlib", isEnum);
                return;
            }

            if (className.Contains("System.Tuple"))
                throw new Exception("生成错误，目前不支持System.Tuple 类型！");

            if (className.IndexOf(",") > 0)
            {
                var cn = className.Substring(0, className.IndexOf(","));
                var an = className.Substring(className.IndexOf(",") + 1, className.IndexOf(",", className.IndexOf(",") + 1) - className.IndexOf(",") - 1);

                className = cn;
                assemblyName = an;
            }

            if (!types.Exists(x => x.ClassName == className && x.AssemblyName == assemblyName))
                types.Add(new TypeInfo() { ClassName = className, AssemblyName = assemblyName, IsEnum = isEnum });

            Type t = Type.GetType(className + "," + assemblyName, true);
            foreach (var p in t.GetProperties())
            {

                if (isSystemType(p.PropertyType.ToString()))
                    continue;

                string asName = p.PropertyType.Assembly.FullName.Substring(0, p.PropertyType.Assembly.FullName.IndexOf(","));
                AddType(types, p.PropertyType.FullName, asName, p.PropertyType.BaseType == typeof(System.Enum));
            }
        }

        #endregion

    }
}
