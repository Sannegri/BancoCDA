using GTA.Math;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace BancoCDA.Classes.Banking
{
    public class Bank
    {
        // Define all variables that make a Bank
        public float XPos { get; set; }
        public float YPos { get; set; }
        public float ZPos { get; set; }
        public int startingMoney { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public BankType typeOfBank { get; set; }
    }
}
