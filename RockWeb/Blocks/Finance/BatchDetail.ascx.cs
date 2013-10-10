//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance.Administration
{
    /// <summary>
    /// 
    /// </summary>
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
                string itemId = PageParameter( "financialBatchId" );
                if ( !string.IsNullOrWhiteSpace( itemId ) )
                {
                    ShowDetail( "financialBatchId", int.Parse( itemId ) );
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
        /// Handles the Click event of the btnSaveFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnSaveFinancialBatch_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var financialBatchService = new FinancialBatchService();
                FinancialBatch financialBatch = null;

                int financialBatchId = 0;
                if ( !string.IsNullOrEmpty( hfBatchId.Value ) )
                {
                    financialBatchId = int.Parse( hfBatchId.Value );
                }

                if ( financialBatchId == 0 )
                {
                    financialBatch = new Rock.Model.FinancialBatch();
                    financialBatch.CreatedByPersonId = CurrentPersonId.Value;
                    financialBatchService.Add( financialBatch, CurrentPersonId );
                }
                else
                {
                    financialBatch = financialBatchService.Get( financialBatchId );
                }

                financialBatch.Name = tbName.Text;
                financialBatch.BatchStartDateTime = dtBatchDate.LowerValue;
                financialBatch.BatchEndDateTime = dtBatchDate.UpperValue;
                financialBatch.CampusId = cpCampus.SelectedCampusId;                
                financialBatch.Status = (BatchStatus) ddlStatus.SelectedIndex;
                decimal fcontrolamt = 0;
                decimal.TryParse( tbControlAmount.Text, out fcontrolamt );
                financialBatch.ControlAmount = fcontrolamt;

                if ( !financialBatch.IsValid )
                {
                    // Controls will render the error messages                    
                    return;
                }

                RockTransactionScope.WrapTransaction( () =>
                {
                    financialBatchService.Save( financialBatch, CurrentPersonId );
                    hfBatchId.SetValue( financialBatch.Id );
                } );
            }

            var savedFinancialBatch = new FinancialBatchService().Get( hfBatchId.ValueAsInt() );
            ShowReadOnly( savedFinancialBatch );
        }

        /// <summary>
        /// Handles the Click event of the btnCancelFinancialBatch control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancelFinancialBatch_Click( object sender, EventArgs e )
        {
            var savedFinancialBatch = new FinancialBatchService().Get( hfBatchId.ValueAsInt() );
            ShowReadOnly( savedFinancialBatch );
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            var financialBatchService = new FinancialBatchService();
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
            fieldsetViewDetails.Visible = !editable;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="financialBatch">The financial batch.</param>
        private void ShowReadOnly( FinancialBatch financialBatch )
        {
            SetEditMode( false );

            string campus = string.Empty;
            if ( financialBatch.CampusId.HasValue )
            {
                campus = financialBatch.Campus.ToString();
            }

            hfBatchId.SetValue( financialBatch.Id );
            lblDetails.Text = new DescriptionList()
                .Add( "Title", financialBatch.Name )
                .Add( "Control Amount", financialBatch.ControlAmount.ToString() )
                .Add( "Status", financialBatch.Status.ToString() )
                .Add( "Campus", campus )
                .Add( "Batch Start Date", financialBatch.BatchStartDateTime )
                .Add( "Batch End Date", financialBatch.BatchEndDateTime )
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
                lActionTitle.Text = ActionTitle.Edit( FinancialBatch.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lActionTitle.Text = ActionTitle.Add( FinancialBatch.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            SetEditMode( true );

            ddlStatus.BindToEnum( typeof( BatchStatus ) );
            hfBatchId.Value = financialBatch.Id.ToString();
            tbName.Text = financialBatch.Name;
            tbControlAmount.Text = financialBatch.ControlAmount.ToString();
            ddlStatus.SelectedIndex = (int)(BatchStatus) financialBatch.Status;
            cpCampus.Campuses = new CampusService().Queryable().OrderBy( a => a.Name ).ToList();
            if ( financialBatch.CampusId.HasValue )
            {
                cpCampus.SelectedCampusId = financialBatch.CampusId;
            }

            dtBatchDate.LowerValue = financialBatch.BatchStartDateTime;
            dtBatchDate.UpperValue = financialBatch.BatchEndDateTime;
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            if ( !itemKey.Equals( "financialBatchId" ) )
            {
                return;
            }

            FinancialBatch financialBatch = null;
            if ( !itemKeyValue.Equals( 0 ) )
            {
                financialBatch = new FinancialBatchService().Get( itemKeyValue );
            }
            else
            {
                financialBatch = new FinancialBatch { Id = 0 };
            }

            bool readOnly = false;
            if ( !IsUserAuthorized( "Edit" ) )
            {
                readOnly = true;
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialBatch.FriendlyTypeName );
            }

            if ( !readOnly )
            {
                btnEdit.Visible = true;
                if ( financialBatch.Id > 0 )
                {
                    ShowReadOnly( financialBatch );
                }
                else
                {
                    ShowEdit( financialBatch );
                }
            }
            else
            {
                btnEdit.Visible = false;
                ShowReadOnly( financialBatch );
            }

            btnSave.Visible = !readOnly;
        }

        #endregion
    }
}
