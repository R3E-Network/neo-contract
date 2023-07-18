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
    public partial class R3E : SmartContract
    {
        internal string dataKey_(UInt256 hashkey) => dataPrefix + hashkey;

        [DisplayName("Execution")]   // success, returndata, req
        public static event Action<bool, object, ForwardRequest> Execution;

        public static readonly UInt256 ORACLE = (UInt256)CryptoLib.Sha256("ORACLE");

        public struct OraclePayload
        {
            public UInt256 hashkey;
            public object data;
            public ulong timestamp;
        }

        public void _deploy(object data, bool update)
        {
            if (!update)
            {
                UInt160 sender = ((Transaction)Runtime.ScriptContainer).Sender;
                grantRole_(ADMIN_ROLE, sender, sender);
            }
        }

        public void executeWithData(OraclePayload[] op,
            ForwardRequest req,
            ByteString signature)
        {
            UInt160 sender = ((Transaction)Runtime.ScriptContainer).Sender;
            checkRole_(ORACLE, sender);
            ExecutionEngine.Assert(Runtime.CheckWitness(sender), "no permission");

            for (uint i = 0; i < op.Length; i++) {
                Storage.Put(Storage.CurrentContext, dataKey_(op[i].hashkey), StdLib.Serialize(op[i]));
            }

            ExecRet ret = execute(req, signature);
            Execution(ret.succ, ret.ret, req);

            for (uint i = 0; i < op.Length; i++) {
                Storage.Delete(Storage.CurrentContext, dataKey_(op[i].hashkey));
            }
        }

        public OraclePayload data(UInt256 hashkey) {
            return (OraclePayload)StdLib.Deserialize(Storage.Get(Storage.CurrentContext, dataKey_(hashkey)));
        }

        public void grantOracleRole(UInt160 oracle, UInt160 admin) {
            grantRole(ORACLE, oracle, admin);
        }
        public void revokeOracleRole(UInt160 oracle, UInt160 admin) {
            revokeRole(ORACLE, oracle, admin);
        }
    }
}
