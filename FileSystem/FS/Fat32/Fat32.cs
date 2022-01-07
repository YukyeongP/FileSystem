using System.Collections.Generic;
using System.IO;
using MD.IO.Buffer;

namespace FileSystem.FS.Fat32
{
    class Fat32
    {
        public BootRecord BR { get; private set; }
        public int[] Fat { get; private set; }
        private Stream stream;

        public Fat32(Stream s)
        {
            stream = s;

            var buffer = stream.Read(0, 0x200);
            BR = new BootRecord(buffer);

            stream.Position = BR.AddressFat;
            var fatSize = BR.fatSectorCount * BR.SectorSize;
            var buffer2 = new byte[fatSize];
            Fat = new int[buffer2.Length / 4];

            stream.Read(buffer2, 0, fatSize);
            var bb = new ByteBuffer2(buffer2);
            for (int i = 0; i < Fat.Length; i++)
                Fat[i] = (int)bb.GetUInt32LE();
        }

        public Filesystem BuildFilesystem()
        {
            // Filesystem 객체를 만들고
            // root node를 만들어서 Filesystem 객체에 넣어준다

            // root node를 만드려면 node를 만들어야 한다.
            // node를 만드려면 DE를 만들어야 한다.
            // DE를 만드려면 Data Area, Root Directory Cluster 등의 정보가 필요하다
            // 이런 정보는 BR에 있으니 BR을 해석해야 한다.

            // node에는 자식 노드, node stream이 있어야 한다
            // 

            var fs = new Filesystem();
            var root = makeRoot();
            fs.RootNode = root;
            
            /*
               for (int i = 0; ; i++)
            {
                var buffer = stream.Read(BR.AddressData + 32 * i, 0x20);
                var de = new DirectoryEntry(buffer);
                var node = makeNode(de);

                if (node.IsLfn || node.IsVolumeName || node.Name == "." || node.Name == "..")
                    continue;
                
                fs.NodeTree.Add(node.Name, node);

                if (node.IsDir)
                {
                    
                    //recursive


                }

                if (buffer[0] == 0x00)
                    break;
            }
            */
            
            return fs;
        }

        public Node makeRoot()
        {
            /*
            var buffer = stream.Read(BR.AddressData, 0x20);
            var de = new DirectoryEntry(buffer);
            var node = makeNode(de);
            */
            var node = new Node();
            node.Name = "/";
            node.IsDir = true;

            return node;
        }

        public Node makeNode(DirectoryEntry de)
        {
            var node = new Node() {
                Name = de.Name,
                IsFile = de.IsFile,
                IsDir = de.IsDir,
                IsLfn = de.IsLfn,
                IsVolumeName = de.IsVolumeName,
                Stream = makeNodeStream(de.ClusterNo)
            };

            return node;
        }

         public NodeStream makeNodeStream(long clusterNo)
        {
            var extents = new List<Extent>();
            if (clusterNo == 0) //
                return new NodeStream(stream, extents);
            //throw new Exception(); 

            while (clusterNo != 0x0FFFFFFF && clusterNo != 0 && clusterNo != 0xffffff8)
            {
                var starts = BR.AddressData + (clusterNo - 2) * BR.ClusterSize;
                var extent = new Extent(starts, BR.ClusterSize);
                extents.Add(extent);
                clusterNo = Fat[clusterNo];
                if (clusterNo == 0)
                {
                    return new NodeStream(stream, new List<Extent>());
                }
            }
            return new NodeStream(stream, extents);
        }
    }

    static class StreamExtension
    {
        public static byte[] Read(this Stream me, long offset, int size)
        {
            var buffer = new byte[size];
            me.Position = offset;
            me.Read(buffer, 0, size);

            return buffer;
        }
    }

}
