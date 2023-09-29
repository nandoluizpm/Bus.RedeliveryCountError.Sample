namespace Bus.RedeliveryCountError.Sample.Messages;

public class CompensateCommandB
{
    public bool ShouldThrow { get; set; }
    public bool ShouldThrowIgnoredException { get; set; }
}