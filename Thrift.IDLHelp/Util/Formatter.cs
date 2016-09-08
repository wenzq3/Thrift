
namespace Thrift.IDLHelp
{
    internal class Formatter
    {
        static internal string FormatLanguage(Language language)
        {
            switch (language)
            {
                case Language.Cocoa:
                    return "cocoa";
                case Language.Cpp:
                    return "cpp";
                case Language.CSharp:
                    return "csharp";
                case Language.Java:
                    return "java";
                case Language.Perl:
                    return "perl";
                case Language.Php:
                    return "php";
                case Language.Python:
                    return "py";
                case Language.Ruby:
                    return "rb";
                default:
                    return string.Empty;
            }
        }
    }
}
