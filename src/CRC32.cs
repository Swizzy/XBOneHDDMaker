namespace XBOneHDDMaker
{
    using System;

    internal sealed class CRC32
    {
        private static UInt32[] _table = null;

        private static void MakeTable()
        {
            _table = new UInt32[256];
            for (UInt32 i = 0; i < 256; i++)
            {
                var c = i;
                for (var j = 0; j < 8; j++)
                    c = (c & 1) > 0 ? (0xEDB88320 ^ (c >> 1)) : (c >> 1);
                _table[i] = c;
            }
        }

        public static UInt32 Compute(byte[] buffer, int length = 0)
        {
            if (_table == null)
                MakeTable();
            if (length == 0)
                length = buffer.Length;
            var c = 0xFFFFFFFF;
            for (var i = 0; i < length; i++)
                c = _table[(c ^ buffer[i]) & 0xFF] ^ (c >> 8);
            return c ^ 0xFFFFFFFF;
        }
    }
}