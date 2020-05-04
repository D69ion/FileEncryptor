using IDEAEncryprion;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace FileEncryptor
{
    public partial class SendFileForm : Form
    {
        private FileInfo SrcFileInfo { get; set; }
        private FileInfo KeyFileInfo { get; set; }
        public SendFileForm(FileInfo file)
        {
            InitializeComponent();
            SrcFileInfo = file;
            KeyFileInfo = null;
            progressBar1.Visible = false;
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            if (checkBox.Checked)
            {
                //Thread thread = new Thread(new ThreadStart(ShowMB));
                //thread.IsBackground = true;
                //thread.Start();
                using (FileStream srcFileStream = File.Open(SrcFileInfo.FullName, FileMode.Open, FileAccess.Read),
                                  resFileStream = File.Create(SrcFileInfo.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(SrcFileInfo.FullName) + ".encryp"),
                                  keyFileStream = File.Create(SrcFileInfo.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(SrcFileInfo.FullName) + ".key"))
                {
                    //вызовы функций шифрования
                    IDEAEncryption encryption = new IDEAEncryption(srcFileStream, resFileStream, keyFileStream, SrcFileInfo.Extension);
                    encryption.Encrypt();
                    SrcFileInfo = new FileInfo(SrcFileInfo.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(SrcFileInfo.FullName) + ".encryp");
                    KeyFileInfo = new FileInfo(SrcFileInfo.DirectoryName + "\\" + Path.GetFileNameWithoutExtension(SrcFileInfo.FullName) + ".key");
                }
                //thread.Abort();
            }
            try
            {
                int port = 4000;
                IPAddress iPAddress = new IPAddress(Convert.ToInt64(textBoxID.Text));
                IPEndPoint endPoint = new IPEndPoint(iPAddress, port);
                TcpClient tcpClient = new TcpClient(endPoint);
                using (Stream srcFile = SrcFileInfo.OpenRead(),
                             networkStream = tcpClient.GetStream())
                {
                    progressBar1.Visible = true;
                    progressBar1.Maximum = (int)(SrcFileInfo.Length / 2);

                    byte[] buffer = new byte[4];
                    byte[] name;

                    if (KeyFileInfo != null)
                    {
                        progressBar1.Maximum += (int)(KeyFileInfo.Length / 2);

                        networkStream.WriteByte(255);
                        name = Encoding.ASCII.GetBytes(KeyFileInfo.Name);
                        networkStream.WriteByte((byte)name.Length);
                        networkStream.Write(name, 0, name.Length);
                        networkStream.Write(BitConverter.GetBytes(KeyFileInfo.Length), 0, 8);

                        name = Encoding.ASCII.GetBytes(SrcFileInfo.Name);
                        networkStream.WriteByte((byte)name.Length);
                        networkStream.Write(name, 0, name.Length);
                        networkStream.Write(BitConverter.GetBytes(SrcFileInfo.Length), 0, 8);

                        using (Stream keyFile = KeyFileInfo.OpenRead())
                        {
                            for (long i = 0; i < KeyFileInfo.Length; i += buffer.Length)
                            {
                                //добавить шифрование
                                keyFile.Read(buffer, 0, buffer.Length);
                                networkStream.Write(buffer, 0, buffer.Length);
                                progressBar1.Value += buffer.Length / 2;
                            }
                        }
                    }
                    else
                    {
                        networkStream.WriteByte(0);
                        name = Encoding.ASCII.GetBytes(SrcFileInfo.Name);
                        networkStream.WriteByte((byte)name.Length);
                        networkStream.Write(name, 0, name.Length);
                        networkStream.Write(BitConverter.GetBytes(SrcFileInfo.Length), 0, 8);
                    }

                    for (long i = 0; i < SrcFileInfo.Length; i += buffer.Length)
                    {
                        //добавить шифрование
                        srcFile.Read(buffer, 0, buffer.Length);
                        networkStream.Write(buffer, 0, buffer.Length);
                        progressBar1.Value += buffer.Length / 2;
                    }
                }
                tcpClient.Close();
                MessageBox.Show("File succsessfully sended", "Succsess", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            this.DialogResult = DialogResult.OK;
        }

        private void ShowMB()
        {
            InfoForm form = new InfoForm("encrypting");
            form.Show();
            //MessageBox.Show("Encrypting");
        }
    }
}
