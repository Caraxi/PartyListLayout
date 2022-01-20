using System;

namespace PartyListLayout.Config {


    [AttributeUsage(AttributeTargets.Field)]
    public class LayoutElementAttribute : System.Attribute {
        public LayoutElementTab Tab { get; }
        public int Priority { get; }
        public string Name { get; }
        public LayoutElementFlags Flags { get; }

        public LayoutElementAttribute(LayoutElementTab tab, string name, int priority = 0, LayoutElementFlags flags = LayoutElementFlags.None) {
            Tab = tab;
            Priority = priority;
            Name = name;
            Flags = flags;
        }
    }

    public enum LayoutElementTab {
        PartyGrid,
        MemberSlot,
    }

    [Flags]
    public enum LayoutElementFlags : ulong {
        None = 0,
        CanTint = 1,
        NoHide = 2,
        NoPosition = 4,
        NoScale = 8,
        All = CanTint, // Show everything, hide nothing
    }
}
