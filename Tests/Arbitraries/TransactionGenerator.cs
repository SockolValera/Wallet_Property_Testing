using FsCheck;
using FsCheck.Fluent;
using WalletPropertyTesting.Domain;

namespace WalletPropertyTesting.Tests.Arbitraries
{
    public static class TransactionGenerator
    {
        public static Arbitrary<Transaction> Transaction()
        {
            var gen =
                from money in MoneyGenerator.Money().Generator
                from isDeposit in Gen.Elements(true, false)
                select isDeposit
                    ? Domain.Transaction.Deposit(money)
                    : Domain.Transaction.Withdraw(money);

            return Arb.From(gen);
        }
    }
}
