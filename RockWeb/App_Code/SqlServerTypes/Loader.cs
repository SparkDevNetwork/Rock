﻿// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SqlServerTypes
{
    /// <summary>
    /// Utility methods related to CLR Types for SQL Server 
    /// </summary>
    internal class Utilities
    {
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr LoadLibrary(string libname);

        /// <summary>
        /// Loads the required native assemblies for the current architecture (x86 or x64)
        /// </summary>
        /// <param name="rootApplicationPath">
        /// Root path of the current application. Use Server.MapPath(".") for ASP.NET applications
        /// and AppDomain.CurrentDomain.BaseDirectory for desktop applications.
        /// </param>
        public static void LoadNativeAssemblies(string rootApplicationPath)
        {
            var isArm = RuntimeInformation.ProcessArchitecture == Architecture.Arm
                || RuntimeInformation.ProcessArchitecture == Architecture.Arm64;

            // Skip loading SqlServerTypes on ARM architecture. The native DLL
            // does not support ARM. This is for development purposes only as
            // Rock is not intended to run in production without full spatial
            // support.
            // Simple spatial features work, such as setting and reading a
            // GeoPoint on a Location. More complex features, like setting or
            // reading a GeoFence on a Location (and related "is in range"
            // features) do not work and will throw DLL not found errors.
            if ( isArm )
            {
                return;
            }

            var nativeBinaryPath = IntPtr.Size > 4
                ? Path.Combine(rootApplicationPath, @"SqlServerTypes\x64\")
                : Path.Combine(rootApplicationPath, @"SqlServerTypes\x86\");

            LoadNativeAssembly(nativeBinaryPath, "msvcr100.dll");
            LoadNativeAssembly(nativeBinaryPath, "SqlServerSpatial110.dll");
        }

        private static void LoadNativeAssembly(string nativeBinaryPath, string assemblyName)
        {
            var path = Path.Combine(nativeBinaryPath, assemblyName);
            var ptr = LoadLibrary(path);
            if (ptr == IntPtr.Zero)
            {
                throw new Exception(string.Format(
                    "Error loading {0} (ErrorCode: {1})",
                    path + " as " + System.Security.Principal.WindowsIdentity.GetCurrent().Name,
                    Marshal.GetLastWin32Error()));
            }
        }
    }
}