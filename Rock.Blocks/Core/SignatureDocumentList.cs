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

using DotLiquid;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Obsidian.UI;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.SignatureDocumentList;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays a list of signature documents.
    /// </summary>

    [DisplayName( "Signature Document List" )]
    [Category( "Core" )]
    [Description( "Displays a list of signature documents." )]
    [IconCssClass( "fa fa-list" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    [LinkedPage( "Detail Page",
        Description = "The page that will show the signature document details.",
        Key = AttributeKey.DetailPage )]

    [Rock.SystemGuid.EntityTypeGuid( "b4526eb4-3ca4-47be-b686-4b9fbee2bf4d" )]
    [Rock.SystemGuid.BlockTypeGuid( "6076609b-d4d2-4825-8bb2-8681e99c59f2" )]
    [CustomizedGrid]
    public class SignatureDocumentList : RockEntityListBlockType<SignatureDocument>
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        private static class NavigationUrlKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new ListBlockBox<SignatureDocumentListOptionsBag>();
            var builder = GetGridBuilder();

            box.IsAddEnabled = false;
            box.IsDeleteEnabled = false;
            box.ExpectedRowCount = null;
            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions();
            box.GridDefinition = builder.BuildDefinition();

            //int? documentTypeId = PageParameter( "SignatureDocumentTemplateId" ).AsIntegerOrNull();
            //if ( documentTypeId.HasValue && documentTypeId.Value != 0 )
            //{
            //    var signatureDocumentTemplateService = new SignatureDocumentTemplateService( new RockContext() );
            //    var signatureDocumentTemplate = signatureDocumentTemplateService.Get( documentTypeId.Value );

            //    //Legacy Add is No Longer Supported in Webform Block
            //    //var isLegacyTemplate = signatureDocumentTemplateService.GetSelect( documentTypeId.Value, s => s.ProviderEntityTypeId.HasValue );
            //    //box.IsAddEnabled = isLegacyTemplate && new SignatureDocument().IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            //    // Following the same logic as the Signature Document Detail to hide the Block if the Current Person is not authorized to view.
            //    bool canEdit = signatureDocumentTemplate?.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) ?? false;
            //    bool canView = canEdit || ( signatureDocumentTemplate?.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson ) ?? false );

            //    if ( !canView )
            //    {
            //        //box.ErrorMessage
            //        //pnlContent.Visible = false;
            //    }
            //}

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the list.
        /// </summary>
        /// <returns>The options that provide additional details to the block.</returns>
        private SignatureDocumentListOptionsBag GetBoxOptions()
        {
            var options = new SignatureDocumentListOptionsBag();

            // in case this is used as a Person Block, set the TargetPerson 
            Person targetPerson = this.RequestContext.GetContextEntity<Person>();

            if ( targetPerson != null )
            {
                options.ShowDocumentType = true;
            }
            else
            {
                options.ShowDocumentType = false;
            }

            return options;
        }

        ///// <summary>
        ///// Determines if the delete button should be enabled in the grid.
        ///// <summary>
        ///// <returns>A boolean value that indicates if the delete button should be enabled.</returns>
        //private bool GetIsDeleteEnabled()
        //{
        //    return BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );
        //}

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();
            qryParams.Add( "SignatureDocumentId", "((Key))" );
            Person targetPerson = this.RequestContext.GetContextEntity<Person>();
            if ( targetPerson != null )
            {
                qryParams.Add( "PersonId", targetPerson.Id.ToString() );
            }

            return new Dictionary<string, string>
            {
                [NavigationUrlKey.DetailPage] = this.GetLinkedPageUrl( AttributeKey.DetailPage, qryParams )
            };
        }

        /// <inheritdoc/>
        protected override IQueryable<SignatureDocument> GetListQueryable( RockContext rockContext )
        {
            var qry = base.GetListQueryable( rockContext )
                .Include( a => a.AppliesToPersonAlias )
                .Include( a => a.AssignedToPersonAlias )
                .Include( a => a.SignedByPersonAlias )
                .Include( a => a.SignatureDocumentTemplate );

            // in case this is used as a Person Block, set the TargetPerson 
            Person targetPerson = this.RequestContext.GetContextEntity<Person>();

            if ( targetPerson != null )
            {
                qry = qry.Where( d =>
                    ( d.AppliesToPersonAlias != null && d.AppliesToPersonAlias.PersonId == targetPerson.Id ) ||
                    ( d.AssignedToPersonAlias != null && d.AssignedToPersonAlias.PersonId == targetPerson.Id ) ||
                    ( d.SignedByPersonAlias != null && d.SignedByPersonAlias.PersonId == targetPerson.Id ) );
            }
            else
            {
                int? documentTypeId = PageParameter( "SignatureDocumentTemplateId" ).AsIntegerOrNull();
                if ( documentTypeId.HasValue && documentTypeId.Value != 0 )
                {
                    qry = qry.Where( d =>
                        d.SignatureDocumentTemplateId == documentTypeId.Value );
                }
            }

            return qry;
        }

        /// <inheritdoc/>
        protected override GridBuilder<SignatureDocument> GetGridBuilder()
        {
            return new GridBuilder<SignatureDocument>()
                .WithBlock( this )
                .AddTextField( "idKey", a => a.IdKey )
                .AddTextField( "name", a => a.Name )
                .AddTextField( "documentKey", a => a.DocumentKey )
                .AddTextField( "documentType", a => a.SignatureDocumentTemplate?.Name )
                .AddPersonField( "appliesToPersonAlias", a => a.AppliesToPersonAlias?.Person )
                .AddPersonField( "assignedToPersonAlias", a => a.AssignedToPersonAlias?.Person )
                .AddPersonField( "signedByPersonAlias", a => a.SignedByPersonAlias?.Person )
                .AddTextField( "status", a => a.Status.ToString() )
                .AddDateTimeField( "lastInviteDate", a => a.LastInviteDate )
                .AddDateTimeField( "signedDateTime", a => a.SignedDateTime )
                .AddTextField( "fileText", a => a.BinaryFileId.HasValue ? "<i class='fa fa-file-alt fa-lg'></i>" : "<i class='fa fa-exclamation-triangle text-danger' title='File deleted'></i>" )
                .AddField( "fileGuid", a => a.BinaryFile == null ? Guid.Empty : a.BinaryFile.Guid )
                .AddField( "isSecurityDisabled", a => !a.IsAuthorized( Authorization.ADMINISTRATE, RequestContext.CurrentPerson ) )
                .AddAttributeFields( GetGridAttributes() );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>An empty result that indicates if the operation succeeded.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new SignatureDocumentService( RockContext );
            var entity = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( entity == null )
            {
                return ActionBadRequest( $"{SignatureDocument.FriendlyTypeName} not found." );
            }

            if ( !entity.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to delete ${SignatureDocument.FriendlyTypeName}." );
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            // Delete the binary file associated with the Signature Document
            if ( entity.BinaryFile != null )
            {
                var binaryFileService = new BinaryFileService( RockContext );
                binaryFileService.Delete( entity.BinaryFile );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk();
        }

        #endregion
    }
}
