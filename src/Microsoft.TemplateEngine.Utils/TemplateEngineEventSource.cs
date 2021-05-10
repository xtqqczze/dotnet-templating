// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics.Tracing;

namespace Microsoft.TemplateEngine.Utils
{
    /// <summary>
    /// This captures information of how various key methods of TemplateEngine ran.
    /// </summary>
    [EventSource(Name = "Microsoft-TemplateEngine")]
    public sealed class TemplateEngineEventSource : EventSource
    {
        private TemplateEngineEventSource() { }

        /// <summary>
        /// Define the singleton instance of the event source.
        /// </summary>
        public static TemplateEngineEventSource Log { get; } = new TemplateEngineEventSource();

        #region CLI Events

        [Event(1, Keywords = Keywords.All)]
        public void New3Command_RunStart()
        {
            WriteEvent(1);
        }

        [Event(2, Keywords = Keywords.All)]
        public void New3Command_RunStop()
        {
            WriteEvent(2);
        }

        [Event(3, Keywords = Keywords.All)]
        public void New3Command_ExecuteStart()
        {
            WriteEvent(3);
        }

        [Event(4, Keywords = Keywords.All)]
        public void New3Command_ExecuteStop()
        {
            WriteEvent(4);
        }

        [Event(5, Keywords = Keywords.All)]
        public void ParseArgsStart()
        {
            WriteEvent(5);
        }

        [Event(6, Keywords = Keywords.All)]
        public void ParseArgsStop()
        {
            WriteEvent(6);
        }

        [Event(7, Keywords = Keywords.All)]
        public void InvokeTemplateAndCheckForUpdateStart(string identity)
        {
            WriteEvent(7, identity);
        }

        [Event(8, Keywords = Keywords.All)]
        public void InvokeTemplateAndCheckForUpdateStop()
        {
            WriteEvent(8);
        }

        [Event(9, Keywords = Keywords.All)]
        public void PostActionStart(string description)
        {
            WriteEvent(9, description);
        }

        [Event(10, Keywords = Keywords.All)]
        public void PostActionStop(bool success)
        {
            WriteEvent(10, success);
        }

        [Event(11, Keywords = Keywords.All)]
        public void TemplateResolver_AddParameterMatchingToTemplatesStart()
        {
            WriteEvent(11);
        }

        [Event(12, Keywords = Keywords.All)]
        public void TemplateResolver_AddParameterMatchingToTemplatesStop()
        {
            WriteEvent(12);
        }

        [Event(13, Keywords = Keywords.All)]
        public void TemplateResolver_GetTemplateResolutionResultForListOrHelpStart(int count)
        {
            WriteEvent(13, count);
        }

        [Event(14, Keywords = Keywords.All)]
        public void TemplateResolver_GetTemplateResolutionResultForListOrHelpStop()
        {
            WriteEvent(14);
        }

        [Event(15, Keywords = Keywords.All)]
        public void TemplateResolver_PerformCoreTemplateQueryStart(int count)
        {
            WriteEvent(15, count);
        }

        [Event(16, Keywords = Keywords.All)]
        public void TemplateResolver_PerformCoreTemplateQueryStop()
        {
            WriteEvent(16);
        }

        [Event(17, Keywords = Keywords.All)]
        public void TemplateResolver_IsTemplateHiddenByHostFileStart(string identity)
        {
            WriteEvent(17, identity);
        }

        [Event(18, Keywords = Keywords.All)]
        public void TemplateResolver_IsTemplateHiddenByHostFileStop(bool hidden)
        {
            WriteEvent(18, hidden ? 1 : 0);
        }

        #endregion

        #region EdgeEvents

        [Event(101, Keywords = Keywords.All)]
        public void TemplateCreator_InstantiateStart()
        {
            WriteEvent(101);
        }

        [Event(102, Keywords = Keywords.All)]
        public void TemplateCreator_InstantiateStop()
        {
            WriteEvent(102);
        }

