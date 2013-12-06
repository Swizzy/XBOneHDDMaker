using System.Windows.Forms;

namespace XBOneHDDMaker
{
    using System.IO;
    using System.Reflection;

    internal sealed partial class Main : Form
    {
        public Main()
        {
            InitializeComponent();
            var ver = Assembly.GetAssembly(typeof(Program)).GetName().Version;
            Text = string.Format("{0} v{1}.{2} (Build: {3})", Assembly.GetAssembly(typeof(Program)).GetName().Name, ver.Major, ver.Minor, ver.Build);
            //FillHDDList();
        }

        private void FillHDDList()
        {
            hddlist.Items.Clear();
            foreach (var drive in System.IO.DriveInfo.GetDrives())
            {
                if (drive.IsReady && drive.DriveType == DriveType.Fixed)
                {
                    var phys = WMI.GetPhysicalDrive(drive.Name).Substring(4);
                    var num = int.Parse(phys.Substring(phys.Length - 1, 1));
                    hddlist.Items.Add(string.Format("{0} Size: {1} Model: {2} Serial: {3}", phys, Utils.GetSizeReadable(drive.TotalSize), WMI.GetModel(num), WMI.GetSerial(num)));
                }
            }
            hddlist.SelectedIndex = 0;
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            var sfd = new SaveFileDialog();

            if (sfd.ShowDialog() != DialogResult.OK)
                return;
            byte[] header;
            File.WriteAllBytes(sfd.FileName, XBOneHDD.MakeTable(805413339648, out header));
            if (sfd.ShowDialog() == DialogResult.OK)
                File.WriteAllBytes(sfd.FileName, header);
        }
    }
}
