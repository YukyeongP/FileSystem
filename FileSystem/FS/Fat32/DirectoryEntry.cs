using MD.IO.Buffer;

namespace MD.FS.FAT32
{
    class DirectoryEntry
    {
        public bool IsEmpty { get; private set; }
        public bool IsFile { get; private set; }
        public bool IsDir { get; private set; }
        public bool IsLfn { get; private set; }
        public bool IsVolumeName { get; private set; }
        public bool IsDeletedFile { get; set; }
        public bool IsDefaultValue { get; private set; }
        public bool IsValid { get; private set; }
        public string Name { get; private set; }
        public int Attr { get; private set; }
        public int ClusterNo { get; private set; }
        public int fileSize { get; private set; }

        private int reserved;
        private int creationTime;
        private int createdDate;
        private int lastAccessDate;
        private string lfnName1;
        private string lfnName2;
        private string lfnName3;

        private int lfnStructCount;

        public DirectoryEntry()
        {
            IsValid = init(new byte[0x20]);
        }


        public DirectoryEntry(byte[] buffer)
        {
            IsValid = init(buffer);
        }

        private bool init(byte[] buffer)
        {
            try
            {
                var bb = new ByteBuffer2(buffer);
                IsEmpty = buffer[0] == 0;
                var attr = bb.GetByte(at: 11);

                if (attr == 0x0f)
                {
                    IsLfn = true;
                    lfnStructCount = bb.GetByte();
                    lfnName1 = bb.GetUnicode(5);
                    bb.Skip(3);
                    lfnName2 = bb.GetUnicode(6);
                    bb.Skip(2);
                    lfnName3 = bb.GetUnicode(2);
                    Name = lfnName1 + lfnName2 + lfnName3;
                }
                else
                {
                    if (bb.GetByte(at: 0) == 0xE5)
                        IsDeletedFile = true;

                    var name = bb.GetAscii(8);
                    if (name.Contains(" ")) name = name.Replace(" ", "");
                    var extension = bb.GetAscii(3);

                    Attr = bb.GetByte();
                    IsFile = (Attr & 0x20) == 0x20;
                    IsDir = (Attr & 0x10) == 0x10;
                    IsVolumeName = Attr == 0x08;

                    Name = IsFile ? name + "." + extension : name;
                    IsDefaultValue = (Name == "." || Name == "..");

                    reserved = bb.GetUInt16LE();
                    creationTime = bb.GetInt16LE();
                    createdDate = bb.GetInt16LE();
                    lastAccessDate = bb.GetInt16LE();
                    var clusterHigh = bb.GetInt16LE();
                    clusterHigh <<= 16;

                    var clusterLow = bb.Skip(4).GetInt16LE();
                    ClusterNo = clusterHigh + clusterLow;

                    fileSize = bb.GetInt32LE();
                }
            } 
            catch
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Format("Name: {0}, Attribute:{1}, Cluster Number: {2}", Name, Attr, ClusterNo);
        }
    }
}
