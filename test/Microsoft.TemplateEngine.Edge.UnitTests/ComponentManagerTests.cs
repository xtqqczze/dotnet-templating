// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Linq;
using Microsoft.TemplateEngine.Abstractions.Installer;
using Microsoft.TemplateEngine.Edge.Settings;
using Microsoft.TemplateEngine.Mocks;
using Microsoft.TemplateEngine.TestHelper;
using Xunit;

namespace Microsoft.TemplateEngine.Edge.UnitTests
{
    public class ComponentManagerTests : IClassFixture<EnvironmentSettingsHelper>
    {
        private EnvironmentSettingsHelper _environmentSettingsHelper;

        public ComponentManagerTests(EnvironmentSettingsHelper environmentSettingsHelper)
        {
            _environmentSettingsHelper = environmentSettingsHelper;
        }

        [Fact]
        public void TestAllEdgeComponentsAdded()
        {
            var environmentSettings = _environmentSettingsHelper.CreateEnvironment(virtualize: true, loadDefaultGenerator: false);
            var componentManager = new ComponentManager(
                new MockSettingsLoader(environmentSettings));

            Assert.Equal(2, componentManager.OfType<IInstallerFactory>().Count());
        }
    }
}
