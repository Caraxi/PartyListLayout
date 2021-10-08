using System.Diagnostics.CodeAnalysis;
using PartyListLayout.Config;
using PartyListLayout.Helper;
using System.Linq;
using System.Threading.Tasks;
using Dalamud.Game;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Game.Command;
using Dalamud.Game.Gui;
using Dalamud.IoC;
using Dalamud.Plugin;
using Lumina.Excel.GeneratedSheets;

namespace PartyListLayout {
    [SuppressMessage("ReSharper", "AutoPropertyCanBeMadeGetOnly.Local")]
    public class Plugin : IDalamudPlugin {
        public string Name => "Party List Layout";

        [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
        [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
        [PluginService] public static Framework Framework { get; private set; } = null!;
        [PluginService] public static GameGui GameGui { get; private set; } = null!;
        [PluginService] public static SigScanner SigScanner { get; private set; } = null!;
        [PluginService] public static KeyState KeyState { get; private set; } = null!;


        public PartyListLayout PartyListLayout;

        public PluginConfig Config;
        public ConfigWindow ConfigWindow;
        
        public static Plugin Instance { get; private set; }

        private bool isDisposed;


        public void Dispose() {
            isDisposed = true;
            PartyListLayout?.Dispose();
            foreach (var hook in Common.HookList.Where(hook => !hook.IsDisposed)) {
                if (hook.IsEnabled) hook.Disable();
                hook.Dispose();
            }

            CommandManager.RemoveHandler("/playout");
            ConfigWindow?.Hide();
        }

        public void Init() {

            Config = (PluginConfig) PluginInterface.GetPluginConfig() ?? new PluginConfig();
            ConfigWindow = new ConfigWindow(this);
            ConfigWindow.SetupLayoutFlags();
#if DEBUG
            SimpleLog.SetupBuildPath();
#endif
            PluginInterface.UiBuilder.OpenConfigUi += OnConfig;

            PartyListLayout = new PartyListLayout(this);
            PartyListLayout.Enable();

            CommandManager.AddHandler("/playout", new CommandInfo(OnConfigCommandHandler) {
                ShowInHelp = true,
                HelpMessage = $"Open or close the {Name} config window."
            });

#if DEBUG
            ConfigWindow.Show();
#endif
        }

        public Plugin() {
            PluginInterface.Create<PartyListLayout>();
            Instance = this;
            Task.Run(FFXIVClientStructs.Resolver.Initialize)
                .ContinueWith((_) => {
                    if (isDisposed) return;
                    Init();
            });
        }

        public void OnConfigCommandHandler(string command, string args) {
            OnConfig();
        }

        public void OnConfig() {
            ConfigWindow?.Toggle();
        }
    }
}
