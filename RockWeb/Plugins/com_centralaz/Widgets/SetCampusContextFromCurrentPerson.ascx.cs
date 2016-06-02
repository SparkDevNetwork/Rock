// <copyright>
// Copyright by Central Christian Church
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

namespace RockWeb.Plugins.com_centralaz.Widgets
{
    /// <summary>
    /// Block that can be used to set the default campus context for the site
    /// </summary>
    [DisplayName( "Set Campus Context From Current Person" )]
    [Category( "com_centralaz > Widgets" )]
    [Description( "Block that can be used to set the default campus context for the site from the current person's campus." )]
    [CustomRadioListField( "Context Scope", "The scope of context to set", "Site,Page", true, "Site", order: 0 )]
    public partial class SetCampusContextFromCurrentPerson : RockBlock
    {
        #region Base Control Methods

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

            if ( !string.IsNullOrWhiteSpace( PageParameter( "campusId" ) ) )
            {
                SetCampusContext( PageParameter( "campusId" ).AsInteger() );
            }
            else if ( CurrentPerson != null )
            {
                if ( CurrentPerson.GetCampus() != null )
                {
                    SetCampusContext( CurrentPerson.GetCampus().Id );
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Sets the campus context.
        /// </summary>
        /// <param name="campusId">The campus identifier.</param>
        /// <param name="refreshPage">if set to <c>true</c> [refresh page].</param>
        /// <returns></returns>
        protected Campus SetCampusContext( int campusId, bool refreshPage = true )
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
                if ( !string.IsNullOrWhiteSpace( PageParameter( "campusId" ) ) )
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
    }
}