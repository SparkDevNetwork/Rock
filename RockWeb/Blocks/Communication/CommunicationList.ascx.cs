﻿// <copyright>
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
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Communication List" )]
    [Category( "Communication" )]
    [Description( "Lists the status of all previously created communications." )]

    [SecurityAction( Authorization.APPROVE, "The roles and/or users that have access to approve new communications." )]

    [LinkedPage( "Detail Page", order: 1 )]
    [LinkedPage( "Email Analytics", defaultValue: Rock.SystemGuid.Page.EMAIL_ANALYTICS, order: 2 )]
    public partial class CommunicationList : Rock.Web.UI.RockBlock, ICustomGridColumns
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

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            gCommunication.DataKeyNames = new string[] { "Id" };
            gCommunication.Actions.ShowAdd = false;
            gCommunication.GridRebind += gCommunication_GridRebind;

            // The created by filter and details column should only be displayed if user is allowed to approve
            canApprove = this.IsUserAuthorized( "Approve" );
            ppSender.Visible = canApprove;

            var detailsField = gCommunication.ColumnsOfType<RockLiteralField>().FirstOrDefault( a => a.HeaderText == "Details" );
            if ( detailsField != null )
            {
                detailsField.Visible = canApprove;
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                SetFilter();
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
            rFilter.SaveUserPreference( "Communication Type", ddlType.SelectedValue );
            rFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            int personId = ppSender.PersonId ?? 0;
            rFilter.SaveUserPreference( "Created By", canApprove ? personId.ToString() : "" );

            if ( !drpCreatedDates.LowerValue.HasValue && !drpCreatedDates.UpperValue.HasValue )
            {
                // If a date range has not been selected, default to last 7 days
                drpCreatedDates.LowerValue = RockDateTime.Today.AddDays( -7 );
            }
            rFilter.SaveUserPreference( "Created Date Range", drpCreatedDates.DelimitedValues );

            rFilter.SaveUserPreference( "Sent Date Range", drpSentDates.DelimitedValues );
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
                case "Subject":
                case "Content":
                    {
                        break;
                    }
                case "Communication Type":
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ( ( CommunicationType ) System.Enum.Parse( typeof( CommunicationType ), e.Value ) ).ConvertToString();
                        }

                        break;
                    }
                case "Status":
                    {
                        if ( !string.IsNullOrWhiteSpace( e.Value ) )
                        {
                            e.Value = ( ( CommunicationStatus ) System.Enum.Parse( typeof( CommunicationStatus ), e.Value ) ).ConvertToString();
                        }

                        break;
                    }
                case "Created By":
                    {
                        string personName = string.Empty;

                        if ( canApprove )
                        {
                            int? personId = e.Value.AsIntegerOrNull();
                            if ( personId.HasValue )
                            {
                                var personService = new PersonService( new RockContext() );
                                var person = personService.Get( personId.Value );
                                if ( person != null )
                                {
                                    personName = person.FullName;
                                }
                            }
                        }

                        e.Value = personName;

                        break;
                    }
                case "Created Date Range":
                case "Sent Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
                    }
                default:
                    {
                        e.Value = string.Empty;
                        break;
                    }
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
                    // Hide delete button if there are any successful recipients
                    e.Row.Cells.OfType<DataControlFieldCell>().First( a => a.ContainingField is DeleteField ).Controls[0].Visible = communicationItem.DeliveredRecipients <= 0;

                    Literal lDetails = e.Row.FindControl( "lDetails" ) as Literal;
                    if ( lDetails != null )
                    {
                        string rockUrlRoot = ResolveRockUrl( "/" );
                        var details = new StringBuilder();
                        if ( communicationItem.CreatedDateTime.HasValue && communicationItem.Sender != null )
                        {
                            details.AppendFormat( "Created on {1} by {0}<br/>", communicationItem.Sender.GetAnchorTag( rockUrlRoot ),
                                communicationItem.CreatedDateTime.Value.ToShortDateString() );
                        }
                        if ( communicationItem.ReviewedDateTime.HasValue && communicationItem.Reviewer != null )
                        {
                            details.AppendFormat( "Reviewed on {1} by {0}", communicationItem.Reviewer.GetAnchorTag( rockUrlRoot ),
                                communicationItem.ReviewedDateTime.Value.ToShortDateString() );
                        }
                        lDetails.Text = details.ToString();
                    }

                    Literal lEmailAnalyticsLink = e.Row.FindControl( "lEmailAnalyticsLink" ) as Literal;
                    if ( lEmailAnalyticsLink != null )
                    {
                        var qryParams = new Dictionary<string, string>();
                        qryParams.Add( "CommunicationId", communicationItem.Id.ToString() );
                        var emailAnalyticsUrl = new PageReference( this.GetAttributeValue( "EmailAnalytics" ), qryParams ).BuildUrl();
                        if ( !string.IsNullOrEmpty( emailAnalyticsUrl ) )
                        {
                            lEmailAnalyticsLink.Text = string.Format( "<div class='text-center'><a href='{0}' class='btn btn-default btn-sm' title='Email Analytics'><i class='fa fa-line-chart'></i></a></div>", emailAnalyticsUrl );
                        }
                    }
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
            NavigateToLinkedPage( "DetailPage", "CommunicationId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the Delete event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs"/> instance containing the event data.</param>
        protected void gCommunication_Delete( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rockContext = new RockContext();
            var communicationService = new CommunicationService( rockContext );
            var communication = communicationService.Get( e.RowKeyId );
            if ( communication != null )
            {
                string errorMessage;
                if ( !communicationService.CanDelete( communication, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                communicationService.Delete( communication );

                rockContext.SaveChanges();
            }

            BindGrid();
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

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void SetFilter()
        {
            tbSubject.Text = rFilter.GetUserPreference( "Subject" );

            ddlType.BindToEnum<CommunicationType>( true );
            ddlType.SetValue( rFilter.GetUserPreference( "Communication Type" ) );

            ddlStatus.BindToEnum<CommunicationStatus>();
            // Replace the Transient status with an empty value (need an empty one, and don't need transient value)
            ddlStatus.Items[0].Text = string.Empty;
            ddlStatus.Items[0].Value = string.Empty;
            ddlStatus.SelectedValue = rFilter.GetUserPreference( "Status" );

            int? personId = rFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();
            if ( !canApprove || !personId.HasValue )
            {
                personId = CurrentPersonId;
                rFilter.SaveUserPreference( "Created By", personId.Value.ToString() );
            }

            if ( personId.HasValue && personId.Value != 0 )
            {
                var personService = new PersonService( new RockContext() );
                var person = personService.Get( personId.Value );
                if ( person != null )
                {
                    ppSender.SetValue( person );
                }
            }

            drpCreatedDates.DelimitedValues = rFilter.GetUserPreference( "Created Date Range" );
            if ( !drpCreatedDates.LowerValue.HasValue && !drpCreatedDates.UpperValue.HasValue )
            {
                // If a date range has not been selected, default to last 7 days
                drpCreatedDates.LowerValue = RockDateTime.Today.AddDays( -7 );
                rFilter.SaveUserPreference( "Created Date Range", drpCreatedDates.DelimitedValues );
            }

            drpSentDates.DelimitedValues = rFilter.GetUserPreference( "Sent Date Range" );

            tbContent.Text = rFilter.GetUserPreference( "Content" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();

            var communications = new CommunicationService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( c => c.Status != CommunicationStatus.Transient );

            string subject = tbSubject.Text;
            if ( !string.IsNullOrWhiteSpace( subject ) )
            {
                communications = communications.Where( c => ( string.IsNullOrEmpty( c.Subject ) && c.Name.Contains( subject ) ) || c.Subject.Contains( subject ) );
            }

            var communicationType = ddlType.SelectedValueAsEnumOrNull<CommunicationType>();
            if ( communicationType != null )
            {
                communications = communications.Where( c => c.CommunicationType == communicationType );
            }

            string status = ddlStatus.SelectedValue;
            if ( !string.IsNullOrWhiteSpace( status ) )
            {
                var communicationStatus = ( CommunicationStatus ) System.Enum.Parse( typeof( CommunicationStatus ), status );
                communications = communications.Where( c => c.Status == communicationStatus );
            }

            if ( canApprove )
            {
                if ( ppSender.PersonId.HasValue )
                {
                    communications = communications
                        .Where( c =>
                            c.SenderPersonAlias != null &&
                            c.SenderPersonAlias.PersonId == ppSender.PersonId.Value );
                }
            }
            else
            {
                // If can't approve, only show current person's communications
                communications = communications
                    .Where( c =>
                        c.SenderPersonAlias != null &&
                        c.SenderPersonAlias.PersonId == CurrentPersonId );
            }

            if ( drpCreatedDates.LowerValue.HasValue )
            {
                communications = communications.Where( a => a.CreatedDateTime.HasValue && a.CreatedDateTime.Value >= drpCreatedDates.LowerValue.Value );
            }

            if ( drpCreatedDates.UpperValue.HasValue )
            {
                DateTime upperDate = drpCreatedDates.UpperValue.Value.Date.AddDays( 1 );
                communications = communications.Where( a => a.CreatedDateTime.HasValue && a.CreatedDateTime.Value < upperDate );
            }

            if ( drpSentDates.LowerValue.HasValue )
            {
                communications = communications.Where( a => ( a.SendDateTime ?? a.FutureSendDateTime ) >= drpSentDates.LowerValue.Value );
            }

            if ( drpSentDates.UpperValue.HasValue )
            {
                DateTime upperDate = drpSentDates.UpperValue.Value.Date.AddDays( 1 );
                communications = communications.Where( a => ( a.SendDateTime ?? a.FutureSendDateTime ) < upperDate );
            }

            string content = tbContent.Text;
            if ( !string.IsNullOrWhiteSpace( content ) )
            {
                communications = communications.Where( c =>
                    c.Message.Contains( content ) ||
                    c.SMSMessage.Contains( content ) ||
                    c.PushMessage.Contains( content ) );
            }

            var recipients = new CommunicationRecipientService( rockContext ).Queryable();

            // We want to limit to only communications that they are authorized to view, but if there are a large number of communications, that could be very slow.
            // So, since communication security is based on CommunicationTemplate, take a shortcut and just limit based on authorized communication templates
            var authorizedCommunicationTemplateIds = new CommunicationTemplateService( rockContext ).Queryable()
                .Where( a => communications.Where( x => x.CommunicationTemplateId.HasValue ).Select( x => x.CommunicationTemplateId.Value ).Distinct().Contains( a.Id ) )
                .ToList().Where( a => a.IsAuthorized( Rock.Security.Authorization.VIEW, this.CurrentPerson ) ).Select( a => a.Id ).ToList();

            var queryable = communications.Where( a => a.CommunicationTemplateId == null || authorizedCommunicationTemplateIds.Contains( a.CommunicationTemplateId.Value ) )
                .Select( c => new CommunicationItem
                {
                    Id = c.Id,
                    CommunicationType = c.CommunicationType,
                    // Subject = string.IsNullOrEmpty( c.Subject ) ? c.Name : c.Subject,
                    Subject = string.IsNullOrEmpty( c.Name ) ? ( string.IsNullOrEmpty( c.Subject ) ? c.PushTitle : c.Subject ) : c.Name,
                    CreatedDateTime = c.CreatedDateTime,
                    SendDateTime = c.SendDateTime ?? c.FutureSendDateTime,
                    SendDateTimePrefix = c.SendDateTime == null && c.FutureSendDateTime != null ? "<span class='label label-info'>Future</span>&nbsp;" : "",
                    Sender = c.SenderPersonAlias != null ? c.SenderPersonAlias.Person : null,
                    ReviewedDateTime = c.ReviewedDateTime,
                    Reviewer = c.ReviewerPersonAlias != null ? c.ReviewerPersonAlias.Person : null,
                    Status = c.Status,
                    Recipients = recipients.Where( r => r.CommunicationId == c.Id ).Count(),
                    PendingRecipients = recipients.Where( r => r.CommunicationId == c.Id && r.Status == CommunicationRecipientStatus.Pending ).Count(),
                    CancelledRecipients = recipients.Where( r => r.CommunicationId == c.Id && r.Status == CommunicationRecipientStatus.Cancelled ).Count(),
                    FailedRecipients = recipients.Where( r => r.CommunicationId == c.Id && r.Status == CommunicationRecipientStatus.Failed ).Count(),
                    DeliveredRecipients = recipients.Where( r => r.CommunicationId == c.Id && ( r.Status == CommunicationRecipientStatus.Delivered || r.Status == CommunicationRecipientStatus.Opened ) ).Count(),
                    OpenedRecipients = recipients.Where( r => r.CommunicationId == c.Id && r.Status == CommunicationRecipientStatus.Opened ).Count()
                } );

            var sortProperty = gCommunication.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderByDescending( c => c.SendDateTime );
            }

            gCommunication.EntityTypeId = EntityTypeCache.Get<Rock.Model.Communication>().Id;
            nbBindError.Text = string.Empty;

            try
            {
                gCommunication.SetLinqDataSource( queryable );
                gCommunication.DataBind();
            }
            catch ( Exception e )
            {
                ExceptionLogService.LogException( e );

                Exception sqlException = e;
                while ( sqlException != null && !( sqlException is System.Data.SqlClient.SqlException ) )
                {
                    sqlException = sqlException.InnerException;
                }

                nbBindError.Text = string.Format( "<p>An error occurred trying to retrieve the communication history. Please try adjusting your filter settings and try again.</p><p>Error: {0}</p>",
                    sqlException != null ? sqlException.Message : e.Message );

                gCommunication.DataSource = new List<object>();
                gCommunication.DataBind();
            }

        }

        #endregion

        protected class CommunicationItem : DotLiquid.Drop
        {
            public int Id { get; set; }
            public CommunicationType CommunicationType { get; set; }
            public DateTime? CreatedDateTime { get; set; }
            public string Subject { get; set; }
            public string SendDateTimePrefix { get; set; }
            public DateTime? SendDateTime { get; set; }
            public Person Sender { get; set; }
            public DateTime? ReviewedDateTime { get; set; }
            public Person Reviewer { get; set; }
            public CommunicationStatus Status { get; set; }
            public int Recipients { get; set; }
            public int PendingRecipients { get; set; }
            public int CancelledRecipients { get; set; }
            public int FailedRecipients { get; set; }
            public int DeliveredRecipients { get; set; }
            public int OpenedRecipients { get; set; }
            public string SendDateTimeFormat
            {
                get
                {
                    return SendDateTime.HasValue ? SendDateTimePrefix + SendDateTime.Value.ToShortDateTimeString() : string.Empty;
                }
            }

            public string TypeIconCssClass
            {
                get
                {
                    var iconCssClass = string.Empty;
                    switch ( this.CommunicationType )
                    {
                        case CommunicationType.RecipientPreference:
                            iconCssClass = "fa fa-user fa-lg";
                            break;
                        case CommunicationType.Email:
                            iconCssClass = "fa fa-envelope fa-lg";
                            break;
                        case CommunicationType.SMS:
                            iconCssClass = "fa fa-comment fa-lg";
                            break;
                        case CommunicationType.PushNotification:
                            iconCssClass = "fa fa-bell fa-lg";
                            break;
                        default:
                            break;
                    }
                    return iconCssClass;
                }
            }
        }

    }
}
