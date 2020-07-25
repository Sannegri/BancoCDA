using System.Collections.Generic;
using Starmans_Banking_System.Classes.Banking;

namespace Starmans_Banking_System.Classes.Mod
{
    public class Configuration
    {
        // Define all variables to make up a Configuration
        public Settings settings { get; set; }
        public List<Bank> banks { get; set; }
    }
}
