using Chaos.NaCl;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Aptos.HdWallet.Utils
{
    /// <summary>
    /// Implements utility methods to be used in the wallet.
    /// </summary>
    public  static class Utils
    {
        /// <summary>
        /// Check if it's a valid hex address.
        /// </summary>
        /// <param name="walletAddress"></param>
        /// <returns>true if is a valid hex address, false otherwise.</returns>
        public static bool IsValidAddress(string walletAddress)
        {
            if (walletAddress[0..2].Equals("0x"))
                walletAddress = walletAddress[2..];

            string pattern = @"[a-fA-F0-9]{64}$";
            Regex rg = new Regex(pattern);
            return rg.IsMatch(walletAddress);
        }

        /// <summary>
        /// Converts a hexadecimal string to an array of bytes
        /// NOTE: string must not contain "0x"
        /// Wrong input:   0x586e3c8d447d7679222e139033e3820235e33da5091e9b0bb8f1a112cf0c8ff5
        /// Correct input: 586e3c8d447d7679222e139033e3820235e33da5091e9b0bb8f1a112cf0c8ff5
        /// </summary>
        /// <param name="input"></param> Valid hexadecimal string
        /// <returns>Byte array representation of hexadecimal string</returns>
        public static byte[] ByteArrayFromHexString(this string input)
        {
            // Catch if a "0x" string is passed
            if (input.Substring(0, 2).Equals("0x"))
                input = input[2..];

            var outputLength = input.Length / 2;
            var output = new byte[outputLength];
            var numeral = new char[2];
            for (int i = 0; i < outputLength; i++)
            {
                input.CopyTo(i * 2, numeral, 0, 2);
                output[i] = Convert.ToByte(new string(numeral), 16);
            }
            return output;
        }

        /// <summary>
        /// Turn byte array to hex string without 0x identifier
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string HexString(this byte[] input)
        {
            string addressHex = BitConverter.ToString(input); // Turn into hexadecimal string
            addressHex = addressHex.Replace("-", "").ToLowerInvariant(); // Remove '-' characters from hexa hash
            //return "0x" + addressHex;
            return addressHex;
        }

        public static string ByteArrayToReadableString(this byte[] input)
        {
            return string.Join(", ", input);
        }

        /// <summary>
        /// Adds or replaces a value in a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary.</param>
        /// <param name="key">The key to add or replace.</param>
        /// <param name="value">The value.</param>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        internal static void AddOrReplace<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue value)
        {
            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        /// <summary>
        /// Attempts to get a value from a dictionary.
        /// </summary>
        /// <param name="dictionary">The dictionary to get the value from.</param>
        /// <param name="key">The key to get.</param>
        /// <typeparam name="TKey">The type of the key.</typeparam>
        /// <typeparam name="TValue">The type of the value.</typeparam>
        /// <returns>The value.</returns>
        internal static TValue TryGet<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            dictionary.TryGetValue(key, out TValue value);
            return value;
        }

        /// <summary>
        /// Slices the array, returning a new array starting at <c>start</c> index and ending at <c>end</c> index.
        /// </summary>
        /// <param name="source">The array to slice.</param>
        /// <param name="start">The starting index of the slicing.</param>
        /// <param name="end">The ending index of the slicing.</param>
        /// <typeparam name="T">The array type.</typeparam>
        /// <returns>The sliced array.</returns>
        internal static T[] Slice<T>(this T[] source, int start, int end)
        {
            if (end < 0)
                end = source.Length;

            var len = end - start;

            // Return new array.
            var res = new T[len];
            for (var i = 0; i < len; i++) res[i] = source[i + start];
            return res;
        }

        /// <summary>
        /// Slices the array, returning a new array starting at <c>start</c>.
        /// </summary>
        /// <param name="source">The array to slice.</param>
        /// <param name="start">The starting index of the slicing.</param>
        /// <typeparam name="T">The array type.</typeparam>
        /// <returns>The sliced array.</returns>
        internal static T[] Slice<T>(this T[] source, int start)
        {
            return Slice(source, start, -1);
        }

        /// <summary>
        /// Calculates the Sha256 of the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <returns>The hash.</returns>
        public static byte[] Sha256(ReadOnlySpan<byte> data)
        {
            return Sha256(data.ToArray(), 0, data.Length);
        }

        /// <summary>
        /// Calculates the SHA256 of the given data.
        /// </summary>
        /// <param name="data">The data to hash.</param>
        /// <param name="offset">The offset at which to start.</param>
        /// <param name="count">The number of bytes to in the array to use as data.</param>
        /// <returns>The hash.</returns>
        private static byte[] Sha256(byte[] data, int offset, int count)
        {
            var SHA256CHECKSUM = SHA256.Create();
            return SHA256CHECKSUM.ComputeHash(data.AsSpan(offset, count).ToArray());
        }

        /// <summary>
        /// Gets the corresponding ed25519 key pair from the passed seed.
        /// </summary>
        /// <param name="seed">The seed</param>
        /// <returns>The key pair.</returns>
        internal static (byte[] privateKey, byte[] publicKey) EdKeyPairFromSeed(byte[] seed) =>
            (Ed25519.ExpandedPrivateKeyFromSeed(seed), Ed25519.PublicKeyFromSeed(seed));
    }
}