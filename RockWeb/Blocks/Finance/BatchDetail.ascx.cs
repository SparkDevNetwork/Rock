// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Batch Detail" )]
    [Category( "Finance" )]
    [Description( "Displays the details of the given financial batch." )]
    public partial class BatchDetail : Rock.Web.UI.RockBlock, IDetailBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );
            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "financialBatchId" ).AsInteger() );
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSaveFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSaveFinancialBatch_Click( object sender, EventArgs e )
        {
            var rockContext = new RockContext();
            var financialBatchService = new FinancialBatchService( rockContext );
            FinancialBatch financialBatch = null;

            int financialBatchId = 0;
            if ( !string.IsNullOrEmpty( hfBatchId.Value ) )
            {
                financialBatchId = int.Parse( hfBatchId.Value );
            }

            if ( financialBatchId == 0 )
            {
                financialBatch = new Rock.Model.FinancialBatch();
                financialBatchService.Add( financialBatch );
            }
            else
            {
                financialBatch = financialBatchService.Get( financialBatchId );
            }

            financialBatch.Name = tbName.Text;
            financialBatch.BatchStartDateTime = drpBatchDate.LowerValue;
            financialBatch.BatchEndDateTime = drpBatchDate.UpperValue;
            financialBatch.CampusId = campCampus.SelectedCampusId;
            financialBatch.Status = (BatchStatus)ddlStatus.SelectedIndex;
            decimal fcontrolamt = 0;
            decimal.TryParse( tbControlAmount.Text, out fcontrolamt );
            financialBatch.ControlAmount = fcontrolamt;

            if ( !financialBatch.IsValid )
            {
                // Controls will render the error messages                    
                return;
            }

            rockContext.SaveChanges();
            hfBatchId.SetValue( financialBatch.Id );

            foreach ( var block in RockPage.RockBlocks.OfType<RockWeb.Blocks.Finance.TransactionList>() )
            {
                ( (RockWeb.Blocks.Finance.TransactionList)block ).RefreshList();
            }

            var savedFinancialBatch = new FinancialBatchService( rockContext ).Get( hfBatchId.ValueAsInt() );
            ShowSummary( savedFinancialBatch );
        }

        /// <summary>
        /// Handles the Click event of the lbCancelFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbCancelFinancialBatch_Click( object sender, EventArgs e )
        {
            if ( hfBatchId.ValueAsInt() != 0 )
            {
                var savedFinancialBatch = new FinancialBatchService( new RockContext() ).Get( hfBatchId.ValueAsInt() );
                ShowSummary( savedFinancialBatch );
            }
            else
            {
                NavigateToParentPage();
            }
        }

        /// <summary>
        /// Handles the Click event of the lbEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbEdit_Click( object sender, EventArgs e )
        {
            var financialBatchService = new FinancialBatchService( new RockContext() );
            var financialBatch = financialBatchService.Get( hfBatchId.ValueAsInt() );
            ShowEdit( financialBatch );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;
            valSummaryBatch.Enabled = editable;
            fieldsetViewSummary.Visible = !editable;
        }

        /// <summary>
        /// Shows the financial batch summary.
        /// </summary>
        /// <param name="financialBatch">The financial batch.</param>
        private void ShowSummary( FinancialBatch financialBatch )
        {
            string batchDate = string.Empty;
            if ( financialBatch.BatchStartDateTime != null )
            {
                batchDate = financialBatch.BatchStartDateTime.Value.ToShortDateString();
            }

            lTitle.Text = string.Format( "{0} <small>{1}</small>", financialBatch.Name.FormatAsHtmlTitle(), batchDate );

            SetEditMode( false );

            string campus = string.Empty;
            if ( financialBatch.CampusId.HasValue )
            {
                campus = financialBatch.Campus.ToString();
            }

            hfBatchId.SetValue( financialBatch.Id );
            lDetailsLeft.Text = new DescriptionList()
                .Add( "Title", financialBatch.Name )
                .Add( "Status", financialBatch.Status.ToString() )
                .Add( "Batch Start Date", Convert.ToDateTime( financialBatch.BatchStartDateTime ).ToString( "MM/dd/yyyy" ) )
                .Html;

            lDetailsRight.Text = new DescriptionList()
                .Add( "Control Amount", financialBatch.ControlAmount.ToString() )
                .Add( "Campus", campus )
                .Add( "Batch End Date", Convert.ToDateTime( financialBatch.BatchEndDateTime ).ToString( "MM/dd/yyyy" ) )
                .Html;
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="financialBatch">The financial batch.</param>
        protected void ShowEdit( FinancialBatch financialBatch )
        {
            if ( financialBatch.Id > 0 )
            {
                lTitle.Text = ActionTitle.Edit( FinancialBatch.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lTitle.Text = ActionTitle.Add( FinancialBatch.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            ddlStatus.BindToEnum( typeof( BatchStatus ) );
            hfBatchId.Value = financialBatch.Id.ToString();
            tbName.Text = financialBatch.Name;
            tbControlAmount.Text = financialBatch.ControlAmount.ToString();
            ddlStatus.SelectedIndex = (int)(BatchStatus)financialBatch.Status;
            campCampus.Campuses = new CampusService( new RockContext() ).Queryable().OrderBy( a => a.Name ).ToList();
            if ( financialBatch.CampusId.HasValue )
            {
                campCampus.SelectedCampusId = financialBatch.CampusId;
            }

            drpBatchDate.LowerValue = financialBatch.BatchStartDateTime;
            drpBatchDate.UpperValue = financialBatch.BatchEndDateTime;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="financialBatchId">The financial batch identifier.</param>
        public void ShowDetail( int financialBatchId )
        {
            FinancialBatch financialBatch = null;

            if ( !financialBatchId.Equals( 0 ) )
            {
                financialBatch = new FinancialBatchService( new RockContext() ).Get( financialBatchId );
            }

            if ( financialBatch == null )
            {
                financialBatch = new FinancialBatch { Id = 0 };
            }

            bool readOnly = false;
            if ( !IsUserAuthorized( Authorization.EDIT ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialBatch.FriendlyTypeName );
            }

            if ( !readOnly )
            {
                lbEdit.Visible = true;
                if ( financialBatch.Id > 0 )
                {
                    ShowSummary( financialBatch );
                }
                else
                {
                    ShowEdit( financialBatch );
                }
            }
            else
            {
                lbEdit.Visible = false;
                ShowSummary( financialBatch );
            }

            lbSave.Visible = !readOnly;
        }

        #endregion
    }
}
