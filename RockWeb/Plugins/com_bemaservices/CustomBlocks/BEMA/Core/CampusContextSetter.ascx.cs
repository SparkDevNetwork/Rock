﻿// <copyright>
// Copyright by BEMA Software Services
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

/*
 * BEMA Modified Core Block ( v10.3.1)
 * Version Number based off of RockVersion.RockHotFixVersion.BemaFeatureVersion
 * 
 * Additional Features:
 * - FE1) Added Ability to limit campuses shown based off of a campus attribute
 */
namespace RockWeb.Plugins.com_bemaservices.Core
{
    /// <summary>
    /// Block that can be used to set the default campus context for the site or page
    /// </summary>
    [DisplayName( "Campus Context Setter" )]
    [Category( "BEMA Services > Core" )]
    [Description( "Block that can be used to set the default campus context for the site or page." )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 0 )]
    [TextField( "Current Item Template", "Lava template for the current item. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 1 )]
    [TextField( "Dropdown Item Template", "Lava template for items in the dropdown. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 2 )]
    [TextField( "No Campus Text", "The text displayed when no campus context is selected.", true, "Select Campus", order: 3 )]
    [TextField( "Clear Selection Text", "The text displayed when a campus can be unselected. This will not display when the text is empty.", false, "", order: 4 )]
    [BooleanField( "Display Query Strings", "Select to always display query strings. Default behavior will only display the query string when it's passed to the page.", false, "", order: 5 )]
    [BooleanField( "Include Inactive Campuses", "Should inactive campuses be listed as well?", false, "", order: 6 )]
    [BooleanField( "Default To Current User's Campus", "Will use the campus of the current user if no context is provided.", order: 7, key: "DefaultToCurrentUser" )]
    [CustomDropdownListField( "Alignment", "Determines the alignment of the dropdown.", "1^Left,2^Right", true, "1", order: 8 )]
    
     /* BEMA.FE1.Start */
      [AttributeField( "Display Campus Boolean Attribute",
        Key = BemaAttributeKey.DisplayCampusBooleanAttribute,
        EntityTypeGuid = "00096BED-9587-415E-8AD4-4E076AE8FBF0",
        Description = "If you want to further narrow down the displayed campuses using a boolean attribute, set it here",
        IsRequired = false,
        Order = 4,
        Category = "BEMA Additional Features" )]
    /* BEMA.FE1.End */

    public partial class CampusContextSetter : RockBlock
    {
         /* BEMA.Start */
        #region Bema Attribute Keys
        private static class BemaAttributeKey
        {

            public const string DisplayCampusBooleanAttribute = "DisplayCampusBooleanAttribute";
           
        }

        #endregion
        /* BEMA.End */

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

            if ( GetAttributeValue( "Alignment" ) == "2" )
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

            var campusIdString = Request.QueryString["campusId"];
            if ( campusIdString != null )
            {
                var campusId = campusIdString.AsInteger();

                // if there is a query parameter, ensure that the Campus Context cookie is set (and has an updated expiration)
                // note, the Campus Context might already match due to the query parameter, but has a different cookie context, so we still need to ensure the cookie context is updated
                currentCampus = SetCampusContext( campusId, false );
            }

            // if the currentCampus isn't determined yet, and DefaultToCurrentUser, and the person has a CampusId, use that as the campus context
            if ( currentCampus == null && GetAttributeValue( "DefaultToCurrentUser" ).AsBoolean() && CurrentPerson != null )
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
                lCurrentSelection.Text = GetAttributeValue( "CurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentSelection.Text = GetAttributeValue( "NoCampusText" );
            }

            bool includeInactive = GetAttributeValue( "IncludeInactiveCampuses" ).AsBoolean();
            var campusList = CampusCache.All( includeInactive )
                .Select( a => new CampusItem { Name = a.Name, Id = a.Id } )
                .ToList();

            /* BEMA.FE1.Start */
            var campusAttributeGuid = GetAttributeValue( BemaAttributeKey.DisplayCampusBooleanAttribute ).AsGuidOrNull();
            if ( campusAttributeGuid.HasValue )
            {
                var campusAttribute = AttributeCache.Get( campusAttributeGuid.Value );
                if ( campusAttribute != null )
                {
                     campusList = CampusCache.All( includeInactive )
                        .Where( c => c.GetAttributeValue( campusAttribute.Key ).AsBoolean() )
                        .Select( a => new CampusItem { Name = a.Name, Id = a.Id } )
                        .ToList();
                }
            }
            /* BEMA.FE1.End */

            // run lava on each campus
            string dropdownItemTemplate = GetAttributeValue( "DropdownItemTemplate" );
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
            if ( !string.IsNullOrEmpty( GetAttributeValue( "ClearSelectionText" ) ) )
            {
                var blankCampus = new CampusItem
                {
                    Name = GetAttributeValue( "ClearSelectionText" ),
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
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var campus = new CampusService( new RockContext() ).Get( campusId );
            if ( campus == null )
            {
                // clear the current campus context
                campus = new Campus()
                {
                    Name = GetAttributeValue( "NoCampusText" ),
                    Guid = Guid.Empty
                };
            }

            // set context and refresh below with the correct query string if needed
            RockPage.SetContextCookie( campus, pageScope, false );

            // Only redirect if refreshPage is true
            if ( refreshPage )
            {
                if ( !string.IsNullOrWhiteSpace( PageParameter( "campusId" ) ) || GetAttributeValue( "DisplayQueryStrings" ).AsBoolean() )
                {
                    var queryString = HttpUtility.ParseQueryString( Request.QueryString.ToStringSafe() );
                    queryString.Set( "campusId", campusId.ToString() );
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