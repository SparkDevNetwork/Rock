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
using System.Data.Entity;
using System.Linq;
using System.Web.UI.WebControls;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Follow
{
    /// <summary>
    /// Block for displaying a button and count of suggested people that can be used to navigate to person suggestion list block.
    /// </summary>
    [DisplayName( "Person Suggestion Notice" )]
    [Category( "Follow" )]
    [Description( "Block for displaying a button and count of suggested people that can be used to navigate to person suggestion list block." )]
    [LinkedPage( "List Page" )]
    public partial class PersonSuggestionNotice : RockBlock
    {
        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                int count = GetCount();
                if ( count <= 0 )
                {
                    this.Visible = false;
                }
                else
                {
                    lbSuggestions.Text = string.Format( "Following Suggestions <span class='badge'>{0:N0}</span>", count );
                    this.Visible = true;
                }
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbSuggestions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void lbSuggestions_Click( object sender, EventArgs e )
        {
            NavigateToLinkedPage( "ListPage" );
        }
        #endregion

        #region Internal Methods

        /// <summary>
        /// Gets the data.
        /// </summary>
        private int GetCount()
        {
            var personAliasEntityType = EntityTypeCache.Read( "Rock.Model.PersonAlias" );
            if ( personAliasEntityType != null && CurrentPersonAlias != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    // PersonAlias query for joining the followed entity id to
                    var personAliasQry = new PersonAliasService( rockContext )
                        .Queryable().AsNoTracking();

                    // Get all the people that the current person currently follows
                    var followedPersonIds = new FollowingService( rockContext )
                        .Queryable().AsNoTracking()
                        .Where( f =>
                            f.EntityTypeId == personAliasEntityType.Id &&
                            f.PersonAliasId == CurrentPersonAlias.Id )
                        .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => p.PersonId )
                        .Distinct();

                    // Get all the person suggestions for the current person that they are not already following
                    return new FollowingSuggestedService( rockContext )
                        .Queryable( "SuggestionType" ).AsNoTracking()
                        .Where( s =>
                            s.SuggestionType != null &&
                            s.EntityTypeId == personAliasEntityType.Id &&
                            s.PersonAliasId == CurrentPersonAlias.Id &&
                            s.Status != FollowingSuggestedStatus.Ignored )
                        .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => new { s, p } )
                        .Where( j => !followedPersonIds.Contains( j.p.PersonId ) )
                        .Count();
                }
            }

            return 0;
        }

        #endregion

}
}