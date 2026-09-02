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
using System.Linq;

using Rock.AI.Agent.Annotations;
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CommunicationSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CommunicationSkill
{
    #region Tool(s)

    /// <summary>
    /// Updates an existing communication template's metadata and addressing.
    /// </summary>
    /// <remarks>
    /// This updates the template's name, category, active state, and addressing
    /// fields only. It does not create templates, and it does not change the email,
    /// SMS, or push message content, which are authored through Rock's
    /// communication template editor.
    /// </remarks>
    [Description( "Updates an existing communication template's metadata and addressing (name, category, active state, from/reply-to/cc/bcc, and the SMS from number). It does not create templates or change the email, SMS, or push message content." )]
    [AgentToolPreamble( "Saving the communication template." )]
    [AgentUsage( "Pass only the properties to change. Templates cannot be created here, and their message content is not editable through this tool." )]
    [AgentToolPrerequisite( "Call ListCommunicationTemplates to determine the communicationTemplateIdKey, ListCategories with the CommunicationTemplate entity type for the categoryIdKey, and LookupSystemPhoneNumbers for the smsFromSystemPhoneNumberIdKey." )]
    [AgentToolGuid( "147D8B24-61A7-458B-B6C8-23765F47D1BE" )]
    public AgentToolResult UpdateCommunicationTemplate(
        string communicationTemplateIdKey,
        string name = null,
        SetOrClear<string> description = null,
        SetOrClear<string> categoryIdKey = null,
        bool? isActive = null,
        SetOrClear<string> fromName = null,
        SetOrClear<string> fromEmail = null,
        SetOrClear<string> replyToEmail = null,
        SetOrClear<string> cc = null,
        SetOrClear<string> bcc = null,
        [Description( "The system phone number SMS is sent from." )]
        SetOrClear<string> smsFromSystemPhoneNumberIdKey = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );

        var template = helper.GetRequiredEntity<Model.CommunicationTemplate>( communicationTemplateIdKey );

        if ( template == null )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListCommunicationTemplates )} function to determine the available communication templates." );
        }

        if ( !template.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to edit that communication template." );
        }

        helper.UpdateProperty( template, t => t.Name, name );
        helper.UpdateProperty( template, t => t.Description, description );
        helper.UpdateProperty( template, t => t.IsActive, isActive );
        helper.UpdateProperty( template, t => t.FromName, fromName );
        helper.UpdateProperty( template, t => t.FromEmail, fromEmail );
        helper.UpdateProperty( template, t => t.ReplyToEmail, replyToEmail );
        helper.UpdateProperty( template, t => t.CCEmails, cc );
        helper.UpdateProperty( template, t => t.BCCEmails, bcc );
        helper.UpdateNavigationProperty( template, t => t.Category, categoryIdKey );
        helper.UpdateNavigationProperty( template, t => t.SmsFromSystemPhoneNumber, smsFromSystemPhoneNumberIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the ListCategories function with the CommunicationTemplate entity type, or the {nameof( LookupSystemPhoneNumbers )} function, to determine valid values." );
        }

        if ( !template.IsValid )
        {
            helper.AddError( template.ValidationResults.Select( r => r.ErrorMessage ).FirstOrDefault() ?? "The communication template could not be saved." );

            return helper.ErrorResult;
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var category = template.CategoryId.HasValue
            ? CategoryCache.Get( template.CategoryId.Value, rockContext )
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
            MessageLength = template.Message?.Length ?? 0
        };

        return Success( result )
            .WithInstructions( "The communication template has been updated." )
            .WithHistoryContent( new KeyNameResult( template.Id, template.Guid, template.Name ) );
    }

    #endregion
}
