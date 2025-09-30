using Ardalis.GuardClauses;

namespace MyModularMonolith.Shared.Domain.ValueObjects
{
    public sealed record Money : ValueObject<decimal>
    {
        public static readonly Money Zero = new(0m);

        private Money(decimal value) : base(value) { }

        public static Money Create(decimal amount)
        {
            Guard.Against.Negative(amount, nameof(amount));
            return new Money(Math.Round(amount, 2, MidpointRounding.AwayFromZero));
        }

        public static implicit operator Money(decimal amount) => Create(amount);

        public Money Add(Money other) 
            => new(Math.Round(Value + other.Value, 2, MidpointRounding.AwayFromZero));
        
        public Money Subtract(Money other) 
        {
            var result = Value - other.Value;
            Guard.Against.Negative(result, nameof(result), "Subtraction would result in negative money amount");
            return new(Math.Round(result, 2, MidpointRounding.AwayFromZero));
        }
        
        public Money SubtractAllowingNegative(Money other)
            => new(Math.Round(Value - other.Value, 2, MidpointRounding.AwayFromZero));
        
        public Money MultiplyBy(decimal factor) 
        {
            Guard.Against.Negative(factor, nameof(factor));
            return new(Math.Round(Value * factor, 2, MidpointRounding.AwayFromZero));
        }
        
        public Money DivideBy(decimal divisor)
        {
            Guard.Against.NegativeOrZero(divisor, nameof(divisor));
            return new(Math.Round(Value / divisor, 2, MidpointRounding.AwayFromZero));
        }

        public Money ApplyDiscount(decimal percentage)
        {
            Guard.Against.NegativeOrZero(percentage, nameof(percentage));
            Guard.Against.OutOfRange(percentage, nameof(percentage), 0.01m, 100m);
            
            var discountFactor = 1 - (percentage / 100);
            return new(Math.Round(Value * discountFactor, 2, MidpointRounding.AwayFromZero));
        }
        
        public Money ApplyTax(decimal taxPercentage)
        {
            Guard.Against.Negative(taxPercentage, nameof(taxPercentage));
            
            var taxFactor = 1 + (taxPercentage / 100);
            return new(Math.Round(Value * taxFactor, 2, MidpointRounding.AwayFromZero));
        }

        public bool IsGreaterThan(Money other) => Value > other.Value;
        public bool IsLessThan(Money other) => Value < other.Value;
        public bool IsGreaterThanOrEqual(Money other) => Value >= other.Value;
        public bool IsLessThanOrEqual(Money other) => Value <= other.Value;
        public bool IsZero => Value == 0m;
        public bool IsPositive => Value > 0m;
        public bool IsNegative => Value < 0m;
                
        public static Money operator +(Money left, Money right) => left.Add(right);
        public static Money operator -(Money left, Money right) => left.Subtract(right);
        public static Money operator *(Money money, decimal factor) => money.MultiplyBy(factor);
        public static Money operator *(decimal factor, Money money) => money.MultiplyBy(factor);
        public static Money operator /(Money money, decimal divisor) => money.DivideBy(divisor);
        
        public static bool operator >(Money left, Money right) => left.IsGreaterThan(right);
        public static bool operator <(Money left, Money right) => left.IsLessThan(right);
        public static bool operator >=(Money left, Money right) => left.IsGreaterThanOrEqual(right);
        public static bool operator <=(Money left, Money right) => left.IsLessThanOrEqual(right);
    }
}
