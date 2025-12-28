using FsCheck.Xunit;
using System.Linq;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;


namespace WalletPropertyTesting.Tests.Properties
{
    public class WalletProperties
    {
        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Wallet_Deposit_IncreasesBalance_AndAddsHistory(Money amount)
        {
            var w = new Wallet();
            var before = w.Balance.Amount;

            w.Deposit(amount);

            return w.Balance.Amount == before + amount.Amount
                   && w.History.Count == 1
                   && w.History[0].Type == TransactionType.Deposit
                   && w.History[0].Amount.Amount == amount.Amount;
        }

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Wallet_Withdraw_DecreasesBalance_AndAddsHistory(Money deposit, Money withdraw)
        {
            var d = deposit;
            var wAmt = withdraw.Amount <= deposit.Amount ? withdraw : new Money(deposit.Amount);

            var w = new Wallet();
            w.Deposit(d);
            var before = w.Balance.Amount;

            w.Withdraw(wAmt);

            return w.Balance.Amount == before - wAmt.Amount
                   && w.History.Last().Type == TransactionType.Withdraw
                   && w.History.Last().Amount.Amount == wAmt.Amount;
        }

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Wallet_HistoryCount_Equals_OperationsCount(ValidOperationSequence seq)
        {
            var w = new Wallet();
            foreach (var op in seq.Ops)
            {
                if (op.Type == WalletOpType.Deposit)
                    w.Deposit(new Money(op.Amount));
                else
                    w.Withdraw(new Money(op.Amount));
            }

            return w.History.Count == seq.Ops.Count;
        }

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Wallet_FinalBalance_Equals_SumDepositsMinusWithdraws(ValidOperationSequence seq)
        {
            var w = new Wallet();
            foreach (var op in seq.Ops)
            {
                if (op.Type == WalletOpType.Deposit)
                    w.Deposit(new Money(op.Amount));
                else
                    w.Withdraw(new Money(op.Amount));
            }

            var deposits = w.History.Where(t => t.Type == TransactionType.Deposit).Sum(t => t.Amount.Amount);
            var withdraws = w.History.Where(t => t.Type == TransactionType.Withdraw).Sum(t => t.Amount.Amount);

            return w.Balance.Amount == deposits - withdraws;
        }

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Wallet_ReplayingHistory_LeadsToSameBalance(Wallet wallet)
        {
            var replay = new Wallet();

            foreach (var t in wallet.History)
            {
                if (t.Type == TransactionType.Deposit)
                    replay.Deposit(t.Amount);
                else
                    replay.Withdraw(t.Amount);
            }

            return replay.Balance.Amount == wallet.Balance.Amount
                   && replay.History.Count == wallet.History.Count;
        }
    }
}
