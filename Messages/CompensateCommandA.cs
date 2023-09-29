namespace Bus.RedeliveryCountError.Sample.Messages;

public class CompensateCommandA
{
    public bool ShouldThrow { get; set; }
    public bool ShouldThrowIgnoredException { get; set; }
}