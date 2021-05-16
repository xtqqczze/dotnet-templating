// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections.Generic;
using Microsoft.TemplateEngine.Abstractions;

namespace Microsoft.TemplateEngine.Edge.Settings
{
    public class ScanResult
    {
        public static readonly ScanResult Empty = new(
            string.Empty,
            Array.Empty<ITemplate>(),
            Array.Empty<ILocalizationLocator>(),
            Array.Empty<(Type InterfaceType, IIdentifiedComponent Instance)>()
            );

        public ScanResult(
            string mountPointUri,
            IReadOnlyList<ITemplate> templates,
            IReadOnlyList<ILocalizationLocator> localizations,
            IReadOnlyList<(Type InterfaceType, IIdentifiedComponent Instance)> components
            )
        {
            MountPointUri = mountPointUri;
            Templates = templates;
            Localizations = localizations;
            Components = components;
        }

        public string MountPointUri { get; }

        public IReadOnlyList<(Type InterfaceType, IIdentifiedComponent Instance)> Components { get; }

        public IReadOnlyList<ILocalizationLocator> Localizations { get; }

        public IReadOnlyList<ITemplate> Templates { get; }
    }
}
