using System.Collections.Generic;
using System.IO;
using MD.IO.Buffer;

namespace FileSystem.FS.Fat32
{
    class Fat32
    {
        public BootRecord BR { get; private set; }
        
        // geometry
        // address: data area, fat start addr, root dir address
     
        public int[] Fat { get; private set; }

        private Stream stream;

        public Fat32(Stream s)
        {
            stream = s;
/*
            stream.Position = 0;
            var buffer = new byte[0x200];
            stream.Read(buffer, 0, 0x200);*/

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

            /*var addressOfnClusters = new List<int>();
            
            for (int i = 0; i<Fat.Length; i++)
                addressOfnClusters.Add(BR.AddressData + (Fat[i] - 2) * BR.ClusterSize);
            */
            return fs;
        }

        public Node makeRoot()
        {
            stream.Position = BR.AddressData;
            var buffer = new byte[0x20];
            stream.Read(buffer, 0, 0x20);

            //var buffer = stream.Read(BR.AddressData, 0x20);

            var de = new DirectoryEntry(buffer);

            var node = makeNode(de);

            return node;
        }

        public Node makeNode(DirectoryEntry de)
        {
            var node = new Node();
            node.Name = de.Name;
            node.IsFile = de.IsFile;
            node.IsDir = de.IsDir;
            return node;
        }

        public NodeStream makeNodeStream()
        {
            var extents = makeExtent(BR.RootClusterNo);
            var nodeStream = new NodeStream(stream, extents);
            return nodeStream;
        }

        public List<Extent> makeExtent(long clusterNo)
        {
            var extents = new List<Extent>();
            while (clusterNo != 0xFFFFFFF)
            {
                var starts = BR.AddressData + (clusterNo - 2) * BR.ClusterSize;
                var extent = new Extent(starts, BR.ClusterSize);
                extents.Add(extent);
                clusterNo = Fat[clusterNo];
            }
            return extents;
        }

        /*
        
        private Node makeRoot()
        {
            // read root Directory entry de
            var buffer = stream.Read(BR.AddressData, 0x20);
            var de = new DirectoryEntry(buffer);

            var node = makeNode(de);

            return node;
        }

        private Node makeNode(DirectoryEntry de)
        {
            //var node = new Node(de);
            
            // 1. fill meta data of node
            var node = new Node();
            //node. = de.

            // 2. make nodestream
            
            NodeStream s = makeNodeStream(de.ClusterNo);
            node.Stream = s;

            return node;
        }

        private NodeStream makeNodeStream(long clusterNo)
        {
            // make extent list

            var extents = new List<Extent>();

            while (clusterNo != 0x0FFFFF)
            {
                var start = BR.AddressData + (clusterNo - 2) * 0x1000;
                var extent = new Extent(start, 0x1000);
                extents.Add(extent);
                clusterNo = Fat[clusterNo];
            }

            return new NodeStream(stream, extents);
        }
         */
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