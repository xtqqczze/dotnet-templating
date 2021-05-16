// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
#if !NETFULL
using System.Runtime.Loader;
#endif
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Abstractions.Mount;

namespace Microsoft.TemplateEngine.Edge.Settings
{
    public class Scanner
    {
        private readonly IEngineEnvironmentSettings _environmentSettings;
        private readonly SettingsFilePaths _paths;

        public Scanner(IEngineEnvironmentSettings environmentSettings)
        {
            _environmentSettings = environmentSettings;
            _paths = new SettingsFilePaths(environmentSettings);
        }

        public ScanResult Scan(string sourceLocation)
        {
            if (string.IsNullOrWhiteSpace(sourceLocation))
            {
                throw new ArgumentException($"{nameof(sourceLocation)} should not be null or empty");
            }

            if (!_environmentSettings.SettingsLoader.TryGetMountPoint(sourceLocation, out var mountPoint))
            {
                throw new Exception($"Source location {sourceLocation} is not supported, or doesn't exist.");
            }

            using (mountPoint)
            {
                var (templates, localisations) = ScanMountPointForTemplatesAndLangpacks(mountPoint);
                var components = ScanForComponents(mountPoint);
                return new ScanResult(mountPoint.MountPointUri, templates, localisations, components);
            }
        }

        private IReadOnlyList<(Type InterfaceType, IIdentifiedComponent Instance)> ScanForComponents(IMountPoint mountPoint)
        {
            List<(Type InterfaceType, IIdentifiedComponent Instance)>? components = null;
            foreach (var asm in LoadAllFromPath(mountPoint))
            {
                try
                {
                    foreach (var type in asm.Assembly.GetTypes())
                    {
                        foreach (var (interfaceType, instance) in ScanType(type))
                        {
                            if (components == null)
                            {
                                components = new List<(Type InterfaceType, IIdentifiedComponent Instance)>();
                            }

                            components.Add((interfaceType, instance));
                        }
                    }
                }
                catch (Exception ex)
                {
                    _environmentSettings.Host.LogDiagnosticMessage(ex.ToString(), null);
                }
            }
            if (components != null)
            {
                return components;
            }
            return Array.Empty<(Type InterfaceType, IIdentifiedComponent Instance)>();
        }

        private IEnumerable<(Type InterfaceType, IIdentifiedComponent Instance)> ScanType(Type type)
        {
            if (!typeof(IIdentifiedComponent).GetTypeInfo().IsAssignableFrom(type) || type.GetTypeInfo().GetConstructor(Type.EmptyTypes) == null || !type.GetTypeInfo().IsClass)
            {
                yield break;
            }

            IReadOnlyList<Type> interfaceTypesToRegisterFor = type.GetTypeInfo().ImplementedInterfaces.Where(x => x != typeof(IIdentifiedComponent) && typeof(IIdentifiedComponent).GetTypeInfo().IsAssignableFrom(x)).ToList();
            if (interfaceTypesToRegisterFor.Count == 0)
            {
                yield break;
            }

            IIdentifiedComponent instance = (IIdentifiedComponent)Activator.CreateInstance(type);

            foreach (Type interfaceType in interfaceTypesToRegisterFor)
            {
                yield return (interfaceType, instance);
            }
        }

        private (List<ITemplate>, List<ILocalizationLocator>) ScanMountPointForTemplatesAndLangpacks(IMountPoint mountPoint)
        {
            var templates = new List<ITemplate>();
            var localizationLocators = new List<ILocalizationLocator>();

            foreach (IGenerator generator in _environmentSettings.SettingsLoader.Components.OfType<IGenerator>())
            {
                IList<ITemplate> templateList = generator.GetTemplatesAndLangpacksFromDir(mountPoint, out IList<ILocalizationLocator> localizationInfo);

                foreach (ILocalizationLocator locator in localizationInfo)
                {
                    localizationLocators.Add(locator);
                }

                foreach (ITemplate template in templateList)
                {
                    templates.Add(template);
                }
            }

            return (templates, localizationLocators);
        }

        private IEnumerable<(string FilePath, Assembly Assembly)> LoadAllFromPath(IMountPoint mountPoint)
        {
            foreach (var file in mountPoint.Root.EnumerateFiles("*.Components.dll", SearchOption.AllDirectories))
            {
                Assembly assembly;
                try
                {
#if !NETFULL
                    using (Stream fileStream = file.OpenRead())
                    {
                        assembly = AssemblyLoadContext.Default.LoadFromStream(fileStream);
                    }
#else
                    using (Stream fileStream = file.OpenRead())
                    using (MemoryStream ms = new MemoryStream())
                    {
                        fileStream.CopyTo(ms);
                        assembly = Assembly.Load(ms.ToArray());
                    }
#endif
                }
                catch (Exception ex)
                {
                    _environmentSettings.Host.LogMessage($"Failed to load {file}." + ex.ToString());
                    continue;
                }
                yield return (file.FullPath, assembly);
            }
        }

        private class MountPointScanSource
        {
            public MountPointScanSource(string location, IMountPoint mountPoint, bool shouldStayInOriginalLocation)
            {
                Location = location;
                MountPoint = mountPoint;
                ShouldStayInOriginalLocation = shouldStayInOriginalLocation;
            }

            public string Location { get; }

            public IMountPoint MountPoint { get; }

            public bool ShouldStayInOriginalLocation { get; }
        }
    }
}
