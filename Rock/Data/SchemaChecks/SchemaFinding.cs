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

using Newtonsoft.Json;

namespace Rock.Data;

/// <summary>
/// One difference found by a schema check.
/// </summary>
internal sealed class SchemaFinding
{
    /// <summary>
    /// The check that produced the finding: <c>mapping</c>, <c>names</c>,
    /// <c>equivalence</c> or <c>data</c>.
    /// </summary>
    public string Check { get; set; }

    /// <summary>
    /// A stable code for the kind of finding, such as <c>MissingColumn</c>.
    /// Allow list entries match on it.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// The object the finding is about, such as <c>dbo.Person.FirstName</c>.
    /// Allow list entries match on it.
    /// </summary>
    public string Object { get; set; }

    /// <summary>
    /// A readable description of the difference.
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Whether the finding fails the check. Informational findings are
    /// reported but never fail.
    /// </summary>
    public bool IsFailure { get; set; }

    /// <summary>
    /// The reason from the matching allow list entry, or <c>null</c> when
    /// the finding is not allowed.
    /// </summary>
    public string AllowedReason { get; set; }

    /// <summary>
    /// Whether this finding fails the check after the allow list is applied.
    /// </summary>
    public bool IsBlocking => IsFailure && AllowedReason == null;

    /// <summary>
    /// Creates a finding that fails the check.
    /// </summary>
    public static SchemaFinding Failure( string check, string code, string objectKey, string message )
    {
        return new SchemaFinding { Check = check, Code = code, Object = objectKey, Message = message, IsFailure = true };
    }

    /// <summary>
    /// Creates a finding that is reported but never fails the check.
    /// </summary>
    public static SchemaFinding Informational( string check, string code, string objectKey, string message )
    {
        return new SchemaFinding { Check = check, Code = code, Object = objectKey, Message = message, IsFailure = false };
    }
}

/// <summary>
/// The reviewed list of findings that are known and accepted, each with
/// a reason. Stored as JSON beside the schema check tool.
/// </summary>
internal sealed class SchemaAllowList
{
    /// <summary>
    /// The accepted findings.
    /// </summary>
    public List<SchemaAllowListEntry> Entries { get; set; } = new List<SchemaAllowListEntry>();

    /// <summary>
    /// Reads an allow list from its JSON text.
    /// </summary>
    /// <param name="json">The JSON text.</param>
    /// <returns>The allow list.</returns>
    public static SchemaAllowList FromJson( string json )
    {
        return JsonConvert.DeserializeObject<SchemaAllowList>( json ) ?? new SchemaAllowList();
    }

    /// <summary>
    /// Writes the allow list as indented JSON, sorted so that diffs stay small.
    /// </summary>
    /// <returns>The JSON text.</returns>
    public string ToJson()
    {
        var sorted = new SchemaAllowList
        {
            Entries = Entries
                .OrderBy( e => e.Check, StringComparer.Ordinal )
                .ThenBy( e => e.Code, StringComparer.Ordinal )
                .ThenBy( e => e.Object, StringComparer.OrdinalIgnoreCase )
                .ToList()
        };

        return JsonConvert.SerializeObject( sorted, Formatting.Indented );
    }

    /// <summary>
    /// Marks every finding that matches an entry as allowed and returns
    /// a failing finding for each entry of the given checks that matched
    /// nothing, and for each extra copy of an entry. Failing on stale and
    /// duplicate entries forces their removal, so the list only shrinks.
    /// </summary>
    /// <param name="findings">The findings to mark.</param>
    /// <param name="checks">The checks that ran; entries for other checks are not reported.</param>
    /// <returns>The stale and duplicate entry findings.</returns>
    public List<SchemaFinding> Apply( IEnumerable<SchemaFinding> findings, params string[] checks )
    {
        var entryGroups = Entries
            .GroupBy( e => Identity( e.Check, e.Code, e.Object ), StringComparer.OrdinalIgnoreCase )
            .ToList();
        var entriesByIdentity = entryGroups.ToDictionary( g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase );
        var matchedIdentities = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

        foreach ( var finding in findings )
        {
            var identity = Identity( finding.Check, finding.Code, finding.Object );

            if ( entriesByIdentity.TryGetValue( identity, out var entry ) )
            {
                finding.AllowedReason = entry.Reason.IsNullOrWhiteSpace() ? "(no reason given)" : entry.Reason;
                matchedIdentities.Add( identity );
            }
        }

        var problems = new List<SchemaFinding>();

        foreach ( var group in entryGroups )
        {
            var entry = group.First();

            if ( !checks.Contains( entry.Check, StringComparer.OrdinalIgnoreCase ) )
            {
                continue;
            }

            if ( !matchedIdentities.Contains( group.Key ) )
            {
                problems.Add( SchemaFinding.Failure( entry.Check, "StaleAllowListEntry", entry.Object, $"Allow list entry for {entry.Code} no longer matches anything. Remove it from the allow list." ) );
            }

            // A merge that keeps both sides can leave two copies of one entry.
            foreach ( var copy in group.Skip( 1 ) )
            {
                problems.Add( SchemaFinding.Failure( copy.Check, "DuplicateAllowListEntry", copy.Object, $"Allow list entry for {copy.Code} is listed more than once. Remove the extra copy." ) );
            }
        }

        return problems;
    }

    /// <summary>
    /// Builds the key an entry and a finding are matched on.
    /// </summary>
    private static string Identity( string check, string code, string objectKey )
    {
        return $"{check}|{code}|{objectKey}";
    }
}

/// <summary>
/// One accepted finding in a <see cref="SchemaAllowList"/>.
/// </summary>
internal sealed class SchemaAllowListEntry
{
    /// <summary>
    /// The check the entry applies to.
    /// </summary>
    public string Check { get; set; }

    /// <summary>
    /// The finding code the entry accepts.
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// The object the entry accepts the finding for.
    /// </summary>
    public string Object { get; set; }

    /// <summary>
    /// Why the finding is accepted.
    /// </summary>
    public string Reason { get; set; }
}
