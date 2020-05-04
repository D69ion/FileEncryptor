using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;


namespace IDEAEncryprion
{
    public class IDEAEncryption
    {
        private static readonly RNGCryptoServiceProvider rNG = new RNGCryptoServiceProvider();
        private FileStream SrcFileStream { get; set; }
        private FileStream ResFileStream { get; set; }
        private FileStream KeyFileStream { get; set; }
        private string Extension { get; set; }

        public ushort[] Key { get; set; }

        public IDEAEncryption(FileStream src, FileStream res, FileStream key, string ext)
        {
            SrcFileStream = src;
            ResFileStream = res;
            KeyFileStream = key;
            Extension = ext;
        }
        /// <summary>
        /// Encrypt file with IDEA encryption
        /// </summary>
        public void Encrypt()
        {
            GenerateKeys();
            var md5 = MD5.Create().ComputeHash(SrcFileStream);

            CreateKeyFile(md5);

            //запись MD5 хеша в зашифрованный файл
            ResFileStream.Write(md5, 0, md5.Length);

            //шифрование файла
            SrcFileStream.Seek(0, SeekOrigin.Begin);
            ResFileStream.WriteByte((byte)(8 - (SrcFileStream.Length % 8)));
            for (long i = 0; i < SrcFileStream.Length; i += 8)
            {
                EncryptionRounds();
            }
            ResFileStream.Flush();
        }

        /// <summary>
        /// First 8 rounds of encryption
        /// </summary>
        private void EncryptionRounds()
        {
            byte[] data = new byte[8];
            //srcFileStream.Seek(startIndex, SeekOrigin.Begin);
            sbyte bytesCount = (sbyte)SrcFileStream.Read(data, 0, 8);
            if (bytesCount == -1)
                return;
            if (bytesCount < 8)
            {
                for (int i = bytesCount; i < 8; i++)
                {
                    data[i] = 0;
                }
            }

            //преобразование в 16 битные(2 байтные) блоки
            ushort[] blocks = new ushort[4];
            for (int i = 0; i < 4; i++)
            {
                blocks[i] = BitConverter.ToUInt16(data, i * 2);
            }

            //раунды шифрования
            for (int i = 0; i < 48; i += 6)
            {
                EncryptionRound(blocks, i);
            }
            EncryptionLastRound(blocks);

            //преобразование обратно в байты
            byte[] temp = null;
            for (int i = 0; i < 8; i += 2)
            {
                temp = BitConverter.GetBytes(blocks[i / 2]);
                data[i] = temp[0];
                data[i + 1] = temp[1];
            }

            ////запись в файл
            //if (bytesCount < 8)
            //    resFileStream.Write(data, 0, bytesCount);
            //else
            ResFileStream.Write(data, 0, 8);
        }

        /// <summary>
        /// Round of encryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        /// <param name="i">Key index</param>
        private void EncryptionRound(ushort[] blocks, int i)
        {
            if (blocks[0] == 0)
                blocks[0] = (ushort)(((long)65536 * Key[i]) % 65537);
            else
                blocks[0] = (ushort)(((long)blocks[0] * Key[i]) % 65537);
            blocks[1] = (ushort)(((long)blocks[1] + Key[i + 1]) % 65536);
            blocks[2] = (ushort)(((long)blocks[2] + Key[i + 2]) % 65536);
            if (blocks[3] == 0)
                blocks[3] = (ushort)(((long)65536 * Key[i + 3]) % 65537);
            else
                blocks[3] = (ushort)(((long)blocks[3] * Key[i + 3]) % 65537);
            ushort temp1 = (ushort)(blocks[0] ^ blocks[2]);
            ushort temp2 = (ushort)(blocks[1] ^ blocks[3]);
            if (temp1 == 0)
                temp1 = (ushort)(((long)65536 * Key[i + 4]) % 65537);
            else
                temp1 = (ushort)(((long)temp1 * Key[i + 4]) % 65537);
            temp2 = (ushort)(((long)temp1 + temp2) % 65536);
            if (temp2 == 0)
                temp2 = (ushort)(((long)65536 * Key[i + 5]) % 65537);
            else
                temp2 = (ushort)(((long)temp2 * Key[i + 5]) % 65537);
            temp1 = (ushort)((temp1 + temp2) % 65536);
            blocks[0] = (ushort)(blocks[0] ^ temp2);
            blocks[1] = (ushort)(blocks[1] ^ temp1);
            blocks[2] = (ushort)(blocks[2] ^ temp2);
            blocks[3] = (ushort)(blocks[3] ^ temp1);
            ushort t = blocks[1];
            blocks[1] = blocks[2];
            blocks[2] = t;
        }

