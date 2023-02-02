using Neo;
using Neo.SmartContract;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract.Framework.Attributes;

namespace GasFreeForwarder
{
    partial class GasFreeForwarder
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

        public static bool SetRelayer(UInt160 relayer)
        {
            Assert(Runtime.CheckWitness(GetOwner()), "SetRelayer: CheckWitness failed, owner-".ToByteArray().Concat(relayer).ToByteString());
            Assert(CheckAddrValid(relayer), "SetRelayer: invalid relayer-".ToByteArray().Concat(relayer).ToByteString());
            RelayerStorage.Put(relayer);
            return true;
        }

        public static bool DeleteRelayer(UInt160 relayer)
        {
            Assert(Runtime.CheckWitness(GetOwner()), "DeleteRelayer: CheckWitness failed, owner-".ToByteArray().Concat(relayer).ToByteString());
            Assert(CheckAddrValid(relayer), "DeleteRelayer: invalid relayer-".ToByteArray().Concat(relayer).ToByteString());
            RelayerStorage.Delete(relayer);
            return true;
        }

        private static void RelayerOnly(UInt160 relayer){
            Assert(CheckAddrValid(relayer) && Runtime.CheckWitness(relayer) && RelayerStorage.Get(relayer), "RelayerOnly: invalid relayer-".ToByteArray().Concat(relayer).ToByteString());
        }
    }
}
