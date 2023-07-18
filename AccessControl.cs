using Neo;
using System;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Services;
using Neo.SmartContract.Framework.Attributes;
using System.ComponentModel;

namespace GasFreeForwarder
{
    partial class GasFreeForwarder
    {
        internal string roleExistenceKey(UInt256 role, UInt160 account) => accessControlPrefix + role + account;

        [DisplayName("RoleGranted")]   // role, account, sender
        public static event Action<UInt256, UInt160, UInt160> RoleGranted;
        [DisplayName("RoleRevoked")]   // role, account, sender
        public static event Action<UInt256, UInt160, UInt160> RoleRevoked;

        public static readonly UInt256 ADMIN_ROLE = UInt256.Zero;

        [Safe]
        public virtual bool hasRole(UInt256 role, UInt160 account)
        {
            ByteString key = Storage.Get(Storage.CurrentReadOnlyContext, roleExistenceKey(role, account));
            if (key is null || key == "") return false;
            else return key == "true";
        }

        internal void checkRole_(UInt256 role, UInt160 account)
        {
            if (!hasRole(role, account))
            {
                throw new Exception("AccessControl: account " + account + " is missing role " + role);
            }
        }

        public virtual void grantRole(UInt256 role, UInt160 account, UInt160 sender)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(sender), "!permission");
            checkRole_(ADMIN_ROLE, sender);
            grantRole_(role, account, sender);
        }

        public virtual void revokeRole(UInt256 role, UInt160 account, UInt160 sender)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(sender), "!permission");
            checkRole_(ADMIN_ROLE, sender);
            revokeRole_(role, account, sender);
        }

        public virtual void renounceRole(UInt256 role, UInt160 account)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(account), "!permission");
            revokeRole_(role, account, account);
        }

        internal virtual void grantRole_(UInt256 role, UInt160 account, UInt160 sender)
        {
            if (!hasRole(role, account))
            {
                Storage.Put(Storage.CurrentContext, roleExistenceKey(role, account), "true");
                RoleGranted(role, account, sender);
            }
        }

        internal virtual void revokeRole_(UInt256 role, UInt160 account, UInt160 sender)
        {
            if (hasRole(role, account))
            {
                Storage.Delete(Storage.CurrentContext, roleExistenceKey(role, account));
                RoleRevoked(role, account, sender);
            }
        }
    }
}
