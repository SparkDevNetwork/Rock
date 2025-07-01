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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Enums.Communication;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.Utility;
using Rock.ViewModels.Blocks.Communication.CommunicationDetail;
using Rock.ViewModels.Controls;
using Rock.ViewModels.Reporting;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

using CommunicationDetailPushOpenAction = Rock.Enums.Blocks.Communication.CommunicationDetail.PushOpenAction;
using CommunicationType = Rock.Enums.Communication.CommunicationType;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Used for displaying details of an existing communication that has already been created.
    /// </summary>

    [DisplayName( "Communication Detail" )]
    [Category( "Communication" )]
    [Description( "Used for displaying details of an existing communication that has already been created." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [BooleanField( "Enable Personal Templates",
        Key = AttributeKey.EnablePersonalTemplates,
        Description = "Should support for personal templates be enabled? These are templates that a user can create and are personal to them. If enabled, they will be able to create a new template based on the current communication.",
        DefaultBooleanValue = false,
        Order = 0,
        IsRequired = false )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "32838848-2423-4BD9-B5EF-5F7E6AC7F5F4" )]
    [Rock.SystemGuid.BlockTypeGuid( "2B63C6ED-20D5-467E-9A6A-C608E1D953E5" )]
    public class CommunicationDetail : RockBlockType
    {
        #region Keys & Constants

        private static class AttributeKey
        {
            public const string EnablePersonalTemplates = "EnablePersonalTemplates";
        }

        private static class PageParameterKey
        {
            // "Communication" allows Communication Id, Guid, or IdKey values,
            // while the older "CommunicationId" only supports Id.
            public const string Communication = "Communication";
            public const string CommunicationId = "CommunicationId";

            public const string Edit = "Edit";
            public const string Tab = "Tab";
        }

        private static class PersonPreferenceKey
        {
            public const string RecipientListSettings = "RecipientListSettings";
        }

        private static class PersonPropertyName
        {
            public static string Age = "Age";
            public static string AgeClassification = "AgeClassification";
            public static string BirthDate = "BirthDate";
            public static string Campus = "Campus";
            public static string Email = "Email";
            public static string Gender = "Gender";
            public static string Grade = "Grade";
            public static string IsDeceased = "IsDeceased";
        }

        private static class InteractionOperation
        {
            public const string Opened = "Opened";
            public const string Click = "Click";
        }

        private static class SankeyNode
        {
            public static SankeyDiagramNodeBag Sent => new SankeyDiagramNodeBag
            {
                Id = 1,
                Name = "Sent",
                Color = "--color-info-tint"
            };

            public static SankeyDiagramNodeBag Delivered => new SankeyDiagramNodeBag
            {
                Id = 2,
                Name = "Delivered",
                Color = "--color-info-shade"
            };

            public static SankeyDiagramNodeBag Failed => new SankeyDiagramNodeBag
            {
                Id = 3,
                Name = "Failed",
                Color = "--color-danger-tint"
            };

            public static SankeyDiagramNodeBag Pending => new SankeyDiagramNodeBag
            {
                Id = 4,
                Name = "Pending",
                Color = "--color-interface-medium"
            };

            public static SankeyDiagramNodeBag Cancelled => new SankeyDiagramNodeBag
            {
                Id = 5,
                Name = "Cancelled",
                Color = "--color-warning-tint"
            };

            public static SankeyDiagramNodeBag Opened => new SankeyDiagramNodeBag
            {
                Id = 6,
                Name = "Opened",
                Color = "--color-success-tint"
            };

            public static SankeyDiagramNodeBag Clicked => new SankeyDiagramNodeBag
            {
                Id = 7,
                Name = "Clicked",
                Color = "--color-success-shade"
            };

            public static SankeyDiagramNodeBag MarkedAsSpam => new SankeyDiagramNodeBag
            {
                Id = 8,
                Name = "Marked As Spam",
                Color = "--color-warning-shade"
            };

            public static SankeyDiagramNodeBag Unsubscribed => new SankeyDiagramNodeBag
            {
                Id = 9,
                Name = "Unsubscribed",
                Color = "--color-danger-shade"
            };
        }

        #endregion Keys & Constants

        #region Fields

        /// <summary>
        /// The backing field for the <see cref="CommunicationTypeByMediumEntityTypeId"/> property.
        /// </summary>
        private static readonly Lazy<Dictionary<int, CommunicationType>> _communicationTypeByMediumEntityTypeId = new Lazy<Dictionary<int, CommunicationType>>( () =>
        {
            var dict = new Dictionary<int, CommunicationType>();

            if ( EmailMediumEntityTypeId > 0 )
            {
                dict.Add( EmailMediumEntityTypeId, CommunicationType.Email );
            }

            if ( SmsMediumEntityTypeId > 0 )
            {
                dict.Add( SmsMediumEntityTypeId, CommunicationType.SMS );
            }

            if ( PushNotificationMediumEntityTypeId > 0 )
            {
                dict.Add( PushNotificationMediumEntityTypeId, CommunicationType.PushNotification );
            }

            return dict;
        } );

        /// <summary>
        /// The backing field for the <see cref="RecipientGridSettings"/> property.
        /// </summary>
        private CommunicationRecipientGridSettingsBag _recipientGridSettings;

        /// <summary>
        /// The backing field for the <see cref="RecipientGridAttributeColumns"/> property.
        /// </summary>
        private List<AttributeCache> _recipientGridAttributeColumns;

        #endregion Fields

        #region Properties

        /// <summary>
        /// Gets whether the current person can approve communications.
        /// </summary>
        private bool CanApprove => BlockCache.IsAuthorized( Authorization.APPROVE, GetCurrentPerson() );

        /// <summary>
        /// Gets the <see cref="Person"/> <see cref="EntityType"/> identifier.
        /// </summary>
        private int PersonEntityTypeId => EntityTypeCache.Get<Person>()?.Id ?? 0;

        /// <summary>
        /// Gets the <see cref="Rock.Model.Communication"/> <see cref="EntityType"/> identifier.
        /// </summary>
        private int CommunicationEntityTypeId => EntityTypeCache.Get<Rock.Model.Communication>()?.Id ?? 0;

        /// <summary>
        /// A lazy-loaded dictionary of <see cref="CommunicationType"/>s by medium <see cref="EntityType"/> identifiers.
        /// </summary>
        private static Dictionary<int, CommunicationType> CommunicationTypeByMediumEntityTypeId = _communicationTypeByMediumEntityTypeId.Value;

        /// <summary>
        /// Gets the friendly name for a <see cref="Rock.Model.Communication"/>.
        /// </summary>
        private string CommunicationFriendlyName => Rock.Model.Communication.FriendlyTypeName;

        /// <summary>
        /// Gets the block person preferences.
        /// </summary>
        private PersonPreferenceCollection BlockPersonPreferences => GetBlockPersonPreferences();

        /// <summary>
        /// Gets the <see cref="CommunicationRecipientGridSettingsBag"/> from block person preferences.
        /// </summary>
        public CommunicationRecipientGridSettingsBag RecipientGridSettings
        {
            get
            {
                if ( _recipientGridSettings == null )
                {
                    _recipientGridSettings = BlockPersonPreferences
                        .GetValue( PersonPreferenceKey.RecipientListSettings )
                        .FromJsonOrNull<CommunicationRecipientGridSettingsBag>() ?? new CommunicationRecipientGridSettingsBag();
                }

                return _recipientGridSettings;
            }
        }

        /// <summary>
        /// Gets the <see cref="Person"/> properties to add as columns to the recipient grid.
        /// </summary>
        public List<string> RecipientGridPropertyColumns => RecipientGridSettings.SelectedProperties;

        /// <summary>
        /// Gets the <see cref="Person"/> attributes to add as columns to the recipient grid.
        /// </summary>
        public List<AttributeCache> RecipientGridAttributeColumns
        {
            get
            {
                if ( _recipientGridAttributeColumns == null )
                {
                    if ( RecipientGridSettings.SelectedAttributes?.Any() != true )
                    {
                        // No attributes selected.
                        _recipientGridAttributeColumns = new List<AttributeCache>();
                    }
                    else
                    {
                        // Get the selected attributes from cache.
                        _recipientGridAttributeColumns = AttributeCache
                            .GetByEntityType( PersonEntityTypeId )
                            .Where( a => RecipientGridSettings.SelectedAttributes.Contains( a.Guid ) )
                            .OrderBy( a => a.Order )
                            .ThenBy( a => a.Name )
                            .ThenBy( a => a.Id )
                            .ToList();
                    }
                }

                return _recipientGridAttributeColumns;
            }
        }

        /// <summary>
        /// Gets the Communication entity key passed to the "Communication" or "CommunicationId" page parameter.
        /// </summary>
        private string CommunicationOrCommunicationIdPageParameter
        {
            get
            {
                var communicationPageParameter = PageParameter( PageParameterKey.Communication );

                if ( communicationPageParameter.IsNotNullOrWhiteSpace() )
                {
                    return communicationPageParameter;
                }
                else
                {
                    // Only allow the CommunicationId to contain an ID, but return it as a string so it can be used as an entity key.
                    return PageParameter( PageParameterKey.CommunicationId ).AsIntegerOrNull()?.ToString();
                }
            }
        }

        /// <summary>
        /// Gets the email medium <see cref="EntityType"/> identifier.
        /// </summary>
        private static int EmailMediumEntityTypeId => EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_EMAIL.AsGuid() )?.Id ?? 0;

        /// <summary>
        /// Gets the SMS medium <see cref="EntityType"/> identifier.
        /// </summary>
        private static int SmsMediumEntityTypeId => EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_SMS.AsGuid() )?.Id ?? 0;

        /// <summary>
        /// Gets the push notification medium <see cref="EntityType"/> identifier.
        /// </summary>
        private static int PushNotificationMediumEntityTypeId => EntityTypeCache.Get( Rock.SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Id ?? 0;

        #endregion Properties

        #region RockBlockType Implementation

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var communicationInfo = LoadCommunicationInfoFromPageParameter();
            var communication = communicationInfo?.Communication;

            var box = new CommunicationDetailInitializationBox
            {
                IsHidden = GetIsBlockHidden( communication )
            };

            if ( box.IsHidden )
            {
                // Return early if the block should be hidden.
                return box;
            }

            if ( !GetIsAuthorizedToView( communication ) )
            {
                // Return early if the current person is not authorized to view the communication.
                box.ErrorMessage = EditModeMessage.NotAuthorizedToView( CommunicationFriendlyName );
                return box;
            }

            var communicationName = communication.Name.IsNullOrWhiteSpace()
                ? $"Communication #{communication.Id}"
                : communication.Name;

            ResponseContext.SetPageTitle( communicationName );
            ResponseContext.SetBrowserTitle( communicationName );

            box.CommunicationDetail = GetCommunicationDetailBag( communicationInfo );

            box.MessagePreview = GetMessagePreview( communicationInfo );

            box.RecipientGridDefinition = GetRecipientGridBuilder( box.CommunicationDetail.Type ).BuildDefinition();
            box.RecipientGridOptions = GetRecipientGridOptions();

            box.Permissions = GetPermissions( communication );

            box.SecurityGrantToken = GetSecurityGrantToken( communication.Id );

            return box;
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            var communication = GetCommunicationQueryFromPageParameter()?.AsNoTracking().FirstOrDefault();
            if ( !GetIsAuthorizedToView( communication ) )
            {
                return string.Empty;
            }

            return GetSecurityGrantToken( communication.Id );
        }

        #endregion RockBlockType Implementation

        #region Block Actions

        /// <summary>
        /// Gets the communication analytics.
        /// </summary>
        /// <param name="bag">The information needed to get communication analytics.</param>
        /// <returns>An object containing information about the communication analytics.</returns>
        [BlockAction]
        public BlockActionResult GetCommunicationAnalytics( CommunicationAnalyticsRequestBag bag )
        {
            if ( bag == null )
            {
                return ActionBadRequest( "Unable to load analytics data." );
            }

            var response = new CommunicationAnalyticsResponseBag();

            // Get the communication and recipient info [for the filtered medium entity type].
            var communicationInfo = LoadCommunicationInfoFromPageParameter( bag.Type, shouldSkipLoadingAttachments: true );
            var communication = communicationInfo?.Communication;

            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            if ( !GetIsAuthorizedToView( communication ) )
            {
                return ActionUnauthorized( EditModeMessage.NotAuthorizedToView( CommunicationFriendlyName ) );
            }

            var deliveryBreakdown = GetCommunicationDetailBag( communicationInfo ).DeliveryBreakdown;
            response.DeliveryBreakdown = deliveryBreakdown;

            if ( deliveryBreakdown == null || deliveryBreakdown.CommunicationType == CommunicationType.SMS )
            {
                // Either we don't have a delivery breakdown (should never happen), OR the client is requesting
                // analytics data for SMS (which we don't track); exit early.
                return ActionOk( response );
            }

            // Get the common interaction data that will be used to drive most of the remaining visuals.
            var interactions = GetInteractions(
                communication.Id,
                communicationInfo.MediumEntityTypeFilterId
            );

            if ( !interactions.Any() )
            {
                response.ShowNoActivityMessage = true;
                return ActionOk( response );
            }

            // Group the interactions by operation (e.g. "Opened", "Click").
            var groupedInteractions = new GroupedInteractions( interactions );

            response.Kpis = GetCommunicationKpis(
                deliveryBreakdown.DeliveredCount,
                groupedInteractions,
                communicationInfo.RecipientActivities
            );

            response.UniqueInteractionsOverTime = GetUniqueInteractionsOverTime(
                communicationInfo,
                groupedInteractions,
                deliveryBreakdown.CommunicationType
            );

            response.ActivityFlow = GetActivityFlow( communicationInfo, groupedInteractions );

            response.UniqueOpensByGender = GetUniqueOpensByGender( groupedInteractions.UniqueOpens );
            response.UniqueOpensByAgeRange = GetUniqueOpensByAgeRange( groupedInteractions.UniqueOpens );

            var clients = GetClients( communication.Id );

            response.TopClients = clients.top;
            response.AllClients = clients.all;

            if ( deliveryBreakdown.CommunicationType != CommunicationType.Email )
            {
                // The remaining analytics are only relevant for email communications; exit early.
                return ActionOk( response );
            }

            response.AllLinksAnalytics = GetAllLinksAnalytics( deliveryBreakdown.DeliveredCount, groupedInteractions );

            return ActionOk( response );
        }

        /// <summary>
        /// Gets the recipient grid data.
        /// </summary>
        /// <returns>A bag containing the recipient grid data.</returns>
        [BlockAction]
        public BlockActionResult GetRecipientGridData()
        {
            var communicationQuery = GetCommunicationQueryFromPageParameter();
            if ( communicationQuery == null )
            {
                return ActionBadRequest();
            }

            // Eager-load related entities needed for authorization checks.
            var communicationWithRecipientRows = communicationQuery
                .AsNoTracking()
                .Select( c => new
                {
                    Communication = c,
                    RecipientRows = c.Recipients
                        .Where( r => r.PersonAlias != null )
                        .Select( r => new CommunicationRecipientRow
                        {
                            Person = r.PersonAlias.Person,
                            CampusName = r.PersonAlias.Person.PrimaryCampus != null
                                ? r.PersonAlias.Person.PrimaryCampus.Name
                                : string.Empty,

                            CommunicationRecipientId = r.Id,
                            Status = r.Status,
                            StatusNote = r.StatusNote,
                            MediumEntityTypeId = r.MediumEntityTypeId,

                            SendDateTime = r.SendDateTime,
                            DeliveredDateTime = r.DeliveredDateTime,
                            LastOpenedDateTime = r.OpenedDateTime,
                            UnsubscribeDateTime = r.UnsubscribeDateTime,
                            SpamComplaintDateTime = r.SpamComplaintDateTime,

                            LastActivityDateTime = (
                                    r.Status == CommunicationRecipientStatus.Failed
                                    || r.Status == CommunicationRecipientStatus.Cancelled
                                ) ? r.ModifiedDateTime : null
                        } )
                } )
                .AsEnumerable() // Materialize the query.
                .Select( a => new CommunicationWithRecipientRows
                {
                    Communication = a.Communication,
                    RecipientRows = a.RecipientRows.ToList()
                } )
                .FirstOrDefault();

            var communication = communicationWithRecipientRows?.Communication;
            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            if ( !GetIsAuthorizedToView( communication ) )
            {
                return ActionUnauthorized( EditModeMessage.NotAuthorizedToView( CommunicationFriendlyName ) );
            }

            // Load interactions so we can supplement each row with this data.
            var groupedInteractions = new GroupedInteractions( GetInteractions( communication.Id ) );

            // A local function to ensure each row reflects the recipient's most recent activity datetime.
            void TrySetLastActivityDateTime( CommunicationRecipientRow row, DateTime? activityDateTime )
            {
                if ( !activityDateTime.HasValue )
                {
                    return;
                }

                if ( !row.LastActivityDateTime.HasValue || activityDateTime > row.LastActivityDateTime )
                {
                    row.LastActivityDateTime = activityDateTime;
                }
            }

            foreach ( var row in communicationWithRecipientRows.RecipientRows )
            {
                // Set the last activity datetime based on info available directly on the communication recipient record.
                TrySetLastActivityDateTime( row, row.SendDateTime );
                TrySetLastActivityDateTime( row, row.LastOpenedDateTime );
                TrySetLastActivityDateTime( row, row.UnsubscribeDateTime );
                TrySetLastActivityDateTime( row, row.SpamComplaintDateTime );

                // Does this recipients have any "Opened" interactions?
                if ( groupedInteractions.AllOpensByRecipientId.TryGetValue( row.CommunicationRecipientId, out var opens ) && opens.Any() )
                {
                    row.OpensCount = opens.Count;
                    row.LastOpenedDateTime = opens.Last().InteractionDateTime;
                    TrySetLastActivityDateTime( row, row.LastOpenedDateTime );
                }

                // Does this recipient have any "Click" interactions?
                if ( groupedInteractions.AllClicksByRecipientId.TryGetValue( row.CommunicationRecipientId, out var clicks ) && clicks.Any() )
                {
                    row.ClicksCount = clicks.Count;
                    row.LastClickedDateTime = clicks.Last().InteractionDateTime;
                    TrySetLastActivityDateTime( row, row.LastClickedDateTime );
                }
            }

            var people = communicationWithRecipientRows.RecipientRows
                .Select( r => r.Person )
                .ToList();

            if ( people.Any() && RecipientGridAttributeColumns.Any() )
            {
                var gridAttributeIds = RecipientGridAttributeColumns
                    .Select( a => a.Id )
                    .ToList();

                Helper.LoadFilteredAttributes( people, RockContext, a => gridAttributeIds.Contains( a.Id ) );
            }

            var builder = GetRecipientGridBuilder( ( CommunicationType ) communication.CommunicationType );
            var gridDataBag = builder.Build(
                communicationWithRecipientRows.RecipientRows
                    .OrderByDescending( r => r.LastActivityDateTime )
                    .ThenBy( r => r.Person.LastName )
                    .ThenBy( r => r.Person.FirstName )
            );

            return ActionOk( new CommunicationRecipientGridDataBag
            {
                GridData = gridDataBag,
                GridDefinition = builder.BuildDefinition()
            } );
        }

        /// <summary>
        /// Approves the current communication.
        /// </summary>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult ApproveCommunication()
        {
            var communication = GetCommunicationQueryFromPageParameter()?.FirstOrDefault();
            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            string outcomeMessage;

            if ( communication.Status == CommunicationStatus.PendingApproval )
            {
                if ( !CanApprove )
                {
                    return ActionUnauthorized( $"Sorry, you are not authorized to approve this {CommunicationFriendlyName}." );
                }

                communication.Status = CommunicationStatus.Approved;
                communication.ReviewedDateTime = RockDateTime.Now;
                communication.ReviewerPersonAliasId = GetCurrentPerson()?.PrimaryAliasId;

                RockContext.SaveChanges();

                outcomeMessage = $"The {CommunicationFriendlyName} has been approved.";
            }
            else
            {
                outcomeMessage = $"This {CommunicationFriendlyName} is already {communication.Status.ConvertToString()}.";
            }

            // Redirect back to the same page so we can refresh all of the communication details.
            var pageParams = GetPageParamsForReload( communication.Id );

            return ActionOk(
                new CommunicationActionResponseBag
                {
                    OutcomeMessage = outcomeMessage,
                    RedirectUrl = this.GetCurrentPageUrl( pageParams )
                }
            );
        }

        /// <summary>
        /// Denies the current communication.
        /// </summary>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult DenyCommunication()
        {
            var communication = GetCommunicationQueryFromPageParameter()?.FirstOrDefault();
            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            string outcomeMessage;

            if ( communication.Status == CommunicationStatus.PendingApproval )
            {
                if ( !CanApprove )
                {
                    return ActionUnauthorized( $"Sorry, you are not authorized to approve or deny this {CommunicationFriendlyName}." );
                }

                communication.Status = CommunicationStatus.Denied;
                communication.ReviewedDateTime = RockDateTime.Now;
                communication.ReviewerPersonAliasId = GetCurrentPerson()?.PrimaryAliasId;

                RockContext.SaveChanges();

                outcomeMessage = $"The {CommunicationFriendlyName} has been denied.";
            }
            else
            {
                outcomeMessage = $"This {CommunicationFriendlyName} is already {communication.Status.ConvertToString()}.";
            }

            // Redirect back to the same page so we can refresh all of the communication details.
            var pageParams = GetPageParamsForReload( communication.Id );
            pageParams.Remove( PageParameterKey.Tab );

            return ActionOk(
                new CommunicationActionResponseBag
                {
                    OutcomeMessage = outcomeMessage,
                    RedirectUrl = this.GetCurrentPageUrl( pageParams )
                }
            );
        }

        /// <summary>
        /// Edits the current communication.
        /// </summary>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult EditCommunication()
        {
            var communication = GetCommunicationQueryFromPageParameter()?.FirstOrDefault();
            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            if ( !CanApprove )
            {
                return ActionUnauthorized( $"Sorry, you are not authorized to edit this {CommunicationFriendlyName}." );
            }

            // In the case of an edit, we'll have either an outcome message OR a redirect URL; not both.
            string outcomeMessage = null;
            string redirectUrl = null;

            if ( communication.Status == CommunicationStatus.PendingApproval )
            {
                // Redirect back to the same page with the Edit parameter.
                var pageParams = GetPageParamsForReload( communication.Id );
                pageParams.AddOrReplace( PageParameterKey.Edit, "true" );
                pageParams.Remove( PageParameterKey.Tab );

                redirectUrl = this.GetCurrentPageUrl( pageParams );
            }
            else
            {
                outcomeMessage = $"Sorry, this {CommunicationFriendlyName} is not able to be edited.";
            }

            return ActionOk(
                new CommunicationActionResponseBag
                {
                    OutcomeMessage = outcomeMessage,
                    RedirectUrl = redirectUrl
                }
            );
        }

        /// <summary>
        /// Cancels the current communication.
        /// </summary>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult CancelCommunication()
        {
            var communication = GetCommunicationQueryFromPageParameter()?
                .Include( c => c.Recipients )
                .FirstOrDefault();

            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            string outcomeMessage;
            var shouldRemoveTabParam = false;

            if ( communication.Status == CommunicationStatus.PendingApproval
                && !communication.IsAuthorized( Authorization.EDIT, GetCurrentPerson() ) )
            {
                return ActionUnauthorized( $"Sorry, you are not authorized to cancel this {CommunicationFriendlyName}." );
            }
            else if ( communication.Status == CommunicationStatus.Approved
                && !communication.Recipients.Any( r => r.Status == CommunicationRecipientStatus.Pending ) )
            {
                outcomeMessage = $"Sorry, unable to cancel this {CommunicationFriendlyName} as it has already been delivered to all recipients.";
            }
            else if ( communication.Status == CommunicationStatus.Approved
                || communication.Status == CommunicationStatus.PendingApproval )
            {
                // TODO (Jason): Shouldn't `Approved` comms also require EDIT auth to cancel (Even though the legacy block didn't require this)?
                // https://app.asana.com/1/20866866924293/project/1208321217019996/task/1210655530351448?focus=true
                var deliveredRecipientCount = communication.Recipients
                    .Count( r =>
                        r.Status == CommunicationRecipientStatus.Sending
                        || r.Status == CommunicationRecipientStatus.Delivered
                        || r.Status == CommunicationRecipientStatus.Opened
                    );

                if ( deliveredRecipientCount == 0 )
                {
                    communication.Status = CommunicationStatus.Draft;
                    RockContext.SaveChanges();

                    outcomeMessage = $"This {CommunicationFriendlyName} has successfully been cancelled without any recipients receiving the {CommunicationFriendlyName}.";

                    // Remove the Tab param as this block will no longer be shown upon redirect.
                    shouldRemoveTabParam = true;
                }
                else
                {
                    var recipientsToCancelQuery = new CommunicationRecipientService( RockContext )
                        .Queryable()
                        .Where( r =>
                            r.CommunicationId == communication.Id
                            && r.Status == CommunicationRecipientStatus.Pending
                        );

                    var cancelledRecipientCount = RockContext.BulkUpdate( recipientsToCancelQuery, r =>
                        new CommunicationRecipient
                        {
                            Status = CommunicationRecipientStatus.Cancelled
                        }
                    );

                    outcomeMessage = $"This {CommunicationFriendlyName} has been cancelled, however the {CommunicationFriendlyName} was delivered to {deliveredRecipientCount:N0} recipients ({cancelledRecipientCount:N0} recipients were cancelled).";
                }
            }
            else
            {
                outcomeMessage = $"This {CommunicationFriendlyName} has already been cancelled.";
            }

            // Redirect back to the same page so we can refresh all of the communication details.
            var pageParams = GetPageParamsForReload( communication.Id );

            if ( shouldRemoveTabParam )
            {
                pageParams.Remove( PageParameterKey.Tab );
            }

            return ActionOk(
                new CommunicationActionResponseBag
                {
                    OutcomeMessage = outcomeMessage,
                    RedirectUrl = this.GetCurrentPageUrl( pageParams )
                }
            );
        }

        /// <summary>
        /// Duplicates the current communication.
        /// </summary>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult DuplicateCommunication()
        {
            var communication = GetCommunicationQueryFromPageParameter()?.AsNoTracking().FirstOrDefault();
            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            if ( communication.Status != CommunicationStatus.Approved )
            {
                return ActionBadRequest( $"Unable to duplicate unapproved {CommunicationFriendlyName}." );
            }

            if ( !GetIsAuthorizedToView( communication ) )
            {
                return ActionUnauthorized( EditModeMessage.NotAuthorizedToView( CommunicationFriendlyName ) );
            }

            var communicationService = new CommunicationService( RockContext );

            var newCommunication = communicationService.Copy( communication.Id, GetCurrentPerson()?.PrimaryAliasId );
            if ( newCommunication == null )
            {
                return ActionInternalServerError( $"Unable to duplicate the {CommunicationFriendlyName}." );
            }

            communicationService.Add( newCommunication );
            RockContext.SaveChanges();

            // Redirect to the new communication.
            var pageParams = GetPageParamsForReload( newCommunication.Id );
            pageParams.Remove( PageParameterKey.Tab );

            return ActionOk(
                new CommunicationRedirectBag
                {
                    CommunicationUrl = this.GetCurrentPageUrl( pageParams )
                }
            );
        }

        /// <summary>
        /// Creates a personal template based on the current communication.
        /// </summary>
        /// <param name="bag">The information needed to create a personal template.</param>
        /// <returns>A bag containing information about the outcome of the request.</returns>
        [BlockAction]
        public BlockActionResult CreatePersonalTemplate( CreatePersonalTemplateRequestBag bag )
        {
            if ( ( bag?.Name ).IsNullOrWhiteSpace() )
            {
                return ActionBadRequest( "Template Name is required." );
            }

            var communicationQuery = GetCommunicationQueryFromPageParameter();
            if ( communicationQuery == null )
            {
                return ActionBadRequest();
            }

            // Eager-load attachments so we can make copies of those when creating the new template.
            var communication = communicationQuery
                .Include( c => c.Attachments )
                .AsNoTracking()
                .FirstOrDefault();

            if ( communication == null )
            {
                return ActionBadRequest( $"Unable to find {CommunicationFriendlyName}." );
            }

            if ( !GetIsAuthorizedToView( communication ) )
            {
                return ActionUnauthorized( EditModeMessage.NotAuthorizedToView( CommunicationFriendlyName ) );
            }

            int? categoryId = null;
            if ( bag.CategoryGuid.HasValue )
            {
                categoryId = CategoryCache.GetId( bag.CategoryGuid.Value );
            }

            var currentPerson = GetCurrentPerson();

            var template = new CommunicationTemplate
            {
                SenderPersonAliasId = currentPerson?.PrimaryAliasId,
                Name = bag.Name,
                CategoryId = categoryId,
                Description = bag.Description,
                Subject = communication.Subject,
                FromName = communication.FromName,
                FromEmail = communication.FromEmail,
                ReplyToEmail = communication.ReplyToEmail,
                CCEmails = communication.CCEmails,
                BCCEmails = communication.BCCEmails,
                Message = "{% raw %}" + communication.Message + "{% endraw %}",
                MessageMetaData = communication.MessageMetaData,
                SmsFromSystemPhoneNumberId = communication.SmsFromSystemPhoneNumberId,
                SMSMessage = communication.SMSMessage,
                PushTitle = communication.PushTitle,
                PushMessage = communication.PushMessage,
                PushSound = communication.PushSound,
                Version = communication.CommunicationTemplate?.Version ?? CommunicationTemplateVersion.Beta
            };

            foreach ( var attachment in communication.Attachments.ToList() )
            {
                var newAttachment = new CommunicationTemplateAttachment
                {
                    BinaryFileId = attachment.BinaryFileId,
                    CommunicationType = attachment.CommunicationType
                };
                template.Attachments.Add( newAttachment );
            }

            var templateService = new CommunicationTemplateService( RockContext );
            templateService.Add( template );
            RockContext.SaveChanges();

            template = templateService.Get( template.Id );
            if ( template != null )
            {
                template.MakePrivate( Authorization.VIEW, currentPerson, RockContext );
                template.MakePrivate( Authorization.EDIT, currentPerson, RockContext );
                template.MakePrivate( Authorization.ADMINISTRATE, currentPerson, RockContext );

                var groupService = new GroupService( RockContext );
                var communicationAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_COMMUNICATION_ADMINISTRATORS.AsGuid() );
                if ( communicationAdministrators != null )
                {
                    template.AllowSecurityRole( Authorization.VIEW, communicationAdministrators, RockContext );
                    template.AllowSecurityRole( Authorization.EDIT, communicationAdministrators, RockContext );
                    template.AllowSecurityRole( Authorization.ADMINISTRATE, communicationAdministrators, RockContext );
                }

                var rockAdministrators = groupService.Get( Rock.SystemGuid.Group.GROUP_ADMINISTRATORS.AsGuid() );
                if ( rockAdministrators != null )
                {
                    template.AllowSecurityRole( Authorization.VIEW, rockAdministrators, RockContext );
                    template.AllowSecurityRole( Authorization.EDIT, rockAdministrators, RockContext );
                    template.AllowSecurityRole( Authorization.ADMINISTRATE, rockAdministrators, RockContext );
                }
            }

            return ActionOk();
        }

        #endregion Block Actions

        #region Private Methods

        /// <summary>
        /// Gets an <see cref="IQueryable{Rock.Model.Communication}"/> based on the page parameter.
        /// </summary>
        /// <returns>An <see cref="IQueryable{Rock.Model.Communication}"/> based on the page parameter.</returns>
        /// <remarks>
        /// Entities needed for authorization checks will be eager-loaded.
        /// </remarks>
        private IQueryable<Rock.Model.Communication> GetCommunicationQueryFromPageParameter()
        {
            // Check page parameter for existing communication.
            var communicationKey = CommunicationOrCommunicationIdPageParameter;
            if ( communicationKey.IsNullOrWhiteSpace() )
            {
                return null;
            }

            return new CommunicationService( RockContext )
                .GetQueryableByKey( communicationKey, !PageCache.Layout.Site.DisablePredictableIds )
                .Include( c => c.CommunicationTemplate )
                .Include( c => c.SystemCommunication );
        }

        /// <summary>
        /// Loads an existing <see cref="Rock.Model.Communication"/> - along with its supporting info - based on the
        /// page parameter.
        /// </summary>
        /// <param name="communicationTypeOverride">
        /// The optional <see cref="CommunicationType"/> for which to get the <see cref="RecipientCountBreakdown"/>.
        /// </param>
        /// <param name="shouldSkipLoadingAttachments">
        /// Whether the eager-loading of this communication's <see cref="CommunicationAttachment"/>s should be skipped.
        /// </param>
        /// <returns>
        /// A <see cref="CommunicationInfo"/> or <see langword="null"/> if unable to find the
        /// <see cref="Rock.Model.Communication"/>.
        /// </returns>
        private CommunicationInfo LoadCommunicationInfoFromPageParameter( CommunicationType? communicationTypeOverride = null, bool shouldSkipLoadingAttachments = false )
        {
            var communicationQuery = GetCommunicationQueryFromPageParameter();
            if ( communicationQuery == null )
            {
                return null;
            }

            if ( !shouldSkipLoadingAttachments )
            {
                communicationQuery = communicationQuery
                    .Include( c => c.Attachments.Select( a => a.BinaryFile ) );
            }

            var communicationFlowInstanceCommunicationQuery = new CommunicationFlowInstanceCommunicationService( RockContext )
                .Queryable();

            return communicationQuery
                .AsNoTracking()
                .Select( c => new
                {
                    Communication = c,

                    CommunicationFlowName = communicationFlowInstanceCommunicationQuery
                        .Where( f => f.CommunicationId == c.Id )
                        .Select( f => f.CommunicationFlowInstance.CommunicationFlow.Name )
                        .FirstOrDefault(),

                    CreatedByPersonNickName = c.CreatedByPersonAlias != null ? c.CreatedByPersonAlias.Person.NickName : string.Empty,
                    CreatedByPersonLastName = c.CreatedByPersonAlias != null ? c.CreatedByPersonAlias.Person.LastName : string.Empty,
                    CreatedByPersonSuffixValueId = c.CreatedByPersonAlias != null ? c.CreatedByPersonAlias.Person.SuffixValueId : null,
                    CreatedByPersonRecordTypeValueId = c.CreatedByPersonAlias != null ? c.CreatedByPersonAlias.Person.RecordTypeValueId : null,

                    ReviewerPersonNickName = c.ReviewerPersonAlias != null ? c.ReviewerPersonAlias.Person.NickName : string.Empty,
                    ReviewerPersonLastName = c.ReviewerPersonAlias != null ? c.ReviewerPersonAlias.Person.LastName : string.Empty,
                    ReviewerPersonSuffixValueId = c.ReviewerPersonAlias != null ? c.ReviewerPersonAlias.Person.SuffixValueId : null,
                    ReviewerPersonRecordTypeValueId = c.ReviewerPersonAlias != null ? c.ReviewerPersonAlias.Person.RecordTypeValueId : null,

                    FromSystemPhoneNumberName = c.SmsFromSystemPhoneNumber != null ? c.SmsFromSystemPhoneNumber.Name : string.Empty,
                    FromSystemPhoneNumber = c.SmsFromSystemPhoneNumber != null ? c.SmsFromSystemPhoneNumber.Number : string.Empty,

                    CommunicationListName = c.ListGroup != null ? c.ListGroup.Name : string.Empty,

                    TotalRecipientCount = c.Recipients.Count(),

                    // TODO (Jason): When Communications are duplicated, their recipients aren't assigned a MediumEntityTypeId.
                    // This breaks the following aggregations.
                    // https://app.asana.com/1/20866866924293/project/1208321217019996/task/1210655530351454?focus=true
                    Counts = c.Recipients
                        .GroupBy( r => new { r.MediumEntityTypeId, r.Status } )
                        .Select( g => new
                        {
                            g.Key.MediumEntityTypeId,
                            g.Key.Status,
                            Count = g.Count()
                        } ),

                    SpamComplaints = c.Recipients
                        .Where( r =>
                            r.SpamComplaintDateTime.HasValue
                        )
                        .Select( r => new RecipientActivity
                        {
                            CommunicationRecipientId = r.Id,
                            MediumEntityTypeId = r.MediumEntityTypeId,
                            ActivityType = RecipientActivityType.SpamComplaint,
                            ActivityDateTime = r.SpamComplaintDateTime
                        } ),

                    Unsubscribes = c.Recipients
                        .Where( r =>
                            r.UnsubscribeDateTime.HasValue
                        )
                        .Select( r => new RecipientActivity
                        {
                            CommunicationRecipientId = r.Id,
                            MediumEntityTypeId = r.MediumEntityTypeId,
                            ActivityType = RecipientActivityType.Unsubscribe,
                            ActivityDateTime = r.UnsubscribeDateTime
                        } )
                } )
                .AsEnumerable() // Materialize the query; we'll perform the remaining aggregations in-memory.
                .Select( a =>
                {
                    var communication = a.Communication;
                    var communicationType = communicationTypeOverride ?? ( CommunicationType ) communication.CommunicationType;

                    // If recipient preference, show only email counts; the individual can choose to show SMS later.
                    if ( communicationType == CommunicationType.RecipientPreference )
                    {
                        communicationType = CommunicationType.Email;
                    }

                    // Translate the communication type to the corresponding medium entity type filter ID.
                    int? mediumEntityTypeFilterId = null;
                    if ( communicationType == CommunicationType.Email )
                    {
                        mediumEntityTypeFilterId = EmailMediumEntityTypeId;
                    }
                    else if ( communicationType == CommunicationType.SMS )
                    {
                        mediumEntityTypeFilterId = SmsMediumEntityTypeId;
                    }
                    else
                    {
                        // No need to filter other communication/medium entity types, as all analytics data will already
                        // relate to only those types (e.g. push notifications).
                    }

                    var communicationInfo = new CommunicationInfo
                    {
                        Communication = a.Communication,
                        CreatedByPersonNickName = a.CreatedByPersonNickName,
                        CreatedByPersonLastName = a.CreatedByPersonLastName,
                        CreatedByPersonSuffixValueId = a.CreatedByPersonSuffixValueId,
                        CreatedByPersonRecordTypeValueId = a.CreatedByPersonRecordTypeValueId,
                        ReviewerPersonNickName = a.ReviewerPersonNickName,
                        ReviewerPersonLastName = a.ReviewerPersonLastName,
                        ReviewerPersonSuffixValueId = a.ReviewerPersonSuffixValueId,
                        ReviewerPersonRecordTypeValueId = a.ReviewerPersonRecordTypeValueId,
                        FromSystemPhoneNumberName = a.FromSystemPhoneNumberName,
                        FromSystemPhoneNumber = a.FromSystemPhoneNumber,
                        CommunicationFlowName = a.CommunicationFlowName,
                        CommunicationListName = a.CommunicationListName,
                        TotalRecipientCount = a.TotalRecipientCount,
                        MediumEntityTypeFilterId = mediumEntityTypeFilterId
                    };

                    var countsByMediumAndStatus = a.Counts
                        .ToDictionary( k => (k.MediumEntityTypeId, k.Status), v => v.Count );

                    // A local function to get the count of recipients for a given status.
                    int GetCount( CommunicationRecipientStatus s )
                    {
                        if ( mediumEntityTypeFilterId.HasValue )
                        {
                            return countsByMediumAndStatus.TryGetValue( (mediumEntityTypeFilterId.Value, s), out var count ) ? count : 0;
                        }

                        return countsByMediumAndStatus
                            .Where( kvp => kvp.Key.Status == s )
                            .Sum( kvp => kvp.Value );
                    }

                    var recipientCount = mediumEntityTypeFilterId.HasValue
                        ? countsByMediumAndStatus.Where( kvp => kvp.Key.MediumEntityTypeId == mediumEntityTypeFilterId.Value ).Sum( kvp => kvp.Value )
                        : a.TotalRecipientCount;

                    communicationInfo.RecipientCountBreakdown = new RecipientCountBreakdown
                    {
                        CommunicationType = communicationType,
                        RecipientCount = recipientCount,
                        PendingCount = GetCount( CommunicationRecipientStatus.Pending ) + GetCount( CommunicationRecipientStatus.Sending ),
                        DeliveredCount = GetCount( CommunicationRecipientStatus.Delivered ) + GetCount( CommunicationRecipientStatus.Opened ),
                        FailedCount = GetCount( CommunicationRecipientStatus.Failed ),
                        CancelledCount = GetCount( CommunicationRecipientStatus.Cancelled )
                    };

                    communicationInfo.RecipientActivities = new List<RecipientActivity>();

                    communicationInfo.RecipientActivities.AddRange(
                        a.SpamComplaints.Where( c =>
                            !mediumEntityTypeFilterId.HasValue
                            || c.MediumEntityTypeId == mediumEntityTypeFilterId
                        )
                    );

                    communicationInfo.RecipientActivities.AddRange(
                        a.Unsubscribes.Where( u =>
                            !mediumEntityTypeFilterId.HasValue
                            || u.MediumEntityTypeId == mediumEntityTypeFilterId
                        )
                    );

                    return communicationInfo;
                } )
                .FirstOrDefault();
        }

        /// <summary>
        /// Gets a value indicating whether the block should be hidden.
        /// </summary>
        /// <param name="communication">
        /// The <see cref="Rock.Model.Communication"/> whose <see cref="CommunicationStatus"/> will dictate whether to
        /// hide the block.
        /// </param>
        /// <returns>Whether the block should be hidden.</returns>
        private bool GetIsBlockHidden( Rock.Model.Communication communication )
        {
            return communication == null
                || communication.Status == CommunicationStatus.Transient
                || communication.Status == CommunicationStatus.Draft
                || communication.Status == CommunicationStatus.Denied
                || (
                    communication.Status == CommunicationStatus.PendingApproval
                    && PageParameter( PageParameterKey.Edit ).AsBoolean()
                    && CanApprove
                );
        }

        /// <summary>
        /// Gets whether the current person is authorized to view the communication.
        /// </summary>
        /// <param name="communication">The <see cref="Rock.Model.Communication"/> for which to check authorization.</param>
        /// <returns>Whether the current person is authorized to view the communication.</returns>
        private bool GetIsAuthorizedToView( Rock.Model.Communication communication )
        {
            return communication?.IsAuthorized( Authorization.VIEW, GetCurrentPerson() ) ?? false;
        }

        /// <summary>
        /// Gets a <see cref="CommunicationDetailBag"/> that contains the details of the communication.
        /// </summary>
        /// <param name="communicationInfo">
        /// The <see cref="CommunicationInfo"/> from which to get the <see cref="CommunicationDetailBag"/>.
        /// </param>
        /// <returns>A <see cref="CommunicationDetailBag"/> that contains the details of the communication.</returns>
        private CommunicationDetailBag GetCommunicationDetailBag( CommunicationInfo communicationInfo )
        {
            var communication = communicationInfo?.Communication;
            if ( communication == null )
            {
                return null;
            }

            // Prefer - in this order - Name, Subject, Push Title.
            var name = !string.IsNullOrWhiteSpace( communication.Name ) ? communication.Name
                : !string.IsNullOrWhiteSpace( communication.Subject ) ? communication.Subject : communication.PushTitle;

            CommunicationDeliveryBreakdownBag deliveryBreakdownBag = null;

            var recipientCountBreakdown = communicationInfo.RecipientCountBreakdown;
            if ( recipientCountBreakdown != null )
            {
                decimal GetPercentage( int count )
                {
                    if ( recipientCountBreakdown.RecipientCount <= 0 )
                    {
                        return 0;
                    }

                    return Math.Round( count / ( decimal ) recipientCountBreakdown.RecipientCount * 100, 1, MidpointRounding.AwayFromZero );
                }

                deliveryBreakdownBag = new CommunicationDeliveryBreakdownBag
                {
                    CommunicationType = recipientCountBreakdown.CommunicationType,
                    RecipientCount = recipientCountBreakdown.RecipientCount,
                    PendingCount = recipientCountBreakdown.PendingCount,
                    PendingPercentage = GetPercentage( recipientCountBreakdown.PendingCount ),
                    DeliveredCount = recipientCountBreakdown.DeliveredCount,
                    DeliveredPercentage = GetPercentage( recipientCountBreakdown.DeliveredCount ),
                    FailedCount = recipientCountBreakdown.FailedCount,
                    FailedPercentage = GetPercentage( recipientCountBreakdown.FailedCount ),
                    CancelledCount = recipientCountBreakdown.CancelledCount,
                    CancelledPercentage = GetPercentage( recipientCountBreakdown.CancelledCount )
                };
            }

            var inferredStatus = ( InferredCommunicationStatus ) communication.Status;
            if ( inferredStatus == InferredCommunicationStatus.Approved && recipientCountBreakdown != null )
            {
                if ( recipientCountBreakdown.PendingCount == 0 )
                {
                    inferredStatus = InferredCommunicationStatus.Sent;
                }
                else if ( recipientCountBreakdown.PendingCount > 0
                    && recipientCountBreakdown.RecipientCount > recipientCountBreakdown.PendingCount )
                {
                    inferredStatus = InferredCommunicationStatus.Sending;
                }
            }

            return new CommunicationDetailBag
            {
                Name = name,
                Type = ( CommunicationType ) communication.CommunicationType,
                InferredStatus = inferredStatus,
                FutureSendDateTime = communication.FutureSendDateTime,
                SendDateTime = communication.SendDateTime,
                TotalRecipientCount = communicationInfo.TotalRecipientCount,
                CommunicationFlowName = communicationInfo.CommunicationFlowName,
                Topic = DefinedValueCache.GetValue( communication.CommunicationTopicValueId ),
                IsBulk = communication.IsBulkCommunication,
                DeliveryBreakdown = deliveryBreakdownBag,
            };
        }

        /// <summary>
        /// Gets a list of <see cref="InteractionInfo"/>s for the given communication.
        /// </summary>
        /// <param name="communicationId">The <see cref="Rock.Model.Communication"/> identifier.</param>
        /// <param name="mediumEntityTypeFilterId">
        /// The medium <see cref="EntityType"/> identifier filter, if interactions should be restricted to a specific medium.
        /// </param>
        /// <returns>A list of <see cref="InteractionInfo"/>s.</returns>
        private List<InteractionInfo> GetInteractions( int communicationId, int? mediumEntityTypeFilterId = null )
        {
            var recipientQuery = new CommunicationRecipientService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( r =>
                    r.CommunicationId == communicationId
                    && (
                        !mediumEntityTypeFilterId.HasValue
                        || r.MediumEntityTypeId == mediumEntityTypeFilterId
                    )
                );

            return GetInteractionsQuery( communicationId )
                .Join(
                    recipientQuery,
                    i => i.EntityId,
                    r => r.Id,
                    ( i, r ) => new InteractionInfo
                    {
                        InteractionDateTime = i.InteractionDateTime,
                        Operation = i.Operation,
                        InteractionData = i.InteractionData,
                        CommunicationRecipientId = i.EntityId,
                        PersonGender = r.PersonAlias != null ? r.PersonAlias.Person.Gender : Gender.Unknown,
                        PersonAge = r.PersonAlias != null ? r.PersonAlias.Person.Age : ( int? ) null,
                    }
                )
                .ToList();
        }

        /// <summary>
        /// Gets a Queryable of <see cref="Interaction"/>s for the given communication.
        /// </summary>
        /// <param name="communicationId">The <see cref="Rock.Model.Communication"/> identifier.</param>
        /// <returns>a Queryable of <see cref="Interaction"/>s.</returns>
        private IQueryable<Interaction> GetInteractionsQuery( int communicationId )
        {
            var interactionChannelId = InteractionChannelCache.Get( Rock.SystemGuid.InteractionChannel.COMMUNICATION.AsGuid() )?.Id;

            return new InteractionService( RockContext )
                .Queryable()
                .AsNoTracking()
                .Where( i =>
                    i.InteractionComponent.InteractionChannelId == interactionChannelId
                    && i.InteractionComponent.EntityId == communicationId
                );
        }

        /// <summary>
        /// Gets the KPIs for this communication.
        /// </summary>
        /// <param name="deliveredRecipientCount">The count of recipients to whom this communication was successfully delivered.</param>
        /// <param name="groupedInteractions">The grouped interaction data from which to derive KPIs.</param> 
        /// <param name="recipientActivities">The list of (non-interaction) recipient activities from which to derive KPIs.</param>
        /// <returns>A <see cref="CommunicationKpisBag"/> containing the KPIs for this communication.</returns>
        private CommunicationKpisBag GetCommunicationKpis( int deliveredRecipientCount, GroupedInteractions groupedInteractions, List<RecipientActivity> recipientActivities )
        {
            var markedAsSpamCount = recipientActivities
                .Where( a => a.ActivityType == RecipientActivityType.SpamComplaint )
                .Count();

            var unsubscribedCount = recipientActivities
                .Where( a => a.ActivityType == RecipientActivityType.Unsubscribe )
                .Count();

            var kpis = new CommunicationKpisBag
            {
                TotalOpensCount = groupedInteractions.TotalOpensCount,
                UniqueOpensCount = groupedInteractions.UniqueOpensCount,
                TotalClicksCount = groupedInteractions.TotalClicksCount,
                UniqueClicksCount = groupedInteractions.UniqueClicksCount,
                TotalMarkedAsSpamCount = markedAsSpamCount,
                TotalUnsubscribesCount = unsubscribedCount,
            };

            decimal GetPercentage( int count, int divisor )
            {
                if ( divisor <= 0 )
                {
                    return 0;
                }

                return Math.Round( count / ( decimal ) divisor * 100, 0, MidpointRounding.AwayFromZero );
            }

            if ( deliveredRecipientCount > 0 )
            {
                kpis.OpenRate = GetPercentage( groupedInteractions.UniqueOpensCount, deliveredRecipientCount );
                kpis.MarkedAsSpamRate = GetPercentage( markedAsSpamCount, deliveredRecipientCount );
                kpis.UnsubscribeRate = GetPercentage( unsubscribedCount, deliveredRecipientCount );
            }

            if ( groupedInteractions.UniqueOpensCount > 0 )
            {
                kpis.ClickThroughRate = GetPercentage( groupedInteractions.UniqueClicksCount, groupedInteractions.UniqueOpensCount );
            }

            return kpis;
        }

        /// <summary>
        /// Gets the unique interactions over time for this communication.
        /// </summary>
        /// <param name="communicationInfo">The <see cref="CommunicationInfo"/>.</param>
        /// <param name="groupedInteractions">The <see cref="GroupedInteractions"/>.</param>
        /// <param name="analyticsCommunicationType">The <see cref="CommunicationType"/> for which we're collecting analytics data.</param>
        /// <returns>A list of <see cref="ChartNumericDataPointBag"/>s representing the unique interactions over time.
        private List<ChartNumericDataPointBag> GetUniqueInteractionsOverTime(
            CommunicationInfo communicationInfo,
            GroupedInteractions groupedInteractions,
            CommunicationType analyticsCommunicationType )
        {
            var uniqueInteractionsOverTime = new List<ChartNumericDataPointBag>();

            var sendDateTime = communicationInfo.Communication.SendDateTime;
            if ( !sendDateTime.HasValue )
            {
                // This communication hasn't been sent yet.
                return uniqueInteractionsOverTime;
            }

            var isEmail = analyticsCommunicationType == CommunicationType.Email;

            var openCountsByDate = groupedInteractions.UniqueOpens
                .GroupBy( o => o.InteractionDateTime.Date )
                .ToDictionary( g => g.Key, g => g.Count() );

            var clickCountsByDate = isEmail
                ? groupedInteractions.UniqueClicks
                    .GroupBy( o => o.InteractionDateTime.Date )
                    .ToDictionary( g => g.Key, g => g.Count() )
                : new Dictionary<DateTime, int>();

            var spamComplaintsByDate = isEmail
                ? communicationInfo.RecipientActivities
                    .Where( a =>
                        a.ActivityDateTime.HasValue
                        && a.ActivityType == RecipientActivityType.SpamComplaint
                    )
                    .GroupBy( a => a.ActivityDateTime.Value.Date )
                    .ToDictionary( g => g.Key, g => g.Count() )
                : new Dictionary<DateTime, int>();

            var unsubscribeCountsByDate = isEmail
                ? communicationInfo.RecipientActivities
                    .Where( a =>
                        a.ActivityDateTime.HasValue
                        && a.ActivityType == RecipientActivityType.Unsubscribe
                    )
                    .GroupBy( a => a.ActivityDateTime.Value.Date )
                    .ToDictionary( g => g.Key, g => g.Count() )
                : new Dictionary<DateTime, int>();

            // The start date for this data should always be the date the communication was sent.
            var startDate = sendDateTime.Value.Date;

            // The end date - however - should be driven by the last interaction (or spam complaint / unsubscribe).
            var endDates = openCountsByDate.Keys
                .Union( clickCountsByDate.Keys )
                .Union( spamComplaintsByDate.Keys )
                .Union( unsubscribeCountsByDate.Keys )
                .ToHashSet();

            if ( !endDates.Any() )
            {
                // We have no interactions to report.
                return uniqueInteractionsOverTime;
            }

            var endDate = endDates.Max().Date;

            var deliveredRecipientCount = communicationInfo.RecipientCountBreakdown?.DeliveredCount ?? 0;
            var uniqueOpensCount = groupedInteractions.UniqueOpensCount;

            if ( endDate < startDate || deliveredRecipientCount <= 0 )
            {
                // Should never happen.
                return uniqueInteractionsOverTime;
            }

            // We have everything we need; time to aggregate events into chart data.
            // Each data point is cumulative, not individual, so we'll keep a running total for each event type.
            var currentDate = startDate;
            var openCount = 0;
            var clickCount = 0;
            var spamComplaintCount = 0;
            var unsubscribeCount = 0;

            decimal GetPercentage( int count, int divisor )
            {
                if ( divisor <= 0 )
                {
                    return 0;
                }

                return Math.Round( count / ( decimal ) divisor * 100, 0, MidpointRounding.AwayFromZero );
            }

            while ( currentDate <= endDate )
            {
                var label = currentDate.ToISO8601DateString();

                openCount += openCountsByDate.GetValueOrDefault( currentDate, 0 );
                uniqueInteractionsOverTime.Add( new ChartNumericDataPointBag
                {
                    SeriesName = "Open Rate",
                    Label = label,
                    Value = GetPercentage( openCount, deliveredRecipientCount ),
                    Color = "--color-info-shade"
                } );

                if ( isEmail )
                {
                    clickCount += clickCountsByDate.GetValueOrDefault( currentDate, 0 );
                    uniqueInteractionsOverTime.Add( new ChartNumericDataPointBag
                    {
                        SeriesName = "Click-Through Rate",
                        Label = label,
                        Value = GetPercentage( clickCount, uniqueOpensCount ),
                        Color = "--color-success-shade"
                    } );

                    spamComplaintCount += spamComplaintsByDate.GetValueOrDefault( currentDate, 0 );
                    uniqueInteractionsOverTime.Add( new ChartNumericDataPointBag
                    {
                        SeriesName = "Spam Rate",
                        Label = label,
                        Value = GetPercentage( spamComplaintCount, deliveredRecipientCount ),
                        Color = "--color-warning-shade"
                    } );

                    unsubscribeCount += unsubscribeCountsByDate.GetValueOrDefault( currentDate, 0 );
                    uniqueInteractionsOverTime.Add( new ChartNumericDataPointBag
                    {
                        SeriesName = "Unsubscribe Rate",
                        Label = label,
                        Value = GetPercentage( unsubscribeCount, deliveredRecipientCount ),
                        Color = "--color-danger-shade"
                    } );
                }

                currentDate = currentDate.AddDays( 1 );
            }

            return uniqueInteractionsOverTime;
        }

        /// <summary>
        /// Gets the activity flow of interactions, Etc. for this communication.
        /// </summary>
        /// <param name="communicationInfo">The <see cref="CommunicationInfo"/>.</param>
        /// <param name="groupedInteractions">The <see cref="GroupedInteractions"/>.</param>
        /// <returns>The activity flow of interactions, Etc. for this communication.</returns>
        private CommunicationActivityFlowBag GetActivityFlow( CommunicationInfo communicationInfo, GroupedInteractions groupedInteractions )
        {
            var recipientCountBreakdown = communicationInfo.RecipientCountBreakdown;

            var level = 1;
            var nodeOrder = 0;

            var sentNode = SankeyNode.Sent;
            sentNode.Order = ++nodeOrder;

            // Start by adding the "Sent" node and edge, as these will always be present.
            var nodes = new List<SankeyDiagramNodeBag> { sentNode };
            var edges = new List<SankeyDiagramEdgeBag>
            {
                new SankeyDiagramEdgeBag
                {
                    TargetId = sentNode.Id,
                    Level = level,
                    Units = recipientCountBreakdown.RecipientCount
                }
            };

            // A local function to assist with edge tooltips.
            string BuildTooltip( string sourceName, string targetName, int units )
            {
                return $@"<strong>{sourceName} > {targetName}:</strong> {units:N0}";
            }

            // Add the "Delivered" node flowing from the "Sent" node only if any communications were actually delivered.
            // Interaction, Etc., nodes will flow from this node, so we'll keep a handle on it for later use.
            level = 2;
            SankeyDiagramNodeBag deliveredNode = null;
            var unitCount = recipientCountBreakdown.DeliveredCount;
            if ( unitCount > 0 )
            {
                deliveredNode = SankeyNode.Delivered;
                deliveredNode.Order = ++nodeOrder;
                nodes.Add( deliveredNode );

                edges.Add( new SankeyDiagramEdgeBag
                {
                    SourceId = sentNode.Id,
                    TargetId = deliveredNode.Id,
                    Level = level,
                    Units = unitCount,
                    Tooltip = BuildTooltip( sentNode.Name, deliveredNode.Name, unitCount )
                } );
            }

            // Add the "Failed", "Pending" and "Cancelled" nodes flowing from the "Sent" node only if we have sends that
            // resulted in each respective outcome.
            unitCount = recipientCountBreakdown.FailedCount;
            if ( unitCount > 0 )
            {
                var failedNode = SankeyNode.Failed;
                failedNode.Order = ++nodeOrder;
                nodes.Add( failedNode );

                edges.Add( new SankeyDiagramEdgeBag
                {
                    SourceId = sentNode.Id,
                    TargetId = failedNode.Id,
                    Level = level,
                    Units = unitCount,
                    Tooltip = BuildTooltip( sentNode.Name, failedNode.Name, unitCount )
                } );
            }

            unitCount = recipientCountBreakdown.PendingCount;
            if ( unitCount > 0 )
            {
                var pendingNode = SankeyNode.Pending;
                pendingNode.Order = ++nodeOrder;
                nodes.Add( pendingNode );

                edges.Add( new SankeyDiagramEdgeBag
                {
                    SourceId = sentNode.Id,
                    TargetId = pendingNode.Id,
                    Level = level,
                    Units = unitCount,
                    Tooltip = BuildTooltip( sentNode.Name, pendingNode.Name, unitCount )
                } );
            }

            unitCount = recipientCountBreakdown.CancelledCount;
            if ( unitCount > 0 )
            {
                var cancelledNode = SankeyNode.Cancelled;
                cancelledNode.Order = ++nodeOrder;
                nodes.Add( cancelledNode );

                edges.Add( new SankeyDiagramEdgeBag
                {
                    SourceId = sentNode.Id,
                    TargetId = cancelledNode.Id,
                    Level = level,
                    Units = unitCount,
                    Tooltip = BuildTooltip( sentNode.Name, cancelledNode.Name, unitCount )
                } );
            }

            if ( deliveredNode != null )
            {
                // Add Interaction [Etc.] nodes flowing from the "Delivered" node, according to this rank:
                // 
                //  [Action]            [Rank]      [Final Bucket]
                // ---------------------------------------------
                //  Unsubscribed        4           Unsubscribed
                //  Marked As Spam      3           Marked As Spam
                //  Clicked             2           Clicked
                //  Opened              1           Opened
                //  Delivered           0           Delivered (no flow)

                level = 3;

                // Keep track of assigned recipient IDs, so we only assign them to a single bucket.
                var level3AssignedRecipientIds = new HashSet<int>();

                // Gather recipients who marked as spam.
                var spamComplaintRecipientIds = new HashSet<int>(
                    communicationInfo.RecipientActivities
                        .Where( a => a.ActivityType == RecipientActivityType.SpamComplaint )
                        .Select( a => a.CommunicationRecipientId )
                        .Distinct()
                );

                level3AssignedRecipientIds.UnionWith( spamComplaintRecipientIds );

                // Gather recipients who unsubscribed.
                var unsubscribedRecipientIds = new HashSet<int>(
                    communicationInfo.RecipientActivities
                        .Where( a => a.ActivityType == RecipientActivityType.Unsubscribe )
                        .Select( a => a.CommunicationRecipientId )
                        .Distinct()
                );

                level3AssignedRecipientIds.UnionWith( unsubscribedRecipientIds );

                // Gather recipients who clicked.
                var clickedRecipientIds = new HashSet<int>(
                    groupedInteractions.UniqueClicks
                        .Where( c =>
                            c.CommunicationRecipientId.HasValue
                            && !level3AssignedRecipientIds.Contains( c.CommunicationRecipientId.Value )
                        )
                        .Select( c => c.CommunicationRecipientId.Value )
                );

                level3AssignedRecipientIds.UnionWith( clickedRecipientIds );

                // Gather recipients who opened.
                var openedRecipientIds = new HashSet<int>(
                    groupedInteractions.UniqueOpens
                        .Where( c =>
                            c.CommunicationRecipientId.HasValue
                            && !level3AssignedRecipientIds.Contains( c.CommunicationRecipientId.Value )
                        )
                        .Select( c => c.CommunicationRecipientId.Value )
                );

                level3AssignedRecipientIds.UnionWith( openedRecipientIds );

                // Now add them to the sankey in the reverse order.
                // Starting with recipients who opened.
                unitCount = openedRecipientIds.Count;
                if ( unitCount > 0 )
                {
                    var openedNode = SankeyNode.Opened;
                    openedNode.Order = ++nodeOrder;
                    nodes.Add( openedNode );

                    edges.Add( new SankeyDiagramEdgeBag
                    {
                        SourceId = deliveredNode.Id,
                        TargetId = openedNode.Id,
                        Level = level,
                        Units = unitCount,
                        Tooltip = BuildTooltip( deliveredNode.Name, openedNode.Name, unitCount )
                    } );
                }

                // Followed by those who clicked.
                unitCount = clickedRecipientIds.Count;
                if ( unitCount > 0 )
                {
                    var clickedNode = SankeyNode.Clicked;
                    clickedNode.Order = ++nodeOrder;
                    nodes.Add( clickedNode );

                    edges.Add( new SankeyDiagramEdgeBag
                    {
                        SourceId = deliveredNode.Id,
                        TargetId = clickedNode.Id,
                        Level = level,
                        Units = unitCount,
                        Tooltip = BuildTooltip( deliveredNode.Name, clickedNode.Name, unitCount )
                    } );
                }

                // Followed by those who marked as spam.
                unitCount = spamComplaintRecipientIds.Count;
                if ( unitCount > 0 )
                {
                    var markedAsSpamNode = SankeyNode.MarkedAsSpam;
                    markedAsSpamNode.Order = ++nodeOrder;
                    nodes.Add( markedAsSpamNode );

                    edges.Add( new SankeyDiagramEdgeBag
                    {
                        SourceId = deliveredNode.Id,
                        TargetId = markedAsSpamNode.Id,
                        Level = level,
                        Units = unitCount,
                        Tooltip = BuildTooltip( deliveredNode.Name, markedAsSpamNode.Name, unitCount )
                    } );
                }

                // Followed by those who unsubscribed.
                unitCount = unsubscribedRecipientIds.Count;
                if ( unitCount > 0 )
                {
                    var unsubscribedNode = SankeyNode.Unsubscribed;
                    unsubscribedNode.Order = ++nodeOrder;
                    nodes.Add( unsubscribedNode );

                    edges.Add( new SankeyDiagramEdgeBag
                    {
                        SourceId = deliveredNode.Id,
                        TargetId = unsubscribedNode.Id,
                        Level = level,
                        Units = unitCount,
                        Tooltip = BuildTooltip( deliveredNode.Name, unsubscribedNode.Name, unitCount )
                    } );
                }
            }

            return new CommunicationActivityFlowBag
            {
                Nodes = nodes,
                Edges = edges
            };
        }

        /// <summary>
        /// Gets the unique opens by gender for this communication.
        /// </summary>
        /// <param name="uniqueOpens">The list of <see cref="InteractionInfo"/>s that represent unique opens.</param>
        /// <returns>A list of <see cref="ChartNumericDataPointBag"/>s representing the unique opens by gender.</returns>
        private List<ChartNumericDataPointBag> GetUniqueOpensByGender( List<InteractionInfo> uniqueOpens )
        {
            var uniqueOpensByGender = new List<ChartNumericDataPointBag>();

            var genderGroups = uniqueOpens
                .GroupBy( o => o.PersonGender )
                .ToDictionary( g => g.Key, g => g.Count() );

            var uniqueOpensCount = uniqueOpens.Count();
            decimal GetPercentage( int count )
            {
                if ( uniqueOpensCount == 0 )
                {
                    return 0;
                }

                return Math.Round( count / ( decimal ) uniqueOpensCount * 100, 0, MidpointRounding.AwayFromZero );
            }

            if ( genderGroups.TryGetValue( Gender.Male, out var maleCount ) && maleCount > 0 )
            {
                uniqueOpensByGender.Add( new ChartNumericDataPointBag
                {
                    Label = Gender.Male.ConvertToString(),
                    Value = GetPercentage( maleCount ),
                    Color = "--color-info-tint"
                } );
            }

            if ( genderGroups.TryGetValue( Gender.Female, out var femaleCount ) && femaleCount > 0 )
            {
                uniqueOpensByGender.Add( new ChartNumericDataPointBag
                {
                    Label = Gender.Female.ConvertToString(),
                    Value = GetPercentage( femaleCount ),
                    Color = "--color-danger-tint"
                } );
            }

            if ( genderGroups.TryGetValue( Gender.Unknown, out var unknownCount ) && unknownCount > 0 )
            {
                uniqueOpensByGender.Add( new ChartNumericDataPointBag
                {
                    Label = Gender.Unknown.ConvertToString(),
                    Value = GetPercentage( unknownCount ),
                    Color = "--color-interface-soft"
                } );
            }

            return uniqueOpensByGender;
        }

        /// <summary>
        /// Gets the unique opens by age range for this communication.
        /// </summary>
        /// <param name="uniqueOpens">The list of <see cref="InteractionInfo"/>s that represent unique opens.</param>
        /// <returns>A list of <see cref="ChartNumericDataPointBag"/>s representing the unique opens by age range.</returns>
        private List<ChartNumericDataPointBag> GetUniqueOpensByAgeRange( List<InteractionInfo> uniqueOpens )
        {
            var labels = new string[] { "18-29", "30-39", "40-49", "50-59", "60-69", "70-79", "80+", "Unknown" };

            var counts = new int[labels.Length];

            foreach ( var o in uniqueOpens )
            {
                int bucket;
                var age = o.PersonAge.GetValueOrDefault();

                if ( age < 18 )
                {
                    bucket = 7;
                }
                else if ( age < 30 )
                {
                    bucket = 0;
                }
                else if ( age < 40 )
                {
                    bucket = 1;
                }
                else if ( age < 50 )
                {
                    bucket = 2;
                }
                else if ( age < 60 )
                {
                    bucket = 3;
                }
                else if ( age < 70 )
                {
                    bucket = 4;
                }
                else if ( age < 80 )
                {
                    bucket = 5;
                }
                else
                {
                    bucket = 6;
                }

                counts[bucket]++;
            }

            var uniqueOpensByAgeRange = new List<ChartNumericDataPointBag>();

            var knownAgeColor = "--color-categorical-3";
            var unknownAgeColor = "--color-interface-soft";

            for ( var i = 0; i < labels.Length; i++ )
            {
                uniqueOpensByAgeRange.Add( new ChartNumericDataPointBag
                {
                    SeriesName = "Age Ranges",
                    Label = labels[i],
                    Value = counts[i],
                    Color = i == labels.Length - 1 ? unknownAgeColor : knownAgeColor
                } );
            }

            return uniqueOpensByAgeRange;
        }

        /// <summary>
        /// Gets analytics data about the clients used to open this communication.
        /// </summary>
        /// <param name="communicationId">The <see cref="Rock.Model.Communication"/> identifier.</param>
        /// <returns>Analytics data about the clients used to open this communication.</returns>
        private (List<ChartNumericDataPointBag> top, List<ChartNumericDataPointBag> all) GetClients( int communicationId )
        {
            var clients = (
                top: ( List<ChartNumericDataPointBag> ) null,
                all: ( List<ChartNumericDataPointBag> ) null
            );

            var seriesName = "Clients";

            var unknownLabel = "Unknown";
            var unknownColor = "--color-interface-soft";

            var othersLabel = "Others";
            var othersColor = "--color-interface-strong";

            // Start by getting the interaction counts by each distinct client (excluding those from robots).
            var clientCounts = GetInteractionsQuery( communicationId )
                .Where( i =>
                    i.InteractionSession == null
                    || i.InteractionSession.DeviceType == null
                    || !i.InteractionSession.DeviceType.ClientType.Equals( "robot", StringComparison.OrdinalIgnoreCase )
                )
                .GroupBy( i =>
                    i.InteractionSession == null
                    || i.InteractionSession.DeviceType == null
                    || i.InteractionSession.DeviceType.Application == null
                        ? unknownLabel
                        : i.InteractionSession.DeviceType.Application
                )
                .Select( g => new
                {
                    ClientName = g.Key,
                    Count = g.Count()
                } )
                .OrderByDescending( a => a.Count )
                .ToList();

            var totalInteractionsCount = clientCounts.Sum( c => c.Count );
            if ( totalInteractionsCount == 0 )
            {
                return clients;
            }

            // Determine each client's percentage of the whole while adding them to the "all" collection.
            clients.all = clientCounts
                .Select( c => new ChartNumericDataPointBag
                {
                    SeriesName = seriesName,
                    Label = c.ClientName,
                    Value = c.Count / ( decimal ) totalInteractionsCount * 100
                } )
                .ToList();

            // Rules for displaying "Top" email clients:
            //  1. If client count < 5, show a "Top" entry for each client.
            //  2. Otherwise, show the top 3 + "Others" where "Others" = sum of all remaining clients' percentages.
            clients.top = new List<ChartNumericDataPointBag>();

            // Ensure we've defined enough colors, up to the max number of bars.
            var maxNumberOfBars = 4;
            var colorQueue = new Queue<string>( new[] {
                "--color-categorical-7",
                "--color-categorical-6",
                "--color-categorical-3",
                "--color-categorical-2"
            } );

            string GetColor( string label ) =>
                label.Equals( unknownLabel )
                    ? unknownColor
                    : colorQueue.Count > 0 ? colorQueue.Dequeue() : "--color-interface-soft";

            decimal GetRoundedValue( decimal value ) => Math.Round( value, 1, MidpointRounding.AwayFromZero );

            // Add a bar for each client, up to the max minus one.
            clients.top.AddRange(
                clients.all.Take( maxNumberOfBars - 1 ).Select( dataPoint =>
                {
                    dataPoint.Color = GetColor( dataPoint.Label );
                    dataPoint.Value = GetRoundedValue( dataPoint.Value );

                    return dataPoint;
                } )
            );

            // Then decide whether the remainder of the clients need to be aggregated into "Others".
            var remainder = clients.all.Skip( maxNumberOfBars - 1 ).ToList();
            if ( remainder.Count < 2 )
            {
                // Add the remaining 1 [or 0] to the "Top" collection.
                clients.top.AddRange(
                    remainder.Select( dataPoint =>
                    {
                        dataPoint.Color = GetColor( dataPoint.Label );
                        dataPoint.Value = GetRoundedValue( dataPoint.Value );

                        return dataPoint;
                    } )
                );
            }
            else
            {
                // Aggregate into "Others" and add it to the "Top" collection.
                clients.top.Add( new ChartNumericDataPointBag
                {
                    SeriesName = seriesName,
                    Label = othersLabel,
                    Color = othersColor,
                    Value = GetRoundedValue( remainder.Sum( r => r.Value ) )
                } );

                // Finish by rounding and assigning a color to the individual remainders.
                remainder.ForEach( dataPoint =>
                {
                    dataPoint.Color = othersColor;
                    dataPoint.Value = GetRoundedValue( dataPoint.Value );
                } );
            }

            return clients;
        }

        /// <summary>
        /// Gets analytics data for all links contained within this communication.
        /// </summary>
        /// <param name="deliveredRecipientCount">The count of recipients to whom this communication was successfully delivered.</param>
        /// <param name="groupedInteractions">The grouped interaction data from which to derive links analytics.</param>
        /// <returns>Analytics data for all links contained within this communication.</returns>
        private List<CommunicationLinkAnalyticsBag> GetAllLinksAnalytics( int deliveredRecipientCount, GroupedInteractions groupedInteractions )
        {
            // The top performing link is the one that has the highest count of total clicks (EXCLUDING unsubscribe clicks).
            // Start by grouping the interactions by URL and assigning the click counts & click-through rates.
            var uniqueClicksCountByUrl = groupedInteractions.UniqueClicks
                .Where( c =>
                    c.InteractionData.IsNotNullOrWhiteSpace()
                    && !c.InteractionData.Contains( "/Unsubscribe/" )
                )
                .GroupBy( c => c.InteractionData )
                .ToDictionary( g => g.Key, g => g.Count() );

            var topLinkTotalClicksCount = 0;

            var allLinksAnalytics = groupedInteractions.AllClicks
                .Where( c => uniqueClicksCountByUrl.ContainsKey( c.InteractionData ) )
                .GroupBy( c => c.InteractionData )
                .Select( g =>
                {
                    var uniqueClicksCount = uniqueClicksCountByUrl[g.Key];
                    var clickThroughRate = deliveredRecipientCount > 0
                        ? Math.Round( uniqueClicksCount / ( decimal ) deliveredRecipientCount * 100, 1, MidpointRounding.AwayFromZero )
                        : 0;

                    var analytics = new CommunicationLinkAnalyticsBag
                    {
                        Url = g.Key,
                        TotalClicksCount = g.Count(),
                        UniqueClicksCount = uniqueClicksCount,
                        ClickThroughRate = clickThroughRate
                    };

                    return analytics;
                } )
                .OrderByDescending( a => a.TotalClicksCount )
                .Select( ( a, index ) =>
                {
                    if ( index == 0 )
                    {
                        topLinkTotalClicksCount = a.TotalClicksCount;
                        a.PercentOfTopLink = 100;
                    }
                    else
                    {
                        // Set the percentage of this link's total clicks count relative to that of the top performing link.
                        a.PercentOfTopLink = topLinkTotalClicksCount > 0
                            ? Math.Round( a.TotalClicksCount / ( decimal ) topLinkTotalClicksCount * 100, 1, MidpointRounding.AwayFromZero )
                            : 0;
                    }

                    return a;
                } )
                .ToList();

            return allLinksAnalytics;
        }

        /// <summary>
        /// Gets a <see cref="CommunicationMessagePreviewBag"/> that contains the information needed to display previews
        /// of the message(s) for this communication.
        /// </summary>
        /// <param name="communicationInfo">
        /// The <see cref="CommunicationInfo"/> from which to get the <see cref="CommunicationMessagePreviewBag"/>.
        /// </param>
        /// <returns>A <see cref="CommunicationMessagePreviewBag"/> that contains preview details.</returns>
        private CommunicationMessagePreviewBag GetMessagePreview( CommunicationInfo communicationInfo )
        {
            var communication = communicationInfo?.Communication;
            if ( communication == null )
            {
                return null;
            }

            var createdByPersonName = Person.FormatFullName(
                communicationInfo.CreatedByPersonNickName,
                communicationInfo.CreatedByPersonLastName,
                communicationInfo.CreatedByPersonSuffixValueId,
                communicationInfo.CreatedByPersonRecordTypeValueId
            );

            var approvedByByPersonName = Person.FormatFullName(
                communicationInfo.ReviewerPersonNickName,
                communicationInfo.ReviewerPersonLastName,
                communicationInfo.ReviewerPersonSuffixValueId,
                communicationInfo.ReviewerPersonRecordTypeValueId
            );

            return new CommunicationMessagePreviewBag
            {
                CreatedByPersonName = createdByPersonName,
                CreatedDateTime = communication.CreatedDateTime,
                ApprovedByPersonName = approvedByByPersonName,
                ApprovedDateTime = communication.ReviewedDateTime,
                FromPersonName = communication.FromName,
                FromPersonEmail = communication.FromEmail,
                ReplyToEmail = communication.ReplyToEmail,
                CcEmails = communication.CCEmails,
                BccEmails = communication.BCCEmails,
                EmailSubject = communication.Subject,
                EmailMessage = communication.Message,
                EmailAttachmentBinaryFiles = communication.GetAttachments( Model.CommunicationType.Email )
                    ?.Select( ca => ca.BinaryFile )
                    ?.ToListItemBagList(),

                FromSystemPhoneNumberName = communicationInfo.FromSystemPhoneNumberName,
                FromSystemPhoneNumber = communicationInfo.FromSystemPhoneNumber,
                SmsMessage = communication.SMSMessage,
                SmsAttachmentBinaryFiles = communication.GetAttachments( Model.CommunicationType.SMS )
                    ?.Select( ca => ca.BinaryFile )
                    ?.ToListItemBagList(),

                PushTitle = communication.PushTitle,
                PushMessage = communication.PushMessage,
                PushOpenAction = ( CommunicationDetailPushOpenAction? ) communication.PushOpenAction,

                CommunicationListName = communicationInfo.CommunicationListName,
                UrlReferrer = communication.UrlReferrer
            };
        }

        /// <summary>
        /// Gets the grid builder for the recipient grid.
        /// </summary>
        /// <param name="communicationType">The <see cref="CommunicationType"/> for which to build the grid.</param>
        /// <returns>The grid builder for the recipient grid.</returns>
        private GridBuilder<CommunicationRecipientRow> GetRecipientGridBuilder( CommunicationType communicationType )
        {
            var builder = new GridBuilder<CommunicationRecipientRow>()
                .WithBlock( this )
                .AddTextField( "personIdKey", r => r.Person.IdKey )
                .AddTextField( "communicationRecipientIdKey", r => r.CommunicationRecipientId.AsIdKey() )
                .AddPersonField( "person", r => r.Person )
                .AddDateTimeField( "lastActivityDateTime", r => r.LastActivityDateTime )
                .AddField( "status", r => r.Status )
                .AddTextField( "statusNote", r => r.StatusNote )
                .AddDateTimeField( "sendDateTime", r => r.SendDateTime )
                .AddDateTimeField( "deliveredDateTime", r => r.DeliveredDateTime );

            if ( communicationType == CommunicationType.RecipientPreference )
            {
                builder.AddField( "medium", r =>
                {
                    var m = r.MediumEntityTypeId.GetValueOrDefault();
                    if ( CommunicationTypeByMediumEntityTypeId.TryGetValue( m, out var t ) )
                    {
                        return t;
                    }

                    return null;
                } );
            }

            if ( communicationType != CommunicationType.SMS )
            {
                builder.AddField( "lastOpenedDateTime", r => r.LastOpenedDateTime );
            }

            if ( communicationType == CommunicationType.RecipientPreference || communicationType == CommunicationType.Email )
            {
                builder
                    .AddField( "opensCount", r => r.OpensCount )
                    .AddField( "clicksCount", r => r.ClicksCount )
                    .AddField( "lastClickedDateTime", r => r.LastClickedDateTime )
                    .AddField( "unsubscribeDateTime", r => r.UnsubscribeDateTime )
                    .AddField( "spamComplaintDateTime", r => r.SpamComplaintDateTime );
            }

            if ( RecipientGridPropertyColumns?.Any() == true )
            {
                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.Age ) )
                {
                    builder.AddField( "age", r => r.Person.Age );
                }

                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.AgeClassification ) )
                {
                    builder.AddField( "ageClassification", r => r.Person.AgeClassification );
                }

                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.BirthDate ) )
                {
                    builder.AddDateTimeField( "birthdate", r => r.Person.BirthDate );
                }

                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.Campus ) )
                {
                    builder.AddTextField( "campus", r => r.Person.PrimaryCampus?.Name );
                }

                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.Email ) )
                {
                    builder.AddTextField( "email", r => r.Person.Email );
                }

                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.Gender ) )
                {
                    builder.AddField( "gender", r => r.Person.Gender );
                }

                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.Grade ) )
                {
                    builder.AddTextField( "grade", r => r.Person.GradeFormatted );
                }

                if ( RecipientGridPropertyColumns.Contains( PersonPropertyName.IsDeceased ) )
                {
                    builder.AddField( "isDeceased", r => r.Person.IsDeceased );
                }
            }

            if ( RecipientGridAttributeColumns.Any() )
            {
                builder.AddAttributeFieldsFrom( r => r.Person, RecipientGridAttributeColumns );
            }

            return builder;
        }

        /// <summary>
        /// Gets the recipient grid options.
        /// </summary>
        /// <returns>The recipient grid options.</returns>
        private CommunicationRecipientGridOptionsBag GetRecipientGridOptions()
        {
            // Add person property-based items.
            var personPropertyItems = new List<ListItemBag>
            {
                new ListItemBag
                {
                    Text = PersonPropertyName.Age,
                    Value = PersonPropertyName.Age
                },
                new ListItemBag
                {
                    Text = "Age Classification",
                    Value = PersonPropertyName.AgeClassification
                },
                new ListItemBag
                {
                    Text = "Birthdate",
                    Value = PersonPropertyName.BirthDate
                },
                new ListItemBag
                {
                    Text = PersonPropertyName.Campus,
                    Value = PersonPropertyName.Campus
                },
                new ListItemBag
                {
                    Text = PersonPropertyName.Email,
                    Value = PersonPropertyName.Email
                },
                new ListItemBag
                {
                    Text = PersonPropertyName.Gender,
                    Value = PersonPropertyName.Gender
                },
                new ListItemBag
                {
                    Text = PersonPropertyName.Grade,
                    Value = PersonPropertyName.Grade
                },
                new ListItemBag
                {
                    Text = "Is Deceased",
                    Value = PersonPropertyName.IsDeceased
                }
            };

            // Add person attribute-based items that the current person is authorized to view.
            var currentPerson = GetCurrentPerson();
            var personAttributeItems = AttributeCache
                .GetByEntityType( PersonEntityTypeId )
                .Where( a => a.IsAuthorized( Authorization.VIEW, currentPerson ) )
                .Select( a => new ListItemBag
                {
                    Text = a.Name,
                    Value = a.Guid.ToString()
                } )
                .ToList();

            return new CommunicationRecipientGridOptionsBag
            {
                PersonPropertyItems = personPropertyItems,
                PersonAttributeItems = personAttributeItems
            };
        }

        /// <summary>
        /// Gets permissions dictating which actions the individual is allowed to perform for this communication.
        /// </summary>
        /// <param name="communication">The <see cref="Rock.Model.Communication"/> for which to check permissions.</param>
        /// <returns>Permissions dictating which actions the individual is allowed to perform for this communication.</returns>
        private CommunicationDetailPermissionsBag GetPermissions( Rock.Model.Communication communication )
        {
            var permissions = new CommunicationDetailPermissionsBag
            {
                CanCreatePersonalTemplate = GetAttributeValue( AttributeKey.EnablePersonalTemplates ).AsBoolean()
            };

            var currentPerson = GetCurrentPerson();

            if ( communication.Status == CommunicationStatus.PendingApproval )
            {
                var canApprove = BlockCache.IsAuthorized( Authorization.APPROVE, GetCurrentPerson() );
                permissions.CanApprove = canApprove;
                permissions.CanDeny = canApprove;
                permissions.CanEdit = canApprove;

                permissions.CanCancel = communication.IsAuthorized( Authorization.EDIT, currentPerson );
            }
            else if ( communication.Status == CommunicationStatus.Approved )
            {
                // If there are still any pending recipients, allow canceling of send.
                var hasPendingRecipients = new CommunicationRecipientService( RockContext )
                    .Queryable()
                    .Where( r =>
                        r.CommunicationId == communication.Id
                        && r.Status == CommunicationRecipientStatus.Pending
                    )
                    .Any();

                // TODO (Jason): Shouldn't `Approved` comms also require EDIT auth to cancel (Even though the legacy block didn't require this)?
                // https://app.asana.com/1/20866866924293/project/1208321217019996/task/1210655530351448?focus=true
                permissions.CanCancel = hasPendingRecipients;

                // Allow them to create a copy if they have VIEW (don't require full EDIT auth).
                permissions.CanDuplicate = communication.IsAuthorized( Authorization.VIEW, currentPerson );
            }

            return permissions;
        }

        /// <summary>
        /// Gets the page parameters needed to reload the page for the specified <see cref="Rock.Model.Communication"/>.
        /// </summary>
        /// <param name="communicationId">The <see cref="Rock.Model.Communication"/> identifier.</param>
        /// <returns>The page parameters needed to reload the page for the specified <see cref="Rock.Model.Communication"/>.</returns>
        private IDictionary<string, string> GetPageParamsForReload( int communicationId )
        {
            // Redirect back to the same page with the provided communication identifier.
            var pageParams = RequestContext.GetPageParameters();
            pageParams.AddOrReplace( PageParameterKey.Communication, communicationId.AsIdKey() );
            pageParams.Remove( PageParameterKey.CommunicationId );

            return pageParams;
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken( int communicationId )
        {
            var securityGrant = new SecurityGrant();

            securityGrant.AddRule( new EntitySecurityGrantRule( CommunicationEntityTypeId, communicationId ) );

            return securityGrant.ToToken();
        }

        #endregion Private Methods

        #region Supporting Classes

        /// <summary>
        /// A POCO to represent a <see cref="Rock.Model.Communication"/> along with the minimum supporting data needed
        /// for display within the communication detail block.
        /// </summary>
        private class CommunicationInfo
        {
            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public Rock.Model.Communication Communication { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person who created this communication.
            /// </summary>
            public string CreatedByPersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who created this communication.
            /// </summary>
            public string CreatedByPersonLastName { get; set; }

            /// <summary>
            /// Gets or sets the suffix <see cref="DefinedValue"/> identifier of the person who created this communication.
            /// </summary>
            public int? CreatedByPersonSuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the record type <see cref="DefinedValue"/> identifier of the person who created this communication.
            /// </summary>
            public int? CreatedByPersonRecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the nickname of the person who reviewed this communication.
            /// </summary>
            public string ReviewerPersonNickName { get; set; }

            /// <summary>
            /// Gets or sets the last name of the person who reviewed this communication.
            /// </summary>
            public string ReviewerPersonLastName { get; set; }

            /// <summary>
            /// Gets or sets the suffix <see cref="DefinedValue"/> identifier of the person who reviewed this communication.
            /// </summary>
            public int? ReviewerPersonSuffixValueId { get; set; }

            /// <summary>
            /// Gets or sets the record type <see cref="DefinedValue"/> identifier of the person who reviewed this communication.
            /// </summary>
            public int? ReviewerPersonRecordTypeValueId { get; set; }

            /// <summary>
            /// Gets or sets the name of the system phone number this communication is being sent from, if SMS.
            /// </summary>
            public string FromSystemPhoneNumberName { get; set; }

            /// <summary>
            /// Gets or sets the system phone number this communication is being sent from.
            /// </summary>
            public string FromSystemPhoneNumber { get; set; }

            /// <summary>
            /// Gets or sets the name of the communication list to which this communication should be sent.
            /// </summary>
            public string CommunicationListName { get; set; }

            /// <summary>
            /// Gets or sets the name of the <see cref="CommunicationFlow"/> this communication belongs to.
            /// </summary>
            public string CommunicationFlowName { get; set; }

            /// <summary>
            /// Gets or sets the total count of all recipients tied to this communication.
            /// </summary>
            public int TotalRecipientCount { get; set; }

            /// <summary>
            /// Gets or sets the the breakdown of recipients counts for this communication.
            /// </summary>
            public RecipientCountBreakdown RecipientCountBreakdown { get; set; }

            /// <summary>
            /// Gets or sets the list of (non-interaction) <see cref="RecipientActivity"/>s tied to this communication.
            /// </summary>
            public List<RecipientActivity> RecipientActivities { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the medium <see cref="EntityType"/> filter that should be applied to
            /// analytics data (e.g. interactions).
            /// </summary>
            public int? MediumEntityTypeFilterId { get; set; }
        }

        /// <summary>
        /// A POCO to represent the breakdown of <see cref="CommunicationRecipient"/> counts.
        /// </summary>
        private class RecipientCountBreakdown
        {
            /// <summary>
            /// Gets or sets the <see cref="Rock.Enums.Communication.CommunicationType"/> represented within these count values.
            /// </summary>
            public CommunicationType CommunicationType { get; set; }

            /// <summary>
            /// Gets or sets the total count of recipients tied to this communication and type.
            /// </summary>
            public int RecipientCount { get; set; }

            /// <summary>
            /// Gets or sets the count of recipients for whom communications of this type are still pending (not yet sent).
            /// </summary>
            public int PendingCount { get; set; }

            /// <summary>
            /// Gets or sets the count of recipients for whom communications of this type were delivered.
            /// </summary>
            public int DeliveredCount { get; set; }

            /// <summary>
            /// Gets or sets the count of recipients for whom communications of this type failed to be delivered.
            /// </summary>
            public int FailedCount { get; set; }

            /// <summary>
            /// Gets or sets the count of recipients for whom communications of this type were cancelled.
            /// </summary>
            public int CancelledCount { get; set; }
        }

        /// <summary>
        /// The types of (non-interaction) activities a <see cref="CommunicationRecipient"> can perform against a
        /// <see cref="Rock.Model.Communication"/>.
        /// </summary>
        private enum RecipientActivityType
        {
            /// <summary>
            /// Marked the communication as spam.
            /// </summary>
            SpamComplaint = 1,

            /// <summary>
            /// Unsubscribed as a result of receiving the communication.
            /// </summary>
            Unsubscribe = 2
        }

        /// <summary>
        /// A POCO to represent a (non-interaction) activity performed by a <see cref="CommunicationRecipient"/>.
        /// </summary>
        private class RecipientActivity
        {
            /// <summary>
            /// Gets or sets the identifier of the <see cref="Rock.Model.CommunicationRecipient"/> who performed this activity.
            /// </summary>
            public int CommunicationRecipientId { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.MediumEntityTypeId"/>
            public int? MediumEntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the type of activity that was performed.
            /// </summary>
            public RecipientActivityType ActivityType { get; set; }

            /// <summary>
            /// Gets or sets the datetime this activity was performed.
            /// </summary>
            public DateTime? ActivityDateTime { get; set; }
        }

        /// <summary>
        /// A POCO to represent the minimum <see cref="Interaction"/> data needed for analytics visuals.
        /// </summary>
        private class InteractionInfo
        {
            /// <inheritdoc cref="Interaction.InteractionDateTime"/>
            public DateTime InteractionDateTime { get; set; }

            /// <inheritdoc cref="Interaction.Operation"/>
            public string Operation { get; set; }

            /// <inheritdoc cref="Interaction.InteractionData"/>
            public string InteractionData { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the <see cref="CommunicationRecipient"/> to whom this interaction relates.
            /// </summary>
            public int? CommunicationRecipientId { get; set; }

            /// <inheritdoc cref="Person.Gender"/>
            public Gender PersonGender { get; set; }

            /// <inheritdoc cref="Person.Age"/>
            public int? PersonAge { get; set; }
        }

        /// <summary>
        /// A POCO to represent grouped <see cref="InteractionInfo"/>s, which will be used throughout this block.
        /// </summary>
        private class GroupedInteractions
        {
            /// <summary>
            /// Gets or sets the complete list of <see cref="InteractionInfo"/>s that represent "Opened" interactions.
            /// </summary>
            public List<InteractionInfo> AllOpens { get; private set; }

            /// <summary>
            /// Gets or sets the complete list of <see cref="InteractionInfo"/>s that represent "Opened" interactions,
            /// grouped by <see cref="CommunicationRecipient"/> identifier.
            /// </summary>
            public Dictionary<int, List<InteractionInfo>> AllOpensByRecipientId { get; private set; }

            /// <summary>
            /// Gets or sets the complete list of <see cref="InteractionInfo"/>s that represent "Click" interactions.
            /// </summary>
            public List<InteractionInfo> AllClicks { get; private set; }

            /// <summary>
            /// Gets or sets the complete list of <see cref="InteractionInfo"/>s that represent "Click" interactions,
            /// grouped by <see cref="CommunicationRecipient"/> identifier.
            /// </summary>
            public Dictionary<int, List<InteractionInfo>> AllClicksByRecipientId { get; private set; }

            /// <summary>
            /// Gets or sets the list of unique, first "Opened" <see cref="InteractionInfo"/>s (at most one per recipient).
            /// </summary>
            public List<InteractionInfo> UniqueOpens { get; private set; }

            /// <summary>
            /// Gets or sets the list of unique, first "Click" <see cref="InteractionInfo"/>s (at most one per recipient).
            /// </summary>
            public List<InteractionInfo> UniqueClicks { get; private set; }

            /// <summary>
            /// Gets the total count of "Opened" interactions.
            /// </summary>
            public int TotalOpensCount => AllOpens.Count;

            /// <summary>
            /// Gets the total count of "Click" interactions.
            /// </summary>
            public int TotalClicksCount => AllClicks.Count;

            /// <summary>
            /// Gets the count of recipients who had at least one "Opened" interaction.
            /// </summary>
            public int UniqueOpensCount => UniqueOpens.Count;

            /// <summary>
            /// Gets the count of recipients who had at least one "Click" interaction.
            /// </summary>
            public int UniqueClicksCount => UniqueClicks.Count;

            /// <summary>
            /// Initializes a new instance of the <see cref="GroupedInteractions"/> class.
            /// </summary>
            /// <param name="interactions">The list of <see cref="InteractionInfo"/>s to group.</param>
            public GroupedInteractions( List<InteractionInfo> interactions )
            {
                interactions = ( interactions ?? new List<InteractionInfo>() )
                    .Where( i => i?.CommunicationRecipientId != null )
                    .ToList();

                var allOpens = interactions.Where( i => i.Operation == InteractionOperation.Opened );
                var allClicks = interactions.Where( i => i.Operation == InteractionOperation.Click );

                var uniqueOpensByRecipient = allOpens
                    .GroupBy( o => o.CommunicationRecipientId.Value )
                    .ToDictionary(
                        g => g.Key,                                             // [CommunicationRecipient].[Id]
                        g => g.OrderBy( i => i.InteractionDateTime ).First()    // The first "Opened" interaction for this recipient.
                    );

                var uniqueClicksByRecipient = allClicks
                    .GroupBy( o => o.CommunicationRecipientId.Value )
                    .ToDictionary(
                        g => g.Key,                                             // [CommunicationRecipient].[Id]
                        g => g.OrderBy( i => i.InteractionDateTime ).First()    // The first "Click" interaction for this recipient.
                    );

                var recipientIdsWithOpens = uniqueOpensByRecipient.Keys;
                var recipientIdsWithClicks = uniqueClicksByRecipient.Keys;

                // When grouping by opens, include unique click interactions whose recipient does NOT have a corresponding
                // open interaction. This is to capture the scenario where an email is viewed without loading the image
                // links that are required to trigger the open event.
                var recipientIdsHavingClicksWithoutOpens = recipientIdsWithClicks.Except( recipientIdsWithOpens );
                var inferredOpens = uniqueClicksByRecipient
                    .Where( kvp => recipientIdsHavingClicksWithoutOpens.Contains( kvp.Key ) )
                    .Select( kvp => kvp.Value )
                    .ToList();

                // Merge [and reorder] the inferred opens in with all actual opens.
                AllOpens = allOpens
                    .Union( inferredOpens )
                    .OrderBy( i => i.InteractionDateTime )
                    .ToList();

                AllOpensByRecipientId = AllOpens
                    .GroupBy( i => i.CommunicationRecipientId.Value )
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy( i => i.InteractionDateTime ).ToList()
                    );

                AllClicks = allClicks
                    .OrderBy( i => i.InteractionDateTime )
                    .ToList();

                AllClicksByRecipientId = AllClicks
                    .GroupBy( i => i.CommunicationRecipientId.Value )
                    .ToDictionary(
                        g => g.Key,
                        g => g.OrderBy( i => i.InteractionDateTime ).ToList()
                    );

                // Merge [and reorder] the inferred opens in with the unique actual opens.
                UniqueOpens = uniqueOpensByRecipient
                    .Select( kvp => kvp.Value )
                    .Union( inferredOpens )
                    .OrderBy( i => i.InteractionDateTime )
                    .ToList();

                UniqueClicks = uniqueClicksByRecipient
                    .Select( kvp => kvp.Value )
                    .OrderBy( i => i.InteractionDateTime )
                    .ToList();
            }
        }

        /// <summary>
        /// A POCO to represent a <see cref="Rock.Model.Communication"/> along with its list of
        /// <see cref="CommunicationRecipientRow"/>s.
        /// </summary>
        private class CommunicationWithRecipientRows
        {
            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Communication"/>.
            /// </summary>
            public Rock.Model.Communication Communication { get; set; }

            /// <summary>
            /// Gets or sets the list of <see cref="CommunicationRecipientRow"/>s tied to this communication.
            /// </summary>
            public List<CommunicationRecipientRow> RecipientRows { get; set; }
        }

        /// <summary>
        /// A POCO to represent a <see cref="CommunicationRecipient"/> SQL projection for the recipient grid.
        /// </summary>
        private class CommunicationRecipientRow
        {
            /// <summary>
            /// Gets or sets the <see cref="Rock.Model.Person"/> that represents this row's recipient.
            /// </summary>
            public Person Person { get; set; }

            /// <summary>
            /// Gets or sets the <see cref="Campus.Name"/> of the recipient person's primary campus.
            /// </summary>
            public string CampusName { get; set; }

            /// <summary>
            /// Gets or sets the identifier of the <see cref="CommunicationRecipient"/> represented by this row.
            /// </summary>
            public int CommunicationRecipientId { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.Status"/>
            public CommunicationRecipientStatus Status { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.StatusNote"/>
            public string StatusNote { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.MediumEntityTypeId"/>
            public int? MediumEntityTypeId { get; set; }

            /// <summary>
            /// Gets or sets the datetime of this recipient's last activity for this communication.
            /// </summary>
            public DateTime? LastActivityDateTime { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.SendDateTime"/>
            public DateTime? SendDateTime { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.DeliveredDateTime"/>
            public DateTime? DeliveredDateTime { get; set; }

            /// <summary>
            /// Gets or sets the count of times this recipient has opened this communication.
            /// </summary>
            public int? OpensCount { get; set; }

            /// <summary>
            /// Gets or sets the last datetime this communication was opened by this recipient.
            /// </summary>
            public DateTime? LastOpenedDateTime { get; set; }

            /// <summary>
            /// Gets or sets the count of times this recipient has clicked any link within this communication.
            /// </summary>
            public int? ClicksCount { get; set; }

            /// <summary>
            /// Gets or sets the last datetime this recipient clicked any link within this communication.
            /// </summary>
            public DateTime? LastClickedDateTime { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.UnsubscribeDateTime"/>
            public DateTime? UnsubscribeDateTime { get; set; }

            /// <inheritdoc cref="CommunicationRecipient.SpamComplaintDateTime"/>
            public DateTime? SpamComplaintDateTime { get; set; }
        }

        #endregion Supporting Classes
    }
}
