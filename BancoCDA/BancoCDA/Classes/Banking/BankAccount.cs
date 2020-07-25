using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Starmans_Banking_System.Classes.Banking
{
    public class BankAccount
    {
        // Define all variables that make a bank account
        [JsonConverter(typeof(StringEnumConverter))]
        public AccountOwner owner { get; set; }
        public Bank bank { get; set; }
        public List<Transaction> transactions { get; set; }
        public int balance { get; set; }
        public bool isOpened { get; set; }
    }
}
