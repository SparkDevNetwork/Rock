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
using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.SystemGuid;
using Rock.Utility;

namespace Rock.AI.Agent.Skills;

internal partial class SystemUtilitySkill
{
    #region Tool

    /// <summary>
    /// Decodes an IdKey into the integer identifier it stands for.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An IdKey is a reversible encoding of a row's integer Id, so this is arithmetic
    /// rather than a lookup: nothing is read from the database and no check is made
    /// that a row with the resulting Id exists, or that the caller is allowed to see
    /// it. That is why it is deliberately narrow.
    /// </para>
    /// <para>
    /// It exists for the small number of places where Rock stores a bare integer and
    /// nothing else will do. A handful of field types are like this, Site and Document
    /// Type among them, and their own field hints say so. Everywhere else a tool
    /// parameter takes the IdKey directly, and stored references use the record's
    /// guid.
    /// </para>
    /// <para>
    /// The reason for the warning rather than a silent conversion is that an Id used
    /// where a guid or an IdKey belongs produces a value that saves cleanly and then
    /// resolves to nothing. It is the same class of failure as writing a label into a
    /// select-backed setting: no error, and no visible effect until someone opens the
    /// record.
    /// </para>
    /// </remarks>
    [Description( "Converts an IdKey into the integer Id it encodes." )]
    [AgentPurpose( "Provides the raw integer Id for the rare setting that stores one, rather than an IdKey or a guid." )]
    [AgentUsage( "Only use this when the value being written is documented as requiring a bare integer Id. Do not use it to fill an ordinary parameter: every parameter in every skill takes the IdKey itself, and stored references to other records take that record's guid. An Id written where one of those belongs saves without error and then resolves to nothing." )]
    [AgentGuardrail( "This decodes the key arithmetically. It does not confirm that a record with that Id exists, and it applies no security check, so a successful result is not evidence that the record is real or readable." )]
    [AgentToolGuid( "0C6D9E4A-7B31-42F8-9E05-3A17C4B8D260" )]
    public AgentToolResult ConvertIdKeyToId(
        string idKey )
    {
        if ( idKey.IsNullOrWhiteSpace() )
        {
            return Error( $"{nameof( idKey )} is required." );
        }

        var id = IdHasher.Instance.GetId( idKey );

        if ( !id.HasValue )
        {
            return Error( $"'{idKey}' is not a valid IdKey." )
                .WithInstructions( "An IdKey is the short opaque string returned as idKey by the tool that read the record. A guid is not an IdKey and cannot be converted to an Id here; use the guid itself wherever a record is referenced by guid." );
        }

        return Success( new { Id = id.Value } )
            .WithInstructions( "Use this only where a bare integer Id is required. Send the IdKey, not this number, to any tool parameter, and send the record's guid where a stored reference names another record." )
            .WithoutHistoryContent();
    }

    #endregion
}
