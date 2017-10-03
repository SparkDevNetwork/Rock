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
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
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
    [GroupTypeField( "Select Group Type", "Optional Group Type that if selected will display a list of groups that pledge can be associated to for selected user", false, "", "", 1 )]
    public partial class PledgeDetail : RockBlock, IDetailBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            nbInvalid.Visible = false;

            var pledgeId = PageParameter( "pledgeId" ).AsInteger();
            if ( !IsPostBack )
            {
                ShowDetail( pledgeId );
            }

            // Add any attribute controls. 
            // This must be done here regardless of whether it is a postback so that the attribute values will get saved.
            var pledge = new FinancialPledgeService( new RockContext() ).Get( pledgeId );
            if ( pledge == null )
            {
                pledge = new FinancialPledge();
            }
            pledge.LoadAttributes();
            phAttributes.Controls.Clear();
            Helper.AddEditControls( pledge, phAttributes, true, BlockValidationGroup );
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            LoadGroups( ddlGroup.SelectedValueAsInt() );
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

            pledge.PersonAliasId = ppPerson.PersonAliasId;
            pledge.GroupId = ddlGroup.SelectedValueAsInt();
            pledge.AccountId = apAccount.SelectedValue.AsIntegerOrNull();
            pledge.TotalAmount = tbAmount.Text.AsDecimal();

            pledge.StartDate = dpDateRange.LowerValue.HasValue ? dpDateRange.LowerValue.Value : DateTime.MinValue;
            pledge.EndDate = dpDateRange.UpperValue.HasValue ? dpDateRange.UpperValue.Value : DateTime.MaxValue;

            pledge.PledgeFrequencyValueId = ddlFrequencyType.SelectedValue.AsIntegerOrNull();

            pledge.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, pledge );

            if ( !pledge.IsValid )
            {
                ShowInvalidResults( pledge.ValidationResults );
                return;
            }

            rockContext.SaveChanges();
            pledge.SaveAttributeValues( rockContext );

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

            using ( var rockContext = new RockContext() )
            {
                FinancialPledge pledge = null;

                if ( pledgeId > 0 )
                {
                    pledge = new FinancialPledgeService( rockContext ).Get( pledgeId );
                    lActionTitle.Text = ActionTitle.Edit( FinancialPledge.FriendlyTypeName ).FormatAsHtmlTitle();
                    pdAuditDetails.SetEntity( pledge, ResolveRockUrl( "~" ) );
                }

                if ( pledge == null )
                {
                    pledge = new FinancialPledge();
                    lActionTitle.Text = ActionTitle.Add( FinancialPledge.FriendlyTypeName ).FormatAsHtmlTitle();
                    // hide the panel drawer that show created and last modified dates
                    pdAuditDetails.Visible = false;
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

                GroupType groupType = null;
                Guid? groupTypeGuid = GetAttributeValue( "SelectGroupType" ).AsGuidOrNull();
                if ( groupTypeGuid.HasValue )
                {
                    groupType = new GroupTypeService( rockContext ).Get( groupTypeGuid.Value );
                }

                if ( groupType != null )
                {
                    ddlGroup.Label = groupType.Name;
                    ddlGroup.Visible = true;
                    LoadGroups( pledge.GroupId );
                    ddlGroup.Enabled = !isReadOnly;
                }
                else
                {
                    ddlGroup.Visible = false;
                }

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

        private void LoadGroups( int? currentGroupId )
        {
            ddlGroup.Items.Clear();

            int? personId = ppPerson.PersonId;
            Guid? groupTypeGuid = GetAttributeValue( "SelectGroupType" ).AsGuidOrNull();
            if ( personId.HasValue && groupTypeGuid.HasValue  )
            {
                using ( var rockContext = new RockContext() )
                {
                    var groups = new GroupMemberService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( m =>
                            m.Group.GroupType.Guid == groupTypeGuid.Value &&
                            m.PersonId == personId.Value &&
                            m.GroupMemberStatus == GroupMemberStatus.Active &&
                            m.Group.IsActive )
                        .Select( m => new
                        {
                            m.GroupId,
                            Name = m.Group.Name
                        } )
                        .ToList()
                        .Distinct()
                        .OrderBy( g => g.Name )
                        .ToList();

                    if ( groups.Any() )
                    {
                        ddlGroup.DataSource = groups;
                        ddlGroup.DataBind();
                        ddlGroup.Items.Insert(0, new ListItem() );
                        ddlGroup.SetValue( currentGroupId );
                    }
                }
            }
        }

        /// <summary>
        /// Shows the invalid results.
        /// </summary>
        /// <param name="validationResults">The validation results.</param>
        private void ShowInvalidResults( List<ValidationResult> validationResults )
        {
            nbInvalid.Text = string.Format( "Please correct the following:<ul><li>{0}</li></ul>", validationResults.AsDelimited( "</li><li>" ) );
            nbInvalid.Visible = true;
        }


    }
}