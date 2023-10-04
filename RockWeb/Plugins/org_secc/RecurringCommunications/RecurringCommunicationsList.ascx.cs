// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
using System;
using System.ComponentModel;
using Rock;
using Rock.Model;
using System.Web.UI;
using Rock.Web.Cache;
using Rock.Data;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI.WebControls;
using Rock.Attribute;
using org.secc.RecurringCommunications.Model;

namespace RockWeb.Plugins.org_secc.RecurringCommunications
{
    [DisplayName( "Recurring Communications List" )]
    [Category( "SECC > Communication" )]
    [Description( "Displays a list of all recurring communications." )]

    [LinkedPage( "Details Page",
        Description = "Page which contains the details of the recurring communication.",
        Key = AttributeKeys.DetailsPage )]
    public partial class RecurringCommunicationsList : Rock.Web.UI.RockBlock
    {
        internal class AttributeKeys
        {
            public const string DetailsPage = "DetailsPage";
        }

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gRC.Actions.ShowAdd = true;
            gRC.Actions.AddClick += Actions_AddClick;
        }

        private void Actions_AddClick( object sender, EventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.DetailsPage );
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
        }

        private void BindGrid()
        {
            RockContext rockContext = new RockContext();
            RecurringCommunicationService recurringCommunicationService = new RecurringCommunicationService( rockContext );

            gRC.DataSource = recurringCommunicationService.Queryable().ToList();
            gRC.DataBind();
        }

        protected void gRC_RowSelected( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            NavigateToLinkedPage( AttributeKeys.DetailsPage, new Dictionary<string, string> { { "RecurringCommunicationId", e.RowKeyId.ToString() } } );
        }

        protected void btnDelete_Click( object sender, Rock.Web.UI.Controls.RowEventArgs e )
        {
            var rcId = ( int ) e.RowKeyValue;
            RockContext rockContext = new RockContext();
            RecurringCommunicationService recurringCommunicationService = new RecurringCommunicationService( rockContext );
            var recurringCommunication = recurringCommunicationService.Get( rcId );
            if ( recurringCommunication != null )
            {
                recurringCommunicationService.Delete( recurringCommunication );
                rockContext.SaveChanges();
            }
            BindGrid();
        }
    }
}