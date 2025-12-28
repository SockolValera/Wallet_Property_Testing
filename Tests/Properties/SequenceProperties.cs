using FsCheck.Xunit;
using System.Linq;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;


namespace WalletPropertyTesting.Tests.Properties
{
    public class SequenceProperties
    {
        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool WalletService_ApplyingValidSequence_DoesNotThrow_AndBalanceMatches(ValidOperationSequence seq)
        {
            var repo = new InMemoryWalletRepository();
            var service = new WalletService(repo);

            var wallet = service.CreateWallet();

            foreach (var op in seq.Ops)
            {
                if (op.Type == WalletOpType.Deposit)
                    service.Deposit(wallet.Id, op.Amount);
                else
                    service.Withdraw(wallet.Id, op.Amount);
            }

            var fromService = service.GetBalance(wallet.Id).Amount;
            var fromRepo = repo.Get(wallet.Id).Balance.Amount;

            return fromService == fromRepo
                   && fromService >= 0m;
        }

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool WalletService_BalanceEqualsNetOperations(ValidOperationSequence seq)
        {
            var repo = new InMemoryWalletRepository();
            var service = new WalletService(repo);

            var wallet = service.CreateWallet();

            foreach (var op in seq.Ops)
            {
                if (op.Type == WalletOpType.Deposit)
                    service.Deposit(wallet.Id, op.Amount);
                else
                    service.Withdraw(wallet.Id, op.Amount);
            }

            var deposits = seq.Ops.Where(o => o.Type == WalletOpType.Deposit).Sum(o => o.Amount);
            var withdraws = seq.Ops.Where(o => o.Type == WalletOpType.Withdraw).Sum(o => o.Amount);

            return service.GetBalance(wallet.Id).Amount == deposits - withdraws;
        }
    }
}
