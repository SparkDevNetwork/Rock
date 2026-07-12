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
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Field.Types;
using Rock.Lava;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Communication.SmsActions
{
    /// <summary>
    /// Processes an SMS Action by creating a Connection Request on a configured
    /// Connection Opportunity for the inbound person.
    /// </summary>
    /// <seealso cref="Rock.Communication.SmsActions.SmsActionComponent" />
    [Description( "Creates a connection request from an inbound SMS message." )]
    [Export( typeof( SmsActionComponent ) )]
    [ExportMetadata( "ComponentName", "Create Connection Request" )]

    [TextValueFilterField( "Message",
        Key = AttributeKey.Message,
        Description = "The message body content that will be filtered on.",
        IsRequired = false,
        Category = AttributeCategories.Filters,
        Order = 1 )]

    [ConnectionTypeSettingsField( "Connection Type Settings",
        Key = AttributeKey.ConnectionTypeSettings,
        Description = "The Connection Type, Opportunity, Status, and Type Source the new request is created under.",
        IsRequired = true,
        Category = AttributeCategories.Connection,
        Order = 2 )]

    [CampusField( "Campus",
        includeInactive: false,
        Description = "Overrides the inbound person's primary campus when set. Leave blank to use the person's primary campus.",
        IsRequired = false,
        ForceVisible = true,
        Category = AttributeCategories.Connection,
        Order = 3,
        Key = AttributeKey.Campus )]

    [CodeEditorField( "Comment Template",
        Description = "Lava template merged into the new Connection Request's Comments field. Receives Message, FromPerson, and the inbound SmsMessage as merge fields.",
        EditorMode = CodeEditorMode.Lava,
        EditorHeight = 100,
        IsRequired = false,
        DefaultValue = "{{ Message }}",
        Category = AttributeCategories.Connection,
        Order = 4,
        Key = AttributeKey.CommentTemplate )]

    [BooleanField( "Pass Nameless Person",
        Key = AttributeKey.PassNamelessPerson,
        Category = AttributeCategories.Connection,
        Description = "When true, an inbound message from an unknown phone number is allowed to create a Connection Request against a nameless Person record. When false, such messages are skipped.",
        ControlType = BooleanFieldType.BooleanControlType.Checkbox,
        DefaultBooleanValue = true,
        Order = 5 )]

    [Rock.SystemGuid.EntityTypeGuid( Rock.SystemGuid.EntityType.SMS_ACTION_CREATE_CONNECTION_REQUEST )]
    public class SmsActionCreateConnectionRequest : SmsActionComponent
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for the per-action attribute values.
        /// </summary>
        private static class AttributeKey
        {
            public const string Message = "Message";
            public const string ConnectionTypeSettings = "ConnectionTypeSettings";
            public const string Campus = "Campus";
            public const string CommentTemplate = "CommentTemplate";
            public const string PassNamelessPerson = "PassNamelessPerson";
        }

        #endregion Attribute Keys

        /// <summary>
        /// Categories for the attributes.
        /// </summary>
        protected class AttributeCategories : BaseAttributeCategories
        {
            /// <summary>
            /// The Connection category.
            /// </summary>
            public const string Connection = "Connection";
        }

        #region Properties

        /// <inheritdoc/>
        public override string Title => "Create Connection Request";

        /// <inheritdoc/>
        public override string IconCssClass => "ti ti-plug";

        /// <inheritdoc/>
        public override string Description => "Creates a connection request from an inbound SMS message.";

        #endregion Properties

        #region Base Method Overrides

        /// <inheritdoc/>
        public override bool ShouldProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            if ( !base.ShouldProcessMessage( action, message, out errorMessage ) )
            {
                return false;
            }

            var attribute = action.Attributes.ContainsKey( AttributeKey.Message ) ? action.Attributes[AttributeKey.Message] : null;
            var msg = GetAttributeValue( action, AttributeKey.Message );
            var filter = ValueFilterFieldType.GetFilterExpression( attribute?.QualifierValues, msg );

            return filter != null ? filter.Evaluate( message, AttributeKey.Message ) : true;
        }

        /// <inheritdoc/>
        public override SmsMessage ProcessMessage( SmsActionCache action, SmsMessage message, out string errorMessage )
        {
            errorMessage = string.Empty;

            ConnectionTypeSettingsFieldType.ParseDelimitedGuids(
                GetAttributeValue( action, AttributeKey.ConnectionTypeSettings ),
                out _,
                out var opportunityGuid,
                out var statusGuid,
                out var sourceGuid );

            if ( !opportunityGuid.HasValue )
            {
                errorMessage = "No Connection Opportunity is configured on this action.";
                return null;
            }

            var passNamelessPerson = GetAttributeValue( action, AttributeKey.PassNamelessPerson ).AsBooleanOrNull() ?? true;
            var fromPerson = message.FromPerson;

            if ( fromPerson != null && fromPerson.IsNameless() && !passNamelessPerson )
            {
                errorMessage = "Inbound message is from a nameless person and this action is configured to skip nameless persons.";
                return null;
            }

            if ( fromPerson?.PrimaryAliasId == null )
            {
                errorMessage = "Inbound message has no resolved person.";
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var opportunity = new ConnectionOpportunityService( rockContext )
                    .Queryable()
                    .AsNoTracking()
                    .Include( o => o.ConnectionType.ConnectionStatuses )
                    .Include( o => o.ConnectionOpportunityCampuses.Select( c => c.DefaultConnectorPersonAlias ) )
                    .FirstOrDefault( o => o.Guid == opportunityGuid.Value );

                if ( opportunity == null || !opportunity.IsActive || !opportunity.ConnectionType.IsActive )
                {
                    errorMessage = $"Connection Opportunity '{opportunityGuid}' could not be resolved.";
                    return null;
                }

                int? statusId = null;
                if ( statusGuid.HasValue )
                {
                    statusId = opportunity.ConnectionType.ConnectionStatuses
                        .FirstOrDefault( s => s.Guid == statusGuid.Value )?.Id;
                }
                if ( !statusId.HasValue )
                {
                    statusId = opportunity.ConnectionType.ConnectionStatuses
                        .FirstOrDefault( s => s.IsDefault )?.Id;
                }
                if ( !statusId.HasValue )
                {
                    errorMessage = $"Connection Opportunity '{opportunity.Name}' has no Status configured.";
                    return null;
                }

                int? sourceId = null;
                if ( sourceGuid.HasValue )
                {
                    sourceId = new ConnectionTypeSourceService( rockContext ).GetId( sourceGuid.Value );
                }

                int? campusId = null;
                var campusGuid = GetAttributeValue( action, AttributeKey.Campus ).AsGuidOrNull();
                if ( campusGuid.HasValue )
                {
                    campusId = CampusCache.Get( campusGuid.Value )?.Id;
                }
                if ( !campusId.HasValue )
                {
                    campusId = fromPerson.PrimaryCampusId;
                }

                var commentTemplate = GetAttributeValue( action, AttributeKey.CommentTemplate ) ?? string.Empty;
                var mergeObjects = new Dictionary<string, object>
                {
                    { "Message", message.Message },
                    { "FromPerson", fromPerson },
                    { "SmsMessage", message }
                };
                var comments = commentTemplate.ResolveMergeFields( mergeObjects, fromPerson );

                var connectionRequest = new ConnectionRequest
                {
                    PersonAliasId = fromPerson.PrimaryAliasId.Value,
                    ConnectionOpportunityId = opportunity.Id,
                    ConnectionTypeId = opportunity.ConnectionTypeId,
                    ConnectionStatusId = statusId.Value,
                    ConnectionState = ConnectionState.Active,
                    ConnectionTypeSourceId = sourceId,
                    CampusId = campusId,
                    ConnectorPersonAliasId = opportunity.GetDefaultConnectorPersonAliasId( campusId ),
                    Comments = comments
                };

                new ConnectionRequestService( rockContext ).Add( connectionRequest );
                rockContext.SaveChanges();
            }

            return null;
        }

        #endregion Base Method Overrides
    }
}
