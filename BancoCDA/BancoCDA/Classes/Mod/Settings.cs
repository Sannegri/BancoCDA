using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Forms;

namespace Starmans_Banking_System.Classes.Mod
{
    public class Settings
    {
        // Define all variables that make the Mod Settings
        public bool showDebugMessages { get; set; }
        public bool addBankRequiresShift { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys addBankKey { get; set; }
    }
}
