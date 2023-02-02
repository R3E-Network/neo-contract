using System.Numerics;
using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Native;
using Neo.SmartContract.Framework.Services;

namespace GasFree
{
    partial class GasFree
    {
        public static class OwnerStorage
        {
            private static readonly byte[] ownerPrefix = { 0x03, 0x02 };

            internal static void Put(UInt160 owner)
            {
                StorageMap map = new(Storage.CurrentContext, ownerPrefix);
                map.Put("owner", owner);
            }

            internal static UInt160 Get()
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, ownerPrefix);
                byte[] v = (byte[])map.Get("owner");
                if(v is null || v.Length != 20)
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
            private static readonly byte[] PausePrefix = { 0x09, 0x03 };

            internal static void Put(BigInteger isPause)
            {
                StorageMap map = new(Storage.CurrentContext, PausePrefix);
                map.Put("PausePrefix", isPause);
            }

            internal static bool Get()
            {
                StorageMap authorMap = new(Storage.CurrentReadOnlyContext, PausePrefix);
                return (BigInteger)authorMap.Get("PausePrefix") == 1;
            }
        }
        

        public static class DepositStorage
        {
            private static readonly byte[] DepositPrefix = { 0x04, 0x01 };

            internal static void Deposit(UInt160 contract, BigInteger amount)
            {
                Assert(amount>0, "Must deposit a positive value.");
                StorageMap map = new(Storage.CurrentContext, DepositPrefix);
                BigInteger value = (BigInteger)map.Get(contract);
                
                map.Put(contract, value+amount);
            }

            internal static void Consume(UInt160 contract, BigInteger amount)
            {
                StorageMap map = new(Storage.CurrentContext, DepositPrefix);
                BigInteger value = (BigInteger)map.Get(contract);
                Assert(amount>0 && value >= amount, "Must consume a positive value.");
                map.Put(contract, value-amount);
            }

            internal static BigInteger Balance(UInt160 contract)
            {
                StorageMap map = new(Storage.CurrentReadOnlyContext, DepositPrefix);
                return (BigInteger)map.Get(contract);
            }
        }
    }
}
