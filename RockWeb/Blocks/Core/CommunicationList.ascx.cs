//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
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

namespace RockWeb.Blocks.Core
{
    [AdditionalActions( new string[] { "Approve" } )]
    [LinkedPage("Detail Page")]
    public partial class CommunicationList : Rock.Web.UI.RockBlock
    {
        private bool canApprove = false;

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            BindFilter();
            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            gCommunication.DataKeyNames = new string[] { "Id" };
            gCommunication.Actions.ShowAdd = false;
            gCommunication.GridRebind += gCommunication_GridRebind;

            // The created by column/filter should only be displayed if user is allowed to approve
            canApprove = this.IsUserAuthorized( "Approve" );
            ppSender.Visible = canApprove;
            gCommunication.Columns[2].Visible = canApprove;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "Subject", tbSubject.Text );
            rFilter.SaveUserPreference( "Channel", cpChannel.SelectedValue );
            rFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            if ( canApprove )
            {
                rFilter.SaveUserPreference( "Created By", ppSender.PersonId.ToString() );
            }
            rFilter.SaveUserPreference( "Content", tbContent.Text );

            BindGrid();
        }

        /// <summary>
        /// Handles the filter display for each saved user value
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void rFilter_DisplayFilterValue( object sender, Rock.Web.UI.Controls.GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Channel":

                    int entityTypeId = 0;
                    if ( int.TryParse( e.Value, out entityTypeId ) )
                    {
                        var entity = EntityTypeCache.Read( entityTypeId );
                        if ( entity != null )
                        {
                            e.Value = entity.FriendlyName;
                        }
                    }

                    break;

                case "Status":

                    if ( !string.IsNullOrWhiteSpace( e.Value ) )
                    {
                        e.Value = ( (CommunicationStatus)System.Enum.Parse( typeof( CommunicationStatus ), e.Value ) ).ConvertToString();
                    }

                    break;

                case "Created By":

                    int personId = 0;
                    if ( int.TryParse( e.Value, out personId ) && personId != 0 )
                    {
                        var personService = new PersonService();
                        var person = personService.Get( personId );
                        if ( person != null )
                        {
                            e.Value = person.FullName;
                        }
                    }

                    break;
            }
        }

        /// <summary>
        /// Handles the RowDataBound event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs" /> instance containing the event data.</param>
        protected void gCommunication_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( e.Row.RowType == DataControlRowType.DataRow )
            {
                var communicationItem = e.Row.DataItem as CommunicationItem;
                if ( communicationItem != null )
                {
                    e.Row.FindControl( "bPending" ).Visible = communicationItem.PendingRecipients > 0;
                    e.Row.FindControl( "bSuccess" ).Visible = communicationItem.SuccessRecipients > 0;
                    e.Row.FindControl( "bWarning" ).Visible = communicationItem.CancelledRecipients > 0;
                    e.Row.FindControl( "bFailed" ).Visible = communicationItem.FailedRecipients > 0;

                    // Hide delete button if there are any successful recipients
                    e.Row.Cells[7].Controls[0].Visible = communicationItem.SuccessRecipients <= 0;
                }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gCommunication_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "CommunicationId", (int)e.RowKeyValue );
        }

        protected void gCommunication_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {

        }

        /// <summary>
        /// Handles the GridRebind event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gCommunication_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        private void BindFilter()
        {
            if ( cpChannel.Items[0].Value != string.Empty )
            {
                cpChannel.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            }

            ddlStatus.BindToEnum( typeof( CommunicationStatus ) );
            // Replace the Transient status with an emtyp value (need an empty one, and don't need transient value)
            ddlStatus.Items[0].Text = string.Empty;
            ddlStatus.Items[0].Value = string.Empty;

            if ( !Page.IsPostBack )
            {
                if ( !canApprove )
                {
                    rFilter.SaveUserPreference( "Created By", string.Empty );
                }

                tbSubject.Text = rFilter.GetUserPreference( "Subject" );
                cpChannel.SelectedValue = rFilter.GetUserPreference( "Channel" );
                ddlStatus.SelectedValue = rFilter.GetUserPreference( "Status" );

                int personId = 0;
                if ( int.TryParse( rFilter.GetUserPreference( "Created By" ), out personId ) )
                {
                    var personService = new PersonService();
                    var person = personService.Get( personId );
                    if ( person != null )
                    {
                        ppSender.SetValue( person );
                    }
                }

                tbContent.Text = rFilter.GetUserPreference( "Content" );
            }
        }

        private void BindGrid()
        {
            using ( new UnitOfWorkScope() )
            {
                var communications = new CommunicationService()
                    .Queryable()
                    .Where( c => c.Status != CommunicationStatus.Transient );

                string subject = rFilter.GetUserPreference( "Subject" );
                if ( !string.IsNullOrWhiteSpace( subject ) )
                {
                    communications = communications.Where( c => c.Subject.StartsWith( subject ) );
                }

                Guid entityTypeGuid = Guid.Empty;
                if ( Guid.TryParse( rFilter.GetUserPreference( "Channel" ), out entityTypeGuid ) )
                {
                    communications = communications.Where( c => c.ChannelEntityType != null && c.ChannelEntityType.Guid.Equals( entityTypeGuid ) );
                }

                string status = rFilter.GetUserPreference( "Status" );
                if ( !string.IsNullOrWhiteSpace( status ) )
                {
                    var communicationStatus = (CommunicationStatus)System.Enum.Parse( typeof( CommunicationStatus ), status );
                    communications = communications.Where( c => c.Status == communicationStatus );
                }

                if ( canApprove )
                {
                    int personId = 0;
                    if ( int.TryParse( rFilter.GetUserPreference( "Created By" ), out personId ) && personId != 0 )
                    {
                        communications = communications.Where( c => c.SenderPersonId.HasValue && c.SenderPersonId.Value == personId );
                    }
                }
                else
                {
                    communications = communications.Where( c => c.SenderPersonId.HasValue && c.SenderPersonId.Value == CurrentPersonId );
                }

                string content = rFilter.GetUserPreference( "Content" );
                if ( !string.IsNullOrWhiteSpace( content ) )
                {
                    communications = communications.Where( c => c.ChannelDataJson.Contains( content ) );
                }

                var recipients = new CommunicationRecipientService().Queryable();

                var sortProperty = gCommunication.SortProperty;

                var queryable = communications
                    .Join( recipients,
                        c => c.Id,
                        r => r.CommunicationId,
                        ( c, r ) => new { c, r } )
                    .GroupBy( cr => cr.c )
                    .Select( g => new CommunicationItem
                    {
                        Id = g.Key.Id,
                        Communication = g.Key,
                        Recipients = g.Count(),
                        PendingRecipients = g.Count( s => s.r.Status == CommunicationRecipientStatus.Pending ),
                        SuccessRecipients = g.Count( s => s.r.Status == CommunicationRecipientStatus.Success ),
                        FailedRecipients = g.Count( s => s.r.Status == CommunicationRecipientStatus.Failed ),
                        CancelledRecipients = g.Count( s => s.r.Status == CommunicationRecipientStatus.Cancelled )
                    } );

                if ( sortProperty != null )
                {
                    queryable = queryable.Sort( sortProperty );
                }
                else
                {
                    queryable = queryable.OrderByDescending( c => c.Communication.Id );
                }

                gCommunication.DataSource = queryable.ToList();
                gCommunication.DataBind();
            }

        }

        #endregion

        protected class CommunicationItem
        {
            public int Id { get; set; }
            public Rock.Model.Communication Communication { get; set; }
            public int Recipients { get; set; }
            public int PendingRecipients { get; set; }
            public int SuccessRecipients { get; set; }
            public int FailedRecipients { get; set; }
            public int CancelledRecipients { get; set; }
        }

    }
}