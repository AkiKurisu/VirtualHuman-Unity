using UnityEngine;
namespace Kurisu.VirtualHuman
{
    [CreateAssetMenu(fileName = "CharaPresetConfig", menuName = "Virtual Human/CharaPresetConfig")]
    public class CharaPresetConfig : ScriptableObject
    {
        public string user_Name = "You";
        public string char_name = "Bot";
        [TextArea]
        public string char_persona;
        [TextArea]
        public string world_scenario;
        [TextArea(5, 10)]
        public string example_dialogue;
    }
}
