using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using System;

namespace MinimalForwarder
{
    public partial class MinimalForwarder
    {
        private static readonly UInt64[] KeccakRoundConstants =
        {
            0x0000000000000001UL, 0x0000000000008082UL, 0x800000000000808aUL, 0x8000000080008000UL,
            0x000000000000808bUL, 0x0000000080000001UL, 0x8000000080008081UL, 0x8000000000008009UL,
            0x000000000000008aUL, 0x0000000000000088UL, 0x0000000080008009UL, 0x000000008000000aUL,
            0x000000008000808bUL, 0x800000000000008bUL, 0x8000000000008089UL, 0x8000000000008003UL,
            0x8000000000008002UL, 0x8000000000000080UL, 0x000000000000800aUL, 0x800000008000000aUL,
            0x8000000080008081UL, 0x8000000000008080UL, 0x0000000080000001UL, 0x8000000080008008UL
        };

        private static readonly int FixedOutputLength = 32;
        private static readonly int Rate = 136;
        private static readonly byte Dsbyte = 0x01;

        [Safe]
        public static ByteString keccak256(ByteString input) => (ByteString)Squeeze(Absorb((byte[])input));

        //[Safe]
        //public static byte[] keccak256(byte[] input) => Squeeze(Absorb(input));

        [Safe]
        public static string keccak256string(string input) => Squeeze(Absorb(input.ToByteArray())).ToByteString();

        private static UInt64[] Absorb(byte[] input)
        {
            int len = input.Length;
            UInt64[] state = new UInt64[25];
            for (int i = 0; i < len; i += Rate)
            {
                if (i + Rate <= len)
                {
                    // longer than rate
                    byte[] buf = input.Range(i, Rate);
                    state = KeccakF1600(XorIn(state, buf));
                }
                else
                {
                    // shorter than rate
                    byte[] buf = input.Range(i, len - i);
                    state = KeccakF1600(XorIn(state, Pad(buf)));
                }
            }
            return state;
        }

        private static byte[] Squeeze(UInt64[] state)
        {
            byte[] output = new byte[FixedOutputLength];
            output = CopyOut(state, output);
            return output;
        }

        private static byte[] Pad(byte[] input)
        {
            // Copy input to buf
            int len = input.Length;
            byte[] buf = new byte[Rate - len];
            buf = input.Concat(buf);
            // dsbyte
            buf[len] = Dsbyte;
            // Final bit
            buf[Rate - 1] ^= 0x80;
            return buf;
        }

