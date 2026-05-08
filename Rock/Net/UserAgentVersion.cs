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
using System.Text;

using Rock.Lava;

namespace Rock.Net;

/// <summary>
/// A Rock-owned, structured representation of the version segments parsed
/// from a user-agent string.
/// </summary>
[LavaType]
public sealed class UserAgentVersion
{
    #region Fields

    // The original parser segments are stored privately so ToString() can
    // round-trip non-numeric segments (e.g. the "rc7" in
    // "Chrome/132.8.28-rc7.8") that the int? properties cannot represent.
    private readonly string _major;
    private readonly string _minor;
    private readonly string _patch;
    private readonly string _patchMinor;

    #endregion

    #region Properties

    /// <summary>
    /// The major version segment as an integer, or <c>null</c> when
    /// the segment is missing or non-numeric.
    /// </summary>
    public int? Major { get; }

    /// <summary>
    /// The minor version segment as an integer, or <c>null</c> when
    /// the segment is missing or non-numeric.
    /// </summary>
    public int? Minor { get; }

    /// <summary>
    /// The patch version segment as an integer, or <c>null</c> when
    /// the segment is missing or non-numeric.
    /// </summary>
    public int? Patch { get; }

    /// <summary>
    /// The patch-minor version segment as an integer, or <c>null</c>
    /// when the segment is missing or non-numeric.
    /// </summary>
    public int? PatchMinor { get; }

    #endregion

    #region Constructors

    /// <summary>
    /// Initializes a new instance of the <see cref="UserAgentVersion"/>
    /// class from raw string segments as captured by the underlying parser.
    /// </summary>
    /// <param name="major">The raw major segment, or <c>null</c>.</param>
    /// <param name="minor">The raw minor segment, or <c>null</c>.</param>
    /// <param name="patch">The raw patch segment, or <c>null</c>.</param>
    /// <param name="patchMinor">The raw patch-minor segment, or <c>null</c>.</param>
    internal UserAgentVersion( string major, string minor, string patch, string patchMinor )
    {
        _major = major;
        _minor = minor;
        _patch = patch;
        _patchMinor = patchMinor;

        Major = major.AsIntegerOrNull();
        Minor = minor.AsIntegerOrNull();
        Patch = patch.AsIntegerOrNull();
        PatchMinor = patchMinor.AsIntegerOrNull();
    }

    #endregion

    #region Methods

    /// <summary>
    /// Returns the dotted-version form built from the original string
    /// segments, with empty segments skipped. Examples: "10",
    /// "10.0.4.78", "132.8.28-rc7.8", or an empty string when no version
    /// data was captured.
    /// </summary>
    /// <returns>The dotted-version string.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();

        AppendSegment( sb, _major );
        AppendSegment( sb, _minor );
        AppendSegment( sb, _patch );
        AppendSegment( sb, _patchMinor );

        return sb.ToString();
    }

    /// <summary>
    /// Appends a version segment to the string builder, prefixed by a dot if
    /// the builder already has content. Segments that are null, empty, or whitespace
    /// are skipped.
    /// </summary>
    /// <param name="sb">The <see cref="StringBuilder"/> to append to.</param>
    /// <param name="segment">The version segment to append.</param>
    private static void AppendSegment( StringBuilder sb, string segment )
    {
        if ( segment.IsNullOrWhiteSpace() )
        {
            return;
        }

        if ( sb.Length > 0 )
        {
            sb.Append( '.' );
        }

        sb.Append( segment );
    }

    #endregion
}
