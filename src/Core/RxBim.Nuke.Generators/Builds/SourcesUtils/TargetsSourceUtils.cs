﻿namespace RxBim.Nuke.Generators
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using static Constants;

    /// <summary>
    /// Utilities for targets source.
    /// </summary>
    internal static class TargetsSourceUtils
    {
        /// <summary>
        /// Returns targets for versions source.
        /// </summary>
        /// <param name="versionNumbers">Version numbers collection.</param>
        /// <param name="buildDeclaredTargets">Target names collection.</param>
        public static string GetVersionBuildTargetsSource(
            IEnumerable<string> versionNumbers,
            IEnumerable<string> buildDeclaredTargets)
        {
            var values = string.Join($"{Environment.NewLine}{Environment.NewLine}",
                versionNumbers.Select(x =>
                    $"""
                            Target {SetVersion}{x} => _ => _
                                .Before<IPublish>(x => x.Publish, x => x.Prerelease, x => x.Release, x => x.List)
                                .Before<IPack>(x => x.Pack)
                                .Before<ICompile>(x => x.Compile)
                                .Before<IRestore>(x => x.Restore)
                                .Before({string.Join(", ", buildDeclaredTargets)})
                                .Executes(() => this.SetVersion("{x}"));
                        """));

            return $$"""
                // {{AutoGeneratedFileHeader}}
                #pragma warning disable CS1591, SA1205, SA1600
                
                using Bimlab.Nuke.Components;
                using Nuke.Common;
                using {{VersionsNamespace}};
                
                partial class Build
                {
                {{values}}
                }
                """;
        }
    }
}