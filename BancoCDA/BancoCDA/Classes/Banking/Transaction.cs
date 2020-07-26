using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace BancoCDA.Classes.Banking
{
    public class Transaction
    {
        public string transactionText { get; set; }
        public int transactionAmount { get; set; }
        public DateTime transactionDate { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public TransactionType type { get; set; }
    }
}
