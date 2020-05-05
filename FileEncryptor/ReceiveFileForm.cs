using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileEncryptor
{
    public partial class ReceiveFileForm : Form
    {
        private TcpClient Client { get; set; }
        private string SavePath { get; set; }

        public ReceiveFileForm(TcpClient client, string savePath)
        {
            InitializeComponent();
            Client = client;
            SavePath = savePath;
            Receive();
        }

        public void Receive()
        {
            try
            {
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider())
                using (AesCryptoServiceProvider aes = new AesCryptoServiceProvider())
                using (NetworkStream netStream = Client.GetStream())
                using (CryptoStream cryptoNetStream = new CryptoStream(netStream, aes.CreateDecryptor(), CryptoStreamMode.Read))
                {
                    //получение открытого ключа
                    RSAParameters rSAParameters = new RSAParameters();
                    netStream.Read(rSAParameters.Modulus, 0, 128);
                    netStream.Read(rSAParameters.Exponent, 0, 3);
                    rsa.ImportParameters(rSAParameters);
                    //шифрование и отправка сеансового ключа открытым ключом
                    byte[] encrData, sessionKey = new byte[48];
                    aes.Key.CopyTo(sessionKey, 0);
                    aes.IV.CopyTo(sessionKey, 32);
                    encrData = rsa.Encrypt(sessionKey, false);
                    netStream.Write(encrData, 0, encrData.Length);

                    string keyfileName = string.Empty,
                           fileName = string.Empty;
                    long keyfileLength = 0,
                         fileLength = 0;
                    byte[] name = null,
                           number = new byte[8],
                           buffer = new byte[16];
                    int bytesCount = 0;

                    if (netStream.ReadByte() == 255)
                    {
                        name = new byte[netStream.ReadByte()];
                        netStream.Read(name, 0, name.Length);
                        keyfileName = Encoding.ASCII.GetString(name);
                        netStream.Read(number, 0, 8);
                        keyfileLength = BitConverter.ToInt64(number, 0);

                        name = new byte[netStream.ReadByte()];
                        netStream.Read(name, 0, name.Length);
                        fileName = Encoding.ASCII.GetString(name);
                        netStream.Read(number, 0, 8);
                        fileLength = BitConverter.ToInt64(number, 0);

                        progressBar1.Maximum = (int)((keyfileLength + fileLength) / 2);

                        using (FileStream keyfileStream = File.Create(SavePath + "\\" + keyfileName))
                        {
                            for (long i = 0; i < keyfileLength; i += buffer.Length)
                            {
                                bytesCount = cryptoNetStream.Read(buffer, 0, buffer.Length);
                                keyfileStream.Write(buffer, 0, bytesCount);
                                progressBar1.Value += buffer.Length / 2;
                            }
                            keyfileStream.SetLength(keyfileLength);
                        }
                    }
                    else
                    {
                        name = new byte[netStream.ReadByte()];
                        netStream.Read(name, 0, name.Length);
                        fileName = Encoding.ASCII.GetString(name);
                        netStream.Read(number, 0, 8);
                        fileLength = BitConverter.ToInt64(number, 0);
                    }
                    using (FileStream fileStream = File.Create(SavePath + "\\" + fileName))
                    {
                        for (long i = 0; i < fileLength; i += buffer.Length)
                        {
                            bytesCount = cryptoNetStream.Read(buffer, 0, buffer.Length);
                            fileStream.Write(buffer, 0, bytesCount);
                            progressBar1.Value += buffer.Length / 2;
                        }
                        fileStream.SetLength(fileLength);
                    }
                }
                Client.Close();
                MessageBox.Show("File succsessfully received", "Succsess", MessageBoxButtons.OK, MessageBoxIcon.None);
            }
            catch (Exception exp)
            {
                MessageBox.Show(exp.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.DialogResult = DialogResult.OK;
        }
    }
}
