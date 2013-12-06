using System;

namespace XBOneHDDMaker
{
    using System.Management;

    class WMI
    {
        public static string GetPhysicalDrive(string device) {
            device = device.Replace("\\", "");
            var queryResults = new ManagementObjectSearcher(string.Format("ASSOCIATORS OF {{Win32_LogicalDisk.DeviceID='{0}'}} WHERE AssocClass = Win32_LogicalDiskToPartition", device));
            foreach (var partition in queryResults.Get())
            {
                queryResults = new ManagementObjectSearcher(string.Format("ASSOCIATORS OF {{Win32_DiskPartition.DeviceID='{0}'}} WHERE AssocClass = Win32_DiskDriveToDiskPartition", partition["DeviceID"]));
                foreach (var drive in queryResults.Get())
                    return drive["DeviceID"].ToString();
            }
            throw new Exception("Not found!");
        }

        public static string GetModel(int device)
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
            var i = 0;
            foreach (var obj in searcher.Get())
            {
                if (i == device)
                    return obj["Model"].ToString();
                i++;
            }
            throw new Exception("Not found!");
        }

        public static string GetSerial(int device)
        {
            var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PhysicalMedia");
            var i = 0;
            foreach (var obj in searcher.Get())
            {
                if (i == device)
                    return obj["SerialNumber"].ToString();
                i++;
            }
            throw new Exception("Not found!");
        }
    }
}
