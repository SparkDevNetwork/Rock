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
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Attribute;

namespace RockWeb.Plugins.com_centralaz.Utility
{
    /// <summary>
    /// Template block for developers to use to start a new block.
    /// </summary>
    [DisplayName( "Lava Tester" )]
    [Category( "com_centralaz > Utility" )]
    [Description( "Allows you to pick a person entity and test your lava." )]
    public partial class LavaTester : Rock.Web.UI.RockBlock
    {
        #region Fields

        // used for private variables
        Dictionary<string, object> mergeFields = null;
        private readonly string _USER_PREF_KEY = "MyLavaTestText";
        #endregion

        #region Properties

        // used for public / protected properties

        #endregion

        #region Base Control Methods

        //  overrides of the base RockBlock methods (i.e. OnInit, OnLoad)

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( this.RockPage, this.CurrentPerson );
            
            base.OnInit( e );

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
            
            if ( string.IsNullOrEmpty( ceLava.Text ) )
            {
                var text = GetUserPreference( _USER_PREF_KEY );
                ceLava.Text = text;

                // Only show instructions the first time.
                if ( !string.IsNullOrEmpty( text ) )
                {
                    nbInstructions.Visible = false;
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            litDebug.Text = "";

            if ( ! Page.IsPostBack )
            {
                if ( cbEnableDebug.Checked )
                {
                    litDebug.Text = mergeFields.lavaDebugInfo();
                }
            }
        }

        #endregion

        #region Events

        // handlers called by the controls on your block

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {

        }

        #endregion

        #region Methods

        // helper functional methods (like BindGrid(), etc.)

        #endregion
        protected void bbTest_Click( object sender, EventArgs e )
        {
            nbInstructions.Visible = false;

            try
            {
                // Save lava test string for future use.
                SetUserPreference( _USER_PREF_KEY, ceLava.Text );

                RockContext rockContext = new RockContext();
                PersonService personService = new PersonService( rockContext );
                Person person;
                if ( ppPerson.PersonId.HasValue )
                {
                    person = personService.Get( ppPerson.PersonId ?? -1, false );
                }
                else
                {
                    person = CurrentPerson;
                }

                if ( gpGroups != null && gpGroups.SelectedValueAsInt().HasValue )
                {
                    GroupService groupService = new GroupService( rockContext );
                    mergeFields.Add( "Group", groupService.Get( gpGroups.SelectedValueAsInt() ?? -1 ) );
                }

                if ( ddlWorkflows != null && ddlWorkflows.Items.Count > 0 && ddlWorkflows.SelectedValueAsInt().HasValue )
                {
                    WorkflowService workflowService = new WorkflowService( rockContext );
                    if ( mergeFields.ContainsKey( "Workflow" ) )
                    {
                        mergeFields.Remove( "Workflow" );
                    }
                    mergeFields.Add( "Workflow", workflowService.Get( ddlWorkflows.SelectedValueAsInt() ?? -1 ) );
                }

                // Get Lava
                mergeFields.Add( "Person", person );

                ResolveLava();
            }
            catch( Exception ex )
            {
                //LogException( ex );
                litDebug.Text = "<pre>" + ex.StackTrace + "</pre>";
            }
        }

        protected void wfpWorkflowType_SelectItem( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            WorkflowService workflowService = new WorkflowService( rockContext );
            int? workflowTypeId = wfpWorkflowType.SelectedValueAsInt();
            if ( workflowTypeId.HasValue )
            {
                var workflows = workflowService.Queryable().AsNoTracking().Where( w => w.WorkflowTypeId == workflowTypeId.Value ).ToList();
                ddlWorkflows.DataSource = workflows;
                ddlWorkflows.DataBind();
                ddlWorkflows.Visible = true;

                if ( workflows.Count > 0 )
                {
                    mergeFields.Add( "Workflow", workflows[0] );
                    ResolveLava();
                }

            }
        }

        protected void ddlWorkflows_SelectedIndexChanged( object sender, EventArgs e )
        {
            RockContext rockContext = new RockContext();
            WorkflowService workflowService = new WorkflowService( rockContext );
            mergeFields.Add( "Workflow", workflowService.Get( ddlWorkflows.SelectedValueAsInt() ?? -1 ) );

            ResolveLava();
        }

        protected void ResolveLava()
        {
            string lava = ceLava.Text;
            litOutput.Text = lava.ResolveMergeFields( mergeFields );
            if ( cbEnableDebug.Checked )
            {
                litDebug.Text = mergeFields.lavaDebugInfo();
                h3DebugTitle.Visible = true;
            }
        }
}
}