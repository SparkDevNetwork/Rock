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
using Rock.Web.UI;

namespace RockWeb.Plugins.com_shepherdchurch.Misc
{
    [DisplayName( "Post User Notification" )]
    [Category( "Shepherd Church > Misc" )]
    [Description( "Post a notification to one or more users." )]

    public partial class PostUserNotification : RockBlock
    {
        #region Properties

        protected string PersonList
        {
            get
            {
                return ( string ) ViewState["PersonList"] ?? string.Empty;
            }
            set
            {
                ViewState["PersonList"] = value;
            }
        }

        #endregion

        #region Base Method Overrides

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

            dvDataView.EntityTypeId = Rock.Web.Cache.EntityTypeCache.GetId( "Rock.Model.Person" );
            ddlClassification.BindToEnum<NotificationClassification>( true );
        }

        /// <summary>
        /// Initialize basic information about the page structure and setup the default content.
        /// </summary>
        /// <param name="sender">Object that is generating this event.
        protected override void OnLoad( EventArgs e )
        {
            if ( !IsPostBack )
            {
                ddlClassification.SelectedValue = ( ( int ) NotificationClassification.Info ).ToString();
                UpdateCount();
            }
            else
            {
                BindPeople();
            }
        }

        #endregion

        #region Core Methods

        /// <summary>
        /// Display a message that shows the number of people who will receive the notification.
        /// </summary>
        /// <param name="count">The number of people to notify.</param>
        protected void UpdateCount()
        {
            int? count = null;

            if ( rblSource.SelectedValue == "Manual Selection" )
            {
                if ( !string.IsNullOrWhiteSpace( PersonList ) )
                {
                    var people = PersonList.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();
                    count = people.Count;
                }
            }
            else if ( rblSource.SelectedValue == "Group" )
            {
                int? groupId = gpGroup.SelectedValueAsId();

                var group = new GroupService( new RockContext() ).Get( groupId ?? 0 );
                if ( groupId.HasValue )
                {
                    count = new GroupMemberService( new RockContext() ).Queryable()
                        .Where( m => m.GroupId == groupId.Value && m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Count();
                }
            }
            else if ( rblSource.SelectedValue == "Data View" )
            {
                int? dataviewId = dvDataView.SelectedValueAsId();

                if ( dataviewId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var personService = new PersonService( rockContext );
                        var parameterExpression = personService.ParameterExpression;
                        var dv = new DataViewService( rockContext ).Get( dataviewId.Value );
                        List<string> errorMessages;
                        var whereExpression = dv.GetExpression( personService, parameterExpression, out errorMessages );

                        count = new PersonService( rockContext )
                            .Get( parameterExpression, whereExpression )
                            .Count();
                    }
                }
            }

            if ( count.HasValue )
            {
                nbCount.Text = string.Format( "Notification will be posted to {0} {1}.", count.Value, "person".PluralizeIf( count != 1 ) );
            }
            else
            {
                nbCount.Text = string.Empty;
            }

            lbSave.Enabled = ( count ?? 0 ) > 0;
        }

        /// <summary>
        /// Bind the repeater listing the people in manual selection.
        /// </summary>
        protected void BindPeople()
        {
            var people = PersonList.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                .Select( p => new
                {
                    Id = p.Split( '^' )[0],
                    Name = p.Split( '^' )[1]
                } );

            rptPeople.DataSource = people;
            rptPeople.DataBind();
        }

        #endregion

        #region Event Handlers

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            PersonList = string.Empty;

            rblSource.SelectedIndex = 0;

            pnlManualSelection.Visible = true;
            pnlGroup.Visible = false;
            pnlDataView.Visible = false;

