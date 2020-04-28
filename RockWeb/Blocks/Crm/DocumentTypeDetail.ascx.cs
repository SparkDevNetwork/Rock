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

namespace RockWeb.Blocks.Crm
{
    [DisplayName( "Document Type Detail" )]
    [Category( "CRM" )]
    [Description( "Displays the details of the given Document Type for editing." )]

    public partial class DocumentTypeDetail : RockBlock, IDetailBlock
    {
        #region Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            /// <summary>
            /// Key for the document type Id
            /// </summary>
            public const string DocumentTypeId = "DocumentTypeId";
        }

        #endregion Keys

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            InitializeActionButtons();
            InitializeSettingsNotification();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                RenderState();
            }
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
            var documentType = GetDocumentType();
            breadCrumbs.Add( new BreadCrumb( IsAddMode() ? "New Document Type" : documentType.Name, pageReference ) );
            return breadCrumbs;
        }

        /// <summary>
        /// Initialize the action buttons that affect the entire record.
        /// </summary>
        private void InitializeActionButtons()
        {
            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'All document records will also be deleted!');", DocumentType.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( DocumentType ) ).Id;
        }

        /// <summary>
        /// Initialize handlers for block configuration changes.
        /// </summary>
        /// <param name="triggerPanel"></param>
        private void InitializeSettingsNotification()
        {
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upDocumentTypeDetail );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            hfIsEditMode.Value = CanEdit() ? "true" : string.Empty;
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            DeleteRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            SaveRecord();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( IsAddMode() )
            {
                NavigateToParentPage();
            }
            else
            {
                hfIsEditMode.Value = string.Empty;
                RenderState();
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            RenderState();
        }

        /// <summary>
        /// Handles the Click event of the lbToggleAdvancedSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbToggleAdvancedSettings_Click( object sender, EventArgs e )
        {
            ShowAdvancedSettings();
        }

        #endregion Events

        #region Block Notification Messages

        /// <summary>
        /// Show a notification message for the block.
        /// </summary>
        /// <param name="notificationControl"></param>
        /// <param name="message"></param>
        /// <param name="notificationType"></param>
        private void ShowBlockNotification( NotificationBox notificationControl, string message, NotificationBoxType notificationType = NotificationBoxType.Info )
        {
            notificationControl.Text = message;
            notificationControl.NotificationBoxType = notificationType;
        }

        /// <summary>
        /// Shows the block error.
        /// </summary>
        /// <param name="notificationControl">The notification control.</param>
        /// <param name="message">The message.</param>
        private void ShowBlockError( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Danger );
        }

        /// <summary>
        /// Shows the block exception.
        /// </summary>
        /// <param name="notificationControl">The notification control.</param>
        /// <param name="ex">The ex.</param>
        /// <param name="writeToLog">if set to <c>true</c> [write to log].</param>
        private void ShowBlockException( NotificationBox notificationControl, Exception ex, bool writeToLog = true )
        {
            ShowBlockError( notificationControl, ex.Message );

            if ( writeToLog )
            {
                LogException( ex );
            }
        }

        /// <summary>
        /// Shows the block success.
        /// </summary>
        /// <param name="notificationControl">The notification control.</param>
        /// <param name="message">The message.</param>
        private void ShowBlockSuccess( NotificationBox notificationControl, string message )
        {
            ShowBlockNotification( notificationControl, message, NotificationBoxType.Success );
        }

        #endregion

        #region Internal Methods

        private void ShowAdvancedSettings()
        {
            var doShow = hfShowAdvancedSettings.Value.ToLower() != "true";
            hfShowAdvancedSettings.Value = doShow ? "true" : string.Empty;

            InitializeAdvancedSettingsToggle();
        }

        /// <summary>
        /// Delete the current record.
        /// </summary>
        private void DeleteRecord()
        {
            var documentType = GetDocumentType();

            if ( documentType != null )
            {
                if ( !documentType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    mdDeleteWarning.Show( "You are not authorized to delete this item.", ModalAlertType.Information );
                    return;
                }

                var documentTypeService = GetDocumentTypeService();
                var errorMessage = string.Empty;

                if ( !documentTypeService.CanDelete( documentType, out errorMessage ) )
                {
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                documentTypeService.Delete( documentType );
                GetRockContext().SaveChanges();
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Save the current record.
        /// </summary>
        /// <returns></returns>
        private void SaveRecord()
        {
            var documentType = GetDocumentType();
            var documentTypeService = GetDocumentTypeService();
            var isNew = documentType == null;

            if ( isNew )
            {
                documentType = new DocumentType();
                documentTypeService.Add( documentType );
            }

            if ( !etpEntityType.SelectedEntityTypeId.HasValue )
            {
                ShowBlockError( nbEditModeMessage, "The Entity Type is required" );
                return;
            }

            if ( !bftpBinaryFileType.SelectedValueAsInt().HasValue )
            {
                ShowBlockError( nbEditModeMessage, "The File Type is required" );
                return;
            }

            documentType.Name = tbName.Text;
            documentType.EntityTypeId = etpEntityType.SelectedEntityTypeId.Value;
            documentType.IconCssClass = rtbIconCssClass.Text;
            documentType.EntityTypeQualifierColumn = rtbQualifierColumn.Text;
            documentType.EntityTypeQualifierValue = rtbQualifierValue.Text;
            documentType.BinaryFileTypeId = bftpBinaryFileType.SelectedValueAsInt().Value;
            documentType.UserSelectable = rcbManuallySelectable.Checked;
            documentType.DefaultDocumentNameTemplate = ceTemplate.Text;

            if ( !documentType.IsValid )
            {
                // Validation control will render the error messages
                return;
            }

            try
            {
                var rockContext = GetRockContext();
                rockContext.SaveChanges();

                if ( !documentType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    documentType.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                }

                if ( !documentType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                {
                    documentType.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                }

                if ( !documentType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                {
                    documentType.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                }

            }
            catch ( Exception ex )
            {
                ShowBlockException( nbEditModeMessage, ex );
                return;
            }

            // If the save was successful, reload the page using the new record Id.
            NavigateToPage( RockPage.Guid, new Dictionary<string, string> {
                { PageParameterKey.DocumentTypeId, documentType.Id.ToString() }
            } );
        }

        /// <summary>
        /// This method satisfies the IDetailBlock requirement
        /// </summary>
        /// <param name="unused"></param>
        public void ShowDetail( int unused )
        {
            RenderState();
        }

        /// <summary>
        /// Initializes the advanced settings toggle.
        /// </summary>
        private void InitializeAdvancedSettingsToggle()
        {
            var doShow = hfShowAdvancedSettings.Value.ToLower() == "true";
            divAdvanced.Visible = doShow;
            lbToggleAdvancedSettings.Text = doShow ? "Hide Advanced Settings" : "Show Advanced Settings";
        }

        /// <summary>
        /// Provide the options for the entity type picker
        /// </summary>
        private void BindEntityTypes()
        {
            var rockContext = GetRockContext();
            var entityTypes = new EntityTypeService( rockContext )
                .Queryable()
                .AsNoTracking()
                .Where( e => e.IsEntity )
                .OrderBy( t => t.FriendlyName )
                .ToList();

            etpEntityType.EntityTypes = entityTypes;
        }

        /// <summary>
        /// Shows the controls needed
        /// </summary>
        public void RenderState()
        {
            nbEditModeMessage.Text = string.Empty;

            if ( IsAddMode() )
            {
                ShowAddMode();
            }
            else if ( IsEditMode() )
            {
                ShowEditMode();
            }
            else if ( IsViewMode() )
            {
                ShowViewMode();
            }
            else
            {
                nbEditModeMessage.Text = "The document type was not found";
            }
        }

        /// <summary>
        /// Shows the mode where the user can edit an existing document type
        /// </summary>
        private void ShowEditMode()
        {
            if ( !IsEditMode() )
            {
                return;
            }

            BindEntityTypes();
            InitializeAdvancedSettingsToggle();

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            var documentType = GetDocumentType();

            if ( documentType != null )
            {
                pdAuditDetails.SetEntity( documentType, ResolveRockUrl( "~" ) );
            }

            lReadOnlyTitle.Text = documentType.Name.FormatAsHtmlTitle();
            lIcon.Text = string.Format( "<i class='{0}'></i>", documentType.IconCssClass );
            etpEntityType.SelectedEntityTypeId = documentType.EntityTypeId;
            rtbIconCssClass.Text = documentType.IconCssClass;
            rtbQualifierColumn.Text = documentType.EntityTypeQualifierColumn;
            rtbQualifierValue.Text = documentType.EntityTypeQualifierValue;
            ceTemplate.Text = documentType.DefaultDocumentNameTemplate;
            bftpBinaryFileType.SelectedValue = documentType.BinaryFileTypeId.ToString();
            rcbManuallySelectable.Checked = documentType.UserSelectable;

            tbName.Text = documentType.Name;

            // Show the advanced settings if there is something there.
            if ( documentType.EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() ||
                documentType.EntityTypeQualifierValue.IsNotNullOrWhiteSpace() ||
                documentType.DefaultDocumentNameTemplate.IsNotNullOrWhiteSpace() )
            {
                ShowAdvancedSettings();
            }
        }

        /// <summary>
        /// Show the mode where a user can add a new document type
        /// </summary>
        private void ShowAddMode()
        {
            if ( !IsAddMode() )
            {
                return;
            }

            pdAuditDetails.Visible = false;

            BindEntityTypes();
            InitializeAdvancedSettingsToggle();

            pnlEditDetails.Visible = true;
            pnlViewDetails.Visible = false;
            HideSecondaryBlocks( true );

            lReadOnlyTitle.Text = ActionTitle.Add( DocumentType.FriendlyTypeName ).FormatAsHtmlTitle();
            lIcon.Text = string.Format( "<i class='fa fa-file-alt'></i>" );
        }

        /// <summary>
        /// Shows the mode where the user is only viewing an existing document type
        /// </summary>
        private void ShowViewMode()
        {
            if ( !IsViewMode() )
            {
                return;
            }

            var canEdit = CanEdit();

            pnlEditDetails.Visible = false;
            pnlViewDetails.Visible = true;
            HideSecondaryBlocks( false );

            btnEdit.Visible = canEdit;
            btnDelete.Visible = canEdit;
            btnSecurity.Visible = canEdit;

            var documentType = GetDocumentType();

            if ( documentType != null )
            {
                pdAuditDetails.SetEntity( documentType, ResolveRockUrl( "~" ) );
            }

            lReadOnlyTitle.Text = documentType.Name.FormatAsHtmlTitle();
            lIcon.Text = string.Format( "<i class='{0}'></i>", documentType.IconCssClass );

            var descriptionList = new DescriptionList();

            if ( documentType.BinaryFileType != null )
            {
                descriptionList.Add( "File Type", documentType.BinaryFileType.Name );
            }

            if ( documentType.EntityType != null )
            {
                descriptionList.Add( "Entity Type", documentType.EntityType.FriendlyName );
                descriptionList.Add( "Qualifier Column", documentType.EntityTypeQualifierColumn );
                descriptionList.Add( "Qualifier Value", documentType.EntityTypeQualifierValue );
            }

            descriptionList.Add( "Manually Selectable", documentType.UserSelectable ? "Yes" : "No" );

            lDocumentTypeDescription.Text = descriptionList.Html;

            if ( canEdit )
            {
                btnSecurity.Title = "Secure " + documentType.Name;
                btnSecurity.EntityId = documentType.Id;
            }
        }

        #endregion Internal Methods

        #region State Determining Methods

        /// <summary>
        /// Can the user edit the document type
        /// </summary>
        /// <returns></returns>
        private bool CanEdit()
        {
            return UserCanAdministrate && GetDocumentType() != null;
        }

        /// <summary>
        /// Can the user add a new document type
        /// </summary>
        /// <returns></returns>
        private bool CanAdd()
        {
            return UserCanAdministrate && GetDocumentType() == null;
        }

        /// <summary>
        /// Is this block currently adding a new document type
        /// </summary>
        /// <returns></returns>
        private bool IsAddMode()
        {
            return CanAdd();
        }

        /// <summary>
        /// Is this block currently editing an existing document type
        /// </summary>
        /// <returns></returns>
        private bool IsEditMode()
        {
            return CanEdit() && hfIsEditMode.Value.Trim().ToLower() == "true";
        }

        /// <summary>
        /// Is the block currently showing information about a document type
        /// </summary>
        /// <returns></returns>
        private bool IsViewMode()
        {
            return GetDocumentType() != null && hfIsEditMode.Value.Trim().ToLower() != "true";
        }

        #endregion State Determining Methods

        #region Data Interface Methods

        /// <summary>
        /// Get the actual document type model for deleting or editing
        /// </summary>
        /// <returns></returns>
        private DocumentType GetDocumentType()
        {
            if ( _documentType == null )
            {
                var documentTypeId = PageParameter( PageParameterKey.DocumentTypeId ).AsIntegerOrNull();

                if ( documentTypeId.HasValue && documentTypeId.Value > 0 )
                {
                    var documentTypeService = GetDocumentTypeService();
                    _documentType = documentTypeService.Queryable( "EntityType,BinaryFileType" ).FirstOrDefault( dt => dt.Id == documentTypeId.Value );
                }
            }

            return _documentType;
        }
        private DocumentType _documentType = null;

        /// <summary>
        /// Get the rock context
        /// </summary>
        /// <returns></returns>
        private RockContext GetRockContext()
        {
            if ( _rockContext == null )
            {
                _rockContext = new RockContext();
            }

            return _rockContext;
        }
        private RockContext _rockContext = null;

        /// <summary>
        /// Get the document type service
        /// </summary>
        /// <returns></returns>
        private DocumentTypeService GetDocumentTypeService()
        {
            if ( _documentTypeService == null )
            {
                var rockContext = GetRockContext();
                _documentTypeService = new DocumentTypeService( rockContext );
            }

            return _documentTypeService;
        }
        private DocumentTypeService _documentTypeService = null;

        #endregion Data Interface Methods        
    }
}