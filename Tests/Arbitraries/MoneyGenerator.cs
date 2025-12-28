using FsCheck;
using FsCheck.Fluent;
using System;
using WalletPropertyTesting.Domain;

namespace WalletPropertyTesting.Tests.Arbitraries
{
    public record MoneyPair(Money A, Money B);                // B <= A
    public record MoneyInsufficientPair(Money A, Money B);    // B > A

    public static class MoneyGenerator
    {
        private const int MaxWhole = 1_000_000;

        public static Gen<decimal> AmountGen()
        {
            var edgeCases = Gen.Elements(0m, 0.01m, 0.10m, 1m, 10m, 100m, 999_999.99m, 1_000_000m);

            var randomMoney =
                from whole in Gen.Choose(0, MaxWhole)
                from cents in Gen.Choose(0, 99)
                select whole + cents / 100m;

            return Gen.OneOf(edgeCases, randomMoney);
        }

        public static Arbitrary<Money> Money()
        {
            var gen = AmountGen().Select(a => new Money(a));
            return Arb.From(gen);
        }

        public static Arbitrary<MoneyPair> MoneyPairForSubtraction()
        {
            var gen =
                from a in AmountGen()
                from k in Gen.Choose(0, 10_000)
                let bAmount = (a == 0m) ? 0m : Math.Round(a * (k / 10_000m), 2)
                select new MoneyPair(new Money(a), new Money(bAmount));

            return Arb.From(gen);
        }

        public static Arbitrary<MoneyInsufficientPair> MoneyPairInsufficient()
        {
            var gen =
                from a in AmountGen()
                from extra in AmountGen()
                let extraNonZero = extra == 0m ? 0.01m : extra
                let b = a + extraNonZero
                select new MoneyInsufficientPair(new Money(a), new Money(b));

            return Arb.From(gen);
        }
    }
}
