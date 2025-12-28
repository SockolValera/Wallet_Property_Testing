using FsCheck;
using FsCheck.Fluent;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests
{
    public static class WalletArbitraries
    {
        public static Arbitrary<Money> Money() => MoneyGenerator.Money();
        public static Arbitrary<MoneyPair> MoneyPairForSubtraction() => MoneyGenerator.MoneyPairForSubtraction();
        public static Arbitrary<MoneyInsufficientPair> MoneyPairInsufficient() => MoneyGenerator.MoneyPairInsufficient();
        public static Arbitrary<Transaction> Transaction() => TransactionGenerator.Transaction();
        public static Arbitrary<WalletOperation> WalletOperation() => OperationGenerator.Operation();
        public static Arbitrary<ValidOperationSequence> ValidOperationSequence() => OperationGenerator.ValidOperationSequence();

        public static Arbitrary<Wallet> Wallet()
        {
            var gen =
                from seq in ValidOperationSequence().Generator
                select BuildWallet(seq);

            return Arb.From(gen);
        }

        private static Wallet BuildWallet(ValidOperationSequence seq)
        {
            var w = new Wallet();
            foreach (var op in seq.Ops)
            {
                if (op.Type == WalletOpType.Deposit)
                    w.Deposit(new Money(op.Amount));
                else
                    w.Withdraw(new Money(op.Amount));
            }
            return w;
        }
    }
}
