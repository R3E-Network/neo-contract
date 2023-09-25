using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using Neo.SmartContract.Framework.Native;

namespace PriceFeed
{
    public partial class PriceFeed
    {
        private static bool IsOwner() => Runtime.CheckWitness((UInt160)Storage.Get(Storage.CurrentContext, adminKey_()));

        private static void Assert(bool condition, string message) => ExecutionEngine.Assert(condition, message);
        private static void MustBeOwner() => Assert(IsOwner(), "owner only");


        public static void _deploy(object data, bool update)
        {
            if (update) return;
            Storage.Put(Storage.CurrentContext, adminKey_(), ((Transaction)Runtime.ScriptContainer).Sender);
        }
        public static int Version() => 1;
        public static void Update(ByteString nefFile, string manifest)
        {
            MustBeOwner();
            ContractManagement.Update(nefFile, manifest, null);
        }
        public static void NewOwner(UInt160 newowner)
        {
            MustBeOwner();
            Assert(newowner != null && newowner.IsValid, "address only");
            Storage.Put(Storage.CurrentContext, adminKey_(), newowner);
        }

        public static void SetOracle(UInt160 oracle)
        {
            MustBeOwner();
            Assert(oracle != null && oracle.IsValid, "address only");
            Storage.Put(Storage.CurrentContext, oracleKey_(), oracle);
        }
        public void AddData(UInt160 quotaToken, UInt160 baseToken, BigInteger decimals, string description)
        {
            MustBeOwner();
            Storage.Put(Storage.CurrentContext, decimalsKey_(quotaToken, baseToken), decimals);
            Storage.Put(Storage.CurrentContext, descriptionKey_(quotaToken, baseToken), description);
            Storage.Put(Storage.CurrentContext, currentRoundKey_(quotaToken, baseToken), (BigInteger)0);
        }
    }
}
