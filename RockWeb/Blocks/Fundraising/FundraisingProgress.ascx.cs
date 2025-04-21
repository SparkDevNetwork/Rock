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

using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Progress" )]
    [Category( "Fundraising" )]
    [Description( "Progress for all people in a fundraising opportunity" )]
    [Rock.SystemGuid.BlockTypeGuid( "75D2BC14-34DF-42EA-8DBB-3F5294B290A9" )]
    public partial class FundraisingProgress : RockBlock
    {
        #region Fields

        public decimal PercentComplete = 0;
        public decimal GroupIndividualFundraisingGoal;
        public decimal GroupContributionTotal;
        public string ProgressCssClass;

        #endregion

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
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                int? groupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();
                int? groupMemberId = this.PageParameter( "GroupMemberId" ).AsIntegerOrNull();

                if ( groupId.HasValue || groupMemberId.HasValue )
                {
                    ShowView( groupId, groupMemberId );
                }
                else
                {
                    pnlView.Visible = false;
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name = "groupId" > The group identifier.</param>
        protected void ShowView( int? groupId, int? groupMemberId )
        {
            var rockContext = new RockContext();
            Group group = null;
            GroupMember groupMember = null;

            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;

            pnlView.Visible = true;
            hfGroupId.Value = groupId.ToStringSafe();
            hfGroupMemberId.Value = groupMemberId.ToStringSafe();

            if ( groupId.HasValue )
            {
                group = new GroupService( rockContext ).Get( groupId.Value );
            }
            else
            {
                groupMember = new GroupMemberService( rockContext ).Get( groupMemberId ?? 0 );
                group = groupMember.Group;
            }

            if ( group == null || ( !( group.GroupTypeId == groupTypeIdFundraising || group.GroupType.InheritedGroupTypeId == groupTypeIdFundraising ) ) )
            {
                pnlView.Visible = false;
                return;
            }

            lTitle.Text = group.Name.FormatAsHtmlTitle();

            BindGroupMembersProgressGrid( group, groupMember, rockContext );
        }

        /// <summary>
        /// Binds the group members progress repeater.
        /// </summary>
        protected void BindGroupMembersProgressGrid( Group group, GroupMember gMember, RockContext rockContext )
        {
            IQueryable<GroupMember> groupMembersQuery;

            if ( gMember != null )
            {
                groupMembersQuery = new GroupMemberService( rockContext ).Queryable().Where( a => a.Id == gMember.Id );

                pnlHeader.Visible = false;
            }
            else
            {
                groupMembersQuery = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == group.Id );
            }

            group.LoadAttributes( rockContext );
            var defaultIndividualFundRaisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();

            groupMembersQuery = groupMembersQuery.Sort( new SortProperty { Property = "Person.LastName, Person.NickName" } );

            var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();
            var participationMode = group.GetAttributeValue( "ParticipationType" ).ConvertToEnumOrNull<ParticipationType>() ?? ParticipationType.Individual;

            if ( participationMode == ParticipationType.Family )
            {
                var groupMembersByFamily = groupMembersQuery.Select( g => g.Person.PrimaryFamily ).Distinct().OrderBy( g => g.Name ).ToList().Select( g =>
                   {
                       var familyGroup = g;
                       var groupService = new GroupService( rockContext );
                       var familyMemberGroupMembersInCurrentGroup = groupService.GroupMembersInAnotherGroup( familyGroup, group );
                       var contributionTotal = new FinancialTransactionDetailService( rockContext )
                       .GetContributionsForGroupMemberList( entityTypeIdGroupMember, familyMemberGroupMembersInCurrentGroup.Select( m => m.Id ).ToList() );

                       decimal groupFundraisingGoal = 0;
                       foreach ( var member in familyMemberGroupMembersInCurrentGroup )
                       {
                           member.LoadAttributes( rockContext );
                           groupFundraisingGoal += member.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull() ?? 0;
                       }

                       decimal percentageAchieved = 0;

                       percentageAchieved = groupFundraisingGoal == 0 ? 100 : contributionTotal / ( 0.01M * groupFundraisingGoal );

                       var progressBarWidth = percentageAchieved;

                       if ( percentageAchieved >= 100 )
                       {
                           progressBarWidth = 100;
                       }

                       var familyMembers = familyGroup.Members.Select( m => m.PersonId ).ToList();
                       var sortedFamilyMembers = familyMemberGroupMembersInCurrentGroup.OrderBy( m => m.Person.AgeClassification ).ThenBy( m => m.Person.Gender );

                       // If there is only one person in the fundraising group from the current family, just use that person's full name...
                       string progressTitle = sortedFamilyMembers.Count() == 1 ? sortedFamilyMembers.First().Person.FullName :

                       // Otherwise, use all the family members in the group to generate a list of their names.
                       string.Format(
                           "{0} ({1})",
                           familyGroup.Name,
                           sortedFamilyMembers.Select( m => m.Person.NickName ).JoinStringsWithRepeatAndFinalDelimiterWithMaxLength( ", ", " & ", 36 ) );

                       return new
                       {
                           ProgressTitle = progressTitle,
                           FundraisingGoal = groupFundraisingGoal.ToString( "0.##" ),
                           ContributionTotal = contributionTotal.ToString( "0.##" ),
                           Percentage = percentageAchieved.ToString( "0.##" ),
                           CssClass = GetProgressCssClass( percentageAchieved ),
                           ProgressBarWidth = progressBarWidth
                       };
                   } ).ToList();

                this.GroupIndividualFundraisingGoal = groupMembersByFamily.Sum( a => decimal.Parse( a.FundraisingGoal ) );
                this.GroupContributionTotal = groupMembersByFamily.Sum( a => decimal.Parse( a.ContributionTotal ) );
                this.PercentComplete = decimal.Round( this.GroupIndividualFundraisingGoal == 0 ? 100 : this.GroupContributionTotal / ( this.GroupIndividualFundraisingGoal * 0.01M ), 2 );
                this.ProgressCssClass = GetProgressCssClass( this.PercentComplete );

                rptFundingProgress.DataSource = groupMembersByFamily;
                rptFundingProgress.DataBind();
            }
            else
            {
                var groupMemberList = groupMembersQuery.ToList().Select( a =>
                {
                    var groupMember = a;
                    groupMember.LoadAttributes( rockContext );

                    var contributionTotal = new FinancialTransactionDetailService( rockContext ).Queryable()
                                .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                        && d.EntityId == groupMember.Id )
                                .Sum( d => ( decimal? ) d.Amount ) ?? 0;

                    var individualFundraisingGoal = groupMember.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                    bool disablePublicContributionRequests = groupMember.GetAttributeValue( "DisablePublicContributionRequests" ).AsBoolean();
                    if ( !individualFundraisingGoal.HasValue )
                    {
                        individualFundraisingGoal = group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();
                    }

                    decimal percentageAchieved = 0;
                    if ( individualFundraisingGoal != null )
                    {
                        percentageAchieved = individualFundraisingGoal == 0 ? 100 : contributionTotal / ( 0.01M * individualFundraisingGoal.Value );
                    }

                    var progressBarWidth = percentageAchieved;

                    if ( percentageAchieved >= 100 )
                    {
                        progressBarWidth = 100;
                    }

                    if ( !individualFundraisingGoal.HasValue )
                    {
                        individualFundraisingGoal = 0;
                    }

                    string progressTitle = groupMember.Person.FullName;

                    return new
                    {
                        ProgressTitle = progressTitle,
                        FundraisingGoal = ( individualFundraisingGoal ?? 0.00M ).ToString( "0.##" ),
                        ContributionTotal = contributionTotal.ToString( "0.##" ),
                        Percentage = percentageAchieved.ToString( "0.##" ),
                        CssClass = GetProgressCssClass( percentageAchieved ),
                        ProgressBarWidth = progressBarWidth
                    };
                } ).ToList();

                this.GroupIndividualFundraisingGoal = groupMemberList.Sum( a => decimal.Parse( a.FundraisingGoal ) );
                this.GroupContributionTotal = groupMemberList.Sum( a => decimal.Parse( a.ContributionTotal ) );
                this.PercentComplete = decimal.Round( this.GroupIndividualFundraisingGoal == 0 ? 100 : this.GroupContributionTotal / ( this.GroupIndividualFundraisingGoal * 0.01M ), 2 );
                this.ProgressCssClass = GetProgressCssClass( this.PercentComplete );

                rptFundingProgress.DataSource = groupMemberList;
                rptFundingProgress.DataBind();
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView( hfGroupId.Value.AsIntegerOrNull(), hfGroupMemberId.Value.AsIntegerOrNull() );
        }

        #endregion

        #region Methods

        private string GetProgressCssClass( decimal percentage )
        {
            var cssClass = "warning";

            if ( percentage >= 100 )
            {
                cssClass = "success";
            }
            else if ( percentage > 40 && percentage < 100 )
            {
                cssClass = "info";
            }

            return cssClass;
        }

        #endregion
    }
}