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
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Event
{
    [DisplayName( "Registration Instance List" )]
    [Category( "Event" )]
    [Description( "Lists all the instances of the given registration template." )]

    [LinkedPage( "Detail Page" )]
    public partial class RegistrationInstanceList : RockBlock, ISecondaryBlock, ICustomGridColumns
    {
        #region Private Variables

        private RegistrationTemplate _template = null;
        private bool _canView = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            int templateId = PageParameter( "RegistrationTemplateId" ).AsInteger();
            if ( templateId != 0 )
            {
                _template = GetRegistrationTemplate( templateId );

                if ( _template != null && _template.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                {
                    _canView = true;

                    rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
                    gInstances.DataKeyNames = new string[] { "Id" };
                    gInstances.RowDataBound += gInstances_RowDataBound;
                    gInstances.Actions.AddClick += gInstances_AddClick;
                    gInstances.GridRebind += gInstances_GridRebind;
                    gInstances.ExportFilename = _template.Name;
                    gInstances.ShowConfirmDeleteDialog = false;

                    // make sure they have Auth to edit the block OR edit to the template
                    bool canEditBlock = UserCanEdit || _template.IsAuthorized( Authorization.EDIT, this.CurrentPerson );
                    gInstances.Actions.ShowAdd = canEditBlock;
                    gInstances.IsDeleteEnabled = canEditBlock;
                }
            }

            string deleteScript = @"
    $('table.js-grid-instances a.grid-delete-button').click(function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to delete this registration instance? All of the registrations and registrants will also be deleted!', function (result) {
            if (result) {
                Rock.dialogs.confirm('Are you really sure? ', function (result) {
                    if (result) {
                        window.location = e.target.href ? e.target.href : e.target.parentElement.href;
                    }
                });
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gInstances, gInstances.GetType(), "deleteInstanceScript", deleteScript, true );

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
                pnlContent.Visible = _canView;
                if ( _canView )
                {
                    SetFilter();
                    BindInstancesGrid();
                }
            }
        }

        #endregion

        #region Instances Grid

        /// <summary>
        /// Handles the RowDataBound event of the gInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        void gInstances_RowDataBound( object sender, System.Web.UI.WebControls.GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                dynamic instance = e.Row.DataItem;
                if ( instance != null && !instance.IsActive )
                {
                    e.Row.AddCssClass( "inactive" );
                }
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Date Range", drpDates.DelimitedValues );
            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                rFilter.SaveUserPreference( "Active Status", string.Empty );
            }
            else
            {
                rFilter.SaveUserPreference( "Active Status", ddlActiveFilter.SelectedValue );
            }

            BindInstancesGrid();
        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Date Range":
                    e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                    break;

                case "Active Status":
                    e.Value = e.Value;
                    break;

                default:
                    e.Value = string.Empty;
                    break;
            }
        }

        /// <summary>
        /// Handles the Click event of the DeleteInstance control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void DeleteInstance_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            RockContext rockContext = new RockContext();
            RegistrationInstanceService instanceService = new RegistrationInstanceService( rockContext );
            RegistrationInstance instance = instanceService.Get( e.RowKeyId );
            if ( instance != null )
            {
                string errorMessage;
                if ( !instanceService.CanDelete( instance, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                rockContext.WrapTransaction( () =>
                {
                    new RegistrationService( rockContext ).DeleteRange( instance.Registrations );
                    instanceService.Delete( instance );
                    rockContext.SaveChanges();
                } );

            }

            BindInstancesGrid();
        }

        /// <summary>
        /// Handles the AddClick event of the gInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gInstances_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "RegistrationInstanceId", 0, "RegistrationTemplateId", _template.Id );
        }

        /// <summary>
        /// Handles the Edit event of the gInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gInstances_Edit( object sender, RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "RegistrationInstanceId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gInstances_GridRebind( object sender, EventArgs e )
        {
            BindInstancesGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            drpDates.DelimitedValues = rFilter.GetUserPreference( "Date Range" );

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( rFilter.GetUserPreference( "Active Status" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }

        }

        /// <summary>
        /// Binds the group members grid.
        /// </summary>
        protected void BindInstancesGrid()
        {
            if ( _template != null )
            {
                pnlInstances.Visible = true;

                lHeading.Text = string.Format( "{0} Instances", _template.Name );

                var rockContext = new RockContext();

                var template = new RegistrationTemplateService( rockContext ).Get( _template.Id );

                var waitListCol = gInstances.ColumnsOfType<RockBoundField>().Where( f => f.DataField == "WaitList" ).First();
                waitListCol.Visible = template != null && template.WaitListEnabled;

                var instanceService = new RegistrationInstanceService( rockContext );
                var qry = instanceService.Queryable().AsNoTracking()
                    .Where( i => i.RegistrationTemplateId == _template.Id );

                // Date Range
                var drp = new DateRangePicker();
                drp.DelimitedValues = rFilter.GetUserPreference( "Date Range" );
                if ( drp.LowerValue.HasValue )
                {
                    qry = qry.Where( i => i.StartDateTime >= drp.LowerValue.Value );
                }

                if ( drp.UpperValue.HasValue )
                {
                    DateTime upperDate = drp.UpperValue.Value.Date.AddDays( 1 );
                    qry = qry.Where( i => i.StartDateTime < upperDate );
                }

                string statusFilter = rFilter.GetUserPreference( "Active Status" );
                if ( !string.IsNullOrWhiteSpace(statusFilter))
                {
                    if ( statusFilter == "inactive")
                    {
                        qry = qry.Where( i => i.IsActive == false );
                    }
                    else
                    {
                        qry = qry.Where( i => i.IsActive == true );
                    }
                }

                SortProperty sortProperty = gInstances.SortProperty;
                if ( sortProperty != null )
                {
                    qry = qry.Sort( sortProperty );
                }
                else
                {
                    qry = qry.OrderByDescending( a => a.StartDateTime );
                }

                var instanceQry = qry.Select( i => new
                {
                    i.Id,
                    i.Guid,
                    i.Name,
                    i.StartDateTime,
                    i.EndDateTime,
                    i.IsActive,
                    Registrants = i.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Where( r => !r.OnWaitList ).Count(),
                    WaitList = i.Registrations.Where( r => !r.IsTemporary ).SelectMany( r => r.Registrants ).Where( r => r.OnWaitList ).Count()
                } );

                gInstances.SetLinqDataSource( instanceQry );
                gInstances.DataBind();
            }
            else
            {
                pnlInstances.Visible = false;
            }
        }

        /// <summary>
        /// Gets the registration template.
        /// </summary>
        /// <param name="registrationTemplateId">The registration template identifier.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        private RegistrationTemplate GetRegistrationTemplate( int registrationTemplateId, RockContext rockContext = null )
        {
            string key = string.Format( "RegistrationTemplate:{0}", registrationTemplateId );
            RegistrationTemplate registrationTemplate = RockPage.GetSharedItem( key ) as RegistrationTemplate;
            if ( registrationTemplate == null )
            {
                rockContext = rockContext ?? new RockContext();
                registrationTemplate = new RegistrationTemplateService( rockContext )
                    .Queryable( "GroupType.Roles" )
                    .AsNoTracking()
                    .FirstOrDefault( i => i.Id == registrationTemplateId );
                RockPage.SaveSharedItem( key, registrationTemplate );
            }

            return registrationTemplate;
        }

        #endregion

        #region ISecondaryBlock

        /// <summary>
        /// Sets the visible.
        /// </summary>
        /// <param name="visible">if set to <c>true</c> [visible].</param>
        public void SetVisible( bool visible )
        {
            pnlContent.Visible = visible;
        }

        #endregion
    }
}