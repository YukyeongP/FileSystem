using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using MD.IO.Buffer;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem.FS.Fat32
{
    class RootDirectoryEntry
    {
        public bool IsFile { get; private set; }
        public string Name { get; private set; }
        public int Attr { get; private set; }
        public int ClusterNo { get; private set; }
        public bool IsValid { get; private set; }

        private int reserved;
        private int creationTime;
        private int createdDate;
        private int lastAccessDate;
        private int fileSize;

        public RootDirectoryEntry(byte[] buffer)
        {
            IsValid = init(buffer);
        }

        private bool init(byte[] buffer)
        {
            try
            {
                var bb = new ByteBuffer2(buffer);

                var name = bb.GetAscii(8);
                if (name.Contains(" "))
                    name = name.Replace(" ", "");
                var extension = bb.GetAscii(3);

                Attr = bb.GetByte();
                IsFile = (Attr & 0x20) == 0x20;

                Name = IsFile ? name + "." + extension : name;

                reserved = bb.GetUInt16LE();
                creationTime = bb.GetInt16LE();
                createdDate = bb.GetInt16LE();
                lastAccessDate = bb.GetInt16LE();
                var clusterHigh = bb.GetInt16LE();
                clusterHigh <<= 16;

                var clusterLow = bb.Skip(4).GetInt16LE();
                ClusterNo = clusterHigh + clusterLow;

                fileSize = bb.GetInt24LE();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public override string ToString()
        {
            return string.Format("Attribute:{0}, Cluster Number: {1}", Attr, ClusterNo);
        }
    }
}
