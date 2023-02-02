using System;
using Neo;
using Neo.SmartContract.Framework.Services;

namespace GasFree
{
    partial class GasFree{
        
        private static void Assert(bool condition, string msg)
        {
            if (!condition)
            {
                throw new InvalidOperationException(msg);
            }
        }

        private static bool CheckAddrValid(params UInt160[] addrs)
        {
            bool valid = true;
            foreach (UInt160 addr in addrs)
            {
                valid = valid && addr is not null && addr.IsValid;
                if (!valid)
                    break;
            }

            return valid;
        }
        
        private static bool CheckWhetherSelf(UInt160 fromAddress)
        {
            return fromAddress.Equals(Runtime.ExecutingScriptHash);
        }
        
    }
}