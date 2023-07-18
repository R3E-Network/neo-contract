using Neo;
using System;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract.Framework.Attributes;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework.Native;
using Neo.Cryptography.ECC;

namespace GasFreeForwarder
{
    partial class GasFreeForwarder
    {
        internal string _dataKey(UInt256 hashkey) => dataPrefix + hashkey;
        
        public static readonly UInt256 ORACLE = (UInt256)CryptoLib.Sha256("ORACLE");

        public struct OraclePayload {
            public UInt256 hashkey;
            public object data;
            public UInt256 timestamp;
        }

        



    }
}
