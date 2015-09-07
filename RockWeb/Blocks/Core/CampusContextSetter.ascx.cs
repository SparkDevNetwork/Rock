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
using System.Linq;
using System.Web;
using System.Web.UI;
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
    /// Block that can be used to set the default campus context for the site
    /// </summary>
    [DisplayName( "Campus Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default campus context for the site." )]

    [CustomRadioListField("Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 0)]
    [TextField( "Current Item Template", "Lava template for the current item. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 1)]
    [TextField( "Dropdown Item Template", "Lava template for items in the dropdown. The only merge field is {{ CampusName }}.", true, "{{ CampusName }}", order: 1 )]
    [TextField( "No Campus Text", "The text to show when there is no campus in the context.", true, "Select Campus", order: 2)]
    public partial class CampusContextSetter : RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            if ( !Page.IsPostBack )
            {
                LoadDropDowns();
            }
        }

        /// <summary>
        /// Loads the drop downs.
        /// </summary>
        private void LoadDropDowns()
        {
            Dictionary<string, object> mergeObjects = new Dictionary<string, object>();
            
            var campusEntityType = EntityTypeCache.Read( "Rock.Model.Campus" );
            var defaultCampus = RockPage.GetCurrentContext( campusEntityType ) as Campus;

            if ( defaultCampus != null )
            {
                mergeObjects.Add( "CampusName", defaultCampus.Name );
                lCurrentSelection.Text = GetAttributeValue( "CurrentItemTemplate" ).ResolveMergeFields( mergeObjects );
            }
            else
            {
                lCurrentSelection.Text = GetAttributeValue( "NoCampusText" );
            }

            var campuses = CampusCache.All()
                .Select( a => new CampusItem { Name = a.Name, Id = a.Id } )
                .ToList();

            var formattedCampuses = new Dictionary<int, string>();
            // run lava on each campus
            foreach ( var campus in campuses )
            {
                mergeObjects.Clear();
                mergeObjects.Add( "CampusName", campus.Name );
                campus.Name = GetAttributeValue( "DropdownItemTemplate" ).ResolveMergeFields( mergeObjects );
            }

            rptCampuses.DataSource = campuses;
            rptCampuses.DataBind();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Handles the ItemCommand event of the rptCampuses control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCampuses_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            bool pageScope = GetAttributeValue( "ContextScope" ) == "Page";
            var campus = new CampusService( new RockContext() ).Get( e.CommandArgument.ToString().AsInteger() );
            if ( campus != null )
            {
                RockPage.SetContextCookie( campus, pageScope, true );
            }
        }

        #endregion

        /// <summary>
        /// Campus Item
        /// </summary>
        public class CampusItem {
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