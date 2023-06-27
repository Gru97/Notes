namespace COR.Command;

public class BankTransferInvoker
{
    private readonly List<ICommand> _commands = new List<ICommand>();

    public BankTransferInvoker AddCommand(ICommand command)
    {
        _commands.Add(command);
        return this;

    }
    public void ExecuteCommands()
    {
        foreach (var command in _commands)
        {
            command.Execute();
        }
    }
}