using Neo;
using System;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract.Framework.Attributes;
using System.ComponentModel;
using System.Numerics;
using Neo.SmartContract.Framework.Native;
using Neo.Cryptography.ECC;

namespace R3E
{
    partial class R3E
    {
        internal string nonceUsedKey(UInt160 addr, BigInteger nonce) => forwarderPrefix + addr + nonce;

        public struct ForwardRequest {
            public UInt160 from;
            public ECPoint pubkey;
            public UInt160 to;
            public long gas;
            public uint nonce;
            public string method;
            public object[] args;
        }
        public struct ExecRet {
            public bool succ;
            public object ret;

        }
        public bool nonceUsed(UInt160 addr, BigInteger nonce) {
            ByteString key = Storage.Get(Storage.CurrentReadOnlyContext, nonceUsedKey(addr, nonce));
            if (key is null || key == "") return false;
            else return key == "true";
        }

        public ByteString serializeHelper(object[] args) => StdLib.Serialize(args);
        public ByteString msgHelper(ForwardRequest req) {
            BigInteger network = Runtime.GetNetwork();
            byte[] networkB = new byte[] { (byte)(network % 256), (byte)((network >> 8) % 256), (byte)((network >> 16) % 256), (byte)((network >> 24) % 256) };
            ByteString fakeScript = StdLib.Serialize(new object[] { req.to, req.method, req.args });
            byte[] fakeTransaction = new byte[] { 0x00, (byte)(req.nonce % 256), (byte)((req.nonce >> 8) % 256), (byte)((req.nonce >> 16) % 256), (byte)((req.nonce >> 24) % 256), (byte)(req.gas % 256), (byte)((req.gas >> 8) % 256), (byte)((req.gas >> 16) % 256), (byte)((req.gas >> 24) % 256), (byte)((req.gas >> 32) % 256), (byte)((req.gas >> 40) % 256), (byte)((req.gas >> 48) % 256), (byte)((req.gas >> 56) % 256), 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };
            fakeTransaction = fakeTransaction.Concat(writeVarInt(fakeScript.Length+3));
            fakeTransaction = fakeTransaction.Concat(new byte[] { 0x0d, (byte)(fakeScript.Length % 256), (byte)(fakeScript.Length >> 8 % 256) });
            fakeTransaction = fakeTransaction.Concat(fakeScript);

            byte[] msg = networkB.Concat(CryptoLib.Sha256((ByteString)fakeTransaction));
            return (ByteString)msg;
        }

        public bool verifySig(ForwardRequest req, ByteString signature) {
            if (req.from != Contract.CreateStandardAccount(req.pubkey))
                return false;
            return CryptoLib.VerifyWithECDsa(msgHelper(req), req.pubkey, signature, NamedCurve.secp256r1);
        }

        public ExecRet execute(ForwardRequest req, ByteString signature) {
            ExecutionEngine.Assert(verifySig(req,signature), "!sig");
            Storage.Put(Storage.CurrentContext, nonceUsedKey(req.from, req.nonce), "true");
            if (Runtime.GasLeft < req.gas) {
                throw new Exception("insufficient GAS");
            }

            bool succ = true;
            object ret = null;
            try{
                ret = Contract.Call(req.to, req.method, CallFlags.All, req.from, req.args);
            } catch {
                succ = false;
            }

            return new ExecRet{succ=succ, ret=ret};
        }

        private byte[] writeVarInt(int value) {
            if (value < 0 || value > 0xFFFF)
                throw new Exception("var int not in range");
            if (value < 0xFD)
                return new byte[]{(byte)value};
            else
                return new byte[]{(byte)0xFD, (byte)(value % 256), (byte)(value >> 8 % 256)};
        }
    }
}