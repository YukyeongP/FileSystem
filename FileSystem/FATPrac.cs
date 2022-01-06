using System;
using System.IO;
using System.Diagnostics;
using MD.IO.Buffer;
using System.Collections.Generic;
using FileSystem.FS.Fat32;
using System.Collections;

namespace FileSystem
{
    class Node2
    {
        public string Name { get; set; }
        public bool IsFile { get; set; }
        public bool IsDir { get; set; }
        public bool IsLfn { get; set; }
        public bool IsVolumeName { get; set; }
        public bool IsDeletedFile { get; set; }
        public int Size { get; private set; }
        public List<Node2> Children { get; private set; }

        public bool IsValid { get; private set; }

        public Node2(bool isFile)
        {
            Children = new List<Node2>();
            IsFile = isFile;
            IsDir = !isFile;
        }

        public Node2(DirectoryEntry de)
        {
            IsDir = (de.Attr & 0x10) == 0x10;
            IsFile = !IsDir;

            Children = new List<Node2>();

            IsValid = de.Attr != 0;
            IsLfn = de.Attr == 0x0f;
            IsVolumeName = de.Attr == 0x08;
            IsDeletedFile = de.Name[0] == '?';
            Name = de.Name;
        }
    }

    class Program
    {
        static List<string> GetDirNames(Stream image)
        {
            var br = ReadBr(image);
            var de = ReadDe(image, br.AddressData);
            var root = new Node2(de);

         /*   var extents = new List<Extent>();
            extents.Add(new Extent(0x400000, 0x1000));
            var rootNs = new NodeStream(image, extents);
*/
            var result = new List<string>();
            
            _GetDirNames(image, root, result);

            return result;
        }

        static void _GetDirNames(Stream image, Node2 root, List<string> result)
        {
            var dirList = new List<string>();
            for (int i = 0; ; i++)
            {
                var de = ReadDe(image, image.Position + 32 * i);

                var node = new Node2(de);

                if (!node.IsValid)
                    break;

                if (node.IsLfn || node.IsVolumeName || node.Name == "." || node.Name == "..")
                    continue;

                result.Add(node.Name);
                Console.WriteLine(node.Name);
//                dirList.Add(node.Name);
                
                if (node.IsDir)
                {
                    image.Seek(0x400000+(de.ClusterNo - 2) * 0x1000, SeekOrigin.Begin);
                    _GetDirNames(image, node, result);
                }

                /*
                var addressOfnCluster = (br.AddressData + (de.ClusterNo - 2) * br.ClusterSize) + 0x40;

                var clusterDe = ReadDe(image, addressOfnCluster);
                dirList.Add(clusterDe.Name);
                */
            }
            //result.AddRange(dirList);
        }
        
        static void TestLeafInode(Stream image)
        {            
            var leafAddress = 0x404040;

            var br = ReadBr(image);
            var de = ReadDe(image, leafAddress);
            var fat = ReadFat(image, br);

            var buffer = new byte[br.ClusterSize];
            var count = image.Read(buffer, 0, br.ClusterSize);

            //var clusters = new List<int> { de.ClusterNo };
            var addressOfnClusters = new List<int> { br.AddressData + (de.ClusterNo - 2) * br.ClusterSize };
            var currentCluster = de.ClusterNo;

            while (true)
            {
                var nextClusterNo = fat[currentCluster++];
                var addressOfnCluster = br.AddressData + (nextClusterNo - 2) * br.ClusterSize;
                if (nextClusterNo == 0xFFFFFFF)
                    break;

                //clusters.Add(nextClusterNo);
                addressOfnClusters.Add(addressOfnCluster);
            }

            //Console.WriteLine("First Address :{0}, Last :{1}", addressOfnClusters[0], addressOfnClusters[addressOfnClusters.Count - 1]);

            // cluster 시작 주소로 초기화
            image.Position = addressOfnClusters[0];
            var leafImageClusterSize = addressOfnClusters.Count * br.ClusterSize;
            var buffer2 = new byte[leafImageClusterSize];
            var count2 = image.Read(buffer2, 0, leafImageClusterSize);
            var bb = new ByteBuffer2(buffer2);
            
            var leafBinaryImage = new byte[leafImageClusterSize];
            while (true)
            {
                leafBinaryImage = bb.GetBytes(leafImageClusterSize);
                if (leafBinaryImage.Length >= leafImageClusterSize)
                    break;
            }

            using (FileStream fileStream = new FileStream(@"C:\Users\김승주\Desktop\YKP\HCcsCode\FileSystem\test.jpg", FileMode.Create))
                fileStream.Write(leafBinaryImage, 0, leafBinaryImage.Length);
        }

        static BootRecord ReadBr(Stream image)
        {
            image.Position = 0; // image.Seek(0,SeekOrigin.begin);

            var buffer = new byte[0x200];
            var count = image.Read(buffer, 0, 0x200);
            Debug.Assert(count == 0x200);

            var br = new BootRecord(buffer);
            if (!br.IsValid)
                return null;

            return br;
       }

        static DirectoryEntry ReadDe(Stream image, long addr)
        {
            image.Position = addr;

            var buffer = new byte[0x20];
            var count = image.Read(buffer, 0, 0x20);

            var de = new DirectoryEntry(buffer);

            if (!de.IsValid)
                return null;

            return de;
        }

        static int[] ReadFat(Stream image, BootRecord br)   
        {
            image.Position = br.AddressFat;

            var fatSize = br.fatSectorCount * br.SectorSize;

            var buffer = new byte[fatSize];
            var count = image.Read(buffer, 0, fatSize);
            var bb = new ByteBuffer2(buffer);

            var clusters = new int[buffer.Length / 4];

            for(int i = 0; i < clusters.Length; i++)
                clusters[i] = (int)bb.GetUInt32LE();
             
            return clusters;
        }

        static void Main1(string[] args)
        {
            //var fs = new FileStream(@"C:\Users\김승주\Desktop\YKP\FAT32_simple.mdf\FAT32_simple.mdf", FileMode.Open, FileAccess.Read);
            //var extents = new List<Extent>();
            //extents.Add(new Extent(0x400000, 0x1000));
            //var nodeStream = new NodeStream(fs, extents);
            //var buff = new byte[nodeStream.Length];
            //nodeStream.Position;
            //nodeStream.Seek(0x20, SeekOrigin.Current);
            //nodeStream.Read(buff, 0, buff.Length);

            var image = new FileStream(@"C:\Users\김승주\Desktop\YKP\FAT32_simple.mdf\FAT32_simple.mdf", FileMode.Open, FileAccess.Read);

            var br = ReadBr(image);
            Console.WriteLine("Address of Data: {0}, : Address of FAT: {1}, Root cluster Number: {2}, Cluster size: {3}",
                br.AddressData, br.AddressFat, br.RootClusterNo, br.ClusterSize);
             
           var de = ReadDe(image, br.AddressData);
            /*            Console.WriteLine("File Name: {0}, : Attribute: {1}, Cluster Number: {2}",
                            de.Name, de.Attr, de.ClusterNo);
            */
            //TestLeafInode(image);

            var res = GetDirNames(image);

         }
    }
}
