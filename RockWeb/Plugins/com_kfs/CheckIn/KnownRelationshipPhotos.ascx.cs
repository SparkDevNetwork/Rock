using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.com_kfs.CheckIn
{
    /// <summary>
    /// Block that adds a content channel item for person paging.
    /// </summary>
    [DisplayName( "Known Releationship Photos" )]
    [Category( "Check-in > Manager" )]
    [Description( "Block that displays a grid of person photos based on known relationships." )]

    [GroupRoleField( "", "Group Type/Role Filter", "The Group Type and role to display other members from.", false, "" )]
    [BooleanField( "Can Check In Only", "Should only Known Relationships with Check In ability be displayed", true, "" )]
    [IntegerField( "Max Relationships To Display", "", false, 50, "" )]
    [BooleanField( "Show Role", "Should the member's role be displayed with their name" )]
    [LinkedPage( "Person Detail Page", "Page to display person details when clicked.", true )]

    public partial class KnownRelationshipPhotos : RockBlock
    {
        #region Fields

        /// <summary>
        /// Gets or sets a value indicating whether [show role].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show role]; otherwise, <c>false</c>.
        /// </value>
        protected bool ShowRole { get; set; }

        /// <summary>
        /// Gets or sets the owner role unique identifier.
        /// </summary>
        /// <value>
        /// The owner role unique identifier.
        /// </value>
        protected Guid ownerRoleGuid { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is inverse relationships owner.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is inverse relationships owner; otherwise, <c>false</c>.
        /// </value>
        protected bool IsInverseRelationshipsOwner { get; set; }

        int? personId = null;
        Guid? personGuid = null;
        Person person = null;
        Boolean canCheckInOnly = false;

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

            canCheckInOnly = GetAttributeValue( "CanCheckInOnly" ).AsBoolean();
            ownerRoleGuid = GetAttributeValue( "GroupType/RoleFilter" ).AsGuidOrNull() ?? Guid.Empty;

            // The 'owner' of the group is determined by built-in KnownRelationshipsOwner role or the role that is marked as IsLeader for the group
            var ownerRole = new GroupTypeRoleService( new RockContext() ).Get( ownerRoleGuid );
            if ( ownerRole != null )
            {
                ownerRole.LoadAttributes();
                IsInverseRelationshipsOwner = ownerRole.Attributes.ContainsKey( "InverseRelationship" )
                    && ( ownerRole.Guid.Equals( Rock.SystemGuid.GroupRole.GROUPROLE_KNOWN_RELATIONSHIPS_OWNER.AsGuid() ) || ownerRole.IsLeader );
            }
            else
            {
                IsInverseRelationshipsOwner = false;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            getPerson();
        }
        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            getPerson();
        }

        protected void getPerson()
        {
            personId = PageParameter( "PersonId" ).AsIntegerOrNull();
            personGuid = PageParameter( "Person" ).AsGuidOrNull();

            using ( var rockContext = new RockContext() )
            {
                var personService = new PersonService( rockContext );

                person = personService.Queryable( "RecordTypeValue", true, true )
                    .FirstOrDefault( a => a.Guid == personGuid || a.Id == personId );
            }
            if ( person != null )
            {
                BindData();
            }
        }

        /// <summary>
        /// Binds the data.
        /// </summary>
        private void BindData()
        {
            ShowRole = GetAttributeValue( "ShowRole" ).AsBoolean();

            if ( person != null && person.Id > 0 )
            {
                if ( ownerRoleGuid != Guid.Empty )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var memberService = new GroupMemberService( rockContext );
                        var group = memberService.Queryable( true )
                            .Where( m =>
                                m.PersonId == person.Id &&
                                m.GroupRole.Guid == ownerRoleGuid )
                            .Select( m => m.Group )
                            .FirstOrDefault();

                        if ( group != null )
                        {
                            lGroupName.Text = group.Name.Pluralize();
                            lGroupTypeIcon.Text = string.Format( "<i class='{0}'></i>", group.GroupType.IconCssClass );
                            lGroupTypeIcon.Visible = !string.IsNullOrWhiteSpace( group.GroupType.IconCssClass );

                            if ( group.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                            {
                                int? maxRelationshipsToDisplay = this.GetAttributeValue( "MaxRelationshipsToDisplay" ).AsIntegerOrNull();

                                var roles = new List<int>();
                                if ( canCheckInOnly )
                                {
                                    foreach ( var role in new GroupTypeRoleService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( r => r.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ) ) )
                                    {
                                        role.LoadAttributes( rockContext );
                                        if ( role.Attributes.ContainsKey( "CanCheckin" ) )
                                        {
                                            bool canCheckIn = false;
                                            if ( bool.TryParse( role.GetAttributeValue( "CanCheckin" ), out canCheckIn ) && canCheckIn )
                                            {
                                                roles.Add( role.Id );
                                            }
                                        }

                                        if ( role.Attributes.ContainsKey( "InverseRelationship" ) )
                                        {
                                            var inverseRoleGuid = role.GetAttributeValue( "InverseRelationship" ).AsGuidOrNull();
                                            if ( inverseRoleGuid != null )
                                            {
                                                var groupTypeRole = new GroupTypeRoleService( rockContext )
                                                    .Queryable().AsNoTracking()
                                                    .FirstOrDefault( r => r.Guid.Equals( ( Guid ) inverseRoleGuid ) );
                                                if ( groupTypeRole != null )
                                                {
                                                    groupTypeRole.LoadAttributes( rockContext );
                                                    if ( groupTypeRole.Attributes.ContainsKey( "CanCheckin" ) )
                                                    {
                                                        bool canCheckIn = false;
                                                        if ( bool.TryParse( groupTypeRole.GetAttributeValue( "CanCheckin" ), out canCheckIn ) && canCheckIn )
                                                        {
                                                            roles.Add( role.Id );
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    foreach ( var role in new GroupTypeRoleService( rockContext )
                                        .Queryable().AsNoTracking()
                                        .Where( r => r.GroupType.Guid.Equals( new Guid( Rock.SystemGuid.GroupType.GROUPTYPE_KNOWN_RELATIONSHIPS ) ) ) )
                                    {
                                        roles.Add( role.Id );
                                    }
                                }

                                IQueryable<GroupMember> qryGroupMembers = new GroupMemberService( rockContext ).GetByGroupId( group.Id, true )
                                    .Where( m => m.PersonId != person.Id )
                                    .Where( m => roles.Contains( m.GroupRoleId ) )
                                    .OrderBy( m => m.Person.LastName )
                                    .ThenBy( m => m.Person.FirstName );

                                if ( maxRelationshipsToDisplay.HasValue )
                                {
                                    qryGroupMembers = qryGroupMembers.Take( maxRelationshipsToDisplay.Value );
                                }

                                rGroupMembers.ItemDataBound += rptrMembers_ItemDataBound;
                                rGroupMembers.DataSource = qryGroupMembers.ToList();
                                rGroupMembers.DataBind();
                            }
                            else
                            {
                                lAccessWarning.Text = string.Format( "<div class='alert alert-info'>You do not have security rights to view {0}.", group.Name.Pluralize() );
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the ItemDataBound event of the rptrMembers control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterItemEventArgs"/> instance containing the event data.</param>
        protected void rptrMembers_ItemDataBound( object sender, RepeaterItemEventArgs e )
        {
            if ( e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem )
            {
                var groupMember = e.Item.DataItem as GroupMember;
                if ( groupMember != null && groupMember.Person != null )
                {
                    Person gm = groupMember.Person;

                    HtmlControl imgPersonImage = e.Item.FindControl( "imgPersonImage" ) as HtmlControl;
                    if ( imgPersonImage != null )
                    {
                        imgPersonImage.Attributes.Add( "src", Person.GetPersonPhotoUrl( gm ) );
                    }

                    HtmlControl personLink = e.Item.FindControl( "personLink" ) as HtmlControl;
                    if ( personLink != null )
                    {
                        string linkedPage = GetAttributeValue( "PersonDetailPage" );
                        if ( !string.IsNullOrWhiteSpace( linkedPage ) )
                        {
                            var pageReference = new Rock.Web.PageReference();
                            if ( personId != null )
                            {
                                var pageParams = new Dictionary<string, string>();
                                pageParams.Add( "PersonId", gm.Id.ToString() );

                                pageReference = new Rock.Web.PageReference( linkedPage, pageParams );
                            }
                            else if ( personGuid != null )
                            {
                                var pageParams = new Dictionary<string, string>();
                                pageParams.Add( "Person", gm.Guid.ToString() );

                                pageReference = new Rock.Web.PageReference( linkedPage, pageParams );
                            }
                            personLink.Attributes.Add( "href", pageReference.BuildUrl() );
                        }
                    }
                }
            }
        }

        #endregion
    }
}