﻿using System;
using System.ComponentModel;
using System.Numerics;
using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;


namespace example
{
    [DisplayName("example")]
    [ManifestExtra("Author", "NEO")]
    [ManifestExtra("Email", "developer@neo.org")]
    [ContractPermission("*", "*")]
    [ManifestExtra("Description", "This is a example")]
    public class example : SmartContract
    {
        [DisplayName("Win")]   // account
        public static event Action<UInt160> Win;

        public struct OraclePayload
        {
            public UInt256 hashkey;
            public object data;
            public ulong timestamp;
        }

        public static readonly UInt256 HASHKEY = (UInt256)CryptoLib.Sha256("RANDOM");
        public static readonly BigInteger MOD = 1048576;


        [InitialValue("NPSMt42Pa2zSTt4rd2duZ5KMLTnkHYNAD9", ContractParameterType.Hash160)]
        private static readonly UInt160 R3E = default;

        // must be two parameters, the first is the account verified by R3E, the second is an array accept all other needed args
        public static void Play(UInt160 account, object[] args)
        {
            OraclePayload data = (OraclePayload)Contract.Call(R3E, "data", CallFlags.ReadOnly, HASHKEY);
            ExecutionEngine.Assert(data.hashkey == HASHKEY, "oracle record not exist");
            ExecutionEngine.Assert(data.timestamp > (BigInteger)Runtime.Time + 30_000, "oracle record exipred");

            if ((BigInteger)CryptoLib.Sha256((ByteString)data.data) % MOD == 0) {
                Win(account);
            }
        }
    }
}
