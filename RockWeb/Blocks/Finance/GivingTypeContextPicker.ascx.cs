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
using Rock.Attribute;
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

    [CustomDropdownListField(
        "Display Type",
        Description = "Determines how the picker options are displayed, either in a dropdown or as buttons.",
        ListSource = "buttons^Buttons,dropDown^Dropdown",
        IsRequired = false,
        DefaultValue = "buttons",
        Order = 0,
        Key = AttributeKey.DisplayType
        )]

    [Rock.SystemGuid.BlockTypeGuid( "57B00D03-1CDC-4492-95CF-7BD127CE61F0" )]
    public partial class GivingTypeContextPicker : RockBlock
    {
        #region Keys

        private static class AttributeKey
        {
            public const string DisplayType = "DisplayType";
        }

        private static class DisplayTypeKey
        {
            public const string Buttons = "buttons";
            public const string DropDown = "dropDown";
        }

        #endregion

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
            if ( !Page.IsPostBack )
            {
                LoadContextOptions();
            }

            base.OnLoad( e );
        }

        /// <summary>
        /// Handles the BlockUpdated event of the Block control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadContextOptions();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the context options.
        /// </summary>
        private void LoadContextOptions()
        {
            if ( CurrentPersonId.HasValue )
            {
                var rockContext = new RockContext();
                var personService = new PersonService( rockContext );
                var personAndBusinesses = personService.GetBusinesses( CurrentPersonId.Value ).ToList();
                var displayType = GetAttributeValue( AttributeKey.DisplayType );

                btnGivingTypes.Visible = displayType == DisplayTypeKey.Buttons;
                ddlGivingTypes.Visible = displayType == DisplayTypeKey.DropDown;

                // only show the ContextPicker if the person has businesses
                this.Visible = personAndBusinesses.Any();

                personAndBusinesses.Insert( 0, this.CurrentPerson );

                var currentPersonOrBusiness = RockPage.GetCurrentContext( EntityTypeCache.Get<Rock.Model.Person>() );

                if ( currentPersonOrBusiness != null )
                {
                    // make sure currentPersonOrGroup is either the currentperson or one of their businesses
                    if ( !personAndBusinesses.Any( a => a.Id == currentPersonOrBusiness.Id ) )
                    {
                        currentPersonOrBusiness = null;
                    }
                }

                if ( currentPersonOrBusiness == null )
                {
                    // person or business not set (or was set to somebody else), so set it the current person
                    SetGivingTypeContext( this.CurrentPersonId.Value, true );
                }
                else
                {
                    if ( displayType == DisplayTypeKey.Buttons )
                    {
                        var pickerList = personAndBusinesses.Select( a => new
                        {
                            Id = a.Id,
                            Name = a.FullName,
                            ButtonClass = ( a.Id == currentPersonOrBusiness.Id ) ? "btn btn-xs btn-primary" : "btn btn-xs btn-default"
                        } ).ToList();

                        rptGivingTypes.DataSource = pickerList;
                        rptGivingTypes.DataBind();
                    }
                    else
                    {
                        var dropdownList = personAndBusinesses.Select( a => new ListItem
                        {
                            Text = a.FullName,
                            Value = a.Id.ToString(),
                        } ).ToArray();

                        ddlGivingTypes.Items.AddRange( dropdownList );
                        ddlGivingTypes.SetValue( currentPersonOrBusiness.Id );
                    }
                }
            }
            else
            {
                // nobody logged in, so make sure the Person Context is cleared out
                RockPage.ClearContextCookie( typeof( Rock.Model.Person ), true, true );
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

        /// <summary>
        /// Handles the SelectedIndexChanged event of the ddlGivingTypes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void ddlGivingTypes_SelectedIndexChanged( object sender, EventArgs e )
        {
            var personOrBusinessId = ddlGivingTypes.SelectedValueAsInt();
            if ( personOrBusinessId.HasValue )
            {
                SetGivingTypeContext( personOrBusinessId.Value, true );
            }
        }

        #endregion
    }
}