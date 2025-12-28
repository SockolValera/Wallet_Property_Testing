using FsCheck;
using FsCheck.Fluent;
using System;
using System.Collections.Generic;

namespace WalletPropertyTesting.Tests.Arbitraries
{
    public enum WalletOpType { Deposit, Withdraw }
    public record WalletOperation(WalletOpType Type, decimal Amount);
    public record ValidOperationSequence(IReadOnlyList<WalletOperation> Ops);

    public static class OperationGenerator
    {
        private const int MaxWhole = 50_000;

        private static Gen<decimal> AmountGen()
        {
            var edge = Gen.Elements(0m, 0.01m, 1m, 10m, 100m, 10_000m, 50_000m);

            var random =
                from whole in Gen.Choose(0, MaxWhole)
                from cents in Gen.Choose(0, 99)
                select whole + cents / 100m;

            return Gen.OneOf(edge, random);
        }

        public static Arbitrary<WalletOperation> Operation()
        {
            var gen =
                from amount in AmountGen()
                from t in Gen.Elements(WalletOpType.Deposit, WalletOpType.Withdraw)
                select new WalletOperation(t, amount);

            return Arb.From(gen);
        }

        public static Arbitrary<ValidOperationSequence> ValidOperationSequence()
        {
            Gen<ValidOperationSequence> gen = Gen.Sized(size =>
            {
                var lenGen = Gen.Choose(0, Math.Min(size, 50));
                return lenGen.SelectMany(len =>
                    BuildOps(len, 0m, new List<WalletOperation>())
                        .Select(ops => new ValidOperationSequence(ops)));
            });

            return Arb.From(gen);
        }

        private static Gen<IReadOnlyList<WalletOperation>> BuildOps(int remaining, decimal balance, List<WalletOperation> acc)
        {
            if (remaining <= 0)
                return Gen.Constant((IReadOnlyList<WalletOperation>)acc);

            var depositGen =
                from a in AmountGen()
                select (op: new WalletOperation(WalletOpType.Deposit, a), newBalance: balance + a);

            Gen<(WalletOperation op, decimal newBalance)> withdrawGen;
            if (balance <= 0m)
            {
                withdrawGen = depositGen;
            }
            else
            {
                withdrawGen =
                    from frac in Gen.Choose(0, 10_000)
                    let amt = Math.Round(balance * (frac / 10_000m), 2)
                    select (op: new WalletOperation(WalletOpType.Withdraw, amt), newBalance: balance - amt);
            }

            var stepGen = Gen.OneOf(depositGen, withdrawGen);

            return stepGen.SelectMany(step =>
            {
                var nextAcc = new List<WalletOperation>(acc) { step.op };
                return BuildOps(remaining - 1, step.newBalance, nextAcc);
            });
        }
    }
}
