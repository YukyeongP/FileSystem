using System;
using System.IO;
using System.Diagnostics;
using MD.IO.Buffer;
using System.Collections.Generic;
using FileSystem.FS.Fat32;

namespace FileSystem
{
    class Program
    {
        /*        static void TestBr(Stream image)
            {
                //readDr
                var br = ReadBr(image);

                if (!br.IsValid)
                    return;

                Debug.Assert(br.SectorSize == 512);
                Debug.Assert(br.SectorCount == 8);
            }*/
        /*  static void TestDir1Inode(Stream image)
            {
                image.Position = 0x400000;

                var buffer = new byte[0x1000];
                var count = image.Read(buffer, 0, 0x1000);

                var de = new DirectoryEntry(buffer);

                if (!de.IsValid)
                    return;
            }
    */
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

        static RootDirectoryEntry ReadDe(Stream image, long addr)
        {
            image.Position = addr;

            var buffer = new byte[0x20];
            var count = image.Read(buffer, 0, 0x20);

            var de = new RootDirectoryEntry(buffer);

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
            var path = @"C:\Users\김승주\Desktop\YKP\FAT32_simple.mdf\FAT32_simple.mdf";
            var image = new FileStream(path, FileMode.Open, FileAccess.Read);
            
            var br = ReadBr(image);
            Console.WriteLine("Address of Data: {0}, : Address of FAT: {1}, Root cluster Number: {2}, Cluster size: {3}", 
                br.AddressData, br.AddressFat, br.RootClusterNo, br.ClusterSize);

            var de = ReadDe(image, br.AddressData);
            Console.WriteLine("File Name: {0}, : Attribute: {1}, Cluster Number: {2}",
                de.Name, de.Attr, de.ClusterNo);

            TestLeafInode(image);
        }
    }
}
