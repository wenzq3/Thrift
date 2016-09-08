using System.ComponentModel;

namespace Thrift.IDLHelp
{
    public enum Language
    {
        [Description("Cocoa")]
        Cocoa = 1,

        [Description("C++")]
        Cpp = 2,

        [Description("C#")]
        CSharp = 3,

        [Description("Java")]
        Java = 4,

        [Description("Perl")]
        Perl = 5,

        [Description("PHP")]
        Php = 6,

        [Description("Python")]
        Python = 7,

        [Description("Ruby")]
        Ruby = 8
    }
}
