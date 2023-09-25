using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using System.Numerics;

namespace MinimalForwarder
{
    public partial class MinimalForwarder
    {
        public static ByteString abiencode(ByteString data)
        {
            int len = data.Length;
            ExecutionEngine.Assert(len <= 32, "Too long data");
            ByteString prefix = "";
            for (int i = 0; i < 32 - len; ++i)
                prefix += "\x00";
            return prefix + data;
        }
        public static ByteString abiencode(ByteString typeHash, ByteString nameHash, ByteString versionHash, BigInteger networkId, UInt160 thisAddress) => abiencode(typeHash) + abiencode(nameHash) + abiencode(versionHash) + abiencode((ByteString)networkId.ToByteArray()) + abiencode(thisAddress);

        public static ByteString abiencode(ByteString typeHash, UInt160 from, UInt160 to, BigInteger value, BigInteger gas, BigInteger nonce, ByteString call) => abiencode(typeHash) + abiencode(from) + abiencode(to) + abiencode((ByteString)value.ToByteArray()) + abiencode((ByteString)gas.ToByteArray()) + abiencode((ByteString)nonce.ToByteArray()) + abiencode(call);
    }
}
