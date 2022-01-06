using FileSystem.FS.Fat32;
using System.Collections.Generic;
using System.IO;

namespace FileSystem
{
    class Node
    {
        public string Name { get; set; }
        public bool IsFile { get; set; }
        public bool IsDir { get; set; }
        public bool IsLfn { get; set; }
        public bool IsVolumeName { get; set; }
        public bool IsDeletedFile { get; set; }
        public int Size { get; private set; }
        public List<Node> Children { get; private set; }

        public bool IsValid { get; private set; }

        public NodeStream Stream { get; set; }


        public Node()
        {
            Children = new List<Node>();
        }

        public Node(bool isFile)
        {
            Children = new List<Node>();
            IsFile = isFile;
            IsDir = !isFile;
        }

        public Node(DirectoryEntry de)
        {
            IsDir = (de.Attr & 0x10) == 0x10;
            IsFile = !IsDir;

            Children = new List<Node>();

            IsValid = de.Attr != 0;
            IsLfn = de.Attr == 0x0f;
            IsVolumeName = de.Attr == 0x08;
            IsDeletedFile = de.Name[0] == '?';
            Name = de.Name;
        }

        public bool ExportTo(string path)
        {
            var resultFile = new FileStream(path, FileMode.Create);
            //resultFile.Write(resultFile, 0, resultFile.Length);

            return true;
        }
    }
}