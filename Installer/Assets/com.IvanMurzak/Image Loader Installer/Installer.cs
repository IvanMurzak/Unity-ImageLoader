/*
┌────────────────────────────────────────────────────────────────────────┐
│  Author: Ivan Murzak (https://github.com/IvanMurzak)                   │
│  Repository: GitHub (https://github.com/IvanMurzak/Unity-ImageLoader)  │
│  Copyright (c) 2025 Ivan Murzak                                        │
│  Licensed under the Apache License, Version 2.0.                       │
│  See the LICENSE file in the project root for more information.        │
└────────────────────────────────────────────────────────────────────────┘
*/
using UnityEditor;

namespace com.IvanMurzak.Unity.ImageLoader.Installer
{
    [InitializeOnLoad]
    public static partial class Installer
    {
        public const string PackageId = "extensions.unity.imageloader";
        public const string Version = "7.0.3";

        static Installer()
        {
#if !IVAN_MURZAK_INSTALLER_PROJECT
            AddScopedRegistryIfNeeded(ManifestPath);
#endif
        }
    }
}