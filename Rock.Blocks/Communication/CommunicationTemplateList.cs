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
using System.Data.Entity;
using System.Linq;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Communication.CommunicationTemplateList;
using Rock.Web.Cache;

namespace Rock.Blocks.Communication
{
    /// <summary>
    /// Lists the available communication templates that can be used when creating new communications.
    /// </summary>
    [DisplayName( "Communication Template List" )]
    [Category( "Communication" )]
    [Description( "Lists the available communication templates that can be used when creating new communications." )]
    [IconCssClass( "ti ti-message" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage,
        Order = 0 )]

    [BooleanField(
        "Personal Templates View",
        Description = "Is the block being used to display personal templates (only templates that current user is allowed to edit)?",
        DefaultBooleanValue = false,
        Key = AttributeKey.PersonalTemplatesView,
        Order = 1 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "E3E33452-302C-4976-8682-59FC720F87C2" )]
    // was [Rock.SystemGuid.BlockTypeGuid( "13FD30DB-06BD-402D-AA1A-BD682F65C7A7" )]
    [Rock.SystemGuid.BlockTypeGuid( "EACDBBD4-C355-4D38-B604-779BC55D3876" )]
    [CustomizedGrid]
    public class CommunicationTemplateList : RockEntityListBlockType<CommunicationTemplate>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
            public const string PersonalTemplatesView = "PersonalTemplatesView";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class PageParameterKey
        {
            public const string TemplateId = "TemplateId";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<CommunicationTemplateListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = true; // matches webforms
            box.IsDeleteEnabled = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private CommunicationTemplateListOptionsBag GetBoxOptions()
        {
            var currentPerson = GetCurrentPerson();

            return new CommunicationTemplateListOptionsBag
            {
                IsBlockAuthorizedToEdit = BlockCache.IsAuthorized( Authorization.EDIT, currentPerson ),
                IsSecurityColumnVisible = BlockCache.IsAuthorized( Authorization.ADMINISTRATE, currentPerson )
            };
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, PageParameterKey.TemplateId, "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<CommunicationTemplate> GetListQueryable( RockContext rockContext )
        {
            return new CommunicationTemplateService( rockContext ).Queryable()
                .Include( ct => ct.Category )
                .Include( ct => ct.CreatedByPersonAlias.Person )
                .Where( ct => ct.UsageType == null ); // By default, exclude templates with a specified usage type (e.g., Communication Flows)
        }

        /// <inheritdoc/>
        protected override List<CommunicationTemplate> GetListItems( IQueryable<CommunicationTemplate> queryable, RockContext rockContext )
        {
            var items = queryable.ToList();

            var currentPerson = GetCurrentPerson();
            var isPersonalView = GetAttributeValue( AttributeKey.PersonalTemplatesView ).AsBoolean();

            // Filter to only templates the current person can view (or edit if personal templates view).
            var authorizationType = isPersonalView ? Authorization.EDIT : Authorization.VIEW;

            return items.Where( ct => ct.IsAuthorized( authorizationType, currentPerson ) ).ToList();
        }

        /// <inheritdoc/>
        protected override IQueryable<CommunicationTemplate> GetOrderedListQueryable( IQueryable<CommunicationTemplate> queryable, RockContext rockContext )
        {
            return queryable.OrderBy( ct => ct.Name );
        }

        /// <inheritdoc/>
        protected override GridBuilder<CommunicationTemplate> GetGridBuilder()
        {
            return new GridBuilder<CommunicationTemplate>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "subject", a => a.Subject )
                .AddTextField( "category", a => a.Category?.Name )
                .AddTextField( "supports", a => GetSupportsText( a ) )
                .AddPersonField( "createdBy", a => a.CreatedByPersonAlias?.Person )
                .AddTextField( "description", a => a.Description )
                .AddTextField( "version", a => a.Version.ConvertToString() )
                .AddField( "isActive", a => a.IsActive )
                .AddField( "isSystem", a => a.IsSystem )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) );
        }

        /// <summary>
        /// Gets the comma-separated supports text for the given communication template.
        /// </summary>
        /// <param name="template">The communication template.</param>
        /// <returns>A comma-separated string of support labels.</returns>
        private string GetSupportsText( CommunicationTemplate template )
        {
            var labels = new List<string>();

            if ( template.SupportsEmailWizard() )
            {
                labels.Add( "Email Wizard" );
            }
            else if ( !string.IsNullOrEmpty( template.Message ) )
            {
                labels.Add( "Simple Email Template" );
            }

            if ( template.Guid == SystemGuid.Communication.COMMUNICATION_TEMPLATE_BLANK.AsGuid() || template.HasSMSTemplate() )
            {
                labels.Add( "SMS" );
            }

            if ( !string.IsNullOrWhiteSpace( template.PushMessage ) )
            {
                labels.Add( "Push" );
            }

            return string.Join( ", ", labels );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Checks whether the specified communication template has any linked communications.
        /// </summary>
        /// <param name="key">The identifier of the entity to check.</param>
        /// <returns>A boolean indicating whether linked communications exist.</returns>
        [BlockAction]
        public BlockActionResult HasLinkedCommunications( string key )
        {
            var entity = new CommunicationTemplateService( RockContext ).Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{CommunicationTemplate.FriendlyTypeName} not found." );
            }

            var hasLinkedCommunications = new CommunicationService( RockContext ).Queryable()
                .Any( c => c.CommunicationTemplateId == entity.Id );

            return ActionOk( hasLinkedCommunications );
        }

        /// <summary>
        /// Deletes the specified communication template.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new CommunicationTemplateService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{CommunicationTemplate.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete this {CommunicationTemplate.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        /// <summary>
        /// Copies the specified communication template by creating a clone.
        /// </summary>
        /// <param name="key">The identifier of the entity to be copied.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Copy( string key )
        {
            var entityService = new CommunicationTemplateService( RockContext );
            var communicationTemplate = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( communicationTemplate == null )
            {
                return ActionBadRequest( $"{CommunicationTemplate.FriendlyTypeName} not found." );
            }

            if ( !communicationTemplate.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to copy this {CommunicationTemplate.FriendlyTypeName}." );
            }

            var copyNumber = 0;
            var copyName = "Copy of " + communicationTemplate.Name;

            // Find a unique name, excluding Communication Flows templates.
            while ( entityService.Queryable().Any( a => a.Name == copyName && a.UsageType == null ) )
            {
                copyNumber++;
                copyName = string.Format( "Copy({0}) of {1}", copyNumber, communicationTemplate.Name );
            }

            var templateCopy = communicationTemplate.CloneWithoutIdentity();
            templateCopy.Name = copyName.Truncate( 100 );
            templateCopy.IsSystem = false;
            templateCopy.LogoBinaryFileId = null;
            templateCopy.ImageFileId = null;

            entityService.Add( templateCopy );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
