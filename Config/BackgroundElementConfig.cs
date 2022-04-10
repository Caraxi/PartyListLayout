using System.Numerics;
using ImGuiNET;

namespace PartyListLayout.Config {
    public class BackgroundElementConfig : ElementConfig {
        [SerializeKey(SerializeKey.BackgroundPadding)]
        public Vector2 Padding = new(0);

        [SerializeKey(SerializeKey.BackgroundColor)]
        public Vector3 BackgroundColor = new(0);
        
        [SerializeKey(SerializeKey.BackgroundOpacity)]
        public float BackgroundOpacity = 1;

        public override void Editor(string name, ref bool c, PartyListLayout l = null) {
            c |= ImGui.DragFloat2("Padding", ref Padding);
            base.Editor(name, ref c, l);
            c |= ImGui.ColorEdit3("Color##bgColor", ref BackgroundColor);
            c |= ImGui.SliderFloat("Opacity##bgOpacity", ref BackgroundOpacity, 0, 1, $"{BackgroundOpacity*100:F0}%%");
            while (BackgroundOpacity > 1) BackgroundOpacity *= 0.01f;
            if (BackgroundOpacity < 0) BackgroundOpacity = 0;
        }

    }
}
