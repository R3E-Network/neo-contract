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
        internal string _roleExistenceKey(UInt256 role, UInt160 account) => accessControlPrefix + role + account;

        [DisplayName("RoleGranted")]   // role, account, sender
        public static event Action<UInt256, UInt160, UInt160> RoleGranted;
        [DisplayName("RoleRevoked")]   // role, account, sender
        public static event Action<UInt256, UInt160, UInt160> RoleRevoked;

        public static readonly UInt256 ADMIN_ROLE = UInt256.Zero;

        [Safe]
        public virtual bool hasRole(UInt256 role, UInt160 account)
        {
            ByteString key = Storage.Get(Storage.CurrentReadOnlyContext, _roleExistenceKey(role, account));
            if (key is null || key == "") return false;
            else return key == "true";
        }

        internal void _checkRole(UInt256 role, UInt160 account)
        {
            if (!hasRole(role, account))
            {
                throw new Exception("AccessControl: account " + account + " is missing role " + role);
            }
        }

        public virtual void grantRole(UInt256 role, UInt160 account, UInt160 sender)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(sender), "!permission");
            _checkRole(ADMIN_ROLE, sender);
            _grantRole(role, account, sender);
        }

        public virtual void revokeRole(UInt256 role, UInt160 account, UInt160 sender)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(sender), "!permission");
            _checkRole(ADMIN_ROLE, sender);
            _revokeRole(role, account, sender);
        }

        public virtual void renounceRole(UInt256 role, UInt160 account)
        {
            ExecutionEngine.Assert(Runtime.CheckWitness(account), "!permission");
            _revokeRole(role, account, account);
        }

        internal virtual void _grantRole(UInt256 role, UInt160 account, UInt160 sender)
        {
            if (!hasRole(role, account))
            {
                Storage.Put(Storage.CurrentContext, _roleExistenceKey(role, account), "true");
                RoleGranted(role, account, sender);
            }
        }

        internal virtual void _revokeRole(UInt256 role, UInt160 account, UInt160 sender)
        {
            if (hasRole(role, account))
            {
                Storage.Delete(Storage.CurrentContext, _roleExistenceKey(role, account));
                RoleRevoked(role, account, sender);
            }
        }
    }
}
