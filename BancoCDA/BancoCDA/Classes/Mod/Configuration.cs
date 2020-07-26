using System.Collections.Generic;
using BancoCDA.Classes.Banking;

namespace BancoCDA.Classes.Mod
{
    public class Configuration
    {
        
        public Settings settings { get; set; }
        public List<Bank> banks { get; set; }
    }
}
