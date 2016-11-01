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
    [DisplayName( "Contribution Election List" )]
    [Category( "com_centralaz > Human Resources" )]
    [Description( "Block for managing contribution elections for a person" )]
    [AccountsField( "Displayed Accounts" )]
    public partial class ContributionElectionList : PersonBlock
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

            gContributionElections.DataKeyNames = new string[] { "Id" };
            gContributionElections.Actions.ShowAdd = true;

            gContributionElections.Actions.AddClick += gContributionElections_Add;
            gContributionElections.GridReorder += gContributionElections_GridReorder;
            gContributionElections.GridRebind += gContributionElections_GridRebind;

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
                    ContributionElection contributionElection = new ContributionElectionService( rockContext ).Get( hfIdValue.Value.AsInteger() );
                    if ( contributionElection == null )
                    {
                        contributionElection = new ContributionElection();
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

        protected void gContributionElections_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gContributionElections control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gContributionElections_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new ContributionElectionService( rockContext );

            var contributionElection = service.Get( e.RowKeyId );
            if ( contributionElection != null )
            {
                int contributionElectionId = contributionElection.Id;
                var changes = new List<string>();

                service.Delete( contributionElection );
                History.EvaluateChange( changes, "Contribution Election", contributionElection.IsFixedAmount ? contributionElection.Amount.FormatAsCurrency() : contributionElection.Amount.ToString( "P" ), "" );
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
        /// Handles the Add event of the gContributionElections control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContributionElections_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gContributionElections control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gContributionElections_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gContributionElections_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var categories = GetSalaries( rockContext );
            if ( categories != null )
            {
                var changedIds = new ContributionElectionService( rockContext ).Reorder( categories.ToList(), e.OldIndex, e.NewIndex );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the RowDataBound event of the gContributionElections control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gContributionElections_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                dynamic contributionElection = e.Row.DataItem;

                if ( contributionElection != null )
                {
                    if ( !contributionElection.IsActive )
                    {
                        e.Row.AddCssClass( "is-inactive" );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the SaveClick event of the modalDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void mdDetails_SaveClick( object sender, EventArgs e )
        {
            int contributionElectionId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out contributionElectionId ) )
            {
                contributionElectionId = 0;
            }

            var rockContext = new RockContext();
            var service = new ContributionElectionService( rockContext );
            ContributionElection contributionElection = null;
            var changes = new List<string>();

            if ( contributionElectionId != 0 )
            {
                contributionElection = service.Get( contributionElectionId );
            }

            if ( contributionElection == null )
            {
                contributionElection = new ContributionElection();
                service.Add( contributionElection );
                changes.Add( "Added new Contribution Election" );
            }

            History.EvaluateChange( changes, "Contribution Election", contributionElection.PersonAliasId, Person.PrimaryAliasId.Value );
            contributionElection.PersonAliasId = Person.PrimaryAliasId.Value;

            History.EvaluateChange( changes, "Contribution Election", contributionElection.IsFixedAmount.ToTrueFalse(), cbFixedAmount.Checked.ToTrueFalse() );
            contributionElection.IsFixedAmount = cbFixedAmount.Checked;

            History.EvaluateChange( changes, "Contribution Election", contributionElection.Amount.ToString(), nbAmount.Text );
            contributionElection.Amount = nbAmount.Text.AsDouble();

            if ( dpActiveDate.SelectedDate.HasValue )
            {
                History.EvaluateChange( changes, "Contribution Election", contributionElection.ActiveDate, dpActiveDate.SelectedDate.Value );
                contributionElection.ActiveDate = dpActiveDate.SelectedDate.Value;
            }

            if ( dpInactiveDate.SelectedDate.HasValue )
            {
                History.EvaluateChange( changes, "Contribution Election", contributionElection.InactiveDate, dpInactiveDate.SelectedDate.Value );
                contributionElection.InactiveDate = dpInactiveDate.SelectedDate.Value;
                contributionElection.IsActive = false;
            }
            else
            {
                contributionElection.IsActive = true;
            }

            if ( ddlAccounts.SelectedValueAsId().HasValue )
            {
                History.EvaluateChange( changes, "Contribution Election", contributionElection.FinancialAccount != null ? contributionElection.FinancialAccount.Name : "", ddlAccounts.SelectedItem.Text );
                contributionElection.FinancialAccountId = ddlAccounts.SelectedValueAsId().Value;
            }

            if ( contributionElection.IsValid )
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

            gContributionElections.DataSource = GetSalaries()
                .Select( c => new
                {
                    Id = c.Id,
                    IsActive = c.IsActive,
                    Name = c.FinancialAccount.Name,
                    Amount = c.IsFixedAmount ? c.Amount.FormatAsCurrency() : c.Amount.ToString( "#.00\\%" ),
                    ActiveDate = c.ActiveDate.ToShortDateString(),
                    InactiveDate = c.InactiveDate.HasValue ? c.InactiveDate.Value.ToShortDateString() : "",
                } ).ToList();

            gContributionElections.EntityTypeId = EntityTypeCache.Read<com.centralaz.HumanResources.Model.ContributionElection>().Id;
            gContributionElections.DataBind();
        }

        private IEnumerable<ContributionElection> GetSalaries( RockContext rockContext = null )
        {
            return GetUnorderedSalaries( rockContext )
                .OrderByDescending( a => a.ActiveDate );
        }

        private IEnumerable<ContributionElection> GetUnorderedSalaries( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var queryable = new ContributionElectionService( rockContext )
                .Queryable().Where( s => s.PersonAlias.PersonId == Person.Id );

            return queryable;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int contributionElectionId )
        {
            ContributionElection contributionElection = null;
            if ( contributionElectionId > 0 )
            {
                contributionElection = new ContributionElectionService( new RockContext() ).Get( contributionElectionId );
            }

            if ( contributionElection == null )
            {
                contributionElection = new ContributionElection
                {
                    Id = 0
                };
            }

            cbFixedAmount.Checked = contributionElection.IsFixedAmount;
            dpActiveDate.SelectedDate = contributionElection.ActiveDate;
            dpInactiveDate.SelectedDate = contributionElection.InactiveDate;
            nbAmount.Text = contributionElection.Amount.ToString();

            LoadAccounts();
            ddlAccounts.SetValue( contributionElection.FinancialAccountId.ToString() );

            hfIdValue.Value = contributionElectionId.ToString();
            mdDetails.Show();
        }

        private void LoadAccounts()
        {
            var rockContext = new RockContext();
            ddlAccounts.Items.Clear();
            FinancialAccountService accountService = new FinancialAccountService( rockContext );

            List<Guid> selectedAccounts = new List<Guid>();

            if ( !string.IsNullOrWhiteSpace( GetAttributeValue( "DisplayedAccounts" ) ) )
            {
                selectedAccounts = GetAttributeValue( "DisplayedAccounts" ).Split( ',' ).AsGuidList();
            }

            var accountList = accountService.Queryable()
                                .Where( a => selectedAccounts.Contains( a.Guid ) )
                                .OrderBy( a => a.Order )
                                .Select( a => new
                                {
                                    a.Id,
                                    a.PublicName
                                } ).ToList();

            if ( accountList.Any() )
            {
                foreach ( var account in accountList )
                {
                    ddlAccounts.Items.Add( new ListItem( account.PublicName, account.Id.ToString() ) );
                }
            }
        }
        #endregion
    }
}