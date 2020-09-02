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

using com.bemaservices.HrManagement.Model;
using OpenXmlPowerTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    [DisplayName( "Pto Bracket Detail" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Displays the details of the given Pto Bracket for editing." )]
    public partial class PtoBracketDetail : RockBlock, IDetailBlock
    {
        #region Fields

        public int _ptoTierId = 0;
        public bool _canEdit = false;

        #endregion

        #region Properties

        public List<PtoBracketType> PtoBracketTypesState { get; set; }

        #endregion

        #region Control Methods

        /// <summary>
        /// Restores the view-state information from a previous user control request that was saved by the <see cref="M:System.Web.UI.UserControl.SaveViewState" /> method.
        /// </summary>
        /// <param name="savedState">An <see cref="T:System.Object" /> that represents the user control state to be restored.</param>
        protected override void LoadViewState( object savedState )
        {
            base.LoadViewState( savedState );

            string json = ViewState["PtoBracketTypesState"] as string;
            if ( string.IsNullOrWhiteSpace( json ) )
            {
                PtoBracketTypesState = new List<PtoBracketType>();
            }
            else
            {
                PtoBracketTypesState = JsonConvert.DeserializeObject<List<PtoBracketType>>( json );
            }

            var ptoTypeService = new PtoTypeService( new RockContext() );
            foreach ( var bracketType in PtoBracketTypesState )
            {
                if ( bracketType.PtoType == null && bracketType.PtoTypeId != null )
                {
                    bracketType.PtoType = ptoTypeService.Get( bracketType.PtoTypeId );
                }
            }

        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>PtoBracket
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            gPtoBracketTypes.DataKeyNames = new string[] { "Guid" };
            gPtoBracketTypes.Actions.ShowAdd = true;
            gPtoBracketTypes.Actions.AddClick += gPtoBracketTypes_Add;
            gPtoBracketTypes.GridRebind += gPtoBracketTypes_GridRebind;

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlPtoBracketDetail );

            _ptoTierId = PageParameter( "PtoTierId" ).AsInteger();
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
                int ptoBracketId = PageParameter( "PtoBracketId" ).AsInteger();
                ShowDetail( ptoBracketId );
            }
            else
            {
                nbInvalidPtoTypes.Visible = false;

                //ShowPtoBracketAttributes();
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

            ViewState["PtoBracketTypesState"] = JsonConvert.SerializeObject( PtoBracketTypesState, Formatting.None, jsonSetting );


            return base.SaveViewState();
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs.
        /// </summary>
        /// <param name="pageReference">The <see cref="Rock.Web.PageReference" />.</param>
        /// <returns>
        /// A <see cref="System.Collections.Generic.List{BreadCrumb}" /> of block related <see cref="Rock.Web.UI.BreadCrumb">BreadCrumbs</see>.
        /// </returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();
            int? ptoBracketId = PageParameter( pageReference, "PtoBracketId" ).AsIntegerOrNull();
            if ( ptoBracketId != null )
            {
                PtoBracket ptoBracket = new PtoBracketService( new RockContext() ).Get( ptoBracketId.Value );
                if ( ptoBracket != null )
                {
                    breadCrumbs.Add( new BreadCrumb( ptoBracket.Name, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New PTO Bracket", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentPtoBracket = GetPtoBracket( hfPtoBracketId.Value.AsInteger() );
            if ( currentPtoBracket != null )
            {
                ShowDetail( currentPtoBracket.Id );
            }
            else
            {
                string ptoBracketId = PageParameter( "PtoBracketId" );
                if ( !string.IsNullOrWhiteSpace( ptoBracketId ) )
                {
                    ShowDetail( ptoBracketId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            PtoBracket ptoBracket = null;

            using ( RockContext rockContext = new RockContext() )
            {
                PtoBracketService ptoBracketService = new PtoBracketService( rockContext );
                PtoBracketTypeService ptoBracketTypeService = new PtoBracketTypeService( rockContext );

                int ptoBracketId = hfPtoBracketId.ValueAsInt();
                if ( ptoBracketId != 0 )
                {
                    ptoBracket = ptoBracketService.Get( ptoBracketId );
                }

                if ( ptoBracket == null )
                {
                    ptoBracket = new PtoBracket();
                    ptoBracket.PtoTierId = _ptoTierId;
                    ptoBracketService.Add( ptoBracket );
                }

                ptoBracket.MinimumYear = tbMinimumYears.Text.AsInteger();
                ptoBracket.MaximumYear = tbMaximumYears.Text.AsIntegerOrNull();
                ptoBracket.IsActive = cbIsActive.Checked;

                // remove any Bracket Types configs that were removed in the UI
                var uiPtoBracketTypes = PtoBracketTypesState.Select( r => r.Guid );
                foreach ( var ptoBracketType in ptoBracket.PtoBracketTypes.Where( r => !uiPtoBracketTypes.Contains( r.Guid ) ).ToList() )
                {
                    ptoBracket.PtoBracketTypes.Remove( ptoBracketType );
                    ptoBracketTypeService.Delete( ptoBracketType );
                }
                //Check for Bracket Types in the Config.  If none are provided, throw an error
                if ( PtoBracketTypesState.Count < 1 )
                {
                    nbInvalidPtoTypes.Visible = true;
                    return;
                }
                else
                {
                    nbInvalidPtoTypes.Visible = false;
                }

                // Add or Update Bracket Type configs from the UI
                foreach ( var ptoBracketTypeState in PtoBracketTypesState )
                {
                    PtoBracketType ptoBracketType = ptoBracket.PtoBracketTypes.Where( a => a.Guid == ptoBracketTypeState.Guid ).FirstOrDefault();
                    if ( ptoBracketType == null )
                    {
                        ptoBracketType = new PtoBracketType();
                        ptoBracket.PtoBracketTypes.Add( ptoBracketType );
                    }

                    ptoBracketType.PtoTypeId = ptoBracketTypeState.PtoTypeId;
                    ptoBracketType.DefaultHours = ptoBracketTypeState.DefaultHours;

                    ptoBracketType.PtoBracketId = ptoBracket.Id;
                }

                //ptoBracket.LoadAttributes();
                //Rock.Attribute.Helper.GetEditValues( phAttributes, ptoBracket );

                if ( !Page.IsValid )
                {
                    return;
                }

                if ( !ptoBracket.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                // use WrapTransaction since SaveAttributeValues does it's own RockContext.SaveChanges()
                rockContext.WrapTransaction( () =>
                {
                    rockContext.SaveChanges();

                    //ptoBracket.SaveAttributeValues( rockContext );

                    rockContext.SaveChanges();

                } );

                var qryParams = new Dictionary<string, string>();
                qryParams["PtoTierId"] = PageParameter( "PtoTierId" );
                NavigateToParentPage( qryParams );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            var qryParams = new Dictionary<string, string>();
            qryParams["PtoTierId"] = PageParameter( "PtoTierId" );
            NavigateToParentPage( qryParams );
        }

        #endregion

        #region Control Events

        #region PtoBracketTypes Grid/Dialog Events

        /// <summary>
        /// Handles the Delete event of the gPtoBracketTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPtoBracketTypes_Delete( object sender, RowEventArgs e )
        {
            Guid rowGuid = ( Guid ) e.RowKeyValue;
            var ptoBracketState = PtoBracketTypesState.Where( g => g.Guid.Equals( rowGuid ) ).FirstOrDefault();
            if ( ptoBracketState != null )
            {
                PtoBracketTypesState.Remove( ptoBracketState );
            }
            BindPtoBracketTypesGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the dlgPtoBracketTypeDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dlgPtoBracketTypeDetails_SaveClick( object sender, EventArgs e )
        {
            Guid guid = hfPtoBracketTypeId.Value.AsGuid();
            var ptoBracketType = PtoBracketTypesState.Where( g => g.Guid.Equals( guid ) ).FirstOrDefault();
            if ( ptoBracketType == null )
            {
                ptoBracketType = new PtoBracketType();
                ptoBracketType.Guid = Guid.NewGuid();
                PtoBracketTypesState.Add( ptoBracketType );
            }
            var ptoType = new PtoTypeService( new RockContext() ).Get( ddlPtoType.SelectedValue.AsInteger() );
            ptoBracketType.PtoTypeId = ptoType.Id;
            ptoBracketType.PtoType = ptoType;
            ptoBracketType.DefaultHours = tbDefaultHours.Text.AsInteger();
            ptoBracketType.IsActive = cbBracketTypeIsActive.Checked;

            BindPtoBracketTypesGrid();

            HideDialog();
        }

        /// <summary>
        /// Handles the GridRebind event of the gPtoBracketTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPtoBracketTypes_GridRebind( object sender, EventArgs e )
        {
            BindPtoBracketTypesGrid();
        }

        /// <summary>
        /// Handles the Add event of the gPtoBracketTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gPtoBracketTypes_Add( object sender, EventArgs e )
        {
            dlgPtoBracketTypeDetails.SaveButtonText = "Add";
            gPtoBracketTypes_ShowEdit( Guid.Empty );
        }

        /// <summary>
        /// Handles the Edit event of the gPtoBracketTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gPtoBracketTypes_Edit( object sender, RowEventArgs e )
        {
            dlgPtoBracketTypeDetails.SaveButtonText = "Save";
            Guid ptoBracketTypesGuid = ( Guid ) e.RowKeyValue;
            gPtoBracketTypes_ShowEdit( ptoBracketTypesGuid );
        }

        protected void gPtoBracketTypes_ShowEdit( Guid ptoBracketTypesGuid )
        {
            ddlPtoType.Items.Clear();
            foreach ( var ptoType in new PtoTypeService( new RockContext() ).Queryable().AsNoTracking() )
            {
                ddlPtoType.Items.Add( new ListItem( ptoType.Name, ptoType.Id.ToString() ) );
            }

            var ptoBracketState = PtoBracketTypesState.FirstOrDefault( l => l.Guid.Equals( ptoBracketTypesGuid ) );
            if ( ptoBracketState != null )
            {
                hfPtoBracketTypeId.Value = ptoBracketTypesGuid.ToString();

                ddlPtoType.SetValue( ptoBracketState.PtoTypeId.ToString() );
                tbDefaultHours.Text = ptoBracketState.DefaultHours.ToString();
                cbBracketTypeIsActive.Checked = ptoBracketState.IsActive;
            }
            else
            {
                hfPtoBracketTypeId.Value = string.Empty;
                cbBracketTypeIsActive.Checked = true;
                ddlPtoType.SetValue( (string)null );
                tbDefaultHours.Text = string.Empty;
            }

            ShowDialog( "PtoBracketTypeDetails", true );
        }

        /// <summary>
        /// Binds the campus grid.
        /// </summary>
        private void BindPtoBracketTypesGrid()
        {
            gPtoBracketTypes.DataSource = PtoBracketTypesState;
            gPtoBracketTypes.DataBind();
        }

        #endregion

        /// <summary>
        /// Handles the Click event of the btnHideDialog control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnHideDialog_Click( object sender, EventArgs e )
        {
            HideDialog();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="ptoBracketId">The ptoBracket identifier.</param>
        public void ShowDetail( int ptoBracketId )
        {
            RockContext rockContext = new RockContext();

            PtoBracket ptoBracket = null;

            if ( !ptoBracketId.Equals( 0 ) )
            {
                ptoBracket = GetPtoBracket( ptoBracketId, rockContext );
                pdAuditDetails.SetEntity( ptoBracket, ResolveRockUrl( "~" ) );
            }

            if ( ptoBracket == null )
            {
                ptoBracket = new PtoBracket { Id = 0, IsActive = true, MinimumYear = 1 };
                ptoBracket.PtoTierId = _ptoTierId;
                ptoBracket.PtoTier = new PtoTierService( rockContext ).Get( _ptoTierId );

                // hide the panel drawer that show created and last modified dates
                pdAuditDetails.Visible = false;
            }

            bool editAllowed = true; //UserCanEdit || ptoBracket.IsAuthorized( Authorization.VIEW, CurrentPerson );
            bool readOnly = true;

            if ( !editAllowed )
            {
                // User is not authorized
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PtoBracket.FriendlyTypeName );
            }
            else
            {
                nbEditModeMessage.Text = string.Empty;

                if ( ptoBracket.Id != 0 && ptoBracket.PtoTierId != _ptoTierId )
                {
                    // Selected Bracket does not belong to the selected Pto Tier
                    nbIncorrectTier.Visible = true;
                }
                else
                {
                    readOnly = false;
                }
            }

            pnlDetails.Visible = !readOnly;
            this.HideSecondaryBlocks( !readOnly );

            if ( !readOnly )
            {
                hfPtoBracketId.Value = ptoBracket.Id.ToString();
                ShowEditDetails( ptoBracket );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="ptoBracket">The ptoBracket.</param>
        private void ShowEditDetails( PtoBracket ptoBracket )
        {
            if ( ptoBracket.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( PtoBracket.FriendlyTypeName ).FormatAsHtmlTitle();
                hlStatus.Visible = false;
            }
            else
            {
                lReadOnlyTitle.Text = ptoBracket.Name.FormatAsHtmlTitle();
                if ( ptoBracket.IsActive )
                {
                    hlStatus.Text = "Active";
                    hlStatus.LabelType = LabelType.Success;
                }
                else
                {
                    hlStatus.Text = "Inactive";
                    hlStatus.LabelType = LabelType.Campus;
                }
            }

            tbMinimumYears.Text = ptoBracket.MinimumYear.ToString();
            tbMaximumYears.Text = ptoBracket.MaximumYear.ToString();
            cbIsActive.Checked = ptoBracket.IsActive;

            PtoBracketTypesState = new List<PtoBracketType>();
            foreach ( var ptoBracketType in ptoBracket.PtoBracketTypes.ToList() )
            {
                PtoBracketTypesState.Add( ptoBracketType );
            }

            BindPtoBracketTypesGrid();

            wpPtoBracketTypes.Expanded = true;
        }

        /// <summary>
        /// Gets the ptoBracket.
        /// </summary>
        /// <param name="ptoBracketId">The ptoBracket identifier.</param>
        /// <returns></returns>
        private PtoBracket GetPtoBracket( int ptoBracketId, RockContext rockContext = null )
        {
            string key = string.Format( "PtoBracket:{0}", ptoBracketId );
            PtoBracket ptoBracket = RockPage.GetSharedItem( key ) as PtoBracket;
            if ( ptoBracket == null )
            {
                rockContext = rockContext ?? new RockContext();
                ptoBracket = new PtoBracketService( rockContext ).Queryable()
                    .Where( e => e.Id == ptoBracketId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, ptoBracket );
            }

            return ptoBracket;
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
                case "PTOBRACKETTYPEDETAILS":
                    dlgPtoBracketTypeDetails.Show();
                    break;
            }
        }

        /// <summary>
        /// Hides the dialog.
        /// </summary>
        private void HideDialog()
        {
            switch ( hfActiveDialog.Value )
            {
                case "PTOBRACKETTYPEDETAILS":
                    dlgPtoBracketTypeDetails.Hide();
                    break;
            }

            hfActiveDialog.Value = string.Empty;
        }

        #endregion
    }
}
