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
//
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Communication;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Communication Queue" )]
    [Category( "Communication" )]
    [Description( "Lists the status of all communications." )]

    [IntegerField( "Time Range", "The Hour window within which all the communiations are looked for.", false, 2, "", 0 )]
    [LinkedPage( "Detail Page" )]
    public partial class CommunicationQueue : Rock.Web.UI.RockBlock
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

            gCommunicationQueue.DataKeyNames = new string[] { "Id" };
            gCommunicationQueue.Actions.ShowAdd = false;
            gCommunicationQueue.GridRebind += gCommunicationQueue_GridRebind;

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
            rFilter.SaveUserPreference( "FutureCommunication", cbFutureComm.Checked ? "True" : string.Empty );
            rFilter.SaveUserPreference( "PendingApproval", cbPendingApproval.Checked ? "True" : string.Empty );
            rFilter.SaveUserPreference( "Medium", cblMedium.SelectedValues.AsDelimited( ";" ) );

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
                        e.Value = ResolveValues( e.Value, cblMedium );
                        break;
                    }
            }
        }

        /// <summary>
        /// Handles the RowSelected event of the gCommunicationQueue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Rock.Web.UI.Controls.RowEventArgs" /> instance containing the event data.</param>
        protected void gCommunicationQueue_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "CommunicationId", e.RowKeyId );
        }

        /// <summary>
        /// Handles the GridRebind event of the gCommunicationQueue control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        void gCommunicationQueue_GridRebind( object sender, EventArgs e )
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

            BindMediums();
            string mediumValues = rFilter.GetUserPreference( "Medium" );

            cblMedium.SetValues( mediumValues.SplitDelimitedValues().AsIntegerList() );
            cbPendingApproval.Checked = rFilter.GetUserPreference( "PendingApproval" ).AsBooleanOrNull() ?? false;
            cbFutureComm.Checked = rFilter.GetUserPreference( "FutureCommunication" ).AsBooleanOrNull() ?? false;
        }

        /// <summary>
        /// Binds the mediums.
        /// </summary>
        private void BindMediums()
        {
            foreach ( var item in MediumContainer.Instance.Components.Values )
            {
                var entityType = item.Value.EntityType;
                cblMedium.Items.Add( new ListItem( item.Metadata.ComponentName, entityType.Id.ToString() ) );
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var timeRange = GetAttributeValue( "TimeRange" ).AsInteger();
            var beginWindow = RockDateTime.Now.AddHours( -timeRange );
            var endWindow = RockDateTime.Now.AddHours( timeRange );

            var rockContext = new RockContext();

            var qryPendingRecipients = new CommunicationRecipientService( rockContext ).Queryable().Where( a => a.Status == CommunicationRecipientStatus.Pending );
            var communications = new CommunicationService( rockContext )
                    .Queryable().AsNoTracking();

            bool pendingApproval = cbPendingApproval.Checked;

            if ( pendingApproval )
            {
                communications = communications.Where( c => c.Status == CommunicationStatus.PendingApproval
                                                || c.Status == CommunicationStatus.Approved );
            }
            else
            {
                communications = communications.Where( c => c.Status == CommunicationStatus.Approved );
            }

            var entityTypeGuids = cblMedium.SelectedValues.AsIntegerList();

            if ( entityTypeGuids.Any() )
            {
                communications = communications.Where( c => c.MediumEntityTypeId.HasValue && entityTypeGuids.Contains( c.MediumEntityTypeId.Value ) );
            }

            bool futureCommunication = cbFutureComm.Checked;

            if ( futureCommunication )
            {
                communications = communications.Where( c => ( !c.ReviewedDateTime.HasValue && c.CreatedDateTime.Value.CompareTo( beginWindow ) < 0 )
                                                 || ( c.ReviewedDateTime.HasValue && c.ReviewedDateTime.Value.CompareTo( beginWindow ) < 0 )
                                                 || ( c.FutureSendDateTime.HasValue && c.FutureSendDateTime.Value.CompareTo( beginWindow ) < 0 ) );
            }
            else
            {
                communications = communications.Where( c => ( !c.ReviewedDateTime.HasValue && c.CreatedDateTime.Value.CompareTo( beginWindow ) < 0 )
                                                    || ( c.ReviewedDateTime.HasValue && c.ReviewedDateTime.Value.CompareTo( beginWindow ) < 0 ) );
            }

            communications = communications.Where( c => qryPendingRecipients.Where( r => r.CommunicationId == c.Id ).Any() );

            var queryable = communications
                .Select( c => new
                {
                    Id = c.Id,
                    MediumName = c.MediumEntityTypeId.HasValue ? c.MediumEntityType.FriendlyName : null,
                    Mediumlabel = c.MediumEntityTypeId.HasValue && c.MediumEntityType.FriendlyName == "SMS" ? "success" : "info",
                    Subject = c.Subject,
                    SendDateTime = c.FutureSendDateTime.HasValue && c.FutureSendDateTime > c.CreatedDateTime ? c.FutureSendDateTime : c.CreatedDateTime,
                    Sender = c.SenderPersonAlias != null ? c.SenderPersonAlias.Person : null,
                    Status = c.Status,
                    PendingRecipients = qryPendingRecipients.Where( r => r.CommunicationId == c.Id ).Count()
                } );

            var sortProperty = gCommunicationQueue.SortProperty;
            if ( sortProperty != null )
            {
                queryable = queryable.Sort( sortProperty );
            }
            else
            {
                queryable = queryable.OrderByDescending( c => c.SendDateTime );
            }

            gCommunicationQueue.EntityTypeId = EntityTypeCache.Read<Rock.Model.Communication>().Id;
            gCommunicationQueue.SetLinqDataSource( queryable );
            gCommunicationQueue.DataBind();
        }

        /// <summary>
        /// Resolves the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <param name="checkBoxList">The check box list.</param>
        /// <returns></returns>
        private string ResolveValues( string values, CheckBoxList checkBoxList )
        {
            var resolvedValues = new List<string>();

            foreach ( string value in values.Split( ';' ) )
            {
                var item = checkBoxList.Items.FindByValue( value );
                if ( item != null )
                {
                    resolvedValues.Add( item.Text );
                }
            }

            return resolvedValues.AsDelimited( ", " );
        }
        #endregion

    }
}