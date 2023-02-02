using System.ComponentModel;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract;

namespace GasFreeForwarder
{

    [DisplayName("GasFreeBase")]
    [ManifestExtra("Author", "Jinghui Liao")]
    [ManifestExtra("Email", "jinghui@wayne.edu")]
    [ManifestExtra("Description", "This is a gasfreeforwarder contract for testing")]
    [ContractPermission("*", "*")]
    public partial class GasFreeBase : SmartContract
    {
        private const string VERSION = "v0";

        [InitialValue("NbVj8GhwToNv4WF2gVaoco6hbkMQ8hrHWP", ContractParameterType.Hash160)]
        private static readonly UInt160 Owner;

        [Safe]
        public static UInt160 GetOwner()
        {
            return Owner;
        }

        [Safe]
        public static string Version() => VERSION;

        public static void SetVersion(string version)
        {
            if (!Runtime.CheckWitness(GetOwner())) return;
            Storage.Put(Storage.CurrentContext, "version", version);
        }      

        public static void _deploy(object data, bool update)
        {
            if (update) return;
        }
    }
}