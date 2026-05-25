/*
┌────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                   │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-ImageLoader)  │
│  Copyright (c) 2025 Ivan Murzak                                        │
│  Licensed under the Apache License, Version 2.0.                       │
│  See the LICENSE file in the project root for more information.        │
└────────────────────────────────────────────────────────────────────────┘
*/

using System.IO;
using System.Linq;
using com.IvanMurzak.Unity.ImageLoader.Installer.SimpleJSON;
using UnityEngine;

namespace com.IvanMurzak.Unity.ImageLoader.Installer
{
    public static partial class Installer
    {
        static string ManifestPath => Path.Combine(Application.dataPath, "../Packages/manifest.json");

        // Property names
        public const string Dependencies = "dependencies";
        public const string ScopedRegistries = "scopedRegistries";
        public const string Name = "name";
        public const string Url = "url";
        public const string Scopes = "scopes";

        // Property values
        public const string RegistryName = "package.openupm.com";
        public const string RegistryUrl = "https://package.openupm.com";
        public static readonly string[] PackageIds = new string[] {
            "com.ivanmurzak",            // Ivan Murzak's OpenUPM packages
            "extensions.unity",          // Ivan Murzak's OpenUPM packages (older)
            "org.nuget.com.ivanmurzak",  // Ivan Murzak's NuGet packages
            "org.nuget.microsoft",       // Microsoft NuGet packages
            "org.nuget.system",          // Microsoft NuGet packages
            "org.nuget.r3",              // R3 package NuGet package
            "com.cysharp.unitask",       // Cysharp UniTask package
        };

        public static void AddScopedRegistryIfNeeded(string manifestPath, int indent = 2)
        {
            if (!File.Exists(manifestPath))
            {
                Debug.LogError($"{manifestPath} not found!");
                return;
            }
            var jsonText = File.ReadAllText(manifestPath)
                .Replace("{ }", "{\n}")
                .Replace("{}", "{\n}")
                .Replace("[ ]", "[\n]")
                .Replace("[]", "[\n]");

            var manifestJson = JSONObject.Parse(jsonText);
            if (manifestJson == null)
            {
                Debug.LogError($"Failed to parse {manifestPath} as JSON.");
                return;
            }

            var modified = false;

            // --- Add scoped registries if needed
            var scopedRegistries = manifestJson[ScopedRegistries];
            if (scopedRegistries == null)
            {
                manifestJson[ScopedRegistries] = new JSONArray();
                modified = true;
            }

            // --- Add OpenUPM registry if needed
            var openUpmRegistry = scopedRegistries.Linq
                .Select(kvp => kvp.Value)
                .Where(r => r.Linq
                    .Any(p => p.Key == Name && p.Value == RegistryName))
                .FirstOrDefault();

            if (openUpmRegistry == null)
            {
                scopedRegistries.Add(openUpmRegistry = new JSONObject
                {
                    [Name] = RegistryName,
                    [Url] = RegistryUrl,
                    [Scopes] = new JSONArray()
                });
                modified = true;
            }

            // --- Add missing scopes
            var scopes = openUpmRegistry[Scopes];
            if (scopes == null)
            {
                openUpmRegistry[Scopes] = scopes = new JSONArray();
                modified = true;
            }
            foreach (var packageId in PackageIds)
            {
                var existingScope = scopes.Linq
                    .Select(kvp => kvp.Value)
                    .Where(value => value == packageId)
                    .FirstOrDefault();
                if (existingScope == null)
                {
                    scopes.Add(packageId);
                    modified = true;
                }
            }

            // --- Package Dependency
            var dependencies = manifestJson[Dependencies];
            if (dependencies == null)
            {
                manifestJson[Dependencies] = dependencies = new JSONObject();
                modified = true;
            }
            if (dependencies[PackageId] != Version)
            {
                dependencies[PackageId] = Version;
                modified = true;
            }

            // --- Write changes back to manifest
            if (modified)
                File.WriteAllText(manifestPath, manifestJson.ToString(indent).Replace("\" : ", "\": "));
        }
    }
}