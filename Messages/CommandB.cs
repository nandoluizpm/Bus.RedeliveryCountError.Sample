namespace Bus.RedeliveryCountError.Sample.Messages;

public class CommandB : CommandBase
{
    public bool ShouldThrowInCompensation { get; set; }
}