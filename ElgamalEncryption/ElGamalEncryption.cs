using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Numerics;

namespace ElgamalEncryption
{
    public class ElGamalEncryption
    {
        private static Random random = new Random();
        private const int mod = 65537;
        private int P { get; set; } //случайное большое простое число
        private int G { get; set; } //первообразный корень P
        private int X { get; set; } //число X такое, что  1 < X < P - 1 (открытый ключ)
        private int Y { get; set; } //число Y = G^X mod P (закрытый ключ)
        private int K { get; set; } //случайное число 1 < K < P - 1

        private FileStream SrcFileStream { get; set; }
        private FileStream ResFileStream { get; set; }
        private FileStream KeyFileStream { get; set; }
        private string Extension { get; set; }


        public ElGamalEncryption(FileStream srcFileStream, FileStream resFileStream, FileStream keyFileStream, string extension)
        {
            SrcFileStream = srcFileStream;
            ResFileStream = resFileStream;
            KeyFileStream = keyFileStream;
            Extension = extension;

        }

        /// <summary>
        /// Encrypt file with El Gamal scheme
        /// </summary>
        /// <param name="SrcFileStream">Input file stream of the source file</param>
        /// <param name="ResFileStream">Output file stream of the encrypted file</param>
        /// <param name="KeyFileStream">Output file stream of the key file</param>
        /// <param name="extension">Source file extension</param>
        public void Encrypt()
        {
            GenerateKeys();

            //создние MD5 хеша
            byte[] md5 = MD5.Create().ComputeHash(SrcFileStream);
            CreateKeyFile(KeyFileStream, md5, Extension);

            ResFileStream.Seek(0, SeekOrigin.Begin);
            ResFileStream.WriteByte(2);
            ResFileStream.Write(md5, 0, md5.Length);

            byte[] temp = new byte[2];
            byte[] dataTemp = new byte[4];
            ushort data = 0;
            SrcFileStream.Seek(0, SeekOrigin.Begin);
            for (long i = 0; i < SrcFileStream.Length; i += 2)
            {
                SrcFileStream.Read(temp, 0, 2);

                //шифрование блока данных
                data = BitConverter.ToUInt16(temp, 0);
                K = random.Next(1, P - 1);
                BigInteger a = BigInteger.Pow(G, K) % P;
                BigInteger b = BigInteger.Pow(Y, K) * data % P;

                //запись зашифрованного блока данных
                dataTemp = BitConverter.GetBytes((int)a);
                ResFileStream.Write(dataTemp, 0, 4);
                dataTemp = BitConverter.GetBytes((int)b);
                ResFileStream.Write(dataTemp, 0, 4);
            }
        }

        /// <summary>
        /// Create key file
        /// </summary>
        /// <param name="keyFileStream">Output file stream of the key file</param>
        /// <param name="md5">MD5 hash</param>
        /// <param name="extension">Source file extension</param>
        private void CreateKeyFile(FileStream keyFileStream, byte[] md5, string extension)
        {
            //запись MD5 хеша
            keyFileStream.Seek(0, SeekOrigin.Begin);
            keyFileStream.Write(md5, 0, md5.Length);
            //запись числа P
            keyFileStream.Seek(16, SeekOrigin.Begin);
            keyFileStream.Write(BitConverter.GetBytes(P), 0, 4);
            //запись числа X
            keyFileStream.Seek(20, SeekOrigin.Begin);
            keyFileStream.Write(BitConverter.GetBytes(X), 0, 4);
            //запись расширения исходного файла
            keyFileStream.Seek(24, SeekOrigin.Begin);
            byte[] temp = Encoding.Default.GetBytes(extension);
            keyFileStream.Write(temp, 0, temp.Length);
        } 

