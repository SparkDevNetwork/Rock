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
using System.IO;
using System.Linq;

using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.Core.BinaryFileDetail;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Blocks.Core
{
    /// <summary>
    /// Displays the details of a particular binary file.
    /// </summary>

    [DisplayName( "Binary File Detail" )]
    [Category( "Core" )]
    [Description( "Shows the details of a particular binary file item." )]
    [IconCssClass( "fa fa-question" )]
    // [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [BooleanField( "Show Binary File Type",
        Key = AttributeKey.ShowBinaryFileType )]

    [LinkedPage( "Edit Label Page",
        Description = "Page used to edit and test the contents of a label file.",
        IsRequired = false,
        Order = 0,
        Key = AttributeKey.EditLabelPage )]

    [WorkflowTypeField( "Workflow",
        Description = "An optional workflow to activate for any new file uploaded",
        AllowMultiple = false,
        IsRequired = false,
        Category = "Advanced",
        Order = 0,
        Key = AttributeKey.Workflow )]

    [TextField( "Workflow Button Text",
        Description = "The button text to show for the rerun workflow button.",
        IsRequired = false,
        DefaultValue = "Rerun Workflow",
        Category = "Advanced",
        Order = 1,
        Key = AttributeKey.WorkflowButtonText )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "de112a87-a1bc-46cc-bea1-d5d658ab1e3a" )]
    [Rock.SystemGuid.BlockTypeGuid( "d0d4fcb2-e21e-4287-9416-81ba60b90f40" )]
    public class BinaryFileDetail : RockDetailBlockType
    {
        #region Keys

        public static class AttributeKey
        {
            public const string ShowBinaryFileType = "ShowBinaryFileType";
            public const string EditLabelPage = "EditLabelPage";
            public const string Workflow = "Workflow";
            public const string WorkflowButtonText = "WorkflowButtonText";
        }

        private static class PageParameterKey
        {
            public const string BinaryFileId = "BinaryFileId";
            public const string BinaryFileTypeId = "BinaryFileTypeId";
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
            using ( var rockContext = new RockContext() )
            {
                var box = new DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag>();

                SetBoxInitialEntityState( box, rockContext );

                box.NavigationUrls = GetBoxNavigationUrls();
                box.Options = GetBoxOptions( box.IsEditable, rockContext );
                box.QualifiedAttributeProperties = AttributeCache.GetAttributeQualifiedColumns<BinaryFile>();

                return box;
            }
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private BinaryFileDetailOptionsBag GetBoxOptions( bool isEditable, RockContext rockContext )
        {
            var options = new BinaryFileDetailOptionsBag();

            return options;
        }

        /// <summary>
        /// Validates the BinaryFile for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="binaryFile">The BinaryFile to be validated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the BinaryFile is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBinaryFile( BinaryFile binaryFile, RockContext rockContext, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        /// <param name="rockContext">The rock context.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag> box, RockContext rockContext )
        {
            var entity = GetInitialEntity( rockContext );

            if ( entity == null )
            {
                box.ErrorMessage = $"The {BinaryFile.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            if ( entity.Id == 0 )
            {
                entity.BinaryFileTypeId = PageParameter( PageParameterKey.BinaryFileTypeId ).AsIntegerOrNull();
            }

            entity.LoadAttributes( rockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( BinaryFile.FriendlyTypeName );
                }
            }
            else
            {
                // New entity is being created, prepare for edit mode by default.
                if ( box.IsEditable )
                {
                    box.Entity = GetEntityBagForEdit( entity, rockContext );
                    box.SecurityGrantToken = GetSecurityGrantToken( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( BinaryFile.FriendlyTypeName );
                }
            }
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="BinaryFileBag"/> that represents the entity.</returns>
        private BinaryFileBag GetCommonEntityBag( BinaryFile entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = new BinaryFileBag
            {
                IdKey = entity.IdKey,
                BinaryFileType = entity.BinaryFileType.ToListItemBag(),
                Description = entity.Description,
                File = entity.ToListItemBag(),
                FileName = entity.FileName,
                MimeType = entity.MimeType,
                ShowBinaryFileType = GetAttributeValue( AttributeKey.ShowBinaryFileType ).AsBoolean(),
                WorkflowButtonText = GetAttributeValue( AttributeKey.WorkflowButtonText ),
                ShowWorkflowButton = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull().HasValue,
            };

            bag.IsLabelFile = IsLabelFile( bag );

            var binaryFileTypeId = PageParameter( PageParameterKey.BinaryFileTypeId ).AsIntegerOrNull();
            if ( bag.BinaryFileType == null && binaryFileTypeId.HasValue )
            {
                var binaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFileTypeId.Value );
                bag.BinaryFileType = binaryFileType.ToListItemBag();
            }

            return bag;
        }

        /// <summary>
        /// Gets the bag for viewing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for view purposes.</param>
        /// <returns>A <see cref="BinaryFileBag"/> that represents the entity.</returns>
        private BinaryFileBag GetEntityBagForView( BinaryFile entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Gets the bag for editing the specified entity.
        /// </summary>
        /// <param name="entity">The entity to be represented for edit purposes.</param>
        /// <returns>A <see cref="BinaryFileBag"/> that represents the entity.</returns>
        private BinaryFileBag GetEntityBagForEdit( BinaryFile entity, RockContext rockContext )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity, rockContext );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <summary>
        /// Updates the entity from the data in the save box.
        /// </summary>
        /// <param name="entity">The entity to be updated.</param>
        /// <param name="box">The box containing the information to be updated.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns><c>true</c> if the box was valid and the entity was updated, <c>false</c> otherwise.</returns>
        private bool UpdateEntityFromBox( BinaryFile entity, DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag> box, RockContext rockContext )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Entity.BinaryFileType ),
                () => entity.BinaryFileTypeId = box.Entity.BinaryFileType.GetEntityId<BinaryFileType>( rockContext ) );

            box.IfValidProperty( nameof( box.Entity.Description ),
                () => entity.Description = box.Entity.Description );

            box.IfValidProperty( nameof( box.Entity.FileName ),
                () => entity.FileName = box.Entity.FileName );

            box.IfValidProperty( nameof( box.Entity.MimeType ),
                () => entity.MimeType = box.Entity.MimeType );

            box.IfValidProperty( nameof( box.Entity.File ),
                () => SaveFile( box.Entity, rockContext, entity ) );

            box.IfValidProperty( nameof( box.Entity.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( rockContext );

                    entity.SetPublicAttributeValues( box.Entity.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <summary>
        /// Gets the initial entity from page parameters or creates a new entity
        /// if page parameters requested creation.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns>The <see cref="BinaryFile"/> to be viewed or edited on the page.</returns>
        private BinaryFile GetInitialEntity( RockContext rockContext )
        {
            return GetInitialEntity<BinaryFile, BinaryFileService>( rockContext, PageParameterKey.BinaryFileId );
        }

        /// <summary>
        /// Gets the box navigation URLs required for the page to operate.
        /// </summary>
        /// <returns>A dictionary of key names and URL values.</returns>
        private Dictionary<string, string> GetBoxNavigationUrls()
        {
            return new Dictionary<string, string>
            {
                [NavigationUrlKey.ParentPage] = this.GetParentPageUrl(),
                [AttributeKey.EditLabelPage] =  this.GetLinkedPageUrl( AttributeKey.EditLabelPage, new Dictionary<string, string> { { "BinaryFileId", "((Key))" } } )
            };
        }

        /// <inheritdoc/>
        protected override string RenewSecurityGrantToken()
        {
            using ( var rockContext = new RockContext() )
            {
                var entity = GetInitialEntity( rockContext );

                if ( entity != null )
                {
                    entity.LoadAttributes( rockContext );
                }

                return GetSecurityGrantToken( entity );
            }
        }

        /// <summary>
        /// Gets the security grant token that will be used by UI controls on
        /// this block to ensure they have the proper permissions.
        /// </summary>
        /// <returns>A string that represents the security grant token.</string>
        private string GetSecurityGrantToken( BinaryFile entity )
        {
            var securityGrant = new Rock.Security.SecurityGrant();

            securityGrant.AddRulesForAttributes( entity, RequestContext.CurrentPerson );

            return securityGrant.ToToken();
        }

        /// <summary>
        /// Attempts to load an entity to be used for an edit action.
        /// </summary>
        /// <param name="idKey">The identifier key of the entity to load.</param>
        /// <param name="rockContext">The database context to load the entity from.</param>
        /// <param name="entity">Contains the entity that was loaded when <c>true</c> is returned.</param>
        /// <param name="error">Contains the action error result when <c>false</c> is returned.</param>
        /// <returns><c>true</c> if the entity was loaded and passed security checks.</returns>
        private bool TryGetEntityForEditAction( string idKey, RockContext rockContext, out BinaryFile entity, out BlockActionResult error )
        {
            var entityService = new BinaryFileService( rockContext );
            error = null;

            // Determine if we are editing an existing entity or creating a new one.
            if ( idKey.IsNotNullOrWhiteSpace() )
            {
                // If editing an existing entity then load it and make sure it
                // was found and can still be edited.
                entity = entityService.Get( idKey, !PageCache.Layout.Site.DisablePredictableIds );
            }
            else
            {
                // Create a new entity.
                entity = new BinaryFile();
                entity.BinaryFileTypeId = PageParameter( PageParameterKey.BinaryFileTypeId ).AsIntegerOrNull();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{BinaryFile.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${BinaryFile.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        /// <summary>
        /// Launches the file upload workflow.
        /// </summary>
        /// <param name="binaryFile">The binary file.</param>
        /// <param name="binaryFileService">The binary file service.</param>
        private BinaryFileBag LaunchFileUploadWorkflow( BinaryFile binaryFile, BinaryFileService binaryFileService )
        {
            // Process uploaded file using an optional workflow (which will probably populate attribute values)
            if ( Guid.TryParse( GetAttributeValue( AttributeKey.Workflow ), out Guid workflowTypeGuid ) )
            {
                // create a rockContext for the workflow so that it can save it's changes, without 
                var workflowRockContext = new RockContext();
                var workflowType = WorkflowTypeCache.Get( workflowTypeGuid );
                if ( workflowType != null && ( workflowType.IsActive ?? true ) )
                {
                    var workflow = Rock.Model.Workflow.Activate( workflowType, binaryFile.FileName );

                    if ( new Rock.Model.WorkflowService( workflowRockContext ).Process( workflow, binaryFile, out List<string> workflowErrors ) )
                    {
                        binaryFile = binaryFileService.Get( binaryFile.Id );
                    }

                    var bag = GetEntityBagForEdit( binaryFile, binaryFileService.Context as RockContext );
                    bag.WorkflowNotificationMessage = $"Successfully processed a <strong>{workflowType.Name}</strong> workflow!";
                    return bag;
                }
            }

            return GetEntityBagForEdit( binaryFile, binaryFileService.Context as RockContext );
        }

        /// <summary>
        /// Gets the identifier as an integer from an idKey.
        /// </summary>
        /// <param name="idKey">The identifier key.</param>
        /// <returns></returns>
        private int? GetId( string idKey )
        {
            int? id = !PageCache.Layout.Site.DisablePredictableIds ? idKey.AsIntegerOrNull() : null;

            if ( !id.HasValue )
            {
                id = Rock.Utility.IdHasher.Instance.GetId( idKey );
            }

            return id;
        }

        /// <summary>
        /// Deletes any orphaned files, i.e. files that were uploaded but not persisted as the content for the binary file.
        /// </summary>
        /// <param name="orphanedBinaryFileIdList">The orphaned binary file identifier list.</param>
        /// <param name="rockContext">The rock context.</param>
        private void DeleteOrphanedFiles( List<Guid> orphanedBinaryFileIdList, RockContext rockContext )
        {
            var binaryFileService = new BinaryFileService( rockContext );
            foreach ( var tempBinaryFile in binaryFileService.Queryable().Where( b => orphanedBinaryFileIdList.Contains( b.Guid ) && b.IsTemporary ) )
            {
                binaryFileService.Delete( tempBinaryFile );
            }
        }

        /// <summary>
        /// Determines whether the instance is holding a label file
        /// </summary>
        /// <param name="binaryFileId">The binary file identifier.</param>
        /// <returns>
        ///   <c>true</c> if [is label file] [the specified binary file identifier]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsLabelFile( BinaryFileBag bag )
        {
            var binaryFileId = GetId( bag.IdKey );
            return ( binaryFileId.HasValue || bag.File.Value.AsGuid() != Guid.Empty )
                && !string.IsNullOrWhiteSpace( GetAttributeValue( AttributeKey.EditLabelPage ) )
                && ( bag.BinaryFileType?.Value.Equals( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL, StringComparison.OrdinalIgnoreCase ) == true );
        }

        #endregion

        #region Block Actions

        /// <summary>
        /// Gets the box that will contain all the information needed to begin
        /// the edit operation.
        /// </summary>
        /// <param name="key">The identifier of the entity to be edited.</param>
        /// <returns>A box that contains the entity and any other information required.</returns>
        [BlockAction]
        public BlockActionResult Edit( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                entity.LoadAttributes( rockContext );

                var box = new DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
                };

                return ActionOk( box );
            }
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new BinaryFileService( rockContext );

                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                var prevBinaryFileTypeId = entity.BinaryFileTypeId;

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Ensure everything is valid before saving.
                if ( !ValidateBinaryFile( entity, rockContext, out var validationMessage ) )
                {
                    return ActionBadRequest( validationMessage );
                }

                entity.IsTemporary = false;

                rockContext.WrapTransaction( () =>
                {
                    DeleteOrphanedFiles( box.Entity.OrphanedBinaryFileIdList, rockContext );

                    rockContext.SaveChanges();
                    entity.SaveAttributeValues( rockContext );
                } );

                Rock.CheckIn.KioskLabel.Remove( entity.Guid );

                if ( !prevBinaryFileTypeId.Equals( entity.BinaryFileTypeId ) )
                {
                    var checkInBinaryFileType = new BinaryFileTypeService( rockContext ).Get( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid() );
                    if ( checkInBinaryFileType != null && (
                        ( prevBinaryFileTypeId.HasValue && prevBinaryFileTypeId.Value == checkInBinaryFileType.Id ) ||
                        ( entity.BinaryFileTypeId.HasValue && entity.BinaryFileTypeId.Value == checkInBinaryFileType.Id ) ) )
                    {
                        Rock.CheckIn.KioskDevice.Clear();
                    }
                }

                return ActionContent( System.Net.HttpStatusCode.Created, this.GetParentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.BinaryFileId] = entity.IdKey
                } ) );
            }
        }

        private void SaveFile( BinaryFileBag bag, RockContext rockContext, BinaryFile entity )
        {
            var entityService = new BinaryFileService( rockContext );
            var binaryFileId = bag.File.GetEntityId<BinaryFile>( rockContext );
            if ( binaryFileId.HasValue && entity.Id != binaryFileId.Value )
            {
                var uploadedBinaryFile = entityService.Get( binaryFileId.Value );
                if ( uploadedBinaryFile != null )
                {
                    entity.BinaryFileTypeId = uploadedBinaryFile.BinaryFileTypeId;
                    entity.FileSize = uploadedBinaryFile.FileSize;
                    var memoryStream = new MemoryStream();

                    // If this is a label file then we need to cleanup some settings that most templates will use by default
                    if ( IsLabelFile( bag ) )
                    {
                        // ^JUS will save changes to EEPROM, doing this for each label is not needed, slows printing dramatically, and shortens the printer's memory life.
                        string label = uploadedBinaryFile.ContentsToString().Replace( "^JUS", string.Empty );

                        // Use UTF-8 instead of ASCII
                        label = label.Replace( "^CI0", "^CI28" );

                        var writer = new StreamWriter( memoryStream );
                        writer.Write( label );
                        writer.Flush();
                    }
                    else
                    {
                        uploadedBinaryFile.ContentStream.CopyTo( memoryStream );
                    }

                    entity.ContentStream = memoryStream;
                }
            }
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            using ( var rockContext = new RockContext() )
            {
                var entityService = new BinaryFileService( rockContext );

                if ( !TryGetEntityForEditAction( key, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                if ( !entityService.CanDelete( entity, out var errorMessage ) )
                {
                    return ActionBadRequest( errorMessage );
                }

                entityService.Delete( entity );
                rockContext.SaveChanges();

                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Called when a new file is uploaded to trigger the workflow.
        /// </summary>
        /// <param name="bag">The binary file.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult FileUploaded( BinaryFileBag bag )
        {
            using ( var rockContext = new RockContext() )
            {
                var binaryFileService = new BinaryFileService( rockContext );
                BinaryFile binaryFile = null;
                var fileId = bag.File.GetEntityId<BinaryFile>( rockContext );
                if ( fileId.HasValue )
                {
                    binaryFile = binaryFileService.Get( fileId.Value );
                }

                if ( binaryFile != null )
                {
                    if ( !string.IsNullOrWhiteSpace( bag.FileName ) )
                    {
                        binaryFile.FileName = bag.FileName;
                    }

                    // set binaryFile.Id to original id since the UploadedFile is a temporary binaryFile with a different id
                    binaryFile.Id = GetId( bag.IdKey ) ?? binaryFile.Id;
                    binaryFile.Description = bag.Description;
                    binaryFile.BinaryFileTypeId = bag.BinaryFileType.GetEntityId<BinaryFileType>( rockContext );
                    if ( binaryFile.BinaryFileTypeId.HasValue )
                    {
                        binaryFile.BinaryFileType = new BinaryFileTypeService( rockContext ).Get( binaryFile.BinaryFileTypeId.Value );
                    }

                    // load attributes, then get the attribute values from the UI
                    binaryFile.LoadAttributes();

                    bag = LaunchFileUploadWorkflow( binaryFile, binaryFileService );
                }

                return ActionOk( bag );
            }
        }

        /// <summary>
        /// Reruns the workflow.
        /// </summary>
        /// <param name="bag">The binary file bag.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult RerunWorkflow( BinaryFileBag bag )
        {
            if ( bag != null )
            {
                var binaryFileId = GetId( bag.IdKey );

                if ( binaryFileId.HasValue )
                {
                    var orphanedBinaryFileIdList = bag.OrphanedBinaryFileIdList;
                    using ( var rockContext = new RockContext() )
                    {
                        var binaryFileService = new BinaryFileService( rockContext );
                        var binaryFile = binaryFileService.Get( binaryFileId.Value );
                        if ( binaryFile != null )
                        {
                            bag = LaunchFileUploadWorkflow( binaryFile, binaryFileService );
                            bag.OrphanedBinaryFileIdList = orphanedBinaryFileIdList;
                            return ActionOk( bag );
                        }
                    }
                }
            }

            return ActionOk( bag );
        }

        /// <summary>
        /// Removes the orphaned files.
        /// </summary>
        /// <param name="orphanedBinaryFileIdList">The orphaned binary file identifier list.</param>
        /// <returns></returns>
        [BlockAction]
        public BlockActionResult RemoveOrphanedFiles( List<Guid> orphanedBinaryFileIdList )
        {
            using ( var rockContext = new RockContext() )
            {
                DeleteOrphanedFiles( orphanedBinaryFileIdList, rockContext );
                rockContext.SaveChanges();
                return ActionOk( this.GetParentPageUrl() );
            }
        }

        /// <summary>
        /// Refreshes the list of attributes that can be displayed for editing
        /// purposes based on any modified values on the entity.
        /// </summary>
        /// <param name="box">The box that contains all the information about the entity being edited.</param>
        /// <returns>A box that contains the entity and attribute information.</returns>
        [BlockAction]
        public BlockActionResult RefreshAttributes( DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag> box )
        {
            using ( var rockContext = new RockContext() )
            {
                if ( !TryGetEntityForEditAction( box.Entity.IdKey, rockContext, out var entity, out var actionError ) )
                {
                    return actionError;
                }

                // Update the entity instance from the information in the bag.
                if ( !UpdateEntityFromBox( entity, box, rockContext ) )
                {
                    return ActionBadRequest( "Invalid data." );
                }

                // Reload attributes based on the new property values.
                entity.LoadAttributes( rockContext );

                var refreshedBox = new DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag>
                {
                    Entity = GetEntityBagForEdit( entity, rockContext )
                };

                var oldAttributeGuids = box.Entity.Attributes.Values.Select( a => a.AttributeGuid ).ToList();
                var newAttributeGuids = refreshedBox.Entity.Attributes.Values.Select( a => a.AttributeGuid );

                // If the attributes haven't changed then return a 204 status code.
                if ( oldAttributeGuids.SequenceEqual( newAttributeGuids ) )
                {
                    return ActionStatusCode( System.Net.HttpStatusCode.NoContent );
                }

                // Replace any values for attributes that haven't changed with
                // the value sent by the client. This ensures any unsaved attribute
                // value changes are not lost.
                foreach ( var kvp in refreshedBox.Entity.Attributes )
                {
                    if ( oldAttributeGuids.Contains( kvp.Value.AttributeGuid ) )
                    {
                        refreshedBox.Entity.AttributeValues[kvp.Key] = box.Entity.AttributeValues[kvp.Key];
                    }
                }

                return ActionOk( refreshedBox );
            }
        }

        #endregion
    }
}
