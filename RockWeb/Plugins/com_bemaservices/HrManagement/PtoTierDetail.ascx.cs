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

using com.bemaservices.HrManagement.Model;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    [DisplayName( "Pto Teir Detail" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Displays the details of the given Pto Tier for editing." )]
    public partial class PtoTierDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            //bool editAllowed = IsUserAuthorized( Authorization.ADMINISTRATE );

            //gAttributes.DataKeyNames = new string[] { "Guid" };
            //gAttributes.Actions.ShowAdd = editAllowed;
            //gAttributes.Actions.AddClick += gAttributes_Add;
            //gAttributes.EmptyDataText = Server.HtmlEncode( None.Text );
            //gAttributes.GridRebind += gAttributes_GridRebind;
            //gAttributes.GridReorder += gAttributes_GridReorder;


            //btnDelete.Attributes["onclick"] = string.Format( "javascript: return Rock.dialogs.confirmDelete(event, '{0}', 'This will also delete all the connection opportunities! Are you sure you wish to continue with the delete?');", ConnectionType.FriendlyTypeName );
            //btnSecurity.EntityTypeId = EntityTypeCache.Get( typeof( Rock.Model.ConnectionType ) ).Id;

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPtoTeir );
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
                ShowDetail( PageParameter( "PtoTierId" ).AsInteger() );
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

            int? ptoTeirId = PageParameter( pageReference, "PtoTeirId" ).AsIntegerOrNull();
            if ( ptoTeirId != null )
            {
                PtoTier ptoTier = new PtoTierService( new RockContext() ).Get( ptoTeirId.Value );
                if ( ptoTier != null )
                {
                    breadCrumbs.Add( new BreadCrumb( ptoTier.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New Pto Tier", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Makes a duplicate of a Connection Type
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCopy_Click( object sender, EventArgs e )
        {

            int newPtoTeirId = 0;

            using ( RockContext rockContext = new RockContext() )
            {
                PtoTierService ptoTierService = new PtoTierService( rockContext );

                newPtoTeirId = ptoTierService.Copy( hfPtoTeirId.Value.AsInteger() );

                var newPtoTeir = ptoTierService.Get( newPtoTeirId );
                if ( newPtoTeir != null)
                {
                    mdCopy.Show( "Pto Tier copied to '" + newPtoTeir.Name + "'", ModalAlertType.Information );
                }
                else
                {
                    mdCopy.Show( "Pto Tier failed to copy.", ModalAlertType.Warning );
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
            var ptoTier = new PtoTierService( rockContext ).Get( hfPtoTeirId.Value.AsInteger() );

            ShowEditDetails( ptoTier );

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
                PtoTierService ptoTierService = new PtoTierService( rockContext );
                //AuthService authService = new AuthService( rockContext );
                PtoTier ptoTier = ptoTierService.Get( int.Parse( hfPtoTeirId.Value ) );

                if ( ptoTier != null )
                {
                    //if ( !ptoTier.IsAuthorized( Authorization.ADMINISTRATE, this.CurrentPerson ) )
                    //{
                    //    mdDeleteWarning.Show( "You are not authorized to delete this connection type.", ModalAlertType.Information );
                    //    return;
                    //}

                    var ptoBrackets = ptoTier.PtoBrackets.ToList();
                    PtoBracketService ptoBracketService = new PtoBracketService( rockContext );
                    foreach ( var ptoBracket in ptoBrackets )
                    {
                        // We may need to add a function for checking if we can delete the bracket prior to deleting.
                        ptoBracketService.Delete( ptoBracket );
                    }

                    rockContext.SaveChanges();

                    ptoTierService.Delete( ptoTier );
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
            PtoTier ptoTier;
            using ( var rockContext = new RockContext() )
            {
               
                PtoTierService ptoTierService = new PtoTierService( rockContext );
              
                //AttributeService attributeService = new AttributeService( rockContext );
                //AttributeQualifierService qualifierService = new AttributeQualifierService( rockContext );

                int ptoTierId = int.Parse( hfPtoTeirId.Value );

                if ( ptoTierId == 0 )
                {
                    ptoTier = new PtoTier();
                    ptoTierService.Add( ptoTier );
                }
                else
                {
                    ptoTier = ptoTierService.Get( ptoTierId );
                }

                ptoTier.Name = tbName.Text;
                ptoTier.IsActive = cbActive.Checked;
                ptoTier.Description = tbDescription.Text;
                ptoTier.Color = cpColor.Text;
                    
                if ( !ptoTier.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                rockContext.SaveChanges();

                //// need WrapTransaction due to Attribute saves
                //rockContext.WrapTransaction( () =>
                //{
                //    rockContext.SaveChanges();

                //    /* Save Attributes */
                //    string qualifierValue = connectionType.Id.ToString();
                //    Helper.SaveAttributeEdits( AttributesState, new ConnectionOpportunity().TypeId, "ConnectionTypeId", qualifierValue, rockContext );

                //    connectionType = connectionTypeService.Get( connectionType.Id );
                //    if ( connectionType != null )
                //    {
                //        if ( !connectionType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                //        {
                //            connectionType.AllowPerson( Authorization.VIEW, CurrentPerson, rockContext );
                //        }

                //        if ( !connectionType.IsAuthorized( Authorization.EDIT, CurrentPerson ) )
                //        {
                //            connectionType.AllowPerson( Authorization.EDIT, CurrentPerson, rockContext );
                //        }

                //        if ( !connectionType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson ) )
                //        {
                //            connectionType.AllowPerson( Authorization.ADMINISTRATE, CurrentPerson, rockContext );
                //        }
                //    }
                //} );


                var qryParams = new Dictionary<string, string>();
                qryParams["PtoTierId"] = ptoTier.Id.ToString();

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
            if ( hfPtoTeirId.Value.Equals( "0" ) )
            {
                NavigateToParentPage();
            }
            else
            {
                ShowReadonlyDetails( GetPtoTier( hfPtoTeirId.ValueAsInt(), new RockContext() ) );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentPtoTier = GetPtoTier( hfPtoTeirId.Value.AsInteger() );
            if ( currentPtoTier != null )
            {
                ShowReadonlyDetails( currentPtoTier );
            }
            else
            {
                string ptoTierId = PageParameter( "PtoTierId" );
                if ( !string.IsNullOrWhiteSpace( ptoTierId ) )
                {
                    ShowDetail( ptoTierId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Attributes Grid and Picker

        ///// <summary>
        ///// Handles the Add event of the gAttributes control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        //protected void gAttributes_Add( object sender, EventArgs e )
        //{
        //    gAttributes_ShowEdit( Guid.Empty );
        //}

        ///// <summary>
        ///// Handles the Edit event of the gAttributes control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        //protected void gAttributes_Edit( object sender, RowEventArgs e )
        //{
        //    Guid attributeGuid = (Guid)e.RowKeyValue;
        //    gAttributes_ShowEdit( attributeGuid );
        //}

        ///// <summary>
        ///// Shows the edit attribute dialog.
        ///// </summary>
        ///// <param name="attributeGuid">The attribute unique identifier.</param>
        //protected void gAttributes_ShowEdit( Guid attributeGuid )
        //{
        //    Attribute attribute;
        //    if ( attributeGuid.Equals( Guid.Empty ) )
        //    {
        //        attribute = new Attribute();
        //        attribute.FieldTypeId = FieldTypeCache.Get( Rock.SystemGuid.FieldType.TEXT ).Id;
        //    }
        //    else
        //    {
        //        attribute = AttributesState.First( a => a.Guid.Equals( attributeGuid ) );
        //    }

        //    edtAttributes.ActionTitle = ActionTitle.Edit( "attribute for Opportunities of Connection type " + tbName.Text );
        //    var reservedKeyNames = new List<string>();
        //    AttributesState.Where( a => !a.Guid.Equals( attributeGuid ) ).Select( a => a.Key ).ToList().ForEach( a => reservedKeyNames.Add( a ) );
        //    edtAttributes.AllowSearchVisible = true;
        //    edtAttributes.ReservedKeyNames = reservedKeyNames.ToList();
        //    edtAttributes.SetAttributeProperties( attribute, typeof( ConnectionType ) );

        //    ShowDialog( "Attributes" );
        //}

        ///// <summary>
        ///// Handles the GridReorder event of the gAttributes control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        //protected void gAttributes_GridReorder( object sender, GridReorderEventArgs e )
        //{
        //    SortAttributes( AttributesState, e.OldIndex, e.NewIndex );
        //    BindAttributesGrid();
        //}

        ///// <summary>
        ///// Handles the Delete event of the gAttributes control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        ///// <exception cref="System.NotImplementedException"></exception>
        //protected void gAttributes_Delete( object sender, RowEventArgs e )
        //{
        //    Guid attributeGuid = (Guid)e.RowKeyValue;
        //    AttributesState.RemoveEntity( attributeGuid );

        //    BindAttributesGrid();
        //}

        ///// <summary>
        ///// Handles the GridRebind event of the gAttributes control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        //protected void gAttributes_GridRebind( object sender, EventArgs e )
        //{
        //    BindAttributesGrid();
        //}

        ///// <summary>
        ///// Handles the SaveClick event of the dlgConnectionTypeAttribute control.
        ///// </summary>
        ///// <param name="sender">The source of the event.</param>
        ///// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        //protected void dlgConnectionTypeAttribute_SaveClick( object sender, EventArgs e )
        //{
        //    Rock.Model.Attribute attribute = new Rock.Model.Attribute();
        //    edtAttributes.GetAttributeProperties( attribute );

        //    // Controls will show warnings
        //    if ( !attribute.IsValid )
        //    {
        //        return;
        //    }

        //    if ( AttributesState.Any( a => a.Guid.Equals( attribute.Guid ) ) )
        //    {
        //        attribute.Order = AttributesState.Where( a => a.Guid.Equals( attribute.Guid ) ).FirstOrDefault().Order;
        //        AttributesState.RemoveEntity( attribute.Guid );
        //    }
        //    else
        //    {
        //        attribute.Order = AttributesState.Any() ? AttributesState.Max( a => a.Order ) + 1 : 0;
        //    }

        //    AttributesState.Add( attribute );
        //    ReOrderAttributes( AttributesState );
        //    BindAttributesGrid();
        //    HideDialog();
        //}

        ///// <summary>
        ///// Binds the Connection Type attributes grid.
        ///// </summary>
        //private void BindAttributesGrid()
        //{
        //    gAttributes.DataSource = AttributesState
        //                 .OrderBy( a => a.Order )
        //                 .ThenBy( a => a.Name )
        //                 .Select( a => new
        //                 {
        //                     a.Id,
        //                     a.Guid,
        //                     a.Name,
        //                     a.Description,
        //                     FieldType = FieldTypeCache.GetName( a.FieldTypeId ),
        //                     a.IsRequired,
        //                     a.IsGridColumn,
        //                     a.AllowSearch
        //                 } )
        //                 .ToList();
        //    gAttributes.DataBind();
        //}

        ///// <summary>
        ///// Reorders the attribute list.
        ///// </summary>
        ///// <param name="itemList">The item list.</param>
        ///// <param name="oldIndex">The old index.</param>
        ///// <param name="newIndex">The new index.</param>
        //private void SortAttributes( List<Attribute> attributeList, int oldIndex, int newIndex )
        //{
        //    var movedItem = attributeList.Where( a => a.Order == oldIndex ).FirstOrDefault();
        //    if ( movedItem != null )
        //    {
        //        if ( newIndex < oldIndex )
        //        {
        //            // Moved up
        //            foreach ( var otherItem in attributeList.Where( a => a.Order < oldIndex && a.Order >= newIndex ) )
        //            {
        //                otherItem.Order = otherItem.Order + 1;
        //            }
        //        }
        //        else
        //        {
        //            // Moved Down
        //            foreach ( var otherItem in attributeList.Where( a => a.Order > oldIndex && a.Order <= newIndex ) )
        //            {
        //                otherItem.Order = otherItem.Order - 1;
        //            }
        //        }

        //        movedItem.Order = newIndex;
        //    }
        //}

        ///// <summary>
        ///// Reorders the attributes.
        ///// </summary>
        ///// <param name="attributeList">The attribute list.</param>
        //private void ReOrderAttributes( List<Attribute> attributeList )
        //{
        //    attributeList = attributeList.OrderBy( a => a.Order ).ToList();
        //    int order = 0;
        //    attributeList.ForEach( a => a.Order = order++ );
        //}

        #endregion
        
        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="connectionTypeId">The Connection Type Type identifier.</param>
        public void ShowDetail( int ptoTierId )
        {
            pnlDetails.Visible = false;

            PtoTier ptoTier = null;
            using ( var rockContext = new RockContext() )
            {
                if ( !ptoTierId.Equals( 0 ) )
                {
                    ptoTier = GetPtoTier( ptoTierId, rockContext );
                    pdAuditDetails.SetEntity( ptoTier, ResolveRockUrl( "~" ) );
                }

                if ( ptoTier == null )
                {
                    ptoTier = new PtoTier { Id = 0 };
                    // hide the panel drawer that show created and last modified dates
                    pdAuditDetails.Visible = false;
                }

                // Admin rights are needed to edit a connection type ( Edit rights only allow adding/removing items )
                bool adminAllowed = true; //UserCanAdministrate || connectionType.IsAuthorized( Authorization.ADMINISTRATE, CurrentPerson );
                pnlDetails.Visible = true;
                hfPtoTeirId.Value = ptoTier.Id.ToString();
                lIcon.Text = string.Format( "<i class='fa fa-clock'></i>");
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !adminAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( ConnectionType.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    //btnSecurity.Visible = false;
                    ShowReadonlyDetails( ptoTier );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    //btnSecurity.Visible = true;

                    //btnSecurity.Title = "Secure " + ptoTier.Name;
                    //btnSecurity.EntityId = ptoTier.Id;

                    if ( !ptoTierId.Equals( 0 ) )
                    {
                        ShowReadonlyDetails( ptoTier );
                    }
                    else
                    {
                        ShowEditDetails( ptoTier );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void ShowEditDetails( PtoTier ptoTier )
        {
            if ( ptoTier == null )
            {
                ptoTier = new PtoTier();
            }
            if ( ptoTier.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( ConnectionType.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = ptoTier.Name.FormatAsHtmlTitle();
            }

            SetEditMode( true );

            // General
            tbName.Text = ptoTier.Name;
            cbActive.Checked = ptoTier.IsActive;
            tbDescription.Text = ptoTier.Description;
            cpColor.Text = ptoTier.Color;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="ptoTier">Type of the connection.</param>
        private void ShowReadonlyDetails( PtoTier ptoTier )
        {
            SetEditMode( false );

            hfPtoTeirId.SetValue( ptoTier.Id );

            lReadOnlyTitle.Text = ptoTier.Name.FormatAsHtmlTitle();
            lPtoTeirDescription.Text = ptoTier.Description.ScrubHtmlAndConvertCrLfToBr();
        }

        /// <summary>
        /// Gets the type of the connection.
        /// </summary>
        /// <param name="ptoTeirId">The Pto Tieridentifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private PtoTier GetPtoTier( int ptoTeirId, RockContext rockContext = null )
        {
            string key = string.Format( "PtoTier:{0}", ptoTeirId );
            PtoTier ptoTier = RockPage.GetSharedItem( key ) as PtoTier;
            if ( ptoTier == null )
            {
                rockContext = rockContext ?? new RockContext();
                ptoTier = new PtoTierService( rockContext ).Queryable()
                    .Where( c => c.Id == ptoTeirId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, ptoTier );
            }

            return ptoTier;
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

        #endregion
    }
}