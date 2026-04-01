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

using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

using Rock.Cms.StructuredContent;
using Rock.Communication;
using Rock.Enums.Blocks.Communication.CommunicationTemplateDetail;
using Rock.Model;
using Rock.Security;
using Rock.Security.SecurityGrantRules;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.SystemCommunicationDetail;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Allows the administration of a system communication.
    /// </summary>
    [DisplayName( "System Communication Detail" )]
    [Category( "Communication" )]
    [Description( "Allows the administration of a system communication." )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [Rock.SystemGuid.EntityTypeGuid( "D57F3F36-26DB-4413-BF62-FA5C9D37E686" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "EFFC90F0-E93E-45C3-8750-F75AF68F9027" )]
    [Rock.SystemGuid.BlockTypeGuid( Rock.SystemGuid.BlockType.SYSTEM_COMMUNICATION_DETAIL )]
    public class SystemCommunicationDetail : RockBlockType, IBreadCrumbBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters.
        /// </summary>
        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var systemCommunication = GetSystemCommunication();
            var currentPerson = GetCurrentPerson();

            var isNew = systemCommunication == null;

            var bag = isNew
                ? new SystemCommunicationBag
                {
                    IsActive = true,
                    IsCssInliningEnabled = false
                }
                : GetBag( systemCommunication );

            var pushTransport = MediumContainer.GetActiveMediumComponentsWithActiveTransports()
                .FirstOrDefault( m => m.TypeGuid == SystemGuid.EntityType.COMMUNICATION_MEDIUM_PUSH_NOTIFICATION.AsGuid() )?.Transport;

            var box = new CustomBlockBox<SystemCommunicationBag, SystemCommunicationDetailOptionsBag>
            {
                Bag = bag,
                Options = new SystemCommunicationDetailOptionsBag
                {
                    IsNew = isNew,
                    IsReadOnly = !isNew && !systemCommunication.IsAuthorized( Authorization.EDIT, currentPerson ),
                    IsRockMobilePushTransportConfigured = pushTransport != null && pushTransport is IRockMobilePush,
                    SmsFromSystemPhoneNumbers = SystemPhoneNumberCache.All( false )
                        .Where( p => p.IsAuthorized( Authorization.VIEW, currentPerson ) )
                        .OrderBy( p => p.Order )
                        .ThenBy( p => p.Name )
                        .ToListItemBagList(),
                    PushMobileApplications = SiteCache.GetAllActiveSites()
                        .Where( s => s.SiteType == SiteType.Mobile )
                        .ToListItemBagList()
                },
                NavigationUrls = GetBoxNavigationUrls(),
                SecurityGrantToken = GetSecurityGrantToken()
            };

            return box;
        }

        /// <summary>
        /// Gets the bag representation of a system communication entity.
        /// </summary>
        /// <param name="systemCommunication">The system communication entity.</param>
        /// <returns>A bag containing the entity data.</returns>
        private SystemCommunicationBag GetBag( SystemCommunication systemCommunication )
        {
            var bag = new SystemCommunicationBag
            {
                Title = systemCommunication.Title,
                IsActive = systemCommunication.IsActive ?? false,
                Category = systemCommunication.Category.ToListItemBag(),
                FromName = systemCommunication.FromName,
                From = systemCommunication.From,
                To = systemCommunication.To,
                Cc = systemCommunication.Cc,
                Bcc = systemCommunication.Bcc,
                Subject = systemCommunication.Subject,
                Body = systemCommunication.Body,
                IsCssInliningEnabled = systemCommunication.CssInliningEnabled,
                LavaFields = systemCommunication.LavaFields,
                SmsFromSystemPhoneNumber = systemCommunication.SmsFromSystemPhoneNumber.ToListItemBag(),
                SmsMessage = systemCommunication.SMSMessage,
                PushTitle = systemCommunication.PushTitle,
                PushMessage = systemCommunication.PushMessage
            };

            if ( systemCommunication.PushOpenAction != null )
            {
                bag.PushOpenAction = ( PushOpenAction ) ( int ) systemCommunication.PushOpenAction.Value;
            }

            var pushData = systemCommunication.PushData.IsNotNullOrWhiteSpace()
                ? systemCommunication.PushData.FromJsonOrNull<PushData>() ?? new PushData()
                : new PushData();

            if ( systemCommunication.PushOpenAction == Rock.Utility.PushOpenAction.ShowDetails )
            {
                if ( pushData.MobileApplicationId.HasValue )
                {
                    var mobileApplication = SiteCache.Get( pushData.MobileApplicationId.Value );
                    if ( mobileApplication != null )
                    {
                        bag.PushMobileApplication = mobileApplication.ToListItemBag();
                    }
                }

                bag.PushOpenMessageJson = systemCommunication.PushOpenMessageJson;

                if ( systemCommunication.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                {
                    bag.PushOpenMessage = new StructuredContentHelper( systemCommunication.PushOpenMessageJson ).Render();
                }
                else
                {
                    bag.PushOpenMessage = systemCommunication.PushOpenMessage;
                }
            }
            else if ( systemCommunication.PushOpenAction == Rock.Utility.PushOpenAction.LinkToMobilePage )
            {
                if ( pushData.MobilePageId.HasValue )
                {
                    bag.PushMobilePage = PageCache.GetGuid( pushData.MobilePageId.Value )?.ToString()?.ToPageRouteValueBag();
                }

                bag.PushMobilePageQueryString = pushData.MobilePageQueryString;
            }
            else if ( systemCommunication.PushOpenAction == Rock.Utility.PushOpenAction.LinkToUrl )
            {
                bag.PushUrl = pushData.Url;
            }

            return bag;
        }

        /// <summary>
        /// Gets the system communication entity from the page parameter,
        /// falling back to the legacy "emailId" parameter.
        /// </summary>
        /// <returns>The system communication entity, or null if not found or creating new.</returns>
        private SystemCommunication GetSystemCommunication()
        {
            var communicationKey = PageParameter( PageParameterKey.CommunicationId );

            if ( communicationKey.IsNullOrWhiteSpace() )
            {
                // Fall back to the legacy parameter.
                communicationKey = PageParameter( "emailId" );
            }

            if ( communicationKey.IsNullOrWhiteSpace() || communicationKey == "0" )
            {
                return null;
            }

            return new SystemCommunicationService( this.RockContext )
                .Get( communicationKey, !PageCache.Layout.Site.DisablePredictableIds );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl()
            };
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</returns>
        private string GetSecurityGrantToken()
        {
            var securityGrant = new SecurityGrant();

            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.VIEW ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.EDIT ) );
            securityGrant.AddRule( new AssetAndFileManagerSecurityGrantRule( Authorization.DELETE ) );

            return securityGrant.ToToken();
        }

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageReference.Parameters );
            var entityKey = pageReference.GetPageParameter( PageParameterKey.CommunicationId );

            if ( entityKey.IsNullOrWhiteSpace() )
            {
                // Fall back to the legacy parameter.
                entityKey = pageReference.GetPageParameter( "emailId" );
            }

            if ( entityKey.IsNullOrWhiteSpace() )
            {
                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                    {
                        new BreadCrumbLink( "New System Communication", breadCrumbPageRef )
                    }
                };
            }
            else
            {
                var title = new SystemCommunicationService( this.RockContext )
                    .GetSelect( entityKey, f => f.Title, !PageCache.Layout.Site.DisablePredictableIds );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb>
                    {
                        new BreadCrumbLink( title ?? "New System Communication", breadCrumbPageRef )
                    }
                };
            }
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the system communication.
        /// </summary>
        /// <param name="bag">The save payload.</param>
        /// <returns>A block action result indicating success or failure.</returns>
        [BlockAction( "Save" )]
        public BlockActionResult Save( SystemCommunicationBag bag )
        {
            var currentPerson = GetCurrentPerson();
            var systemCommunication = GetSystemCommunication();

            var isNew = systemCommunication == null;
            if ( isNew )
            {
                systemCommunication = new SystemCommunication();
                new SystemCommunicationService( this.RockContext ).Add( systemCommunication );
            }

            if ( !isNew && !systemCommunication.IsAuthorized( Authorization.EDIT, currentPerson ) )
            {
                return ActionForbidden( "You do not have permission to edit this system communication." );
            }

            // Core fields.
            systemCommunication.IsActive = bag.IsActive;
            systemCommunication.Title = bag.Title;

            var newCategoryGuid = bag.Category?.Value?.AsGuidOrNull();
            if ( systemCommunication.Category?.Guid != newCategoryGuid )
            {
                if ( newCategoryGuid.HasValue )
                {
                    systemCommunication.Category = new CategoryService( this.RockContext ).Get( newCategoryGuid.Value );
                    systemCommunication.CategoryId = systemCommunication.Category?.Id;
                }
                else
                {
                    systemCommunication.Category = null;
                    systemCommunication.CategoryId = null;
                }
            }

            // Email fields.
            systemCommunication.FromName = bag.FromName;
            systemCommunication.From = bag.From;
            systemCommunication.To = bag.To;
            systemCommunication.Cc = bag.Cc;
            systemCommunication.Bcc = bag.Bcc;
            systemCommunication.Subject = bag.Subject;
            systemCommunication.Body = bag.Body;
            systemCommunication.CssInliningEnabled = bag.IsCssInliningEnabled;
            systemCommunication.LavaFields = bag.LavaFields;

            // SMS fields.
            var newSmsFromPhoneNumberGuid = bag.SmsFromSystemPhoneNumber?.Value?.AsGuidOrNull();
            if ( systemCommunication.SmsFromSystemPhoneNumber?.Guid != newSmsFromPhoneNumberGuid )
            {
                if ( newSmsFromPhoneNumberGuid.HasValue )
                {
                    systemCommunication.SmsFromSystemPhoneNumber = new SystemPhoneNumberService( this.RockContext ).Get( newSmsFromPhoneNumberGuid.Value );
                    systemCommunication.SmsFromSystemPhoneNumberId = systemCommunication.SmsFromSystemPhoneNumber?.Id;
                }
                else
                {
                    systemCommunication.SmsFromSystemPhoneNumber = null;
                    systemCommunication.SmsFromSystemPhoneNumberId = null;
                }
            }

            systemCommunication.SMSMessage = bag.SmsMessage;

            // Push fields.
            systemCommunication.PushTitle = bag.PushTitle;
            systemCommunication.PushMessage = bag.PushMessage;
            systemCommunication.PushOpenAction = ( Rock.Utility.PushOpenAction ) ( int ) bag.PushOpenAction;

            var pushData = new PushData();
            var newPushOpenMessageJson = default( string );
            var newPushOpenMessage = default( string );

            if ( systemCommunication.PushOpenAction == Rock.Utility.PushOpenAction.ShowDetails )
            {
                if ( bag.PushOpenMessageJson.IsNotNullOrWhiteSpace() )
                {
                    newPushOpenMessageJson = bag.PushOpenMessageJson;
                    newPushOpenMessage = new StructuredContentHelper( bag.PushOpenMessageJson ).Render();
                }
                else
                {
                    newPushOpenMessage = bag.PushOpenMessage;
                }

                var newMobileApplicationGuid = bag.PushMobileApplication?.Value?.AsGuidOrNull();
                if ( newMobileApplicationGuid.HasValue )
                {
                    pushData.MobileApplicationId = SiteCache.GetId( newMobileApplicationGuid.Value );
                }
                else
                {
                    pushData.MobileApplicationId = null;
                }
            }
            else if ( systemCommunication.PushOpenAction == Rock.Utility.PushOpenAction.LinkToMobilePage )
            {
                pushData.MobilePageQueryString = bag.PushMobilePageQueryString;

                var newMobilePageGuid = bag.PushMobilePage?.Page?.Value?.AsGuidOrNull();
                if ( newMobilePageGuid.HasValue )
                {
                    var newMobilePage = PageCache.Get( newMobilePageGuid.Value );
                    pushData.MobilePageId = newMobilePage?.Id;
                    pushData.MobileApplicationId = newMobilePage?.SiteId;
                }
                else
                {
                    pushData.MobilePageId = null;
                    pushData.MobileApplicationId = null;
                }
            }
            else if ( systemCommunication.PushOpenAction == Rock.Utility.PushOpenAction.LinkToUrl )
            {
                pushData.Url = bag.PushUrl;
            }

            new StructuredContentHelper( newPushOpenMessageJson )
                .DetectAndApplyDatabaseChanges( systemCommunication.PushOpenMessageJson, RockContext );

            systemCommunication.PushOpenMessage = newPushOpenMessage;
            systemCommunication.PushOpenMessageJson = newPushOpenMessageJson;
            systemCommunication.PushData = pushData.ToJson();

            // Validate entity.
            if ( !systemCommunication.IsValid )
            {
                var validationErrorMessage = systemCommunication.ValidationResults
                    .Select( a => a.ErrorMessage )
                    .ToList()
                    .AsDelimited( "\n" );

                return ActionBadRequest( validationErrorMessage );
            }

            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Gets a preview of the email message with Lava merge fields resolved.
        /// </summary>
        /// <param name="bag">The preview request.</param>
        /// <returns>The rendered HTML preview.</returns>
        [BlockAction( "GetPreviewMessage" )]
        public BlockActionResult GetPreviewMessage( SystemCommunicationDetailGetPreviewMessageRequestBag bag )
        {
            if ( bag.Message.IsNullOrWhiteSpace() )
            {
                return ActionOk( string.Empty );
            }

            var mergeFields = this.RequestContext.GetCommonMergeFields();
            var resolvedPreviewHtml = bag.Message.ResolveMergeFields( mergeFields );

            if ( bag.IsCssInlined )
            {
                resolvedPreviewHtml = resolvedPreviewHtml.ConvertHtmlStylesToInlineAttributes();
            }

            return ActionOk( resolvedPreviewHtml );
        }

        #endregion Block Actions
    }
}
