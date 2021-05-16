// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Utils;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.TemplateEngine.Edge.Settings
{
    internal partial class ComponentManager
    {
        private const int MaxLoadAttempts = 20;

        internal void UpdateUserComponents(ScanResult[] scanResults)
        {
            bool anyComponentFound = false;
            var settings = new SettingsStore();
            foreach (var scanResult in scanResults)
            {
                foreach (var component in scanResult.Components)
                {
                    anyComponentFound = true;
                    settings.ProbingMountPoints.Add(scanResult.MountPointUri);
                    settings.ComponentTypeToGuidList[component.InterfaceType.AssemblyQualifiedName].Add(component.Instance.Id);
                    if (!settings.ComponentTypeToGuidList.TryGetValue(component.InterfaceType.AssemblyQualifiedName, out HashSet<Guid> idsForInterfaceTypeForSettings))
                    {
                        settings.ComponentTypeToGuidList[component.InterfaceType.AssemblyQualifiedName] = idsForInterfaceTypeForSettings = new HashSet<Guid>();
                    }
                    idsForInterfaceTypeForSettings.Add(component.Instance.Id);
                }
            }
            if (anyComponentFound)
            {
                JObject serialized = JObject.FromObject(settings);
                _paths.WriteAllText(_paths.ComponentsFile, serialized.ToString());
            }
            else
            {
                _paths.Delete(_paths.ComponentsFile);
            }
        }

        private SettingsStore? LoadSettingsStore(IEngineEnvironmentSettings environmentSettings)
        {
            if (!_paths.Exists(_paths.ComponentsFile))
            {
                return null;
            }

            string? userSettings = null;
            using (Timing.Over(environmentSettings.Host, "Read settings"))
            {
                for (int i = 0; i < MaxLoadAttempts; ++i)
                {
                    try
                    {
                        userSettings = environmentSettings.Host.FileSystem.ReadAllText(_paths.ComponentsFile);
                        break;
                    }
                    catch (IOException)
                    {
                        if (i == MaxLoadAttempts - 1)
                        {
                            throw;
                        }

                        Task.Delay(2).Wait();
                    }
                }
            }

            JObject parsed;
            using (Timing.Over(environmentSettings.Host, "Parse settings"))
            {
                try
                {
                    parsed = JObject.Parse(userSettings);
                }
                catch (Exception ex)
                {
                    throw new EngineInitializationException("Error parsing the user settings file", "Settings File", ex);
                }
            }

            using (Timing.Over(environmentSettings.Host, "Deserialize user settings"))
            {
                return new SettingsStore(parsed);
            }
        }

        internal class SettingsStore
        {
            public SettingsStore()
            {
            }

            public SettingsStore(JObject obj)
            {
                JToken componentGuidToAssemblyQualifiedNameToken;
                if (obj.TryGetValue(nameof(ComponentGuidToAssemblyQualifiedName), StringComparison.OrdinalIgnoreCase, out componentGuidToAssemblyQualifiedNameToken))
                {
                    JObject componentGuidToAssemblyQualifiedNameObject = componentGuidToAssemblyQualifiedNameToken as JObject;
                    if (componentGuidToAssemblyQualifiedNameObject != null)
                    {
                        foreach (JProperty entry in componentGuidToAssemblyQualifiedNameObject.Properties())
                        {
                            if (entry.Value != null && entry.Value.Type == JTokenType.String)
                            {
                                ComponentGuidToAssemblyQualifiedName[entry.Name] = entry.Value.ToString();
                            }
                        }
                    }
                }

                JToken probingPathsToken;
                if (obj.TryGetValue(nameof(ProbingMountPoints), StringComparison.OrdinalIgnoreCase, out probingPathsToken))
                {
                    JArray probingPathsArray = probingPathsToken as JArray;
                    if (probingPathsArray != null)
                    {
                        foreach (JToken path in probingPathsArray)
                        {
                            if (path != null && path.Type == JTokenType.String)
                            {
                                ProbingMountPoints.Add(path.ToString());
                            }
                        }
                    }
                }

                JToken componentTypeToGuidListToken;
                if (obj.TryGetValue(nameof(ComponentTypeToGuidList), StringComparison.OrdinalIgnoreCase, out componentTypeToGuidListToken))
                {
                    JObject componentTypeToGuidListObject = componentTypeToGuidListToken as JObject;
                    if (componentTypeToGuidListObject != null)
                    {
                        foreach (JProperty entry in componentTypeToGuidListObject.Properties())
                        {
                            JArray values = entry.Value as JArray;

                            if (values != null)
                            {
                                HashSet<Guid> set = new HashSet<Guid>();
                                ComponentTypeToGuidList[entry.Name] = set;

                                foreach (JToken value in values)
                                {
                                    if (value != null && value.Type == JTokenType.String)
                                    {
                                        Guid id;
                                        if (Guid.TryParse(value.ToString(), out id))
                                        {
                                            set.Add(id);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            [JsonProperty]
            public Dictionary<string, string> ComponentGuidToAssemblyQualifiedName { get; } = new();

            [JsonProperty]
            public HashSet<string> ProbingMountPoints { get; } = new();

            [JsonProperty]
            public Dictionary<string, HashSet<Guid>> ComponentTypeToGuidList { get; } = new();
        }
    }
}
