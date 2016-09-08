namespace Thrift.IDLHelp
{
    public class FileModel
    {
        #region Constructors

        public FileModel()
        {

        }

        public FileModel(string name, string content)
        {
            Name = name;
            Content = content;
        }

        #endregion

        #region Properties

        public string Name { get; private set; }

        public string Content { get; private set; }

        #endregion
    }
}
