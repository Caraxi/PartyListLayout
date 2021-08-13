using System;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;

namespace PartyListLayout.Config {

    public class ElementConfig {
        [JsonIgnore] internal LayoutElementFlags EditorFlags = LayoutElementFlags.All;

        public bool Hide;
        public Vector2 Position = new(0);
        public Vector2 Scale = new(1);

        public bool ShouldSerializeMultiplyColor() => EditorFlags.HasFlag(LayoutElementFlags.CanTint);
        public Vector3 MultiplyColor = new(100/255f);
        public bool ShouldSerializeAddColor() => EditorFlags.HasFlag(LayoutElementFlags.CanTint);
        public Vector3 AddColor = new(0);

        public virtual void Editor(string name, ref bool c, PartyListLayout l = null) {
            c |= ImGui.DragFloat2($"Position##{name}", ref Position);
            c |= ImGui.SliderFloat2($"Scale##{name}", ref Scale, 0, 5);

            if (EditorFlags.HasFlag(LayoutElementFlags.CanTint)) {
                c |= ImGui.ColorEdit3($"Tint (Multiply)##{name}", ref MultiplyColor, ImGuiColorEditFlags.Uint8);
                c |= ImGui.ColorEdit3($"Tint (Add)##{name}", ref AddColor, ImGuiColorEditFlags.Uint8);

            }

        }
    }
    
}
