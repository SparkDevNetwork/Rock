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
using System.Collections.Generic;
using System.Linq;

using Rock.Data;

namespace Rock.Tests.Shared.TestFramework;

/// <summary>
/// Helpers for asserting on schema check findings. Shared by the unit tests
/// of the checks in Rock.Tests and the database tests in Rock.Tests.Integration.
/// </summary>
internal static class SchemaFindings
{
    /// <summary>
    /// Describes findings as sorted <c>STATUS check code object</c> lines,
    /// where STATUS is <c>FAIL</c>, <c>ALLOW</c> (an allow-listed failure)
    /// or <c>INFO</c>.
    /// </summary>
    /// <param name="findings">The findings to describe.</param>
    /// <returns>One line per finding, in ordinal order.</returns>
    internal static string[] Describe( IEnumerable<SchemaFinding> findings )
    {
        return findings
            .Select( f => $"{( !f.IsFailure ? "INFO" : f.IsBlocking ? "FAIL" : "ALLOW" )} {f.Check} {f.Code} {f.Object}" )
            .OrderBy( s => s, StringComparer.Ordinal )
            .ToArray();
    }
}
