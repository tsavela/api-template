﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Api.Tests.Setup
{
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Parses embedded file-resource as stream.
        /// Don't forget to set the file BuildAction to EmbeddedResource!
        /// </summary>
        /// <param name="assembly">Assembly where resource is to be found</param>
        /// <param name="filePath">Path of file resource. Folder path must be specified with "." as separator.</param>
        public static Stream GetResourceAsStream(this Assembly assembly, string filePath)
        {
            var resourcePath = $"{assembly.GetName().Name}.{filePath}";
            var sr = assembly.GetManifestResourceStream(resourcePath);
            if (sr == null)
            {
                throw new ApplicationException($"Embedded resource '{filePath}' was not found");
            }
            return sr;
        }

        /// <summary>
        /// Parses embedded file-resource as text.
        /// Don't forget to set the file BuildAction to EmbeddedResource!
        /// </summary>
        /// <param name="assembly">Assembly where resource is to be found</param>
        /// <param name="filePath">Path of file resource. Folder path must be specified with "." as separator.</param>
        public static string GetResourceAsText(this Assembly assembly, string filePath)
        {
            using (var stream = assembly.GetResourceAsStream(filePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        /// <summary>
        /// Parses embedded file-resource as array of strings.
        /// Don't forget to set the file BuildAction to EmbeddedResource!
        /// </summary>
        /// <param name="assembly">Assembly where resource is to be found</param>
        /// <param name="filePath">Path of file resource. Folder path must be specified with "." as separator.</param>
        public static IEnumerable<string> GetResourceAsLines(this Assembly assembly, string filePath)
        {
            using (var stream = assembly.GetResourceAsStream(filePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        yield return line;
                    }
                }
            }
        }

        /// <summary>
        /// Parses embedded file-resource as text.
        /// Don't forget to set the file BuildAction to EmbeddedResource!
        /// </summary>
        /// <param name="assembly">Assembly where resource is to be found</param>
        /// <param name="filePath">Path of file resource. Folder path must be specified with "." as separator.</param>
        public static byte[] GetResourceAsBytes(this Assembly assembly, string filePath)
        {
            using (var stream = assembly.GetResourceAsStream(filePath))
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }
    }
}
