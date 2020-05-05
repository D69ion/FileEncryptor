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
using System.Security.Cryptography;
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

                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                using (NetworkStream networkStream = tcpClient.GetStream())
                using (CryptoStream cryptoNetStream = new CryptoStream(networkStream, aes.CreateEncryptor(), CryptoStreamMode.Write))
                {
                    //отправка открытого ключа
                    RSAParameters rSAParameters = rsa.ExportParameters(false);
                    networkStream.Write(rSAParameters.Modulus, 0, 128);
                    networkStream.Write(rSAParameters.Exponent, 0, 3);
                    //получение и дешифровка сеансового ключа
                    byte[] encrData = new byte[128],
                           sessionKey;
                    networkStream.Read(encrData, 0, encrData.Length);
                    sessionKey = rsa.Decrypt(encrData, false);
                    //инициализация сеансовго ключа
                    for (int i = 0; i < 32; i++)
                        aes.Key[i] = sessionKey[i];
                    for (int i = 32; i < 48; i++)
                        aes.IV[i - 32] = sessionKey[i];

                    progressBar1.Visible = true;
                    progressBar1.Maximum = (int)(SrcFileInfo.Length / 2);

                    byte[] buffer = new byte[16];
                    byte[] name;
                    int bytesCount = 0;

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
                                bytesCount = keyFile.Read(buffer, 0, buffer.Length);
                                cryptoNetStream.Write(buffer, 0, bytesCount);
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
                    using (FileStream srcFile = SrcFileInfo.OpenRead())
                    {
                        for (long i = 0; i < SrcFileInfo.Length; i += buffer.Length)
                        {
                            //добавить шифрование
                            bytesCount = srcFile.Read(buffer, 0, buffer.Length);
                            cryptoNetStream.Write(buffer, 0, bytesCount);
                            progressBar1.Value += buffer.Length / 2;
                        }
                    }
                }
                tcpClient.Close();
                KeyFileInfo.Delete();
                SrcFileInfo.Delete();
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
