using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace GasFreeForwarder
{
    partial class GasFreeForwarder
    {
        public static readonly byte accessControlPrefix = 0x01; 
        public static readonly byte forwarderPrefix = 0x02; 
        public static readonly byte dataPrefix = 0x02; 

    }
}
