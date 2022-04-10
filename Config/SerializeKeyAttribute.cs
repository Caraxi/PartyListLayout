using System;

namespace PartyListLayout.Config {

    public enum SerializeKey : ushort {
        None = 0,
        Version = 1,
        // Layout General
        ReverseFill = 1000,
        Columns = 1010,
        SlotWidth = 1020,
        SlotHeight = 1030,

        // Party Grid ElementConfigs
        ClickableArea = 2000,
        PartyTypeText = 2010,
        Background = 2020,

        // Member Slot ElementConfigs
        Name = 3000,
        ClassIcon = 3010,
        BarHP = 3020,
        NumberHP = 3040,
        BarMP = 3050,
        NumberMP = 3060,
        BarOvershield = 3070,
        IconOvershield = 3080,
        ChocoboTimer = 3090,
        ChocoboTimerClockIcon = 3100,
        Castbar = 3110,
        CastbarText = 3120,
        Slot = 3130,
        LeaderIcon = 3140,
        BarEnmity = 3150,
        TextEnmity = 3160,
        StatusEffects = 3170,
        SelectionArea = 3180,

        // ElementConfig
        ElementHide = 10000,
        ElementPosition = 10010,
        ElementScale = 10020,
        ElementMultiplyColor = 10030,
        ElementAddColor = 10040,

        // TextElementConfig
        TextElementColor = 11000,
        TextElementGlow = 11010,

        // PlayerNameTextElementConfig
        PlayerTextRemoveLevel = 11500,
        PlayerTextRemoveFirst = 11510,
        PlayerTextRemoveSecond = 11520,
        KeepVisibleWhileCasting = 12041, // This should have been 11530 but some idiot forgot to change it, and now it is here forever.

        // StatusEffectsConfig
        StatusEffectsTwoLines = 12000,
        StatusEffectsVertical = 12010,
        StatusEffectsReverseFill = 12020,
        StatusEffectsSeparation = 12030,
        StatusEffectsMax = 12040,
        StatusEffectsReverseOrder = 12050,

        CastbarTextShowTarget = 13000,

        // BackgroundElementConfig
        BackgroundPadding = 14000,
        BackgroundColor = 14010,
        BackgroundOpacity = 14020,
    }

    public class SerializeKeyAttribute : Attribute {
        public SerializeKey Key { get; }


        /// <summary>
        /// Only serialize this field if the ElementConfig has the defined flag.
        /// Only functions on fields inside ElementConfig objects.
        /// </summary>
        public LayoutElementFlags RequireFlag { get; set; } = LayoutElementFlags.None;

        /// <summary>
        /// Only serialize this field if the ElementConfig does not have the defined flag.
        /// Only functions on fields inside ElementConfig objects.
        /// </summary>
        public LayoutElementFlags ExcludeFlag { get; set; } = LayoutElementFlags.None;

        public SerializeKeyAttribute(SerializeKey key) {
            this.Key = key;
        }
    }
}
