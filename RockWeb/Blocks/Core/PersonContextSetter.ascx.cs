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
using System.Linq;
using System.Web;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default person context for the site or page
    /// </summary>
    [DisplayName( "Person Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default person context for the site." )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Page", order: 1 )]
    [GroupField( "Group", "The Group to use as the source of people to select from", true, order: 2 )]
    [TextField( "No Person Text", "The text to show when there is no person in the context.", true, "Select Person", order: 3 )]
    [TextField( "Clear Selection Text", "The text displayed when a person can be unselected. This will not display when the text is empty.", true, "", order: 4 )]
    [BooleanField( "Display Query Strings", "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", false, "", order: 5 )]
    public partial class PersonContextSetter : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            // repaint the screen after block settings are updated
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            LoadDropDowns();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadDropDowns();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the list of people that are in the selected group
        /// </summary>
        private void LoadDropDowns()
        {
            var personEntityType = EntityTypeCache.Read<Person>();
            var currentPerson = RockPage.GetCurrentContext( personEntityType ) as Person;

            int? personIdParam = Request.QueryString["personId"].AsIntegerOrNull();

            // if a personId is in the page params, use that instead of the person context
            if ( personIdParam.HasValue )
            {
                if ( currentPerson == null || currentPerson.Id != personIdParam.Value )
                {
                    SetPersonContext( personIdParam.Value, false );
                }
            }

            RockContext rockContext = new RockContext();
            Group group = null;
            var groupGuid = this.GetAttributeValue( "Group" ).AsGuidOrNull();
            if ( groupGuid.HasValue )
            {
                group = new GroupService( rockContext ).Get( groupGuid.Value );
            }

            if ( group == null )
            {
                nbSelectGroupWarning.Visible = true;
            }
            else
            {
                nbSelectGroupWarning.Visible = false;

                var personQry = new GroupMemberService( rockContext ).Queryable().Where( a => a.GroupId == group.Id ).Select( a => a.Person ).Distinct();

                if ( currentPerson != null )
                {
                    // ensure that the current person is in the selected group
                    currentPerson = personQry.Where( a => a.Id == currentPerson.Id ).FirstOrDefault();
                }

                lCurrentSelection.Text = currentPerson != null ? currentPerson.ToString() : GetAttributeValue( "NoPersonText" );

                var personList = personQry.OrderBy( a => a.LastName ).ThenBy( a => a.NickName ).ToList().Select( a => new
                {
                    Name = a.FullName,
                    Id = a.Id
                } ).ToList();

                // check if the person can be unselected
                if ( !string.IsNullOrEmpty( GetAttributeValue( "ClearSelectionText" ) ) )
                {
                    personList.Insert( 0, new { Name = GetAttributeValue( "ClearSelectionText" ), Id = 0 } );
                }

                rptPersons.DataSource = personList;
                rptPersons.DataBind();
            }
        }

        /// <summary>
        /// Sets the group context.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Person SetPersonContext( int personId, bool refreshPage = false )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var person = new PersonService( new RockContext() ).Get( personId );
            if ( person == null )
            {
                person = new Person { LastName = this.GetAttributeValue( "NoPersonText" ), Guid = Guid.Empty };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( person, pageScope, false );

            if ( refreshPage )
            {
                // Only redirect if refreshPage is true
                if ( !string.IsNullOrWhiteSpace( PageParameter( "personId" ) ) || GetAttributeValue( "DisplayQueryStrings" ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "personId", personId.ToString() );
                    Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
                }
                else
                {
                    Response.Redirect( Request.RawUrl, false );
                }

                Context.ApplicationInstance.CompleteRequest();
            }

            return person;
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptGroups control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptPersons_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var personId = e.CommandArgument.ToString();

            if ( personId != null )
            {
                SetPersonContext( personId.AsInteger(), true );
            }
        }

        #endregion
    }
}