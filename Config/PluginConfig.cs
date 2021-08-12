using Dalamud.Configuration;
using Newtonsoft.Json;

namespace PartyListLayout.Config {
    public class PluginConfig : IPluginConfiguration {


        [JsonIgnore] public bool PreviewMode;

        public LayoutConfig CurrentLayout = new();
        public bool AutoSave = true;
        public int MaxAutoSave = 20;

        public bool HideKofi = false;

        public int Version { get; set; } = 1;
    }
}
