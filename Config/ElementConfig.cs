using System.Numerics;
using ImGuiNET;

namespace PartyListLayout.Config {

    public class ElementConfig {
        public bool Hide;
        public Vector2 Position = new(0);
        public Vector2 Scale = new(1);
        
        public virtual void Editor(string name, ref bool c, LayoutElementFlags flags, PartyListLayout l = null) {
            c |= ImGui.DragFloat2($"Position##{name}", ref Position);
            c |= ImGui.SliderFloat2($"Scale##{name}", ref Scale, 0, 5);
        }
    }
    
}
