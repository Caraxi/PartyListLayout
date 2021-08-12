using System.Numerics;
using Newtonsoft.Json;

namespace PartyListLayout.Config {
    public class LayoutConfig {

        [JsonIgnore]
        public static readonly LayoutConfig Default = new();


        public bool ReverseFill;
        public int Columns = 1;
        public int SlotWidth = 260;
        public int SlotHeight = 80;

        // Elements
        public ElementConfig BarHP = new();
        public TextElementConfig NumberHP = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};
        public ElementConfig BarMP = new();
        public TextElementConfig NumberMP = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};
        public ElementConfig BarOvershield = new();
        public ElementConfig IconOvershield = new();
        public TextElementConfig Name = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};
        public ElementConfig Castbar = new();
        public TextElementConfig CastbarText = new() { Color = new Vector4(1), Glow = new Vector4(0x9D / 255f, 0x83 / 255f, 0x5B / 255f, 0xFF / 255f) };
        public ElementConfig ClassIcon = new();
        public ElementConfig Slot = new();
        public ElementConfig LeaderIcon = new();
        public TextElementConfig ChocoboTimer = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};
        public TextElementConfig ChocoboTimerClockIcon = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};
        public ElementConfig BarEnmity = new();
        public TextElementConfig TextEnmity = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};
        public StatusEffectsConfig StatusEffects = new();

        public int Version = 1;

    }
}
