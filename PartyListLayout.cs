using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PartyListLayout.Config;
using PartyListLayout.GameStructs.NumberArray;
using PartyListLayout.Helper;

namespace PartyListLayout {
    public unsafe class PartyListLayout : IDisposable {

        private readonly Plugin plugin;

        public PartyListLayout(Plugin plugin) {
            this.plugin = plugin;
        }

        public bool Enabled { get; private set; }

        public readonly LayoutConfig DefaultLayout = new();
        public LayoutConfig CurrentLayout => plugin.Config.CurrentLayout;


        private (byte x, byte y)[] statusSlotPositions;

        public (byte x, byte y)[] StatusSlotPositions {
            get{
                if (statusSlotPositions == null) {
                    statusSlotPositions = new (byte x, byte y)[10];
                    byte xO = 0;
                    byte yO = 0;
                    
                    for (var i = 0; i < 10; i++) {
                        statusSlotPositions[i] = (xO, yO);
                        if (CurrentLayout.StatusEffects.ReverseFill) {
                            if (CurrentLayout.StatusEffects.Vertical) {
                                xO++;
                                if (CurrentLayout.StatusEffects.TwoLines && xO % 2 == 0) {
                                    xO = 0;
                                    yO++;
                                }
                            } else {
                                yO++;
                                if (CurrentLayout.StatusEffects.TwoLines && yO % 2 == 0) {
                                    yO = 0;
                                    xO++;
                                }
                            }
                        } else {
                            if (CurrentLayout.StatusEffects.Vertical) {
                                yO++;
                                if (CurrentLayout.StatusEffects.TwoLines && yO == 5) {
                                    yO = 0;
                                    xO++;
                                }
                            } else {
                                xO++;
                                if (CurrentLayout.StatusEffects.TwoLines && xO == 5) {
                                    xO = 0;
                                    yO++;
                                }
                            }
                        }
                    }
                }

                return statusSlotPositions;
            }
        }

        private delegate void* PartyListOnUpdate(AddonPartyList* @this, void* numArrayData, void* stringArrayData);
        private HookWrapper<PartyListOnUpdate> partyListOnUpdateHook;
        
        
        public void Enable() {
            if (Enabled) return;
            statusSlotPositions = null;
            partyListOnUpdateHook ??= Common.Hook<PartyListOnUpdate>("48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 48 8B 7A 20", PartyListUpdateDetour, false);
            partyListOnUpdateHook?.Enable();
            try {
                
                Update(Common.GetUnitBase<AddonPartyList>());
                
            } catch (Exception ex) {
                SimpleLog.Error(ex);
            }

            plugin.PluginInterface.Framework.OnUpdateEvent += FrameworkUpdate;
            Enabled = true;
        }
        
        private void* PartyListUpdateDetour(AddonPartyList* @this, void* a2, void* a3) {
            var ret = partyListOnUpdateHook.Original(@this, a2, a3);
            try {
                Update(@this);
            } catch (Exception ex) {
                SimpleLog.Error(ex);
            }
            return ret;
        }

        public void Disable() {
            if (!Enabled) return;
            plugin.PluginInterface.Framework.OnUpdateEvent -= FrameworkUpdate;
            if (plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen) {
                plugin.Config.PreviewMode = false;
                FrameworkUpdate(plugin.PluginInterface.Framework);
            }
            partyListOnUpdateHook?.Disable();
            try {
                Update(Common.GetUnitBase<AddonPartyList>(), true);
            } catch (Exception ex) {
                SimpleLog.Error(ex);
            }

            FrameworkUpdate(plugin.PluginInterface.Framework);

            Enabled = false;
        }

        private bool isPreviewing;

        private void FrameworkUpdate(Dalamud.Game.Internal.Framework framework) {
            if (!(plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen)) {
                if (isPreviewing) {
                    isPreviewing = false;
                } else {
                    return;
                }
            }
            isPreviewing = true;
            var atkArrayDataHolder = Framework.Instance()->GetUiModule()->RaptureAtkModule.AtkModule.AtkArrayDataHolder;

            Common.GetUnitBase<AddonPartyList>()->AtkUnitBase.OnUpdate(atkArrayDataHolder.NumberArrays, atkArrayDataHolder.StringArrays);
        }

