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
using System.Collections.Generic;
using System.Web.UI;
using church.ccv.Residency.Data;
using Rock;
using Rock.Model;
using Rock.Web;
using Rock.Web.UI;
using Rock.Data;
using Rock.Constants;

namespace RockWeb.Plugins.church_ccv.Residency
{
    [DisplayName( "Resident Name" )]
    [Category( "CCV > Residency" )]
    [Description( "Simple block used to add or view a Resident" )]

    public partial class PersonDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                ShowDetail( PageParameter( "PersonId" ).AsInteger(), PageParameter( "GroupId" ).AsInteger() );
            }
        }

        /// <summary>
        /// Returns breadcrumbs specific to the block that should be added to navigation
        /// based on the current page reference.  This function is called during the page's
        /// oninit to load any initial breadcrumbs
        /// </summary>
        /// <param name="pageReference">The page reference.</param>
        /// <returns></returns>
        public override List<BreadCrumb> GetBreadCrumbs( PageReference pageReference )
        {
            var breadCrumbs = new List<BreadCrumb>();

            int? personId = this.PageParameter( pageReference, "PersonId" ).AsInteger();
            if ( personId != null )
            {
                Person person = new PersonService( new Rock.Data.RockContext() ).Get( personId.Value );
                if ( person != null )
                {
                    breadCrumbs.Add( new BreadCrumb( person.FullName, pageReference ) );
                }
                else
                {
                    breadCrumbs.Add( new BreadCrumb( "Resident", pageReference ) );
                }
            }
            else
            {
                // don't show a breadcrumb if we don't have a pageparam to work with
            }

            return breadCrumbs;
        }

        /// <summary>
        /// Shows the readonly details.
        /// </summary>
        /// <param name="person">The person.</param>
        private void ShowReadonlyDetails( Person person )
        {
            lReadOnlyTitle.Text = person.ToString().FormatAsHtmlTitle();

            SetEditMode( false );
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the Click event of the btnCancel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void btnCancel_Click( object sender, EventArgs e )
        {
            SetEditMode( false );

            if ( hfPersonId.ValueAsInt().Equals( 0 ) )
            {
                // Cancelling on Add.  Return to Grid
                NavigateToParentPage();
            }
            else
            {
                // Cancelling on Edit.  Return to Details
                PersonService personService = new PersonService( new RockContext() );
                Person item = personService.Get( hfPersonId.ValueAsInt() );
                ShowReadonlyDetails( item );
            }
        }

        /// <summary>
        /// Handles the Click event of the btnEdit control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnEdit_Click( object sender, EventArgs e )
        {
            PersonService personService = new PersonService( new RockContext() );
            Person item = personService.Get( hfPersonId.ValueAsInt() );
            ShowEditDetails( item );
        }

        /// <summary>
        /// Sets the edit mode.
        /// </summary>
        /// <param name="editable">if set to <c>true</c> [editable].</param>
        private void SetEditMode( bool editable )
        {
            pnlEditDetails.Visible = editable;

            HideSecondaryBlocks( editable );
        }

        /// <summary>
        /// Handles the Click event of the btnSave control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void btnSave_Click( object sender, EventArgs e )
        {
            if ( !ppPerson.PersonId.HasValue )
            {
                // controls will render error messages
                return;
            }

            // NOTE: this block only saves when adding a record
            var rockContext = new RockContext(); GroupMemberService groupMemberService = new GroupMemberService( rockContext );
            int groupId = hfGroupId.ValueAsInt();
            Group group = new GroupService( rockContext ).Get( groupId );
            int? defaultGroupRoleId = group.GroupType.DefaultGroupRoleId;

            // check to see if the person is alread a member of the gorup/role
            var existingGroupMember = groupMemberService.GetByGroupIdAndPersonIdAndGroupRoleId(
                groupId, ppPerson.SelectedValue ?? 0, defaultGroupRoleId ?? 0 );

            if ( existingGroupMember != null )
            {
                // person already in group, show warning message
                var person = new PersonService( rockContext ).Get( ppPerson.PersonId.Value );
                nbWarning.Title = "Resident already added";
                nbWarning.Text = string.Format( "{0} already is in the group {1}", person, group );
                return;
            }

            GroupMember groupMember = new GroupMember();
            groupMember.GroupId = group.Id;

            groupMember.PersonId = ppPerson.PersonId.Value;
            groupMember.GroupRoleId = group.GroupType.DefaultGroupRoleId ?? 0;
            groupMemberService.Add( groupMember );
            rockContext.SaveChanges();

            var qryParams = new Dictionary<string, string>();
            qryParams["PersonId"] = groupMember.PersonId.ToString();
            qryParams["GroupId"] = groupMember.GroupId.ToString();
            NavigateToPage( this.RockPage.Guid, qryParams );
        }

        /// <summary>
        /// Shows the detail.
        /// </summary>
        /// <param name="personId">The person identifier.</param>
        /// <param name="groupId">The group identifier.</param>
        public void ShowDetail( int personId, int groupId )
        {

            pnlDetails.Visible = true;

            // Load depending on Add(0) or Edit
            Person person = null;
            var rockContext = new RockContext();

            if ( !personId.Equals( 0 ) )
            {
                person = new PersonService( rockContext ).Get( personId );
            }
            
            if ( person == null )
            {
                person = new Person { Id = 0 };
            }

            hfPersonId.Value = person.Id.ToString();
            hfGroupId.Value = groupId.ToString();

            if ( person.Id > 0 )
            {
                ShowReadonlyDetails( person );
            }
            else
            {
                ShowEditDetails( person );
            }
        }

        /// <summary>
        /// Shows the edit details.
        /// </summary>
        /// <param name="person">The residency period.</param>
        private void ShowEditDetails( Person person )
        {
            if ( person.Id == 0 )
            {
                lReadOnlyTitle.Text = ActionTitle.Add( Person.FriendlyTypeName ).FormatAsHtmlTitle();
            }
            else
            {
                lReadOnlyTitle.Text = person.ToString().FormatAsHtmlTitle();
            }

            SetEditMode( true );

            ppPerson.SetValue( person );
        }

        #endregion
    }
}