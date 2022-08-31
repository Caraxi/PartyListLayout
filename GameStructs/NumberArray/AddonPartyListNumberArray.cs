using System;
using System.Runtime.InteropServices;

namespace PartyListLayout.GameStructs.NumberArray {
    [StructLayout(LayoutKind.Sequential, Size = 727 * 4)]
    public unsafe struct AddonPartyListIntArray {
        /* 000 */ public int Unknown000;
        /* 001 */ public int Unknown001;
        
        /* 002 */ public int PartyLeaderIndex;
        
        /* 003 */ public int Unknown003;
        /* 004 */ public int Unknown004;
        /* 005 */ public int PartyMemberCount;
        /* 006 */ public int Unknown006;
        /* 007 */ public int Unknown007;
        /* 008 */ public int Unknown008;

        /* 009 */ public AddonPartyListMembersIntArray PartyMember;
    }

    [StructLayout(LayoutKind.Sequential, Size = 42 * 4 * 8)]
    public unsafe struct AddonPartyListMembersIntArray {
        public AddonPartyListMemberIntArray Member0;
        public AddonPartyListMemberIntArray Member1;
        public AddonPartyListMemberIntArray Member2;
        public AddonPartyListMemberIntArray Member3;
        public AddonPartyListMemberIntArray Member4;
        public AddonPartyListMemberIntArray Member5;
        public AddonPartyListMemberIntArray Member6;
        public AddonPartyListMemberIntArray Member7;

        public AddonPartyListMemberIntArray this[int i] {
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
    
    [StructLayout(LayoutKind.Sequential, Size = 42 * 4)]
    public unsafe struct AddonPartyListMemberIntArray {
        /* 00 */ public int Level;
        /* 01 */ public int ClassJobIcon;
        /* 02 */ public int Unknown2;
        /* 03 */ public int Unknown3;
        /* 04 */ public int HP;
        /* 05 */ public int HPMax;
        /* 06 */ public int ShieldPercentage;
        /* 07 */ public int MP;
        /* 08 */ public int MPMax;
        /* 09 */ public int Unknown9;
        /* 10 */ public int Unknown10;
        /* 11 */ public int PartySlot;
        /* 12 */ public int Unknown12;
        /* 13 */ public int Unknown13;
        /* 14 */ public int StatusEffectCount;
        /* 15-24 */ public AddonPartyListMemberStatusEffectsIntArray StatusEffect;
        /* 25 */ public int Unknown25;
        /* 26 */ public int Unknown26;
        /* 27 */ public int Unknown27;
        /* 28 */ public int Unknown28;
        /* 29 */ public int Unknown29;
        /* 30 */ public int Unknown30;
        /* 31 */ public int Unknown31;
        /* 32 */ public int Unknown32;
        /* 33 */ public int Unknown33;
        /* 34 */ public int Unknown34;
        /* 35 */ public int CastingPercent;
        /* 36 */ public int CastingActionId;
        /* 37 */ public int CastingTargetIndex;
        /* 38 */ public int ObjectId;
        /* 39 */ public int Unknown39;
        /* 40 */ public int Unknown40;
        /* 41 */ public int Unknown41;
    }

    [StructLayout(LayoutKind.Sequential, Size = 4 * 10)]
    public struct AddonPartyListMemberStatusEffectsIntArray {
        public int Status0;
        public int Status1;
        public int Status2;
        public int Status3;
        public int Status4;
        public int Status5;
        public int Status6;
        public int Status7;
        public int Status8;
        public int Status9;

        public int this[int i] {
            get {
                return i switch {
                    0 => Status0,
                    1 => Status1,
                    2 => Status2,
                    3 => Status3,
                    4 => Status4,
                    5 => Status5,
                    6 => Status6,
                    7 => Status7,
                    8 => Status8,
                    9 => Status9,
                    _ => throw new IndexOutOfRangeException("Index should be between 0 and 9")
                };
            }
            set {
                switch (i) {
                    case 0: Status0 = value; break;
                    case 1: Status1 = value; break;
                    case 2: Status2 = value; break;
                    case 3: Status3 = value; break;
                    case 4: Status4 = value; break;
                    case 5: Status5 = value; break;
                    case 6: Status6 = value; break;
                    case 7: Status7 = value; break;
                    case 8: Status8 = value; break;
                    case 9: Status9 = value; break;
                    default: throw new IndexOutOfRangeException("Index should be between 0 and 9");
                }
            }
        }



    }

}
