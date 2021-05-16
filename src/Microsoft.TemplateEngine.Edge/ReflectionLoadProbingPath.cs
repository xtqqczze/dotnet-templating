// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable enable

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.TemplateEngine.Abstractions;
using Microsoft.TemplateEngine.Abstractions.Mount;

#if !NETFULL
using System.Runtime.Loader;
#endif

namespace Microsoft.TemplateEngine.Edge
{
    public class ReflectionLoadProbingPath
    {
        private readonly ConcurrentDictionary<string, Assembly?> _loadedAssemblies = new();
        private ISettingsLoader _settingsLoader;
        private string _mountPointUri;

        private ReflectionLoadProbingPath(ISettingsLoader settingsLoader, string mountPointUri)
        {
            _settingsLoader = settingsLoader;
            _mountPointUri = mountPointUri;
        }

#if !NETFULL
        private Assembly? SelectBestMatch(AssemblyLoadContext loadContext, AssemblyName match, IEnumerable<IFile> candidates)
#else
        private static Assembly? SelectBestMatch(object sender, AssemblyName match, IEnumerable<IFile> candidates)
#endif
        {
            return _loadedAssemblies.GetOrAdd(match.ToString(), n =>
            {
                Stack<string> bestMatch = new Stack<string>();
                byte[] pk = match.GetPublicKey();
                bool cultureMatch = false;
                bool majorVersionMatch = false;
                bool minorVersionMatch = false;
                bool buildMatch = false;
                bool revisionMatch = false;

                foreach (IFile file in candidates)
                {
                    if (!file.Exists)
                    {
                        continue;
                    }

#if !NETFULL
                    AssemblyName candidateName = AssemblyLoadContext.GetAssemblyName(file.FullPath);
#else
                    AssemblyName candidateName = AssemblyName.GetAssemblyName(file.FullPath);
#endif

                    //Only pursue things that may have the same identity
                    if (!string.Equals(candidateName.Name, match.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    //If the required match has a strong name, the public key token must match
                    if (pk != null && !pk.SequenceEqual(candidateName.GetPublicKey() ?? Enumerable.Empty<byte>()))
                    {
                        continue;
                    }

                    if (match.Version != null)
                    {
                        //Don't go backwards
                        if (candidateName.Version.Major < match.Version.Major)
                        {
                            continue;
                        }

                        if (candidateName.Version.Major == match.Version.Major)
                        {
                            //Don't go backwards
                            if (candidateName.Version.Minor < match.Version.Minor)
                            {
                                continue;
                            }

                            if (candidateName.Version.Minor == match.Version.Minor)
                            {
                                //Don't go backwards
                                if (candidateName.Version.Build < match.Version.Build)
                                {
                                    continue;
                                }

                                if (candidateName.Version.Build == match.Version.Build)
                                {
                                    //Don't go backwards
                                    if (candidateName.Version.Revision < match.Version.Revision)
                                    {
                                        continue;
                                    }

                                    if (candidateName.Version.Revision != match.Version.Revision)
                                    {
                                        if (revisionMatch)
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        revisionMatch = true;
                                    }

                                    majorVersionMatch = true;
                                    minorVersionMatch = true;
                                    buildMatch = true;
                                }
                                else
                                {
                                    if (buildMatch)
                                    {
                                        continue;
                                    }

                                    majorVersionMatch = true;
                                    minorVersionMatch = true;
                                }
                            }
                            else
                            {
                                if (minorVersionMatch)
                                {
                                    continue;
                                }

                                majorVersionMatch = true;
                            }
                        }
                        else
                        {
                            if (majorVersionMatch)
                            {
                                continue;
                            }
                        }
                    }

                    if (string.Equals(candidateName.CultureName, match.CultureName, StringComparison.OrdinalIgnoreCase))
                    {
                        cultureMatch = true;
                    }
                    else if (cultureMatch)
                    {
                        continue;
                    }

                    bestMatch.Push(file.FullPath);
                }

                while (bestMatch.Count > 0)
                {
                    try
                    {
                        string attempt = bestMatch.Pop();
#if !NETFULL
                        Assembly result = loadContext.LoadFromAssemblyPath(attempt);
#else
                        Assembly result = Assembly.LoadFile(attempt);
#endif
                        return result;
                    }
                    catch
                    {
                    }
                }

                return null;
            });
        }

#if !NETFULL

        private static Assembly? Resolving(AssemblyLoadContext assemblyLoadContext, AssemblyName assemblyName)
#else
        private static Assembly? Resolving(object sender, ResolveEventArgs resolveEventArgs)
#endif
        {
#if !NETFULL
            string stringName = assemblyName.Name;
#else
            string stringName = resolveEventArgs.Name;
            AssemblyName assemblyName = new AssemblyName(stringName);
#endif

            foreach (ReflectionLoadProbingPath selector in Instance)
            {
                var info = selector.GetRootDir();
                Assembly? found = null;

                if (info?.Exists ?? false)
                {
                    IEnumerable<IFile> files = info.EnumerateFiles($"{stringName}.dll", SearchOption.AllDirectories)
                        .Where(x => x.FullPath.IndexOf($"{Path.DirectorySeparatorChar}lib{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase) > -1
                        && (x.FullPath.IndexOf($"{Path.DirectorySeparatorChar}netstandard", StringComparison.OrdinalIgnoreCase) > -1
                        || x.FullPath.IndexOf($"{Path.DirectorySeparatorChar}netcoreapp", StringComparison.OrdinalIgnoreCase) > -1))
                        .OrderByDescending(x => x.FullPath);
#if !NETFULL
                    found = SelectBestMatch(assemblyLoadContext, assemblyName, files);
#else
                    found = SelectBestMatch(sender, assemblyName, files);
#endif
                }

                if (found != null)
                {
                    foreach (AssemblyName reference in found.GetReferencedAssemblies())
                    {
#if !NETFULL
                        Resolving(assemblyLoadContext, reference);
#else
                        ResolveEventArgs referenceArgs = new ResolveEventArgs(reference.FullName, found);
                        Resolving(sender, referenceArgs);
#endif
                    }

                    return found;
                }
            }

            return null;
        }
    }
}