        [Event(103, Keywords = Keywords.All)]
        public void SettingsLoader_LoadTemplateStart(string identity)
        {
            WriteEvent(103, identity);
        }

        [Event(104, Keywords = Keywords.All)]
        public void SettingsLoader_LoadTemplateStop(bool success)
        {
            WriteEvent(104, success);
        }

        [Event(105, Keywords = Keywords.All)]
        public void SettingsLoader_EnsureLoadedStart()
        {
            WriteEvent(105);
        }

        [Event(106, Keywords = Keywords.All)]
        public void SettingsLoader_EnsureLoadedStop()
        {
            WriteEvent(106);
        }

        [Event(107, Keywords = Keywords.All)]
        public void GlobalSettingsProvider_GetPackagesStart()
        {
            WriteEvent(107);
        }

        [Event(108, Keywords = Keywords.All)]
        public void GlobalSettingsProvider_GetPackagesStop()
        {
            WriteEvent(108);
        }

        [Event(109, Keywords = Keywords.All)]
        public void Scanner_ScanStart(string path)
        {
            WriteEvent(109, path);
        }

        [Event(110, Keywords = Keywords.All)]
        public void Scanner_ScanStop()
        {
            WriteEvent(110);
        }

        [Event(111, Keywords = Keywords.All)]
        public void SettingsLoader_RebuildCacheStart(bool force)
        {
            WriteEvent(111, force ? 1 : 0);
        }

        [Event(112, Keywords = Keywords.All)]
        public void SettingsLoader_RebuildCacheStop(bool rebuilt)
        {
            WriteEvent(112, rebuilt);
        }

        [Event(113, Keywords = Keywords.All)]
        public void SettingsLoader_TemplateCacheParsingStart()
        {
            WriteEvent(113);
        }

        [Event(114, Keywords = Keywords.All)]
        public void SettingsLoader_TemplateCacheParsingStop()
        {
            WriteEvent(114);
        }

        [Event(116, Keywords = Keywords.All)]
        public void NugetApiManager_GetPackageMetadataAsyncStart(string source)
        {
            WriteEvent(116, source);
        }

        [Event(117, Keywords = Keywords.All)]
        public void NugetApiManager_GetPackageMetadataAsyncStop(int count)
        {
            WriteEvent(117, count);
        }

        [Event(118, Keywords = Keywords.All)]
        public void SettingsLoader_ParseSettingsStart()
        {
            WriteEvent(118);
        }

        [Event(119, Keywords = Keywords.All)]
        public void SettingsLoader_ParseSettingsStop()
        {
            WriteEvent(119);
        }

        [Event(120, Keywords = Keywords.All)]
        public void SettingsLoader_FirstRunStart()
        {
            WriteEvent(120);
        }

        [Event(121, Keywords = Keywords.All)]
        public void SettingsLoader_FirstRunStop()
        {
            WriteEvent(121);
        }

        [Event(122, Keywords = Keywords.All)]
        public void ComponentManager_ConstructorStart()
        {
            WriteEvent(122);
        }

        [Event(123, Keywords = Keywords.All)]
        public void ComponentManager_ConstructorStop()
        {
            WriteEvent(123);
        }

        [Event(124, Keywords = Keywords.All)]
        public void AssemblyComponentCatalogStart()
        {
            WriteEvent(124);
        }

        [Event(125, Keywords = Keywords.All)]
        public void AssemblyComponentCatalogStop()
        {
            WriteEvent(125);
        }

        [Event(126, Keywords = Keywords.All)]
        public void ComponentManager_RegisterManyStart()
        {
            WriteEvent(126);
        }

        [Event(127, Keywords = Keywords.All)]
        public void ComponentManager_RegisterManyStop()
        {
            WriteEvent(127);
        }

        #endregion

        public static class Keywords
        {
            public const EventKeywords All = (EventKeywords)0x1;
            public const EventKeywords PerformanceLog = (EventKeywords)0x2;
        }

    }
}
