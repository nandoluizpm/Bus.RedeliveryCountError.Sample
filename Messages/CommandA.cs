namespace Bus.RedeliveryCountError.Sample.Messages;

public class CommandA : CommandBase
{
    public bool ShouldThrowInCompensation { get; set; }
}