// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
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
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Communication Recipient List" )]
    [Category( "Communication" )]
    [Description( "Lists communications sent to an individual" )]

    [ContextAware]
    [LinkedPage( "Detail Page" )]
    public partial class CommunicationRecipientList : RockBlock
    {
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
            rFilter.SaveUserPreference( "Medium", cpMedium.SelectedValue );
            rFilter.SaveUserPreference( "Status", ddlStatus.SelectedValue );
            int personId = ppSender.PersonId ?? 0;
            rFilter.SaveUserPreference( "Created By", personId.ToString() );

            rFilter.SaveUserPreference( "Date Range", drpDates.DelimitedValues );

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
                case "Medium":
                    {
                        var entity = EntityTypeCache.Read( e.Value.AsGuid() );
                        if ( entity != null )
                        {
                            e.Value = entity.FriendlyName;
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
            NavigateToLinkedPage( "DetailPage", "CommunicationId", e.RowKeyId );
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
            tbSubject.Text = rFilter.GetUserPreference( "Subject" );

            if ( cpMedium.Items[0].Value != string.Empty )
            {
                cpMedium.Items.Insert( 0, new ListItem( string.Empty, string.Empty ) );
            }

            cpMedium.SelectedValue = rFilter.GetUserPreference( "Medium" );

            ddlStatus.BindToEnum<CommunicationStatus>( true, new CommunicationStatus[] { CommunicationStatus.Transient } );
            ddlStatus.SelectedValue = rFilter.GetUserPreference( "Status" );

            int? personId = rFilter.GetUserPreference( "Created By" ).AsIntegerOrNull();

            if ( personId.HasValue && personId.Value != 0 )
            {
                var personService = new PersonService( new RockContext() );
                var person = personService.Get( personId.Value );
                if ( person != null )
                {
                    ppSender.SetValue( person );
                }
            }

            drpDates.DelimitedValues = rFilter.GetUserPreference( "Date Range" );

            tbContent.Text = rFilter.GetUserPreference( "Content" );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            // If configured for a person and person is null, return
            int personEntityTypeId = EntityTypeCache.Read<Person>().Id;
            if ( ContextTypesRequired.Any( e => e.Id == personEntityTypeId ) && _person == null )
            {
                return;
            }

            var rockContext = new RockContext();

            var qryCommunications = new CommunicationService( rockContext ).Queryable().Where( c => c.Status != CommunicationStatus.Transient );

            string subject = tbSubject.Text;
            if ( !string.IsNullOrWhiteSpace( subject ) )
            {
                qryCommunications = qryCommunications.Where( c => c.Subject.Contains( subject ) );
            }

            Guid? entityTypeGuid = cpMedium.SelectedValue.AsGuidOrNull();
            if ( entityTypeGuid.HasValue )
            {
                qryCommunications = qryCommunications.Where( c => c.MediumEntityType != null && c.MediumEntityType.Guid.Equals( entityTypeGuid.Value ) );
            }

            var communicationStatus = ddlStatus.SelectedValue.ConvertToEnumOrNull<CommunicationStatus>();
            if ( communicationStatus.HasValue )
            {
                qryCommunications = qryCommunications.Where( c => c.Status == communicationStatus.Value );
            }

            // only communications for the selected recipient (_person)
            if ( _person != null )
            {
                qryCommunications = qryCommunications.Where( c => c.Recipients.Any( a => a.PersonAlias.PersonId == _person.Id ) );
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
                qryCommunications = qryCommunications.Where( c => c.MediumDataJson.Contains( content ) );
            }

            var sortProperty = gCommunication.SortProperty;
            if ( sortProperty != null )
            {
                qryCommunications = qryCommunications.Sort( sortProperty );
            }
            else
            {
                qryCommunications = qryCommunications.OrderByDescending( c => c.CreatedDateTime );
            }

            gCommunication.EntityTypeId = EntityTypeCache.Read<Rock.Model.Communication>().Id;
            gCommunication.SetLinqDataSource( qryCommunications.Include(a => a.MediumEntityType).AsNoTracking() );
            gCommunication.DataBind();
        }

        #endregion
    }
}