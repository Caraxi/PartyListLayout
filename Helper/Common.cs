using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Dalamud.Hooking;
using FFXIVClientStructs.Attributes;
using FFXIVClientStructs.FFXIV.Component.GUI;

namespace PartyListLayout.Helper {
    internal static unsafe class Common {

        public static AtkUnitBase* GetUnitBase(string name, int index = 1) {
            return AtkStage.GetSingleton()->RaptureAtkUnitManager->GetAddonByName(name);
        }

        public static T* GetUnitBase<T>(string name = null, int index = 1) where T : unmanaged {
            if (string.IsNullOrEmpty(name)) {
                var attr = (Addon) typeof(T).GetCustomAttribute(typeof(Addon));
                if (attr != null) {
                    name = attr.AddonIdentifiers.FirstOrDefault();
                }
            }

            if (string.IsNullOrEmpty(name)) return null;
            
            return (T*) Plugin.Instance.PluginInterface.Framework.Gui.GetUiObjectByName(name, index);
        }

        public static HookWrapper<T> Hook<T>(string signature, T detour, bool enable = true, int addressOffset = 0) where T : Delegate {
            var addr = Plugin.Instance.PluginInterface.TargetModuleScanner.ScanText(signature);
            var h = new Hook<T>(addr + addressOffset, detour);
            var wh = new HookWrapper<T>(h);
            if (enable) wh.Enable();
            HookList.Add(wh);
            return wh;
        }

        public static List<IHookWrapper> HookList = new();

        public static void OpenBrowser(string url) {
            Process.Start(new ProcessStartInfo {FileName = url, UseShellExecute = true});
        }

    }
}
