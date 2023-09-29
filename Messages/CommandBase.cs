namespace Bus.RedeliveryCountError.Sample.Messages;

public class CommandBase
{
    public int MillisecondsDelay { get; set; }
    public bool ShouldThrowException { get; set; }
    public bool ShouldThrowIgnoredException { get; set; }
}