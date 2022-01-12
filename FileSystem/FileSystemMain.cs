using System;

namespace FileSystem
{
    class main
    {
        static void Main(string[] args)
        {
            var fname = @"C:\Users\김승주\Desktop\YKP\FAT32_simple.mdf\FAT32_simple.mdf";
            var flash = new DataStore(fname);
            if (!flash.IsValid)
                return;

            var fs = flash.BuildFilesystem();
            
            var file = fs.GetNode("dir1/leaf.jpg"); 

            var path = @"C:\Users\김승주\Desktop\leaf.jpg";
            if (!file.ExportTo(path))
            {
                throw new Exception();
            }
        }
    }
}