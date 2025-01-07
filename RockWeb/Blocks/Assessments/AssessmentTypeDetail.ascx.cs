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
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Assessments
{
    [DisplayName( "Assessment Type Detail" )]
    [Category( "Assessments" )]
    [Description( "Displays the details of the given Assessment Type for editing." )]

    [Rock.SystemGuid.BlockTypeGuid( "A81AB554-B438-4C7F-9C45-1A9AE2F889C5" )]
    public partial class AssessmentTypeDetail : RockBlock
    {
        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string EntityId = "AssessmentTypeId";
        }

        #endregion

        #region Private Variables

        private int _entityId = 0;
        private EntityTypeCache _entityType = null;
        private RockContext _dataContext = null;
        private bool _blockContextIsValid = false;

        #endregion

        #region Configuration Properties

        public string BlockIconCssClass = "fa fa-directions";
        public string DeleteItemDetailMessage = "This action will also remove all of the associated Assessments.";

        private Type _entitySystemType = typeof( Rock.Model.AssessmentType );

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.InitializeSettingsNotification( upMain );

            _blockContextIsValid = this.InitializeBlockContext();

            _blockContextIsValid = _blockContextIsValid && this.InitializeEntityType();

            if ( !_blockContextIsValid )
            {
                return;
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !_blockContextIsValid )
            {
                base.OnLoad( e );
                return;
            }

            if ( !Page.IsPostBack )
            {
                this.ShowDetail( _entityId );
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? entityId = PageParameter( pageReference, PageParameterKey.EntityId ).AsIntegerOrNull();

            if ( entityId != null )
            {
                var assessmentType = new AssessmentTypeService( this.GetDataContext() ).Get( entityId.Value );

                if ( assessmentType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( assessmentType.Title, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Assessment Type", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        #endregion

        #region Control Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var entity = this.GetTargetEntity();

            ShowEditDetails( entity );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            this.DeleteCurrentEntity();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            var recordId = this.SaveCurrentEntity();

            if ( recordId <= 0 )
            {
                return;
            }

            // Update the query string for this page and reload.
            var qryParams = new Dictionary<string, string>();
            qryParams[PageParameterKey.EntityId] = recordId.ToString();

            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfEntityId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowReadonlyDetails( this.GetTargetEntity() );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentEntity = GetTargetEntity();

            if ( currentEntity != null )
            {
                ShowReadonlyDetails( currentEntity );
            }
            else
            {
                string entityId = PageParameter( PageParameterKey.EntityId );

                if ( !string.IsNullOrWhiteSpace( entityId ) )
                {
                    ShowDetail( entityId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns>The Id of the new record, or -1 if the process could not be completed.</returns>
        private int SaveCurrentEntity()
        {
            AssessmentType assessmentType;

            var rockContext = this.GetDataContext();

            var assessmentTypeService = new AssessmentTypeService( rockContext );

            int assessmentTypeId = int.Parse( hfEntityId.Value );

            if ( assessmentTypeId == 0 )
            {
                assessmentType = new AssessmentType();

                assessmentTypeService.Add( assessmentType );
            }
            else
            {
                assessmentType = assessmentTypeService.Queryable()
                                          .Where( c => c.Id == assessmentTypeId )
                                          .FirstOrDefault();
            }


            // Update Basic properties
            assessmentType.Title = tbTitle.Text;
            assessmentType.IsActive = cbIsActive.Checked;
            assessmentType.Description = tbDescription.Text;

            assessmentType.AssessmentPath = tbAssessmentPath.Text;
            assessmentType.AssessmentResultsPath = tbResultsPath.Text;

            assessmentType.RequiresRequest = cbRequiresRequest.Checked;
            assessmentType.MinimumDaysToRetake = nbDaysBeforeRetake.Text.AsInteger();

            if ( !assessmentType.IsValid )
            {
                // Controls will render the error messages
                return -1;
            }

            rockContext.SaveChanges();

            return assessmentType.Id;
        }

        /// <summary>
        /// Initialize buttons that perform entity-specific actions.
        /// </summary>
        private void InitializeActionButtons( AssessmentType currentEntity )
        {
            // Delete button
            var deleteClickScript = $"javascript: return Rock.dialogs.confirmDelete(event, '\"{currentEntity.Title}\" assessment type', '{this.DeleteItemDetailMessage}');";

            btnDelete.Attributes["onclick"] = deleteClickScript;

            // Security button
            btnSecurity.EntityTypeId = _entityType.Id;
        }

        /// <summary>
        /// Get the Rock Entity Type information for the entity type displayed by this block.
        /// </summary>
        /// <returns></returns>
        private bool InitializeEntityType()
        {
            _entityType = EntityTypeCache.Get( this._entitySystemType );

            return ( _entityType != null );
        }

        /// <summary>
        /// Retrieve a singleton data context for data operations in this block.
        /// </summary>
        /// <returns></returns>
        private RockContext GetDataContext()
        {
            if ( _dataContext == null )
            {
                _dataContext = new RockContext();
            }

            return _dataContext;
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification( UpdatePanel triggerPanel )
        {
            // Set up Block Settings change notification.
            BlockUpdated += Block_BlockUpdated;

            AddConfigurationUpdateTrigger( triggerPanel );
        }

        /// <summary>
        /// Initialize the essential context in which this block is operating.
        /// </summary>
        /// <returns>True, if the block context is valid.</returns>
        private bool InitializeBlockContext()
        {
            _entityId = PageParameter( PageParameterKey.EntityId ).AsInteger();

            return true;
        }


        /// <summary>
        /// Shows the detail panel containing the main content of the block.
        /// </summary>
        /// <param name="entityId">The Connection Type Type identifier.</param>
        public void ShowDetail( int entityId )
        {
            var rockContext = this.GetDataContext();

            var currentEntity = this.GetTargetEntity( entityId );

            // Set properties for postback
            hfEntityId.Value = currentEntity.Id.ToString();

            // Show the audit details for an existing entity.
            if ( currentEntity.Id != 0 )
            {
                pdAuditDetails.SetEntity( currentEntity, ResolveRockUrl( "~" ) );
            }
            else
            {
                pdAuditDetails.Visible = false;
            }

            // Configure security button
            btnSecurity.Title = "Secure " + GetEntityDescription( currentEntity );

            btnSecurity.EntityId = currentEntity.Id;

            // Check permissions for current user.
            bool viewAllowed = currentEntity.IsAuthorized( Authorization.VIEW, CurrentPerson );

            pnlDetails.Visible = viewAllowed;

            bool editAllowed = this.UserCanEdit || currentEntity.IsAuthorized( Authorization.EDIT, CurrentPerson );

            // Set block header
            lIcon.Text = string.Format( "<i class='{0}'></i>", this.BlockIconCssClass );

            // Set block content
            bool readOnly;

            nbEditModeMessage.Text = string.Empty;

            if ( !editAllowed )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( _entityType.FriendlyName );
            }
            else
            {
                readOnly = !entityId.Equals( 0 );
            }

            btnEdit.Visible = editAllowed;
            btnDelete.Visible = editAllowed;
            btnSecurity.Visible = editAllowed;

            if ( readOnly )
            {
                ShowReadonlyDetails( currentEntity );
            }
            else
            {
                ShowEditDetails( currentEntity );
            }

            this.InitializeActionButtons( currentEntity );
        }

        /// <summary>
        /// Get a unique friendly description for the current entity.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private string GetEntityDescription( IEntity entity )
        {
            var targetEntity = entity as AssessmentType;

            if ( targetEntity == null )
            {
                return "(None)";
            }

            return targetEntity.Title;
        }

        /// <summary>
        /// Shows a form that allows the entity properties to be updated.
        /// </summary>
        /// <param name="assessmentType">Type of the connection.</param>
        private void ShowEditDetails( AssessmentType assessmentType )
        {
            if ( assessmentType == null )
            {
                assessmentType = new AssessmentType();
            }
            if ( assessmentType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( _entityType.FriendlyName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = assessmentType.Title.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            hlInactive.Visible = false;

            // General properties
            tbTitle.Text = assessmentType.Title;
            cbIsActive.Checked = assessmentType.IsActive;
            tbDescription.Text = assessmentType.Description;

            tbAssessmentPath.Text = assessmentType.AssessmentPath;
            tbResultsPath.Text = assessmentType.AssessmentResultsPath;

            cbRequiresRequest.Checked = assessmentType.RequiresRequest;

            if ( assessmentType.MinimumDaysToRetake > 0 )
            {
                nbDaysBeforeRetake.Text = assessmentType.MinimumDaysToRetake.ToString();
            }
        }

        /// <summary>
        /// Shows a form that summarises the key properties of the current entity.
        /// </summary>
        /// <param name="assessmentType">Type of the connection.</param>
        private void ShowReadonlyDetails( AssessmentType assessmentType )
        {
            SetEditMode( false );

            hfEntityId.SetValue( assessmentType.Id );

            lReadOnlyTitle.Text = assessmentType.Title.FormatAsHtmlTitle();

            // Create the read-only description text.
            var descriptionListMain = new DescriptionList();

            descriptionListMain.Add( string.Empty, assessmentType.Description );
            descriptionListMain.Add( "Requires Request", assessmentType.RequiresRequest, true );
            descriptionListMain.Add( "Minimum Days To Retake", assessmentType.MinimumDaysToRetake, true );
            descriptionListMain.Add( "Valid Duration", assessmentType.ValidDuration, true );


            lAssessmentTypeDescription.Text = descriptionListMain.Html;

            // Configure Label: Inactive
            hlInactive.Visible = !assessmentType.IsActive;
        }

        /// <summary>
        /// Gets the specified entity data model and sets it as the current model, or retrieves the current model if none is specified.
        /// If the entity cannot be found, a newly-initialized model is created.
        /// </summary>
        /// <param name="entityId">The connection type identifier.</param>
        /// <returns></returns>
        private AssessmentType GetTargetEntity( int? entityId = null )
        {
            if ( entityId == null )
            {
                entityId = hfEntityId.ValueAsInt();
            }

            string key = string.Format( "AssessmentType:{0}", entityId );

            var entity = RockPage.GetSharedItem( key ) as AssessmentType;

            if ( entity == null )
            {
                var rockContext = this.GetDataContext();

                entity = new AssessmentTypeService( rockContext ).Queryable()
                    .Where( c => c.Id == entityId )
                    .FirstOrDefault();

                if ( entity == null )
                {
                    entity = new AssessmentType
                    {
                        Id = 0,
                        IsActive = true
                    };
                }

                RockPage.SaveSharedItem( key, entity );
            }

            return entity;
        }

        /// <summary>
        /// Delete the current entity.
        /// </summary>
        private void DeleteCurrentEntity()
        {
            var targetEntity = this.GetTargetEntity();

            if ( targetEntity == null )
            {
                return;
            }

            if ( !targetEntity.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
            {
                mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                return;
            }

            var rockContext = this.GetDataContext();

            var assessmentTypeService = new AssessmentTypeService( rockContext );

            string errorMessage;

            if ( !assessmentTypeService.CanDelete( targetEntity, out errorMessage ) )
            {
                mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                return;
            }

            assessmentTypeService.Delete( targetEntity );

            rockContext.SaveChanges();

            NavigateToParentPage();
        }

        /// <summary>
        /// Sets the edit mode for the block.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        #endregion
    }
}