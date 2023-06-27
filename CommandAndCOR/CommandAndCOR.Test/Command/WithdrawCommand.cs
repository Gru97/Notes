namespace COR.Command;

public class WithdrawCommand : ICommand
{
    private readonly decimal _amount;
    private readonly string _sourceAccount;

    public WithdrawCommand(decimal amount, string sourceAccount)
    {
        _amount = amount;
        _sourceAccount = sourceAccount;
        _sourceAccount = sourceAccount;
    }

    string sourceAccount;
    public void Execute()
    {
        Console.WriteLine($"I withdraw {_amount}$ from {sourceAccount}");
    }
}