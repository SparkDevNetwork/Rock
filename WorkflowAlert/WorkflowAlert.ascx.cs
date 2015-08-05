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
using System.ComponentModel;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Plugins.cc_newspring.Blocks.WorkflowAlert
{
    /// <summary>
    /// Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user.
    /// </summary>
    [DisplayName( "My Workflows Alerts" )]
    [Category( "NewSpring" )]
    [Description( "Block to display the workflow types that user is authorized to view, and the activities that are currently assigned to the user." )]
    [LinkedPage( "Listing Page", "Page used to view all workflows assigned to the current user." )]
    public partial class WorkflowAlert : Rock.Web.UI.RockBlock
    {
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Check for current person
            if ( CurrentPerson != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Search the DB for active workflows assigned to the current user, and return the count
                    workflowAlertNumber.Value = new WorkflowActivityService( rockContext ).Queryable().AsNoTracking().Where( w => w.CompletedDateTime == null && w.AssignedPersonAliasId == CurrentPersonAliasId ).Count().ToString();
                }
            }
        }

        /// <summary>
        /// Navigates to the Listing page.
        /// </summary>
        protected void lbListingPage_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ListingPage" );
        }
    }
}