            UpdateCount();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblSource control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblSource_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblSource.SelectedValue == "Manual Selection" )
            {
                pnlManualSelection.Visible = true;
                pnlGroup.Visible = false;
                pnlDataView.Visible = false;
            }
            else if ( rblSource.SelectedValue == "Group" )
            {
                pnlManualSelection.Visible = false;
                pnlGroup.Visible = true;
                pnlDataView.Visible = false;
            }
            else if ( rblSource.SelectedValue == "Data View" )
            {
                pnlManualSelection.Visible = false;
                pnlGroup.Visible = false;
                pnlDataView.Visible = true;
            }

            UpdateCount();
        }

        /// <summary>
        /// Handles the SelectPerson event of the ppPerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ppPerson_SelectPerson( object sender, EventArgs e )
        {
            int? personAliasId = ppPerson.PersonAliasId;

            if ( personAliasId.HasValue )
            {
                var people = PersonList.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

                people.Add( string.Format( "{0}^{1}", personAliasId.Value, new PersonAliasService( new RockContext() ).Get( personAliasId.Value ).Person.FullName ) );

                PersonList = string.Join( "|", people );

                BindPeople();
                ppPerson.SetValue( null );

                UpdateCount();
            }
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the dvDataView control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void dvDataView_SelectedIndexChanged( object sender, EventArgs e )
        {
            UpdateCount();
        }

        /// <summary>
        /// Handles the SelectItem event of the gpGroup control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gpGroup_SelectItem( object sender, EventArgs e )
        {
            UpdateCount();
        }

        /// <summary>
        /// Handles the Command event of the lbRemovePerson control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="CommandEventArgs"/> instance containing the event data.</param>
        protected void lbRemovePerson_Command( object sender, CommandEventArgs e )
        {
            var people = PersonList.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ).ToList();

            int index = people.FindIndex( p => p.Split( '^' )[0] == e.CommandArgument.ToString() );
            people.RemoveAt( index );

            PersonList = string.Join( "|", people );

            BindPeople();
            UpdateCount();
        }

        /// <summary>
        /// Handles the Click event of the lbSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSave_Click( object sender, EventArgs e )
        {
            List<int> personAliasIds = null;

            if ( rblSource.SelectedValue == "Manual Selection" )
            {
                personAliasIds = PersonList.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries )
                    .Select( p => p.Split( '^' )[0].AsInteger() )
                    .ToList();
            }
            else if ( rblSource.SelectedValue == "Group" )
            {
                int? groupId = gpGroup.SelectedValueAsId();

                var group = new GroupService( new RockContext() ).Get( groupId ?? 0 );
                if ( groupId.HasValue )
                {
                    personAliasIds = new GroupMemberService( new RockContext() ).Queryable()
                        .Where( m => m.GroupId == groupId.Value && m.GroupMemberStatus == GroupMemberStatus.Active )
                        .Select( m => m.Person )
                        .ToList()
                        .Select( p => p.PrimaryAliasId.Value )
                        .ToList();
                }
            }
            else if ( rblSource.SelectedValue == "Data View" )
            {
                int? dataviewId = dvDataView.SelectedValueAsId();

                if ( dataviewId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        var personService = new PersonService( rockContext );
                        var parameterExpression = personService.ParameterExpression;
                        var dv = new DataViewService( rockContext ).Get( dataviewId.Value );
                        List<string> errorMessages;
                        var whereExpression = dv.GetExpression( personService, parameterExpression, out errorMessages );

                        personAliasIds = new PersonService( rockContext )
                            .Get( parameterExpression, whereExpression )
                            .ToList()
                            .Select( p => p.PrimaryAliasId.Value )
                            .ToList();
                    }
                }
            }

            using ( var rockContext = new RockContext() )
            {
                var notificationService = new NotificationService( rockContext );
                var notificationRecipientService = new NotificationRecipientService( rockContext );

                var notification = new Notification();

                notification.Title = tbTitle.Text;
                notification.Message = ceMessage.Text;
                notification.SentDateTime = RockDateTime.Now;
                notification.IconCssClass = tbIconCssClass.Text;
                notification.Classification = ddlClassification.SelectedValueAsEnum<NotificationClassification>();
                notificationService.Add( notification );

                foreach ( var aliasId in personAliasIds )
                {
                    notification.Recipients.Add( new NotificationRecipient { PersonAliasId = aliasId } );
                }

                rockContext.SaveChanges();
            }

            pnlPost.Visible = false;
            pnlResults.Visible = true;
        }

        /// <summary>
        /// Handles the Click event of the lbDone control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbDone_Click( object sender, EventArgs e )
        {
            NavigateToCurrentPage();
        }

        #endregion
    }
}