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
    [DisplayName( "Salary List" )]
    [Category( "com_centralaz > Human Resources" )]
    [Description( "Block for managing salaries for a person" )]
    public partial class SalaryList : PersonBlock
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

            gSalaries.DataKeyNames = new string[] { "Id" };
            gSalaries.Actions.ShowAdd = true;

            gSalaries.Actions.AddClick += gSalaries_Add;
            gSalaries.GridReorder += gSalaries_GridReorder;
            gSalaries.GridRebind += gSalaries_GridRebind;

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
                    Salary salary = new SalaryService( rockContext ).Get( hfIdValue.Value.AsInteger() );
                    if ( salary == null )
                    {
                        salary = new Salary();
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

        protected void gSalaries_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gSalaries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gSalaries_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var service = new SalaryService( rockContext );

            var salary = service.Get( e.RowKeyId );
            if ( salary != null )
            {
                int salaryId = salary.Id;
                var changes = new List<string>();

                service.Delete( salary );
                History.EvaluateChange( changes, "Salary", salary.IsSalariedEmployee ? salary.Amount.FormatAsCurrency() + "/yr" : salary.Amount.FormatAsCurrency() + "/hr", "" );
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
        /// Handles the Add event of the gSalaries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSalaries_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        /// <summary>
        /// Handles the GridRebind event of the gSalaries control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gSalaries_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        void gSalaries_GridReorder( object sender, GridReorderEventArgs e )
        {
            var rockContext = new RockContext();
            var categories = GetSalaries( rockContext );
            if ( categories != null )
            {
                var changedIds = new SalaryService( rockContext ).Reorder( categories.ToList(), e.OldIndex, e.NewIndex );
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
            int salaryId = 0;
            if ( hfIdValue.Value != string.Empty && !int.TryParse( hfIdValue.Value, out salaryId ) )
            {
                salaryId = 0;
            }

            var rockContext = new RockContext();
            var service = new SalaryService( rockContext );
            Salary salary = null;
            var changes = new List<string>();

            if ( salaryId != 0 )
            {
                salary = service.Get( salaryId );
            }

            if ( salary == null )
            {
                salary = new Salary();
                service.Add( salary );
                changes.Add( "Added new Salary" );
            }

            History.EvaluateChange( changes, "Salary", salary.PersonAliasId, Person.PrimaryAliasId.Value );
            salary.PersonAliasId = Person.PrimaryAliasId.Value;

            History.EvaluateChange( changes, "Salary", salary.IsSalariedEmployee.ToTrueFalse(), cbSalaried.Checked.ToTrueFalse() );
            salary.IsSalariedEmployee = cbSalaried.Checked;

            History.EvaluateChange( changes, "Salary", salary.Amount.ToString(), nbAmount.Text );
            salary.Amount = nbAmount.Text.AsDouble();

            History.EvaluateChange( changes, "Salary", salary.HousingAllowance.ToString(), nbHousing.Text );
            salary.HousingAllowance = nbHousing.Text.AsDouble();

            History.EvaluateChange( changes, "Salary", salary.FuelAllowance.ToString(), nbFuel.Text );
            salary.FuelAllowance = nbFuel.Text.AsDouble();

            History.EvaluateChange( changes, "Salary", salary.PhoneAllowance.ToString(), nbPhone.Text );
            salary.PhoneAllowance = nbPhone.Text.AsDouble();

            if ( dpEffectiveDate.SelectedDate.HasValue )
            {
                History.EvaluateChange( changes, "Salary", salary.EffectiveDate, dpEffectiveDate.SelectedDate.Value );
                salary.EffectiveDate = dpEffectiveDate.SelectedDate.Value;
            }

            if ( dpReviewedDate.SelectedDate.HasValue )
            {
                History.EvaluateChange( changes, "Salary", salary.ReviewedDate, dpReviewedDate.SelectedDate.Value );
                salary.ReviewedDate = dpReviewedDate.SelectedDate.Value;
            }

            if ( salary.IsValid )
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

            gSalaries.DataSource = GetSalaries()
                .Select( c => new
                {
                    Id = c.Id,
                    Amount = c.IsSalariedEmployee ? c.Amount.FormatAsCurrency() + "/yr" : c.Amount.FormatAsCurrency() + "/hr",
                    HousingAllowance = c.HousingAllowance,
                    FuelAllowance = c.FuelAllowance,
                    PhoneAllowance = c.PhoneAllowance,
                    EffectiveDate = c.EffectiveDate,
                    ReviewedDate = c.ReviewedDate,
                } ).ToList();

            gSalaries.EntityTypeId = EntityTypeCache.Read<com.centralaz.HumanResources.Model.Salary>().Id;
            gSalaries.DataBind();
        }

        private IEnumerable<Salary> GetSalaries( RockContext rockContext = null )
        {
            return GetUnorderedSalaries( rockContext )
                .OrderByDescending( a => a.EffectiveDate );
        }

        private IEnumerable<Salary> GetUnorderedSalaries( RockContext rockContext = null )
        {
            rockContext = rockContext ?? new RockContext();

            var queryable = new SalaryService( rockContext )
                .Queryable().Where( s => s.PersonAlias.PersonId == Person.Id )
                .ToList();

            return queryable;
        }


        /// <summary>
        /// Shows the edit.
        /// </summary>
        /// <param name="attributeId">The attribute id.</param>
        protected void ShowEdit( int salaryId )
        {
            Salary salary = null;
            if ( salaryId > 0 )
            {
                salary = new SalaryService( new RockContext() ).Get( salaryId );
            }

            if ( salary == null )
            {
                salary = new Salary
                {
                    Id = 0
                };
            }

            cbSalaried.Checked = salary.IsSalariedEmployee;
            dpEffectiveDate.SelectedDate = salary.EffectiveDate;
            dpReviewedDate.SelectedDate = salary.ReviewedDate;
            nbAmount.Text = salary.Amount.ToString();
            nbHousing.Text = salary.HousingAllowance.ToString();
            nbFuel.Text = salary.FuelAllowance.ToString();
            nbPhone.Text = salary.PhoneAllowance.ToString();

            hfIdValue.Value = salaryId.ToString();
            mdDetails.Show();
        }

        #endregion

    }
}