        /// <summary>
        /// Last half round of encryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        private void EncryptionLastRound(ushort[] blocks)
        {
            ushort t = blocks[1];
            blocks[1] = blocks[2];
            blocks[2] = t;
            if (blocks[0] == 0)
                blocks[0] = (ushort)(((long)65536 * Key[48]) % 65537);
            else
                blocks[0] = (ushort)(((long)blocks[0] * Key[48]) % 65537);
            blocks[1] = (ushort)(((long)blocks[1] + Key[49]) % 65536);
            blocks[2] = (ushort)(((long)blocks[2] + Key[50]) % 65536);
            if (blocks[3] == 0)
                blocks[3] = (ushort)(((long)65536 * Key[51]) % 65537);
            else
                blocks[3] = (ushort)(((long)blocks[3] * Key[51]) % 65537);
        }

        /// <summary>
        /// Create key file
        /// </summary>
        /// <param name="keyFileStream">Output file stream of the key file</param>
        /// <param name="extension">Source file extension</param>
        /// <param name="md5">MD5 hash</param>
        private void CreateKeyFile(byte[] md5)
        {
            //запись MD5 хеша в файл с ключом
            KeyFileStream.Seek(0, SeekOrigin.Begin);
            KeyFileStream.Write(md5, 0, md5.Length);

            //запись ключей в файл с ключом
            for (int i = 0; i < 52; i++)
            {
                KeyFileStream.Write(BitConverter.GetBytes(Key[i]), 0, 2);
            }

            //запись расширения файла в зашифрованный файл
            byte[] temp = Encoding.Default.GetBytes(Extension);
            KeyFileStream.Write(temp, 0, temp.Length);

            KeyFileStream.Flush();
        }

        /// <summary>
        /// Generate new encryption key
        /// </summary>
        private void GenerateKeys()
        {
            byte[] byteKey = new byte[104];
            byte[] bytes = new byte[16];
            rNG.GetBytes(bytes);

            bytes.CopyTo(byteKey, 0);

            BitArray bitArray;
            for (int i = 1; i < 6; i++)
            {
                bitArray = new BitArray(bytes);
                bitArray = LeftShift(bitArray);
                bytes = ConvertToBytes(bitArray);
                bytes.CopyTo(byteKey, i * 16);
            }
            bitArray = new BitArray(bytes);
            bitArray = LeftShift(bitArray);
            bytes = ConvertToBytes(bitArray);
            for (int i = 0; i < 8; i++)
            {
                byteKey[i + 96] = bytes[i];
            }

            ushort[] key = new ushort[52];
            for (int i = 0; i < 52; i++)
            {
                key[i] = BitConverter.ToUInt16(byteKey, i * 2);
            }
            Key = key;
        }

        private byte[] ConvertToBytes(BitArray bitArray)
        {
            byte[] bytes = new byte[16];

            bitArray.CopyTo(bytes, 0);
            return bytes;
        }

        /// <summary>
        /// Left shift 25 characters
        /// </summary>
        /// <param name="res"></param>
        /// <returns></returns>
        private BitArray LeftShift(BitArray bitArray)
        {
            BitArray temp = new BitArray(128);
            bitArray = MirrorBits(bitArray);
            for (int i = 0; i < 25; i++)
            {
                temp[102 + i] = bitArray[i];
            }
            for (int i = 0; i < 102; i++)
            {
                temp[i] = bitArray[25 + i];
            }
            return MirrorBits(temp);
        }

        private BitArray MirrorBits(BitArray bitArray)
        {
            BitArray res = new BitArray(128);
            for (int i = 0; i < 128; i += 8)
            {
                for (int j = 0, k = 7; j < 8; j++, k--)
                {
                    res[j + i] = bitArray[k + i];
                }
            }
            return res;
        }
    }
}
