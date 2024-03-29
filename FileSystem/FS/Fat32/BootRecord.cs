﻿using MD.IO.Buffer;

namespace MD.FS.FAT32
{
    class BootRecord
    {
        public int SectorSize { get; private set; }
        public int SectorCount { get; private set; }
        public int ClusterSize { get; private set; }
        public int AddressFat { get => reservedSectorCount * SectorSize; }
        public int AddressData { get => AddressFat + (fatCount * fatSectorCount * SectorSize); }
        public int RootClusterNo { get; private set; }

        private int reservedSectorCount;
        private int fatCount;

        // change to property
        public int fatSectorCount;

        public bool IsValid { get; private set; }

        public BootRecord(byte[] buffer)
        {
            IsValid = init(buffer);
        }

        private bool init(byte[] buffer)
        {
            try
            {
                var bb = new ByteBuffer2(buffer);

                SectorSize = bb.Skip(11).GetUInt16LE();
                SectorCount = bb.GetByte();
                ClusterSize = SectorSize * SectorCount;

                reservedSectorCount = bb.GetInt16LE();
                fatCount = bb.GetByte();
                fatSectorCount = bb.Skip(19).GetInt24LE();
                RootClusterNo = bb.Skip(5).GetInt24LE();
            }
            catch
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            return string.Format("root cluster no: {0}, address of data: {1}", RootClusterNo, AddressData);
        }
    }
}
