// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Web.UI.WebControls;

using Newtonsoft.Json;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;
using Attribute = Rock.Model.Attribute;
using com.bemaservices.PastoralCare.Model;

namespace RockWeb.Plugins.com_bemaservices.PastoralCare
{
    [DisplayName( "Care Type Detail" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Displays the details of the given Care Type for editing." )]
    public partial class CareTypeDetail : RockBlock, IDetailBlock
    {
        #region Properties

        private List<Attribute> AttributesState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["AttributesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                AttributesState = new List<Attribute>();
            }
            else
            {
                AttributesState = JsonConvert.DeserializeObject<List<Attribute>>( json );
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            bool editAllowed = IsUserAuthorized( Authorization.ADMINISTRATE );

            gAttributes.DataKeyNames = new string[] { "Guid" };
            gAttributes.Actions.ShowAdd = editAllowed;
            gAttributes.Actions.AddClick += gAttributes_Add;
            gAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            gAttributes.GridRebind += gAttributes_GridRebind;
            gAttributes.GridReorder += gAttributes_GridReorder;

            btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will also delete all the care items! Are you sure you wish to continue with the delete?');", CareType.FriendlyTypeName );
            btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( com.bemaservices.PastoralCare.Model.CareType ) ).Id;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upCareType );
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
                ShowDetail( PageParameter( "CareTypeId" ).AsInteger() );
            }
        }

        /// <summary>
        /// Saves any user control view-state changes that have occurred since the last page postback.
        /// </summary>
        /// <returns>
        /// Returns the user control's current view state. If there is no view state associated with the control, it returns null.
        /// </returns>
        protected override object SaveViewState()
        {
            var jsonSetting = new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new Rock.Utility.IgnoreUrlEncodedKeyContractResolver()
            };

            ViewState["AttributesState"] = JsonConvert.SerializeObject( AttributesState, Formatting.None, jsonSetting );

            return base.SaveViewState();
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