        private static UInt64[] KeccakF1600(UInt64[] a)
        {
            UInt64 t, bc0, bc1, bc2, bc3, bc4, d0, d1, d2, d3, d4;

            for (int i = 0; i < 24; i += 4)
            {
                // Round 1
                bc0 = a[0] ^ a[5] ^ a[10] ^ a[15] ^ a[20];
                bc1 = a[1] ^ a[6] ^ a[11] ^ a[16] ^ a[21];
                bc2 = a[2] ^ a[7] ^ a[12] ^ a[17] ^ a[22];
                bc3 = a[3] ^ a[8] ^ a[13] ^ a[18] ^ a[23];
                bc4 = a[4] ^ a[9] ^ a[14] ^ a[19] ^ a[24];
                d0 = bc4 ^ (LeftRotate(bc1, 1) | (bc1 >> 63));
                d1 = bc0 ^ (LeftRotate(bc2, 1) | (bc2 >> 63));
                d2 = bc1 ^ (LeftRotate(bc3, 1) | (bc3 >> 63));
                d3 = bc2 ^ (LeftRotate(bc4, 1) | (bc4 >> 63));
                d4 = bc3 ^ (LeftRotate(bc0, 1) | (bc0 >> 63));

                bc0 = a[0] ^ d0;
                t = a[6] ^ d1;
                bc1 = LeftRotate(t, 44) | (t >> 20);
                t = a[12] ^ d2;
                bc2 = LeftRotate(t, 43) | (t >> 21);
                t = a[18] ^ d3;
                bc3 = LeftRotate(t, 21) | (t >> 43);
                t = a[24] ^ d4;
                bc4 = LeftRotate(t, 14) | (t >> 50);
                a[0] = bc0 ^ (bc2 & ~bc1) ^ KeccakRoundConstants[i];
                a[6] = bc1 ^ (bc3 & ~bc2);
                a[12] = bc2 ^ (bc4 & ~bc3);
                a[18] = bc3 ^ (bc0 & ~bc4);
                a[24] = bc4 ^ (bc1 & ~bc0);

                t = a[10] ^ d0;
                bc2 = LeftRotate(t, 3) | (t >> 61);
                t = a[16] ^ d1;
                bc3 = LeftRotate(t, 45) | (t >> 19);
                t = a[22] ^ d2;
                bc4 = LeftRotate(t, 61) | (t >> 3);
                t = a[3] ^ d3;
                bc0 = LeftRotate(t, 28) | (t >> 36);
                t = a[9] ^ d4;
                bc1 = LeftRotate(t, 20) | (t >> 44);
                a[10] = bc0 ^ (bc2 & ~bc1);
                a[16] = bc1 ^ (bc3 & ~bc2);
                a[22] = bc2 ^ (bc4 & ~bc3);
                a[3] = bc3 ^ (bc0 & ~bc4);
                a[9] = bc4 ^ (bc1 & ~bc0);

                t = a[20] ^ d0;
                bc4 = LeftRotate(t, 18) | (t >> 46);
                t = a[1] ^ d1;
                bc0 = LeftRotate(t, 1) | (t >> 63);
                t = a[7] ^ d2;
                bc1 = LeftRotate(t, 6) | (t >> 58);
                t = a[13] ^ d3;
                bc2 = LeftRotate(t, 25) | (t >> 39);
                t = a[19] ^ d4;
                bc3 = LeftRotate(t, 8) | (t >> 56);
                a[20] = bc0 ^ (bc2 & ~bc1);
                a[1] = bc1 ^ (bc3 & ~bc2);
                a[7] = bc2 ^ (bc4 & ~bc3);
                a[13] = bc3 ^ (bc0 & ~bc4);
                a[19] = bc4 ^ (bc1 & ~bc0);

                t = a[5] ^ d0;
                bc1 = LeftRotate(t, 36) | (t >> 28);
                t = a[11] ^ d1;
                bc2 = LeftRotate(t, 10) | (t >> 54);
                t = a[17] ^ d2;
                bc3 = LeftRotate(t, 15) | (t >> 49);
                t = a[23] ^ d3;
                bc4 = LeftRotate(t, 56) | (t >> 8);
                t = a[4] ^ d4;
                bc0 = LeftRotate(t, 27) | (t >> 37);
                a[5] = bc0 ^ (bc2 & ~bc1);
                a[11] = bc1 ^ (bc3 & ~bc2);
                a[17] = bc2 ^ (bc4 & ~bc3);
                a[23] = bc3 ^ (bc0 & ~bc4);
                a[4] = bc4 ^ (bc1 & ~bc0);

                t = a[15] ^ d0;
                bc3 = LeftRotate(t, 41) | (t >> 23);
                t = a[21] ^ d1;
                bc4 = LeftRotate(t, 2) | (t >> 62);
                t = a[2] ^ d2;
                bc0 = LeftRotate(t, 62) | (t >> 2);
                t = a[8] ^ d3;
                bc1 = LeftRotate(t, 55) | (t >> 9);
                t = a[14] ^ d4;
                bc2 = LeftRotate(t, 39) | (t >> 25);
                a[15] = bc0 ^ (bc2 & ~bc1);
                a[21] = bc1 ^ (bc3 & ~bc2);
                a[2] = bc2 ^ (bc4 & ~bc3);
                a[8] = bc3 ^ (bc0 & ~bc4);
                a[14] = bc4 ^ (bc1 & ~bc0);

                // Round 2
                bc0 = a[0] ^ a[5] ^ a[10] ^ a[15] ^ a[20];
                bc1 = a[1] ^ a[6] ^ a[11] ^ a[16] ^ a[21];
                bc2 = a[2] ^ a[7] ^ a[12] ^ a[17] ^ a[22];
                bc3 = a[3] ^ a[8] ^ a[13] ^ a[18] ^ a[23];
                bc4 = a[4] ^ a[9] ^ a[14] ^ a[19] ^ a[24];
                d0 = bc4 ^ (LeftRotate(bc1, 1) | (bc1 >> 63));
                d1 = bc0 ^ (LeftRotate(bc2, 1) | (bc2 >> 63));
                d2 = bc1 ^ (LeftRotate(bc3, 1) | (bc3 >> 63));
                d3 = bc2 ^ (LeftRotate(bc4, 1) | (bc4 >> 63));
                d4 = bc3 ^ (LeftRotate(bc0, 1) | (bc0 >> 63));

                bc0 = a[0] ^ d0;
                t = a[16] ^ d1;
                bc1 = LeftRotate(t, 44) | (t >> 20);
                t = a[7] ^ d2;
                bc2 = LeftRotate(t, 43) | (t >> 21);
                t = a[23] ^ d3;
                bc3 = LeftRotate(t, 21) | (t >> 43);
                t = a[14] ^ d4;
                bc4 = LeftRotate(t, 14) | (t >> 50);
                a[0] = bc0 ^ (bc2 & ~bc1) ^ KeccakRoundConstants[i + 1];
                a[16] = bc1 ^ (bc3 & ~bc2);
                a[7] = bc2 ^ (bc4 & ~bc3);
                a[23] = bc3 ^ (bc0 & ~bc4);
                a[14] = bc4 ^ (bc1 & ~bc0);

                t = a[20] ^ d0;
                bc2 = LeftRotate(t, 3) | (t >> 61);
                t = a[11] ^ d1;
                bc3 = LeftRotate(t, 45) | (t >> 19);
                t = a[2] ^ d2;
                bc4 = LeftRotate(t, 61) | (t >> 3);
                t = a[18] ^ d3;
                bc0 = LeftRotate(t, 28) | (t >> 36);
                t = a[9] ^ d4;
                bc1 = LeftRotate(t, 20) | (t >> 44);
                a[20] = bc0 ^ (bc2 & ~bc1);
                a[11] = bc1 ^ (bc3 & ~bc2);
                a[2] = bc2 ^ (bc4 & ~bc3);
                a[18] = bc3 ^ (bc0 & ~bc4);
                a[9] = bc4 ^ (bc1 & ~bc0);

                t = a[15] ^ d0;
                bc4 = LeftRotate(t, 18) | (t >> 46);
                t = a[6] ^ d1;
                bc0 = LeftRotate(t, 1) | (t >> 63);
                t = a[22] ^ d2;
                bc1 = LeftRotate(t, 6) | (t >> 58);
                t = a[13] ^ d3;
                bc2 = LeftRotate(t, 25) | (t >> 39);
                t = a[4] ^ d4;
                bc3 = LeftRotate(t, 8) | (t >> 56);
                a[15] = bc0 ^ (bc2 & ~bc1);
                a[6] = bc1 ^ (bc3 & ~bc2);
                a[22] = bc2 ^ (bc4 & ~bc3);
                a[13] = bc3 ^ (bc0 & ~bc4);
                a[4] = bc4 ^ (bc1 & ~bc0);

                t = a[10] ^ d0;
                bc1 = LeftRotate(t, 36) | (t >> 28);
                t = a[1] ^ d1;
                bc2 = LeftRotate(t, 10) | (t >> 54);
                t = a[17] ^ d2;
                bc3 = LeftRotate(t, 15) | (t >> 49);
                t = a[8] ^ d3;
                bc4 = LeftRotate(t, 56) | (t >> 8);
                t = a[24] ^ d4;
                bc0 = LeftRotate(t, 27) | (t >> 37);
                a[10] = bc0 ^ (bc2 & ~bc1);
                a[1] = bc1 ^ (bc3 & ~bc2);
                a[17] = bc2 ^ (bc4 & ~bc3);
                a[8] = bc3 ^ (bc0 & ~bc4);
                a[24] = bc4 ^ (bc1 & ~bc0);

                t = a[5] ^ d0;
                bc3 = LeftRotate(t, 41) | (t >> 23);
                t = a[21] ^ d1;
                bc4 = LeftRotate(t, 2) | (t >> 62);
                t = a[12] ^ d2;
                bc0 = LeftRotate(t, 62) | (t >> 2);
                t = a[3] ^ d3;
                bc1 = LeftRotate(t, 55) | (t >> 9);
                t = a[19] ^ d4;
                bc2 = LeftRotate(t, 39) | (t >> 25);
                a[5] = bc0 ^ (bc2 & ~bc1);
                a[21] = bc1 ^ (bc3 & ~bc2);
                a[12] = bc2 ^ (bc4 & ~bc3);
                a[3] = bc3 ^ (bc0 & ~bc4);
                a[19] = bc4 ^ (bc1 & ~bc0);

                // Round 3
                bc0 = a[0] ^ a[5] ^ a[10] ^ a[15] ^ a[20];
                bc1 = a[1] ^ a[6] ^ a[11] ^ a[16] ^ a[21];
                bc2 = a[2] ^ a[7] ^ a[12] ^ a[17] ^ a[22];
                bc3 = a[3] ^ a[8] ^ a[13] ^ a[18] ^ a[23];
                bc4 = a[4] ^ a[9] ^ a[14] ^ a[19] ^ a[24];
                d0 = bc4 ^ (LeftRotate(bc1, 1) | (bc1 >> 63));
                d1 = bc0 ^ (LeftRotate(bc2, 1) | (bc2 >> 63));
                d2 = bc1 ^ (LeftRotate(bc3, 1) | (bc3 >> 63));
                d3 = bc2 ^ (LeftRotate(bc4, 1) | (bc4 >> 63));
                d4 = bc3 ^ (LeftRotate(bc0, 1) | (bc0 >> 63));

                bc0 = a[0] ^ d0;
                t = a[11] ^ d1;
                bc1 = LeftRotate(t, 44) | (t >> 20);
                t = a[22] ^ d2;
                bc2 = LeftRotate(t, 43) | (t >> 21);
                t = a[8] ^ d3;
                bc3 = LeftRotate(t, 21) | (t >> 43);
                t = a[19] ^ d4;
                bc4 = LeftRotate(t, 14) | (t >> 50);
                a[0] = bc0 ^ (bc2 & ~bc1) ^ KeccakRoundConstants[i + 2];
                a[11] = bc1 ^ (bc3 & ~bc2);
                a[22] = bc2 ^ (bc4 & ~bc3);
                a[8] = bc3 ^ (bc0 & ~bc4);
                a[19] = bc4 ^ (bc1 & ~bc0);

                t = a[15] ^ d0;
                bc2 = LeftRotate(t, 3) | (t >> 61);
                t = a[1] ^ d1;
                bc3 = LeftRotate(t, 45) | (t >> 19);
                t = a[12] ^ d2;
                bc4 = LeftRotate(t, 61) | (t >> 3);
                t = a[23] ^ d3;
                bc0 = LeftRotate(t, 28) | (t >> 36);
                t = a[9] ^ d4;
                bc1 = LeftRotate(t, 20) | (t >> 44);
                a[15] = bc0 ^ (bc2 & ~bc1);
                a[1] = bc1 ^ (bc3 & ~bc2);
                a[12] = bc2 ^ (bc4 & ~bc3);
                a[23] = bc3 ^ (bc0 & ~bc4);
                a[9] = bc4 ^ (bc1 & ~bc0);

                t = a[5] ^ d0;
                bc4 = LeftRotate(t, 18) | (t >> 46);
                t = a[16] ^ d1;
                bc0 = LeftRotate(t, 1) | (t >> 63);
                t = a[2] ^ d2;
                bc1 = LeftRotate(t, 6) | (t >> 58);
                t = a[13] ^ d3;
                bc2 = LeftRotate(t, 25) | (t >> 39);
                t = a[24] ^ d4;
                bc3 = LeftRotate(t, 8) | (t >> 56);
                a[5] = bc0 ^ (bc2 & ~bc1);
                a[16] = bc1 ^ (bc3 & ~bc2);
                a[2] = bc2 ^ (bc4 & ~bc3);
                a[13] = bc3 ^ (bc0 & ~bc4);
                a[24] = bc4 ^ (bc1 & ~bc0);

                t = a[20] ^ d0;
                bc1 = LeftRotate(t, 36) | (t >> 28);
                t = a[6] ^ d1;
                bc2 = LeftRotate(t, 10) | (t >> 54);
                t = a[17] ^ d2;
                bc3 = LeftRotate(t, 15) | (t >> 49);
                t = a[3] ^ d3;
                bc4 = LeftRotate(t, 56) | (t >> 8);
                t = a[14] ^ d4;
                bc0 = LeftRotate(t, 27) | (t >> 37);
                a[20] = bc0 ^ (bc2 & ~bc1);
                a[6] = bc1 ^ (bc3 & ~bc2);
                a[17] = bc2 ^ (bc4 & ~bc3);
                a[3] = bc3 ^ (bc0 & ~bc4);
                a[14] = bc4 ^ (bc1 & ~bc0);

                t = a[10] ^ d0;
                bc3 = LeftRotate(t, 41) | (t >> 23);
                t = a[21] ^ d1;
                bc4 = LeftRotate(t, 2) | (t >> 62);
                t = a[7] ^ d2;
                bc0 = LeftRotate(t, 62) | (t >> 2);
                t = a[18] ^ d3;
                bc1 = LeftRotate(t, 55) | (t >> 9);
                t = a[4] ^ d4;
                bc2 = LeftRotate(t, 39) | (t >> 25);
                a[10] = bc0 ^ (bc2 & ~bc1);
                a[21] = bc1 ^ (bc3 & ~bc2);
                a[7] = bc2 ^ (bc4 & ~bc3);
                a[18] = bc3 ^ (bc0 & ~bc4);
                a[4] = bc4 ^ (bc1 & ~bc0);

                // Round 4
                bc0 = a[0] ^ a[5] ^ a[10] ^ a[15] ^ a[20];
                bc1 = a[1] ^ a[6] ^ a[11] ^ a[16] ^ a[21];
                bc2 = a[2] ^ a[7] ^ a[12] ^ a[17] ^ a[22];
                bc3 = a[3] ^ a[8] ^ a[13] ^ a[18] ^ a[23];
                bc4 = a[4] ^ a[9] ^ a[14] ^ a[19] ^ a[24];
                d0 = bc4 ^ (LeftRotate(bc1, 1) | (bc1 >> 63));
                d1 = bc0 ^ (LeftRotate(bc2, 1) | (bc2 >> 63));
                d2 = bc1 ^ (LeftRotate(bc3, 1) | (bc3 >> 63));
                d3 = bc2 ^ (LeftRotate(bc4, 1) | (bc4 >> 63));
                d4 = bc3 ^ (LeftRotate(bc0, 1) | (bc0 >> 63));

                bc0 = a[0] ^ d0;
                t = a[1] ^ d1;
                bc1 = LeftRotate(t, 44) | (t >> 20);
                t = a[2] ^ d2;
                bc2 = LeftRotate(t, 43) | (t >> 21);
                t = a[3] ^ d3;
                bc3 = LeftRotate(t, 21) | (t >> 43);
                t = a[4] ^ d4;
                bc4 = LeftRotate(t, 14) | (t >> 50);
                a[0] = bc0 ^ (bc2 & ~bc1) ^ KeccakRoundConstants[i + 3];
                a[1] = bc1 ^ (bc3 & ~bc2);
                a[2] = bc2 ^ (bc4 & ~bc3);
                a[3] = bc3 ^ (bc0 & ~bc4);
                a[4] = bc4 ^ (bc1 & ~bc0);

                t = a[5] ^ d0;
                bc2 = LeftRotate(t, 3) | (t >> 61);
                t = a[6] ^ d1;
                bc3 = LeftRotate(t, 45) | (t >> 19);
                t = a[7] ^ d2;
                bc4 = LeftRotate(t, 61) | (t >> 3);
                t = a[8] ^ d3;
                bc0 = LeftRotate(t, 28) | (t >> 36);
                t = a[9] ^ d4;
                bc1 = LeftRotate(t, 20) | (t >> 44);
                a[5] = bc0 ^ (bc2 & ~bc1);
                a[6] = bc1 ^ (bc3 & ~bc2);
                a[7] = bc2 ^ (bc4 & ~bc3);
                a[8] = bc3 ^ (bc0 & ~bc4);
                a[9] = bc4 ^ (bc1 & ~bc0);

                t = a[10] ^ d0;
                bc4 = LeftRotate(t, 18) | (t >> 46);
                t = a[11] ^ d1;
                bc0 = LeftRotate(t, 1) | (t >> 63);
                t = a[12] ^ d2;
                bc1 = LeftRotate(t, 6) | (t >> 58);
                t = a[13] ^ d3;
                bc2 = LeftRotate(t, 25) | (t >> 39);
                t = a[14] ^ d4;
                bc3 = LeftRotate(t, 8) | (t >> 56);
                a[10] = bc0 ^ (bc2 & ~bc1);
                a[11] = bc1 ^ (bc3 & ~bc2);
                a[12] = bc2 ^ (bc4 & ~bc3);
                a[13] = bc3 ^ (bc0 & ~bc4);
                a[14] = bc4 ^ (bc1 & ~bc0);

                t = a[15] ^ d0;
                bc1 = LeftRotate(t, 36) | (t >> 28);
                t = a[16] ^ d1;
                bc2 = LeftRotate(t, 10) | (t >> 54);
                t = a[17] ^ d2;
                bc3 = LeftRotate(t, 15) | (t >> 49);
                t = a[18] ^ d3;
                bc4 = LeftRotate(t, 56) | (t >> 8);
                t = a[19] ^ d4;
                bc0 = LeftRotate(t, 27) | (t >> 37);
                a[15] = bc0 ^ (bc2 & ~bc1);
                a[16] = bc1 ^ (bc3 & ~bc2);
                a[17] = bc2 ^ (bc4 & ~bc3);
                a[18] = bc3 ^ (bc0 & ~bc4);
                a[19] = bc4 ^ (bc1 & ~bc0);

                t = a[20] ^ d0;
                bc3 = LeftRotate(t, 41) | (t >> 23);
                t = a[21] ^ d1;
                bc4 = LeftRotate(t, 2) | (t >> 62);
                t = a[22] ^ d2;
                bc0 = LeftRotate(t, 62) | (t >> 2);
                t = a[23] ^ d3;
                bc1 = LeftRotate(t, 55) | (t >> 9);
                t = a[24] ^ d4;
                bc2 = LeftRotate(t, 39) | (t >> 25);
                a[20] = bc0 ^ (bc2 & ~bc1);
                a[21] = bc1 ^ (bc3 & ~bc2);
                a[22] = bc2 ^ (bc4 & ~bc3);
                a[23] = bc3 ^ (bc0 & ~bc4);
                a[24] = bc4 ^ (bc1 & ~bc0);
            }

            return a;
        }