        private void SetupPreview(AddonPartyList* partyList, AddonPartyListIntArray* intArray) {
            var ia = (AddonPartyListMemberIntArray*) &intArray->PartyMember;
            for (var i = intArray->PartyMemberCount; i < 8; i++) {
                var m = partyList->PartyMember[i];
                m.PartyMemberComponent->OwnerNode->AtkResNode.ToggleVisibility(true);

                m.Name->SetText($" Party Member'{i+1}");
                m.Name->AtkResNode.ToggleVisibility(i % 2 == 0);

                m.ClassJobIcon->AtkResNode.ToggleVisibility(true);
                m.ClassJobIcon->LoadIconTexture(62101 + i, 0);

                m.CastingActionName->SetText($"Spell Name {i+1}");
                m.CastingActionName->AtkResNode.ToggleVisibility(i % 2 != 0);
                m.CastingProgressBar->AtkResNode.ToggleVisibility(i % 2 != 0);
                m.CastingProgressBarBackground->AtkResNode.ToggleVisibility(i % 2 != 0);

                for (var j = 0; j < 10; j++) {
                    var nodeList = m.StatusIcon[j]->AtkComponentBase.UldManager.NodeList;
                    nodeList[0]->ToggleVisibility(false);
                    nodeList[1]->ToggleVisibility(true);
                    if (intArray->PartyMember[i].StatusEffect[j] == 0) nodeList[1]->GetAsAtkImageNode()->LoadIconTexture(10205, 0);
                }

                for (var j = 0; j < m.PartyMemberComponent->UldManager.NodeListSize; j++) {
                    var n = m.PartyMemberComponent->UldManager.NodeList[j];
                    if (n == null) continue;
                    switch (n->NodeID) {
                        case 2: {
                            var tn = (AtkTextNode*) n;
                            tn->SetNumber(i + 1);
                            n->ToggleVisibility(true);
                            break;
                        }
                        case 8: {
                            n->ToggleVisibility(true);
                            n->SetScale((Environment.TickCount / (7 * i) % 100) / 100f, 1f);
                            break;
                        }
                        case 9: {
                            n->ToggleVisibility(true);
                            break;
                        }
                    }
                }



                ia[i].CastingPercent = Environment.TickCount / (8 + i) % 100;
            }
        }


