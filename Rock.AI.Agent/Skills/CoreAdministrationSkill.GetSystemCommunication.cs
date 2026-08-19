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

using System.ComponentModel;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single system communication in full detail.
    /// </summary>
    /// <remarks>
    /// The message body is never returned. It is large and no authoring task
    /// needs its contents; the body length tells a caller whether the template
    /// has content without paying for it.
    /// </remarks>
    [Description( "Gets a single system communication in full detail. The message body itself is never returned." )]
    [AgentPurpose( "Retrieves the settings of one system communication template." )]
    [AgentToolPrerequisite( "Call ListSystemCommunications to determine the systemCommunicationIdKey." )]
    [AgentToolGuid( "1D1D0F7C-6B22-4C4E-9E4B-BC5A0A9F1D74" )]
    public AgentToolResult GetSystemCommunication( string systemCommunicationIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var systemCommunication = helper.GetRequiredEntity<Rock.Model.SystemCommunication>( systemCommunicationIdKey );

        if ( systemCommunication == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListSystemCommunications )} function to determine the available system communications." );
        }

        var category = systemCommunication.CategoryId.HasValue
            ? CategoryCache.Get( systemCommunication.CategoryId.Value, AgentRequestContext.RockContext )
            : null;

        var result = new SystemCommunicationDetailResult
        {
            Id = systemCommunication.Id,
            Guid = systemCommunication.Guid,
            Title = systemCommunication.Title,
            Subject = systemCommunication.Subject,
            From = systemCommunication.From,
            FromName = systemCommunication.FromName,
            To = systemCommunication.To,
            Cc = systemCommunication.Cc,
            Bcc = systemCommunication.Bcc,
            Category = category != null
                ? new KeyNameResult { Id = category.Id, Guid = category.Guid, Name = category.Name }
                : null,
            IsActive = systemCommunication.IsActive ?? false,
            IsSystem = systemCommunication.IsSystem,
            HasSmsMessage = systemCommunication.SMSMessage.IsNotNullOrWhiteSpace(),
            HasPushMessage = systemCommunication.PushMessage.IsNotNullOrWhiteSpace(),

            // The length rather than the body. A caller needs to know the
            // template has content, not what it says.
            BodyLength = systemCommunication.Body?.Length ?? 0
        };

        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this system communication." );
        }

        return Success( result )
            .WithHistoryContent( new KeyNameResult( systemCommunication.Id, systemCommunication.Guid, systemCommunication.Title ) );
    }

    #endregion
}
