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
using System.Web.UI;
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Security
{
    /// <summary>
    /// Block for displaying people who have viewed this person's profile and whose profile's this person has viewed. A block level attribute determines which view is displayed.
    /// </summary>
    [DisplayName( "Person Viewed" )]
    [Category( "Security" )]
    [Description( "Block for displaying people who have viewed this person's profile and whose profile's this person has viewed. A block level attribute determines which view is displayed." )]

    [BooleanField( "See Profiles Viewed", "Flag indicating whether this block will show you a list of people this person has viewed or a list of people who have viewed this person (this is the default).", false )]
    [ContextAware( typeof( Person ) )]
    [LinkedPage( "Detail Page" )]
    public partial class PersonViewedSummary : RockBlock
    {
        #region Fields

        private int? personId = null;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            gViewed.DataKeyNames = new string[] { "Id" };
            gViewedBy.DataKeyNames = new string[] { "Id" };
            var person = ContextEntity<Person>();
            if ( person != null )
            {
                personId = person.Id;
            }

            gViewed.GridRebind += gViewed_GridRebind;
            gViewedBy.GridRebind += gViewedBy_GridRebind;
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
        /// Handles the GridRebind event of the gViewed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gViewed_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gViewedBy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gViewedBy_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the RowSelected event of the gViewed control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gViewed_RowSelected( object sender, RowEventArgs e )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "targetId", e.RowKeyValue.ToString() );
            queryParams.Add( "viewerId", personId.ToString() );
            queryParams.Add( "viewedBy", "false" );
            NavigateToLinkedPage( "DetailPage", queryParams );
        }

        /// <summary>
        /// Handles the RowSelected event of the gViewedBy control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void gViewedBy_RowSelected( object sender, RowEventArgs e )
        {
            Dictionary<string, string> queryParams = new Dictionary<string, string>();
            queryParams.Add( "viewerId", e.RowKeyValue.ToString() );
            queryParams.Add( "targetId", personId.ToString() );
            queryParams.Add( "viewedBy", "true" );
            NavigateToLinkedPage( "DetailPage", queryParams );
        }

        #endregion

        #region Internal Methods

        /// <summary>
        /// Binds the grid.
        /// </summary>
        protected void BindGrid()
        {
            var showProfilesViewed = GetAttributeValue( "SeeProfilesViewed" ).AsBoolean();
            var rockContext = new RockContext();

            var personViewedService = new PersonViewedService( rockContext );
            var personService = new PersonService( rockContext );
            if ( showProfilesViewed )
            {
                // This grid should show the profiles viewed by this person.
                pnlViewed.Visible = true;
                pnlViewedBy.Visible = false;

                if ( personId.HasValue )
                {
                    var viewedList = personViewedService.Queryable()
                        .Where( p => p.ViewerPersonAlias != null && p.ViewerPersonAlias.PersonId == personId )
                        .GroupBy( p => p.TargetPersonAlias.PersonId )
                        .Select( p => new
                        {
                            TargetPersonId = p.Key,
                            FirstViewed = p.Min( g => g.ViewDateTime ),
                            LastViewed = p.Max( g => g.ViewDateTime ),
                            ViewedCount = p.Count()
                        } );

                    var pQry = personService.Queryable();

                    var qry = viewedList
                        .Join( pQry, v => v.TargetPersonId, p => p.Id, ( v, p ) => new
                        {
                            p.Id,
                            FullName = p.NickName + " " + p.LastName,
                            p.BirthDate,
                            p.Gender,
                            FirstViewedDate = v.FirstViewed,
                            LastViewedDate = v.LastViewed,
                            ViewedCount = v.ViewedCount
                        } );

                    SortProperty sortProperty = gViewed.SortProperty;
                    if ( sortProperty != null )
                    {
                        qry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        qry = qry.OrderByDescending( q => q.LastViewedDate );
                    }

                    gViewed.EntityTypeId = EntityTypeCache.Read<PersonViewed>().Id;
                    gViewed.DataSource = qry.ToList();
                    gViewed.DataBind();
                }
            }
            else
            {
                // This grid should show the profiles that have viewed this person.
                pnlViewed.Visible = false;
                pnlViewedBy.Visible = true;

                if ( personId.HasValue )
                {
                    var viewedList = personViewedService.Queryable()
                        .Where( p => p.TargetPersonAlias != null && p.TargetPersonAlias.PersonId == personId )
                        .GroupBy( p => p.ViewerPersonAlias.PersonId )
                        .Select( p => new
                        {
                            ViewerPersonId = p.Key,
                            FirstViewed = p.Min( g => g.ViewDateTime ),
                            LastViewed = p.Max( g => g.ViewDateTime ),
                            ViewedCount = p.Count()
                        } );

                    var pQry = personService.Queryable();

                    var qry = viewedList
                        .Join( pQry, v => v.ViewerPersonId, p => p.Id, ( v, p ) => new
                        {
                            p.Id,
                            FullName = p.NickName + " " + p.LastName,
                            p.BirthDate,
                            p.Gender,
                            FirstViewedDate = v.FirstViewed,
                            LastViewedDate = v.LastViewed,
                            ViewedCount = v.ViewedCount
                        } );

                    SortProperty sortProperty = gViewedBy.SortProperty;
                    if ( sortProperty != null )
                    {
                        qry = qry.Sort( sortProperty );
                    }
                    else
                    {
                        qry = qry.OrderByDescending( q => q.LastViewedDate );
                    }

                    gViewedBy.EntityTypeId = EntityTypeCache.Read<Person>().Id;
                    gViewedBy.DataSource = qry.ToList();
                    gViewedBy.DataBind();
                }
            }
        }

        #endregion
    }
}