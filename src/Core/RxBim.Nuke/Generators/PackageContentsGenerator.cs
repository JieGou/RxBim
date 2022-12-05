﻿namespace RxBim.Nuke.Generators
{
    extern alias nc;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Extensions;
    using Models;
    using nc::Nuke.Common.ProjectModel;

    /// <summary>
    /// Generator of PackageContents.xml file.
    /// </summary>
    public abstract class PackageContentsGenerator
    {
        /// <summary>
        /// Generates PackageContents.xml file.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="outputDirectory">The output path.</param>
        /// <param name="allAssembliesTypes">Assemblies types data.</param>
        public void Generate(Project project, string outputDirectory, IEnumerable<AssemblyType> allAssembliesTypes)
        {
            var outputFilePath = Path.Combine(outputDirectory, "PackageContents.xml");
            var componentsList =
                GetComponents(project, allAssembliesTypes.Select(x => x.AssemblyName).Distinct()).ToList();
            project.ToApplicationPackage(componentsList).ToXElement().Save(outputFilePath);
        }

        /// <summary>
        /// Gets <see cref="Components"/> collection.
        /// </summary>
        /// <param name="project">The project.</param>
        /// <param name="assembliesNames">Assemblies names.</param>
        protected abstract IEnumerable<Components> GetComponents(Project project, IEnumerable<string> assembliesNames);
    }
}