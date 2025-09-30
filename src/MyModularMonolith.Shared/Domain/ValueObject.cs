using System;
using System.Collections.Generic;
using System.Text;

namespace MyModularMonolith.Shared.Domain;

public abstract record ValueObject;

public abstract record ValueObject<T> : ValueObject
{
    public T Value { get; init; }

    protected ValueObject(T value)
    {
        Value = value;
    }

    public static implicit operator T(ValueObject<T> valueObject) => valueObject.Value;

    public override string ToString() => Value?.ToString() ?? string.Empty;
}