        /// <summary>
        /// Generate encryption keys
        /// </summary>
        private void GenerateKeys()
        {
            //генерация большого простого числа
            int limit = random.Next(ushort.MaxValue + 64, ushort.MaxValue * 3 / 2);
            BitArray primes = SieveOfAtkin(limit);
            List<int> list = new List<int>();
            long t = limit * 3 / 4;
            for (int number = (int)t; number <= limit; number++)
                if (primes[number])
                    if (random.Next(0, 100) == 34)
                    {
                        list.Add(number);
                    }

            P = list[random.Next(0, list.Count)];

            primes = null;
            list = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();

            G = PrimitiveRoot(P);
            X = random.Next(1, P - 1);
            Y = (int)BinPowMod(G, X, P);
            //Y = (int)BigInteger.Pow(G, X) % P;
            //BigInteger y = BigInteger.Pow(G, X) % P;
            //K = random.Next(1, P - 1);
        }

        /// <summary>
        /// Generate simple numbers
        /// </summary>
        /// <param name="limit">Maximal value of simple number</param>
        /// <returns></returns>
        private BitArray SieveOfAtkin(int limit)
        {
            BitArray sieve = new BitArray(limit + 1);
            // Предварительное просеивание
            for (long x2 = 1, dx2 = 3; x2 < limit; x2 += dx2, dx2 += 2)
                for (long y2 = 1, dy2 = 3, n; y2 < limit; y2 += dy2, dy2 += 2)
                {
                    // n = 4x² + y²
                    n = (x2 << 2) + y2;
                    if (n <= limit && (n % 12 == 1 || n % 12 == 5))
                        sieve[(int)n] ^= true;
                    // n = 3x² + y²
                    n -= x2;
                    if (n <= limit && n % 12 == 7)
                        sieve[(int)n] ^= true;
                    // n = 3x² - y² (при x > y)
                    if (x2 > y2)
                    {
                        n -= y2 << 1;
                        if (n <= limit && n % 12 == 11)
                            sieve[(int)n] ^= true;
                    }
                }
            // Все числа, кратные квадратам, помечаются как составные
            int r = 5;
            for (long r2 = r * r, dr2 = (r << 1) + 1; r2 < limit; ++r, r2 += dr2, dr2 += 2)
                if (sieve[r])
                    for (long mr2 = r2; mr2 < limit; mr2 += r2)
                        sieve[(int)mr2] = false;
            // Числа 2 и 3 — заведомо простые
            if (limit > 2)
                sieve[2] = true;
            if (limit > 3)
                sieve[3] = true;
            return sieve;
        }

        /// <summary>
        /// Primitive number root
        /// </summary>
        /// <param name="number">The number for which it is necessary to find the primitive root</param>
        /// <returns></returns>
        private int PrimitiveRoot(int number)
        {
            List<int> fact = new List<int>();
            int phi = (int)EylerFunction(number), n = phi;
            for (int i = 2; i * i <= n; ++i)
                if (n % i == 0)
                {
                    fact.Add(i);
                    while (n % i == 0)
                        n /= i;
                }
            if (n > 1)
                fact.Add(n);

            for (int res = 2; res <= number; ++res)
            {
                bool ok = true;
                for (int i = 0; i < fact.Count && ok; ++i)
                    ok &= BinPowMod(res, phi / fact[i], number) != 1;
                if (ok) return res;
            }
            return -1;
        }

        /// <summary>
        /// Eyler function for number
        /// </summary>
        /// <param name="number">The number for which it is necessary to find the eyler function</param>
        /// <returns></returns>
        private long EylerFunction(long number)
        {
            long result = number;
            for (long i = 2; i * i <= number; i++)
                if (number % i == 0)
                {
                    while (number % i == 0)
                        number /= i;
                    result -= result / i;
                }
            if (number > 1)
                result -= result / number;
            return result;
        }

        /// <summary>
        /// Binary exponentiation in modulus
        /// </summary>
        /// <param name="value">The number that must be raised to the power</param>
        /// <param name="pow">The pow to which it is necessary to build a number</param>
        /// <param name="mod">Modulus</param>
        /// <returns></returns>
        private long BinPowMod(long value, int pow, int mod)
        {
            long res = 1;
            while (pow > 0)
            {
                if (pow % 2 == 1)
                    res = (res * value) % mod;
                pow >>= 1;
                value = (value * value) % mod;
            }
            return res;
        }
    }
}
