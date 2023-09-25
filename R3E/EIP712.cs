using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using System.Numerics;


namespace MinimalForwarder
{
    public partial class MinimalForwarder
    {
        public const string _version = "0.0.1";
        public const string _name = "MinimalForwader";
        public const string _type = "ForwardRequest(address from,address to,uint256 value,uint256 gas,uint256 nonce,bytes data)";
        public ByteString _TYPE_HASH => (ByteString)BigInteger.Parse("32789846889875464333465174299582028435151612399515816426518116240359049367517");
        // 0x487e6549a3e7599719b89e6a81618ba72806a32891d39b883e39f4b0704b8fdd
        // keccak256("ForwardRequest(address from,address to,uint256 value,uint256 gas,uint256 nonce,bytes data)".ToByteArray());


        public ByteString domainSeparatorV4() => keccak256(abiencode(_TYPE_HASH, keccak256(_name), keccak256(_version), Runtime.GetNetwork(), Runtime.ExecutingScriptHash));
        public ByteString hashTypedDataV4(ByteString structHash) => toTypedDataHash(domainSeparatorV4(), structHash);
    }
}
