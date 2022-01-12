using System.IO;
using MD.IO.Buffer;
using System.Collections.Generic;
using System;

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
            var fs = new Filesystem();
            var root = MakeRoot();
            fs.RootNode = root;

            var allNodes = ExpandAll(root);
            fs.NodeTree = Unfold(allNodes);

            return fs;
        }

        public Node MakeRoot()
        {
            var node = new Node();
            node.Name = "/";
            node.IsDir = true;
            node.Stream = MakeNodeStream(BR.RootClusterNo);

            return node;
        }

        public Node ExpandAll(Node root)
        {
            var node = Expand(root);
            for (int i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i].IsDir)
                    ExpandAll(node.Children[i]);
            }

            return node;
        }

        public Node Expand(Node dirNode)
        {
            for (int i = 0; ; i++)
            {
                var s = dirNode.Stream;
                var buffer = s.Read(s.Position, 0x20);
                var de = new DirectoryEntry(buffer);
                
                if (de.IsEmpty)
                    break;

                if (!de.IsLfn)
                {
                    var node = MakeNode(de);

                    if (de.IsDefaultValue || de.IsVolumeName)
                        continue;

                    dirNode.Children.Add(node);
                }
                else
                {
                    dirNode.Children.Add(MakeLfn(s, de));
                }
            }

            return dirNode;
        }

        public Node MakeNode(DirectoryEntry de)
        {
            var node = new Node()
            {
                Name = de.Name,
                IsFile = de.IsFile,
                IsDir = de.IsDir,
                IsLfn = de.IsLfn,
                IsVolumeName = de.IsVolumeName,
                Stream = MakeNodeStream(de.ClusterNo)
            };

            return node;
        }

        public Node MakeLfn(Stream s, DirectoryEntry de)
        {
            var stack = new Stack<DirectoryEntry>();
            do
            {
                stack.Push(de);
                var b0 = s.Read(s.Position, 0x20);
                de = new DirectoryEntry(b0);
            }
            while (de.IsLfn);

            var node = MakeNode(de);

            var newName = "";
            while (stack.Count > 0)
            {
                var dd = stack.Pop();
                newName += dd.Name;
            }

            node.Name = newName;

            return node;
        }

        public Dictionary<string, Node> Unfold(Node node)
        {
            var result = new Dictionary<string, Node>();

            unfold(node, result);

            return result;
        }

        public void unfold(Node node, Dictionary<string, Node> all) 
        {
            foreach (var child in node.Children)
            {
                if (child.IsFile)
                    all.Add(node.Name + "/" + child.Name, child);

                if (child.IsDir)
                    unfold(child, all);
            }
        }

        public NodeStream MakeNodeStream(long clusterNo)
        {
            var extents = new List<Extent>();
            if (clusterNo == 0) 
                return new NodeStream(stream, extents);

            while (clusterNo != 0x0FFFFFFF && clusterNo != 0 && clusterNo != 0xffffff8)
            {
                var starts = BR.AddressData + (clusterNo - 2) * BR.ClusterSize;
                var extent = new Extent(starts, BR.ClusterSize);
                extents.Add(extent);
                clusterNo = Fat[clusterNo];

                if (clusterNo == 0)
                    return new NodeStream(stream, new List<Extent>());
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