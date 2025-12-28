using FsCheck.Xunit;
using System;
using System.Linq;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;


namespace WalletPropertyTesting.Tests.Properties
{
    public class RepositoryProperties
    {
        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Repository_SaveThenGet_PreservesIdAndBalance(Wallet wallet)
        {
            var repo = new InMemoryWalletRepository();
            repo.Save(wallet);

            var loaded = repo.Get(wallet.Id);

            return loaded.Id == wallet.Id
                   && loaded.Balance.Amount == wallet.Balance.Amount
                   && loaded.History.Count == wallet.History.Count;
        }

        [Property]
        public bool Repository_GetUnknown_Throws()
        {
            var repo = new InMemoryWalletRepository();
            try
            {
                repo.Get(Guid.NewGuid());
                return false;
            }
            catch (KeyNotFoundException)
            {
                return true;
            }
        }

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Repository_All_ContainsSavedWallets(Wallet a, Wallet b)
        {
            var repo = new InMemoryWalletRepository();
            repo.Save(a);
            repo.Save(b);

            var all = repo.All().Select(x => x.Id).ToHashSet();
            return all.Contains(a.Id) && all.Contains(b.Id);
        }
    }
}
