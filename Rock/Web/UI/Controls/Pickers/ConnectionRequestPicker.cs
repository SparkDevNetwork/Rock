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
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class ConnectionRequestPicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "";
            this.IconCssClass = "fa fa-plug";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="connectionRequest">The connection request.</param>
        public void SetValue( ConnectionRequest connectionRequest )
        {
            if ( connectionRequest != null && 
                connectionRequest.PersonAlias != null &&
                connectionRequest.PersonAlias.Person != null &&
                connectionRequest.ConnectionOpportunity != null )
            {
                ItemId = connectionRequest.Id.ToString();
                ItemName = connectionRequest.PersonAlias.Person.FullName;
                InitialItemParentIds = string.Format( "T{0},O{1}",
                    connectionRequest.ConnectionOpportunity.ConnectionTypeId,
                    connectionRequest.ConnectionOpportunity.Id );
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="connectionRequests">The connectionRequests.</param>
        public void SetValues( IEnumerable<ConnectionRequest> connectionRequests )
        {
            var theConnectionRequests = connectionRequests.ToList();

            if ( theConnectionRequests.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentConnectionRequestIds = string.Empty;

                foreach ( var connectionRequest in theConnectionRequests )
                {
                    if ( connectionRequest != null &&
                        connectionRequest.PersonAlias != null &&
                        connectionRequest.PersonAlias.Person != null &&
                        connectionRequest.ConnectionOpportunity != null )
                    {
                        ids.Add( connectionRequest.Id.ToString() );
                        names.Add( connectionRequest.PersonAlias.Person.FullName );
                        parentConnectionRequestIds += string.Format( "T{0},O{1},",
                            connectionRequest.ConnectionOpportunity.ConnectionTypeId,
                            connectionRequest.ConnectionOpportunity.Id );
                    }
                }

                InitialItemParentIds = parentConnectionRequestIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            int? connectionRequestId = ItemId.AsIntegerOrNull();
            if ( connectionRequestId.HasValue )
            {
                var connectionRequest = new ConnectionRequestService( new RockContext() ).Get( connectionRequestId.Value );
                SetValue( connectionRequest );
            }
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var connectionRequestIds = new List<int>();
            foreach( var itemId in ItemIds )
            {
                int? connectionRequestId = ItemId.AsIntegerOrNull();
                if ( connectionRequestId.HasValue )
                {
                    connectionRequestIds.Add( connectionRequestId.Value );
                }
            }

            var connectionRequests = new ConnectionRequestService( new RockContext() ).Queryable().Where( g => connectionRequestIds.Contains( g.Id ) );
            this.SetValues( connectionRequests );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/ConnectionRequests/Getchildren/"; }
        }

    }
}