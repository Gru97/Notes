using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COR.Command
{
    public class CheckBalanceCommand : ICommand
    {
        private readonly decimal _amount;
        private readonly string _sourceAccount;
        public CheckBalanceCommand(decimal amount, string sourceAccount)
        {
            _amount = amount;
            _sourceAccount = sourceAccount;
        }
        public void Execute()
        {
            Console.WriteLine($"I check balance of {_sourceAccount}");
        }
    }
}
