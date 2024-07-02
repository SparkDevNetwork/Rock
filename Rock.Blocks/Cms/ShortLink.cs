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
using System.ComponentModel;
using System.Linq;

using Rock;
using Rock.Attribute;
using Rock.Blocks;
using Rock.Data;
using Rock.Model;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CMS.PageShortLinkDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Cms
{
    /// <summary>
    ///
    /// </summary>
    [DisplayName( "Shortened Links" )]
    [Category( "Administration" )]
    [Description( "Displays a dialog for adding a short link to the current page." )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [IntegerField( "Minimum Token Length",
        Key = AttributeKey.MinimumTokenLength,
        Description = "The minimum number of characters for the token.",
        IsRequired = false,
        DefaultIntegerValue = 7,
        Order = 0 )]

    #endregion Block Attributes

    [Rock.SystemGuid.EntityTypeGuid( "026C6A93-5295-43E9-B67D-C3708ACB25B9" )]
    [Rock.SystemGuid.BlockTypeGuid( "C85551E8-A363-4AA6-9BFD-E6A1C9CEDE80" )]
    public class ShortLink : RockBlockType
    {
        #region Keys

        private static class AttributeKey
        {
            public const string MinimumTokenLength = "MinimumTokenLength";
        }

        private static class PageParameterKey
        {
            public const string Url = "Url";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var minTokenLength = GetAttributeValue( AttributeKey.MinimumTokenLength ).AsIntegerOrNull() ?? 7;

            var box = new DetailBlockBox<PageShortLinkBag, PageShortLinkDetailOptionsBag>();
            box.Options = GetBoxOptions();
            var defaultSite = box.Options.SiteOptions.FirstOrDefault();
            var defaultSiteId = SiteCache.GetId( defaultSite?.Value.AsGuid() ?? Guid.Empty ) ?? 0;

            box.Entity = new PageShortLinkBag();
            box.Entity.Site = defaultSite ?? new ListItemBag();
            box.Entity.Url = PageParameter( PageParameterKey.Url );
            box.Entity.DefaultDomainURL = new SiteService( RockContext )
                .GetDefaultDomainUri( defaultSiteId )
                .ToString();
            if ( defaultSiteId != 0 )
            {
                box.Entity.Token = new PageShortLinkService( RockContext ).GetUniqueToken( SiteCache.GetId( box.Entity.Site.Value.AsGuid() ) ?? 0, minTokenLength );
            }

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private PageShortLinkDetailOptionsBag GetBoxOptions()
        {
            var options = new PageShortLinkDetailOptionsBag
            {
                SiteOptions = SiteCache
                    .All()
                    .Where( site => site.EnabledForShortening )
                    .OrderBy( site => site.Name )
                    .Select( site => new ListItemBag
                    {
                        Value = site.Guid.ToString(),
                        Text = site.Name
                    } )
                   .ToList(),
            };

            return options;
        }

        /// <summary>
        /// Validates the PageShortLink for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="pageShortLink">The PageShortLink to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the PageShortLink is valid, <c>false</c> otherwise.</returns>
        private bool ValidatePageShortLink( PageShortLink pageShortLink, PageShortLinkService service, out string errorMessage )
        {
            errorMessage = null;

            // should have a token of minimum length
            var minTokenLength = GetAttributeValue( AttributeKey.MinimumTokenLength ).AsIntegerOrNull() ?? 7;
            if ( pageShortLink.Token.Length < minTokenLength )
            {
                errorMessage = string.Format( "Please enter a token that is a least {0} characters long.", minTokenLength );
                return false;
            }

            // should have a token that is unique for the siteId
            bool isTokenUsedBySite = !service.VerifyUniqueToken( pageShortLink.SiteId, pageShortLink.Id, pageShortLink.Token );
            if ( isTokenUsedBySite )
            {
                errorMessage = "The selected token is already being used. Please enter a different token.";
                return false;
            }

            if ( pageShortLink.SiteId == 0 )
            {
                errorMessage = "Please select a valid site.";
                return false;
            }

            if ( pageShortLink.Url.IsNullOrWhiteSpace() )
            {
                errorMessage = "Please enter a valid URL.";
                return false;
            }

            if ( !pageShortLink.IsValid )
            {
                errorMessage = pageShortLink.ValidationResults.Select( x => x.ErrorMessage ).JoinStrings( "," );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( PageShortLink entity, ValidPropertiesBox<PageShortLinkBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Site ),
                () => entity.SiteId = SiteCache.GetId( box.Bag.Site.Value.AsGuid() ).ToIntSafe() );

            box.IfValidProperty( nameof( box.Bag.Token ),
                () => entity.Token = box.Bag.Token );

            box.IfValidProperty( nameof( box.Bag.Url ),
                () => entity.Url = box.Bag.Url );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            var utmSettings = entity.GetAdditionalSettings<PageShortLink.UtmSettings>();

            box.IfValidProperty( nameof( box.Bag.UtmSourceValue ), () =>
            {
                utmSettings.UtmSourceValueId = DefinedValueCache.GetId( ( box.Bag.UtmSourceValue?.Value ).AsGuid() );
            } );

            box.IfValidProperty( nameof( box.Bag.UtmMediumValue ), () =>
            {
                utmSettings.UtmMediumValueId = DefinedValueCache.GetId( ( box.Bag.UtmMediumValue?.Value ).AsGuid() );
            } );

            box.IfValidProperty( nameof( box.Bag.UtmCampaignValue ), () =>
            {
                utmSettings.UtmCampaignValueId = DefinedValueCache.GetId( ( box.Bag.UtmCampaignValue?.Value ).AsGuid() );
            } );

            box.IfValidProperty( nameof( box.Bag.UtmTerm ), () =>
            {
                utmSettings.UtmTerm = box.Bag.UtmTerm;
            } );

            box.IfValidProperty( nameof( box.Bag.UtmContent ), () =>
            {
                utmSettings.UtmContent = box.Bag.UtmContent;
            } );

            entity.SetAdditionalSettings( utmSettings );

            return true;
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<PageShortLinkBag> box )
        {
            var entityService = new PageShortLinkService( RockContext );
            var entity = new PageShortLink();

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidatePageShortLink( entity, entityService, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }
            entityService.Add( entity );

            RockContext.SaveChanges();

            return ActionOk();
        }
        #endregion
    }
}
