using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DependencyInjectionCsharp
{
    public interface IAccount
    {
        void PrintDetails();
    }

    class CurrentAccount : IAccount
    {
        public void PrintDetails()
        {
            Console.WriteLine("Details Of Current Account..");
        }
    }
    class SavingAccount : IAccount
    {
        public void PrintDetails()
        {
            Console.WriteLine("Details Of Saving Account..");
        }
    }

    class Account
    {
        public void PrintAccounts(IAccount account)
        {
            account.PrintDetails();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Account sa = new Account();
            sa.PrintAccounts(new SavingAccount());

            Account ca = new Account();
            ca.PrintAccounts(new CurrentAccount());

            Console.ReadLine();
        }
    }
}
