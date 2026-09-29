// <copyright>
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
using System.Runtime.InteropServices;

namespace Rock.AI.Agent.Tests.TestFramework
{
    /// <summary>
    /// Helpers for the SQL Server spatial native libraries used by tests that
    /// evaluate <see cref="System.Data.Entity.Spatial.DbGeography"/> operations
    /// client-side against a mocked database.
    /// </summary>
    /// <remarks>
    /// The native library (SqlServerSpatial110.dll) does not support ARM, so any
    /// test that computes distances or builds geography points in memory can run
    /// only on a non-ARM process. On ARM those tests are skipped rather than
    /// failed. This mirrors the ARM handling in
    /// <c>RockWeb/App_Code/SqlServerTypes/Loader.cs</c>. A Windows-specific check
    /// is unnecessary because this test project targets .NET Framework, which
    /// only runs on Windows.
    /// </remarks>
    internal static class SqlServerSpatialSupport
    {
        /// <summary>
        /// Gets a value indicating whether the current process can load the SQL
        /// Server spatial native library.
        /// </summary>
        public static bool IsSupported =>
            RuntimeInformation.ProcessArchitecture != Architecture.Arm
            && RuntimeInformation.ProcessArchitecture != Architecture.Arm64;

        /// <summary>
        /// Loads the SQL Server spatial native libraries when the current process
        /// architecture supports them. Does nothing otherwise.
        /// </summary>
        public static void EnsureNativeAssembliesLoaded()
        {
            if ( IsSupported )
            {
                SqlServerTypes.Utilities.LoadNativeAssemblies( AppDomain.CurrentDomain.BaseDirectory );
            }
        }
    }
}
