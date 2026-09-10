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
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunicationSkill
{
    #region Tool(s)

    /// <summary>
    /// Gets a single communication template in full detail.
    /// </summary>
    /// <remarks>
    /// The email message body is never returned. It is large and no authoring task
    /// needs its contents; the message length tells a caller whether the template
    /// has content without paying for it.
    /// </remarks>
    [Description( "Gets a single communication template in full detail. The email message body itself is never returned." )]
    [AgentPurpose( "Retrieves the settings of one communication template." )]
    [AgentToolPrerequisite( "Call ListCommunicationTemplates to determine the communicationTemplateIdKey." )]
    [AgentToolGuid( "702660D9-B56E-45BA-9415-0EA764CFBA91" )]
    public AgentToolResult GetCommunicationTemplate( string communicationTemplateIdKey )
    {
        var helper = new AgentToolHelper( AgentRequestContext, _logger );

        var template = helper.GetRequiredEntity<Model.CommunicationTemplate>( communicationTemplateIdKey );

        if ( template == null )
        {
            return helper.ErrorResult
                .WithInstructions( "Call the ListCommunicationTemplates function to determine the available communication templates." );
        }

        var category = template.CategoryId.HasValue
            ? CategoryCache.Get( template.CategoryId.Value, AgentRequestContext.RockContext )
            : null;

        var smsFromNumber = template.SmsFromSystemPhoneNumberId.HasValue
            ? SystemPhoneNumberCache.Get( template.SmsFromSystemPhoneNumberId.Value )
            : null;

        var result = new CommunicationTemplateDetailResult
        {
            Id = template.Id,
            Guid = template.Guid,
            Name = template.Name,
            Description = template.Description,
            Category = KeyNameResult.FromCache( category ),
            IsActive = template.IsActive,
            IsSystem = template.IsSystem,
            Subject = template.Subject,
            FromName = template.FromName,
            FromEmail = template.FromEmail,
            ReplyToEmail = template.ReplyToEmail,
            Cc = template.CCEmails,
            Bcc = template.BCCEmails,
            HasSmsMessage = template.SMSMessage.IsNotNullOrWhiteSpace(),
            SmsFromSystemPhoneNumber = KeyNameResult.FromCache( smsFromNumber ),
            HasPushMessage = template.PushMessage.IsNotNullOrWhiteSpace(),
            PushTitle = template.PushTitle,

            // The length rather than the body. A caller needs to know the template
            // has content, not what it says.
            MessageLength = template.Message?.Length ?? 0
        };

        if ( !result.Sanitize( AgentRequestContext ) )
        {
            return Error( "You do not have permission to view this communication template." );
        }

        return Success( result )
            .WithHistoryContent( new KeyNameResult( template.Id, template.Guid, template.Name ) );
    }

    #endregion
}
