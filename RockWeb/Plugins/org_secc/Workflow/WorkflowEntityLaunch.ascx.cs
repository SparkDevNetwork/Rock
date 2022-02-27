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
//
// <copyright>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Plugins.org_secc.Workflow
{
    /// <summary>
    /// Block for launching workflows for a single entity using querystring parameters.
    /// </summary>
    [DisplayName( "Workflow Entity Launch" )]
    [Category( "SECC > Workflow" )]
    [Description( "Block for launching workflows for a single entity using querystring parameters." )]
    [EntityTypeField( "Entity Type", "The entity type to use when loading the entity by Id." )]
    [WorkflowTypeField ("Workflow Type", "The workflow type to launch for the given entity." )]
    public partial class WorkflowEntityLaunch : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {

            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                bbtnLaunch.Enabled = false;
                Guid? entityTypeGuid = GetAttributeValue( "EntityType" ).AsGuidOrNull();
                Guid? workflowGuid = GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
                int? entityId = PageParameter( "EntityId" ).AsIntegerOrNull();
                if ( !workflowGuid.HasValue || !entityTypeGuid.HasValue )
                {
                    nbInformation.Text = "Please configure the block before using it.<br />";
                    nbInformation.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;

                } 
                else if ( !entityId.HasValue || entityId <= 0 )
                {
                    nbInformation.Text += "Please pass the EntityId as a page parameter.<br />";
                    nbInformation.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
                }
                else
                {
                    var entity = getEntity();
                    var workflowType = WorkflowTypeCache.Get( workflowGuid.Value );
                    nbInformation.Text = string.Format( "Clicking \"Launch\" below will start a new instance of <i>{0}</i> for <i>{1}</i> (ID: {2:d}).", workflowType.Name, entity.ToString(), entityId.Value );
                    nbInformation.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Info;
                    bbtnLaunch.Enabled = true;
                }
            }
        }

        /**
         * Handle the launch click
         */
        protected void Launch_Click( object sender, EventArgs e )
        {
            bbtnLaunch.Enabled = false;
            Guid? workflowGuid = GetAttributeValue( "WorkflowType" ).AsGuidOrNull();
            var entity = getEntity();

            if ( entity != null && workflowGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var workflowType = WorkflowTypeCache.Get( workflowGuid.Value );

                    if ( workflowType != null )
                    {
                        Dictionary<String, String> attributes = new Dictionary<String, String>();
                        if ( entity.TypeName == "Rock.Model.GroupMember" )
                        {
                            attributes.Add( "Group", ( ( GroupMember ) entity).Group.Guid.ToString() );
                            attributes.Add( "Person", ( ( GroupMember ) entity).Person.PrimaryAlias.Guid.ToString() );
                        }

                        // Get the service and launch the workflow!
                        var cachedEntityType = EntityTypeCache.Get( GetAttributeValue( "EntityType" ).AsGuid() );
                        Type entityType = cachedEntityType.GetEntityType();

                        var service = Reflection.GetServiceForEntityType( entityType, Reflection.GetDbContextForEntityType( entityType ) );

                        MethodInfo getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                        MethodInfo launchWorkflowMethod = entity.GetType().GetMethod( "LaunchWorkflow", new Type[] { typeof( int ),  typeof( string ), typeof( Dictionary<string, string> ) } );
                        if ( launchWorkflowMethod != null )
                        {
                            litOutput.Text = "Launching workflow for " + entity.ToString() + "<br />";
                            var workflowResult = launchWorkflowMethod.Invoke( entity, new object[] { workflowType.Id, entity.ToString(), attributes } );
                        }
                    }
                }
            }
        }

        /**
         * Get the entity from the attributes
         */
        private IEntity getEntity()
        {
            Guid? entityTypeGuid = GetAttributeValue( "EntityType" ).AsGuidOrNull();
            int? entityId = PageParameter( "EntityId" ).AsIntegerOrNull();

            if ( entityTypeGuid.HasValue && entityId.HasValue && entityId > 0 )
            {
                using ( var rockContext = new RockContext() )
                {
                    var cachedEntityType = EntityTypeCache.Get( entityTypeGuid.Value );
                    Type entityType = cachedEntityType.GetEntityType();
                    var service = Reflection.GetServiceForEntityType( entityType, Reflection.GetDbContextForEntityType( entityType ) );

                    MethodInfo getMethod = service.GetType().GetMethod( "Get", new Type[] { typeof( int ) } );
                    if ( getMethod != null )
                    {
                        var result = getMethod.Invoke( service, new object[] { entityId.Value } );
                        var entity = result as IEntity;

                        return entity;

                    }
                }
            }
            return null;
        }
    }
}