using System;

namespace MD.FS
{
    class FAT32Analyzer
    {
        static void Main(string[] args)
        {
            var fname = @"C:\Users\김승주\Desktop\YKP\FAT32_simple.mdf\FAT32_simple.mdf";
            var flash = new DataStore(fname);
            if (!flash.IsValid)
                return;

            var fs = flash.BuildFilesystem();
            var f1 = fs["dir1/leaf.jpg"];

            var path = @"C:\Users\김승주\Desktop\leaf.jpg";

            if (!f1.ExportTo(path))
            {
                throw new Exception();
            }
        }
    }
}