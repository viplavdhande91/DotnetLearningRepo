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
        public IAccount account { get; set; }

        public void PrintAccounts()
        {
            account.PrintDetails();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Account sa = new Account();
            sa.account = new SavingAccount();
            sa.PrintAccounts();

            Account ca = new Account();
            ca.account = new CurrentAccount();
            ca.PrintAccounts();

            Console.ReadLine();
        }
    }
}
