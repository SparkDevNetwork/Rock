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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Donation Entry" )]
    [Category( "Fundraising" )]
    [Description( "Block that starts out a Fundraising Donation by prompting for information prior to going to a TransactionEntry block" )]

    [LinkedPage( "Transaction Entry Page", "The Transaction Entry page to navigate to after prompting for the Fundraising Specific inputs", required: true, order: 1 )]
    [BooleanField( "Show First Name Only", "Only show the First Name of each participant instead of Full Name", defaultValue: false, order: 2 )]
    [BooleanField( "Allow Automatic Selection", "If enabled and there is only one participant and registrations are not enabled then that participant will automatically be selected and this page will get bypassed.", defaultValue: false, order: 3 )]
    public partial class FundraisingDonationEntry : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? groupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();
                int? groupMemberId = this.PageParameter( "GroupMemberId" ).AsIntegerOrNull();
                ShowView( groupId, groupMemberId );
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="groupMemberId">The group member identifier.</param>
        protected void ShowView( int? groupId, int? groupMemberId )
        {
            Group group = null;
            GroupMember groupMember = null;
            var rockContext = new RockContext();
            if ( groupMemberId.HasValue )
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId.Value );
                if ( groupMember != null )
                {
                    group = groupMember.Group;
                }
            }
            else if ( groupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( groupId.Value );
            }

            PopulateGroupDropDown();
            ddlFundraisingOpportunity.Visible = group == null;
            lFundraisingOpportunity.Visible = group != null;
            if ( group != null )
            {
                group.LoadAttributes( rockContext );
                var opportunityTitle = group.GetAttributeValue( "OpportunityTitle" );
                lFundraisingOpportunity.Text = opportunityTitle;
                RockPage.Title = "Donate to " + opportunityTitle;
                RockPage.BrowserTitle = "Donate to " + opportunityTitle;
                RockPage.Header.Title = "Donate to " + opportunityTitle;

                ddlFundraisingOpportunity.SetValue( group.Id );
                ddlFundraisingOpportunity_SelectedIndexChanged( null, null );

                //
                // If they did not specify a group member in the query string AND there is only one
                // participant AND that participant is Active AND a registration instance has not
                // been configured. This allows for single-member projects that do not need the user
                // to select a specific person before donating.
                //
                if ( GetAttributeValue( "AllowAutomaticSelection" ).AsBoolean( false ) && groupMember == null )
                {
                    var members = group.Members.Where( m => m.GroupRole.Guid == "F82DF077-9664-4DA8-A3D9-7379B690124D".AsGuid() ).ToList();
                    if ( members.Count == 1 && members[0].GroupMemberStatus == GroupMemberStatus.Active )
                    {
                        group.LoadAttributes( rockContext );
                        if ( string.IsNullOrWhiteSpace( group.GetAttributeValue( "RegistrationInstance" ) ) )
                        {
                            groupMember = group.Members.First();
                        }
                    }
                }
            }

            if ( groupMember != null )
            {
                // if the GroupMember was specified, we know the Fundraising Opportunity and the Participant, so navigate to the Transaction Entry block
                ddlParticipant.SetValue( groupMember.Id );
                btnNext_Click( null, null );
            }
        }

        /// <summary>
        /// Populates the group drop down.
        /// </summary>
        protected void PopulateGroupDropDown()
        {
            var rockContext = new RockContext();
            Guid groupTypeFundraisingOpportunity = "4BE7FC44-332D-40A8-978E-47B7035D7A0C".AsGuid();
            var fundraisingOpportunityList = new GroupService( rockContext ).Queryable().Where( a => a.GroupType.Guid == groupTypeFundraisingOpportunity && a.IsActive && a.Members.Any() ).OrderBy( a => a.Order ).ThenBy( a => a.Name ).ToList();
            ddlFundraisingOpportunity.Items.Clear();
            ddlFundraisingOpportunity.Items.Add( new ListItem() );

            foreach ( var fundraisingOpportunity in fundraisingOpportunityList )
            {
                fundraisingOpportunity.LoadAttributes( rockContext );
                var dateRange = DateRangePicker.CalculateDateRangeFromDelimitedValues( fundraisingOpportunity.GetAttributeValue( "OpportunityDateRange" ) );
                if ( RockDateTime.Now <= ( dateRange.End ?? DateTime.MaxValue ) )
                {
                    var listItem = new ListItem( fundraisingOpportunity.GetAttributeValue( "OpportunityTitle" ), fundraisingOpportunity.Id.ToString() );
                    if ( listItem.Text.IsNullOrWhiteSpace() )
                    {
                        // just in case the OpportunityTitle wasn't set
                        listItem.Text = fundraisingOpportunity.Name;
                    }

                    ddlFundraisingOpportunity.Items.Add( listItem );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlFundraisingOpportunity control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlFundraisingOpportunity_SelectedIndexChanged( object sender, EventArgs e )
        {
            var groupId = ddlFundraisingOpportunity.SelectedValue.AsIntegerOrNull();
            ddlParticipant.Items.Clear();
            ddlParticipant.Items.Add( new ListItem() );
            if ( groupId.HasValue )
            {
                var rockContext = new RockContext();

                var groupMemberService = new GroupMemberService( rockContext );
                var groupMemberList = groupMemberService.Queryable().Where( a => a.GroupId == groupId && a.GroupMemberStatus == GroupMemberStatus.Active )
                    .OrderBy( a => a.Person.NickName ).ThenBy( a => a.Person.LastName ).Include( a => a.Person ).ToList();

                bool showOnlyFirstName = this.GetAttributeValue( "ShowFirstNameOnly" ).AsBoolean();
                foreach ( var groupMember in groupMemberList )
                {
                    groupMember.LoadAttributes( rockContext );

                    // only include participants that have not disabled public contribution requests
                    if ( !groupMember.GetAttributeValue( "DisablePublicContributionRequests" ).AsBoolean() )
                    {
                        var listItem = new ListItem();
                        listItem.Value = groupMember.Id.ToString();
                        if ( showOnlyFirstName )
                        {
                            listItem.Text = groupMember.Person.NickName;
                        }
                        else
                        {
                            listItem.Text = groupMember.Person.FullName;
                        }

                        ddlParticipant.Items.Add( listItem );
                    }
                }
            }
        }

        /// <summary>
        /// Handles the Click event of the btnNext control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnNext_Click( object sender, EventArgs e )
        {
            var groupMemberId = ddlParticipant.SelectedValue.AsIntegerOrNull();
            if ( groupMemberId.HasValue )
            {
                var rockContext = new RockContext();
                var groupMember = new GroupMemberService( rockContext ).Get( groupMemberId.Value );
                if ( groupMember != null )
                {
                    var queryParams = new Dictionary<string, string>();
                    queryParams.Add( "GroupMemberId", groupMemberId.ToString() );

                    groupMember.LoadAttributes( rockContext );
                    groupMember.Group.LoadAttributes( rockContext );
                    var financialAccount = new FinancialAccountService( rockContext ).Get( groupMember.Group.GetAttributeValue( "FinancialAccount" ).AsGuid() );
                    if ( financialAccount != null )
                    {
                        queryParams.Add( "AccountIds", financialAccount.Id.ToString() );
                    }

                    if ( groupMember.Group.GetAttributeValue( "CapFundraisingAmount" ).AsBoolean() )
                    {
                        var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

                        var contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                                    .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                            && d.EntityId == groupMemberId )
                                    .Sum( a => (decimal?)a.Amount ) ?? 0.00M;

                        var individualFundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                        if ( !individualFundraisingGoal.HasValue )
                        {
                            individualFundraisingGoal = groupMember.Group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                        }

                        var amountLeft = individualFundraisingGoal - contributionTotal;
                        queryParams.Add( "AmountLimit", amountLeft.ToString() );
                    }

                    NavigateToLinkedPage( "TransactionEntryPage", queryParams );
                }
            }
        }

        #endregion
    }
}