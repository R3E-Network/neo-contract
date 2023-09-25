using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract.Framework.Native;
using System.Numerics;

namespace PriceFeed
{
    public partial class PriceFeed : SmartContract
    {
        private static bool IsOracle() => Runtime.CheckWitness((UInt160)Storage.Get(Storage.CurrentContext, oracleKey_()));
        private static void MustBeOracle() => Assert(IsOracle(), "oracle only");
        
        public void putRoundData(UInt160 quotaToken, UInt160 baseToken, BigInteger roundId, PriceFeedData data)
        {
            MustBeOracle();
            BigInteger currentRound = (BigInteger)Storage.Get(Storage.CurrentReadOnlyContext, currentRoundKey_(quotaToken, baseToken));
            data.updatedAt = Runtime.Time;
            data.roundId = currentRound + 1;
            Storage.Put(Storage.CurrentContext, dataKey_(quotaToken, baseToken, roundId), StdLib.Serialize(data));
            Storage.Put(Storage.CurrentContext, currentRoundKey_(quotaToken, baseToken), data.roundId);
        }
    }
}
