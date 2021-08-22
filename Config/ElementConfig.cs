using System;
using System.Collections.Generic;
using System.Numerics;
using ImGuiNET;
using Newtonsoft.Json;
using PartyListLayout.Converter;

namespace PartyListLayout.Config {

    public class ElementConfig {
        [JsonIgnore] internal LayoutElementFlags EditorFlags = LayoutElementFlags.All;

        public bool ShouldSerializeHide() => !EditorFlags.HasFlag(LayoutElementFlags.NoHide);

        [SerializeKey(SerializeKey.ElementHide, ExcludeFlag = LayoutElementFlags.NoHide)]
        public bool Hide;

        [SerializeKey(SerializeKey.ElementPosition)]
        public Vector2 Position = new(0);

        [SerializeKey(SerializeKey.ElementScale)]
        public Vector2 Scale = new(1);

        public bool ShouldSerializeMultiplyColor() => EditorFlags.HasFlag(LayoutElementFlags.CanTint);

        [SerializeKey(SerializeKey.ElementMultiplyColor, RequireFlag = LayoutElementFlags.CanTint)]
        public Vector3 MultiplyColor = new(100/255f);
        public bool ShouldSerializeAddColor() => EditorFlags.HasFlag(LayoutElementFlags.CanTint);

        [SerializeKey(SerializeKey.ElementAddColor, RequireFlag = LayoutElementFlags.CanTint)]
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
