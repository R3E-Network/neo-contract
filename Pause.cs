using Neo;
using Neo.SmartContract.Framework;
using Neo.SmartContract.Framework.Attributes;
using Neo.SmartContract.Framework.Services;

namespace GasFree
{
    partial class GasFree
    {
        [Safe]
        public static bool IsPaused()
        {
            return PauseStorage.Get();
        }

        public static bool Pause(UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "Pause: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "Pause: not author".ToByteArray().Concat(author).ToByteString());
            PauseStorage.Put(1);
            return true;
        }

        public static bool UnPause(UInt160 author)
        {
            Assert(Runtime.CheckWitness(author), "Unpause: CheckWitness failed, author-".ToByteArray().Concat(author).ToByteString());
            Assert(IsAuthor(author), "Unpause: not author".ToByteArray().Concat(author).ToByteString());
            PauseStorage.Put(0);
            return true;
        }

    }
}
