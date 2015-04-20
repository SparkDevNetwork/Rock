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
using System.IO;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Blocks.WorkFlow
{
    /// <summary>
    /// Activates a workflow and then redirects user to workflow entry page.
    /// </summary>
    [DisplayName( "Activate Workflow (Deprecated)" )]
    [Category( "WorkFlow" )]
    [Description( "Activates a workflow and then redirects user to workflow entry page. NOTE: This block has been deprecated and will be removed in a future update. The Workflow Entry block now supports the same functionality." )]

    [LinkedPage( "Workflow Entry Page" )]
    public partial class ActivateWorkflow : Rock.Web.UI.RockBlock
    {

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // Notify the Rock Administrators that an obsolete block is being used
            try
            {
                var pageReference = new Rock.Web.PageReference( GetAttributeValue( "WorkflowEntryPage" ) );
                string subject = string.Format( "Your '{0}' site is using an obsolete block!", RockPage.Site.Name );
                string message = string.Format( @"Your '{0}' site is still using the 'Activate Workflow' block on page:
{1}!<br/><br/>The Activate Workflow block has been deprecated and will be removed during a future update. 
This block was previously used to activate a workflow, set attribute values from any existing query string parameters, 
and then redirect the user to a page with the Workflow Entry block. Because the Workflow Entry block now also 
supports setting attribute values from query string parameters, the Activate Workflow block is no 
longer needed.<br/><br/>Please update any place that links to page ID: {1}, to instead link directly to the Workflow 
Entry page (Page ID: {2}).", RockPage.Site.Name, RockPage.PageId, pageReference.PageId );
                Rock.Communication.Email.NotifyAdmins( subject, message );
            }
            catch ( Exception ex )
            {
                ExceptionLogService.LogException( ex, System.Web.HttpContext.Current );
            }

            // Pass all the query string parameters to the entry page
            var pageParams = new Dictionary<string, string>();
            foreach( var param in PageParameters())
            {
                pageParams.Add( param.Key, param.Value.ToString() );
            }
            NavigateToLinkedPage( "WorkflowEntryPage", pageParams );
        }

        #endregion
    }
}