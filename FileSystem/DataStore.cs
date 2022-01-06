using System.IO;
using FileSystem.FS.Fat32;

namespace FileSystem
{
    class DataStore
    {
        public string Path { get; private set; }
        public bool IsValid { get; private set; }
        
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
            Filesystem fs = null;
            var fileSystemName = "Fat32";

            if (fileSystemName == "Fat32")
            {
                var fat32 = new Fat32(fStream);
                fs = fat32.BuildFilesystem();
                //
            }

            return fs;
        }
    }
}