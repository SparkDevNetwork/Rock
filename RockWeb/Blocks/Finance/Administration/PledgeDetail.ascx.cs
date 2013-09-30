//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance.Administration
{
    /// <summary>
    /// 
    /// </summary>
    public partial class PledgeDetail : RockBlock, IDetailBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !IsPostBack )
            {
                int itemId;

                // Using TryParse guarantees a valid int if it passes, while a 
                // `string.IsNullOrWhitespace` check doesn't. The param could be any
                // non-null value
                if ( int.TryParse( PageParameter( "pledgeId" ), out itemId ) )
                {
                    ShowDetail( "pledgeId", itemId );
                }
                else
                {
                    pnlDetails.Visible = false;
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            FinancialPledge pledge;
            var pledgeService = new FinancialPledgeService();
            var pledgeId = int.Parse( hfPledgeId.Value );

            if ( pledgeId == 0 )
            {
                pledge = new FinancialPledge();
                pledgeService.Add( pledge, CurrentPersonId );
            }
            else
            {
                pledge = pledgeService.Get( pledgeId );
            }

            pledge.PersonId = ppPerson.PersonId;
            pledge.AccountId = int.Parse( fpFund.SelectedValue );
            pledge.TotalAmount = decimal.Parse( tbAmount.Text );
            pledge.StartDate = DateTime.Parse( dtpStartDate.Text );
            pledge.EndDate = DateTime.Parse( dtpEndDate.Text );
            pledge.PledgeFrequencyValueId = int.Parse( ddlFrequencyType.SelectedValue );

            if ( !pledge.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            RockTransactionScope.WrapTransaction( () => pledgeService.Save( pledge, CurrentPersonId ) );
            NavigateToParentPage();
        }

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            NavigateToParentPage();
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="itemKey">The item key.</param>
        /// <param name="itemKeyValue">The item key value.</param>
        public void ShowDetail( string itemKey, int itemKeyValue )
        {
            pnlDetails.Visible = true;
            var frequencyTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY );
            ddlFrequencyType.BindToDefinedType( DefinedTypeCache.Read( frequencyTypeGuid ) );
            FinancialPledge pledge;

            if ( itemKeyValue > 0 )
            {
                pledge = new FinancialPledgeService().Get( itemKeyValue );
                lActionTitle.Text = ActionTitle.Edit( FinancialPledge.FriendlyTypeName );
            }
            else
            {
                pledge = new FinancialPledge();
                lActionTitle.Text = ActionTitle.Add( FinancialPledge.FriendlyTypeName );
            }

            var isReadOnly = !IsUserAuthorized( "Edit" );
            var isNewPledge = pledge.Id == 0;

            hfPledgeId.Value = pledge.Id.ToString();
            ppPerson.SetValue( pledge.Person );
            ppPerson.Enabled = !isReadOnly;
            fpFund.SetValue( pledge.Account );
            fpFund.Enabled = !isReadOnly;
            tbAmount.Text = !isNewPledge ? pledge.TotalAmount.ToString() : string.Empty;
            tbAmount.ReadOnly = isReadOnly;
            dtpStartDate.Text = !isNewPledge ? pledge.StartDate.ToShortDateString() : string.Empty;
            dtpStartDate.ReadOnly = isReadOnly;
            dtpEndDate.Text = !isNewPledge ? pledge.EndDate.ToShortDateString() : string.Empty;
            dtpEndDate.ReadOnly = isReadOnly;
            ddlFrequencyType.SelectedValue = !isNewPledge ? pledge.PledgeFrequencyValueId.ToString() : string.Empty;
            ddlFrequencyType.Enabled = !isReadOnly;

            if ( isReadOnly )
            {
                nbEditModeMessage.Text = EditModeMessage.ReadOnlyEditActionNotAllowed( FinancialPledge.FriendlyTypeName );
                lActionTitle.Text = ActionTitle.View( BlockType.FriendlyTypeName );
                btnCancel.Text = "Close";
            }

            btnSave.Visible = !isReadOnly;
        }
    }
}