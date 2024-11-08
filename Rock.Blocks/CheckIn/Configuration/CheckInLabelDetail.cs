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
using Rock.CheckIn.v2;
using Rock.CheckIn.v2.Labels;
using Rock.Constants;
using Rock.Data;
using Rock.Enums.CheckIn.Labels;
using Rock.Model;
using Rock.Security;
using Rock.ViewModels.Blocks;
using Rock.ViewModels.Blocks.CheckIn.Configuration.CheckInLabelDetail;
using Rock.ViewModels.CheckIn.Labels;
using Rock.ViewModels.Utility;
using Rock.Web;
using Rock.Web.Cache;

namespace Rock.Blocks.CheckIn.Configuration
{
    /// <summary>
    /// Displays the details of a particular check in label.
    /// </summary>

    [DisplayName( "Check-in Label Detail" )]
    [Category( "Check-in > Configuration" )]
    [Description( "Displays the details of a particular check in label." )]
    [IconCssClass( "fa fa-question" )]
    [SupportedSiteTypes( Model.SiteType.Web )]

    #region Block Attributes

    [LinkedPage( "Designer Page",
        Description = "The page that will show the label designer.",
        Key = AttributeKey.DesignerPage )]

    #endregion

    [Rock.SystemGuid.EntityTypeGuid( "e61908fc-ec33-4b55-b3b9-d83e32a1f064" )]
    [Rock.SystemGuid.BlockTypeGuid( "3299706f-2bb8-49db-831b-86a2b282bb02" )]
    public class CheckInLabelDetail : RockEntityDetailBlockType<CheckInLabel, CheckInLabelBag>, IBreadCrumbBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DesignerPage = "DesignerPage";
        }

        private static class PageParameterKey
        {
            public const string CheckInLabelId = "CheckInLabelId";
        }

        private static class NavigationUrlKey
        {
            public const string ParentPage = "ParentPage";
            public const string DesignerPage = "DesignerPage";
        }

        #endregion Keys

        #region Methods

        /// <inheritdoc/>
        public override object GetObsidianBlockInitialization()
        {
            var box = new DetailBlockBox<CheckInLabelBag, CheckInLabelDetailOptionsBag>();

            SetBoxInitialEntityState( box );

            box.NavigationUrls = GetBoxNavigationUrls();
            box.Options = GetBoxOptions( box.IsEditable );

            return box;
        }

        /// <summary>
        /// Gets the box options required for the component to render the view
        /// or edit the entity.
        /// </summary>
        /// <param name="isEditable"><c>true</c> if the entity is editable; otherwise <c>false</c>.</param>
        /// <returns>The options that provide additional details to the block.</returns>
        private CheckInLabelDetailOptionsBag GetBoxOptions( bool isEditable )
        {
            var options = new CheckInLabelDetailOptionsBag();

            if ( isEditable )
            {
                options.AttendanceLabelFilterSources = FieldSourceHelper.GetAttendanceLabelFilterSources();
                options.CheckoutLabelFilterSources = FieldSourceHelper.GetCheckoutLabelFilterSources();
                options.FamilyLabelFilterSources = FieldSourceHelper.GetFamilyLabelFilterSources();
                options.PersonLabelFilterSources = FieldSourceHelper.GetPersonLabelFilterSources();
                options.PersonLocationLabelFilterSources = FieldSourceHelper.GetPersonLocationLabelFilterSources();
            }

            return options;
        }

        /// <summary>
        /// Validates the CheckInLabel for any final information that might not be
        /// valid after storing all the data from the client.
        /// </summary>
        /// <param name="checkInLabel">The CheckInLabel to be validated.</param>
        /// <param name="errorMessage">On <c>false</c> return, contains the error message.</param>
        /// <returns><c>true</c> if the CheckInLabel is valid, <c>false</c> otherwise.</returns>
        private bool ValidateCheckInLabel( CheckInLabel checkInLabel, out string errorMessage )
        {
            errorMessage = null;

            return true;
        }

        /// <summary>
        /// Sets the initial entity state of the box. Populates the Entity or
        /// ErrorMessage properties depending on the entity and permissions.
        /// </summary>
        /// <param name="box">The box to be populated.</param>
        private void SetBoxInitialEntityState( DetailBlockBox<CheckInLabelBag, CheckInLabelDetailOptionsBag> box )
        {
            var entity = GetInitialEntity();

            if ( entity == null )
            {
                box.ErrorMessage = $"The {CheckInLabel.FriendlyTypeName} was not found.";
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToView( CheckInLabel.FriendlyTypeName );
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
                    box.ErrorMessage = EditModeMessage.NotAuthorizedToEdit( CheckInLabel.FriendlyTypeName );
                }
            }

            PrepareDetailBox( box, entity );
        }

        /// <summary>
        /// Gets the entity bag that is common between both view and edit modes.
        /// </summary>
        /// <param name="entity">The entity to be represented as a bag.</param>
        /// <returns>A <see cref="CheckInLabelBag"/> that represents the entity.</returns>
        private CheckInLabelBag GetCommonEntityBag( CheckInLabel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            return new CheckInLabelBag
            {
                ConditionalPrintCriteria = entity.GetConditionalPrintCriteria(),
                IdKey = entity.IdKey,
                Description = entity.Description,
                IsActive = entity.IsActive,
                IsSystem = entity.IsSystem,
                LabelFormat = entity.LabelFormat,
                LabelSize = entity.GetLabelSizeDescription(),
                LabelType = entity.LabelType,
                LabelContent = entity.LabelFormat == LabelFormat.Zpl
                    ? entity.Content
                    : string.Empty,
                Name = entity.Name,
                PreviewImage = entity.PreviewImage != null
                    ? Convert.ToBase64String( entity.PreviewImage )
                    : string.Empty,
            };
        }

        /// <inheritdoc/>
        protected override CheckInLabelBag GetEntityBagForView( CheckInLabel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicView( entity, RequestContext.CurrentPerson );

            return bag;
        }

        //// <inheritdoc/>
        protected override CheckInLabelBag GetEntityBagForEdit( CheckInLabel entity )
        {
            if ( entity == null )
            {
                return null;
            }

            var bag = GetCommonEntityBag( entity );

            bag.LoadAttributesAndValuesForPublicEdit( entity, RequestContext.CurrentPerson );

            return bag;
        }

        /// <inheritdoc/>
        protected override bool UpdateEntityFromBox( CheckInLabel entity, ValidPropertiesBox<CheckInLabelBag> box )
        {
            if ( box.ValidProperties == null )
            {
                return false;
            }

            box.IfValidProperty( nameof( box.Bag.Description ),
                () => entity.Description = box.Bag.Description );

            box.IfValidProperty( nameof( box.Bag.IsActive ),
                () => entity.IsActive = box.Bag.IsActive );

            if ( entity.Id == 0 )
            {
                // Setting the label type or format is only supported when
                // creating brand new labels.
                box.IfValidProperty( nameof( box.Bag.LabelFormat ),
                    () => entity.LabelFormat = box.Bag.LabelFormat );

                box.IfValidProperty( nameof( box.Bag.LabelType ),
                    () => entity.LabelType = box.Bag.LabelType );

                // If this is a new designed label, then set some defaults.
                if ( entity.LabelFormat == LabelFormat.Designed )
                {
                    var designedBag = new DesignedLabelBag
                    {
                        Width = 4,
                        Height = 2,
                        Fields = new List<LabelFieldBag>()
                    };

                    entity.Content = designedBag.ToJson();
                }
            }

            box.IfValidProperty( nameof( box.Bag.Name ),
                () => entity.Name = box.Bag.Name );

            box.IfValidProperty( nameof( box.Bag.ConditionalPrintCriteria ),
                () => entity.SetConditionalPrintCriteria( box.Bag.ConditionalPrintCriteria ) );

            if ( entity.LabelFormat == LabelFormat.Zpl )
            {
                box.IfValidProperty( nameof( box.Bag.LabelContent ),
                    () => entity.Content = box.Bag.LabelContent );
            }

            box.IfValidProperty( nameof( box.Bag.AttributeValues ),
                () =>
                {
                    entity.LoadAttributes( RockContext );

                    entity.SetPublicAttributeValues( box.Bag.AttributeValues, RequestContext.CurrentPerson );
                } );

            return true;
        }

        /// <inheritdoc/>
        protected override CheckInLabel GetInitialEntity()
        {
            return GetInitialEntity<CheckInLabel, CheckInLabelService>( RockContext, PageParameterKey.CheckInLabelId );
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
                [NavigationUrlKey.DesignerPage] = this.GetLinkedPageUrl( AttributeKey.DesignerPage, "CheckInLabelId", "((Key))" )
            };
        }

        /// <inheritdoc/>
        protected override bool TryGetEntityForEditAction( string idKey, out CheckInLabel entity, out BlockActionResult error )
        {
            var entityService = new CheckInLabelService( RockContext );
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
                entity = new CheckInLabel();
                entityService.Add( entity );
            }

            if ( entity == null )
            {
                error = ActionBadRequest( $"{CheckInLabel.FriendlyTypeName} not found." );
                return false;
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                error = ActionBadRequest( $"Not authorized to edit ${CheckInLabel.FriendlyTypeName}." );
                return false;
            }

            return true;
        }

        #endregion

        #region IBreadCrumbBlock

        /// <inheritdoc/>
        public BreadCrumbResult GetBreadCrumbs( PageReference pageReference )
        {
            using ( var rockContext = new RockContext() )
            {
                var key = pageReference.GetPageParameter( PageParameterKey.CheckInLabelId );
                var pageParameters = new Dictionary<string, string>();

                var name = new CheckInLabelService( rockContext )
                    .GetSelect( key, l => l.Name );

                if ( name != null )
                {
                    pageParameters.Add( PageParameterKey.CheckInLabelId, key );
                }

                var breadCrumbPageRef = new PageReference( pageReference.PageId, 0, pageParameters );
                var breadCrumb = new BreadCrumbLink( name ?? "New Label", breadCrumbPageRef );

                return new BreadCrumbResult
                {
                    BreadCrumbs = new List<IBreadCrumb> { breadCrumb }
                };
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

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<CheckInLabelBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Saves the entity contained in the box.
        /// </summary>
        /// <param name="box">The box that contains all the information required to save.</param>
        /// <returns>A new entity bag to be used when returning to view mode, or the URL to redirect to after creating a new entity.</returns>
        [BlockAction]
        public BlockActionResult Save( ValidPropertiesBox<CheckInLabelBag> box )
        {
            var entityService = new CheckInLabelService( RockContext );

            if ( !TryGetEntityForEditAction( box.Bag.IdKey, out var entity, out var actionError ) )
            {
                return actionError;
            }

            // Update the entity instance from the information in the bag.
            if ( !UpdateEntityFromBox( entity, box ) )
            {
                return ActionBadRequest( "Invalid data." );
            }

            // Ensure everything is valid before saving.
            if ( !ValidateCheckInLabel( entity, out var validationMessage ) )
            {
                return ActionBadRequest( validationMessage );
            }

            var isNew = entity.Id == 0;

            RockContext.WrapTransaction( () =>
            {
                RockContext.SaveChanges();
                entity.SaveAttributeValues( RockContext );
            } );

            if ( isNew )
            {
                return ActionContent( System.Net.HttpStatusCode.Created, this.GetCurrentPageUrl( new Dictionary<string, string>
                {
                    [PageParameterKey.CheckInLabelId] = entity.IdKey
                } ) );
            }

            // Ensure navigation properties will work now.
            entity = entityService.Get( entity.Id );
            entity.LoadAttributes( RockContext );

            var bag = GetEntityBagForEdit( entity );

            return ActionOk( new ValidPropertiesBox<CheckInLabelBag>
            {
                Bag = bag,
                ValidProperties = bag.GetType().GetProperties().Select( p => p.Name ).ToList()
            } );
        }

        /// <summary>
        /// Deletes the specified entity.
        /// </summary>
        /// <param name="key">The identifier of the entity to be deleted.</param>
        /// <returns>A string that contains the URL to be redirected to on success.</returns>
        [BlockAction]
        public BlockActionResult Delete( string key )
        {
            var entityService = new CheckInLabelService( RockContext );

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
        /// Generate the ZPL that can be used as a preview of how this label
        /// will look.
        /// </summary>
        /// <param name="key">The key that identifies the label to preview.</param>
        /// <param name="content">The ZPL content.</param>
        /// <param name="attendanceId">The attendance identifer to use when rendering the preview. If not specified then a generic preview is generated.</param>
        /// <returns>The result of the operation.</returns>
        [BlockAction]
        public BlockActionResult PreviewZpl( string key, string content, string attendanceId )
        {
            var entityService = new CheckInLabelService( RockContext );
            var checkInLabel = entityService.Get( key, !PageCache.Layout.Site.DisablePredictableIds );

            if ( checkInLabel == null )
            {
                return ActionBadRequest( $"{CheckInLabel.FriendlyTypeName} not found." );
            }

            if ( checkInLabel.LabelFormat != LabelFormat.Zpl )
            {
                return ActionBadRequest( "Incorrect label format." );
            }

            if ( !BlockCache.IsAuthorized( Authorization.EDIT, RequestContext.CurrentPerson ) )
            {
                return ActionBadRequest( $"Not authorized to edit ${CheckInLabel.FriendlyTypeName}." );
            }

            if ( attendanceId.IsNullOrWhiteSpace() )
            {
                using ( var memoryStream = new MemoryStream() )
                {
                    var sw = System.Diagnostics.Stopwatch.StartNew();
                    var labelData = LabelDesigner.GetPreviewLabelData( checkInLabel.LabelType, RequestContext.CurrentPerson, RockContext );
                    var mergeFields = new Dictionary<string, object>();

                    foreach ( var prop in labelData.GetType().GetProperties() )
                    {
                        mergeFields.Add( prop.Name, prop.GetValue( labelData ) );
                    }

                    var zpl = content.ResolveMergeFields( mergeFields );
                    sw.Stop();

                    return ActionOk( new
                    {
                        Content = zpl,
                        Duration = sw.ElapsedMilliseconds
                    } );
                }
            }
            else
            {
                checkInLabel.Content = content;

                var attendance = new AttendanceService( RockContext ).Get( attendanceId.AsInteger() );

                if ( attendance == null )
                {
                    return ActionBadRequest( "Attendance record was not found." );
                }

                var attendanceLabel = new LabelAttendanceDetail( attendance, RockContext );
                var director = new CheckInDirector( RockContext );

                var sw = System.Diagnostics.Stopwatch.StartNew();
                var data = director.LabelProvider.RenderLabelUnconditionally( checkInLabel,
                    attendanceLabel,
                    new List<LabelAttendanceDetail> { attendanceLabel },
                    attendance.SearchResultGroup,
                    null );
                sw.Stop();

                var zpl = System.Text.Encoding.UTF8.GetString( data.Data );

                return ActionOk( new
                {
                    Content = zpl,
                    Duration = sw.ElapsedMilliseconds
                } );
            }
        }

        #endregion
    }
}