        private static UInt64[] XorIn(UInt64[] a, byte[] buf)
        {
            for (int i = 0; i * 8 < buf.Length; i++)
            {
                a[i] ^= (UInt64)buf[i * 8]
                    | (UInt64)buf[i * 8 + 1] << 8
                    | (UInt64)buf[i * 8 + 2] << 16
                    | (UInt64)buf[i * 8 + 3] << 24
                    | (UInt64)buf[i * 8 + 4] << 32
                    | (UInt64)buf[i * 8 + 5] << 40
                    | (UInt64)buf[i * 8 + 6] << 48
                    | (UInt64)buf[i * 8 + 7] << 56;
            }
            return a;
        }

        private static byte[] CopyOut(UInt64[] a, byte[] buf)
        {
            for (int i = 0; i * 8 < buf.Length; i++)
            {
                buf[i * 8] = (byte)a[i];
                buf[i * 8 + 1] = (byte)(a[i] >> 8);
                buf[i * 8 + 2] = (byte)(a[i] >> 16);
                buf[i * 8 + 3] = (byte)(a[i] >> 24);
                buf[i * 8 + 4] = (byte)(a[i] >> 32);
                buf[i * 8 + 5] = (byte)(a[i] >> 40);
                buf[i * 8 + 6] = (byte)(a[i] >> 48);
                buf[i * 8 + 7] = (byte)(a[i] >> 56);
            }
            return buf;
        }

        private static UInt64 LeftRotate(UInt64 left, int right)
        {
            return left << right & UInt64.MaxValue;
        }
    }
}
