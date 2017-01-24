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
using System.ComponentModel;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Finance
{
    /// <summary>
    /// Block that can be used to set the Person context to either the current person or one of the current person's businesses
    /// </summary>
    [DisplayName( "Giving Type Context Setter" )]
    [Category( "Finance" )]
    [Description( "Block that can be used to set the Person context to either the current person or one of the current person's businesses." )]
    public partial class GivingTypeContextPicker : RockBlock
    {
        #region Base Control Methods

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
        /// Loads the person and person's businesses.
        /// </summary>
        private void LoadDropDowns()
        {
            if ( CurrentPersonId.HasValue )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var personAndBusinesses = personService.GetBusinesses( CurrentPersonId.Value ).ToList();

                // only show the ContextPicker if the person has businesses
                this.Visible = personAndBusinesses.Any();

                personAndBusinesses.Insert( 0, this.CurrentPerson );

                var currentPersonOrGroup = RockPage.GetCurrentContext( EntityTypeCache.Read<Rock.Model.Person>() );
                if ( currentPersonOrGroup != null )
                {
                    // make sure currentPersonOrGroup is either the currentperson or one of their businesses
                    if ( !personAndBusinesses.Any( a => a.Id == currentPersonOrGroup.Id ) )
                    {
                        currentPersonOrGroup = null;
                    }
                }

                currentPersonOrGroup = currentPersonOrGroup ?? this.CurrentPerson;

                var pickerList = personAndBusinesses.Select( a => new
                {
                    Id = a.Id,
                    Name = a.FullName,
                    ButtonClass = ( a.Id == currentPersonOrGroup.Id ) ? "btn btn-xs btn-primary" : "btn btn-xs btn-default"
                } ).ToList();

                rptGivingTypes.DataSource = pickerList;
                rptGivingTypes.DataBind();
            }
        }

        /// <summary>
        /// Sets the giving type context.
        /// </summary>
        /// <param name="personOrBusinessId">The person or business identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Person SetGivingTypeContext( int personOrBusinessId, bool refreshPage = false )
        {
            var personOrBusiness = new PersonService( new RockContext() ).Get( personOrBusinessId );
            if ( personOrBusiness == null )
            {
                personOrBusiness = this.CurrentPerson;
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( personOrBusiness, true, false );

            if ( refreshPage )
            {
                Response.Redirect( Request.RawUrl, false );
                Context.ApplicationInstance.CompleteRequest();
            }

            return personOrBusiness;
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptGivingTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptGivingTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var personOrBusinessId = e.CommandArgument.ToString();

            if ( personOrBusinessId != null )
            {
                SetGivingTypeContext( personOrBusinessId.AsInteger(), true );
            }
        }

        #endregion
    }
}