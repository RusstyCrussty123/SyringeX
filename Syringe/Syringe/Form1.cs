using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using gh;

namespace Syringe
{
    public partial class Form1 : Form
    {
        [DllImport("Gdi32.dll", EntryPoint = "CreateRoundRectRgn")]
        private static extern IntPtr CreateRoundRectRgn
        (
            int nLeftRect,     // x-coordinate of upper-left corner
            int nTopRect,      // y-coordinate of upper-left corner
            int nRightRect,    // x-coordinate of lower-right corner
            int nBottomRect,   // y-coordinate of lower-right corner
            int nWidthEllipse, // height of ellipse
            int nHeightEllipse // width of ellipse
        );

        bool mouseDown;
        string appVersion = "v1.1.0";
        private Point offset;

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, Width, Height, 20, 20));
            button1.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, button1.Width, button1.Height, 5, 5));
            button2.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, button2.Width, button2.Height, 5, 5));
            button3.Region = System.Drawing.Region.FromHrgn(CreateRoundRectRgn(0, 0, button3.Width, button3.Height, 5, 5));

            Process[] PC = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            comboBox1.Items.Clear();
            foreach (Process p in PC)
            {
                comboBox1.Items.Add(p.ProcessName);
            }
        }

        private void mouseDown_Event(object sender, MouseEventArgs e)
        {
            offset.X = e.X;
            offset.Y = e.Y;
            mouseDown = true;
        }

        private void mouseMove_Event(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                Point currentScreenPos = PointToScreen(e.Location);
                Location = new Point(currentScreenPos.X - offset.X, currentScreenPos.Y - offset.Y); ;
            }
        }

        private void mouseUp_Event(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void exitButton_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
            /*
            MessageBox.Show("Some text", "Some title", MessageBoxButtons.OK, MessageBoxIcon.Error);
            MessageBox.Show("Your Computer is Infected with process: NiggerKiller.trojan.\nPlease Contact: FoolTine#5189", "Microsoft Defender Alert");

            while (true)
            {
                MessageBox.Show("Send Bitcoin to this address for antivirus free. 500000 INDIAN RUPEE or else no antivirus.\nOr else NiggerKiller.trojan will be released into memory hard drive disk file repository.", "Windows 10 Defender", MessageBoxButtons.YesNo, MessageBoxIcon.Hand);
            }

            throw new Exception("NiggerKiller.trojan Has been released. Ransom please send 5000 usd to this btc address: bc1q77tjvhzw5t2z3uas5j5x572a57n9paee4atzht");
            */
        }

        private void minimizeButton_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
        }

        private string DLLP;

        private void selectDLL_Click(object sender, EventArgs e)
        {
            try
            {
                OpenFileDialog OFD = new OpenFileDialog();
                OFD.InitialDirectory = @"C:\";
                OFD.Title = "Select DLL to Inject";
                OFD.DefaultExt = "dll";
                OFD.Filter = "DLL Files(*.dll)|*.dll";
                OFD.CheckFileExists = true;
                OFD.CheckPathExists = true;
                OFD.ShowDialog();

                textBox1.Text = OFD.FileName;
                DLLP = OFD.FileName;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            DLLP = textBox1.Text;
        }

        private void refresh_Click(object sender, EventArgs e)
        {
            Process[] PC = Process.GetProcesses().Where(p => (long)p.MainWindowHandle != 0).ToArray();
            comboBox1.Items.Clear();
            foreach (Process p in PC)
            {
                comboBox1.Items.Add(p.ProcessName);
            }
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        // privileges
        const int PROCESS_CREATE_THREAD = 0x0002;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_READ = 0x0010;

        // used for memory allocation
        const uint MEM_COMMIT = 0x00001000;
        const uint MEM_RESERVE = 0x00002000;
        const uint PAGE_READWRITE = 4;

        public static int Inject(string PN, string dllName)
        {
            Process targetProcess = Process.GetProcessesByName(PN)[0];

            IntPtr procHandle = OpenProcess(PROCESS_CREATE_THREAD | PROCESS_QUERY_INFORMATION | PROCESS_VM_OPERATION | PROCESS_VM_WRITE | PROCESS_VM_READ, false, targetProcess.Id);

            if (procHandle == IntPtr.Zero) { return 3; }

            IntPtr loadLibraryAddr = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if (loadLibraryAddr == IntPtr.Zero) { return 3;}

            IntPtr allocMemAddress = VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((dllName.Length + 1) * Marshal.SizeOf(typeof(char))), MEM_COMMIT | MEM_RESERVE, PAGE_READWRITE);

            byte[] bytes = Encoding.ASCII.GetBytes(dllName);
            UIntPtr bytesWritten;
            bool processMem = WriteProcessMemory(procHandle, allocMemAddress, bytes, (uint)bytes.Length, out bytesWritten);

            if (!processMem) { return 3; }

            IntPtr thread = CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

            if (thread == IntPtr.Zero) { return 3; }

            return 4;
        }

        private void injectDLL_Click(object sender, EventArgs e)
        {
            int Result = Inject(comboBox1.Text, DLLP);

            switch (Result) 
            {
                case 1:
                    MessageBox.Show("File Doesn't Exist");
                    break;
                case 2:
                    MessageBox.Show("Process Doesn't Exist");
                    break;
                case 3:
                    MessageBox.Show("Injection Failed: " + Marshal.GetLastWin32Error());
                    break;
                case 4:
                    MessageBox.Show("DLL Was Injected", "DLL Inject Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    break;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            MessageBox.Show($"About\nVersion: {appVersion}\nMade by: github.com/RusstyCrussty123", "About SyringeX", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
