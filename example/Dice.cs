using System;
using System.ComponentModel;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;

namespace example
{
    [DisplayName("example")]
    [ManifestExtra("Author", "NEO")]
    [ManifestExtra("Email", "developer@neo.org")]
    [ManifestExtra("Email", "developer@neo.org")]
    [ManifestExtra("Description", "This is a example")]
    public class example : SmartContract
    {
        public static bool Main()
        {
            return true;
        }
    }
}
