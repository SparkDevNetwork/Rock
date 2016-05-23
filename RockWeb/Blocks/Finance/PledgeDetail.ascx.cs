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
using System.ComponentModel;

using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    [DisplayName( "Pledge Detail" )]
    [Category( "Finance" )]
    [Description( "Allows the details of a given pledge to be edited." )]
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
                ShowDetail( PageParameter( "pledgeId" ).AsInteger() );
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
            var rockContext = new RockContext();
            var pledgeService = new FinancialPledgeService( rockContext );
            var pledgeId = hfPledgeId.Value.AsInteger();

            if ( pledgeId == 0 )
            {
                pledge = new FinancialPledge();
                pledgeService.Add( pledge );
            }
            else
            {
                pledge = pledgeService.Get( pledgeId );
            }

            if (ppPerson.PersonId.HasValue)
            {
                pledge.PersonAliasId = ppPerson.PersonAliasId;
            }

            pledge.AccountId = apAccount.SelectedValue.AsIntegerOrNull();
            pledge.TotalAmount = tbAmount.Text.AsDecimal();

            pledge.StartDate = dpDateRange.LowerValue.HasValue ? dpDateRange.LowerValue.Value : DateTime.MinValue;
            pledge.EndDate = dpDateRange.UpperValue.HasValue ? dpDateRange.UpperValue.Value : DateTime.MaxValue;

            pledge.PledgeFrequencyValueId = ddlFrequencyType.SelectedValue.AsIntegerOrNull();

            if ( !pledge.IsValid )
            {
                // Controls will render the error messages
                return;
            }

            rockContext.SaveChanges();

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
        /// <param name="pledgeId">The pledge identifier.</param>
        public void ShowDetail( int pledgeId )
        {
            pnlDetails.Visible = true;
            var frequencyTypeGuid = new Guid( Rock.SystemGuid.DefinedType.FINANCIAL_FREQUENCY );
            ddlFrequencyType.BindToDefinedType( DefinedTypeCache.Read( frequencyTypeGuid ), true );

            FinancialPledge pledge = null;

            if ( pledgeId > 0 )
            {
                pledge = new FinancialPledgeService( new RockContext() ).Get( pledgeId );
                lActionTitle.Text = ActionTitle.Edit( FinancialPledge.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            if ( pledge == null )
            {
                pledge = new FinancialPledge();
                lActionTitle.Text = ActionTitle.Add( FinancialPledge.FriendlyTypeName ).FormatAsHtmlTitle();
            }

            var isReadOnly = !IsUserAuthorized( Authorization.EDIT );
            var isNewPledge = pledge.Id == 0;

            hfPledgeId.Value = pledge.Id.ToString();
            if ( pledge.PersonAlias != null )
            {
                ppPerson.SetValue( pledge.PersonAlias.Person );
            }
            else
            {
                ppPerson.SetValue( null );
            }
            ppPerson.Enabled = !isReadOnly;
            apAccount.SetValue( pledge.Account );
            apAccount.Enabled = !isReadOnly;
            tbAmount.Text = !isNewPledge ? pledge.TotalAmount.ToString() : string.Empty;
            tbAmount.ReadOnly = isReadOnly;

            dpDateRange.LowerValue = pledge.StartDate;
            dpDateRange.UpperValue = pledge.EndDate;
            dpDateRange.ReadOnly = isReadOnly;

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