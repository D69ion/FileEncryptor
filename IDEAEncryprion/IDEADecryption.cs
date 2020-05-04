using System;
using System.IO;
using System.Numerics;


namespace IDEAEncryprion
{
    public class IDEADecryption
    {
        public ushort[] Key { get; set; }
        const int mod = 65537; //2^16 + 1
        private FileStream SrcFileStream { get; set; }
        private FileStream ResFileStream { get; set; }
        private FileStream KeyFileStream { get; set; }

        public IDEADecryption(FileStream src, FileStream res, FileStream key)
        {
            SrcFileStream = src;
            ResFileStream = res;
            KeyFileStream = key;
        }
        /// <summary>
        /// Decrypt the file encrypted IDEA encryption
        /// </summary>
        /// <param name="SrcFileStream">Input file stream of the encrypted file</param>
        /// <param name="ResFileStream">Output file stream of the decrypted file</param>
        /// <param name="KeyFileStream">Input file stream of the key file</param>
        public void Decrypt()
        {
            GenerateKey();

            SrcFileStream.Seek(17, SeekOrigin.Begin);
            ResFileStream.Seek(0, SeekOrigin.Begin);
            for (long i = 17; i < SrcFileStream.Length; i += 8)
            {
                DecryptionRounds();
            }
            SrcFileStream.Seek(16, SeekOrigin.Begin);
            byte offset = (byte)SrcFileStream.ReadByte();
            if (!(offset == 8))
                ResFileStream.SetLength(ResFileStream.Length - SrcFileStream.ReadByte());
            ResFileStream.Flush();
        }

        /// <summary>
        /// First 8 rounds of decryption
        /// </summary>
        /// <param name="srcFileStream">Input file stream of the encrypted file</param>
        /// <param name="resFileStream">Output file stream of the decrypted file</param>
        /// <param name="startIndex">The position from which the reading starts in the source file stream</param>
        private void DecryptionRounds()
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

            //раунды дешифрования
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
                data[i] = temp[0];
                data[i + 1] = temp[1];
            }

            ResFileStream.Write(data, 0, 8);
        }

        /// <summary>
        /// Round of decryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        /// <param name="i">Key index</param>
        private void DecryptionRound(ushort[] blocks, int i)
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
            temp1 = (ushort)(((long)temp1 + temp2) % 65536);
            blocks[0] = (ushort)(blocks[0] ^ temp2);
            blocks[1] = (ushort)(blocks[1] ^ temp1);
            blocks[2] = (ushort)(blocks[2] ^ temp2);
            blocks[3] = (ushort)(blocks[3] ^ temp1);
            ushort t = blocks[1];
            blocks[1] = blocks[2];
            blocks[2] = t;
        }

        /// <summary>
        /// Last half round of decryption
        /// </summary>
        /// <param name="blocks">Data blocks</param>
        private void DecryptionLastRound(ushort[] blocks)
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
        /// Regenerate key for decryption
        /// </summary>
        /// <param name="KeyFileStream">Input file stream of the key file</param>
        private void GenerateKey()
        {
            Key = new ushort[52];
            KeyFileStream.Seek(16, SeekOrigin.Begin);
            ushort[] keyData = new ushort[52];
            byte[] temp = new byte[104];
            KeyFileStream.Read(temp, 0, 104);
            for (int i = 0; i < 52; i++)
            {
                keyData[i] = BitConverter.ToUInt16(temp, i * 2);
            }

            sbyte count = -1;
            Key[++count] = MultiplicativeInversion(keyData[48]);
            Key[++count] = AdditiveInversion(keyData[49]);
            Key[++count] = AdditiveInversion(keyData[50]);
            Key[++count] = MultiplicativeInversion(keyData[51]);
            Key[++count] = keyData[46];
            Key[++count] = keyData[47];

            //Key[++count] = MultiplicativeInversion(keyData[42]);
            //Key[++count] = AdditiveInversion(keyData[44]);
            //Key[++count] = AdditiveInversion(keyData[43]);
            //Key[++count] = MultiplicativeInversion(keyData[45]);
            //Key[++count] = keyData[40];
            //Key[++count] = keyData[41];
            //Key[++count] = MultiplicativeInversion(keyData[36]);
            //Key[++count] = AdditiveInversion(keyData[38]);
            //Key[++count] = AdditiveInversion(keyData[37]);
            //Key[++count] = MultiplicativeInversion(keyData[39]);
            //Key[++count] = keyData[34];
            //Key[++count] = keyData[35];
            //Key[++count] = MultiplicativeInversion(keyData[30]);
            //Key[++count] = AdditiveInversion(keyData[32]);
            //Key[++count] = AdditiveInversion(keyData[31]);
            //Key[++count] = MultiplicativeInversion(keyData[33]);
            //Key[++count] = keyData[28];
            //Key[++count] = keyData[29];
            //Key[++count] = MultiplicativeInversion(keyData[24]);
            //Key[++count] = AdditiveInversion(keyData[26]);
            //Key[++count] = AdditiveInversion(keyData[25]);
            //Key[++count] = MultiplicativeInversion(keyData[27]);
            //Key[++count] = keyData[22];
            //Key[++count] = keyData[23];
            //Key[++count] = MultiplicativeInversion(keyData[18]);
            //Key[++count] = AdditiveInversion(keyData[20]);
            //Key[++count] = AdditiveInversion(keyData[19]);
            //Key[++count] = MultiplicativeInversion(keyData[21]);
            //Key[++count] = keyData[16];
            //Key[++count] = keyData[17];
            //Key[++count] = MultiplicativeInversion(keyData[12]);
            //Key[++count] = AdditiveInversion(keyData[14]);
            //Key[++count] = AdditiveInversion(keyData[13]);
            //Key[++count] = MultiplicativeInversion(keyData[15]);
            //Key[++count] = keyData[10];
            //Key[++count] = keyData[11];
            //Key[++count] = MultiplicativeInversion(keyData[6]);
            //Key[++count] = AdditiveInversion(keyData[8]);
            //Key[++count] = AdditiveInversion(keyData[7]);
            //Key[++count] = MultiplicativeInversion(keyData[9]);
            //Key[++count] = keyData[4];
            //Key[++count] = keyData[5];

            for (int i = 42; i > 5; i -= 6)
            {
                Key[++count] = MultiplicativeInversion(keyData[i]);
                Key[++count] = AdditiveInversion(keyData[i + 2]);
                Key[++count] = AdditiveInversion(keyData[i + 1]);
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
            BigInteger bigInteger = BigInteger.ModPow(value, mod - 2, mod);
            return (ushort)bigInteger;
        }
    }
}
