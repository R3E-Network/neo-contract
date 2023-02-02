using System;
using System.ComponentModel;
using System.Numerics;

namespace GasFreeForwarder
{
    partial class GasFreeForwarder
    {
        [DisplayName("transfer")]
        public static event Action<byte[], byte[], BigInteger> Transferred;

        [DisplayName("deposited")]
        public static event Action<byte[], BigInteger> Deposited;

        [DisplayName("executed")]
        public static event Action<BigInteger> Executed;
    }
}