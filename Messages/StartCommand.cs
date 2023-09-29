namespace Bus.RedeliveryCountError.Sample.Messages;

public class StartCommand
{
    public int ActivityAExecutionDelay { get; set; }
    public int ActivityBExecutionDelay { get; set; }
    public int ActivityCExecutionDelay { get; set; }
    public bool ActivityAThrowOnExecution { get; set; }
    public bool ActivityBThrowOnExecution { get; set; }
    public bool ActivityCThrowOnExecution { get; set; }
    public bool ActivityAThrowOnCompensation { get; set; }
    public bool ActivityBThrowOnCompensation { get; set; }
    public bool ShouldThrowIgnoredException { get; set; }
}