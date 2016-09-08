using System;
using System.IO;

namespace Thrift.IDLHelp
{
    public class ThriftCmd
    {
        public string Execute(Language language, string tempPath, string thriftIDLCode)
        {
            string formattedLanguage = Formatter.FormatLanguage(language);
            string guid = Guid.NewGuid().ToString();
            string tempPathIDL = Path.Combine(tempPath, guid, "IDL");
            string tempPathCode = Path.Combine(tempPath, guid, "Code");
            string tempPathOut = Path.Combine(tempPath, guid, "Out");

            Directory.CreateDirectory(tempPathIDL);
            Directory.CreateDirectory(tempPathCode);
            Directory.CreateDirectory(tempPathOut);

            string idlFilePath = Path.Combine(tempPathIDL, "IDL.thrift");
            string arguments = $"--gen \"{formattedLanguage}\" -out \"{tempPathCode}\" \"{idlFilePath}\"";

            File.WriteAllText(idlFilePath, thriftIDLCode);

            ThriftExe.Execute(Path.Combine( tempPath, guid), arguments);
            return guid;
        }
    }
}