        private void Update(AddonPartyList* partyList, bool reset = false) {

            if (partyList == null) return;
            if (partyList->AtkUnitBase.UldManager.NodeListSize < 17) return;

            var atkArrayDataHolder = Framework.Instance()->GetUiModule()->RaptureAtkModule.AtkModule.AtkArrayDataHolder;
            var partyListNumbers = atkArrayDataHolder.NumberArrays[4];
            var partyIntList = (AddonPartyListIntArray*) partyListNumbers->IntArray;

            if (plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen && !reset) {
                SetupPreview(partyList, partyIntList);
            }

            var visibleIndex = 0;

            var maxX = 0;
            var maxY = 0;

            for (var i = 0; i < 13; i++) {
                try {
                    var pm = i switch {
                        >= 0 and <= 7 => partyList->PartyMember[i],
                        8 => partyList->Unknown08,
                        9 => partyList->Unknown09,
                        10 => partyList->Unknown10,
                        11 => partyList->Chocobo,
                        12 => partyList->Pet,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var intList = i switch {
                        >= 0 and <= 7 => partyIntList->PartyMember[i],
                        _ => default
                    };
                    
                    var c = pm.PartyMemberComponent;
                    if (c == null) continue;
                    var cNode = c->OwnerNode;
                    if (cNode == null) continue;
                    
                    if (cNode->AtkResNode.IsVisible || reset) UpdateSlot(cNode, visibleIndex, pm, intList, ref maxX, ref maxY, reset);
                    if (cNode->AtkResNode.IsVisible) visibleIndex++;

                    if (i == 11) {
                        partyList->MpBarSpecialResNode->SetPositionFloat(153 + cNode->AtkResNode.X, 60 + cNode->AtkResNode.Y);
                        var cTextNode = partyList->MpBarSpecialResNode->ChildNode;
                        HandleElementConfig(cTextNode, CurrentLayout.ChocoboTimer, reset, defColor: DefaultLayout.ChocoboTimer.Color, defGlow: DefaultLayout.ChocoboTimer.Glow);
                        if (cTextNode != null) HandleElementConfig(cTextNode->PrevSiblingNode, CurrentLayout.ChocoboTimerClockIcon, reset, defColor: DefaultLayout.ChocoboTimerClockIcon.Color, defGlow: DefaultLayout.ChocoboTimerClockIcon.Glow, defPosX: 18);
                    }

                    if (partyListNumbers->IntArray[2] == i && cNode->AtkResNode.IsVisible) {
                        if (reset) {
                            partyList->LeaderMarkResNode->SetPositionShort(0, 30);
                            HandleElementConfig(partyList->LeaderMarkResNode->ChildNode, CurrentLayout.LeaderIcon, reset, defPosX: 0, defPosY: 40);
                        } else {
                            partyList->LeaderMarkResNode->SetPositionShort(0, 0);
                            HandleElementConfig(partyList->LeaderMarkResNode->ChildNode, CurrentLayout.LeaderIcon, reset, defPosX: cNode->AtkResNode.X, defPosY: cNode->AtkResNode.Y + 30);
                        }
                    }
                } catch {
                    // 
                }
            }
            
            // Collision Node Update
            partyList->AtkUnitBase.UldManager.NodeList[1]->SetWidth(reset ? (ushort)500 : (ushort) maxX);
            partyList->AtkUnitBase.UldManager.NodeList[1]->SetHeight(reset ? (ushort)480 : (ushort) maxY);
            
            // Background Update
            partyList->AtkUnitBase.UldManager.NodeList[3]->ToggleVisibility(reset);
        }

        private ByteColor GetColor(Vector4 vector4) {
            return new ByteColor() {
                R = (byte) (vector4.X * 255),
                G = (byte) (vector4.Y * 255),
                B = (byte) (vector4.Z * 255),
                A = (byte) (vector4.W * 255)
            };
        }
        
        private void HandleElementConfig(AtkResNode* resNode, ElementConfig eCfg, bool reset, float defScaleX = 1f, float defScaleY = 1f, float defPosX = 0, float defPosY = 0, Vector4 defColor = default, Vector4 defGlow = default) {
            if (resNode == null) return;
            if (eCfg.Hide && !reset) {
                resNode->SetScale(0, 0);
            } else {
                resNode->SetScale(reset ? defScaleX : defScaleX * eCfg.Scale.X, reset ? defScaleY : defScaleY * eCfg.Scale.Y);
                resNode->SetPositionFloat(reset ? defPosX : defPosX + eCfg.Position.X, reset ? defPosY : defPosY + eCfg.Position.Y);

                if (eCfg is TextElementConfig tec && resNode->Type == NodeType.Text) {
                    var tn = (AtkTextNode*)resNode;
                    tn->TextColor = GetColor(reset ? defColor : tec.Color );
                    tn->EdgeColor = GetColor(reset ? defGlow : tec.Glow);
                }
                
            }
        }
        
        private void UpdateSlot(AtkComponentNode* cNode, int visibleIndex, AddonPartyList.PartyListMemberStruct memberStruct, AddonPartyListMemberIntArray intArray, ref int maxX, ref int maxY, bool reset, int? forceColumnCount = null) {
            var c = cNode->Component;
            if (c == null) return;
            c->UldManager.NodeList[0]->SetWidth(reset ? (ushort)366 : (ushort)CurrentLayout.SlotWidth); // Collision Node
            c->UldManager.NodeList[1]->SetWidth(reset ? (ushort)367 : (ushort)(CurrentLayout.SlotWidth + 1));
            c->UldManager.NodeList[2]->SetWidth(reset ? (ushort)320 : (ushort)(CurrentLayout.SlotWidth - 46)); 
            c->UldManager.NodeList[3]->SetWidth(reset ? (ushort)320 : (ushort)(CurrentLayout.SlotWidth - 46));
            
            c->UldManager.NodeList[0]->SetHeight(reset ? (ushort) 44 : (ushort)(CurrentLayout.SlotHeight - 16));
            c->UldManager.NodeList[1]->SetHeight(reset ? (ushort) 69 : (ushort)(CurrentLayout.SlotHeight + 9));
            c->UldManager.NodeList[2]->SetHeight(reset ? (ushort) 69 : (ushort)(CurrentLayout.SlotHeight + 9));
            c->UldManager.NodeList[2]->SetHeight(reset ? (ushort) 48 : (ushort)(CurrentLayout.SlotHeight - 12));
            
            // Elements
            var hpComponent = memberStruct.HPGaugeComponent;
            if (hpComponent != null) {
                try {
                    HandleElementConfig(hpComponent->UldManager.NodeList[0], CurrentLayout.BarHP, reset);
                    HandleElementConfig(hpComponent->UldManager.NodeList[2], CurrentLayout.NumberHP, reset, defPosX: 4, defPosY: 21, defColor: DefaultLayout.NumberHP.Color, defGlow: DefaultLayout.NumberHP.Glow);

                    var hpGauge = hpComponent->UldManager.NodeList[0]->GetComponent();
                    if (hpGauge != null) {
                        HandleElementConfig(hpGauge->UldManager.NodeList[10], CurrentLayout.IconOvershield, reset, defPosX: 90, defPosY: 9);
                        hpGauge->UldManager.NodeList[8]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                        hpGauge->UldManager.NodeList[9]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                        HandleElementConfig(hpGauge->UldManager.NodeList[7], CurrentLayout.BarOvershield, reset, defPosY: 8);
                    }

                } catch {
                    // 
                }
            }

            var mpComponent = memberStruct.MPGaugeBar;
            if (mpComponent != null) {
                try {
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[0], CurrentLayout.BarMP, reset, defPosY: 16);
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[1], CurrentLayout.BarMP, reset, defPosY: 16);
                    mpComponent->AtkComponentBase.UldManager.NodeList[2]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                    mpComponent->AtkComponentBase.UldManager.NodeList[3]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[4], CurrentLayout.NumberMP, reset, defPosX: 5, defPosY: 22, defColor: DefaultLayout.NumberMP.Color, defGlow: DefaultLayout.NumberMP.Glow);
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[5], CurrentLayout.NumberMP, reset, defPosX: -17, defPosY: 21, defColor: DefaultLayout.NumberHP.Color, defGlow: DefaultLayout.NumberMP.Glow);
                } catch {
                    //
                }
            }
            
            HandleElementConfig((AtkResNode*) memberStruct.Name, CurrentLayout.Name, reset, defPosX: 17);
            HandleElementConfig((AtkResNode*) memberStruct.ClassJobIcon, CurrentLayout.ClassIcon, reset, defPosX: 24, defPosY: 18);
            c->UldManager.NodeList[4]->SetPositionFloat(memberStruct.ClassJobIcon->AtkResNode.X - 21 * (reset ? 1 : memberStruct.ClassJobIcon->AtkResNode.ScaleX), memberStruct.ClassJobIcon->AtkResNode.Y - 13 * (reset ? 1 : memberStruct.ClassJobIcon->AtkResNode.ScaleY));
            c->UldManager.NodeList[4]->SetScale(memberStruct.ClassJobIcon->AtkResNode.ScaleX, memberStruct.ClassJobIcon->AtkResNode.ScaleY);
            HandleElementConfig((AtkResNode*) memberStruct.GroupSlotIndicator, CurrentLayout.Slot, reset);
            HandleElementConfig((AtkResNode*) memberStruct.CastingActionName, CurrentLayout.CastbarText, reset, defPosY: 10, defColor: DefaultLayout.CastbarText.Color, defGlow: DefaultLayout.CastbarText.Glow);
            HandleElementConfig((AtkResNode*) memberStruct.CastingProgressBar, CurrentLayout.Castbar, reset, defPosX: 8 * (reset ? 1 : CurrentLayout.Castbar.Scale.X), defPosY: 7 * (reset ? 1 : CurrentLayout.Castbar.Scale.Y), defScaleX: intArray.CastingPercent >= 0 ? intArray.CastingPercent / 100f : 1f);
            HandleElementConfig((AtkResNode*) memberStruct.CastingProgressBarBackground, CurrentLayout.Castbar, reset);

            HandleElementConfig(memberStruct.EmnityBarContainer, CurrentLayout.BarEnmity, reset, defPosX: 21, defPosY: 36);

            for (var i = 0; i < c->UldManager.NodeListSize; i++) {
                var node = c->UldManager.NodeList[i];
                if (node == null) continue;
                switch (node->NodeID) {
                    case 2:
                        HandleElementConfig(node, CurrentLayout.TextEnmity, reset,  defPosX: 14, defPosY: 34, defColor: CurrentLayout.TextEnmity.Color, defGlow: CurrentLayout.TextEnmity.Glow);
                        break;
                }
            }



            if (reset) {
                cNode->AtkResNode.SetPositionFloat(0, visibleIndex * 40);
            } else {

                int columnIndex;
                int rowIndex;
                var columnCount = forceColumnCount ?? CurrentLayout.Columns;
                if (CurrentLayout.ReverseFill) {
                    columnIndex = visibleIndex % columnCount;
                    rowIndex = visibleIndex / columnCount;
                } else {
                    rowIndex = visibleIndex % columnCount;
                    columnIndex = visibleIndex / columnCount;
                }

                cNode->AtkResNode.SetPositionFloat(columnIndex * CurrentLayout.SlotWidth, rowIndex * CurrentLayout.SlotHeight);
                
                var xM = (columnIndex + 1) * CurrentLayout.SlotWidth;
                var yM = (rowIndex + 1) * CurrentLayout.SlotHeight + 16;
                
                if (xM > maxX) maxX = xM;
                if (yM > maxY) maxY = yM;
            }

            for (byte si = 0; si < 10; si++) {
                var siComponent = memberStruct.StatusIcon[si];
                if (siComponent == null) continue;
                var itcNode = siComponent->AtkComponentBase.OwnerNode;
                if (itcNode == null) continue;

                if (si >= CurrentLayout.StatusEffects.MaxDisplayed && !reset) {
                    itcNode->AtkResNode.SetScale(0, 0);
                    continue;
                } else {
                    itcNode->AtkResNode.SetScale(1, 1);
                }


                
                var (xSlot, ySlot) = reset ? (si, (byte)0) : StatusSlotPositions[si];
                
                var x = 263 + xSlot * ((25 + (reset ? 0 : CurrentLayout.StatusEffects.Separation.X)) * (reset ? 1 : CurrentLayout.StatusEffects.Scale.X));
                var y = 12 + ySlot * ((38 + (reset ? 0 : CurrentLayout.StatusEffects.Separation.Y)) * (reset ? 1 : CurrentLayout.StatusEffects.Scale.Y));
                
                HandleElementConfig((AtkResNode*) itcNode, CurrentLayout.StatusEffects, reset, defPosX: x, defPosY: y);

                if ((plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen) && !itcNode->AtkResNode.IsVisible) {
                    var imageNode = (AtkImageNode*)itcNode->Component->UldManager.NodeList[1];
                    if (intArray.StatusEffect[si] == 0) imageNode->LoadIconTexture(10205, 0);
                    imageNode->AtkResNode.ToggleVisibility(true);
                    itcNode->AtkResNode.ToggleVisibility(true);
                }
            }
        }

        public void Dispose() {
            Disable();
        }

        public void ConfigUpdated() {
            statusSlotPositions = null;
            Update(Common.GetUnitBase<AddonPartyList>());
        }
    }
}
