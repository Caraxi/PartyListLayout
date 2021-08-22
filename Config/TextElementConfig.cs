using System.Numerics;
using ImGuiNET;

namespace PartyListLayout.Config {

    public class TextElementConfig : ElementConfig {

        [SerializeKey(SerializeKey.TextElementColor)]
        public Vector4 Color = new (1);

        [SerializeKey(SerializeKey.TextElementGlow)]
        public Vector4 Glow = new(1);

        public override void Editor(string name, ref bool c, PartyListLayout l = null) {
            base.Editor(name, ref c, l);
            c |= ImGui.ColorEdit4($"Color##{name}", ref Color);
            c |= ImGui.ColorEdit4($"Glow##{name}", ref Glow);
        }
    }
}
