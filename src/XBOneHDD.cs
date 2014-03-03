namespace XBOneHDDMaker
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal static class XBOneHDD
    {
        internal static byte[] MakeTable(ulong devicesize, out byte[] gptheader)
        {
            var totalsectors = (devicesize / 0x200) - 34; // Get sector count (34 sectors for MBR + GPT i guess? dunno!)
            devicesize -= 107374182400; // Subract all the static partitions
            devicesize -= 106972160; // To make it match the one made in linux (and retail HDD)
                 
            var mbr = MakeMBRHeader(); // Create a MBR...

            gptheader = ((List<byte>)MakeGPTHeader(totalsectors, totalsectors - 0x42)).ToArray(); // Account for backup sectors (twice?!)            

            var partitions = new List<byte>();

            #region Partitions

            //Now we add Temp Content...
            partitions.AddRange(MakeGPTEntry(0x800, 0x51fffff, new byte[] { 0xA5, 0x7D, 0x72, 0xB3, 0xAC, 0xA3, 0x3D, 0x4B, 0x9F, 0xD6, 0x2E, 0xA5, 0x44, 0x41, 0x01, 0x1B }, "Temp Content"));
            ulong currentsector = 0x5200000;
            //Now we add User Content... 
            partitions.AddRange(MakeGPTEntry(currentsector, currentsector + (devicesize / 0x200), new byte[] { 0xE0, 0xB5, 0x9B, 0x86, 0x56, 0x33, 0xE6, 0x4B, 0x85, 0xF7, 0x29, 0x32, 0x3A, 0x67, 0x5C, 0xC7 }, "User Content"));
            currentsector += devicesize / 0x200;
            //Now we add System Support... 
            partitions.AddRange(MakeGPTEntry(currentsector + 1, currentsector + 0x5000000, new byte[] { 0x47, 0x7A, 0x0D, 0xC9, 0xB9, 0xCC, 0xBA, 0x4C, 0x8C, 0x66, 0x04, 0x59, 0xF6, 0xB8, 0x57, 0x24 }, "System Support"));
            currentsector += 0x5000000;
            //Now we add System Update... 
            partitions.AddRange(MakeGPTEntry(currentsector + 1, currentsector + 0x1800000, new byte[] { 0xD7, 0x6A, 0x05, 0x9A, 0xED, 0x32, 0x41, 0x41, 0xAE, 0xB1, 0xAF, 0xB9, 0xBD, 0x55, 0x65, 0xDC }, "System Update"));
            currentsector += 0x1800000;
            //Now we add System Update 2... 
            partitions.AddRange(MakeGPTEntry(currentsector + 1, currentsector + 0xE00000, new byte[] { 0x7C, 0x19, 0xB2, 0x24, 0x01, 0x9D, 0xF9, 0x45, 0xA8, 0xE1, 0xDB, 0xBC, 0xFA, 0x16, 0x1E, 0xB2 }, "System Update 2"));

            partitions.AddRange(new byte[0x3D80]); // Padding

            #endregion

            var hash = BitConverter.GetBytes(CRC32.Compute(partitions.ToArray())); // Calculate the CRC32 for the Partition entry array
            Array.Copy(hash, 0, gptheader, 0x58, hash.Length); // Copy the CRC32 to the return value

            var wholegpt = new List<byte>();
            wholegpt.AddRange(gptheader);
            wholegpt.AddRange(partitions);
            hash = BitConverter.GetBytes(CRC32.Compute(wholegpt.ToArray(), 0x5c)); // Calculate the CRC32 for the GPT Header
            Array.Copy(hash, 0, gptheader, 0x10, hash.Length);
            wholegpt.Clear();
            wholegpt.AddRange(mbr);
            wholegpt.AddRange(gptheader);
            wholegpt.AddRange(partitions);
            return wholegpt.ToArray();
        }

        private static IEnumerable<byte> MakeMBRHeader()
        {
            var ret = new List<byte>();
            ret.AddRange(new byte[0x1BE]); // Padding (ussually contains bootcode)
            ret.Add(0x00); // Boot indicator
            ret.Add(0x00); // Starting Head
            ret.Add(0x02); // Starting Sector // Linux makes this 0x01
            ret.Add(0x00); // Starting Cylinder
            ret.Add(0xEE); // System ID
            ret.Add(0xFF); // Ending Head
            ret.Add(0xFF); // Ending Sector
            ret.Add(0xFF); // Ending Cylinder
            ret.AddRange(new byte[] { 0x01, 0x00, 0x00, 0x00 }); // Starting LBA
            ret.AddRange(new byte[] { 0xFF, 0xFF, 0xFF, 0xFF }); // Size (LBA) should always be 0xFF....
            ret.AddRange(new byte[0x30]); // Padding
            ret.AddRange(new byte[] { 0x55, 0xAA }); // End bytes
            return ret;
        }

        private static IEnumerable<byte> MakeGPTHeader(ulong totalsectors, ulong lastdatasector)
        {
            var ret = new List<byte>();
            ret.AddRange(new byte[] { 0x45, 0x46, 0x49, 0x20, 0x50, 0x41, 0x52, 0x54 }); //GPT Signature
            ret.AddRange(new byte[] { 0x00, 0x00, 0x01, 0x00 }); // Revision
            ret.AddRange(new byte[] { 0x5C, 0x00, 0x00, 0x00 }); // Header Size
            ret.AddRange(new byte[4]); // GPT CRC32 Header Checksum
            ret.AddRange(new byte[4]); // Reserved
            ret.AddRange(new byte[] { 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // Primary Sector
            ret.AddRange(BitConverter.GetBytes(totalsectors)); // Backup GPT Header
            ret.AddRange(new byte[] { 0x22, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // First Useable Sector
            ret.AddRange(BitConverter.GetBytes(lastdatasector)); // Last data Sector
            ret.AddRange(new byte[] { 0xDB, 0x4B, 0x34, 0xA2, 0xDE, 0xD6, 0x66, 0x47, 0x9E, 0xB5, 0x41, 0x09, 0xA1, 0x22, 0x28, 0xE5 }); // Disk GUID
            ret.AddRange(new byte[] { 0x02, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }); // First Partition Entry
            ret.AddRange(new byte[] { 0x80, 0x00, 0x00, 0x00 }); // Number of partition entries
            ret.AddRange(new byte[] { 0x80, 0x00, 0x00, 0x00 }); // Size of Partition entries
            ret.AddRange(new byte[4]); // Partition entry array CRC32
            ret.AddRange(new byte[0x1A4]);
            return ret;
        }

        private static IEnumerable<byte> MakeGPTEntry(ulong start, ulong end, IEnumerable<byte> guid, string name)
        {
            var ret = new List<byte>(); // Make buffer...
            ret.AddRange(new byte[] { 0xA2, 0xA0, 0xD0, 0xEB, 0xE5, 0xB9, 0x33, 0x44, 0x87, 0xC0, 0x68, 0xB6, 0xB7, 0x26, 0x99, 0xC7 }); // Add Type GUID
            ret.AddRange(guid); // Add Partition GUID
            ret.AddRange(BitConverter.GetBytes(start)); // Add Start sector
            ret.AddRange(BitConverter.GetBytes(end)); // Add End sector
            ret.AddRange(new byte[8]); // Add attribute bits
            var tmp = Encoding.Unicode.GetBytes(name); // Convert Name
            if (tmp.Length < 72) // Check name length
            {
                ret.AddRange(tmp); // Add unicode name
                ret.AddRange(new byte[72 - tmp.Length]); // Add trailing zero's
            }
            else if (tmp.Length > 72)
            {
                Array.Resize(ref tmp, 72); // Strip excessive characters
                ret.AddRange(tmp); // Add updated unicode name
            }
            else
                ret.AddRange(tmp); // Add unicode name as-is
            return ret; //Return the GPT Entry
        }
    }
}