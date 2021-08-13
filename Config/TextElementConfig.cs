using System.Numerics;
using ImGuiNET;

namespace PartyListLayout.Config {

    public class TextElementConfig : ElementConfig {
        public Vector4 Color = new (1);
        public Vector4 Glow = new(1);

        public override void Editor(string name, ref bool c, LayoutElementFlags flags, PartyListLayout l = null) {
            base.Editor(name, ref c, flags, l);
            c |= ImGui.ColorEdit4($"Color##{name}", ref Color);
            c |= ImGui.ColorEdit4($"Glow##{name}", ref Glow);
        }
    }
    
}
