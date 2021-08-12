using PartyListLayout.Config;
using PartyListLayout.Helper;
using System.Linq;
using Dalamud.Game.Command;
using Dalamud.Plugin;

namespace PartyListLayout {
    public class Plugin : IDalamudPlugin {
        public string Name => "Party List Layout";
        public DalamudPluginInterface PluginInterface { get; private set; }

        public PartyListLayout PartyListLayout;

        public PluginConfig Config;
        public ConfigWindow ConfigWindow;
        
        public static Plugin Instance { get; private set; }

        public void Dispose() {
            PartyListLayout?.Dispose();
            foreach (var hook in Common.HookList.Where(hook => !hook.IsDisposed)) {
                if (hook.IsEnabled) hook.Disable();
                hook.Dispose();
            }

            PluginInterface.CommandManager.RemoveHandler("/playout");
            ConfigWindow?.Hide();
        }

        public void Initialize(DalamudPluginInterface pluginInterface) { 
            FFXIVClientStructs.Resolver.Initialize();

            Config = (PluginConfig) pluginInterface.GetPluginConfig() ?? new PluginConfig();

            Instance = this;
#if DEBUG
            SimpleLog.SetupBuildPath();
#endif
            this.PluginInterface = pluginInterface;

            pluginInterface.UiBuilder.OnOpenConfigUi += OnConfigCommandHandler;

            PartyListLayout = new PartyListLayout(this);
            PartyListLayout.Enable();

            pluginInterface.CommandManager.AddHandler("/playout", new CommandInfo(OnConfigCommandHandler) {
                ShowInHelp = true,
                HelpMessage = $"Open or close the {Name} config window."
            });

            ConfigWindow = new ConfigWindow(this);
            #if DEBUG
            ConfigWindow.Show();
            #endif
        }

        public void OnConfigCommandHandler(object command, object args) {
            ConfigWindow?.Toggle();
        }
    }
}
