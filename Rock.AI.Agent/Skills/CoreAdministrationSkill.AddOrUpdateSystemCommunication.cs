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
using Rock.AI.Agent.Classes;
using Rock.AI.Agent.Classes.Common;
using Rock.AI.Agent.Classes.Skills.CoreAdministrationSkill;
using Rock.Configuration;
using Rock.Model;
using Rock.Security;
using Rock.SystemGuid;
using Rock.Web.Cache;

namespace Rock.AI.Agent.Skills;

internal sealed partial class CoreAdministrationSkill
{
    #region Tool(s)

    /// <summary>
    /// Adds a system communication template or updates an existing one.
    /// </summary>
    /// <remarks>
    /// An existing template can be read with <see cref="GetSystemCommunication"/>,
    /// but its body is the one field no tool returns; only the body length is
    /// reported. A caller therefore cannot confirm the stored body by reading it,
    /// so send the whole body when changing it, because an omitted body leaves the
    /// existing one unchanged rather than clearing it.
    /// </remarks>
    [Description( "Adds a new system communication template or updates an existing one. These are the templates used by workflow actions and other features to send email, SMS, or push messages." )]
    [AgentUsage( "title is required when adding. Supplying systemCommunicationIdKey updates that template and leaves any parameter you omit unchanged." )]
    [AgentToolPrerequisite( "Call ListCategories with the SystemCommunication entity type to determine the categoryIdKey." )]
    [AgentToolGuid( "A847041F-D744-40C8-9DF9-D4614B4AB1F2" )]
    public AgentToolResult AddOrUpdateSystemCommunication(
        string systemCommunicationIdKey = null,
        string title = null,
        SetOrClear<string> categoryIdKey = null,
        bool? isActive = null,
        SetOrClear<string> from = null,
        SetOrClear<string> fromName = null,
        SetOrClear<string> to = null,
        SetOrClear<string> cc = null,
        SetOrClear<string> bcc = null,
        SetOrClear<string> subject = null,
        [Description( "The email body, which may contain Lava. Send the whole body when changing it; an omitted body leaves the existing one unchanged." )]
        SetOrClear<string> body = null,
        [Description( "The SMS message text. Setting this makes the template able to send SMS in addition to email." )]
        SetOrClear<string> smsMessage = null,
        SetOrClear<string> pushTitle = null,
        [Description( "The push notification message. Setting this makes the template able to send a push notification in addition to email." )]
        SetOrClear<string> pushMessage = null )
    {
        using var rockContext = RockApp.Current.CreateRockContext();

        var helper = new AgentToolHelper( rockContext, AgentRequestContext, _logger );
        var systemCommunicationService = new SystemCommunicationService( rockContext );

        Rock.Model.SystemCommunication systemCommunication;
        var isNew = systemCommunicationIdKey.IsNullOrWhiteSpace();

        if ( !isNew )
        {
            systemCommunication = helper.GetRequiredEntity<Rock.Model.SystemCommunication>( systemCommunicationIdKey );

            if ( systemCommunication == null )
            {
                return helper.ErrorResult
                    .WithInstructions( $"Call the {nameof( ListSystemCommunications )} function to determine the available system communications." );
            }
        }
        else
        {
            if ( title.IsNullOrWhiteSpace() )
            {
                return Error( $"{nameof( title )} is required when adding a system communication." );
            }

            // Created through the context rather than with new, so Entity Framework
            // hands back a proxy and can track the navigation properties set later.
            systemCommunication = rockContext.Set<Rock.Model.SystemCommunication>().Create();

            systemCommunication.IsActive = true;

            systemCommunicationService.Add( systemCommunication );
        }

        helper.UpdateProperty( systemCommunication, sc => sc.Title, title );
        helper.UpdateProperty( systemCommunication, sc => sc.IsActive, isActive );
        helper.UpdateProperty( systemCommunication, sc => sc.From, from );
        helper.UpdateProperty( systemCommunication, sc => sc.FromName, fromName );
        helper.UpdateProperty( systemCommunication, sc => sc.To, to );
        helper.UpdateProperty( systemCommunication, sc => sc.Cc, cc );
        helper.UpdateProperty( systemCommunication, sc => sc.Bcc, bcc );
        helper.UpdateProperty( systemCommunication, sc => sc.Subject, subject );
        helper.UpdateProperty( systemCommunication, sc => sc.Body, body );
        helper.UpdateProperty( systemCommunication, sc => sc.SMSMessage, smsMessage );
        helper.UpdateProperty( systemCommunication, sc => sc.PushTitle, pushTitle );
        helper.UpdateProperty( systemCommunication, sc => sc.PushMessage, pushMessage );
        helper.UpdateNavigationProperty( systemCommunication, sc => sc.Category, categoryIdKey );

        if ( helper.HasErrors )
        {
            return helper.ErrorResult
                .WithInstructions( $"Call the {nameof( ListCategories )} function with the SystemCommunication entity type to determine the available categories." );
        }

        // The caller must be able to edit the system communication, which for a
        // new one resolves to the default security at the root of the chain.
        if ( !systemCommunication.IsAuthorized( Authorization.EDIT, AgentRequestContext.CurrentPerson ) )
        {
            return Error( "You are not authorized to save that system communication." );
        }

        helper.SaveChangesIfNoErrors();

        if ( helper.HasErrors )
        {
            return helper.ErrorResult;
        }

        var category = systemCommunication.CategoryId.HasValue
            ? CategoryCache.Get( systemCommunication.CategoryId.Value, rockContext )
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
            Category = KeyNameResult.FromCache( category ),
            IsActive = systemCommunication.IsActive ?? false,
            IsSystem = systemCommunication.IsSystem,
            HasSmsMessage = systemCommunication.SMSMessage.IsNotNullOrWhiteSpace(),
            HasPushMessage = systemCommunication.PushMessage.IsNotNullOrWhiteSpace(),
            BodyLength = systemCommunication.Body?.Length ?? 0
        };

        return Success( result )
            .WithInstructions( isNew
                ? "The system communication has been created."
                : "The system communication has been updated." )
            .WithHistoryContent( new KeyNameResult( systemCommunication.Id, systemCommunication.Guid, systemCommunication.Title ) );
    }

    #endregion
}
