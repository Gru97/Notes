using COR.Command;

namespace CommandAndCOR.Test
{
    public class Test
    {
        [Fact]
        public void CORTest()
        {

            var request = new Request(10, "edible", "125");
            var handler = ParcelDeliveryBuilder.Create(request);
            handler.Handle(request);
        }
        [Fact]
        public void CommandTest()
        {

            decimal amount = 10;
            string sourceAccount = "133565";
            string destinationAccount = "963333";
            var transfer = new BankTransferInvoker();
            transfer
                .AddCommand(new CheckBalanceCommand(amount, sourceAccount))
                .AddCommand(new WithdrawCommand(amount, sourceAccount))
                .AddCommand(new DepositCommand(amount, destinationAccount))
                .ExecuteCommands();
        }


    }
}