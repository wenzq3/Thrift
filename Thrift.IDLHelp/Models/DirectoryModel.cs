using System;
using System.Collections.Generic;
using System.IO;

namespace Thrift.IDLHelp
{
    public class DirectoryModel
    {
        #region Constructors

        public DirectoryModel(string name)
        {
            Name = name;
            Files = new List<FileModel>();
            Directories = new List<DirectoryModel>();
        }

        #endregion

        #region Properties

        public string Name { get; private set; }

        public IList<FileModel> Files { get; private set; }

        public IList<DirectoryModel> Directories { get; private set; }

        #endregion

        #region Methods

        private void FillFromFileSystemRecursive(string path, DirectoryModel directory)
        {
            foreach (var filePath in Directory.GetFiles(path))
            {
                var bytes = File.ReadAllBytes(filePath);
                var base64 = Convert.ToBase64String(bytes);
                var fileName = Path.GetFileName(filePath);

                directory.AddFile(new FileModel(fileName, base64));
            }

            foreach (var childDirectoryPath in Directory.GetDirectories(path))
            {
                var childDirectory = new DirectoryModel(Path.GetFileName(childDirectoryPath));
                FillFromFileSystemRecursive(childDirectoryPath, childDirectory);

                directory.AddDirectory(childDirectory);
            }
        }

        private void CreateOnFileSystemRecursive(string path, DirectoryModel directory)
        {
            if (directory.Files != null)
            {
                foreach (var file in directory.Files)
                {
                    var filePath = Path.Combine(path, file.Name);
                    var content = Convert.FromBase64String(file.Content);

                    File.WriteAllBytes(filePath, content);
                }
            }

            if (directory.Directories != null)
            {
                foreach (var childDirectory in directory.Directories)
                {
                    var childDirectoryPath = Path.Combine(path, childDirectory.Name);
                    Directory.CreateDirectory(childDirectoryPath);

                    CreateOnFileSystemRecursive(childDirectoryPath, childDirectory);
                }
            }
        }

        public void FillFromFileSystem(string path)
        {
            FillFromFileSystemRecursive(path, this);
        }

        public string CreateOnFileSystem(string path)
        {
            var newPath = Path.Combine(path, Name);
            Directory.CreateDirectory(newPath);

            CreateOnFileSystemRecursive(newPath, this);

            return newPath;
        }

        private void GetFilesRecursive(List<FileModel> files, DirectoryModel directory)
        {
            files.AddRange(directory.Files);

            if (directory.Directories.Count > 0)
            {
                foreach (var childDirectory in Directories)
                {
                    GetFilesRecursive(files, childDirectory);
                }
            }
        }

        public IList<FileModel> GetFiles(bool recursive)
        {
            if (!recursive)
            {
                return Files;
            }

            var list = new List<FileModel>();

            GetFilesRecursive(list, this);

            return list;
        }

        public void AddFile(FileModel file) => Files.Add(file);

        public void AddDirectory(DirectoryModel directory) => Directories.Add(directory);

        #endregion
    }
}