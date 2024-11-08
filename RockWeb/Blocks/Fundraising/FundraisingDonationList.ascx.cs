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
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Web.Cache;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Donation List" )]
    [Category( "Fundraising" )]
    [Description( "Lists donations in a grid for the current fundraising opportunity or participant." )]

    [CustomCheckboxListField( "Hide Grid Columns", "The grid columns that should be hidden from the user.", "Amount, Donor Address, Donor Email, Participant", false, "", "Advanced", order: 0 )]
    [CustomCheckboxListField( "Hide Grid Actions", "The grid actions that should be hidden from the user.", "Communicate, Merge Person, Bulk Update, Excel Export, Merge Template", false, "", "Advanced", order: 1 )]
    [CodeEditorField( "Donor Column", "The value that should be displayed for the Donor column. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, true, @"<a href=""/Person/{{ Donor.Id }}"">{{ Donor.FullName }}</a>", "Advanced", order: 2 )]
    [CodeEditorField( "Participant Column", "The value that should be displayed for the Participant column. <span class='tip tip-lava'></span>", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, true, @"<a href=""/Person/{{ Participant.PersonId }}"" class=""pull-right margin-l-sm btn btn-sm btn-default"">
    <i class=""fa fa-user""></i>
</a>
<a href=""/GroupMember/{{ Participant.Id }}"">{{ Participant.Person.FullName }}</a>", "Advanced", order: 3 )]

    [ContextAware]
    [Rock.SystemGuid.BlockTypeGuid( "4AAE3DB5-C9F8-4985-B6DC-9037B2F91100" )]
    public partial class FundraisingDonationList : RockBlock
    {
        #region Base Method Overrides

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            base.BlockUpdated += FundraisingDonationsList_BlockUpdated;

            gDonations.GridRebind += gDonations_GridRebind;
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.</param>
        /// <param name="e">Arguments that describe this event.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Show the block content.
        /// </summary>
        protected void ShowDetails()
        {
            var group = ContextEntity<Group>();
            var groupMember = ContextEntity<GroupMember>();

            if ( groupMember != null )
            {
                group = groupMember.Group;
            }

            pnlDetails.Visible = false;

            //
            // Only show the panel and content if the group type is a fundraising opportunity.
            //
            var rockContext = new RockContext();
            var groupService = new GroupService( rockContext );
            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var fundraisingGroupTypeIdList = new GroupTypeService( rockContext ).Queryable().Where( a => a.Id == groupTypeIdFundraising || a.InheritedGroupTypeId == groupTypeIdFundraising ).Select( a => a.Id ).ToList();

            if ( group != null && fundraisingGroupTypeIdList.Contains( group.GroupTypeId ) )
            {
                pnlDetails.Visible = true;
                BindGrid();
            }
        }

        /// <summary>
        /// Bind the grid to the donations that should be visible for the proper context.
        /// </summary>
        protected void BindGrid( bool isExporting = false )
        {
            var rockContext = new RockContext();
            var groupMemberService = new GroupMemberService( rockContext );
            var financialTransactionDetailService = new FinancialTransactionDetailService( rockContext );
            var entityTypeIdGroupMember = EntityTypeCache.GetId<GroupMember>();
            var hideGridColumns = GetAttributeValue( "HideGridColumns" ).Split( ',' );
            var hideGridActions = GetAttributeValue( "HideGridActions" ).Split( ',' );
            var mergeFields = LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson );
            Group group = null;
            Dictionary<int, GroupMember> groupMembers;

            //
            // Get the donations for the entire opportunity group or for just the
            // one individual being viewed.
            //
            if ( ContextEntity<Group>() != null )
            {
                group = ContextEntity<Group>();

                groupMembers = groupMemberService.Queryable()
                    .Where( m => m.GroupId == group.Id )
                    .ToDictionary( m => m.Id );
            }
            else
            {
                var groupMember = ContextEntity<GroupMember>();
                group = groupMember.Group;

                groupMembers = new Dictionary<int, GroupMember> { { groupMember.Id, groupMember } };
            }

            //
            // Get the list of donation entries for the grid that match the list of members.
            //
            var groupMemberIds = groupMembers.Keys.ToList();
            var donations = financialTransactionDetailService.Queryable()
                .Where( d => d.EntityTypeId == entityTypeIdGroupMember && groupMemberIds.Contains( d.EntityId.Value ) )
                .ToList()
                .Select( d => new
                {
                    IsExporting = isExporting,
                    DonorId = d.Transaction.AuthorizedPersonAlias.PersonId,
                    Donor = d.Transaction.AuthorizedPersonAlias.Person,
                    Group = group,
                    Participant = groupMembers[d.EntityId.Value],
                    Amount = d.Amount,
                    Address = d.Transaction.AuthorizedPersonAlias.Person.GetHomeLocation( rockContext ).ToStringSafe().ConvertCrLfToHtmlBr(),
                    Date = d.Transaction.TransactionDateTime
                } ).AsQueryable();

            //
            // Apply user sorting or default to donor name.
            //
            if ( gDonations.SortProperty != null )
            {
                donations = donations.Sort( gDonations.SortProperty );
            }
            else
            {
                donations = donations.Sort( new SortProperty { Property = "Donor.LastName, Donor.NickName" } );
            }

            gDonations.ObjectList = donations.Select( d => d.Donor )
                .DistinctBy( p => p.Id )
                .Cast<object>()
                .ToDictionary( p => ( ( Person ) p ).Id.ToString() );

            //
            // Hide any columns they don't want visible to the user.
            //
            gDonations.ColumnsOfType<CurrencyField>()
                .First( c => c.DataField == "Amount" )
                .Visible = !hideGridColumns.Contains( "Amount" );
            gDonations.ColumnsOfType<RockBoundField>()
                .First( c => c.DataField == "Address" )
                .Visible = !hideGridColumns.Contains( "Donor Address" );
            gDonations.ColumnsOfType<RockBoundField>()
                .First( c => c.DataField == "Donor.Email" )
                .Visible = !hideGridColumns.Contains( "Donor Email" );
            gDonations.ColumnsOfType<RockLiteralField>()
                .First( c => c.HeaderText == "Participant" )
                .Visible = !hideGridColumns.Contains( "Participant" ) && ContextEntity<GroupMember>() == null;

            //
            // Hide any grid actions they don't want visible to the user.
            //
            gDonations.Actions.ShowCommunicate = !hideGridActions.Contains( "Communicate" );
            gDonations.Actions.ShowMergePerson = !hideGridActions.Contains( "Merge Person" );
            gDonations.Actions.ShowBulkUpdate = !hideGridActions.Contains( "Bulk Update" );
            gDonations.Actions.ShowExcelExport = !hideGridActions.Contains( "Excel Export" );
            gDonations.Actions.ShowMergeTemplate = !hideGridActions.Contains( "Merge Template" );

            //
            // If all the grid actions are hidden, hide the select column too.
            //
            gDonations.ColumnsOfType<SelectField>().First().Visible = gDonations.Actions.ShowCommunicate || gDonations.Actions.ShowMergePerson || gDonations.Actions.ShowBulkUpdate || gDonations.Actions.ShowExcelExport || gDonations.Actions.ShowMergeTemplate;

            gDonations.DataSource = donations.ToList();
            gDonations.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void FundraisingDonationsList_BlockUpdated( object sender, EventArgs e )
        {
            ShowDetails();
        }

        /// <summary>
        /// Handles the GridRebind event of the gDonations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gDonations_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gDonations control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Web.UI.WebControls.GridViewRowEventArgs"/> instance containing the event data.</param>
        protected void gDonations_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var item = e.Row.DataItem;
                var isExporting = ( bool ) item.GetPropertyValue( "IsExporting" );

                //
                // Get the merge fields to be available.
                //
                var options = new CommonMergeFieldsOptions();
                var mergeFields = LavaHelper.GetCommonMergeFields( RockPage, CurrentPerson, options );
                mergeFields.AddOrReplace( "Group", item.GetPropertyValue( "Group" ) );
                mergeFields.AddOrReplace( "Donor", item.GetPropertyValue( "Donor" ) );
                mergeFields.AddOrReplace( "Participant", item.GetPropertyValue( "Participant" ) );

                //
                // Set the Donor column value.
                //
                var column = gDonations.ColumnsOfType<RockLiteralField>().First( c => c.HeaderText == "Donor" );
                if ( column.Visible )
                {
                    var literal = ( Literal ) e.Row.Cells[gDonations.Columns.IndexOf( column )].Controls[0];
                    var donorText = GetAttributeValue( "DonorColumn" ).ResolveMergeFields( mergeFields );
                    if ( isExporting )
                    {
                        donorText = donorText.ScrubHtmlAndConvertCrLfToBr();
                    }
                    literal.Text = donorText;
                }

                //
                // Set the Participant column value.
                //
                column = gDonations.ColumnsOfType<RockLiteralField>().First( c => c.HeaderText == "Participant" );
                if ( column.Visible )
                {
                    var literal = ( Literal ) e.Row.Cells[gDonations.Columns.IndexOf( column )].Controls[0];
                    var donorText = GetAttributeValue( "ParticipantColumn" ).ResolveMergeFields( mergeFields );
                    if ( isExporting )
                    {
                        donorText = donorText.SanitizeHtml().Trim();
                    }
                    literal.Text = donorText;
                }
            }
        }

        #endregion
    }
}