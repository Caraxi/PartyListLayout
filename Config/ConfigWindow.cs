
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Dalamud.Utility;
using ImGuiNET;
using Newtonsoft.Json;
using PartyListLayout.Converter;
using PartyListLayout.GameStructs.NumberArray;
using PartyListLayout.GameStructs.StringArray;
using PartyListLayout.Helper;
using Util = PartyListLayout.Helper.Util;

namespace PartyListLayout.Config {
    public unsafe class ConfigWindow {

        private readonly Plugin plugin;
        private bool isVisible;

        private string importError = string.Empty;
        private readonly Stopwatch errorStopwatch = new();

        public PluginConfig Config => plugin.Config;

        private IEnumerable<(FieldInfo field, LayoutElementAttribute attr)> layoutOptions = null;

        public bool IsOpen {
            get => isVisible;
            set {
                if (value) {
                    Show();
                } else {
                    Hide();
                }
            }
        }

        public ConfigWindow(Plugin plugin) {
            this.plugin = plugin;
        }

        public void Hide() {
            if (!isVisible) return;
            isVisible = false;
            Plugin.PluginInterface.UiBuilder.Draw -= DrawWindow;
            AutoSavePreset();
        }

        public void Show() {
            if (isVisible) return;
            isVisible = true;
            Plugin.PluginInterface.UiBuilder.Draw += DrawWindow;
        }

        public void Toggle() {
            if (isVisible) {
                Hide();
            } else {
                Show();
            }
        }


        private void ElementConfigEditor(string name, ElementConfig eCfg, ElementConfig defaultCfg, ref bool c, LayoutElementFlags flags) {

            var elementVisible = !eCfg.Hide;


            if (flags.HasFlag(LayoutElementFlags.NoHide)) {
                ImGui.PushStyleColor(ImGuiCol.FrameBg, Vector4.Zero);
                ImGui.PushStyleColor(ImGuiCol.FrameBgActive, Vector4.Zero);
                ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, Vector4.Zero);
                ImGui.Checkbox($"##hideItem_{name}", ref elementVisible);
                ImGui.PopStyleColor(3);
            } else {
                if (ImGui.Checkbox($"##hideItem_{name}", ref elementVisible)) {
                    eCfg.Hide = !elementVisible;
                    c = true;
                }
            }


            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, eCfg.Hide ? 0x88FFFFFF : 0xFFFFFFFF);
            var isOpen = ImGui.CollapsingHeader($"{name}");
            ImGui.PopStyleColor();

