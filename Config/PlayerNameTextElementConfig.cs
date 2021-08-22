using ImGuiNET;

namespace PartyListLayout.Config {
    public class PlayerNameTextElementConfig : TextElementConfig {

        [SerializeKey(SerializeKey.PlayerTextRemoveLevel)]
        public bool RemoveLevel = false;

        [SerializeKey(SerializeKey.PlayerTextRemoveFirst)]
        public bool RemoveFirst = false;

        [SerializeKey(SerializeKey.PlayerTextRemoveSecond)]
        public bool RemoveSecond = false;

        [SerializeKey(SerializeKey.KeepVisibleWhileCasting)]
        public bool KeepVisibleWhileCasting = false;

        public override void Editor(string name, ref bool c, PartyListLayout l = null) {
            base.Editor(name, ref c, l);
            c |= ImGui.Checkbox("Hide Player Level", ref RemoveLevel);
            c |= ImGui.Checkbox("Hide First Name", ref RemoveFirst);
            c |= ImGui.Checkbox("Hide Second Name", ref RemoveSecond);
            c |= ImGui.Checkbox("Keep Visible While Casting", ref KeepVisibleWhileCasting);
        }
    }
}
