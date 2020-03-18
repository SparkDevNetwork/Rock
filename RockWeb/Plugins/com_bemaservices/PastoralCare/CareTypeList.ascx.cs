// <copyright>
// Copyright by BEMA Information Technologies
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
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;

using com.bemaservices.PastoralCare.Model;

namespace RockWeb.Plugins.com_bemaservices.PastoralCare
{
    /// <summary>
    /// Block to display the care types.
    /// </summary>
    [DisplayName( "Care Type List" )]
    [Category( "BEMA Services > Pastoral Care" )]
    [Description( "Block to display the care types." )]
    [LinkedPage( "Detail Page", "Page used to view details of a care type." )]
    [BooleanField( "Allow Shared Attributes", "Displays a link to a page for care item attributes shared across care types", false )]
    [LinkedPage( "Shared Attribute Page", "Page used to view shared care item attributes." )]

    public partial class CareTypeList : Rock.Web.UI.RockBlock
    {
        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            lbAddCareType.Visible = UserCanAdministrate;
            rptCareTypes.ItemCommand += rptCareTypes_ItemCommand;
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
                GetData();
            }
            var areSharedAttributesAllowed = GetAttributeValue( "AllowSharedAttributes" ).AsBoolean();
            liSharedAttributes.Visible = areSharedAttributesAllowed;
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the BlockUpdated event of the control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
            GetData();
        }

        /// <summary>
        /// Handles the ItemCommand event of the rptCareTypes control.
        /// </summary>
        /// <param name="source">The source of the event.</param>
        /// <param name="e">The <see cref="RepeaterCommandEventArgs"/> instance containing the event data.</param>
        protected void rptCareTypes_ItemCommand( object source, RepeaterCommandEventArgs e )
        {
            int? careTypeId = e.CommandArgument.ToString().AsIntegerOrNull();
            if ( careTypeId.HasValue )
            {
                NavigateToLinkedPage( "DetailPage", "CareTypeId", careTypeId.Value );
            }

            GetData();
        }

        /// <summary>
        /// Handles the Click event of the lbAddCareType control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbAddCareType_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "DetailPage", "CareTypeId", 0 );
        }
        #endregion

        #region Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        private void GetData()
        {
            using ( var rockContext = new RockContext() )
            {
                // Get all of the care types
                var allCareTypes = new CareTypeService( rockContext ).Queryable()
                    .OrderBy( w => w.Name )
                    .ToList();

                var authorizedCareTypes = new List<CareType>();
                foreach ( var careType in allCareTypes )
                {
                    if ( UserCanEdit || careType.IsAuthorized( Authorization.VIEW, CurrentPerson ) )
                    {
                        authorizedCareTypes.Add( careType );
                    }
                }

                rptCareTypes.DataSource = authorizedCareTypes.ToList();
                rptCareTypes.DataBind();
            }
        }

        #endregion

        protected void lbSharedAttributes_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "SharedAttributePage" );
        }
    }
}