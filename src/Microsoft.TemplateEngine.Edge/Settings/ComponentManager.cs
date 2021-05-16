// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using Microsoft.TemplateEngine.Abstractions;

namespace Microsoft.TemplateEngine.Edge.Settings
{
    internal partial class ComponentManager : IComponentManager, IDisposable
    {
        private readonly Dictionary<Guid, string> _componentIdToAssemblyQualifiedTypeName = new();
        private readonly ConcurrentDictionary<Type, HashSet<Guid>> _componentIdsByType = new();
        private readonly SettingsFilePaths _paths;

        public ComponentManager(ISettingsLoader settingsLoader)
        {
            var environmentSettings = settingsLoader.EnvironmentSettings;
            _paths = new SettingsFilePaths(environmentSettings);

            foreach (var component in environmentSettings.Host.BuiltInComponents)
            {
                AddComponent(component.Type, component.Instance);
            }

            var settings = LoadSettingsStore(environmentSettings);

            if (settings != null)
            {
                foreach (string mountPointUri in settings.ProbingMountPoints)
                {
                    ReflectionLoadProbingPath.Add(settingsLoader, mountPointUri);
                }

                foreach (KeyValuePair<string, HashSet<Guid>> bucket in settings.ComponentTypeToGuidList)
                {
                    Type interfaceType = Type.GetType(bucket.Key);
                    if (interfaceType != null)
                    {
                        foreach (var guid in bucket.Value)
                        {
                            _componentIdsByType[interfaceType].Add(guid);
                        }
                    }
                }

                foreach (KeyValuePair<string, string> entry in settings.ComponentGuidToAssemblyQualifiedName)
                {
                    if (Guid.TryParse(entry.Key, out Guid componentId))
                    {
                        _componentIdToAssemblyQualifiedTypeName[componentId] = entry.Value;
                    }
                }
            }
        }

        internal ConcurrentDictionary<Type, ConcurrentDictionary<Guid, object>> ComponentCache { get; } = new();

        public void AddComponent(Type type, IIdentifiedComponent component)
        {
            if (!type.IsAssignableFrom(component.GetType()))
            {
                throw new ArgumentException($"{component.GetType().Name} should be assignable from {type.Name} type", nameof(type));
            }

            var typeCache = ComponentCache.GetOrAdd(type, (t) => new ConcurrentDictionary<Guid, object>());

            Guid id = component.Id;
            typeCache[id] = component;
            var ids = _componentIdsByType.GetOrAdd(type, (type) => new HashSet<Guid>());
            ids.Add(id);
        }

        public void RemoveComponent(Type interfaceType, IIdentifiedComponent instance)
        {
            ComponentCache[interfaceType].TryRemove(instance.Id, out _);
        }

        public IEnumerable<T> OfType<T>()
            where T : class, IIdentifiedComponent
        {
            if (!_componentIdsByType.TryGetValue(typeof(T), out HashSet<Guid> ids))
            {
                yield break;
            }

            foreach (Guid id in ids)
            {
                if (TryGetComponent(id, out T? component))
                {
                    yield return component!;
                }
            }
        }

        public bool TryGetComponent<T>(Guid id, out T? component)
            where T : class, IIdentifiedComponent
        {
            component = default;
            if (ComponentCache.TryGetValue(typeof(T), out var typeCache)
                && typeCache.TryGetValue(id, out object resolvedComponent)
                && resolvedComponent is T t)
            {
                component = t;
                return true;
            }

            if (_componentIdToAssemblyQualifiedTypeName.TryGetValue(id, out string assemblyQualifiedName))
            {
                Type type = TypeEx.GetType(assemblyQualifiedName);
                component = Activator.CreateInstance(type) as T;

                if (component != null)
                {
                    AddComponent(typeof(T), component);
                    return true;
                }
            }
            return false;
        }

        public void Dispose() => throw new NotImplementedException();
    }
}
