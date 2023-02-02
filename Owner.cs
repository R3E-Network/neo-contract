using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using System.Numerics;
using Neo.SmartContract.Framework.Attributes;

namespace GasFree
{
    partial class GasFree
    {
        [InitialValue("NbVj8GhwToNv4WF2gVaoco6hbkMQ8hrHWP", ContractParameterType.Hash160)]
        private static readonly UInt160 InitialOwner;

        [Safe]
        public static UInt160 GetOwner()
        {
            return OwnerStorage.Get();
        }

        public static bool SetOwner(UInt160 owner)
        {
            Assert(Runtime.CheckWitness(GetOwner()), "SetOwner: CheckWitness failed, owner-".ToByteArray().Concat(owner).ToByteString());
            Assert(CheckAddrValid(owner), "SetOwner: invalid owner-".ToByteArray().Concat(owner).ToByteString());
            OwnerStorage.Put(owner);
            return true;
        }
    }
}
