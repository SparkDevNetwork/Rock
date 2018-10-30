﻿// <copyright>
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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.MergeTemplates;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// 
    /// </summary>
    [DisplayName( "Merge Template Detail" )]
    [Category( "Core" )]
    [Description( "Block for administrating a Merge Template" )]

    [EnumField( "Merge Templates Ownership", "Set this to restrict if the merge template must be a Personal or Global merge template. Note: If the user has EDIT authorization to this block, both Global and Personal templates can be edited regardless of this setting.", typeof( MergeTemplateOwnership ), true, "Global" )]
    public partial class MergeTemplateDetail : RockBlock, IDetailBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            cpCategory.EntityTypeId = EntityTypeCache.Get( Rock.SystemGuid.EntityType.MERGE_TEMPLATE.AsGuid() ).Id;
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.MergeTemplate ) ).Id;
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
                int? mergeTemplateIdParam = PageParameter( "MergeTemplateId" ).AsIntegerOrNull();
                if ( mergeTemplateIdParam.HasValue )
                {
                    ShowDetail( mergeTemplateIdParam.Value, PageParameter( "ParentCategoryId" ).AsIntegerOrNull() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetail( hfMergeTemplateId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var service = new MergeTemplateService( new RockContext() );
            var item = service.Get( hfMergeTemplateId.Value.AsInteger() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            int? categoryId = null;

            var rockContext = new RockContext();
            var service = new MergeTemplateService( rockContext );
            var item = service.Get( hfMergeTemplateId.Value.AsInteger() );

            if ( item != null )
            {
                string errorMessage;
                if ( !service.CanDelete( item, out errorMessage ) )
                {
                    ShowReadonlyDetails( item );
                    mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                }
                else
                {
                    categoryId = item.CategoryId;

                    service.Delete( item );
                    rockContext.SaveChanges();

                    // set IsTemporary to true so that the file will eventually get cleaned up
                    BinaryFileService binaryFileService = new BinaryFileService( rockContext );
                    var binaryFile = binaryFileService.Get( item.TemplateBinaryFileId );
                    if ( binaryFile != null && binaryFile.IsTemporary )
                    {
                        binaryFile.IsTemporary = false;
                    }

                    // reload page, selecting the deleted item's parent
                    var qryParams = new Dictionary<string, string>();
                    if ( categoryId != null )
                    {
                        qryParams["CategoryId"] = categoryId.ToString();
                    }

                    qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

                    NavigateToPage( RockPage.Guid, qryParams );
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            MergeTemplate mergeTemplate;
            var rockContext = new RockContext();
            MergeTemplateService mergeTemplateService = new MergeTemplateService( rockContext );

            int mergeTemplateId = hfMergeTemplateId.Value.AsInteger();
            int? origBinaryFileId = null;

            if ( mergeTemplateId == 0 )
            {
                mergeTemplate = new MergeTemplate();
                mergeTemplateService.Add( mergeTemplate );
            }
            else
            {
                mergeTemplate = mergeTemplateService.Get( mergeTemplateId );
                origBinaryFileId = mergeTemplate.TemplateBinaryFileId;
            }

            mergeTemplate.Name = tbName.Text;
            mergeTemplate.Description = tbDescription.Text;
            mergeTemplate.MergeTemplateTypeEntityTypeId = ddlMergeTemplateType.SelectedValue.AsInteger();
            mergeTemplate.TemplateBinaryFileId = fuTemplateBinaryFile.BinaryFileId ?? 0;
            mergeTemplate.PersonAliasId = ppPerson.PersonAliasId;
            mergeTemplate.CategoryId = cpCategory.SelectedValue.AsInteger();

            int personalMergeTemplateCategoryId = CategoryCache.Get( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() ).Id;
            if ( mergeTemplate.PersonAliasId.HasValue )
            {
                if ( mergeTemplate.CategoryId == 0 )
                {
                    // if the category picker isn't shown and/or the category isn't selected, and it's a personal filter...
                    mergeTemplate.CategoryId = personalMergeTemplateCategoryId;
                }

                // ensure Personal templates are only in the Personal merge template category
                if ( mergeTemplate.CategoryId != personalMergeTemplateCategoryId )
                {
                    // prohibit personal templates from being in something other than the Personal category
                    cpCategory.Visible = true;
                    cpCategory.ShowErrorMessage( "Personal Merge Templates must be in Personal category" );
                }
            }
            else
            {
                if ( mergeTemplate.CategoryId == personalMergeTemplateCategoryId )
                {
                    // prohibit global templates from being in Personal category
                    cpCategory.ShowErrorMessage( "Person is required when using the Personal category" );
                }
            }

            if ( !mergeTemplate.IsValid || !Page.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            BinaryFileService binaryFileService = new BinaryFileService( rockContext );
            if ( origBinaryFileId.HasValue && origBinaryFileId.Value != mergeTemplate.TemplateBinaryFileId )
            {
                // if a new the binaryFile was uploaded, mark the old one as Temporary so that it gets cleaned up
                var oldBinaryFile = binaryFileService.Get( origBinaryFileId.Value );
                if ( oldBinaryFile != null && !oldBinaryFile.IsTemporary )
                {
                    oldBinaryFile.IsTemporary = true;
                }
            }

            // ensure the IsTemporary is set to false on binaryFile associated with this MergeTemplate
            var binaryFile = binaryFileService.Get( mergeTemplate.TemplateBinaryFileId );
            if ( binaryFile != null && binaryFile.IsTemporary )
            {
                binaryFile.IsTemporary = false;
            }

            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );
            qryParams["MergeTemplateId"] = mergeTemplate.Id.ToString();
            NavigateToPage( RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfMergeTemplateId.Value.Equals( "0" ) )
            {
                int? categoryId = PageParameter( "ParentCategoryId" ).AsIntegerOrNull();
                if ( categoryId.HasValue )
                {
                    // Cancelling on Add, and we know the categoryId, so we are probably in treeview mode, so navigate to the current page
                    var qryParams = new Dictionary<string, string>();
                    if ( categoryId != 0 )
                    {
                        qryParams["CategoryId"] = categoryId.ToString();
                    }

                    qryParams["ExpandedIds"] = PageParameter( "ExpandedIds" );

                    NavigateToPage( RockPage.Guid, qryParams );
                }
                else
                {
                    // Cancelling on Add.  Return to Grid
                    NavigateToParentPage();
                }
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                var service = new MergeTemplateService( new RockContext() );
                var item = service.Get( hfMergeTemplateId.Value.AsInteger() );
                ShowReadonlyDetails( item );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mergeTemplateId">The mergeTemplateId value.</param>
        public void ShowDetail( int mergeTemplateId )
        {
            ShowDetail( mergeTemplateId, null );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="mergeTemplateId">The merge template identifier.</param>
        /// <param name="parentCategoryId">The parent category identifier.</param>
        public void ShowDetail( int mergeTemplateId, int? parentCategoryId )
        {
            pnlDetails.Visible = false;

            var rockContext = new RockContext();
            var mergeTemplateService = new MergeTemplateService( rockContext );

            MergeTemplate mergeTemplate = null;

            if ( !mergeTemplateId.Equals( 0 ) )
            {
                mergeTemplate = mergeTemplateService.Get( mergeTemplateId );
                pdAuditDetails.SetEntity( mergeTemplate, ResolveRockUrl( "~" ) );
            }

            var mergeTemplateOwnership = this.GetAttributeValue( "MergeTemplatesOwnership" ).ConvertToEnum<MergeTemplateOwnership>( MergeTemplateOwnership.Global );

            if ( mergeTemplate == null )
            {
                mergeTemplate = new MergeTemplate();
                mergeTemplate.CategoryId = parentCategoryId ?? 0;

                if ( mergeTemplateOwnership == MergeTemplateOwnership.Personal )
                {
                    mergeTemplate.PersonAliasId = this.CurrentPersonAliasId;
                    mergeTemplate.PersonAlias = this.CurrentPersonAlias;

                    // Don't show security on a personal merge template.
                    btnSecurity.Visible = false;
                }

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            pnlDetails.Visible = true;
            hfMergeTemplateId.Value = mergeTemplateId.ToString();

            // render UI based on Authorized
            bool readOnly = false;

            nbEditModeMessage.Text = string.Empty;
            if ( mergeTemplate.PersonAliasId.HasValue && mergeTemplate.PersonAlias.PersonId == this.CurrentPersonId )
            {
                // Allow Person to edit their own Merge Templates
            }
            else if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( MergeTemplate.FriendlyTypeName );
            }

            btnSecurity.Visible = mergeTemplate.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
            btnSecurity.Title = mergeTemplate.Name;
            btnSecurity.EntityId = mergeTemplate.Id;

            if ( readOnly )
            {
                btnEdit.Visible = false;
                btnDelete.Visible = false;
                ShowReadonlyDetails( mergeTemplate );
            }
            else
            {
                btnEdit.Visible = true;
                string errorMessage = string.Empty;
                btnDelete.Visible = mergeTemplateService.CanDelete( mergeTemplate, out errorMessage );
            
                if ( mergeTemplate.Id > 0 )
                {
                    ShowReadonlyDetails( mergeTemplate );
                }
                else
                {
                    ShowEditDetails( mergeTemplate );
                }
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            ddlMergeTemplateType.Items.Clear();
            ddlMergeTemplateType.Items.Add( new ListItem() );

            foreach ( var item in MergeTemplateTypeContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive )
                {
                    var entityType = item.Value.EntityType;
                    ddlMergeTemplateType.Items.Add( new ListItem( entityType.FriendlyName, entityType.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        public void ShowEditDetails( MergeTemplate mergeTemplate )
        {
            if ( mergeTemplate.Id > 0 )
            {
                lActionTitle.Text = ActionTitle.Edit( mergeTemplate.ToString() ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( MergeTemplate.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            var mergeTemplateOwnership = this.GetAttributeValue( "MergeTemplatesOwnership" ).ConvertToEnum<MergeTemplateOwnership>( MergeTemplateOwnership.Global );
            if ( this.IsUserAuthorized( Authorization.EDIT ) )
            {
                // If Authorized to EDIT, owner should be able to be changed, or converted to Global
                ppPerson.Visible = true;
                ppPerson.Required = false;
                cpCategory.Visible = true;
                cpCategory.ExcludedCategoryIds = string.Empty;
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.Global )
            {
                ppPerson.Visible = false;
                ppPerson.Required = false;
                cpCategory.Visible = true;
                cpCategory.ExcludedCategoryIds = CategoryCache.Get( Rock.SystemGuid.Category.PERSONAL_MERGE_TEMPLATE.AsGuid() ).Id.ToString();
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.Personal )
            {
                // if ONLY personal merge templates are permitted, hide the category since it'll always be saved in the personal category
                cpCategory.Visible = false;

                // merge template should be only for the current person, so hide the person picker
                ppPerson.Visible = false;

                // it is required, but not shown..
                ppPerson.Required = false;
            }
            else if ( mergeTemplateOwnership == MergeTemplateOwnership.PersonalAndGlobal )
            {
                ppPerson.Visible = true;
                ppPerson.Required = false;
                cpCategory.Visible = true;
                cpCategory.ExcludedCategoryIds = string.Empty;
            }

            tbName.Text = mergeTemplate.Name;
            tbDescription.Text = mergeTemplate.Description;

            LoadDropDowns();

            ddlMergeTemplateType.SetValue( mergeTemplate.MergeTemplateTypeEntityTypeId );

            fuTemplateBinaryFile.BinaryFileTypeGuid = Rock.SystemGuid.BinaryFiletype.MERGE_TEMPLATE.AsGuid();
            fuTemplateBinaryFile.BinaryFileId = mergeTemplate.TemplateBinaryFileId;

            cpCategory.AllowMultiSelect = false;
            cpCategory.SetValue( mergeTemplate.CategoryId );
            if ( mergeTemplate.PersonAliasId.HasValue )
            {
                ppPerson.SetValue( mergeTemplate.PersonAlias.Person );
            }
            else
            {
                ppPerson.SetValue( null );
            }
        }

        /// <summary>
        /// Handles the FileUploaded event of the fuTemplateBinaryFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void fuTemplateBinaryFile_FileUploaded( object sender, EventArgs e )
        {
            nbFileTypeWarning.Visible = false;
            var mergeTemplateEntityType = EntityTypeCache.Get( ddlMergeTemplateType.SelectedValue.AsInteger() );
            var binaryFile = new BinaryFileService( new RockContext() ).Get( fuTemplateBinaryFile.BinaryFileId ?? 0 );
            if ( binaryFile != null )
            {
                string fileExtension = Path.GetExtension( binaryFile.FileName );
                fileExtension = fileExtension.TrimStart( '.' );
                if ( string.IsNullOrWhiteSpace( fileExtension ) )
                {
                    // nothing more to do
                    return;
                }

                MergeTemplateType mergeTemplateType = null;

                if ( mergeTemplateEntityType != null )
                {
                    mergeTemplateType = MergeTemplateTypeContainer.GetComponent( mergeTemplateEntityType.Name );
                }
                else
                {
                    // if a merge template type isn't selected, automatically pick the first matching one 
                    foreach ( var item in MergeTemplateTypeContainer.Instance.Components.Values )
                    {
                        if ( item.Value.IsActive )
                        {
                            var testMergeTemplateType = item.Value;
                            if ( testMergeTemplateType.SupportedFileExtensions != null && testMergeTemplateType.SupportedFileExtensions.Any() )
                            {
                                if ( testMergeTemplateType.SupportedFileExtensions.Contains( fileExtension ) )
                                {
                                    mergeTemplateType = testMergeTemplateType;
                                    var entityType = EntityTypeCache.Get( mergeTemplateType.EntityType.Id );
                                    if ( entityType != null )
                                    {
                                        ddlMergeTemplateType.SetValue( entityType.Id );
                                    }

                                    break;
                                }
                            }
                        }
                    }
                }

                if ( mergeTemplateType == null )
                {
                    // couldn't automatically pick one, so warn that they need to pick it
                    nbFileTypeWarning.Text = "Warning: Please select a template type.";
                    nbFileTypeWarning.Visible = true;
                    nbFileTypeWarning.Dismissable = true;
                    return;
                }

                if ( mergeTemplateType.SupportedFileExtensions != null && mergeTemplateType.SupportedFileExtensions.Any() )
                {
                    if ( !mergeTemplateType.SupportedFileExtensions.Contains( fileExtension ) )
                    {
                        nbFileTypeWarning.Text = string.Format(
                                "Warning: The selected template type doesn't support '{0}' files. Please use a {1} file for this template type.",
                                fileExtension,
                                mergeTemplateType.SupportedFileExtensions.Select( a => a.Quoted() ).ToList().AsDelimited( ", ", " or " ) );
                        nbFileTypeWarning.Visible = true;
                        nbFileTypeWarning.Dismissable = true;
                        return;
                    }
                }
            }

            if ( binaryFile != null && string.IsNullOrWhiteSpace( tbName.Text ) )
            {
                tbName.Text = Path.GetFileNameWithoutExtension( binaryFile.FileName ).SplitCase().ReplaceWhileExists( "  ", " " );
            }
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="mergeTemplate">The merge template.</param>
        private void ShowReadonlyDetails( MergeTemplate mergeTemplate )
        {
            SetEditMode( false );
            hfMergeTemplateId.SetValue( mergeTemplate.Id );
            lReadOnlyTitle.Text = mergeTemplate.Name.FormatAsHtmlTitle();

            var getFileUrl = string.Format( 
                "{0}GetFile.ashx?guid={1}", 
                System.Web.VirtualPathUtility.ToAbsolute( "~" ), 
                mergeTemplate.TemplateBinaryFile != null ? mergeTemplate.TemplateBinaryFile.Guid : (Guid?)null );

            DescriptionList descriptionListCol1 = new DescriptionList()
                .Add( "Template File", string.Format( "<a href='{0}'>{1}</a>", getFileUrl, mergeTemplate.TemplateBinaryFile.FileName ) )
                .Add( "Description", mergeTemplate.Description ?? string.Empty )
                .Add( "Type", mergeTemplate.MergeTemplateTypeEntityType );

            DescriptionList descriptionListCol2 = new DescriptionList()
                .Add( "Category", mergeTemplate.Category != null ? mergeTemplate.Category.Name : string.Empty )
                .Add( "Person", mergeTemplate.PersonAlias, false );

            lblMainDetailsCol1.Text = descriptionListCol1.Html;
            lblMainDetailsCol2.Text = descriptionListCol2.Html;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;
        }

        #endregion
    }
}