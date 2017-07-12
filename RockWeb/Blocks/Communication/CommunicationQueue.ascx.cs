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
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace RockWeb.Blocks.Communication
{
    [DisplayName( "Communication Queue" )]
    [Category( "Communication" )]
    [Description( "Lists the status of all communications." )]

    [LinkedPage( "Detail Page" )]
    public partial class CommunicationQueue : Rock.Web.UI.RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

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
            else
            {
                // Because the RockCheckBoxList does not handle postback event correctly, check to see if it was selected
                // and update values manually.
                if (Request.Form["__EVENTTARGET"] == cblMedium.UniqueID )
                {
                    for ( int i = 0; i < cblMedium.Items.Count; i++ )
                    {
                        string value = Request.Form[cblMedium.UniqueID + "$" + i.ToString()];
                        if ( value != null )
                        {
                            cblMedium.Items[i].Selected = true;
                        }
                        else
                        {
                            cblMedium.Items[i].Selected = false;
                        }
                    }

                    BindGrid();

                }

            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        protected void cbFilter_Changed( object sender, EventArgs e )
        {
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
            string mediumValues = GetBlockUserPreference( "Medium" );

            cblMedium.SetValues( mediumValues.SplitDelimitedValues().AsIntegerList() );
            cbPendingApproval.Checked = GetBlockUserPreference( "PendingApproval" ).AsBooleanOrNull() ?? false;
            cbFutureComm.Checked = GetBlockUserPreference( "FutureCommunication" ).AsBooleanOrNull() ?? false;
        }

        /// <summary>
        /// Binds the mediums.
        /// </summary>
        private void BindMediums()
        {
            foreach ( var item in MediumContainer.Instance.Components.Values )
            {
                if ( item.Value.IsActive )
                {
                    var entityType = item.Value.EntityType;
                    cblMedium.Items.Add( new ListItem( item.Metadata.ComponentName, entityType.Id.ToString() ) );
                }
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();

            var jobEntityType = EntityTypeCache.Read( typeof( Rock.Model.ServiceJob ) );

            int expirationDays = GetJobAttributeValue("ExpirationPeriod", 3, rockContext );
            int delayMins = GetJobAttributeValue( "DelayPeriod", 30, rockContext );

            var communications = new CommunicationService( rockContext )
                .GetQueued( expirationDays, delayMins, cbFutureComm.Checked, cbPendingApproval.Checked );

            var mediumEntityTypeIds = cblMedium.SelectedValues.AsIntegerList();

            if ( mediumEntityTypeIds.Any() )
            {
                communications = communications.Where( c => c.MediumEntityTypeId.HasValue && mediumEntityTypeIds.Contains( c.MediumEntityTypeId.Value ) );
            }

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
                    PendingRecipients = c.Recipients.Where( r => r.Status == CommunicationRecipientStatus.Pending ).Count()
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

            SetBlockUserPreference( "FutureCommunication", cbFutureComm.Checked ? "True" : string.Empty );
            SetBlockUserPreference( "PendingApproval", cbPendingApproval.Checked ? "True" : string.Empty );
            SetBlockUserPreference( "Medium", cblMedium.SelectedValues.AsDelimited( ";" ) );

        }

        private int GetJobAttributeValue( string key, int defaultValue, RockContext rockContext )
        {
            var jobEntityType = EntityTypeCache.Read( typeof( Rock.Model.ServiceJob ) );

            int intValue = 3;
            var jobExpirationAttribute = new AttributeService( rockContext )
                .Queryable().AsNoTracking()
                .Where( a =>
                    a.EntityTypeId == jobEntityType.Id &&
                    a.EntityTypeQualifierColumn == "Class" &&
                    a.EntityTypeQualifierValue == "Rock.Jobs.SendCommunications" &&
                    a.Key == key )
                .FirstOrDefault();
            if ( jobExpirationAttribute != null )
            {
                intValue = jobExpirationAttribute.DefaultValue.AsIntegerOrNull() ?? 3;
                var attributeValue = new AttributeValueService( rockContext )
                    .Queryable().AsNoTracking()
                    .Where( v => v.AttributeId == jobExpirationAttribute.Id )
                    .FirstOrDefault();
                if ( attributeValue != null )
                {
                    intValue = attributeValue.Value.AsIntegerOrNull() ?? intValue;
                }
            }

            return intValue;
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