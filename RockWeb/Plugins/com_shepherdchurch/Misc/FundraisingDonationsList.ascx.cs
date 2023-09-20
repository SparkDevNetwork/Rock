using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace Plugins.com_shepherdchurch.Misc
{
    [DisplayName( "Fundraising Donations List" )]
    [Category( "Shepherd Church > Misc" )]
    [Description( "Lists donations in a grid for the current fundraising opportunity or participant." )]

    [BooleanField( "Show Amount", "Determines if the Amount column should be displayed in the Donation List.", true, order: 1 )]
    [BooleanField( "Show Donor Person Link", "Determines if the Donor Person Link should be displayed in the Donation List.", true, "Donor", order: 2 )]
    [TextField( "Donor Person Link", "The base route that should be used with the the Donor Person link.", false, "/Person/", "Donor", order: 3 )]
    [BooleanField( "Show Donor Address", "Determines if the Donor's Address should be displayed in the Donation List.", true, "Donor", order: 4 )]
    [BooleanField( "Show Donor Email", "Determines if the Donor's Email should be displayed in the Donation List.", true, "Donor", order: 5 )]
    [BooleanField( "Show Participant Column", "Determines if the Participant column should be displayed in the Donation List.", true, "Participant", order: 6 )]
    [TextField( "Participant Group Member Link", "The base route that should be used prior to the GroupMemberId. Note: including {GroupId} will include the current GroupId in the url.", false, "/GroupMember/", "Participant", order: 7 )]
    [BooleanField( "Show Participant Group Member Link", "Determines if the Participant Group Member Link should be displayed in the Donation List.", true, "Participant", order: 8 )]
    [TextField( "Participant Person Link", "The base route that should be used prior to the PersonId.", false, "/Person/", "Participant", order: 9 )]
    [BooleanField( "Show Participant Person Link", "Determines if the Participant Person Link should be displayed in the Donation List.", true, "Participant", order: 10 )]
    [BooleanField( "Show Communicate", "Show Communicate button in grid footer?", true, "Grid Actions", order: 1 )]
    [BooleanField( "Show Merge Person", "Show Merge Person button in grid footer?", true, "Grid Actions", order: 2 )]
    [BooleanField( "Show Bulk Update", "Show Bulk Update button in grid footer?", true, "Grid Actions", order: 3 )]
    [BooleanField( "Show Excel Export", "Show Export to Excel button in grid footer?", true, "Grid Actions", order: 4 )]
    [BooleanField( "Show Merge Template", "Show Export to Merge Template button in grid footer?", true, "Grid Actions", order: 5 )]

    [ContextAware]
    public partial class FundraisingDonationsList : RockBlock
    {
        public string ParticipantPersonLink = "/Person/";
        public string ParticipantGroupMemberLink = "/GroupMember/";

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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetails();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Show the block content.
        /// </summary>
        protected void ShowDetails()
        {
            ParticipantGroupMemberLink = GetAttributeValue( "ParticipantGroupMemberLink" );
            ParticipantPersonLink = GetAttributeValue( "ParticipantPersonLink" );
            
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
            if ( group != null && group.GroupTypeId == GroupTypeCache.Get( Rock.SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY.AsGuid() ).Id )
            {
                if ( !string.IsNullOrWhiteSpace( ParticipantGroupMemberLink ) )
                {
                    ParticipantGroupMemberLink = ParticipantGroupMemberLink.Replace( "{GroupId}", group.Id.ToString() );
                }
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
            Dictionary<int, GroupMember> groupMembers;

            //
            // Get the donations for the entire opportunity group or for just the
            // one individual being viewed.
            //
            if ( ContextEntity<Group>() != null )
            {
                var group = ContextEntity<Group>();

                groupMembers = groupMemberService.Queryable()
                    .Where( m => m.GroupId == group.Id )
                    .ToDictionary( m => m.Id );

                gDonations.Columns.OfType<RockTemplateField>().Where( c => c.HeaderText == "Participant" ).ToList().ForEach( c => c.Visible = true );
                gDonations.Columns.OfType<DateField>().Where( c => c.HeaderText == "Date" ).ToList().ForEach( c => c.Visible = false );
            }
            else
            {
                var groupMember = ContextEntity<GroupMember>();

                groupMembers = new Dictionary<int, GroupMember> { { groupMember.Id, groupMember } };

                gDonations.Columns.OfType<RockTemplateField>().Where( c => c.HeaderText == "Participant" ).ToList().ForEach( c => c.Visible = false );
                gDonations.Columns.OfType<DateField>().Where( c => c.HeaderText == "Date" ).ToList().ForEach( c => c.Visible = true );
            }

            var showDonorPersonLink = GetAttributeValue( "ShowDonorPersonLink" ).AsBoolean();
            var donorPersonLink = GetAttributeValue( "DonorPersonLink" );
            var showParticipantPersonLink = GetAttributeValue( "ShowParticipantPersonLink" ).AsBoolean();
            var showParticipantGroupMemberLink = GetAttributeValue( "ShowParticipantGroupMemberLink" ).AsBoolean();

            //
            // Get the list of donation entries for the grid that match the list of members.
            //
            var groupMemberIds = groupMembers.Keys.ToList();
            var donations = financialTransactionDetailService.Queryable()
                .Where( d => d.EntityTypeId == entityTypeIdGroupMember && groupMemberIds.Contains( d.EntityId.Value ) )
                .ToList()
                .Select( d => new
                {
                    DonorId = d.Transaction.AuthorizedPersonAlias.PersonId,
                    Donor = d.Transaction.AuthorizedPersonAlias.Person,
                    DonorName = ( ( isExporting || !showDonorPersonLink ) ? d.Transaction.AuthorizedPersonAlias.Person.FullName : string.Format( "<a href=\"{0}{1}\">{2}</a>", donorPersonLink, d.Transaction.AuthorizedPersonAlias.Person.Id, d.Transaction.AuthorizedPersonAlias.Person.FullName ) ),
                    Email = d.Transaction.AuthorizedPersonAlias.Person.Email,
                    Participant = groupMembers[d.EntityId.Value],
                    ParticipantName = ( isExporting ? groupMembers[d.EntityId.Value].Person.FullName : 
                    ( ( showParticipantPersonLink || showParticipantGroupMemberLink ) ?
                        ( showParticipantPersonLink ? string.Format( "<a href=\"{0}{1}\" class=\"pull-right margin-l-sm btn btn-sm btn-default\"><i class=\"fa fa-user\"></i></a>", ParticipantPersonLink, groupMembers[d.EntityId.Value].PersonId ) : string.Empty ) +
                        ( showParticipantGroupMemberLink ? string.Format( "<a href=\"{0}{1}\">{2}</a>", ParticipantGroupMemberLink, groupMembers[d.EntityId.Value].Id, groupMembers[d.EntityId.Value].Person.FullName ) : string.Empty )
                        : groupMembers[d.EntityId.Value].Person.FullName )
                    ),
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

            if ( !GetAttributeValue( "ShowDonorAddress" ).AsBoolean() )
            {
                gDonations.Columns[2].Visible = false;
            }

            if ( !GetAttributeValue( "ShowDonorEmail" ).AsBoolean() )
            {
                gDonations.Columns[3].Visible = false;
            }

            if ( !GetAttributeValue( "ShowParticipantColumn" ).AsBoolean() )
            {
                gDonations.Columns[4].Visible = false;
            }

            gDonations.Columns[6].Visible = GetAttributeValue( "ShowAmount" ).AsBoolean();

            gDonations.Actions.ShowCommunicate = GetAttributeValue( "ShowCommunicate" ).AsBoolean();
            gDonations.Actions.ShowMergePerson = GetAttributeValue( "ShowMergePerson" ).AsBoolean();
            gDonations.Actions.ShowBulkUpdate = GetAttributeValue( "ShowBulkUpdate" ).AsBoolean();
            gDonations.Actions.ShowExcelExport = GetAttributeValue( "ShowExcelExport" ).AsBoolean();
            gDonations.Actions.ShowMergeTemplate = GetAttributeValue( "ShowMergeTemplate" ).AsBoolean();

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
        /// Handles the GridRebind event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridRebindEventArgs"/> instance containing the event data.</param>
        private void gDonations_GridRebind( object sender, Rock.Web.UI.Controls.GridRebindEventArgs e )
        {
            BindGrid( e.IsExporting );
        }

        #endregion
    }
}