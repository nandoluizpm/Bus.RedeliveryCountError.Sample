using System;

namespace Bus.RedeliveryCountError.Sample.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}