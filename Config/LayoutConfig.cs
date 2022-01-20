using System;
using System.Numerics;
using Newtonsoft.Json;
using PartyListLayout.Helper;

namespace PartyListLayout.Config {
    public class LayoutConfig {
        [JsonIgnore]
        public static readonly LayoutConfig Default = new();


        [SerializeKey(SerializeKey.ReverseFill)]
        public bool ReverseFill = true;

        [SerializeKey(SerializeKey.Columns)]
        public int Columns = 1;

        [SerializeKey(SerializeKey.SlotWidth)]
        public int SlotWidth = 388;

        [SerializeKey(SerializeKey.SlotHeight)]
        public int SlotHeight = 44;

        // Party Grid
        [SerializeKey(SerializeKey.PartyTypeText)]
        [LayoutElement(LayoutElementTab.PartyGrid, "Party Type Text", 1)]
        public TextElementConfig PartyTypeText = new() { Position = new Vector2(7, 20), Glow = Util.V4FromRgba(0xFF5B839D) };

        [SerializeKey(SerializeKey.Background)]
        [LayoutElement(LayoutElementTab.PartyGrid, "Background", 2, LayoutElementFlags.NoScale)]
        public BackgroundElementConfig Background = new();

        // Member Slot
        [SerializeKey(SerializeKey.Name)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Name", 1)]
        public PlayerNameTextElementConfig Name = new() { Color = new Vector4(1), Glow = Util.V4FromRgba(0xFF866131) };

        [SerializeKey(SerializeKey.ClassIcon)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Class Icon", 2)]
        public ElementConfig ClassIcon = new();

        [SerializeKey(SerializeKey.BarHP)]
        [LayoutElement(LayoutElementTab.MemberSlot, "HP Bar", 3, LayoutElementFlags.CanTint)]
        public ElementConfig BarHP = new();

        [SerializeKey(SerializeKey.NumberHP)]
        [LayoutElement(LayoutElementTab.MemberSlot, "HP Number", 4)]
        public TextElementConfig NumberHP = new() { Color = new Vector4(1), Glow = Util.V4FromRgba(0xFF866131)};

        [SerializeKey(SerializeKey.BarMP)]
        [LayoutElement(LayoutElementTab.MemberSlot, "MP Bar", 5, LayoutElementFlags.CanTint)]
        public ElementConfig BarMP = new();

        [SerializeKey(SerializeKey.NumberMP)]
        [LayoutElement(LayoutElementTab.MemberSlot, "MP Number", 6)]
        public TextElementConfig NumberMP = new() { Color = new Vector4(1), Glow = Util.V4FromRgba(0xFF866131)};

        [SerializeKey(SerializeKey.BarOvershield)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Overshield Bar", 7, LayoutElementFlags.CanTint)]
        public ElementConfig BarOvershield = new();

        [SerializeKey(SerializeKey.IconOvershield)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Overshield Icon", 8)]
        public ElementConfig IconOvershield = new();

        [SerializeKey(SerializeKey.ChocoboTimer)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Chocobo Timer", 9)]
        public TextElementConfig ChocoboTimer = new() { Color = new Vector4(1), Glow = Util.V4FromRgba(0xFF866131)};

        [SerializeKey(SerializeKey.ChocoboTimerClockIcon)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Chocobo Timer Clock Icon", 10)]
        public TextElementConfig ChocoboTimerClockIcon = new() { Color = new Vector4(1), Glow = Util.V4FromRgba(0xFF866131)};

        [SerializeKey(SerializeKey.Castbar)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Castbar", 11, LayoutElementFlags.CanTint)]
        public ElementConfig Castbar = new();

        [SerializeKey(SerializeKey.CastbarText)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Castbar Text", 12)]
        public CastbarTextElementConfig CastbarText = new() { Color = new Vector4(1), Glow = Util.V4FromRgba(0xFF5B839D)};

        [SerializeKey(SerializeKey.Slot)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Slot Number", 13)]
        public ElementConfig Slot = new();

        [SerializeKey(SerializeKey.LeaderIcon)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Leader Icon", 14)]
        public ElementConfig LeaderIcon = new();

        [SerializeKey(SerializeKey.BarEnmity)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Ennity Bar", 15, LayoutElementFlags.CanTint)]
        public ElementConfig BarEnmity = new();

        [SerializeKey(SerializeKey.TextEnmity)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Enmity Text", 16)]
        public TextElementConfig TextEnmity = new() { Color = new Vector4(1), Glow = Util.V4FromRgba(0xFF866131)};

        [SerializeKey(SerializeKey.StatusEffects)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Status Effects", 17)]
        public StatusEffectsConfig StatusEffects = new();

        [SerializeKey(SerializeKey.SelectionArea)]
        [LayoutElement(LayoutElementTab.MemberSlot, "Selection Area", 18, LayoutElementFlags.NoHide)]
        public ElementConfig SelectionArea = new();

        [SerializeKey(SerializeKey.Version)]
        public int Version = 1;

    }
}
