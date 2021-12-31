using System;
using System.IO;
using FileSystem.FS.Fat32;

namespace FileSystem
{
    class Filesystem
    {
        public int NodeRoot { get; private set; } 
        public Node GetNode(string path)
        {
            var image = new FileStream(path, FileMode.Open, FileAccess.Read);
            var node = new Node();
            return node;
        }
    }
   /* class NodeStream
    {
        private FileStream image;
        public NodeStream(FileStream image)
        {
            this.image=image;
        }
        
        public int Read(byte[] buffer, int offset, int length)
        {
            image.Position = offset; // image.Seek(0,SeekOrigin.begin);

            buffer = new byte[length];
            var count = image.Read(buffer, 0, length);

            return 0;
        }
        
        //private vector<Extent> extents;
        //FileStream* stream;
    };
   */ 
    class DataStore
    {
        public string Path { get; private set; }
        public DataStore(string path)
        {
            Path = path;
        }

        public Filesystem BuildFilesystem()
        {
            Filesystem fs = null;
            var fileSystemName = "Fat32";

            if (fileSystemName == "Fat32")
            {
                var fat32 = new Fat32();
                fat32.BuildFilesystem();
            }
            else if (fileSystemName == "NTFS")
            { }
            return fs;
        }
    }
    class main
    {
        static void Main(string[] args)
        {
            //var fname = "e:\\fat32.bin";
            var fname = @"C:\Users\김승주\Desktop\YKP\FAT32_simple.mdf\FAT32_simple.mdf";
            var flash = new DataStore(fname);
            var image = new FileStream(fname, FileMode.Open, FileAccess.Read);

            var fs = flash.BuildFilesystem();

            var file = fs.GetNode("/dir1/leaf.jpg");

            var buffer = new byte[100];
            file.Read(buffer, 0, 100);

            var path = "";
            if (!file.ExportTo(path))
            {
            }
        }
    }
}
