using System.IO;
using FileSystem.FS.Fat32;
using System.Collections.Generic;
using System;

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
        public bool IsDefaultValue { get; private set; }

        public int Size { get; private set; }
        public List<Node> Children { get; set; }
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
            IsDefaultValue = (Name == "." || Name == "..");
        }

        public bool ExportTo(string path, bool tryToMake=false)
        {
            var p = path.Split('\\');
            
            if (File.Exists(p[p.Length-1]))
                return false;

            using (var resultFile = new FileStream(path, FileMode.Create))
            {
                try
                { 
                    var buffer = new byte[0x1000];

                    for (int i = 0; ; i++)
                    {
                        var read = Stream.Read(buffer, 0, 0x1000);
                        if (read <= 0)
                            break;

                        resultFile.Write(buffer, 0, read);
                    }
                }
                catch
                {
                    return false;
                }
            }

            return true;
        }

        public override string ToString()
        {
            return $"name: {Name}";
        }
    }
}