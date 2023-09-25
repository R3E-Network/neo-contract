﻿using System;
using System.ComponentModel;
using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using Neo.Cryptography.ECC;
using Neo.SmartContract.Framework.Native;

namespace MinimalForwarder
{
    [DisplayName("MinimalForwarder")]
    [ContractPermission("*", "*")]
    [ManifestExtra("Author", "Hecate2")]
    [ManifestExtra("Description", "openzeppelin MinimalForwarder")]
    public partial class MinimalForwarder : SmartContract
    {
        public struct ForwardRequest
        {
            public UInt160 from;
            public ECPoint pubkey;
            public UInt160 to;
            public BigInteger value;  // ETH value, though not available in Neo. Set to 0 for usual
            public BigInteger gas;
            public BigInteger nonce;
            public string method;
            public object[] args;
        }

        private const byte PREFIX_NONCES = 0x99;
        public StorageMap _nonces => new StorageMap(PREFIX_NONCES);  // address + nonce used -> "t"

        [Safe]
        public bool NonceUsed(UInt160 addr, BigInteger nonce)
        {
            ByteString value = _nonces[addr + nonce];
            if (value is null || value == "") return false;
            else return value == "t";
        }

        public ByteString DataToVerify(ForwardRequest req) => hashTypedDataV4(keccak256(abiencode(_TYPE_HASH, req.from, req.to, req.value, req.gas, req.nonce, keccak256(StdLib.Serialize(new object[] { req.method, req.args })))));

        public bool Verify(ForwardRequest req, ByteString signature)
        {
            // The public key is generated by secp256k1, not r1
            //if (req.from != Contract.CreateStandardAccount(req.pubkey))
            //    return false;
            return CryptoLib.VerifyWithECDsa(DataToVerify(req), req.pubkey, signature, NamedCurve.secp256k1);
        }

        public struct ExecRet
        {
            public bool succ;
            public object ret;
        }
        public ExecRet execute(ForwardRequest req, ByteString signature)
        {
            ExecutionEngine.Assert(Verify(req, signature), "!sig");
            ExecutionEngine.Assert(!NonceUsed(req.from, req.nonce), "!nonce");
            _nonces[req.from + req.nonce] = "t";
            if (Runtime.GasLeft < req.gas)
                throw new Exception("Insufficient GAS");

            bool succ = true;
            object ret = null;
            try
            {
                ret = Contract.Call(req.to, req.method, CallFlags.All, req.from, req.args);
            }
            catch
            {
                succ = false;
            }

            return new ExecRet { succ=succ, ret=ret };
        }
    }
}
