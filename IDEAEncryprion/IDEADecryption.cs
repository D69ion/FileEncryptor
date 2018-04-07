using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;


namespace IDEAEncryprion
{
    public class IDEADecryption
    {
        public ushort[] Key { get; set; }
        const int mod = 65537;//2^16 + 1

        /// <summary>
        /// Decrypt the file encrypted IDEA encryption
        /// </summary>
        /// <param name="srcFileStream"></param>
        /// <param name="decryptedFileStream"></param>
        /// <param name="keyFileStream">Input file stream of the key file</param>
        public void Decrypt(FileStream srcFileStream, FileStream decryptedFileStream, FileStream keyFileStream)
        {
            GenerateKey(keyFileStream);

            for (long i = 17; i < srcFileStream.Length; i += 8)//неточность
            {
                DecryptionRounds(srcFileStream, decryptedFileStream, i);
            }
            decryptedFileStream.Flush();
        }

        /// <summary>
        /// First 8 rounds of decryption
        /// </summary>
        /// <param name="srcFileStream">Input file stream of the encrypted file</param>
        /// <param name="decryptedFileStream">Output file stream of the decrypted file</param>
        /// <param name="startIndex">The position from which the reading starts in the source file stream</param>
        private void DecryptionRounds(FileStream srcFileStream, FileStream decryptedFileStream, long startIndex)
        {
            byte[] data = new byte[8];
            srcFileStream.Seek(startIndex, SeekOrigin.Begin);
            sbyte bytescount = (sbyte)srcFileStream.Read(data, 0, 8);
            if (bytescount == -1)
                return;
            if (bytescount < 8)
            {
                for(int i = bytescount; i < 8; i++)
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
                DecryptionRound(blocks, i);
            }
            DecryptionLastRound(blocks);

            //преобразование обратно в байты
            byte[] temp = null;
            for (int i = 0; i < 8; i += 2)
            {
                temp = BitConverter.GetBytes(blocks[i / 2]);
                byte temp1 = temp[0];
                byte temp2 = temp[1];
                data[i] = temp1;
                data[i + 1] = temp2;
            }

            //запись в файл
            decryptedFileStream.Write(data, 0, 8);
        }

        /// <summary>
        /// Round of decryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        /// <param name="i">Key index</param>
        private void DecryptionRound(ushort[] blocks, int i)
        {
            if (blocks[0] == 0)
                blocks[0] = (ushort)((65536 * Key[i]) % 65537);
            else
                blocks[0] = (ushort)((blocks[0] * Key[i]) % 65537);
            blocks[1] = (ushort)((blocks[1] + Key[i + 1]) % 65536);
            blocks[2] = (ushort)((blocks[2] + Key[i + 2]) % 65536);
            if (blocks[3] == 0)
                blocks[3] = (ushort)((65536 * Key[i]) % 65537);
            else
                blocks[3] = (ushort)((blocks[3] * Key[i + 3]) % 65537);
            ushort temp1 = (ushort)(blocks[0] ^ blocks[2]);
            ushort temp2 = (ushort)(blocks[1] ^ blocks[3]);
            if (temp1 == 0)
                temp1 = (ushort)((65536 * Key[i + 4]) % 65537);
            else
                temp1 = (ushort)((temp1 * Key[i + 4]) % 65537);
            temp2 = (ushort)((temp1 + temp2) % 65536);
            if (temp2 == 0)
                temp2 = (ushort)((65536 * Key[i + 5]) % 65537);
            else
                temp2 = (ushort)((temp2 * Key[i + 5]) % 65537);
            temp1 = (ushort)((temp1 + temp2) % 65536);
            blocks[0] = (ushort)(blocks[0] ^ temp2);
            blocks[1] = (ushort)(blocks[1] ^ temp1);
            blocks[2] = (ushort)(blocks[2] ^ temp2);
            blocks[3] = (ushort)(blocks[3] ^ temp1);
        }

        /// <summary>
        /// Last half round of decryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        private void DecryptionLastRound(ushort[] blocks)
        {
            if (blocks[0] == 0)
                blocks[0] = (ushort)((65536 * Key[48]) % 65537);
            else
                blocks[0] = (ushort)((blocks[0] * Key[48]) % 65537);
            blocks[1] = (ushort)((blocks[1] + Key[49]) % 65536);
            blocks[2] = (ushort)((blocks[2] + Key[50]) % 65536);
            if (blocks[3] == 0)
                blocks[3] = (ushort)((65536 * Key[51]) % 65537);
            else
                blocks[3] = (ushort)((blocks[3] * Key[51]) % 65537);

        }

        /// <summary>
        /// Regenerate key for decryption
        /// </summary>
        /// <param name="keyFileStream">Input file stream of the key file</param>
        private void GenerateKey(FileStream keyFileStream)
        {
            Key = new ushort[52];
            keyFileStream.Position = 16;
            ushort[] keyData = new ushort[52];
            byte[] temp = new byte[2];
            for(int i = 0; i < 52; i++)
            {
                keyFileStream.Read(temp, 0, 2);
                keyData[i] = BitConverter.ToUInt16(temp, 0);
            }

            sbyte count = -1;
            for(int i = 48; i > 5; i -= 6)
            {
                Key[++count] = MultiplicativeInversion(keyData[i]);
                Key[++count] = AdditiveInversion(keyData[i + 1]);
                Key[++count] = AdditiveInversion(keyData[i + 2]);
                Key[++count] = MultiplicativeInversion(keyData[i + 3]);
                Key[++count] = keyData[i - 2];
                Key[++count] = keyData[i - 1];
            }
            Key[++count] = MultiplicativeInversion(keyData[0]);
            Key[++count] = AdditiveInversion(keyData[1]);
            Key[++count] = AdditiveInversion(keyData[2]);
            Key[++count] = MultiplicativeInversion(keyData[3]);
        }

        /// <summary>
        /// Additive inversion of a number
        /// </summary>
        /// <param name="value">Number</param>
        /// <returns></returns>
        private ushort AdditiveInversion(ushort value)
        {
            return (ushort)(65536 - value);
        }

        /// <summary>
        /// Multiplicative inversion of the number
        /// </summary>
        /// <param name="value">Number</param>
        /// <returns></returns>
        private ushort MultiplicativeInversion(ushort value)
        {
            return BinPow(value, mod - 2, mod);
        }

        /// <summary>
        /// Быстрое возведение в степень по модулю
        /// </summary>
        /// <param name="value">Number</param>
        /// <param name="pow">Pow</param>
        /// <param name="mod">Module</param>
        /// <returns></returns>
        private ushort BinPow(ushort value, int pow, int mod)
        {
            ushort res = 1;
            while (pow > 0)
            {
                if (pow % 2 == 1)
                    res = (ushort)((res * value) % mod);
                pow >>= 1;
                value = (ushort)((value * value) % mod);
            }
            return res;
        }
    }
}
