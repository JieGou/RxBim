﻿namespace RxBim.Nuke.Helpers
{
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection.Metadata;
    using System.Reflection.PortableExecutable;
    using Models;

    /// <summary>
    /// An assembly scanner utilities.
    /// </summary>
    public static class AssemblyScanner
    {
        /// <summary>
        /// Scans an assembly.
        /// </summary>
        /// <param name="file">The assembly file path.</param>
        public static IEnumerable<AssemblyType> Scan(string file)
        {
            if (File.Exists(file))
            {
                using var fs = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var peReader = new PEReader(fs);
                var mr = peReader.GetMetadataReader();

                foreach (var typeDefinitionHandle in mr.TypeDefinitions)
                {
                    var typeDefinition = mr.GetTypeDefinition(typeDefinitionHandle);
                    var baseTypeName = GetBaseTypeName(mr, typeDefinition);
                    var fullName = GetFullName(mr, typeDefinition);
                    if (string.IsNullOrEmpty(fullName))
                    {
                        continue;
                    }

                    yield return new AssemblyType(Path.GetFileNameWithoutExtension(file), fullName, baseTypeName);
                }
            }
        }

        private static string? GetBaseTypeName(MetadataReader mr, TypeDefinition typeDefinition)
        {
            var baseTypeDefinition = typeDefinition.BaseType;
            string? baseTypeName = null;
            if (!baseTypeDefinition.IsNil)
            {
                try
                {
                    var referenceHandle = (TypeReferenceHandle)baseTypeDefinition;
                    var typeReference = mr.GetTypeReference(referenceHandle);
                    baseTypeName = mr.GetString(typeReference.Name);
                }
                catch
                {
                    // ignore
                }
            }

            return baseTypeName;
        }

        private static string? GetFullName(MetadataReader mr, TypeDefinition typeDefinition)
        {
            try
            {
                var ns = mr.GetString(typeDefinition.Namespace);
                var name = mr.GetString(typeDefinition.Name);
                return $"{ns}.{name}";
            }
            catch
            {
                // ignore
            }

            return null;
        }
    }
}