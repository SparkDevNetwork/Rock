using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Fundraising
{
    [DisplayName( "Fundraising Opportunity Participant" )]
    [Category( "Fundraising" )]
    [Description( "Public facing block that shows a fundraising opportunity participant" )]

    [CodeEditorField( "Profile Lava Template", "Lava template for what to display at the top of the main panel. Usually used to display information about the participant such as photo, name, etc.", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
    @"
{{ GroupMember.Person.FullName }}
<h1>{{ Group | Attribute:'OpportunityTitle' }}</h1>
{{ Group | Attribute:'OpportunitySummary' }}
", order: 1 )]

    [CodeEditorField( "Updates Lava Template", "Lava template for the Updates (Content Channel Items)", CodeEditorMode.Lava, CodeEditorTheme.Rock, 100, false,
    @"
{% for item in ContentChannelItems %}
<article class='margin-b-lg'>
  <h3>{{ item.Title }}</h3>
  {{ item | Attribute:'Image' }}
  <div>
    {{ item.Content }}
  </div>

</article>
{% endfor %}", order: 3 )]
    [NoteTypeField( "Note Type", "Note Type to use for comments", false, "Rock.Model.Group", defaultValue: "9BB1A7B6-0E51-4E0E-BFC0-1E42F4F2DA95", order: 4 )]
    [LinkedPage( "Donation Page", "The page where a person can donate to the fundraising opportunity", required: false, order: 5 )]
    [LinkedPage( "Main Page", "The main page for the fundraising opportunity", required: false, order: 6 )]

    [BooleanField( "Enable Debug", "Show Lava Debug Help", false, order: 8 )]
    public partial class FundraisingParticipant : RockBlock
    {
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
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                int? groupId = this.PageParameter( "GroupId" ).AsIntegerOrNull();

                // DEBUG
                groupId = 292389;

                if ( groupId.HasValue )
                {
                    ShowView( groupId.Value );
                }
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            ShowView( hfGroupId.Value.AsInteger() );
        }

        /// <summary>
        /// Handles the Click event of the btnEditPreferences control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnEditPreferences_Click( object sender, EventArgs e )
        {
            // TODO
        }

        /// <summary>
        /// Handles the Click event of the btnMainPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnMainPage_Click( object sender, EventArgs e )
        {
            // TODO
        }

        /// <summary>
        /// Handles the Click event of the btnUpdatesTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnUpdatesTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Updates" );
        }

        /// <summary>
        /// Handles the Click event of the btnContributionsTab control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnContributionsTab_Click( object sender, EventArgs e )
        {
            SetActiveTab( "Contributions" );
        }

        #endregion

        #region Methods

        /// <summary>
        /// Shows the view.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        protected void ShowView( int groupId )
        {
            hfGroupId.Value = groupId.ToString();
            var rockContext = new RockContext();

            var group = new GroupService( rockContext ).Get( groupId );
            if ( group == null )
            {
                return;
            }

            group.LoadAttributes( rockContext );
            var mergeFields = LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson, new CommonMergeFieldsOptions { GetLegacyGlobalMergeFields = false } );
            mergeFields.Add( "Group", group );
            var groupTypeRolePartipantGuid = "F82DF077-9664-4DA8-A3D9-7379B690124D".AsGuid();

            var groupMember = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == groupId && a.GroupRole.Guid == groupTypeRolePartipantGuid && a.PersonId == this.CurrentPersonId ).FirstOrDefault();
            mergeFields.Add( "GroupMember", groupMember );

            var photoGuid = group.GetAttributeValue( "OpportunityPhoto" );
            imgPhoto.ImageUrl = string.Format( "~/GetImage.ashx?Guid={0}", photoGuid );

            string profileLavaTemplate = this.GetAttributeValue( "ProfileLavaTemplate" );
            lMainTopContentHtml.Text = profileLavaTemplate.ResolveMergeFields( mergeFields );

            if ( this.GetAttributeValue( "EnableDebug" ).AsBoolean() )
            {
                lLavaHelp.Text = mergeFields.lavaDebugInfo( rockContext );
            }
        }

        /// <summary>
        /// Sets the active tab.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        protected void SetActiveTab( string tabName )
        {
            hfActiveTab.Value = tabName;
            pnlUpdates.Visible = tabName == "Updates";
            pnlContributions.Visible = tabName == "Contributions";
            btnUpdatesTab.CssClass = tabName == "Updates" ? "btn btn-primary" : "btn btn-default";
            btnContributionsTab.CssClass = tabName == "Contributions" ? "btn btn-primary" : "btn btn-default";
        }

        #endregion
    }
}