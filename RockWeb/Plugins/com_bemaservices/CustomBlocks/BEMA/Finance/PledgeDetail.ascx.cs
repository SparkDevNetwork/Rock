// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;

/*
 * BEMA Modified Core Block ( v10.2.1)
 * Version Number based off of RockVersion.RockHotFixVersion.BemaFeatureVersion
 * 
 * Additional Features:
 * - FE1) Added Ability to save then add new pledge with a new button
 * - UI1) Added Ability to add a class to the person picker
 * - UI2) Added Ability to limit accounts to active accounts
 */
namespace Rock.Plugins.com_bemaservices.Finance
{
    [DisplayName( "Pledge Detail" )]
    [Category( "BEMA Services > Finance" )]
    [Description( "Allows the details of a given pledge to be edited." )]
    [GroupTypeField( "Select Group Type", "Optional Group Type that if selected will display a list of groups that pledge can be associated to for selected user", false, "", "", 1 )]

    /* BEMA.FE1.Start */
    [BooleanField(
        "Show Add then New Button?",
        Key = BemaAttributeKey.ShowAddThenNewButton,
        DefaultValue = "False",
        Category = "BEMA Additional Features" )]
    // UMC Value = true
    /* BEMA.FE1.End */

    /* BEMA.UI1.Start */
    [TextField(
        "Person Picker CSS Override",
        Key = BemaAttributeKey.PersonPickerCssOverride,
        Description = "This attribute will override the person picker's css with whatever class is specified here.",
        IsRequired = false,
        DefaultValue = "",
        Category = "BEMA Additional Features" )]
    // UMC Value = "js-person"
    /* BEMA.UI1.End */

    /* BEMA.UI2.Start */
    [BooleanField(
        "Are accounts limited to active accounts?",
        Key = BemaAttributeKey.IsPickerLimitedToActiveAccounts,
        DefaultValue = "False",
        Category = "BEMA Additional Features" )]
    // UMC Value = true
    /* BEMA.UI2.End */
    public partial class PledgeDetail : RockBlock, IDetailBlock
    {
        /* BEMA.Start */
        #region Attribute Keys
        private static class BemaAttributeKey
        {
            public const string ShowAddThenNewButton = "ShowAddThenNewButton";
            public const string PersonPickerCssOverride = "PersonPickerCssOverride";
            public const string IsPickerLimitedToActiveAccounts = "IsPickerLimitedToActiveAccounts";
        }

        #endregion
        /* BEMA.End */

        #region Properties
        /* BEMA.FE1.Start */
        private Control _focusControl = null;
        /* BEMA.FE1.End */
        #endregion
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

            /* BEMA.UI1.Start */
            var personPickerClass = GetAttributeValue( BemaAttributeKey.PersonPickerCssOverride );
            if ( personPickerClass.IsNotNullOrWhiteSpace() )
            {
                ppPerson.CssClass = personPickerClass;
            }
            /* BEMA.UI1.End */

            /* BEMA.UI2.Start */
            var areInactiveAccountsHidden = GetAttributeValue( BemaAttributeKey.IsPickerLimitedToActiveAccounts ).AsBoolean();
            if ( areInactiveAccountsHidden )
            {
                apAccount.DisplayActiveOnly = true;
            }
            /* BEMA.UI2.End */

            // Add any attribute controls. 
            // This must be done here regardless of whether it is a postback so that the attribute values will get saved.
            var pledge = new FinancialPledgeService( new RockContext() ).Get( pledgeId );
            if ( pledge == null )
            {
                pledge = new FinancialPledge();

                /* BEMA.FE1.Start */
                btnSaveNew.Visible = GetAttributeValue( BemaAttributeKey.ShowAddThenNewButton ).AsBoolean();
                /* BEMA.FE1.End */
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

        /* BEMA.FE1.Start */
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.PreRender" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnPreRender( EventArgs e )
        {
            if ( _focusControl != null )
            {
                _focusControl.Focus();
            }

            base.OnPreRender( e );
        }
        /* BEMA.FE1.End */

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

            pledge.PledgeFrequencyValueId = dvpFrequencyType.SelectedValue.AsIntegerOrNull();

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

        /* BEMA.FE1.Start */
        protected void btnSaveNew_Click( object sender, EventArgs e )
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

            pledge.PledgeFrequencyValueId = dvpFrequencyType.SelectedValue.AsIntegerOrNull();

            pledge.LoadAttributes( rockContext );
            Rock.Attribute.Helper.GetEditValues( phAttributes, pledge );

            if ( !pledge.IsValid )
            {
                ShowInvalidResults( pledge.ValidationResults );
                return;
            }

            rockContext.SaveChanges();
            pledge.SaveAttributeValues( rockContext );

            pledge.AttributeValues.Clear();

            // clear person and amount values
            ppPerson.SetValue( null );
            tbAmount.Text = string.Empty;

            _focusControl = ppPerson;

            var pledgeNew = new FinancialPledgeService( new RockContext() ).Get( pledgeId );
            if ( pledgeNew == null )
            {
                pledgeNew = new FinancialPledge();
            }
            pledgeNew.LoadAttributes();
            phAttributes.Controls.Clear();
            Helper.AddEditControls( pledgeNew, phAttributes, true, BlockValidationGroup );

        }
        /* BEMA.FE1.End */

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
            dvpFrequencyType.DefinedTypeId = DefinedTypeCache.Get( frequencyTypeGuid ).Id;

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

                dvpFrequencyType.SelectedValue = !isNewPledge ? pledge.PledgeFrequencyValueId.ToString() : string.Empty;
                dvpFrequencyType.Enabled = !isReadOnly;

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
                            m.Group.IsActive && !m.Group.IsArchived )
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