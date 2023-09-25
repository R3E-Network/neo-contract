using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;

namespace PriceFeed
{
    public partial class PriceFeed : SmartContract
    {
        public static readonly byte adminPrefix = 0x00;
        public static readonly byte oraclePrefix = 0x01;
        public static readonly byte decimalsPrefix = 0x02;
        public static readonly byte descriptionPrefix = 0x03;
        public static readonly byte dataPrefix = 0x04;
        public static readonly byte currentRoundPrefix = 0x05;


        internal static byte[] adminKey_() => new byte[]{adminPrefix};
        internal static byte[] oracleKey_() => new byte[]{oraclePrefix};
        internal string decimalsKey_(UInt160 quotaToken, UInt160 baseToken) => decimalsPrefix + quotaToken + baseToken;
        internal string descriptionKey_(UInt160 quotaToken, UInt160 baseToken) => descriptionPrefix + quotaToken + baseToken;
        internal string dataKey_(UInt160 quotaToken, UInt160 baseToken, BigInteger roundId) => dataPrefix + quotaToken + baseToken + roundId;
        internal string currentRoundKey_(UInt160 quotaToken, UInt160 baseToken) => currentRoundPrefix + quotaToken + baseToken;
    }
}
