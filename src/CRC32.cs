namespace XBOneHDDMaker
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Security.Cryptography;

    internal sealed class CRC32
    {
        private readonly UInt32[] _mTable;

        public CRC32()
        {
            _mTable = InitializeTable(0xedb88320);
        }

        public UInt32 Compute(byte[] buffer)
        {
            return ~CalculateHash(_mTable, 0xffffffff, buffer, 0, buffer.Length);
        }

        private UInt32[] InitializeTable(UInt32 polynomial)
        {
            var createTable = new UInt32[256];
            for (UInt32 i = 0; i < 256; i++)
            {
                UInt32 entry = i;
                for (int j = 8; j > 0; j--) {
                    entry = (entry & 1) > 0 ? ((entry >> 1) ^ polynomial) : (entry >> 1);
                }
                createTable[i] = entry;
            }
            return createTable;
        }

        private UInt32 CalculateHash(UInt32[] table, UInt32 seed, IList<byte> buffer, int start, int size)
        {
            var crc = seed;
            for (var i = start; i < size; i++)            
                crc = (crc << 8) ^ table[(crc >> 24) ^ buffer[i]];
            return crc;
        }
    }
}