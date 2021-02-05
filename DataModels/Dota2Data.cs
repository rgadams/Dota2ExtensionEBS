using System.Collections.Generic;
using Newtonsoft.Json;

namespace Dota2ExtensionEBS
{
    public class Dota2Data
    {
        public int HeroId { get; set; }
        public List<string> Abilities { get; set; }
        public List<string> ActiveItems { get; set; }
        public List<string> BackpackItems { get; set; }
        public string NeutralItem { get; set; }
        public string TpSlot { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }

        static public Dota2Data FromString(string json)
        {
            return  JsonConvert.DeserializeObject<Dota2Data>(json);
        }
    }
}
