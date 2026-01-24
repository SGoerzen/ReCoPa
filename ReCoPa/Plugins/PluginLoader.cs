using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using ReCoPa.Plugins;

namespace ReCoPa.PluginHost;

public class PluginLoader
{
    private readonly string _pluginDir;

    public PluginLoader(string pluginDir)
    {
        _pluginDir = pluginDir;
        Directory.CreateDirectory(_pluginDir);
    }

    public IReadOnlyList<IPluginPackage> LoadPlugins()
    {
        var result = new List<IPluginPackage>()
        {
            new CorePluginPackage()
        };

        foreach (var dll in Directory.GetFiles(_pluginDir, "*.dll"))
        {
            try
            {
                var asm = Assembly.LoadFrom(dll);

                var plugins = asm.GetTypes()
                    .Where(t =>
                        typeof(IPluginPackage).IsAssignableFrom(t) &&
                        !t.IsAbstract &&
                        t.GetConstructor(Type.EmptyTypes) != null)
                    .Select(t => (IPluginPackage)Activator.CreateInstance(t)!);

                result.AddRange(plugins);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Plugin load failed: {dll}\n{ex}");
            }
        }

        return result;
    }
}