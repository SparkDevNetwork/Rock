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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block that can be used to set the default campus context for the site
    /// </summary>
    [DisplayName( "Campus Context Setter" )]
    [Category( "Core" )]
    [Description( "Block that can be used to set the default campus context for the site." )]
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
            var campusService = new CampusService( new RockContext() );
            Campus defaultCampus = null;

            // default campus to the whatever the context cookie has for it
            var contextCookie = Request.Cookies["Rock:context"];
            if ( contextCookie != null )
            {
                var cookieValue = contextCookie.Values[typeof( Rock.Model.Campus ).FullName];

                try
                {
                    string contextItem = Rock.Security.Encryption.DecryptString( cookieValue );
                    string[] contextItemParts = contextItem.Split( '|' );
                    if ( contextItemParts.Length == 2 )
                    {
                        defaultCampus = campusService.GetByPublicKey( contextItemParts[1] );
                    }
                }
                catch
                {
                    // don't set defaultCampus if cookie is corrupt
                }
            }

            lCurrentSelection.Text = defaultCampus != null ? defaultCampus.ToString() : "Select Campus";

            var campuses = campusService.Queryable().OrderBy( a => a.Name ).ToList().Select( a => new
            {
                a.Name,
                ContextKey = HttpUtility.UrlDecode(a.ContextKey)
            } ).ToList();

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
            var contextCookie = Request.Cookies["Rock:context"];
            if ( contextCookie == null )
            {
                contextCookie = new HttpCookie( "Rock:context" );
            }

            contextCookie.Values[typeof( Rock.Model.Campus ).FullName] = e.CommandArgument as string;
            contextCookie.Expires = RockDateTime.Now.AddYears( 1 );

            Response.Cookies.Add( contextCookie );

            // reload page to ensure that all blocks get the new context setting
            Response.Redirect( Request.RawUrl, false );
            Context.ApplicationInstance.CompleteRequest();
        }

        #endregion
    }
}