            int? careTypeId = PageParameter( pageReference, "CareTypeId" ).AsIntegerOrNull();
            if ( careTypeId != null )
            {
                CareType careType = new CareTypeService( new RockContext() ).Get( careTypeId.Value );
                if ( careType != null )
                {
                    breadCrumbs.Add( new BreadCrumb( careType.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Care Type", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Makes a duplicate of a Care Type
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {

            int newCareTypeId = 0;

            using ( RockContext rockContext = new RockContext() )
            {
                CareTypeService careTypeService = new CareTypeService( rockContext );

                newCareTypeId = careTypeService.Copy( hfCareTypeId.Value.AsInteger() );

                var newCareType = careTypeService.Get( newCareTypeId );
                if ( newCareType != null )
                {
                    mdCopy.Show( "Care Type copied to '" + newCareType.Name + "'", ModalAlertType.Information );
                }
                else
                {
                    mdCopy.Show( "Care Type failed to copy.", ModalAlertType.Warning );
                }
            }
        }
        #endregion

        #region Events

        #region Control Events

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var careType = new CareTypeService( rockContext ).Get( hfCareTypeId.Value.AsInteger() );

            LoadStateDetails( careType, rockContext );
            ShowEditDetails( careType );

        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDeleteConfirm_Click( object sender, EventArgs e )
        {
            using ( var rockContext = new RockContext() )
            {
                CareTypeService careTypeService = new CareTypeService( rockContext );
                AuthService authService = new AuthService( rockContext );
                CareType careType = careTypeService.Get( int.Parse( hfCareTypeId.Value ) );

                if ( careType != null )
                {
                    if ( !careType.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                    {
                        mdDeleteWarning.Show( "You are not authorized to delete this care type.", ModalAlertType.Information );
                        return;
                    }

                    //var careItems = careType.CareItems.ToList();
                    //CareItemService careItemService = new CareItemService( rockContext );
                    //foreach ( var careItem in careItems )
                    //{
                    //    careItemService.Delete( careItem );
                    //}

                    rockContext.SaveChanges();
                    string errorMessage;
                    if ( !careTypeService.CanDelete( careType, out errorMessage ) )
                    {
                        mdDeleteWarning.Show( errorMessage, ModalAlertType.Information );
                        return;
                    }

                    careTypeService.Delete( careType );
                    rockContext.SaveChanges();
                }
            }

            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnDeleteCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDeleteCancel_Click( object sender, EventArgs e )
        {
            btnDelete.Visible = true;
            btnEdit.Visible = true;
            pnlDeleteConfirm.Visible = false;
        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnDelete_Click( object sender, EventArgs e )
        {
            btnDelete.Visible = false;
            btnEdit.Visible = false;
            pnlDeleteConfirm.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            CareType careType;
            using ( var rockContext = new RockContext() )
            {
                CareTypeService careTypeService = new CareTypeService( rockContext );
                AttributeService attributeService = new AttributeService( rockContext );
                AttributeQualifierService qualifierService = new AttributeQualifierService( rockContext );

                int careTypeId = int.Parse( hfCareTypeId.Value );

                if ( careTypeId == 0 )
                {
                    careType = new CareType();
                    careTypeService.Add( careType );
                }
                else
                {
                    careType = careTypeService.Queryable(  ).Where( c => c.Id == careTypeId ).FirstOrDefault();
                }

                careType.Name = tbName.Text;
                careType.IsActive = cbActive.Checked;
                careType.Description = tbDescription.Text;

                if ( !careType.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // need WrapTransaction due to Attribute saves
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    /* Save Attributes */
                    string qualifierValue = careType.Id.ToString();
                    Helper.SaveAttributeEdits( AttributesState, new CareTypeItem().TypeId, "CareTypeId", qualifierValue, rockContext );

                    careType = careTypeService.Get( careType.Id );
                    if ( careType != null )
                    {
                        if ( !careType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                        {
                            careType.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                        }

                        if ( !careType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                        {
                            careType.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                        }

                        if ( !careType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                        {
                            careType.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                        }
                    }
                } );

                var qryParams = new Dictionary<string, string>();
                qryParams["CareTypeId"] = careType.Id.ToString();

                NavigateToPage( RockPage.Guid, qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            if ( hfCareTypeId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowReadonlyDetails( GetCareType( hfCareTypeId.ValueAsInt(), new RockContext() ) );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentCareType = GetCareType( hfCareTypeId.Value.AsInteger() );
            if ( currentCareType != null )
            {
                ShowReadonlyDetails( currentCareType );
            }
            else
            {
                string careTypeId = PageParameter( "CareTypeId" );
                if ( !string.IsNullOrWhiteSpace( careTypeId ) )
                {
                    ShowDetail( careTypeId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Attributes Grid and Picker

        /// <summary>
        /// Handles the Add event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Add( object sender, EventArgs e )
        {
            gAttributes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAttributes_Edit( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            gAttributes_ShowEdit( attributeGuid );
        }

        /// <summary>
        /// Shows the edit attribute dialog.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        protected void gAttributes_ShowEdit( Guid attributeGuid )
        {
            Attribute attribute;
            if ( attributeGuid.Equals( Guid.Empty ) )
            {
                attribute = new Attribute();
                attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
            }
            else
            {
                attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
            }

            edtAttributes.ActionTitle = ActionTitle.Edit( "attribute for Items of Care type " + tbName.Text );
            var reservedKeyNames = new List<string>();
            AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
            edtAttributes.AllowSearchVisible = true;
            edtAttributes.ReservedKeyNames = reservedKeyNames.ToList();
            edtAttributes.SetAttributeProperties( attribute, typeof( CareType ) );

            ShowDialog( "Attributes" );
        }

        /// <summary>
        /// Handles the GridReorder event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        {
            SortAttributes( AttributesState, e.OldIndex, e.NewIndex );
            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gAttributes_Delete( object sender, RowEventArgs e )
        {
            Guid attributeGuid = ( Guid ) e.RowKeyValue;
            AttributesState.RemoveEntity( attributeGuid );

            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gAttributes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gAttributes_GridRebind( object sender, EventArgs e )
        {
            BindAttributesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgCareTypeAttribute control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgCareTypeAttribute_SaveClick( object sender, EventArgs e )
        {
            Rock.Model.Attribute attribute = new Rock.Model.Attribute();
            edtAttributes.GetAttributeProperties( attribute );

            // Controls will show warnings
            if ( !attribute.IsValid )
            {
                return;
            }

            if ( AttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
            {
                attribute.Order = AttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
                AttributesState.RemoveEntity( attribute.Guid );
            }
            else
            {
                attribute.Order = AttributesState.Any() ? AttributesState.Max( a => a.Order ) + 1 : 0;
            }

            AttributesState.Add( attribute );
            ReOrderAttributes( AttributesState );
            BindAttributesGrid();
            HideDialog();
        }

        /// <summary>
        /// Binds the Care Type attributes grid.
        /// </summary>
        private void BindAttributesGrid()
        {
            gAttributes.DataSource = AttributesState
                         .OrderBy( a => a.Order )
                         .ThenBy( a => a.Name )
                         .Select( a => new
                         {
                             a.Id,
                             a.Guid,
                             a.Name,
                             a.Description,
                             FieldType = FieldTypeCache.GetName( a.FieldTypeId ),
                             a.IsRequired,
                             a.IsGridColumn,
                             a.AllowSearch
                         } )
                         .ToList();
            gAttributes.DataBind();
        }

        /// <summary>
        /// Reorders the attribute list.
        /// </summary>
        /// <param name="itemList">The item list.</param>
        /// <param name="oldIndex">The old index.</param>
        /// <param name="newIndex">The new index.</param>
        private void SortAttributes( List<Attribute> attributeList, int oldIndex, int newIndex )
        {
            var movedItem = attributeList.Where( a => a.Order == oldIndex ).FirstOrDefault();
            if ( movedItem != null )
            {
                if ( newIndex < oldIndex )
                {
                    // Moved up
                    foreach ( var otherItem in attributeList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order + 1;
                    }
                }
                else
                {
                    // Moved Down
                    foreach ( var otherItem in attributeList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
                    {
                        otherItem.Order = otherItem.Order - 1;
                    }
                }

                movedItem.Order = newIndex;
            }
        }

        /// <summary>
        /// Reorders the attributes.
        /// </summary>
        /// <param name="attributeList">The attribute list.</param>
        private void ReOrderAttributes( List<Attribute> attributeList )
        {
            attributeList = attributeList.OrderBy( a => a.Order ).ToList();
            int order = 0;
            attributeList.ForEach( a => a.Order = order++ );
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="careTypeId">The Care Type Type identifier.</param>
        public void ShowDetail( int careTypeId )
        {
            pnlDetails.Visible = false;

            CareType careType = null;
            using ( var rockContext = new RockContext() )
            {
                if ( !careTypeId.Equals( 0 ) )
                {
                    careType = GetCareType( careTypeId, rockContext );
                    pdAuditDetails.SetEntity( careType, ResolveRockUrl( "~" ) );
                }

                if ( careType == null )
                {
                    careType = new CareType { Id = 0 };
                    careType.IsActive = true;
                    // hide the panel drawer that show created and last modified dates
                    pdAuditDetails.Visible = false;
                }

                // Admin rights are needed to edit a care type ( Edit rights only allow adding/removing items )
                bool adminAllowed = UserCanAdministrate || careType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                pnlDetails.Visible = true;
                hfCareTypeId.Value = careType.Id.ToString();
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !adminAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( CareType.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    btnSecurity.Visible = false;
                    ShowReadonlyDetails( careType );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    btnSecurity.Visible = true;

                    btnSecurity.Title = "Secure " + careType.Name;
                    btnSecurity.EntityId = careType.Id;

                    if ( !careTypeId.Equals( 0 ) )
                    {
                        ShowReadonlyDetails( careType );
                    }
                    else
                    {
                        LoadStateDetails( careType, rockContext );
                        ShowEditDetails( careType );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="careType">Type of the care.</param>
        private void ShowEditDetails( CareType careType )
        {
            if ( careType == null )
            {
                careType = new CareType();
            }
            if ( careType.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( CareType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = careType.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            // General
            tbName.Text = careType.Name;
            cbActive.Checked = careType.IsActive;
            tbDescription.Text = careType.Description;

            BindAttributesGrid();
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="careType">Type of the care.</param>
        private void ShowReadonlyDetails( CareType careType )
        {
            SetEditMode( false );

            hfCareTypeId.SetValue( careType.Id );
            AttributesState = null;

            lReadOnlyTitle.Text = careType.Name.FormatAsHtmlTitle();
            lCareTypeDescription.Text = careType.Description.ScrubHtmlAndConvertCrLfToBr();
        }

        /// <summary>
        /// Gets the type of the care.
        /// </summary>
        /// <param name="careTypeId">The care type identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private CareType GetCareType( int careTypeId, RockContext rockContext = null )
        {
            string key = string.Format( "CareType:{0}", careTypeId );
            CareType careType = RockPage.GetSharedItem( key ) as CareType;
            if ( careType == null )
            {
                rockContext = rockContext ?? new RockContext();
                careType = new CareTypeService( rockContext ).Queryable()
                    .Where( c => c.Id == careTypeId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, careType );
            }

            return careType;
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            pnlViewDetails.Visible = !editable;

            this.HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="dialog">The dialog.</param>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( string dialog, bool setValues = false )
        {
            hfActiveDialog.Value = dialog.ToUpper().Trim();
            ShowDialog( setValues );
        }

        /// <summary>
        /// Shows the dialog.
        /// </summary>
        /// <param name="setValues">if set to <c>true</c> [set values].</param>
        private void ShowDialog( bool setValues = false )
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgAttribute.Show();
                    break;
            }
        }

        /// <summary>
        /// Loads the state details.
        /// </summary>
        /// <param name="careType">Type of the care.</param>
        /// <param name="rockContext">The rock context.</param>
        private void LoadStateDetails( CareType careType, RockContext rockContext )
        {
            var attributeService = new AttributeService( rockContext );
            AttributesState = attributeService
                .GetByEntityTypeId( new CareTypeItem().TypeId, true ).AsQueryable()
                .Where( a =>
                    a.EntityTypeQualifierColumn.Equals( "CareTypeId", StringComparison.OrdinalIgnoreCase ) &&
                    a.EntityTypeQualifierValue.Equals( careType.Id.ToString() ) )
                .OrderBy( a => a.Order )
                .ThenBy( a => a.Name )
                .ToList();
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "ATTRIBUTES":
                    dlgAttribute.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}