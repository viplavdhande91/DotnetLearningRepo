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
        private IAccount account;

        public Account(IAccount account) // Parameterized Constructor
        {
            this.account = account;
        }

        public void PrintAccounts()
        {
            account.PrintDetails();
        }

    }

    class Program
    {
        static void Main(string[] args)
        {
            IAccount ca = new CurrentAccount();
            Account a = new Account(ca);
            a.PrintAccounts();

            IAccount sa = new SavingAccount();
            Account a2 = new Account(sa);
            a2.PrintAccounts();

            Console.ReadLine();
        }
    }
}
