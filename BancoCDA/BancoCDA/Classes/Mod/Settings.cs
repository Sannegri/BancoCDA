using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Windows.Forms;

namespace BancoCDA.Classes.Mod
{
    public class Settings
    {
        
        public bool showDebugMessages { get; set; }
        public bool addBankRequiresShift { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Keys addBankKey { get; set; }
    }
}
