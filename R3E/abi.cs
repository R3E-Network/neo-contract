using Neo;
using Neo.SmartContract.Framework;
using System.Numerics;

namespace MinimalForwarder
{
    public partial class MinimalForwarder
    {
        public static ByteString abiencode(ByteString data, bool reverse)
        {
            int len = data.Length;
            byte[] dataBytes = (byte[])data;
            //ExecutionEngine.Assert(len <= 32, "Too long data");
            if (len > 32)
            {
                dataBytes = dataBytes.Range(0, 32);
                if (reverse)
                    dataBytes = dataBytes.Reverse();
                return (ByteString)dataBytes;
            }
            ByteString prefix = "";
            for (int i = 0; i < 32 - len; ++i)
                prefix += "\x00";
            if (reverse)
                return prefix + (ByteString)dataBytes.Reverse();
            else
                return prefix + data;
        }
        public static ByteString abiencode(ByteString data) => abiencode(data, false);
        public static ByteString abiencodeUInt160(UInt160 data) => abiencode(data, false);
        public static ByteString abiencodeBigInteger(BigInteger data) => abiencode((ByteString)data.ToByteArray(), true);
        public static ByteString abiencode(ByteString typeHash, ByteString nameHash, ByteString versionHash, BigInteger networkId, UInt160 thisAddress) => abiencode(typeHash) + abiencode(nameHash) + abiencode(versionHash) + abiencodeBigInteger(networkId) + abiencode(thisAddress);
        public static ByteString abiencode(ByteString typeHash, UInt160 from, UInt160 to, BigInteger value, BigInteger gas, BigInteger nonce, ByteString call) => abiencode(typeHash) + abiencodeUInt160(from) + abiencodeUInt160(to) + abiencodeBigInteger(value) + abiencodeBigInteger(gas) + abiencodeBigInteger(nonce) + abiencode(call);
    }
}
