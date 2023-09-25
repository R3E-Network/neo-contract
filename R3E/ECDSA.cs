using Neo.SmartContract.Framework;

namespace MinimalForwarder
{
    public partial class MinimalForwarder
    {
        public static ByteString toTypedDataHash(ByteString domainSeparator, ByteString structHash) => keccak256((ByteString)"\x19\x01" + domainSeparator + structHash);
    }
}
