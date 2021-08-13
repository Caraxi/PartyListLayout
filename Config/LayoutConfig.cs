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

        // Member Slot
        [LayoutElement(LayoutElementTab.MemberSlot, "Name", 1)]
        public TextElementConfig Name = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};

        [LayoutElement(LayoutElementTab.MemberSlot, "Class Icon", 2)]
        public ElementConfig ClassIcon = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "HP Bar", 3)]
        public ElementConfig BarHP = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "HP Number", 4)]
        public TextElementConfig NumberHP = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};

        [LayoutElement(LayoutElementTab.MemberSlot, "MP Bar", 5)]
        public ElementConfig BarMP = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "MP Number", 6)]
        public TextElementConfig NumberMP = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};

        [LayoutElement(LayoutElementTab.MemberSlot, "Overshield Bar", 7)]
        public ElementConfig BarOvershield = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "Overshield Icon", 8)]
        public ElementConfig IconOvershield = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "Chocobo Timer", 9)]
        public TextElementConfig ChocoboTimer = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};

        [LayoutElement(LayoutElementTab.MemberSlot, "Chocobo Timer Clock Icon", 10)]
        public TextElementConfig ChocoboTimerClockIcon = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};

        [LayoutElement(LayoutElementTab.MemberSlot, "Castbar", 11)]
        public ElementConfig Castbar = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "Castbar Text", 12)]
        public TextElementConfig CastbarText = new() { Color = new Vector4(1), Glow = new Vector4(0x9D / 255f, 0x83 / 255f, 0x5B / 255f, 0xFF / 255f) };

        [LayoutElement(LayoutElementTab.MemberSlot, "Slot Number", 13)]
        public ElementConfig Slot = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "Leader Icon", 14)]
        public ElementConfig LeaderIcon = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "Ennity Bar", 15)]
        public ElementConfig BarEnmity = new();

        [LayoutElement(LayoutElementTab.MemberSlot, "Enmity Text", 16)]
        public TextElementConfig TextEnmity = new() { Color = new Vector4(1), Glow = new Vector4(0x31/255f, 0x61/255f, 0x86/255f, 0xFF/255f)};

        [LayoutElement(LayoutElementTab.MemberSlot, "Status Effects", 17)]
        public StatusEffectsConfig StatusEffects = new();

        public int Version = 1;

    }
}
