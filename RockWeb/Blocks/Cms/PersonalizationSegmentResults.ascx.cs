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
using System.Web.UI;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI;

namespace RockWeb.Blocks.Cms
{
    [DisplayName( "Personalization Segment Results" )]
    [Category( "Cms" )]
    [Description( "Block that lists Known Individuals for a given Personalization Segment." )]

    [Rock.SystemGuid.BlockTypeGuid( "438432E3-22A8-43D9-9F06-179C3B65D298" )]
    public partial class PersonalizationSegmentResults : RockBlock
    {
        #region PageParameter Keys

        private static class PageParameterKey
        {
            public const string PersonalizationSegmentGuid = "PersonalizationSegmentGuid";
        }

        #endregion PageParameter Keys

        #region Private Variables

        private PersonalizationSegmentCache _personalizationSegment = null;

        #endregion

        #region UserPreferenceKeys

        /// <summary>
        /// Keys to use for UserPreferences
        /// </summary>
        protected static class UserPreferenceKey
        {
            /// <summary>
            /// The First Name
            /// </summary>
            public const string NickName = "Nick Name";

            /// <summary>
            /// The Last Name
            /// </summary>
            public const string LastName = "Last Name";
        }

        #endregion UserPreferanceKeys

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _personalizationSegment = GetPersonalizationSegment();
            gResults.Visible = _personalizationSegment != null;
            if ( _personalizationSegment != null )
            {
                gResults.DataKeyNames = new string[] { "Id" };
                gResults.Actions.ShowAdd = false;
                gResults.GridRebind += gResults_GridRebind;
                gResults.PersonIdField = "Id";

                gfFilter.ApplyFilterClick += gfFilter_ApplyFilterClick;
                gfFilter.ClearFilterClick += gfFilter_ClearFilterClick;

                lTitle.Text = ( "Known Individuals for " + _personalizationSegment.Name ).FormatAsHtmlTitle();
            }

            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
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

            if ( !Page.IsPostBack )
            {
                if ( _personalizationSegment != null )
                {
                    BindFilter();
                    BindGrid();
                }
                else
                {
                    pnlView.Visible = false;
                }
            }
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
            BindFilter();
            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gResults control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gResults_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            gfFilter.SetFilterPreference( UserPreferenceKey.NickName, tbNickName.Text );
            gfFilter.SetFilterPreference( UserPreferenceKey.LastName, tbLastName.Text );
            BindGrid();
        }

        /// <summary>
        /// Handles the ClearFilterClick event of the gfFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfFilter_ClearFilterClick( object sender, EventArgs e )
        {
            gfFilter.DeleteFilterPreferences();
            BindFilter();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbNickName.Text = gfFilter.GetFilterPreference( UserPreferenceKey.NickName );
            tbLastName.Text = gfFilter.GetFilterPreference( UserPreferenceKey.LastName );
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var rockContext = new RockContext();
            var personalizationSegmentService = new PersonalizationSegmentService( rockContext );

            var personAliasQry = personalizationSegmentService.GetPersonAliasPersonalizationSegmentQuery( _personalizationSegment ).Select( a => a.PersonAliasId );

            var qry = new PersonService( rockContext ).Queryable().Where( g => g.Aliases.Any( k => personAliasQry.Contains( k.Id ) ) );

            var firstNameFilter = tbNickName.Text;
            if ( !string.IsNullOrWhiteSpace( firstNameFilter ) )
            {
                qry = qry.Where( m =>
                    m.FirstName.StartsWith( firstNameFilter ) ||
                    m.NickName.StartsWith( firstNameFilter ) );
            }

            // Filter by Last Name
            string lastNameFilter = tbLastName.Text;
            if ( !string.IsNullOrWhiteSpace( lastNameFilter ) )
            {
                qry = qry.Where( m => m.LastName.StartsWith( lastNameFilter ) );
            }

            // sort the query based on the column that was selected to be sorted
            var sortProperty = gResults.SortProperty;
            if ( gResults.AllowSorting && sortProperty != null )
            {
                qry = qry.Sort( sortProperty );
            }
            else
            {
                qry = qry.OrderBy( p => p.LastName ).ThenBy( p => p.FirstName );
            }

            gResults.SetLinqDataSource( qry );
            gResults.DataBind();
        }

        /// <summary>
        /// Get the personalization segment
        /// </summary>
        /// <returns></returns>
        private PersonalizationSegmentCache GetPersonalizationSegment( RockContext rockContext = null )
        {
            var personalizationSegmentGuid = PageParameter( PageParameterKey.PersonalizationSegmentGuid ).AsGuidOrNull();
            if ( !personalizationSegmentGuid.HasValue )
            {
                return null;
            }

            return PersonalizationSegmentCache.Get( personalizationSegmentGuid.Value );
        }

        #endregion
    }
}