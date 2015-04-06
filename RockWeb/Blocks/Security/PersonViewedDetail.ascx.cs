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
using System.Web.UI;
using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for displaying details about who viewed your profile or whose profile you viewed. Details include when the profile was viewed and the source of the view.
    /// </summary>
    [DisplayName( "Person Viewed Detail" )]
    [Category( "Security" )]
    [Description( "Block for displaying details about who viewed your profile or whose profile you viewed. Details include when the profile was viewed and the source of the view." )]
    public partial class PersonViewedDetail : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gViewDetails.DataKeyNames = new string[] { "Id" };
            gViewDetails.GridRebind += gViewDetails_GridRebind;
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
                BindGrid();
            }
        }

        #endregion

        #region Edit Events

        /// <summary>
        /// Handles the GridRebind event of the gViewDetails control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gViewDetails_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        public void BindGrid()
        {
            int targetId = int.Parse( PageParameter( "targetId" ) );
            int viewerId = int.Parse( PageParameter( "viewerId" ) );
            bool viewedBy = Convert.ToBoolean( PageParameter( "viewedBy" ) );
            var personViewedService = new PersonViewedService( new RockContext() );
            var personViewedList = personViewedService.Queryable()
                .Where( p =>
                    p.ViewerPersonAlias != null &&
                    p.ViewerPersonAlias.PersonId == viewerId &&
                    p.TargetPersonAlias != null &&
                    p.TargetPersonAlias.PersonId == targetId )
                .Select( p => new
                {
                    Id = p.TargetPersonAlias.PersonId,
                    Source = p.Source,
                    TargetPerson = p.TargetPersonAlias.Person,
                    ViewerPerson = p.ViewerPersonAlias.Person,
                    ViewDateTime = p.ViewDateTime,
                    IpAddress = p.IpAddress
                } ).ToList();

            if ( viewedBy )
            {
                gridTitle.InnerText = string.Format( 
                    "{0} Viewed By {1}",
                    personViewedList.Select( p => p.TargetPerson.FullName ).FirstOrDefault(),
                    personViewedList.Select( p => p.ViewerPerson.FullName ).FirstOrDefault() );
            }
            else
            {
                gridTitle.InnerText = string.Format( 
                    "{0} Viewed {1}",
                    personViewedList.Select( p => p.ViewerPerson.FullName ).FirstOrDefault(),
                    personViewedList.Select( p => p.TargetPerson.FullName ).FirstOrDefault() );
            }

            SortProperty sortProperty = gViewDetails.SortProperty;
            if ( sortProperty != null )
            {
                personViewedList = personViewedList.AsQueryable().Sort( sortProperty ).ToList();
            }
            else
            {
                personViewedList = personViewedList.OrderByDescending( p => p.ViewDateTime ).ToList();
            }

            gViewDetails.EntityTypeId = EntityTypeCache.Read<PersonViewed>().Id;
            gViewDetails.DataSource = personViewedList;
            gViewDetails.DataBind();
        }

        #endregion
    }
}