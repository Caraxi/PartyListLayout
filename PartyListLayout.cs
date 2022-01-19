using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Component.GUI;
using PartyListLayout.Config;
using PartyListLayout.GameStructs.NumberArray;
using PartyListLayout.GameStructs.StringArray;
using PartyListLayout.Helper;
using Vector3 = System.Numerics.Vector3;

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
                    var newList = new List<(byte, byte)>();
                    byte xO = 0;
                    byte yO = 0;

                    for (var i = 0; i < CurrentLayout.StatusEffects.MaxDisplayed; i++) {
                        newList.Add((xO, yO));
                        if (CurrentLayout.StatusEffects.TwoLines && CurrentLayout.StatusEffects.ReverseFill) {
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

                    if (CurrentLayout.StatusEffects.ReverseOrder) newList.Reverse();
                    while(newList.Count < 10) newList.Add((0, 0));
                    statusSlotPositions = newList.ToArray();
                }

                return statusSlotPositions;
            }
        }

        private (byte x, byte y)[] partyListPositions;

        public (byte x, byte y)[] PartyListPositions {
            get {
                if (partyListPositions == null) {
                    partyListPositions = new (byte x, byte y)[16];

                    byte x = 0;
                    byte y = 0;

                    for (var i = 0; i < partyListPositions.Length; i++) {
                        partyListPositions[i] = (x, y);
                        x++;
                    }
                }

                return partyListPositions;
            }
        }

        private delegate void* PartyListOnUpdate(AddonPartyList* @this, NumberArrayData* numArrayData, StringArrayData* stringArrayData);
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

            Plugin.Framework.Update += FrameworkUpdate;
            Enabled = true;
        }
        
        private void* PartyListUpdateDetour(AddonPartyList* @this, NumberArrayData* a2, StringArrayData* a3) {
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
            Plugin.Framework.Update -= FrameworkUpdate;
            if (plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen) {
                plugin.Config.PreviewMode = false;
                FrameworkUpdate(Plugin.Framework);
            }
            partyListOnUpdateHook?.Disable();
            try {
                Update(Common.GetUnitBase<AddonPartyList>(), true);
            } catch (Exception ex) {
                SimpleLog.Error(ex);
            }

            FrameworkUpdate(Plugin.Framework);

            Enabled = false;
        }

        private bool isPreviewing;

        private void FrameworkUpdate(Dalamud.Game.Framework framework) {
            if (!(plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen)) {
                if (isPreviewing) {
                    SimpleLog.Log("Preview Mode Disabled");
                    CleanupPreview();
                    isPreviewing = false;
                } else {
                    return;
                }
            }
            if (plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen) isPreviewing = true;
            var atkArrayDataHolder = Framework.Instance()->GetUiModule()->RaptureAtkModule.AtkModule.AtkArrayDataHolder;

            var addon = Common.GetUnitBase<AddonPartyList>();
            if (addon != null) {
                addon->AtkUnitBase.OnUpdate(atkArrayDataHolder.NumberArrays, atkArrayDataHolder.StringArrays);
            }
        }

        private readonly Stopwatch previewTickStopwatch = new();

        private void SetupPreview(AddonPartyList* partyList, AddonPartyListIntArray* intArray, AddonPartyListStringArray* stringArray) {
            if (!previewTickStopwatch.IsRunning) previewTickStopwatch.Start();
            var previewtickCountCounter = (int) (previewTickStopwatch.ElapsedMilliseconds % int.MaxValue);
            var ia = (AddonPartyListMemberIntArray*) &intArray->PartyMember;
            for (var i = intArray->PartyMemberCount; i < 8 && i < plugin.Config.PreviewCount; i++) {
                var m = partyList->PartyMember[i];
                var mia = intArray->PartyMember[i];

                m.PartyMemberComponent->OwnerNode->AtkResNode.ToggleVisibility(true);

                m.Name->SetText($" Party Member'{i+1}");

                m.PartyMemberComponent->OwnerNode->AtkResNode.Color.A = 255;

                m.Name->AtkResNode.ToggleVisibility(i % 2 == 0 || CurrentLayout.Name.KeepVisibleWhileCasting);

                m.ClassJobIcon->AtkResNode.ToggleVisibility(true);
                m.ClassJobIcon->LoadIconTexture(mia.ClassJobIcon > 0 ? mia.ClassJobIcon : 62101 + i, 0);
                m.CastingActionName->SetText($"Spell Name {i+1}");
                m.CastingActionName->AtkResNode.ToggleVisibility(i % 2 != 0);
                m.CastingProgressBar->AtkResNode.ToggleVisibility(i % 2 != 0);
                m.CastingProgressBarBackground->AtkResNode.ToggleVisibility(i % 2 != 0);

                for (var j = 0; j < 10; j++) {
                    var nodeList = m.StatusIcon[j]->AtkComponentBase.UldManager.NodeList;
                    nodeList[0]->ToggleVisibility(false);
                    nodeList[1]->ToggleVisibility(true);
                    nodeList[1]->GetAsAtkImageNode()->LoadIconTexture(17861 + j, 0);
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
                            n->SetScale((previewtickCountCounter / (7 * i) % 100) / 100f, 1f);
                            break;
                        }
                        case 9: {
                            n->ToggleVisibility(true);
                            break;
                        }
                    }
                }



                ia[i].CastingPercent = previewtickCountCounter / (8 + i) % 100;

            }
        }

        private void CleanupPreview() {
            var addon = Common.GetUnitBase<AddonPartyList>();
            if (addon == null) return;
            var atkArrayDataHolder = Framework.Instance()->GetUiModule()->RaptureAtkModule.AtkModule.AtkArrayDataHolder;
            var partyListNumbers = atkArrayDataHolder.NumberArrays[4];
            var partyListStrings = atkArrayDataHolder.StringArrays[3];
            var partyIntList = (AddonPartyListIntArray*) partyListNumbers->IntArray;
            var partyStringList = (AddonPartyListStringArray*)partyListStrings->StringArray;

            for (var p = 0; p < 8; p++) {

                // Reload all the status effect icons
                for (var s = 0; s < 10; s++) {
                    var siComponent = addon->PartyMember[p].StatusIcon[s];
                    if (siComponent == null) continue;
                    var itcNode = siComponent->AtkComponentBase.OwnerNode;
                    var icon = (AtkImageNode*)itcNode->Component->UldManager.NodeList[1];
                    icon->LoadIconTexture(partyIntList->PartyMember[p].StatusEffect[s], 0);
                }
            }
        }


        private void Update(AddonPartyList* partyList, bool reset = false) {

            if (partyList == null) return;
            if (partyList->AtkUnitBase.UldManager.NodeListSize < 21) return;

            var atkArrayDataHolder = Framework.Instance()->GetUiModule()->RaptureAtkModule.AtkModule.AtkArrayDataHolder;
            var partyListNumbers = atkArrayDataHolder.NumberArrays[4];
            var partyListStrings = atkArrayDataHolder.StringArrays[3];
            var partyIntList = (AddonPartyListIntArray*) partyListNumbers->IntArray;
            var partyStringList = (AddonPartyListStringArray*)partyListStrings->StringArray;

            if (plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen && !reset) {
                SetupPreview(partyList, partyIntList, partyStringList);
            }

            HandleElementConfig((AtkResNode*) partyList->PartyTypeTextNode, reset ? DefaultLayout.PartyTypeText : CurrentLayout.PartyTypeText, false);


            var visibleIndex = 0;

            var maxX = 0;
            var maxY = 0;


            for (var i = 0; i < partyList->MemberCount; i++) {
                var pm = partyList->PartyMember[i];
                var intList = partyIntList->PartyMember[i];
                var stringList = partyStringList->PartyMembers[i];
            }


            for (var i = 0; i < 17; i++) {
                try {
                    var pm = i switch {
                        >= 0 and <= 7 => partyList->PartyMember[i],
                        >= 8 and <= 14 => partyList->TrustMember[i - 8],
                        15 => partyList->Chocobo,
                        16 => partyList->Pet,
                        _ => throw new ArgumentOutOfRangeException()
                    };

                    var intList = i switch {
                        >= 0 and <= 7 => partyIntList->PartyMember[i],
                        _ => default
                    };

                    var stringList = i switch {
                        >= 0 and <= 7 => partyStringList->PartyMembers[i],
                        _ => default
                    };

                    var c = pm.PartyMemberComponent;
                    if (c == null) continue;
                    var cNode = c->OwnerNode;
                    if (cNode == null) continue;
                    
                    if (cNode->AtkResNode.IsVisible || reset) UpdateSlot(cNode, visibleIndex, pm, intList, stringList, ref maxX, ref maxY, reset);
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

        private void HandleElementConfig(AtkResNode* resNode, ElementConfig eCfg, bool reset, float defScaleX = 1f, float defScaleY = 1f, float defPosX = 0, float defPosY = 0, Vector4 defColor = default, Vector4 defGlow = default, Vector3 defMultiplyColor = default, Vector3 defAddColor = default, string defText = null) {
            if (resNode == null) return;
            if (eCfg.Hide && !reset) {
                resNode->SetScale(0, 0);
            } else {
                resNode->SetScale(reset ? defScaleX : defScaleX * eCfg.Scale.X, reset ? defScaleY : defScaleY * eCfg.Scale.Y);
                resNode->SetPositionFloat(reset ? defPosX : defPosX + eCfg.Position.X, reset ? defPosY : defPosY + eCfg.Position.Y);

                if (eCfg.EditorFlags.HasFlag(LayoutElementFlags.CanTint)) {
                    var multiply = reset ? defMultiplyColor : eCfg.MultiplyColor;
                    resNode->MultiplyRed = (byte)(multiply.X * 255);
                    resNode->MultiplyGreen = (byte)(multiply.Y * 255);
                    resNode->MultiplyBlue = (byte)(multiply.Z * 255);

                    var add = reset ? defAddColor : eCfg.AddColor;
                    resNode->AddRed = (ushort)(add.X * 1000);
                    resNode->AddGreen = (ushort)(add.Y * 1000);
                    resNode->AddBlue = (ushort)(add.Z * 1000);
                }

                if (eCfg is TextElementConfig tec && resNode->Type == NodeType.Text) {
                    var tn = (AtkTextNode*)resNode;
                    tn->TextColor = GetColor(reset ? defColor : tec.Color );
                    tn->EdgeColor = GetColor(reset ? defGlow : tec.Glow);

                    if (defText != null && tec is PlayerNameTextElementConfig pnTac) {
                        if (reset) {
                            tn->SetText(defText);
                        } else {
                            if (pnTac.KeepVisibleWhileCasting) resNode->ToggleVisibility(true);

                            var splitName = defText.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

                            if (splitName.Length == 3) {
                                var newNameBuilder = new StringBuilder();
                                if (!pnTac.RemoveLevel) {
                                    newNameBuilder.Append(splitName[0]);
                                }

                                if (!pnTac.RemoveFirst) {
                                    if (newNameBuilder.Length > 0) newNameBuilder.Append(" ");
                                    newNameBuilder.Append(splitName[1]);
                                }

                                if (!pnTac.RemoveSecond) {
                                    if (newNameBuilder.Length > 0) newNameBuilder.Append(" ");
                                    newNameBuilder.Append(splitName[2]);
                                }

                                var newName = newNameBuilder.ToString();

                                tn->SetText(newName);
                            }
                        }
                    }
                }



            }
        }

        private void UpdateSlot(AtkComponentNode* cNode, int visibleIndex, AddonPartyList.PartyListMemberStruct memberStruct, AddonPartyListMemberIntArray intArray, AddonPartyListPartyMemberStrings stringArray, ref int maxX, ref int maxY, bool reset, int? forceColumnCount = null) {
            var c = cNode->Component;
            if (c == null) return;
            c->UldManager.NodeList[0]->SetWidth(reset ? (ushort)366 : (ushort)((CurrentLayout.SlotWidth - 30) * CurrentLayout.SelectionArea.Scale.X)); // Collision Node
            c->UldManager.NodeList[0]->SetHeight(reset ? (ushort) 44 : (ushort)(CurrentLayout.SlotHeight * CurrentLayout.SelectionArea.Scale.Y));
            c->UldManager.NodeList[0]->SetPositionFloat(reset ? 16 : 46 + CurrentLayout.SelectionArea.Position.X, reset ? 12 : 18 + CurrentLayout.SelectionArea.Position.Y);

            c->UldManager.NodeList[1]->SetWidth(reset ? (ushort)367 : (ushort)((CurrentLayout.SlotWidth - 5) * CurrentLayout.SelectionArea.Scale.X));
            c->UldManager.NodeList[1]->SetHeight(reset ? (ushort) 69 : (ushort)((CurrentLayout.SlotHeight + 9) * CurrentLayout.SelectionArea.Scale.Y));
            c->UldManager.NodeList[1]->SetPositionFloat(reset ? 5 : 5 + CurrentLayout.SelectionArea.Position.X, reset ? -3 : -3 + CurrentLayout.SelectionArea.Position.Y);


            c->UldManager.NodeList[2]->SetWidth(reset ? (ushort)320 : (ushort)((CurrentLayout.SlotWidth  - 30) * CurrentLayout.SelectionArea.Scale.X));
            c->UldManager.NodeList[2]->SetHeight(reset ? (ushort) 69 : (ushort)((CurrentLayout.SlotHeight + 9) * CurrentLayout.SelectionArea.Scale.Y));

            c->UldManager.NodeList[3]->SetWidth(reset ? (ushort)320 : (ushort)((CurrentLayout.SlotWidth) * CurrentLayout.SelectionArea.Scale.X));
            c->UldManager.NodeList[3]->SetHeight(reset ? (ushort) 48 : (ushort)((CurrentLayout.SlotHeight - 12) * CurrentLayout.SelectionArea.Scale.Y));

            // Elements
            var hpComponent = memberStruct.HPGaugeComponent;
            if (hpComponent != null) {
                try {
                    HandleElementConfig(hpComponent->UldManager.NodeList[0], CurrentLayout.BarHP, reset, defMultiplyColor: DefaultLayout.BarHP.MultiplyColor);
                    HandleElementConfig(hpComponent->UldManager.NodeList[2], CurrentLayout.NumberHP, reset, defPosX: 4, defPosY: 21, defColor: DefaultLayout.NumberHP.Color, defGlow: DefaultLayout.NumberHP.Glow);

                    var hpGauge = hpComponent->UldManager.NodeList[0]->GetComponent();
                    if (hpGauge != null) {
                        HandleElementConfig(hpGauge->UldManager.NodeList[10], CurrentLayout.IconOvershield, reset, defPosX: 90, defPosY: 9);
                        hpGauge->UldManager.NodeList[8]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                        hpGauge->UldManager.NodeList[9]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                        HandleElementConfig(hpGauge->UldManager.NodeList[7], CurrentLayout.BarOvershield, reset, defPosY: 8, defMultiplyColor: DefaultLayout.BarOvershield.MultiplyColor);
                    }

                } catch {
                    // 
                }
            }

            var mpComponent = memberStruct.MPGaugeBar;
            if (mpComponent != null) {
                try {
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[0], CurrentLayout.BarMP, reset, defPosY: 16, defMultiplyColor: DefaultLayout.BarMP.MultiplyColor);
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[1], CurrentLayout.BarMP, reset, defPosY: 16, defMultiplyColor: DefaultLayout.BarMP.MultiplyColor);
                    mpComponent->AtkComponentBase.UldManager.NodeList[2]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                    mpComponent->AtkComponentBase.UldManager.NodeList[3]->SetScale(reset ? 1 : 0, reset ? 1 : 0);
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[4], CurrentLayout.NumberMP, reset, defPosX: 5, defPosY: 22, defColor: DefaultLayout.NumberMP.Color, defGlow: DefaultLayout.NumberMP.Glow);
                    HandleElementConfig(mpComponent->AtkComponentBase.UldManager.NodeList[5], CurrentLayout.NumberMP, reset, defPosX: -17, defPosY: 21, defColor: DefaultLayout.NumberHP.Color, defGlow: DefaultLayout.NumberMP.Glow);
                } catch {
                    //
                }
            }
            
            HandleElementConfig((AtkResNode*) memberStruct.Name, CurrentLayout.Name, reset, defPosX: 17, defText: (plugin.Config.PreviewMode && plugin.ConfigWindow.IsOpen && !reset) ? $" Party{visibleIndex + 1} Member{ visibleIndex + 1}" : stringArray.PlayerNameAndLevel);
            HandleElementConfig((AtkResNode*) memberStruct.ClassJobIcon, CurrentLayout.ClassIcon, reset, defPosX: 24, defPosY: 18);
            c->UldManager.NodeList[4]->SetPositionFloat((reset ? 0 : -CurrentLayout.SelectionArea.Position.X) + memberStruct.ClassJobIcon->AtkResNode.X - 21 * (reset ? 1 : memberStruct.ClassJobIcon->AtkResNode.ScaleX), (reset ? 0 : -CurrentLayout.SelectionArea.Position.Y) + memberStruct.ClassJobIcon->AtkResNode.Y - 13 * (reset ? 1 : memberStruct.ClassJobIcon->AtkResNode.ScaleY));
            c->UldManager.NodeList[4]->SetScale(memberStruct.ClassJobIcon->AtkResNode.ScaleX, memberStruct.ClassJobIcon->AtkResNode.ScaleY);
            HandleElementConfig((AtkResNode*) memberStruct.GroupSlotIndicator, CurrentLayout.Slot, reset);
            HandleElementConfig((AtkResNode*) memberStruct.CastingActionName, CurrentLayout.CastbarText, reset, defPosY: 10, defColor: DefaultLayout.CastbarText.Color, defGlow: DefaultLayout.CastbarText.Glow);
            HandleElementConfig((AtkResNode*) memberStruct.CastingProgressBar, CurrentLayout.Castbar, reset, defPosX: 8 * (reset ? 1 : CurrentLayout.Castbar.Scale.X), defPosY: 7 * (reset ? 1 : CurrentLayout.Castbar.Scale.Y), defScaleX: intArray.CastingPercent >= 0 ? intArray.CastingPercent / 100f : 1f);
            HandleElementConfig((AtkResNode*) memberStruct.CastingProgressBarBackground, CurrentLayout.Castbar, reset);

            HandleElementConfig(memberStruct.EmnityBarContainer, CurrentLayout.BarEnmity, reset, defPosX: 21, defPosY: 36, defMultiplyColor: DefaultLayout.BarEnmity.MultiplyColor);

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
                    if (intArray.StatusEffect[si] == 0 || intArray.StatusEffectCount < (si + 1)) {
                        imageNode->LoadIconTexture(17861 + si, 0);
                    }
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
