using System;
using System.Collections.Generic;

namespace Stompbox
{
    public class PluginFactory
    {
        Dictionary<string, IAudioPlugin> loadedPlugins = new Dictionary<string, IAudioPlugin>();

        StompboxClient StompboxClient;

        public PluginFactory(StompboxClient StompboxClient)
        {
            this.StompboxClient = StompboxClient;
        }

        public void ClearPlugins()
        {
            loadedPlugins.Clear();
        }

        public IAudioPlugin GetPlugin(string id)
        {
            if (!loadedPlugins.ContainsKey(id))
                return null;

            return loadedPlugins[id];
        }

        public IAudioPlugin GetPluginDefinition(string name)
        {
            if (loadedPlugins.ContainsKey(name))
                return loadedPlugins[name];

            return StompboxClient.CreatePlugin(name, name);
        }

        public IAudioPlugin CreateNewPlugin(string name)
        {
            return CreatePlugin(name, null);
        }

        public IAudioPlugin CreatePlugin(string id)
        {
            if (id == null)
                return null;

            string name = id;
            string[] idName = id.Split('_');

            if (idName.Length > 1)
            {
                name = idName[0];
            }

            return CreatePlugin(name, id);
        }

        public IAudioPlugin CreatePlugin(string name, string id)
        {
            IAudioPlugin newPlugin = null;

            if (id == null)
            {
                id = name;
                int number = 1;

                while (loadedPlugins.ContainsKey(id))
                {
                    number++;

                    id = name + "_" + number;
                }
            }
            else
            {
                if (loadedPlugins.ContainsKey(id))
                    return loadedPlugins[id];
            }

            newPlugin = StompboxClient.CreatePlugin(name, id);

            loadedPlugins[id] = newPlugin;

            if (newPlugin != null)
            {
                StompboxClient.Debug("New plugin: " + newPlugin.Name + "[" + newPlugin.ID + "]");
            }

            return newPlugin;
        }

        public void ReleasePlugin(IAudioPlugin plugin)
        {
            if (loadedPlugins.ContainsKey(plugin.ID))
            {
                loadedPlugins.Remove(plugin.ID);
            }
        }
    }
}
