using System.IO;
using MD.FS.FAT32;

namespace MD.FS
{
    class DataStore
    {
        public string Path { get; private set; }
        public bool IsValid { get; private set; }

        public string Maker { get; set; }

        public string Model { get; set; }


        private FileStream fStream;

        public DataStore(string path)
        {
            IsValid = init(path);
        }

        public bool init(string path)
        {
            try
            {
                Path = path;
                fStream = new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                return false;
            }

            return true;
        }

        public Filesystem BuildFilesystem()
        {
            var fileSystemName = "Fat32";

            if (fileSystemName == "Fat32")
            {
                var fat32 = new Fat32(fStream);
                return fat32.BuildFilesystem();
            }

            return null;
        }

        public override string ToString()
        {
            return string.Format("Path : {0}", Path);
        }
    }
}