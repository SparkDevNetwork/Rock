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
using System.Collections.Generic;
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
    /// Block that can be used to set the default campus context for the site or page
    /// </summary>
    [DisplayName( "Campus Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default campus context for the site or page." )]
    [CustomRadioListField( "Context Scope",
        Description = "The scope of context to set",
        ListSource = "Site,Page",
        IsRequired = true,
        DefaultValue = "Site",
        Order = 0,
        Key = AttributeKey.ContextScope )]

    [TextField( "Current Item Template",
        Description = "Lava template for the current item. The only merge field is {{ CampusName }}.",
        IsRequired = true,
        DefaultValue = "{{ CampusName }}",
        Order = 1,
        Key = AttributeKey.CurrentItemTemplate )]

    [TextField( "Dropdown Item Template",
        Description = "Lava template for items in the dropdown. The only merge field is {{ CampusName }}.",
        IsRequired = true,
        DefaultValue = "{{ CampusName }}",
        Order = 2,
        Key = AttributeKey.DropdownItemTemplate )]

    [TextField( "No Campus Text",
        Description = "The text displayed when no campus context is selected.",
        IsRequired = true,
        DefaultValue = "Select Campus",
        Order = 3,
        Key = AttributeKey.NoCampusText )]

    [TextField( "Clear Selection Text",
        Description = "The text displayed when a campus can be unselected. This will not display when the text is empty.",
        IsRequired = false,
        Order = 4,
        Key = AttributeKey.ClearSelectionText )]

    [BooleanField( "Display Query Strings",
        Description = "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.",
        DefaultValue = "false",
        Order = 5,
        Key = AttributeKey.DisplayQueryStrings )]

    [BooleanField( "Include Inactive Campuses",
        Description = "Should inactive campuses be listed as well?",
        DefaultValue = "false",
        Order = 6,
        Key = AttributeKey.IncludeInactiveCampuses )]

    [BooleanField( "Default To Current User's Campus",
        Description = "Will use the campus of the current user if no context is provided.",
        Order = 7,
        Key = AttributeKey.DefaultToCurrentUser )]

    [CustomDropdownListField( "Alignment",
        Description = "Determines the alignment of the dropdown.",
        ListSource = "1^Left,2^Right",
        IsRequired = true,
        DefaultValue = "1",
        Order = 8,
        Key = AttributeKey.Alignment )]

    public partial class CampusContextSetter : RockBlock
    {
        public static class AttributeKey
        {
            public const string ContextScope = "ContextScope";
            public const string CurrentItemTemplate = "CurrentItemTemplate";
            public const string DropdownItemTemplate = "DropdownItemTemplate";
            public const string NoCampusText = "NoCampusText";
            public const string ClearSelectionText = "ClearSelectionText";
            public const string DisplayQueryStrings = "DisplayQueryStrings";
            public const string IncludeInactiveCampuses = "IncludeInactiveCampuses";
            public const string DefaultToCurrentUser = "DefaultToCurrentUser";
            public const string Alignment = "Alignment";
        }

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

            LoadDropdowns();

            if ( GetAttributeValue( AttributeKey.Alignment ) == "2" )
            {
                ulDropdownMenu.AddCssClass( "dropdown-menu-right" );
            }
        }

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            LoadDropdowns();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Loads the campuses
        /// </summary>
        protected void LoadDropdowns()
        {
            var campusEntityType = EntityTypeCache.Get( typeof( Campus ) );
            var currentCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

            var campusIdString = Request.QueryString["CampusId"];
            if ( campusIdString != null )
            {
                var campusId = campusIdString.AsInteger();

                // if there is a query parameter, ensure that the Campus Context cookie is set (and has an updated expiration)
                // note, the Campus Context might already match due to the query parameter, but has a different cookie context, so we still need to ensure the cookie context is updated
                currentCampus = SetCampusContext( campusId, false );
            }

            // if the currentCampus isn't determined yet, and DefaultToCurrentUser, and the person has a CampusId, use that as the campus context
            if ( currentCampus == null && GetAttributeValue( AttributeKey.DefaultToCurrentUser ).AsBoolean() && CurrentPerson != null )
            {
                var campusId = CurrentPerson.GetFamily().CampusId;
                if ( campusId.HasValue )
                {
                    currentCampus = SetCampusContext( campusId.Value, false );
                }
            }

            if ( currentCampus != null )
            {
                var mergeObjects = new Dictionary<string, object>();
                mergeObjects.Add( "CampusName", currentCampus.Name );
                lCurrentSelection.Text = GetAttributeValue( AttributeKey.CurrentItemTemplate ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentSelection.Text = GetAttributeValue( AttributeKey.NoCampusText );
            }

            bool includeInactive = GetAttributeValue( AttributeKey.IncludeInactiveCampuses ).AsBoolean();
            var campusList = CampusCache.All( includeInactive )
                .Select( a => new CampusItem { Name = a.Name, Id = a.Id } )
                .ToList();

            // run lava on each campus
            string dropdownItemTemplate = GetAttributeValue( AttributeKey.DropdownItemTemplate );
            if ( !string.IsNullOrWhiteSpace( dropdownItemTemplate ) )
            {
                foreach ( var campus in campusList )
                {
                    var mergeObjects = new Dictionary<string, object>();
                    mergeObjects.Add( "CampusName", campus.Name );
                    campus.Name = dropdownItemTemplate.ResolveMergeFields( mergeObjects );
                }
            }

            // check if the campus can be unselected
            if ( !string.IsNullOrEmpty( GetAttributeValue( AttributeKey.ClearSelectionText ) ) )
            {
                var blankCampus = new CampusItem
                {
                    Name = GetAttributeValue( AttributeKey.ClearSelectionText ),
                    Id = Rock.Constants.All.Id
                };

                campusList.Insert( 0, blankCampus );
            }

            rptCampuses.DataSource = campusList;
            rptCampuses.DataBind();
        }

        /// <summary>
        /// Sets the campus context.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Campus SetCampusContext( int campusId, bool refreshPage = false )
        {
            bool pageScope = GetAttributeValue( AttributeKey.ContextScope ) == "Page";
            var campus = new CampusService( new RockContext() ).Get( campusId );
            if ( campus == null )
            {
                // clear the current campus context
                campus = new Campus()
                {
                    Name = GetAttributeValue( AttributeKey.NoCampusText ),
                    Guid = Guid.Empty
                };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( campus, pageScope, false );

            // Only redirect if refreshPage is true
            if ( refreshPage )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "CampusId" ) ) || GetAttributeValue( AttributeKey.DisplayQueryStrings ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "CampusId", campusId.ToString() );
                    Response.Redirect( string.Format( "{0}?{1}", Request.Url.AbsolutePath, queryString ), false );
                }
                else
                {
                    Response.Redirect( Request.RawUrl, false );
                }

                Context.ApplicationInstance.CompleteRequest();
            }

            return campus;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the ItemCommand event of the rptCampuses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCampuses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            var campusId = e.CommandArgument.ToString();

            if ( campusId != null )
            {
                SetCampusContext( campusId.AsInteger(), true );
            }
        }

        #endregion

        /// <summary>
        /// Campus Item
        /// </summary>
        public class CampusItem
        {
            /// <summary>
            /// Gets or sets the name.
            /// </summary>
            /// <value>
            /// The name.
            /// </value>
            public string Name { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            /// <value>
            /// The identifier.
            /// </value>
            public int Id { get; set; }
        }
    }
}