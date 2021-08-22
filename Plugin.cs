using PartyListLayout.Config;
using PartyListLayout.Helper;
using System.Linq;
using System.Threading.Tasks;
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

        private bool isDisposed = false;

        public void Dispose() {
            isDisposed = true;
            PartyListLayout?.Dispose();
            foreach (var hook in Common.HookList.Where(hook => !hook.IsDisposed)) {
                if (hook.IsEnabled) hook.Disable();
                hook.Dispose();
            }

            PluginInterface.CommandManager.RemoveHandler("/playout");
            ConfigWindow?.Hide();
        }

        public void SecondInit() {

            Config = (PluginConfig) PluginInterface.GetPluginConfig() ?? new PluginConfig();
            ConfigWindow = new ConfigWindow(this);
            ConfigWindow.SetupLayoutFlags();
#if DEBUG
            SimpleLog.SetupBuildPath();
#endif
            PluginInterface.UiBuilder.OnOpenConfigUi += OnConfigCommandHandler;

            PartyListLayout = new PartyListLayout(this);
            PartyListLayout.Enable();

            PluginInterface.CommandManager.AddHandler("/playout", new CommandInfo(OnConfigCommandHandler) {
                ShowInHelp = true,
                HelpMessage = $"Open or close the {Name} config window."
            });

#if DEBUG
            ConfigWindow.Show();
#endif
        }

        public void Initialize(DalamudPluginInterface pluginInterface) {
            this.PluginInterface = pluginInterface;
            Instance = this;
            Task.Run(FFXIVClientStructs.Resolver.Initialize).ContinueWith((_) => {
                if (isDisposed) return;
                SecondInit();
            });
        }

        public void OnConfigCommandHandler(object command, object args) {
            ConfigWindow?.Toggle();
        }
    }
}
