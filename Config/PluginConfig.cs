using Dalamud.Configuration;
using Newtonsoft.Json;

namespace PartyListLayout.Config {
    public class PluginConfig : IPluginConfiguration {


        [JsonIgnore] public bool PreviewMode;
        [JsonIgnore] public int PreviewCount = 8;

        public LayoutConfig CurrentLayout = new();
        public bool AutoSave = true;
        public int MaxAutoSave = 20;

        public bool HideKofi = false;

        public int Version { get; set; } = 1;
    }
}
