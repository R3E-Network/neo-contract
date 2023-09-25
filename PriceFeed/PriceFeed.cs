using System.ComponentModel;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using Neo.SmartContract.Framework.Native;

namespace PriceFeed
{
    [DisplayName("PriceFeed")]
    [ManifestExtra("Author", "NEO")]
    [ManifestExtra("Email", "developer@neo.org")]
    [ManifestExtra("Description", "This is a R3E PriceFeed")]
    public partial class PriceFeed : SmartContract
    {
        public class PriceFeedData {
            public BigInteger roundId;
            public BigInteger answer;
            public ulong startedAt;
            public ulong updatedAt;
        }

        public BigInteger Decimals(UInt160 quotaToken, UInt160 baseToken)
        {
            return (BigInteger)Storage.Get(Storage.CurrentReadOnlyContext, decimalsKey_(quotaToken, baseToken));
        }
        public string Description(UInt160 quotaToken, UInt160 baseToken)
        {
            return Storage.Get(Storage.CurrentReadOnlyContext, descriptionKey_(quotaToken, baseToken));
        }
        
        public PriceFeedData getRoundData(UInt160 quotaToken, UInt160 baseToken, BigInteger roundId)
        {
            return (PriceFeedData)StdLib.Deserialize(Storage.Get(Storage.CurrentReadOnlyContext, dataKey_(quotaToken, baseToken, roundId)));
        }
        
        public PriceFeedData latestRoundData(UInt160 quotaToken, UInt160 baseToken, BigInteger roundId)
        {
            var currentRound = (BigInteger)Storage.Get(Storage.CurrentReadOnlyContext, currentRoundKey_(quotaToken, baseToken));
            return (PriceFeedData)StdLib.Deserialize(Storage.Get(Storage.CurrentReadOnlyContext, dataKey_(quotaToken, baseToken, currentRound)));
        }
    }
}
