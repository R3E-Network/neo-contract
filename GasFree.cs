using System;
using System.ComponentModel;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using Neo.SmartContract.Framework.Native;
using Neo.Cryptography.ECC;

namespace GasFree
{
    public struct NEP712Domain
    {
        public string name;
        public string version;
        public BigInteger chainId;
        public UInt160 verifyingContract;
    }

    public struct MetaTransaction {
        public UInt160 from;
        public ByteString signer;
        public UInt160 to;
        public string function;
        public BigInteger deadline;
        public BigInteger maxGas;
        public BigInteger nonce;
        public object data;
    }

    [DisplayName("GasFree")]
    [ManifestExtra("Author", "Jinghui Liao")]
    [ManifestExtra("Email", "jinghui@wayne.edu")]
    [ManifestExtra("Description", "This is a gasfree contract for testing")]
    [ContractPermission("*", "*")]
    public partial class GasFree : SmartContract
    {
        private const string VERSION = "v0";

        [Safe]
        public static string Version() => VERSION;

        private static readonly ByteString NEP712DOMAIN_TYPEHASH =
            CryptoLib.Sha256(
                (ByteString)"NEP712Domain(string name,string version,BigInteger chainId,UInt160 verifyingContract)"
                    .ToByteArray());

        private static readonly ByteString MAIL_TYPEHASH =
            CryptoLib.Sha256((ByteString)"MetaTransaction(UInt160 from,ByteString signer,UInt160 to,string function,BigInteger deadline,BigInteger maxGas,BigInteger nonce,object data)".ToByteArray());

        // CREATEA A FUNCTION TO CALCULATE THE DOMAIN SEPARATOR
        private static void GetDomainSeperator()
        {
            NEP712Domain domain = new NEP712Domain();
            domain.name = "Neo MetaTransaction";
            domain.version = "1";
            domain.chainId = Runtime.GetNetwork();
            domain.verifyingContract = Runtime.ExecutingScriptHash;

            ByteString domainSeparator = CryptoLib.Sha256(
                NEP712DOMAIN_TYPEHASH +
                CryptoLib.Sha256((ByteString)domain.name.ToByteArray()) +
                CryptoLib.Sha256((ByteString)domain.version.ToByteArray()) +
                CryptoLib.Sha256((ByteString)domain.chainId.ToByteArray()) +
                CryptoLib.Sha256((ByteString)domain.verifyingContract)
            );

            Storage.Put(Storage.CurrentContext, "DOMAIN_SEPARATOR", domainSeparator);
        }


        // CREATE A FUNCTION TO CALCULATE THE MAIL
        private static ByteString GetMetaTransactionHash(MetaTransaction meta)
        {
            return CryptoLib.Sha256(
                MAIL_TYPEHASH +
                meta.from +
                meta.signer+
                meta.to +
                meta.function +
                meta.deadline +
                meta.maxGas +
                meta.nonce +
                StdLib.Serialize(meta.data)
            );
        }

        private static long _preExecution(BigInteger value, UInt160 contract){
            Assert(value<=DepositStorage.Balance(contract), "Insufficient GAS balance.");
            return Runtime.GasLeft;
        }


        private static bool _onExecution(MetaTransaction metaTx){
            if (metaTx.to is not null && ContractManagement.GetContract(metaTx.to) is not null)
            try{
                Contract.Call(metaTx.to, metaTx.function, CallFlags.All, metaTx.from, 0, metaTx.data);
            }catch{
                return false;
            }
                
            return true;
        }

        private static long _postExecution(){
            return Runtime.GasLeft;
        }

        public static bool ExecuteMetaTx(MetaTransaction metaTx, string domainName, ByteString signature)
        {
            var gasLeft = _preExecution(metaTx.maxGas, metaTx.to);
            Assert(VerifySignature(metaTx, signature), "Signature verification failed.");

            // Assert(_preExecution(), "Pre-execution failed.");
            _onExecution(metaTx);
            // Assert(_postExecution(), "Post-execution failed.");
            var gasUsed = gasLeft - _postExecution();

            DepositStorage.Consume(metaTx.to, gasUsed);

            return true;
        }



        public static bool VerifySignature(MetaTransaction metaTx, ByteString signature)
        {
            ByteString domainSeparator = Storage.Get(Storage.CurrentContext, "DOMAIN_SEPARATOR");
            ByteString digest = CryptoLib.Sha256(
                0x1901 +
                domainSeparator +
                GetMetaTransactionHash(metaTx)            );

            ExecutionEngine.Assert(metaTx.from == Contract.CreateStandardAccount((ECPoint)metaTx.signer),
                "Wrong public key.");
            return CryptoLib.VerifyWithECDsa(digest, (ECPoint)metaTx.signer, signature, NamedCurve.secp256r1);
        }

        public static void OnNEP17Payment(UInt160 from, BigInteger amount, object data)
        {
            Assert(Runtime.CallingScriptHash == GAS.Hash && CheckAddrValid(from) && !CheckWhetherSelf(from) && amount > 0,
                "OnNEP17Payment: invalid params");
            UInt160 target = (UInt160)data;
            Assert(CheckAddrValid(target) && ContractManagement.GetContract(target) is not null,
                "Target contract invalid.");
            DepositStorage.Deposit(target, amount);
        }

        public static void _deploy(object data, bool update)
        {
            if (update) return;

            GetDomainSeperator();
        }
    }
}