using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace GasFreeForwarder
{
    partial class GasFreeForwarder
    {
        public static class OwnerStorage
        {
            private static readonly byte[] ownerPrefix = { 0x01, 0x02 };

            internal static void Put(UInt160 owner)
            {
                StorageMap map = new(Storage.CurrentContext, ownerPrefix);
                map.Put("owner", owner);
            }

            internal static UInt160 Get()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, ownerPrefix);
                byte[] v = (byte[])map.Get("owner");
                if (v is null || v.Length != 20)
                {
                    return InitialOwner;
                }
                return (UInt160)v;
            }

            internal static void Delete()
            {
                StorageMap map = new(Storage.CurrentContext, ownerPrefix);
                map.Delete("owner");
            }
        }

        public static class PauseStorage
        {
            private static readonly byte[] PausePrefix = { 0x03, 0x04 };

            internal static void Put(BigInteger isPause)
            {
                StorageMap map = new(Storage.CurrentContext, PausePrefix);
                map.Put("PausePrefix", isPause);
            }

            internal static bool Get()
            {
                StorageMap relayerMap = new(Storage.CurrentReadOnlyContext, PausePrefix);
                return (BigInteger)relayerMap.Get("PausePrefix") == 1;
            }
        }


        public static class DepositStorage
        {
            private static readonly byte[] DepositPrefix = { 0x05, 0x06 };

            internal static void Deposit(UInt160 contract, BigInteger amount)
            {
                Assert(amount > 0, "Must deposit a positive value.");
                StorageMap map = new(Storage.CurrentContext, DepositPrefix);
                BigInteger value = (BigInteger)map.Get(contract);

                map.Put(contract, value + amount);
            }

            internal static void Consume(UInt160 contract, BigInteger amount)
            {
                StorageMap map = new(Storage.CurrentContext, DepositPrefix);
                BigInteger value = (BigInteger)map.Get(contract);
                Assert(amount > 0 && value >= amount, "Must consume a positive value.");
                map.Put(contract, value - amount);
            }

            internal static BigInteger Balance(UInt160 contract)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, DepositPrefix);
                return (BigInteger)map.Get(contract);
            }
        }

        // DDOS stands for defense against distributed denial of service attacks
        public static class DDOSStorage
        {
            private static readonly byte[] DDOSPrefix = { 0x07, 0x08 };

            internal static void DOSDetect(UInt160 contract, UInt160 from)
            {
                StorageMap map = new(Storage.CurrentContext, DDOSPrefix);
                var key = contract + from + Ledger.CurrentIndex;
                BigInteger value = (BigInteger)map.Get(key);
                Assert(value < 2, "DOS detected.");
                map.Put(key, value + 1);
            }
        }


        public static class RelayerStorage
        {
            private static readonly byte[] RelayerPrefix = new byte[] { 0x09, 0x0a };

            internal static void Put(UInt160 relayer)
            {
                StorageMap map = new(Storage.CurrentContext, RelayerPrefix);
                map.Put(relayer, 1);
            }

            internal static void Delete(UInt160 relayer)
            {
                StorageMap map = new(Storage.CurrentContext, RelayerPrefix);
                map.Delete(relayer);
            }

            internal static bool Get(UInt160 relayer)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, RelayerPrefix);
                return (BigInteger)map.Get(relayer) == 1;
            }

            internal static BigInteger Count()
            {
                StorageMap relayerMap = new(Storage.CurrentReadOnlyContext, RelayerPrefix);
                var iterator = relayerMap.Find();
                BigInteger count = 0;
                while (iterator.Next())
                {
                    count++;
                }
                return count;
            }

            internal static UInt160[] Find(BigInteger count)
            {
                StorageMap relayerMap = new(Storage.CurrentReadOnlyContext, RelayerPrefix);
                var iterator = relayerMap.Find(FindOptions.RemovePrefix | FindOptions.KeysOnly);
                UInt160[] addrs = new UInt160[(uint)count];
                uint i = 0;
                while (iterator.Next())
                {
                    addrs[i] = (UInt160)iterator.Value;
                    i++;
                }
                return addrs;
            }
        }
    }
}
