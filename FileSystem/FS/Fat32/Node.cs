using System;
using MD.IO.Buffer;

namespace FileSystem.FS.Fat32
{
    class Node
    {
        public string Name { get; private set; }
        public int Attr { get; private set; }
        public bool IsFile { get; private set; }
        public bool IsDir { get; private set; }
        public int ClusterNo { get; private set; }
        public bool IsValid { get; private set; }
        //public NodeStream NodeSt { get; private set; }

        private int reserved;
        private int creationTime;
        private int createdDate;
        private int lastAccessDate;
        private int fileSize;

        public Node()
        {
            IsValid = init();
        }

        private bool init()
        {
            try
            {
                var buffer = new byte[0x20];
                var bb = new ByteBuffer2(buffer);

                var name = bb.GetAscii(8);
                if (name.Contains(" "))
                    name = name.Replace(" ", "");
                var extension = bb.GetAscii(3);

                Attr = bb.GetByte();
                IsFile = (Attr & 0x20) == 0x20;
                IsDir = (Attr & 0x10) == 0x10;

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
        public bool ExportTo(string path)
        {
            return false;
        }

        public int Read(byte[] buffer, int offset, int length)
        {
            /*image.Position = offset; // image.Seek(0,SeekOrigin.begin);

            buffer = new byte[length];
            var count = image.Read(buffer, 0, length);
*/
            return 0;
        }

        public override string ToString()
        {
            return string.Format("Attribute:{0}, Cluster Number: {1}", Attr, ClusterNo);
        }
    }
}
