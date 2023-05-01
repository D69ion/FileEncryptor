using System;
using System.IO;
using System.Text;
using System.Windows.Forms;
using IDEAEncryprion;
using System.Threading;
using System.Net.Sockets;
using System.Net;

namespace FileEncryptor
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            buttonDecrypt.Enabled = false;
            buttonEncrypt.Enabled = false;
            buttonSend.Enabled = false;
            textBoxFileName.ReadOnly = true;
            textBoxFilePath.ReadOnly = true;
            textBoxLog.ReadOnly = true;
            comboBoxSelectEncryption.Enabled = false;
            SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\FileEncryptor";
            KeyFilePath = "";
            FileInfo = null;
            Thread thread = new Thread(new ThreadStart(StartListen));
            thread.IsBackground = true;
            thread.Start();
            textBoxID.Text = GetIP();
        }

        public FileInfo FileInfo { get; set; }

        public string SavePath { get; set; }

        public string KeyFilePath { get; set; }

        private void Button_SelectFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                FileInfo = new FileInfo(fileDialog.FileName);
                textBoxFileName.Text = FileInfo.Name;
                textBoxFilePath.Text = FileInfo.FullName;
                if (FileInfo.Extension.Equals(".encryp"))
                {
                    buttonDecrypt.Enabled = true;
                    buttonEncrypt.Enabled = false;
                    comboBoxSelectEncryption.Enabled = false;
                }
                else
                {
                    buttonEncrypt.Enabled = true;
                    buttonDecrypt.Enabled = false;
                    comboBoxSelectEncryption.Enabled = true;
                }
                textBoxLog.Text += DateTime.Now.ToString() + " Selected file: " + FileInfo.Name + "\r\n";
                buttonSend.Enabled = true;
            }
            fileDialog.Dispose();
        }

        private void ButtonEncrypt_Click(object sender, EventArgs e)
        {
            try
            {
                FileService fileService = new FileService(FileInfo, SavePath);
                fileService.UseEncryption(comboBoxSelectEncryption.Text);
            }
            catch (NotImplementedException)
            {
                MessageBox.Show("Not implemented", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //очищение textbox и полей данных
            ClearData();
            buttonEncrypt.Enabled = false;

            //вывод информации
            string info = "File was encrypted and located on: \r\n" + SavePath;
            string caption = "Encryption complete";
            textBoxLog.Text += DateTime.Now.ToString() + " " + info + "\r\n";
            MessageBox.Show(info, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void ButtonDecrypt_Click(object sender, EventArgs e)
        {
            DecryptionForm decryptionForm = new DecryptionForm(this);
            decryptionForm.ShowDialog();
            if (decryptionForm.DialogResult == DialogResult.OK)
            {
                decryptionForm.Dispose();

                try
                {
                    FileService fileService = new FileService(FileInfo, SavePath, KeyFilePath);
                    fileService.UseDecryption();
                }
                catch (NotImplementedException)
                {
                    MessageBox.Show("Not implemented", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                //очищение textbox и полей данных
                ClearData();
                buttonDecrypt.Enabled = false;

                //вывод информации
                string info = "File was decrypted and located on: \r\n" + SavePath;
                string caption = "Decyption complete";
                textBoxLog.Text += DateTime.Now.ToString() + " " + info + "\r\n";
                MessageBox.Show(info, caption, MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            decryptionForm.Dispose();
        }

        private void SaveDirectoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog browserDialog = new FolderBrowserDialog();
            //browserDialog.RootFolder = Environment.SpecialFolder.MyDocuments;
            if (browserDialog.ShowDialog() == DialogResult.OK)
            {
                SavePath = browserDialog.SelectedPath;
                textBoxLog.Text += DateTime.Now.ToString() + " Save folder is selected: " + browserDialog.SelectedPath + "\r\n";
            }
            browserDialog.Dispose();
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            SendFileForm form = new SendFileForm(FileInfo);
            form.ShowDialog();
            form.Dispose();
        }

        /// <summary>
        /// Clearing data fields
        /// </summary>
        private void ClearData()
        {
            textBoxFileName.Clear();
            textBoxFilePath.Clear();
            KeyFilePath = String.Empty;
            FileInfo = null;
        }


        private string GetIP()
        {
            string externalip = new WebClient().DownloadString("http://icanhazip.com");

            externalip = externalip.Remove(externalip.IndexOf('\n'), 1);
            string[] st = externalip.Split('.');

            byte[] vs = new byte[st.Length];
            for (int i = 0; i < vs.Length; i++)
            {
                vs[i] = Convert.ToByte(st[i]);
                Console.WriteLine(vs[i]);
            }

            uint temp = BitConverter.ToUInt32(vs, 0);
            return temp.ToString();
        }

        private void StartListen()
        {
            int port = 4000;
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), port);
            TcpListener tcpListener = new TcpListener(endPoint);
            tcpListener.Start();
            TcpClient client;
            while (true)
            {
                client = tcpListener.AcceptTcpClient();
                ReceiveFileForm receiveForm = new ReceiveFileForm(client, SavePath);
                receiveForm.ShowDialog();
                //receiveForm.Receive();
                receiveForm.Dispose();
            }
            tcpListener.Stop();
        }
    }
}
