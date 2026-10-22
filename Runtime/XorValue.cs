using System;
using System.Globalization;
using UnityEngine;

namespace MagicExcel {
    internal static class XorHelper {
        public static byte[] GenerateKey() {
            var key = new byte[16];
            var rand = new System.Random();
            rand.NextBytes(key);
            return key;
        }

        public static byte[] Encrypt(byte[] data, byte[] key) {
            for (int i = 0; i < data.Length; i++)
                data[i] ^= key[i % key.Length];
            return data;
        }

        public static byte[] Decrypt(byte[] data, byte[] key) {
            byte[] result = new byte[data.Length];
            for (int i = 0; i < data.Length; i++)
                result[i] = (byte)(data[i] ^ key[i % key.Length]);
            return result;
        }
    }

    [Serializable]
    public struct XorInt {
        [SerializeField] byte[] encryptedData;
        [SerializeField] byte[] key;

        public XorInt(int value) {
            key = XorHelper.GenerateKey();
            encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
        }

        public int Value {
            readonly get => BitConverter.ToInt32(XorHelper.Decrypt(encryptedData, key), 0);
            set {
                key = XorHelper.GenerateKey();
                encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
            }
        }

        public static implicit operator int(XorInt v) => v.Value;
        public static implicit operator XorInt(int v) => new(v);

        public static XorInt Parse(string s) => new(int.Parse(s));
        public override readonly string ToString() => Value.ToString();
    }

    [Serializable]
    public struct XorFloat {
        [SerializeField] byte[] encryptedData;
        [SerializeField] byte[] key;

        public XorFloat(float value) {
            key = XorHelper.GenerateKey();
            encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
        }

        public float Value {
            readonly get => BitConverter.ToSingle(XorHelper.Decrypt(encryptedData, key), 0);
            set {
                key = XorHelper.GenerateKey();
                encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
            }
        }

        public static implicit operator float(XorFloat v) => v.Value;
        public static implicit operator XorFloat(float v) => new(v);

        public static XorFloat Parse(string s) => new(float.Parse(s, CultureInfo.InvariantCulture));
        public override readonly string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    [Serializable]
    public struct XorDouble {
        [SerializeField] byte[] encryptedData;
        [SerializeField] byte[] key;

        public XorDouble(double value) {
            key = XorHelper.GenerateKey();
            encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
        }

        public double Value {
            readonly get => BitConverter.ToDouble(XorHelper.Decrypt(encryptedData, key), 0);
            set {
                key = XorHelper.GenerateKey();
                encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
            }
        }

        public static implicit operator double(XorDouble v) => v.Value;
        public static implicit operator XorDouble(double v) => new(v);

        public static XorDouble Parse(string s) => new(double.Parse(s, CultureInfo.InvariantCulture));
        public override readonly string ToString() => Value.ToString(CultureInfo.InvariantCulture);
    }

    [Serializable]
    public struct XorLong {
        [SerializeField] byte[] encryptedData;
        [SerializeField] byte[] key;

        public XorLong(long value) {
            key = XorHelper.GenerateKey();
            encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
        }

        public long Value {
            readonly get => BitConverter.ToInt64(XorHelper.Decrypt(encryptedData, key), 0);
            set {
                key = XorHelper.GenerateKey();
                encryptedData = XorHelper.Encrypt(BitConverter.GetBytes(value), key);
            }
        }

        public static implicit operator long(XorLong v) => v.Value;
        public static implicit operator XorLong(long v) => new(v);

        public static XorLong Parse(string s) => new(long.Parse(s));
        public override readonly string ToString() => Value.ToString();
    }
}