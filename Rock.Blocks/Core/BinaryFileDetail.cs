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
    public class BinaryFileDetail : RockEntityDetailBlockType<BinaryFile, BinaryFileBag>
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

        #region Fields

        private BinaryFileType _binaryFileType;

        #endregion

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.Entity );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="bag"><c>true</c> if the block should be visible; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private BinaryFileDetailOptionsBag GetBoxOptions( BinaryFileBag bag )
        {
            var options = new BinaryFileDetailOptionsBag()
            {
                IsBlockVisible = bag.IdKey.IsNotNullOrWhiteSpace() || bag.BinaryFileType != null,
            };
            return options;
        }

        /// <summary>
        /// Validates the BinaryFile for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="binaryFile">The BinaryFile to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the BinaryFile is valid, <c>false</c> otherwise.</returns>
        private bool ValidateBinaryFile( BinaryFile binaryFile, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {BinaryFile.FriendlyTypeName} was not found.";
                return;
            }

            var isViewable = BlockCache.IsAuthorized( Authorization.VIEW, RequestContext.CurrentPerson );
            box.IsEditable = BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson );

            entity.LoadAttributes( RockContext );

            if ( entity.Id != 0 )
            {
                // Existing entity was found, prepare for view mode by default.
                if ( isViewable )
                {
                    box.Entity = GetEntityBagForView( entity );
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
                    box.Entity = GetEntityBagForEdit( entity );
                }
                else
                {
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( BinaryFile.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="BinaryFileBag"/> that represents the entity.</returns>
        private BinaryFileBag GetCommonEntityBag( BinaryFile entity )
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
                FileId = entity.IdKey,
                MimeType = entity.MimeType,
                ShowBinaryFileType = GetAttributeValue( AttributeKey.ShowBinaryFileType ).AsBoolean(),
                WorkflowButtonText = GetAttributeValue( AttributeKey.WorkflowButtonText ),
                ShowWorkflowButton = GetAttributeValue( AttributeKey.Workflow ).AsGuidOrNull().HasValue,
            };

            bag.IsLabelFile = IsLabelFile( bag );
            if ( entity.Id == 0 )
            {
                bag.BinaryFileType = GetBinaryFileType().ToListItemBag();
            }

            return bag;
        }

        /// <summary>
        /// Gets the binary file type id.
        /// </summary>
        /// <returns></returns>
        private int? GetBinaryFileTypeId()
        {
            var binaryFileTypeIdParam = PageParameter( PageParameterKey.BinaryFileTypeId );
            return Rock.Utility.IdHasher.Instance.GetId( binaryFileTypeIdParam ) ?? binaryFileTypeIdParam.AsIntegerOrNull();
        }

        /// <summary>
        /// Gets the binary file type.
        /// </summary>
        /// <returns></returns>
        private BinaryFileType GetBinaryFileType()
        {
            if ( _binaryFileType == null )
            {
                var binaryFileTypeId = GetBinaryFileTypeId();
                if ( binaryFileTypeId.HasValue )
                {
                    _binaryFileType = new BinaryFileTypeService( RockContext ).Get( binaryFileTypeId.Value );
                }
            }

            return _binaryFileType;
        }

        /// <inheritdoc/>
        protected override BinaryFileBag GetEntityBagForView( BinaryFile entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <inheritdoc/>
        protected override BinaryFileBag GetEntityBagForEdit( BinaryFile entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson, enforceSecurity: false );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( BinaryFile entity, ValidPropertiesBox<BinaryFileBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.BinaryFileType ),
                () => entity.BinaryFileTypeId = box.Bag.BinaryFileType.GetEntityId<BinaryFileType>( RockContext ) );

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.FileName ),
                () => entity.FileName = box.Bag.FileName );

            box.IfValidProperty( nameof( box.Bag.MimeType ),
                () => entity.MimeType = box.Bag.MimeType );

            box.IfValidProperty( nameof( box.Bag.File ),
                () => SaveFile( box.Bag, entity ) );

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson, enforceSecurity: false );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override BinaryFile GetInitialEntity()
        {
            var entity = GetInitialEntity<BinaryFile, BinaryFileService>( RockContext, PageParameterKey.BinaryFileId );

            if ( entity?.Id == 0 )
            {
                entity.BinaryFileTypeId = GetBinaryFileTypeId();
            }

            return entity;
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
        protected override bool TryGetEntityForEditAction( string idKey, out BinaryFile entity, out BlockActionResult error )
        {
            var entityService = new BinaryFileService( RockContext );
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
                entity.BinaryFileTypeId = GetBinaryFileTypeId();
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

                    var bag = GetEntityBagForEdit( binaryFile );
                    bag.WorkflowNotificationMessage = $"Successfully processed a <strong>{workflowType.Name}</strong> workflow!";
                    return bag;
                }
            }

            return GetEntityBagForEdit( binaryFile );
        }

        /// <summary>
        /// Gets the identifier as an integer from an idKey.
        /// </summary>
        /// <param name="idKey">The identifier key.</param>
        /// <returns></returns>
        private int? GetId( string idKey )
        {
            var id = !PageCache.Layout.Site.DisablePredictableIds ? idKey.AsIntegerOrNull() : null;

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
        /// <param name="bag">The binary file bag.</param>
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

        /// <summary>
        /// Sets the uploaded file as a stream on the BinaryFile entity.
        /// </summary>
        /// <param name="bag">The bag.</param>
        /// <param name="entity">The entity.</param>
        private void SaveFile( BinaryFileBag bag, BinaryFile entity )
        {
            var entityService = new BinaryFileService( RockContext );
            var binaryFileId = bag.File.GetEntityId<BinaryFile>( RockContext );
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
            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            entity.LoadAttributes( RockContext );

            var box = new DetailBlockBox<BinaryFileBag, BinaryFileDetailOptionsBag>
            {
                Entity = GetEntityBagForEdit( entity )
            };

            return ActionOk( box );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<BinaryFileBag> box )
        {
            var entityService = new BinaryFileService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            var prevBinaryFileTypeId = entity.BinaryFileTypeId;

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateBinaryFile( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            entity.IsTemporary = false;

            RockContext.WrapTransaction( () =>
            {
                DeleteOrphanedFiles( box.Bag.OrphanedBinaryFileIdList, RockContext );

                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            Rock.CheckIn.KioskLabel.Remove( entity.Guid );

            if ( !prevBinaryFileTypeId.Equals( entity.BinaryFileTypeId ) )
            {
                var checkInBinaryFileType = new BinaryFileTypeService( RockContext ).Get( Rock.SystemGuid.BinaryFiletype.CHECKIN_LABEL.AsGuid() );
                if ( checkInBinaryFileType != null && (
                    ( prevBinaryFileTypeId.HasValue && prevBinaryFileTypeId.Value == checkInBinaryFileType.Id ) ||
                    ( entity.BinaryFileTypeId.HasValue && entity.BinaryFileTypeId.Value == checkInBinaryFileType.Id ) ) )
                {
                    Rock.CheckIn.KioskDevice.Clear();
                }
            }

            return ActionOk( this.GetParentPageUrl( new Dictionary<string, string>
            {
                [PageParameterKey.BinaryFileId] = entity.IdKey
            } ) );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new BinaryFileService( RockContext );

            if ( !TryGetEntityForEditAction( key, out var entity, out var actionError ) )
            {
                return actionError;
            }

            if ( !entityService.CanDelete( entity, out var errorMessage ) )
            {
                return ActionBadRequest( errorMessage );
            }

            entityService.Delete( entity );
            RockContext.SaveChanges();

            return ActionOk( this.GetParentPageUrl() );
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
                            return ActionOk( new ValidPropertiesBox<BinaryFileBag>()
                            {
                                Bag = bag,
                                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList(),
                            } );
                        }
                    }
                }
            }

            return ActionOk( new ValidPropertiesBox<BinaryFileBag>()
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList(),
            } );
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

        #endregion
    }
}
