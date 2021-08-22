using System.Numerics;
using ImGuiNET;

namespace PartyListLayout.Config {
    public class StatusEffectsConfig : ElementConfig {

        [SerializeKey(SerializeKey.StatusEffectsTwoLines)]
        public bool TwoLines;

        [SerializeKey(SerializeKey.StatusEffectsVertical)]
        public bool Vertical;

        [SerializeKey(SerializeKey.StatusEffectsReverseFill)]
        public bool ReverseFill;

        [SerializeKey(SerializeKey.StatusEffectsSeparation)]
        public Vector2 Separation = new(0);

        [SerializeKey(SerializeKey.StatusEffectsMax)]
        public int MaxDisplayed = 10;
        
        public override void Editor(string name, ref bool c, PartyListLayout l = null) {
            base.Editor(name, ref c, l);
            ImGui.SetNextItemWidth(120 * ImGui.GetIO().FontGlobalScale);
            c |= ImGui.InputInt($"Max Statuses##{name}", ref MaxDisplayed);
            if (MaxDisplayed < 0) MaxDisplayed = 0;
            if (MaxDisplayed > 10) MaxDisplayed = 10;

            c |= ImGui.Checkbox($"Vertical##{name}", ref Vertical);
            ImGui.SameLine();
            c |= ImGui.Checkbox($"Two Lines##{name}", ref TwoLines);
            if (TwoLines) {
                ImGui.SameLine();
                c |= ImGui.Checkbox($"{(Vertical?"Fill Rows First": "Fill Columns First")}##{name}", ref ReverseFill);
            }

            c |= ImGui.SliderFloat2("Icon Separation", ref Separation, -100, 100);
            
            if (l != null) {
                ImGui.Indent();
                ImGui.Indent();
                var dl = ImGui.GetWindowDrawList();
                var p = ImGui.GetCursorScreenPos();
                
                var mX = 0;
                var mY = 0;
            
                for (var i = 0; i < 10; i++) {
                    var (xO, yO) = l.StatusSlotPositions[i];
                
                    dl.AddRect(p + new Vector2(30 * xO, 40 * yO), p + new Vector2(30 * xO + 24, 40 * yO + 32), 0xFFFFFFFF);
                    dl.AddText(p + new Vector2(30 * xO + 3, 40 * yO), 0xFFFFFFFF, $"{i + 1}");
                
                    if (xO > mX) mX = xO;
                    if (yO > mY) mY = yO;
                }

                ImGui.Dummy(new Vector2(30 * (mX + 1), 40 * (mY + 1)));
                ImGui.Unindent();
                ImGui.Unindent();
            }
            
        }
    }
    
}
