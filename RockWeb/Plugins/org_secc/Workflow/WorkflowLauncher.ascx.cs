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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Plugins.org_secc.Workflow
{
    /// <summary>
    /// Block for launching workflows for common entity types.
    /// </summary>
    [DisplayName( "Workflow Launcher" )]
    [Category( "SECC > Workflow" )]
    [Description( "Block for launching workflows for common entity types" )]
    public partial class WorkflowLauncher : RockBlock
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
                BindData();
            }
        }


        /// <summary>
        /// Binds the dropdown menus
        /// </summary>
        private void BindData()
        {

            using ( var rockContext = new RockContext() )
            {
                RegistrationInstanceService registrationInstanceService = new RegistrationInstanceService( rockContext );

                var registrationInstances = registrationInstanceService.Queryable().Where( ri => ri.IsActive == true ).AsNoTracking().ToList();
                ddlRegistrationInstances.DataSource = registrationInstances;
                RegistrationInstance emptyRegistrationInstance = new RegistrationInstance { Id = -1, Name = "" };
                registrationInstances.Insert( 0, emptyRegistrationInstance );
                ddlRegistrationInstances.DataBind();

                var entityTypes = new EntityTypeService( new RockContext() ).GetEntities()
                    .Where( t => t.Guid.ToString() == Rock.SystemGuid.EntityType.GROUP.ToLower() ||
                                 t.Guid.ToString() == "5cd9c0c8-c047-61a0-4e36-0fdb8496f066" ||
                                 t.Guid.ToString() == Rock.SystemGuid.EntityType.DATAVIEW.ToLower() )
                    .OrderBy( t => t.FriendlyName )
                    .ToList();
                entityTypes.Insert( 0, new EntityType() { Id = -1, FriendlyName = "Select One" } );
                ddlEntityType.DataSource = entityTypes;
                ddlEntityType.DataBind();
            }
        }


        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlRegistrationInstances control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlRegistrationInstances_SelectedIndexChanged( object sender, EventArgs e )
        {
            int? registrationInstanceId = ddlRegistrationInstances.SelectedValueAsInt();
            if ( registrationInstanceId.HasValue && registrationInstanceId.Value != -1 )
            {
                BindRegistrationsUsingRegistrationInstances( registrationInstanceId.Value );
            }
        }


        /// <summary>
        /// Binds the Registrations (people who registered) using the RegistrationInstances.
        /// </summary>
        private void BindRegistrationsUsingRegistrationInstances( int? registrationInstanceId )
        {
            if ( registrationInstanceId == -1 )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                RegistrationService registrationService = new RegistrationService( rockContext );

                var registrations = registrationService.Queryable().AsNoTracking().Where( r => r.RegistrationInstanceId == registrationInstanceId.Value ).ToList();
                Registration emptyRegistration = new Registration { Id = -1, FirstName = "All Registrants" };
                registrations.Insert( 0, emptyRegistration );
                ddlRegistrations.DataSource = registrations;
                ddlRegistrations.Visible = true;
                ddlRegistrations.DataBind();

            }
        }

        protected void Launch_Click( object sender, EventArgs e )
        {
            if ( !wtpWorkflowType.SelectedValueAsInt().HasValue )
            {
                return;
            }
            using ( var rockContext = new RockContext() )
            {
                if ( ddlEntityType.SelectedValue == "Rock.Model.Group" )
                {
                    litOutput.Text = "";
                    GroupMemberService groupMemberService = new GroupMemberService( rockContext );
                    List<GroupMember> groupMembers = new List<GroupMember>();
                    if ( gmpGroupMemberPicker.SelectedIndex > 0 )
                    {
                        GroupMember groupMember = groupMemberService.Get( gmpGroupMemberPicker.SelectedItem.Value.AsInteger() );
                        if ( groupMember != null )
                        {
                            groupMembers.Add( groupMember );
                        }
                    }
                    else
                    {
                        int? groupId = gpGroupPicker.SelectedValueAsInt();
                        groupMembers = groupMemberService.Queryable().Where( gm => gm.GroupId == groupId ).ToList();
                    }

                    // Now iterate through all the selected group members and launch the workflow!
                    foreach ( GroupMember groupMember in groupMembers )
                    {
                        litOutput.Text += "Launching workflow for " + groupMember.ToString() + "<br />";
                        Dictionary<String, String> attributes = new Dictionary<String, String>();
                        attributes.Add( "Group", groupMember.Group.Guid.ToString() );
                        attributes.Add( "Person", groupMember.Person.PrimaryAlias.Guid.ToString() );
                        groupMember.LaunchWorkflow( wtpWorkflowType.SelectedValueAsInt().Value, groupMember.ToString(), attributes, null );
                    }
                }
                else if ( ddlEntityType.SelectedValue == "Rock.Model.RegistrationInstance" )
                {
                    litOutput.Text = "";
                    RegistrationService registrationService = new RegistrationService( rockContext );

                    if ( ddlRegistrations.SelectedValueAsInt().HasValue && ddlRegistrations.SelectedValueAsInt() > 0 )
                    {
                        var registration = registrationService.Get( ddlRegistrations.SelectedValueAsInt().Value );
                        litOutput.Text += "Launching workflow for " + registration.ToString() + "<br />";
                        registration.LaunchWorkflow( wtpWorkflowType.SelectedValueAsInt().Value, registration.ToString(), null, null );
                    }
                    else if ( ddlRegistrationInstances.SelectedValueAsInt().HasValue && ddlRegistrationInstances.SelectedValueAsInt() > 0 )
                    {
                        int registrationInstanceId = ddlRegistrationInstances.SelectedValueAsInt().Value;
                        var registrations = registrationService.Queryable().Where( r => r.RegistrationInstanceId == registrationInstanceId );
                        foreach ( Registration registration in registrations )
                        {

                            litOutput.Text += "Launching workflow for " + registration.ToString() + "<br />";
                            registration.LaunchWorkflow( wtpWorkflowType.SelectedValueAsInt().Value, registration.ToString() );
                        }
                    }
                }
                else if ( ddlEntityType.SelectedValue == "Rock.Model.DataView" )
                {
                    litOutput.Text = "";
                    if ( ddlEntities.SelectedValueAsInt().HasValue && ddlEntities.SelectedValueAsInt() > 0 )
                    {
                        var personService = new PersonService( rockContext );
                        var person = personService.Get( ddlEntities.SelectedValueAsInt().Value );
                        litOutput.Text += "Launching workflow for " + person.FullName + "<br />";
                        person.LaunchWorkflow(wtpWorkflowType.SelectedValueAsInt().Value, person.FullName, null, null );
                    }
                    else if ( dvItemPicker.SelectedValueAsInt().HasValue && dvItemPicker.SelectedValueAsInt() > 0 )
                    {
                        DataViewService dataViewService = new DataViewService( rockContext );

                        var dataview = dataViewService.Get( dvItemPicker.SelectedValueAsInt().Value );
                        var errors = new List<string>();

                        if ( dataview.EntityType.Guid == new Guid( Rock.SystemGuid.EntityType.PERSON ) )
                        {
                            IQueryable<Person> entities = ( IQueryable<Person> ) dataview.GetQuery( new DataViewGetQueryArgs() );

                            foreach ( Person person in entities )
                            {
                                litOutput.Text += "Launching workflow for " + person.FullName + "<br />";
                                person.LaunchWorkflow(wtpWorkflowType.SelectedValueAsInt().Value, person.FullName, null, null );
                            }
                        }
                    }

                }
            }
        }

        protected void ddlEntityType_SelectedIndexChanged( object sender, EventArgs e )
        {
            divGroup.Visible = false;
            divRegistration.Visible = false;
            divDataView.Visible = false;
            if ( ddlEntityType.SelectedValue == "Rock.Model.Group" )
            {
                divGroup.Visible = true;
            }
            else if ( ddlEntityType.SelectedValue == "Rock.Model.RegistrationInstance" )
            {
                divRegistration.Visible = true;
            }
            else if ( ddlEntityType.SelectedValue == "Rock.Model.DataView" )
            {
                divDataView.Visible = true;
            }
        }

        protected void gpGroupPicker_SelectItem( object sender, EventArgs e )
        {
            gmpGroupMemberPicker.GroupId = gpGroupPicker.SelectedValue.AsInteger();
            gmpGroupMemberPicker.Visible = true;
        }


        protected void dvItemPicker_SelectItem( object sender, EventArgs e )
        {
            if ( !dvItemPicker.SelectedValueAsId().HasValue )
            {
                return;
            }
            ddlEntities.Visible = true;

            using ( var rockContext = new RockContext() )
            {
                DataViewService dataViewService = new DataViewService( rockContext );
                var dataview = dataViewService.Get( dvItemPicker.SelectedValueAsId().Value );

                if ( dataview.EntityType.Guid == new Guid( Rock.SystemGuid.EntityType.PERSON ) )
                {
                    List<Person> people = ( ( IQueryable<Person> ) dataview.GetQuery( new DataViewGetQueryArgs() { SortProperty = null, DatabaseTimeoutSeconds = null } ) ).OrderBy( p => p.LastName ).ToList();
                    Person emptyPerson = new Person { Id = -1, FirstName = "All People" };
                    people.Insert( 0, emptyPerson );
                    ddlEntities.DataSource = people;
                    ddlEntities.DataBind();
                }
            }
        }
    }
}