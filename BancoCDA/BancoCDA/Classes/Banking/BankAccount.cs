using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace BancoCDA.Classes.Banking
{
    public class BankAccount
    {
        // Define todas as variaveis que cria a conta do banco
        [JsonConverter(typeof(StringEnumConverter))]
        public AccountOwner owner { get; set; }
        public Bank bank { get; set; }
        public List<Transaction> transactions { get; set; }
        public int balance { get; set; }
        public bool isOpened { get; set; }
    }
}
