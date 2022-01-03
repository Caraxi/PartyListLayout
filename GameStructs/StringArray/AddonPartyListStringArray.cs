using System;
using System.Runtime.InteropServices;
using PartyListLayout.Helper;

namespace PartyListLayout.GameStructs.StringArray {

    [StructLayout(LayoutKind.Sequential, Size = 0x718)]
    public unsafe struct AddonPartyListStringArray {

        public byte* String000;
        public byte* String001;
        public byte* String002;
        public byte* String003;
        public byte* String004;
        public byte* String005;

        public AddonPartyListMembersStringArray PartyMembers;

    }

    [StructLayout(LayoutKind.Sequential, Size = 0x340)]
    public unsafe struct AddonPartyListMembersStringArray {

        public AddonPartyListPartyMemberStrings Member0;
        public AddonPartyListPartyMemberStrings Member1;
        public AddonPartyListPartyMemberStrings Member2;
        public AddonPartyListPartyMemberStrings Member3;
        public AddonPartyListPartyMemberStrings Member4;
        public AddonPartyListPartyMemberStrings Member5;
        public AddonPartyListPartyMemberStrings Member6;
        public AddonPartyListPartyMemberStrings Member7;


        public AddonPartyListPartyMemberStrings this[int i] {
            get {
                return i switch {
                    0 => Member0,
                    1 => Member1,
                    2 => Member2,
                    3 => Member3,
                    4 => Member4,
                    5 => Member5,
                    6 => Member6,
                    7 => Member7,
                    _ => throw new IndexOutOfRangeException("Index should be between 0 and 7")
                };
            }
            set {
                switch (i) {
                    case 0: Member0 = value; break;
                    case 1: Member1 = value; break;
                    case 2: Member2 = value; break;
                    case 3: Member3 = value; break;
                    case 4: Member4 = value; break;
                    case 5: Member5 = value; break;
                    case 6: Member6 = value; break;
                    case 7: Member7 = value; break;
                    default: throw new IndexOutOfRangeException("Index should be between 0 and 7");
                }
            }
        }
    }



    [StructLayout(LayoutKind.Sequential, Size = 0x68)]
    public unsafe struct AddonPartyListPartyMemberStrings {
        public byte* PartyPositionLabel;
        private byte* playerNameAndLevel;
        public byte* String02;
        public byte* String03;
        public byte* String04;
        public byte* String05;
        public byte* String06;
        public byte* String07;
        public byte* String08;
        public byte* String09;
        public byte* String10;
        public byte* String11;
        public byte* String12;


        public string PlayerNameAndLevel => Util.ReadString(playerNameAndLevel);
    }
}
