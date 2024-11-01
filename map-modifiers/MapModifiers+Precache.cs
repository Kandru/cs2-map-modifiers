using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;

namespace MapModifiersPlugin
{
    public partial class MapModifiersPlugin : BasePlugin, IPluginConfig<PluginConfig>
    {
        private readonly List<string> _precacheModels = new List<string>
        {
            "models/props/cs_office/file_cabinet1.vmdl"
        };

        private void OnServerPrecacheResources(ResourceManifest manifest) {
            foreach (var model in _precacheModels)
            {
                Console.WriteLine($"[MapModifiers] Precaching {model}");
                manifest.AddResource(model);
            }
        }
    }
}
