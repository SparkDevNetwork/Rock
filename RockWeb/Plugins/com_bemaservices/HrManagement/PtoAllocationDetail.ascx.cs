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
using CSScriptLibrary;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;

namespace RockWeb.Plugins.com_bemaservices.HrManagement
{
    [DisplayName( "PTO Allocation Detail" )]
    [Category( "BEMA Services > HR Management" )]
    [Description( "Displays the details of the given PTO Allocation for editing." )]
    public partial class PtoAllocationDetail : RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upPtoAllocation );
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
                ShowDetail( PageParameter( "PtoAllocationId" ).AsInteger() );
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

            int? ptoAllocationId = PageParameter( pageReference, "PtoAllocationId" ).AsIntegerOrNull();
            if ( ptoAllocationId != null )
            {
                PtoAllocation ptoAllocation = new PtoAllocationService( new RockContext() ).Get( ptoAllocationId.Value );
                if ( ptoAllocation != null )
                {
                    breadCrumbs.Add( new BreadCrumb( ptoAllocation.ToString(), pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "New PTO Allocation", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
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
            var ptoAllocation = new PtoAllocationService( rockContext ).Get( hfPtoAllocationId.Value.AsInteger() );

            ShowEditDetails( ptoAllocation );

        }

        /// <summary>
        /// Handles the Click event of the btnDelete control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnDeleteConfirm_Click( object sender, EventArgs e )
        {
            Dictionary<string, string> qryParams = new Dictionary<string, string>();

            using ( var rockContext = new RockContext() )
            {
                PtoAllocationService ptoAllocationService = new PtoAllocationService( rockContext );
                var ptoRequestService = new PtoRequestService( rockContext );
                PtoAllocation ptoAllocation = ptoAllocationService.Get( int.Parse( hfPtoAllocationId.Value ) );

                var personId = ptoAllocation.PersonAlias.PersonId.ToString();
                qryParams.Add( "PersonId", personId );

                if ( ptoAllocation != null )
                {
                    rockContext.WrapTransaction( () =>
                    {

                        var changes = new History.HistoryChangeList();

                        var ptoRequests = ptoAllocation.PtoRequests.ToList();
                        foreach ( var ptoRequest in ptoRequests )
                        {
                            var requestChanges = new History.HistoryChangeList();

                            requestChanges.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "PtoRequest" );
                            HistoryService.SaveChanges(
                                rockContext,
                                typeof( PtoRequest ),
                                Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                                ptoRequest.Id,
                                requestChanges );

                            ptoRequestService.Delete( ptoRequest );
                        }

                        changes.AddChange( History.HistoryVerb.Delete, History.HistoryChangeType.Record, "PtoAllocation" );
                        HistoryService.SaveChanges(
                            rockContext,
                            typeof( PtoAllocation ),
                            Rock.SystemGuid.Category.HISTORY_PERSON.AsGuid(),
                            ptoAllocation.Id,
                            changes );

                        ptoAllocationService.Delete( ptoAllocation );

                        rockContext.SaveChanges();
                    } );
                }
            }

            NavigateToParentPage( qryParams );
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
            PtoAllocation ptoAllocation;
            using ( var rockContext = new RockContext() )
            {

                PtoAllocationService ptoAllocationService = new PtoAllocationService( rockContext );

                int ptoAllocationId = int.Parse( hfPtoAllocationId.Value );

                if ( ptoAllocationId == 0 )
                {
                    ptoAllocation = new PtoAllocation();
                    ptoAllocationService.Add( ptoAllocation );
                }
                else
                {
                    ptoAllocation = ptoAllocationService.Get( ptoAllocationId );
                }

                ptoAllocation.PtoTypeId = ddlPtoType.SelectedItem.Value.AsInteger();
                ptoAllocation.PtoAllocationStatus = ddlStatus.SelectedValue.ConvertToEnum<PtoAllocationStatus>();
                ptoAllocation.StartDate = dtpStartDate.SelectedDate.Value;

                if ( dtpEndDate.SelectedDate.HasValue )
                {
                    ptoAllocation.EndDate = dtpEndDate.SelectedDate.Value;
                }
                else
                {
                    ptoAllocation.EndDate = null;
                }

                ptoAllocation.Hours = tbHours.Text.AsDecimal();
                ptoAllocation.PtoAccrualSchedule = ddlPtoAccrualSchedule.SelectedItem.Value.ConvertToEnum<PtoAccrualSchedule>();
                ptoAllocation.PtoAllocationSourceType = PtoAllocationSourceType.Manual;
                ptoAllocation.PersonAliasId = ppPerson.PersonAliasId.Value;
                ptoAllocation.Note = tbNote.Text;

                if ( ptoAllocation.PtoAllocationStatus == PtoAllocationStatus.Denied )
                {
                    foreach ( var ptoRequest in ptoAllocation.PtoRequests )
                    {
                        ptoRequest.PtoRequestApprovalState = PtoRequestApprovalState.Denied;
                    }
                }

                if ( !ptoAllocation.IsValid )
                {
                    // Controls will render the error messages
                    return;
                }

                rockContext.SaveChanges();

                var qryParams = new Dictionary<string, string>();
                qryParams["PtoAllocationId"] = ptoAllocation.Id.ToString();

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
            if ( hfPtoAllocationId.Value.Equals( "0" ) )
            {
                var personId = PageParameter( "PersonId" ).ToString();
                Dictionary<string, string> qryParams = new Dictionary<string, string>();
                qryParams.Add( "PersonId", personId );
                NavigateToParentPage( qryParams );
            }
            else
            {
                ShowReadonlyDetails( GetPtoAllocation( hfPtoAllocationId.ValueAsInt(), new RockContext() ) );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            var currentPtoAllocation = GetPtoAllocation( hfPtoAllocationId.Value.AsInteger() );
            if ( currentPtoAllocation != null )
            {
                ShowReadonlyDetails( currentPtoAllocation );
            }
            else
            {
                string ptoAllocationId = PageParameter( "PtoAllocationId" );
                if ( !string.IsNullOrWhiteSpace( ptoAllocationId ) )
                {
                    ShowDetail( ptoAllocationId.AsInteger() );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="connectionTypeId">The Connection Type Type identifier.</param>
        public void ShowDetail( int ptoAllocationId )
        {
            pnlDetails.Visible = false;

            PtoAllocation ptoAllocation = null;
            using ( var rockContext = new RockContext() )
            {
                if ( !ptoAllocationId.Equals( 0 ) )
                {
                    ptoAllocation = GetPtoAllocation( ptoAllocationId, rockContext );
                    pdAuditDetails.SetEntity( ptoAllocation, ResolveRockUrl( "~" ) );
                }

                if ( ptoAllocation == null )
                {
                    ptoAllocation = new PtoAllocation { Id = 0 };
                    ptoAllocation.PtoAllocationStatus = PtoAllocationStatus.Pending;
                    // hide the panel drawer that show created and last modified dates
                    pdAuditDetails.Visible = false;

                    var personId = PageParameter( "PersonId" ).AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var personService = new PersonService( rockContext );
                        var person = personService.Get( personId.Value );
                        if ( person != null )
                        {
                            ptoAllocation.PersonAlias = person.PrimaryAlias;
                        }
                    }
                }

                bool adminAllowed = UserCanEdit;
                pnlDetails.Visible = true;
                hfPtoAllocationId.Value = ptoAllocation.Id.ToString();
                bool readOnly = false;

                nbEditModeMessage.Text = string.Empty;
                if ( !adminAllowed )
                {
                    readOnly = true;
                    nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( PtoAllocation.FriendlyTypeName );
                }

                if ( readOnly )
                {
                    btnEdit.Visible = false;
                    btnDelete.Visible = false;
                    //btnSecurity.Visible = false;
                    ShowReadonlyDetails( ptoAllocation );
                }
                else
                {
                    btnEdit.Visible = true;
                    btnDelete.Visible = true;
                    //btnSecurity.Visible = true;

                    //btnSecurity.Title = "Secure " + ptoAllocation.Name;
                    //btnSecurity.EntityId = ptoAllocation.Id;

                    if ( !ptoAllocationId.Equals( 0 ) )
                    {
                        ShowReadonlyDetails( ptoAllocation );
                    }
                    else
                    {
                        ShowEditDetails( ptoAllocation );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="connectionType">Type of the connection.</param>
        private void ShowEditDetails( PtoAllocation ptoAllocation )
        {
            if ( ptoAllocation == null )
            {
                ptoAllocation = new PtoAllocation();
                ptoAllocation.PtoAllocationStatus = PtoAllocationStatus.Active;
            }
            if ( ptoAllocation.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( PtoAllocation.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = ptoAllocation.ToString().FormatAsHtmlTitle();
            }

            SetEditMode( true );

            // General

            ddlStatus.BindToEnum<PtoAllocationStatus>();
            ddlStatus.SetValue( ptoAllocation.PtoAllocationStatus.ConvertToInt() );

            if ( ptoAllocation.PersonAlias != null )
            {
                ppPerson.SetValue( ptoAllocation.PersonAlias.Person );
            }

            ddlPtoType.DataSource = new PtoTypeService( new RockContext() )
                .Queryable()
                .AsNoTracking()
                .Where( p => p.IsActive == true )
                .ToList();

            ddlPtoType.DataValueField = "Id";
            ddlPtoType.DataTextField = "Name";

            ddlPtoType.DataBind();

            if ( ptoAllocation.PtoType != null )
            {
                ddlPtoType.SetValue( ptoAllocation.PtoType );
            }

            dtpStartDate.SelectedDate = ptoAllocation.StartDate;
            dtpEndDate.SelectedDate = ptoAllocation.EndDate;
            tbHours.Text = ptoAllocation.Hours.ToString();
            ddlPtoAccrualSchedule.BindToEnum<PtoAccrualSchedule>();
            ddlPtoAccrualSchedule.SetValue( ptoAllocation.PtoAccrualSchedule.ConvertToInt() );

            tbNote.Text = ptoAllocation.Note;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="ptoAllocation">Type of the connection.</param>
        private void ShowReadonlyDetails( PtoAllocation ptoAllocation )
        {
            SetEditMode( false );

            hfPtoAllocationId.SetValue( ptoAllocation.Id );

            lReadOnlyTitle.Text = ptoAllocation.ToString().FormatAsHtmlTitle();
            lPtoAllocationDescription.Text = ptoAllocation.Note.ScrubHtmlAndConvertCrLfToBr();

            string statusLabelClass = "";
            var statusText = ptoAllocation.PtoAllocationStatus.ConvertToString();

            switch ( ptoAllocation.PtoAllocationStatus )
            {
                case PtoAllocationStatus.Inactive:
                    statusLabelClass = "label label-danger";
                    break;
                case PtoAllocationStatus.Active:
                    statusLabelClass = "label label-info";
                    break;
                case PtoAllocationStatus.Pending:
                    statusLabelClass = "label label-warning";
                    break;
            }

            hlStatus.Text = statusText;
            hlStatus.CssClass = statusLabelClass;
            hlStatus.Visible = true;
            tdPerson.Description = ptoAllocation.PersonAlias.Person.FullName;
            tdPtoType.Description = string.Format( "{0}: {1} hours", ptoAllocation.PtoType.Name, ptoAllocation.Hours );
            tdDateRange.Description = string.Format( "{0}{1}", ptoAllocation.StartDate.ToString( "d" ), ptoAllocation.EndDate.HasValue ? ptoAllocation.EndDate.Value.ToString( " - MM/dd/yyyy" ) : "" );
        }

        /// <summary>
        /// Gets the type of the connection.
        /// </summary>
        /// <param name="ptoAllocationId">The Pto Allocationidentifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private PtoAllocation GetPtoAllocation( int ptoAllocationId, RockContext rockContext = null )
        {
            string key = string.Format( "PtoAllocation:{0}", ptoAllocationId );
            PtoAllocation ptoAllocation = RockPage.GetSharedItem( key ) as PtoAllocation;
            if ( ptoAllocation == null )
            {
                rockContext = rockContext ?? new RockContext();
                ptoAllocation = new PtoAllocationService( rockContext ).Queryable()
                    .Where( c => c.Id == ptoAllocationId )
                    .FirstOrDefault();
                RockPage.SaveSharedItem( key, ptoAllocation );
            }

            return ptoAllocation;
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