            if (eCfg.Hide) {
                ImGui.SameLine();
                var s = ImGui.CalcTextSize("Hidden ");
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - s.X);
                ImGui.PushStyleColor(ImGuiCol.Text, 0xFF4444FF);
                ImGui.Text("Hidden");
                ImGui.PopStyleColor();
            }

            if (isOpen) {
                ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - ImGui.CalcTextSize("Default").X - 13 * ImGui.GetIO().FontGlobalScale);

                ImGui.PushStyleColor(ImGuiCol.Button, 0xAA00FFFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xCC00FFFF);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xEE00FFFF);
                if (ImGui.SmallButton($"Default##{uid++}")) {
                    var d = JsonConvert.SerializeObject(defaultCfg);
                    var o = JsonConvert.DeserializeObject(d, eCfg.GetType());

                    foreach (var oF in o.GetType().GetFields()) {
                        oF.SetValue(eCfg, oF.GetValue(o));
                    }

                    c = true;
                }
                ImGui.PopStyleColor(3);
                ImGui.SameLine(0);


                ImGui.Indent(40 * ImGui.GetIO().FontGlobalScale);
                eCfg.Editor(name, ref c, plugin.PartyListLayout);
                ImGui.Unindent(40 * ImGui.GetIO().FontGlobalScale);
            }
        }

        private class TabAttribute : Attribute {

            public string TabName { get; }

            public bool AlignRight { get; }

            public TabAttribute(string tabName, bool alignRight = false) {
                this.TabName = tabName;
                this.AlignRight = alignRight;
            }
        }

        private enum Tabs {
            [Tab("Party Grid")]
            Grid,
            [Tab("Member Slot")]
            MemberSlot,
            [Tab("General Options", true)]
            Options,
            [Tab("Presets", true)]
            Presets,
        }

        private Tabs selectedTab = Tabs.Grid;
        private string inputPresetName = string.Empty;

        private readonly char[] filenameIllegalChars = {
            '<', '>', ':', '"', '/', '|', '?', '*', '.'
        };

        public void SetupLayoutFlags() {
            layoutOptions ??= typeof(LayoutConfig).GetFields().Select(f => (f, (LayoutElementAttribute)f.GetCustomAttribute(typeof(LayoutElementAttribute)))).Where(a => a.Item2 != null).OrderBy(a => a.Item2.Priority).ThenBy(a => a.Item2.Name);
            foreach (var o in layoutOptions) {
                var f = (ElementConfig) o.field.GetValue(Config.CurrentLayout);
                f.EditorFlags = o.attr.Flags;
            }
        }


        private void DrawWindow() {
            SetupLayoutFlags();

            uid = 0;
            var isOpen = true;

            ImGui.SetNextWindowSizeConstraints(new Vector2(650) * ImGui.GetIO().FontGlobalScale, new Vector2(float.MaxValue));

            if (ImGui.Begin($"{plugin.Name} Config", ref isOpen)) {
                var c = false;
                var p = ImGui.GetCursorPos();

                if (!string.IsNullOrEmpty(importError)) {
                    if (!errorStopwatch.IsRunning) errorStopwatch.Restart();
                    ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - (ImGui.CalcTextSize(importError).X + 5));
                    ImGui.TextColored(new Vector4(1, 0, 0, 1), importError);
                    if (errorStopwatch.ElapsedMilliseconds > 5000) {
                        importError = string.Empty;
                        errorStopwatch.Stop();
                    }
                } else {
                    ImGui.SetCursorPosX(ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X - (230 * ImGui.GetIO().FontGlobalScale));

                    if (!Config.HideKofi) {
                        var buttonText = "Support on Ko-fi";
                        var buttonColor = 0x005E5BFFu;
                        ImGui.PushStyleColor(ImGuiCol.Button, 0xFF000000 | buttonColor);
                        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0xDD000000 | buttonColor);
                        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0xAA000000 | buttonColor);

                        if (ImGui.Button(buttonText, new Vector2(120, 24))) {
                            Common.OpenBrowser("https://ko-fi.com/Caraxi");
                        }
                        ImGui.PopStyleColor(3);
                    } else {
                        ImGui.Dummy(new Vector2(120, 24));
                    }

                    ImGui.SameLine();

                    if (ImGui.Button("Export")) {
                        var newExport = Util.Base64Encode(Util.Compress(LayoutSerializer.SerializeLayout(Config.CurrentLayout)));
                        ImGui.SetClipboardText(newExport);
                    }

                    if (ImGui.IsItemHovered()) ImGui.SetTooltip($"Copy {plugin.Name} config to clipboard.");
                    ImGui.SameLine();
                    if (ImGui.Button("Import")) {
                        importError = string.Empty;

                        try {

                            AutoSavePreset();
                            var importLayout = LayoutSerializer.DeserializeLayout(Util.Decompress(Util.Base64Decode(ImGui.GetClipboardText())));
                            if (importLayout != null) {
                                plugin.Config.CurrentLayout = importLayout;
                                SetupLayoutFlags();
                            }
                            /*
                            var json = ImGui.GetClipboardText();
                            var cfg = ImportConfig(json);
                            if (cfg != null) {
                                plugin.Config.CurrentLayout = cfg;
                                SetupLayoutFlags();
                            }
                            */
                            c = true;
                        } catch (Exception ex) {
                            SimpleLog.Error(ex);
                            importError = "Invalid Import String";
                        }
                    }

                    if (ImGui.IsItemHovered()) ImGui.SetTooltip($"Load {plugin.Name} config from clipboard.");
                }


                ImGui.SetCursorPos(p);
                c |= ImGui.Checkbox("Preview", ref Config.PreviewMode);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip("Party list must not be hidden for preview to work.\nEither join a party or disable 'Hide party list when solo' in the character config.");
                if (Config.PreviewMode) {
                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(80);
                    c |= ImGui.SliderInt("##previewCount", ref Config.PreviewCount, 1, 8);
                }

                ImGui.Separator();


                ImGui.BeginChild("partyListLayout_scroll", new Vector2(-1, -1), false);

                var tabHeight = ImGui.CalcTextSize("A").Y + (2 * ImGui.GetStyle().FramePadding.Y);


                ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
                ImGui.BeginChild("partyListLayout_tabSelect", new Vector2(-1, tabHeight), true, ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse);
                ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 0);
                ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(2, 0));


                var first = true;
                var firstRight = true;
                var alignRightPos = ImGui.GetWindowContentRegionMax().X - ImGui.GetWindowContentRegionMin().X;

                foreach (var t in (Tabs[])Enum.GetValues(typeof(Tabs))) {
                    var attr = t.GetAttribute<TabAttribute>();
                    if (attr == null) continue;

                    if (attr.AlignRight) {
                        var size = ImGui.CalcTextSize(attr.TabName).X + ( 2 * ImGui.GetStyle().FramePadding.X) + ImGui.GetStyle().ItemSpacing.X;
                        alignRightPos -= size;
                        if (first) {
                            ImGui.Dummy(Vector2.Zero);
                        }
                        if (firstRight) {
                            alignRightPos += ImGui.GetStyle().ItemSpacing.X;
                            firstRight = false;
                        }
                        ImGui.SameLine(alignRightPos);
                    } else {
                        if (!first) ImGui.SameLine();
                    }

                    first = false;

                    if (selectedTab == t) ImGui.PushStyleColor(ImGuiCol.Button, *ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive));
                    if (selectedTab == t) ImGui.PushStyleColor(ImGuiCol.ButtonHovered, *ImGui.GetStyleColorVec4(ImGuiCol.ButtonActive));
                    var buttonPressed = ImGui.Button($"{attr.TabName}");
                    if (selectedTab == t) ImGui.PopStyleColor(2);
                    if (buttonPressed) selectedTab = t;
                }

                ImGui.PopStyleVar();
                ImGui.PopStyleVar();
                ImGui.EndChild();
                ImGui.PopStyleVar();


                ImGui.BeginChild("partyListLayout_selectedTab", new Vector2(-1));

                switch (selectedTab) {
                    case Tabs.Grid: {
                        ImGui.Text("Columns / Rows:");
                        ImGui.Indent();
                        c |= ImGui.Checkbox("Fill Rows First?", ref Config.CurrentLayout.ReverseFill);
                        c |= ImGui.SliderInt($"{(Config.CurrentLayout.ReverseFill ? "Column" : "Row")} Count###columnCount", ref Config.CurrentLayout.Columns, 1, 8);
                        c |= ImGui.Checkbox("Grow List Upwards", ref Config.CurrentLayout.GrowUp);
                        ImGui.Unindent();

                        ImGui.Text("Sizing:");
                        ImGui.Indent();
                        c |= ImGui.SliderInt("Width", ref Config.CurrentLayout.SlotWidth, 50, 500);
                        c |= ImGui.SliderInt("Height", ref Config.CurrentLayout.SlotHeight, 5, 160);
                        ImGui.Unindent();

                        

                        ImGui.Separator();
                        
                        foreach (var a in layoutOptions.Where(a => a.attr.Tab == LayoutElementTab.PartyGrid)) {
                            ElementConfigEditor(a.attr.Name, (ElementConfig) a.field.GetValue(Config.CurrentLayout), (ElementConfig) a.field.GetValue(LayoutConfig.Default), ref c, a.attr.Flags);
                        }
                        break;
                    }
                    case Tabs.MemberSlot: {
                        foreach (var a in layoutOptions.Where(a => a.attr.Tab == LayoutElementTab.MemberSlot)) {
                            ElementConfigEditor(a.attr.Name, (ElementConfig) a.field.GetValue(Config.CurrentLayout), (ElementConfig) a.field.GetValue(LayoutConfig.Default), ref c, a.attr.Flags);
                        }
                        break;
                    }
                    case Tabs.Presets: {

                        var dirStr = Plugin.PluginInterface.GetPluginConfigDirectory();
                        var dirPath = new DirectoryInfo(Path.Combine(dirStr, "Presets"));

                        if (!dirPath.Exists) {
                            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Preset Directory Missing");
                            dirPath.Create();
                            break;
                        }

                        ImGui.SetNextItemWidth(-120);
                        ImGui.InputTextWithHint("###newPresetNameInput", "New Preset Name", ref inputPresetName, 128, ImGuiInputTextFlags.CallbackCharFilter, data => {
                            if (data->EventChar == '/') data->EventChar = '\\';
                            if (filenameIllegalChars.Contains((char)data->EventChar)) data->EventChar = '_';
                            return 0;
                        });
                        ImGui.SameLine();


                        if (ImGui.Button("Save Preset", new Vector2(-1, ImGui.GetItemRectSize().Y))) {
                            if (!(string.IsNullOrEmpty(inputPresetName) || inputPresetName.Any(filenameIllegalChars.Contains)  || inputPresetName.Contains("\\\\") || inputPresetName.StartsWith("\\") || inputPresetName.EndsWith("\\"))) {
                                var json = GetConfigExport(true);
                                var presetPath = Path.Combine(dirStr, "Presets", inputPresetName + PresetFileSuffix);
                                var presetFile = new FileInfo(presetPath);
                                if (presetFile.Directory != null) {
                                    if (!presetFile.Exists) {
                                        if (!presetFile.Directory.Exists) {
                                            presetFile.Directory.Create();
                                        }

                                        File.WriteAllText(presetPath, json);
                                    }
                                }

                            }

                            inputPresetName = string.Empty;
                        }

                        if (inputPresetName.Any(filenameIllegalChars.Contains) || inputPresetName.Contains("\\\\") || inputPresetName.StartsWith("\\") || inputPresetName.EndsWith("\\")) {
                            ImGui.TextColored(new Vector4(1, 0, 0, 1), "Invalid Preset Name");
                        } else {
                            var presetPath = Path.Combine(dirStr, "Presets", inputPresetName + PresetFileSuffix);
                            var presetFile = new FileInfo(presetPath);
                            if (presetFile.Exists) {
                                ImGui.TextColored(new Vector4(1, 0, 0, 1), "Preset already exists.");
                            }
                        }

                        if (ImGui.BeginTable("partyListLayout_presetTable", 4)) {
                            ImGui.TableSetupColumn("Preset Name");
                            ImGui.TableSetupColumn("Created", ImGuiTableColumnFlags.WidthFixed, 120 * ImGui.GetIO().FontGlobalScale);
                            ImGui.TableSetupColumn("Modified", ImGuiTableColumnFlags.WidthFixed, 120 * ImGui.GetIO().FontGlobalScale);
                            ImGui.TableSetupColumn("Load", ImGuiTableColumnFlags.WidthFixed, 100 * ImGui.GetIO().FontGlobalScale);

                            ImGui.TableHeadersRow();
                            ImGui.TableNextColumn();

                            ImGui.PushStyleColor(ImGuiCol.Text, 0xFF00FFFF);

                            ImGui.Text("Default Preset");
                            ImGui.TableNextColumn();
                            ImGui.Text("Built-in");
                            ImGui.TableNextColumn();
                            ImGui.Text("Built-in");
                            ImGui.TableNextColumn();
                            if (ImGui.Button("Load Default")) {
                                AutoSavePreset();
                                plugin.Config.CurrentLayout = new LayoutConfig();
                                c = true;
                            }
                            ImGui.TableNextColumn();

                            ImGui.PopStyleColor();


                            PrintPresetList(dirPath, ref c);

                            ImGui.EndTable();
                        }

                        break;
                    }
                    case Tabs.Options: {

                        c |= ImGui.Checkbox($"Autosave Layout    Keep", ref plugin.Config.AutoSave);
                        ImGui.SameLine();
                        ImGui.SetNextItemWidth(90 * ImGui.GetIO().FontGlobalScale);
                        c |= ImGui.InputInt("Backups##autosaveLimit", ref plugin.Config.MaxAutoSave);


                        ImGui.Dummy(new Vector2(25) * ImGui.GetIO().FontGlobalScale);
                        c |= ImGui.Checkbox("Hide Ko-fi Button", ref plugin.Config.HideKofi);


#if DEBUG
                        var atkArrayDataHolder = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance()->GetUiModule()->GetRaptureAtkModule()->AtkModule.AtkArrayDataHolder;
                        var partyListNumbers = atkArrayDataHolder.NumberArrays[4];
                        var partyListStrings = atkArrayDataHolder.StringArrays[3];
                        var partyIntList = (AddonPartyListIntArray*) partyListNumbers->IntArray;
                        var partyStringList = (AddonPartyListStringArray*)partyListStrings->StringArray;

                        try {
                            Dalamud.Utility.Util.ShowStruct(partyIntList);
                        } catch (Exception ex) {
                            ImGui.Text($"{ex}");
                        }
#endif
                        break;
                    }
                }

                ImGui.EndChild();


                try {
                    if (c) {
                        plugin.PartyListLayout.ConfigUpdated();
                        Plugin.PluginInterface.SavePluginConfig(Config);
                    }
                } catch (Exception ex) {
                    SimpleLog.Error(ex);
                }

                ImGui.EndChild();

            }
            ImGui.End();
            if (!isOpen) {
                Hide();
            }
        }

        private const string PresetFileSuffix = $".preset.json";

        private bool DirectoryHasPresets(DirectoryInfo directoryInfo) {
            return directoryInfo.GetFiles($"*{PresetFileSuffix}").Length > 0 || directoryInfo.GetDirectories().Any(DirectoryHasPresets);
        }

        private long uid;
        private void PrintPresetList(DirectoryInfo directoryInfo, ref bool c) {

            foreach (var d in directoryInfo.GetDirectories()) {
                if (!DirectoryHasPresets(d)) continue;
                ImGui.PushStyleColor(ImGuiCol.Text, 0xFFFFFF55);
                var dOpen = ImGui.TreeNodeEx(d.Name);
                ImGui.PopStyleColor();
                ImGui.TableNextRow();
                ImGui.TableNextColumn();

                if (!dOpen) continue;
                PrintPresetList(d, ref c);
                ImGui.TreePop();
            }

            var files = directoryInfo.GetFiles($"*{PresetFileSuffix}");
            foreach (var f in files.OrderByDescending(f => f.LastWriteTime)) {
                ImGui.Bullet();
                ImGui.SameLine();
                ImGui.Text($"{f.Name.Substring(0, f.Name.Length - PresetFileSuffix.Length)}");
                ImGui.TableNextColumn();
                ImGui.TextWrapped($"{f.CreationTime.ToShortDateString()}  {f.CreationTime.ToShortTimeString()}");
                ImGui.TableNextColumn();
                ImGui.TextWrapped($"{f.LastWriteTime.ToShortDateString()}  {f.LastWriteTime.ToShortTimeString()}");
                ImGui.TableNextColumn();

                if (Plugin.KeyState[0x11]) {
                    if (ImGui.SmallButton($"Delete Preset##{uid++}")) {
                        f.Delete();
                    }
                } else {
                    if (ImGui.SmallButton($"Load Preset##{uid++}")) {
                        try {
                            AutoSavePreset();
                            var json = File.ReadAllText(f.FullName);
                            var layoutCfg = ImportConfig(json, true);
                            if (layoutCfg != null) {
                                plugin.Config.CurrentLayout = layoutCfg;
                                SetupLayoutFlags();
                                c = true;
                            }
                        } catch (Exception ex) {
                            SimpleLog.Error(ex);
                        }
                    }
                }


                ImGui.TableNextColumn();
            }
        }

        private void AutoSavePreset() {
            if (plugin.Config.AutoSave) {
                var json = GetConfigExport(true);

                var dir = Path.Combine(Plugin.PluginInterface.GetPluginConfigDirectory(), "Presets", "__AutoSave__");
                var file = Path.Combine(dir, $"{DateTime.Now.ToFileTimeUtc()}{PresetFileSuffix}");
                if (File.Exists(file)) return;
                var dirInfo = new DirectoryInfo(dir);
                if (!dirInfo.Exists) dirInfo.Create();

                File.WriteAllText(file, json);

                var autoSaveList = dirInfo.GetFiles($"*{PresetFileSuffix}");
                if (autoSaveList.Length > Config.MaxAutoSave) {
                    foreach (var f in autoSaveList.OrderByDescending(f => f.LastWriteTime).Skip(Config.MaxAutoSave).Take(2)) {
                        f.Delete();
                    }
                }
            }
        }

        private string GetConfigExport(bool uncompressed = false) {
            var json = JsonConvert.SerializeObject(Config.CurrentLayout, uncompressed ? Formatting.Indented : Formatting.None, new VectorJsonConverter());
            if (uncompressed) return json;
            var compressedString = Util.Compress($"PartyListLayout::{json}");
            SimpleLog.Verbose($"Compressed Length: {compressedString.Length}");
            return Util.Base64Encode(compressedString);
        }

        private LayoutConfig ImportConfig(string str, bool uncompressed = false) {
            try {
                string json;

                if (uncompressed) {
                    json = str;
                } else {
                    var bytes = Util.Base64Decode(str);
                    var decompressedString = Util.DecompressString(bytes);
                    if (string.IsNullOrEmpty(decompressedString)) {
                        importError = "Empty Clipboard";
                        return null;
                    }
                    if (decompressedString.StartsWith("SimpleTweaks.UiAdjustments@PartyListLayout::")) {
                        decompressedString = decompressedString.Replace("SimpleTweaks.UiAdjustments@", "");
                    }

                    if (!decompressedString.StartsWith($"PartyListLayout::", StringComparison.InvariantCultureIgnoreCase)) {
                        SimpleLog.Log("Incorrect Identifier");
                        SimpleLog.Log($"{decompressedString}");
                        return null;
                    }

                    json = decompressedString.Substring($"PartyListLayout::".Length);
                }
                var obj = JsonConvert.DeserializeObject<LayoutConfig>(json, new VectorJsonConverter());
                return obj;
            } catch (Exception ex) {
                SimpleLog.Error(ex);
                importError = "Failed to import config. Invalid Config String";
                return null;
            }
        }

    }
}
