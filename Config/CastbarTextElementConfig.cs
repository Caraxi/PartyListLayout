using System.Numerics;
using ImGuiNET;

namespace PartyListLayout.Config {

    public class CastbarTextElementConfig : TextElementConfig {

        [SerializeKey(SerializeKey.CastbarTextShowTarget)]
        public bool ShowTarget = false;

        public override void Editor(string name, ref bool c, PartyListLayout l = null) {
            base.Editor(name, ref c, l);
            c |= ImGui.Checkbox("Show Target", ref ShowTarget);
        }
    }
}
