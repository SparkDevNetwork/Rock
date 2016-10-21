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
using System.Web.UI.WebControls;
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
using com.centralaz.HumanResources.Model;

namespace RockWeb.Plugins.com_centralaz.HumanResources
{
    /// <summary>
    /// Block for managing categories for an specific entity type.
    /// </summary>
    [DisplayName( "Retirement Fund List" )]
    [Category( "com_centralaz > Human Resources" )]
    [Description( "Block for managing retirement funds for a person" )]
    [DefinedTypeField( "Fund Defined Type", required: true )]
    public partial class RetirementFundList : PersonBlock
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

            gRetirementFunds.DataKeyNames = new string[] { "Id" };
            gRetirementFunds.Actions.ShowAdd = true;

            gRetirementFunds.Actions.AddClick += gRetirementFunds_Add;
            gRetirementFunds.GridReorder += gRetirementFunds_GridReorder;
            gRetirementFunds.GridRebind += gRetirementFunds_GridRebind;

            mdDetails.SaveClick += mdDetails_SaveClick;
            mdDetails.OnCancelScript = string.Format( "$('#{0}').val('');", hfIdValue.ClientID );

            SetDisplay();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }
            else
            {
                if ( !string.IsNullOrWhiteSpace( hfIdValue.Value ) )
                {
                    var rockContext = new RockContext();
                    RetirementFund retirementFund = new RetirementFundService( rockContext ).Get( hfIdValue.Value.AsInteger() );
                    if ( retirementFund == null )
                    {
                        retirementFund = new RetirementFund();
                    }
                    mdDetails.Show();
                }
            }

            base.OnLoad( e );
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
            SetDisplay();
            BindGrid();
        }

        protected void gRetirementFunds_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gRetirementFunds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gRetirementFunds_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new RetirementFundService( rockContext );

            var retirementFund = service.Get( e.RowKeyId );
            if ( retirementFund != null )
            {
                int retirementFundId = retirementFund.Id;
                var changes = new List<string>();

                service.Delete( retirementFund );
                History.EvaluateChange( changes, "Employee Retirement Amount", retirementFund.IsFixedAmount ? retirementFund.EmployeeAmount.FormatAsCurrency() : retirementFund.EmployeeAmount.ToString( "P" ), "" );
                History.EvaluateChange( changes, "Employer Retirement Amount", retirementFund.IsFixedAmount ? retirementFund.EmployerAmount.FormatAsCurrency() : retirementFund.EmployerAmount.ToString( "P" ), "" );
                rockContext.SaveChanges();

                if ( changes.Any() )
                {
                    HistoryService.SaveChanges( rockContext, typeof( Person ), com.centralaz.HumanResources.SystemGuid.Category.HISTORY_HUMAN_RESOURCES.AsGuid(),
                        Person.Id, changes );
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Add event of the gRetirementFunds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gRetirementFunds_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gRetirementFunds control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gRetirementFunds_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gRetirementFunds_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var categories = GetSalaries( rockContext );
            if ( categories != null )
            {
                var changedIds = new RetirementFundService( rockContext ).Reorder( categories.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdDetails_SaveClick( object sender, EventArgs e )
        {
            int retirementFundId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out retirementFundId ) )
            {
                retirementFundId = 0;
            }

            var rockContext = new RockContext();
            var service = new RetirementFundService( rockContext );
            RetirementFund retirementFund = null;
            var changes = new List<string>();

            if ( retirementFundId != 0 )
            {
                retirementFund = service.Get( retirementFundId );
            }

            if ( retirementFund == null )
            {
                retirementFund = new RetirementFund();
                service.Add( retirementFund );
                changes.Add( "Added new Retirement Fund" );
            }

            History.EvaluateChange( changes, "Retirement Fund", retirementFund.PersonAliasId, Person.PrimaryAliasId.Value );
            retirementFund.PersonAliasId = Person.PrimaryAliasId.Value;

            History.EvaluateChange( changes, "Retirement Fund", retirementFund.IsFixedAmount.ToTrueFalse(), cbFixedAmount.Checked.ToTrueFalse() );
            retirementFund.IsFixedAmount = cbFixedAmount.Checked;

            History.EvaluateChange( changes, "Retirement Fund", retirementFund.EmployeeAmount.ToString(), nbEmployeeAmount.Text );
            retirementFund.EmployeeAmount = nbEmployeeAmount.Text.AsDouble();

            History.EvaluateChange( changes, "Retirement Fund", retirementFund.EmployerAmount.ToString(), nbEmployerAmount.Text );
            retirementFund.EmployerAmount = nbEmployerAmount.Text.AsDouble();

            if ( dpActiveDate.SelectedDate.HasValue )
            {
                History.EvaluateChange( changes, "Retirement Fund", retirementFund.ActiveDate, dpActiveDate.SelectedDate.Value );
                retirementFund.ActiveDate = dpActiveDate.SelectedDate.Value;
            }

            if ( dpInactiveDate.SelectedDate.HasValue )
            {
                History.EvaluateChange( changes, "Retirement Fund", retirementFund.InactiveDate, dpInactiveDate.SelectedDate.Value );
                retirementFund.InactiveDate = dpInactiveDate.SelectedDate.Value;
            }

            if ( dvpFund.SelectedValueAsId().HasValue )
            {
                History.EvaluateChange( changes, "Retirement Fund", retirementFund.FundValue.Value, dvpFund.SelectedItem.Text );
                retirementFund.FundValueId = dvpFund.SelectedValueAsId().Value;
            }

            if ( retirementFund.IsValid )
            {
                rockContext.SaveChanges();

                if ( changes.Any() )
                {
                    HistoryService.SaveChanges( rockContext, typeof( Person ), com.centralaz.HumanResources.SystemGuid.Category.HISTORY_HUMAN_RESOURCES.AsGuid(),
                        Person.Id, changes );
                }

                hfIdValue.Value = string.Empty;
                mdDetails.Hide();

                BindGrid();
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the display.
        /// </summary>
        private void SetDisplay()
        {
            pnlList.Visible = true;
            nbMessage.Visible = false;
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // Exclude the categories for block and service job attributes, since they are controlled through code attribute decorations
            var exclusions = new List<Guid>();
            exclusions.Add( Rock.SystemGuid.EntityType.BLOCK.AsGuid() );
            exclusions.Add( Rock.SystemGuid.EntityType.SERVICE_JOB.AsGuid() );

            var rockContext = new RockContext();

            gRetirementFunds.DataSource = GetSalaries()
                .Select( c => new
                {
                    Id = c.Id,
                    Name = c.FundValue.Value,
                    EmployeeAmount = c.IsFixedAmount ? c.EmployeeAmount.FormatAsCurrency() : c.EmployeeAmount.ToString( "P" ),
                    EmployerAmount = c.IsFixedAmount ? c.EmployerAmount.FormatAsCurrency() : c.EmployerAmount.ToString( "P" ),
                    ActiveDate = c.ActiveDate,
                    InactiveDate = c.InactiveDate,
                } ).ToList();

            gRetirementFunds.EntityTypeId = EntityTypeCache.Read<com.centralaz.HumanResources.Model.RetirementFund>().Id;
            gRetirementFunds.DataBind();
        }

        private IEnumerable<RetirementFund> GetSalaries( RockContext rockContext = null )
        {
            return GetUnorderedSalaries( rockContext )
                .OrderByDescending( a => a.ActiveDate );
        }

        private IEnumerable<RetirementFund> GetUnorderedSalaries( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var queryable = new RetirementFundService( rockContext )
                .Queryable().Where( s => s.PersonAlias.PersonId == Person.Id );

            return queryable;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int retirementFundId )
        {
            RetirementFund retirementFund = null;
            if ( retirementFundId > 0 )
            {
                retirementFund = new RetirementFundService( new RockContext() ).Get( retirementFundId );
            }

            if ( retirementFund == null )
            {
                retirementFund = new RetirementFund
                {
                    Id = 0
                };
            }

            cbFixedAmount.Checked = retirementFund.IsFixedAmount;
            dpActiveDate.SelectedDate = retirementFund.ActiveDate;
            dpInactiveDate.SelectedDate = retirementFund.InactiveDate;
            nbEmployeeAmount.Text = retirementFund.EmployeeAmount.ToString();
            nbEmployerAmount.Text = retirementFund.EmployerAmount.ToString();

            if ( GetAttributeValue( "FundDefinedType" ).AsGuid() != null )
            {
                dvpFund.DefinedTypeId = DefinedTypeCache.Read( GetAttributeValue( "FundDefinedType" ).AsGuid() ).Id;
                dvpFund.SetValue( retirementFund.FundValueId.ToString() );
            }

            hfIdValue.Value = retirementFundId.ToString();
            mdDetails.Show();
        }

        #endregion

    }
}