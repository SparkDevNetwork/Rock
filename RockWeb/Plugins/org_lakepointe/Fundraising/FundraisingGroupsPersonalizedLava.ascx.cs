
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;
using Rock.Web.UI;
using Rock.Lava;
using Rock.Security;

namespace RockWeb.Plugins.org_lakepointe.Fundraising
{
    [DisplayName( "Personalized Fundraising List" )]
    [Category( "Lake Poine > Fundraising" )]
    [Description("Lists Fundraising Groups that the current person is an active member in.")]

    [DefinedValueField( "53C8FFF6-3022-4A2D-9BAE-FD3435BEA43D", "Fundraising Opportunity Types", "Select which opportunity types are shown, or leave blank to show all", false, true, order: 1 )]
    [LinkedPage( "Fundrasing Participant Page", required: true, order: 2 )]
    [IntegerField( "Max Results", "The maximum number of results to display.", false, 10, order: 3 )]
    [SlidingDateRangeField("Date Range", "Date range to limit by.", false, "", enabledSlidingDateRangeTypes: "Previous, Last, Current, Next, Upcoming, DateRange", order: 4)]
    [CodeEditorField( "Lava Template", "The lava template to use for the results", CodeEditorMode.Lava, CodeEditorTheme.Rock, defaultValue:
        @"{% include '~~/Assets/Lava/FundraisingListParticipant.lava' %}", order: 5 )]
    [BooleanField("Enable Debug", "Shows the fields available to merge in lava.", false, "", 6 )]
    
    public partial class FundraisingGroupsPersonalizedLava : RockBlock
    {


        #region Base Control Methods
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            //this event gets fired after the block settings are updated
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }


        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadContent();
            }
        }
        #endregion

        #region Events
        private void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContent();
        }
        #endregion

        #region Methods
        private void LoadContent()
        {
            var rockContext = new RockContext();
            var groupTypeIdFundraising = GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id;
            var fundraisingGroupTypeIdList = new GroupTypeService( rockContext ).Queryable().Where( a => a.Id == groupTypeIdFundraising || a.InheritedGroupTypeId == groupTypeIdFundraising ).Select( a => a.Id ).ToList();

            var fundraisingGroupMembers = new GroupMemberService( rockContext )
                .Queryable("Group,GroupRole")
                .Where( m => m.PersonId == CurrentPersonId )
                .Where( m => m.GroupMemberStatus == GroupMemberStatus.Active )
                .Where( m => m.Group.IsActive == true )
                .Where( m => fundraisingGroupTypeIdList.Contains( m.Group.GroupTypeId ) )
                .ToList();

            var summaryList = new List<FundraisingParticipantSummary>();
            foreach ( var gm in fundraisingGroupMembers )
            {
                var summaryItem = new FundraisingParticipantSummary();
                gm.LoadAttributes();
                gm.Group.LoadAttributes();

                summaryItem.Group = gm.Group;
                summaryItem.GroupMember = gm;
                summaryItem.OpportunityStartDate = DateRangePicker.CalculateDateRangeFromDelimitedValues( gm.Group.GetAttributeValue( "OpportunityDateRange" ) ).Start;
                summaryItem.OpportunityEndDate = DateRangePicker.CalculateDateRangeFromDelimitedValues( gm.Group.GetAttributeValue( "OpportunityDateRange" ) ).End;
                summaryItem.Role = gm.GroupRole.Name;

                var personalGoal = gm.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimalOrNull();

                if ( personalGoal == null )
                {
                    summaryItem.FundraisingGoal = gm.Group.GetAttributeValue( "IndividualFundraisingGoal" ).AsDecimal();
                }
                else
                {
                    summaryItem.FundraisingGoal = (Decimal)personalGoal;
                }

                var entityTypeIdGroupMember = EntityTypeCache.GetId<Rock.Model.GroupMember>();

                summaryItem.AmountRaised = new FinancialTransactionDetailService( rockContext ).Queryable()
                            .Where( d => d.EntityTypeId == entityTypeIdGroupMember
                                    && d.EntityId == gm.Id )
                            .Sum( a => (decimal?)a.Amount ) ?? 0.00M;

                summaryList.Add( summaryItem );
            }


            var opportunityTypes = this.GetAttributeValue( "FundraisingOpportunityTypes" ).SplitDelimitedValues().AsGuidList();
            if ( opportunityTypes.Any() )
            {
                summaryList = summaryList.Where( a => opportunityTypes.Contains( a.Group.GetAttributeValue( "OpportunityType" ).AsGuid() ) ).ToList();
            }

            var dateRange = SlidingDateRangePicker.CalculateDateRangeFromDelimitedValues( GetAttributeValue( "DateRange" ) ?? "-1||" );

            if ( dateRange.Start.HasValue )
            {
                summaryList = summaryList.Where( a => a.OpportunityStartDate >= dateRange.Start ).ToList();
            }

            if ( dateRange.End.HasValue )
            {
                summaryList = summaryList.Where( a => a.OpportunityStartDate <= dateRange.End ).ToList();
            }

            summaryList = summaryList
                .OrderBy( a => a.OpportunityStartDate )
                .TakeLast( this.GetAttributeValue( "MaxResults" ).AsInteger() )
                .ToList();

            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            mergeFields.Add( "MissionGroups", summaryList );

            Dictionary<string, object> linkedPages = new Dictionary<string, object>();
            linkedPages.Add( "ParticipantPage", LinkedPageRoute( "FundrasingParticipantPage" ) );
            mergeFields.Add( "LinkedPages", linkedPages );

            string template = GetAttributeValue( "LavaTemplate" );

            // show debug info
            bool enableDebug = GetAttributeValue( "EnableDebug" ).AsBoolean();
            if ( enableDebug && IsUserAuthorized( Authorization.EDIT ) )
            {
                lDebug.Visible = true;
                lDebug.Text = mergeFields.lavaDebugInfo();
            }

            lViewHtml.Text = template.ResolveMergeFields( mergeFields );

        }

        #endregion  
    }

    [DotLiquid.LiquidType("Group", "GroupMember", "OpportunityStartDate", "OpportunityEndDate", "Role", "FundraisingGoal", "AmountRaised")]
    public class FundraisingParticipantSummary
    {
        public Group Group { get; set; }
        public GroupMember GroupMember { get; set; }
        public DateTime? OpportunityStartDate { get; set; }
        public DateTime? OpportunityEndDate { get; set; }
        public string Role { get; set; }
        public decimal FundraisingGoal { get; set; }
        public decimal AmountRaised { get; set; }
    }

 
}