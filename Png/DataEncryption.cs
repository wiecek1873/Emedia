﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace EmediaWPF
{
    class DataEncryption : Singleton<DataEncryption>
    {
        private RandomNumberGenerator rng = new RandomNumberGenerator();

        public BigInteger p;
        public BigInteger q;
        public BigInteger n;
        public BigInteger totient; //To jest to przekreślone o
        public BigInteger e;
        public BigInteger d;

        public int KeyLength {get => n.ToByteArray().Length;} // todo

        public DataEncryption()
        {
            rng = new RandomNumberGenerator();
            PrepareKeys();
        }

        public DataEncryption(EncryptionSave keys)
        {
            rng = new RandomNumberGenerator();
            d = keys.d;
            e = keys.e;
            n = keys.n;
        }

        public EncryptionSave GetKeys()
        {
            return new EncryptionSave
            {
                d = d,
                e = e,
                n = n
            };
        }

        public byte[] EncryptData(byte[] chunkData)
        {
            int dataLength = KeyLength - 1;
            List<byte> partToEncrypt = new List<byte>();
            List<byte> encryptedData = new List<byte>();

            foreach (byte byteOfData in chunkData)
            {
                partToEncrypt.Add(byteOfData);
                if (partToEncrypt.Count == dataLength)
                {
					byte[] encrypted = Encrypt(partToEncrypt.ToArray());
                    Array.Resize<byte>(ref encrypted, KeyLength);
					encryptedData.AddRange(encrypted);
                    partToEncrypt.Clear();
                }
            }

            if (partToEncrypt.Count > 0)
            {
                byte[] encrypted = Encrypt(partToEncrypt.ToArray());
                Array.Resize<byte>(ref encrypted, KeyLength);
                encryptedData.AddRange(encrypted);
                partToEncrypt.Clear();
            }

            return encryptedData.ToArray();
        }

        public byte[] DecryptData(byte[] chunkData)
        {
            int dataLength = KeyLength - 1;
            List<byte> partToDecrypt = new List<byte>();
            List<byte> decryptedData = new List<byte>();

            foreach (byte data in chunkData)
            {
                partToDecrypt.Add(data);
                if (partToDecrypt.Count == KeyLength)
                {
                    byte[] decrypted = Decrypt(partToDecrypt.ToArray());
                    Array.Resize<byte>(ref decrypted, dataLength);
                    decryptedData.AddRange(decrypted);
                    partToDecrypt.Clear();
                }
            }

            return decryptedData.ToArray();
        }

        public byte[] Encrypt(byte[] data)
        {
            return BigInteger.ModPow(new BigInteger(data, true), e, n).ToByteArray(true);
        }

        public byte[] Decrypt(byte[] encryptedData)
        {
            return BigInteger.ModPow(new BigInteger(encryptedData, true), d, n).ToByteArray(true);
        }

        private async void PrepareKeys()
        {
            p = await PrimeNumbers.NextPrimeAsync(rng.Next(2, 1000, 999999999));
            q = await PrimeNumbers.NextPrimeAsync(rng.Next(2, 1000, 999999999));

            if (p == q)
                p = await PrimeNumbers .NextPrimeAsync(p);

            n = p * q;
            totient = (p - 1) * (q - 1);

            do
            {
                e = rng.Next(2, 1000, totient);
            }
            while (!PrimeNumbers.AreCoPrime(e, totient));

            d = ExtendedEuclideanAlgorithm(e, totient);
        }

        private BigInteger ExtendedEuclideanAlgorithm(BigInteger e, BigInteger totient)
        {
            BigInteger k = 0;
            BigInteger value;
            do
            {
                k++;
                value = (1 + k * totient) % e;
            }
            while (value != 0);

            return (1 + k * totient) / e;
        }
    }
}
