using FsCheck.Xunit;
using System;
using WalletPropertyTesting.Domain;
using WalletPropertyTesting.Tests.Arbitraries;

namespace WalletPropertyTesting.Tests.Properties
{
    public class MoneyProperties
    {
        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Money_Amount_IsNeverNegative(Money m) => m.Amount >= 0m;

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Money_Addition_IsCommutative(Money a, Money b)
            => (a + b).Amount == (b + a).Amount;

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Money_Subtraction_Works_WhenEnoughFunds(MoneyPair pair)
        {
            var r = pair.A - pair.B;
            return r.Amount == pair.A.Amount - pair.B.Amount && r.Amount >= 0m;
        }

        [Property(Arbitrary = new[] { typeof(WalletArbitraries) })]
        public bool Money_Subtraction_Throws_WhenInsufficientFunds(MoneyInsufficientPair pair)
        {
            try
            {
                _ = pair.A - pair.B;
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        [Property]
        public bool Money_CannotBeCreatedNegative(decimal amount)
        {
            if (amount >= 0m)
            {
                _ = new Money(amount);
                return true;
            }

            try
            {
                _ = new Money(amount);
                return false;
            }
            catch (ArgumentException)
            {
                return true;
            }
        }
    }
}

