namespace COR.Command;

public class DepositCommand : ICommand
{
    private readonly decimal _amount;
    private readonly string _destinationAccount;

    public DepositCommand(decimal amount, string destinationAccount)
    {
        _amount = amount;
        _destinationAccount = destinationAccount;
    }

    public void Execute()
    {
        Console.WriteLine($"I deposit {_amount}$ from {_destinationAccount}");
    }
}