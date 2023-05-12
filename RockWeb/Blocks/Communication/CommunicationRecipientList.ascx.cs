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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Utility;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    /* 8-4-2021 MDP

    Note that there are two blocks that are very similar
    - CommunicationList (People > Communication History)
    - CommunicationRecipientList (The block shown in the Person Profile History tab)

    So any changes you make to one might need to be made to the other.

    There are a few differences between these two blocks in what these blocks do,
    but it might be worth considering combining these blocks into one block in the future.
     
     */

    [DisplayName( "Communication Recipient List" )]
    [Category( "Communication" )]
    [Description( "Lists communications sent to an individual" )]

    #region Block Attributes

    [ContextAware]
    [LinkedPage(
        "Detail Page",
        Key = AttributeKey.DetailPage )]

    #endregion Block Attributes
    [Rock.SystemGuid.BlockTypeGuid( "EBEA5996-5695-4A42-A21C-29E11E711BE8" )]
    public partial class CommunicationRecipientList : RockBlock, ICustomGridColumns
    {
        #region Attribute Keys

        /// <summary>
        /// Keys to use for Block Attributes
        /// </summary>
        private static class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        #endregion Attribute Keys

        #region Page Parameter Keys

        /// <summary>
        /// Keys to use for Page Parameters
        /// </summary>
        private static class PageParameterKey
        {
            public const string CommunicationId = "CommunicationId";
        }

        #endregion

        #region Fields

        private Person _person = null;

        #endregion

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

            if ( IsUserAuthorized( Rock.Security.Authorization.EDIT ) )
            {
                gCommunication.RowSelected += gCommunication_RowSelected;
            }

            gCommunication.RowDataBound += gCommunication_RowDataBound;
            gCommunication.GridRebind += gCommunication_GridRebind;
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            var contextEntity = this.ContextEntity();
            if ( contextEntity != null )
            {
                if ( contextEntity is Person )
                {
                    _person = contextEntity as Person;
                }
            }

            if ( !Page.IsPostBack )
            {
                SetFilter();
            }

            BindGrid();

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
            rFilter.SetFilterPreference( "Subject", tbSubject.Text );
            rFilter.SetFilterPreference( "Communication Type", ddlType.SelectedValue );
            rFilter.SetFilterPreference( "Status", ddlStatus.SelectedValue );
            int personId = ppSender.PersonId ?? 0;
            rFilter.SetFilterPreference( "Created By", personId.ToString() );

            rFilter.SetFilterPreference( "Date Range", drpDates.DelimitedValues );

            rFilter.SetFilterPreference( "Content", tbContent.Text );

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
                            var status = e.Value.ConvertToEnumOrNull<CommunicationStatus>();
                            e.Value = status.ConvertToString();
                        }

                        break;
                    }

                case "Created By":
                    {
                        string personName = string.Empty;

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

                        e.Value = personName;

                        break;
                    }

                case "Date Range":
                    {
                        e.Value = DateRangePicker.FormatDelimitedValues( e.Value );
                        break;
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
            NavigateToLinkedPage( AttributeKey.DetailPage, PageParameterKey.CommunicationId, e.RowKeyId );
        }

        /// <summary>
        /// Handles the RowDataBound event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridViewRowEventArgs"/> instance containing the event data.</param>
        /// <exception cref="System.NotImplementedException"></exception>
        protected void gCommunication_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            if ( !UserCanEdit && e.Row.RowType == DataControlRowType.DataRow )
            {
                CommunicationItem communication = e.Row.DataItem as CommunicationItem;
                if (
                    !CurrentPersonAliasId.HasValue ||
                    communication == null ||
                    !communication.CreatedByPersonAliasId.HasValue ||
                    communication.CreatedByPersonAliasId.Value != CurrentPersonAliasId.Value )
                {
                    var lb = e.Row.Cells[5].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
                    if ( lb != null )
                    {
                        lb.Visible = false;
                    }
                }
            }
        }

        /// <summary>
        /// Handles the GridRebind event of the gCommunication control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void gCommunication_GridRebind( object sender, EventArgs e )
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
            tbSubject.Text = rFilter.GetFilterPreference( "Subject" );

            ddlType.BindToEnum<CommunicationType>( true );
            ddlType.SetValue( rFilter.GetFilterPreference( "Communication Type" ) );

            ddlStatus.BindToEnum<CommunicationStatus>( true, new CommunicationStatus[] { CommunicationStatus.Transient } );
            ddlStatus.SelectedValue = rFilter.GetFilterPreference( "Status" );

            int? personId = rFilter.GetFilterPreference( "Created By" ).AsIntegerOrNull();

            if ( personId.HasValue && personId.Value != 0 )
            {
                var personService = new PersonService( new RockContext() );
                var person = personService.Get( personId.Value );
                if ( person != null )
                {
                    ppSender.SetValue( person );
                }
            }

            drpDates.DelimitedValues = rFilter.GetFilterPreference( "Date Range" );

            tbContent.Text = rFilter.GetFilterPreference( "Content" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // If configured for a person and person is null, return
            int personEntityTypeId = EntityTypeCache.Get<Person>().Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && _person == null )
            {
                return;
            }

            var rockContext = new RockContext();

            var qryCommunications = new CommunicationService( rockContext ).Queryable().Where( c => c.Status != CommunicationStatus.Transient );

            string subject = tbSubject.Text;
            if ( !string.IsNullOrWhiteSpace( subject ) )
            {
                qryCommunications = qryCommunications.Where( c => ( string.IsNullOrEmpty( c.Subject ) && c.Name.Contains( subject ) ) || c.Subject.Contains( subject ) );
            }

            var communicationType = ddlType.SelectedValueAsEnumOrNull<CommunicationType>();
            if ( communicationType != null )
            {
                qryCommunications = qryCommunications.Where( c => c.CommunicationType == communicationType );
            }

            var communicationStatus = ddlStatus.SelectedValue.ConvertToEnumOrNull<CommunicationStatus>();
            if ( communicationStatus.HasValue )
            {
                qryCommunications = qryCommunications.Where( c => c.Status == communicationStatus.Value );
            }

            // only communications for the selected recipient (_person)
            if ( _person != null )
            {
                qryCommunications = qryCommunications
                    .Where( c =>
                        c.Recipients.Any( a =>
                            a.PersonAlias.PersonId == _person.Id &&
                            ( a.Status == CommunicationRecipientStatus.Delivered || a.Status == CommunicationRecipientStatus.Opened ) ) );
            }

            if ( drpDates.LowerValue.HasValue )
            {
                qryCommunications = qryCommunications.Where( a => a.CreatedDateTime >= drpDates.LowerValue.Value );
            }

            if ( drpDates.UpperValue.HasValue )
            {
                DateTime upperDate = drpDates.UpperValue.Value.Date.AddDays( 1 );
                qryCommunications = qryCommunications.Where( a => a.CreatedDateTime < upperDate );
            }

            string content = tbContent.Text;
            if ( !string.IsNullOrWhiteSpace( content ) )
            {
                qryCommunications = qryCommunications.Where( c =>
                    c.Message.Contains( content ) ||
                    c.SMSMessage.Contains( content ) ||
                    c.PushMessage.Contains( content ) );
            }

            var queryable = qryCommunications
                .WherePersonAuthorizedToView( rockContext, this.CurrentPerson )
                .Select( c => new CommunicationItem
                {
                    Id = c.Id,
                    CommunicationType = c.CommunicationType,
                    Subject = string.IsNullOrEmpty( c.Subject ) ? ( string.IsNullOrEmpty( c.PushTitle ) ? c.Name : c.PushTitle ) : c.Subject,
                    CreatedDateTime = c.CreatedDateTime,
                    Sender = c.SenderPersonAlias != null ? c.SenderPersonAlias.Person : null,
                    Status = c.Status,
                    CreatedByPersonAliasId = c.CreatedByPersonAliasId
                } );

            var sortProperty = gCommunication.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderByDescending( c => c.CreatedDateTime );
            }

            gCommunication.EntityTypeId = EntityTypeCache.Get<Rock.Model.Communication>().Id;
            gCommunication.SetLinqDataSource( queryable.AsNoTracking() );
            gCommunication.DataBind();
        }

        #endregion

        protected class CommunicationItem : RockDynamic
        {
            public int Id { get; set; }

            public CommunicationType CommunicationType { get; set; }

            public DateTime? CreatedDateTime { get; set; }

            public string Subject { get; set; }

            public Person Sender { get; set; }

            public CommunicationStatus Status { get; set; }

            public int? CreatedByPersonAliasId { get; set; }
        }
    }
}