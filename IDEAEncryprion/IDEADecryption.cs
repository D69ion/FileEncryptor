using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Cryptography;


namespace IDEAEncryprion
{
    class IDEADecryption
    {
        public ushort[] Key { get; set; }
        const int mod = 65537;//2^16 + 1

        /// <summary>
        /// Decrypt the file encrypted IDEA encryption
        /// </summary>
        /// <param name="srcFileStream"></param>
        /// <param name="decryptedFileStream"></param>
        /// <param name="keyFileStream"></param>
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
        /// <param name="srcFileStream">Input source file stream</param>
        /// <param name="decryptedFileStream">Output decrypted file stream</param>
        /// <param name="startIndex">The position from which the reading starts in the source file stream</param>
        private void DecryptionRounds(FileStream srcFileStream, FileStream decryptedFileStream, long startIndex)
        {

        }

        /// <summary>
        /// Round of decryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        /// <param name="i">Key index</param>
        private void DecryptionRound(ushort[] blocks, int i)
        {

        }

        /// <summary>
        /// Last half round of decryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        private void DecryptionLastRound(ushort[] blocks)
        {

        }

        /// <summary>
        /// Regenerate key for decryption
        /// </summary>
        /// <param name="keyFileStream">Key file streams</param>
        private void GenerateKey(FileStream keyFileStream)
        {
            keyFileStream.Position = 16;
            ushort[] keyData = new ushort[52];
            byte[] temp = null;
            for(int i = 0; i < 52; i++)
            {
                keyFileStream.Read(temp, 0, 2);
                keyData[i] = BitConverter.ToUInt16(temp, 0);
            }

            sbyte count = -1;
            for(int i = 50; i > 8; i -= 6)
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
