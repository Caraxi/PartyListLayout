using System;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace PartyListLayout; 

public unsafe class PartyListNodes{

    public PartyListNodes(AddonPartyList* partyList) {
        this.PartyBackground = partyList->AtkUnitBase.UldManager.NodeList[4]->GetAsAtkNineGridNode();
        Verify();
    }

    public void Verify() {
        if (PartyBackground == null) throw new Exception("Party Background Missing or Invalid");
    }
    
    public readonly AtkNineGridNode* PartyBackground;
}
