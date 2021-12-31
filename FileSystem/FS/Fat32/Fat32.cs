using System.IO;
using MD.IO.Buffer;

namespace FileSystem.FS.Fat32
{
    class Fat32
    {
        public BootRecord BR { get; private set; }

        public Fat32()
        {

        }

        public Filesystem BuildFilesystem()
        {
            var fs = new Filesystem();
            return fs;
        }
        static int[] ReadFat(Stream image, BootRecord br)
        {
            image.Position = br.AddressFat;

            var fatSize = br.fatSectorCount * br.SectorSize;

            var buffer = new byte[fatSize];
            var count = image.Read(buffer, 0, fatSize);
            var bb = new ByteBuffer2(buffer);

            var clusters = new int[buffer.Length / 4];

            for (int i = 0; i < clusters.Length; i++)
                clusters[i] = (int)bb.GetUInt32LE();

            return clusters;
        }
    }
}