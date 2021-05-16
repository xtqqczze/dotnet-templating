// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System.Reflection;
using System.Runtime.Loader;

namespace Microsoft.TemplateEngine.Edge.Settings
{
    internal partial class ComponentManager
    {
        private class ComponentManagerAssemblyLoadContext : AssemblyLoadContext
        {
            public TestAssemblyLoadContext() : base(true)
            {
            }

            protected override Assembly? Load(AssemblyName name)
            {
                return null;
            }
        }
    }
}
