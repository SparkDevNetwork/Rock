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
using System.Web.UI;
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
    /// Block for displaying people that have been suggested to current person to follow.
    /// </summary>
    [DisplayName( "Person Suggestion List" )]
    [Category( "Follow" )]
    [Description( "Block for displaying people that have been suggested to current person to follow." )]

    [Rock.SystemGuid.BlockTypeGuid( "3726D4D5-EAA4-4AB7-A2BC-DDE8199E16FA" )]
    public partial class PersonSuggestionList : RockBlock, ICustomGridColumns
    {

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            this.BlockUpdated += Block_BlockUpdated;
            this.AddConfigurationUpdateTrigger( upnlContent );

            base.OnInit( e );

            gSuggestions.DataKeyNames = new string[] { "Id" };
            gSuggestions.IsDeleteEnabled = false;
            gSuggestions.GridRebind += gSuggestions_GridRebind;
            gSuggestions.PersonIdField = "Person.Id";

            var lbFollow = new LinkButton();
            lbFollow.ID = "lbFollow";
            lbFollow.CssClass = "btn btn-primary btn-sm pull-left margin-v-sm";
            lbFollow.Text = "<i class='fa fa-flag'></i> Follow";
            lbFollow.Click += lbFollow_Click;
            gSuggestions.Actions.AddCustomActionControl( lbFollow );

            var lbIgnore = new LinkButton();
            lbIgnore.ID = "lbIgnore";
            lbIgnore.CssClass = "btn btn-default btn-sm pull-left margin-v-sm js-ignore";
            lbIgnore.Text = "<i class='fa fa-flag-o'></i> Ignore";
            lbIgnore.Click += lbIgnore_Click;
            gSuggestions.Actions.AddCustomActionControl( lbIgnore );

            string ignoreScript = @"
    $('a.js-ignore').on('click', function( e ){
        e.preventDefault();
        Rock.dialogs.confirm('Are you sure you want to ignore the selected people?', function (result) {
            if (result) {
                window.location = e.target.href ? e.target.href : e.target.parentElement.href;
            }
        });
    });
";
            ScriptManager.RegisterStartupScript( gSuggestions, gSuggestions.GetType(), "ignoreScript", ignoreScript, true );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }

            base.OnLoad( e );
        }

        #endregion

        #region Events

        /// <summary>
        /// Handles the Click event of the lbUnfollow control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        void lbFollow_Click( object sender, EventArgs e )
        {
            // Get the suggestion ids that were selected
            var itemsSelected = new List<int>();
            gSuggestions.SelectedKeys.ToList().ForEach( f => itemsSelected.Add( f.ToString().AsInteger() ) );

            // Get the personAlias entity type
            var personAliasEntityType = EntityTypeCache.Get( typeof( Rock.Model.PersonAlias ));

            // If we have a valid current person and items were selected
            if ( personAliasEntityType != null && CurrentPersonAliasId.HasValue && itemsSelected.Any() )
            {
                using ( var rockContext = new RockContext() )
                {

                    // Get all the person alias id's that were selected
                    var followingSuggestedService = new FollowingSuggestedService( rockContext );
                    var selectedPersonAliasIds = followingSuggestedService
                        .Queryable()
                        .Where( f => itemsSelected.Contains( f.Id ) )
                        .Select( f => f.EntityId )
                        .Distinct()
                        .ToList();

                    // Find any of the selected person alias ids that current person is already following
                    var followingService = new FollowingService( rockContext );
                    var alreadyFollowing = followingService
                        .Queryable()
                        .Where( f =>
                            f.EntityTypeId == personAliasEntityType.Id &&
                            f.PersonAliasId == CurrentPersonAliasId.Value &&
                            string.IsNullOrEmpty( f.PurposeKey ) &&
                            selectedPersonAliasIds.Contains( f.EntityId ) )
                        .Select( f => f.EntityId )
                        .Distinct()
                        .ToList();

                    // For each selected person alias id that the current person is not already following
                    foreach ( var personAliasId in selectedPersonAliasIds.Where( p => !alreadyFollowing.Contains( p ) ) )
                    {
                        // Add a following record
                        var following = new Following();
                        following.EntityTypeId = personAliasEntityType.Id;
                        following.EntityId = personAliasId;
                        following.PersonAliasId = CurrentPersonAliasId.Value;
                        followingService.Add( following );
                    }

                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        void lbIgnore_Click( object sender, EventArgs e )
        {
            // Get the suggestion ids that were selected
            var itemsSelected = new List<int>();
            gSuggestions.SelectedKeys.ToList().ForEach( f => itemsSelected.Add( f.ToString().AsInteger() ) );

            // If any items were selected
            if ( itemsSelected.Any() )
            {
                using ( var rockContext = new RockContext() )
                {
                    // Update the status of each suggestion to be ignored
                    var followingSuggestedService = new FollowingSuggestedService( rockContext );
                    foreach ( var suggestion in followingSuggestedService.Queryable()
                        .Where( f => itemsSelected.Contains( f.Id ) ) )
                    {
                        suggestion.Status = FollowingSuggestedStatus.Ignored;
                    }

                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }
        /// <summary>
        /// Handles the GridRebind event of the gSuggestions control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gSuggestions_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
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
            if ( cbIncludeIgnored.Checked )
            {
                gfFilter.SetFilterPreference( FilterSetting.IncludeIgnored, cbIncludeIgnored.Checked.ToString() );
            }
            else
            {
                // Set to string.Empty instead of "false" to prevent the filter from showing up as enabled.
                gfFilter.SetFilterPreference( FilterSetting.IncludeIgnored, string.Empty );
            }

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds data to the filter controls.
        /// </summary>
        private void BindFilter()
        {
            cbIncludeIgnored.Checked = gfFilter.GetFilterPreference( FilterSetting.IncludeIgnored ).AsBoolean();
        }


        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var personAliasEntityType = EntityTypeCache.Get( "Rock.Model.PersonAlias" );
            if ( personAliasEntityType != null && CurrentPersonAlias != null )
            {
                var rockContext = new RockContext();

                // PersonAlias query for joining the followed entity id to
                var personAliasQry = new PersonAliasService( rockContext ).Queryable();

                // Get all the people that the current person currently follows
                var followedPersonIds = new FollowingService( rockContext ).Queryable()
                    .Where( f =>
                        f.EntityTypeId == personAliasEntityType.Id &&
                        string.IsNullOrEmpty( f.PurposeKey ) &&
                        f.PersonAliasId == CurrentPersonAlias.Id )
                    .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => p.PersonId )
                    .Distinct();

                // Get all the person suggestions for the current person that they are not already following
                var qry = new FollowingSuggestedService( rockContext )
                    .Queryable( "SuggestionType" )
                    .Where( s => cbIncludeIgnored.Checked || s.Status != FollowingSuggestedStatus.Ignored )
                    .Where( s =>
                        s.SuggestionType != null &&
                        s.EntityTypeId == personAliasEntityType.Id &&
                        s.PersonAliasId == CurrentPersonAlias.Id )
                    .Join( personAliasQry, s => s.EntityId, p => p.Id, ( s, p ) => new { s, p } )
                    .Where( j => !followedPersonIds.Contains( j.p.PersonId ) )
                    .Select( j => new
                    {
                        j.s.Id,
                        j.s.LastPromotedDateTime,
                        j.s.StatusChangedDateTime,
                        j.s.SuggestionType.ReasonNote,
                        j.s.Status,
                        Person = j.p.Person,
                        LastName = j.p.Person.LastName,
                        NickName = j.p.Person.NickName
                    } );

                // Sort the result
                SortProperty sortProperty = gSuggestions.SortProperty;
                if ( sortProperty == null )
                {
                    sortProperty = new SortProperty( new GridViewSortEventArgs( "LastName,NickName", SortDirection.Ascending ) );
                }

                // Bind grid to the query
                gSuggestions.DataSource = qry.Sort( sortProperty )
                    .Select( s => new
                    {
                        s.Id,
                        s.LastPromotedDateTime,
                        s.StatusChangedDateTime,
                        s.ReasonNote,
                        s.Status,
                        StatusLabel = s.Status == FollowingSuggestedStatus.Ignored ?
                            "<span class='label label-warning'>Ignored</span>" :
                            "<span class='label label-success'>Suggested</span>",
                        s.Person,
                        s.LastName,
                        s.NickName
                    } )
                    .ToList();

                gSuggestions.DataBind();
            }
        }

        #endregion

        /// <summary>
        /// Constant like string-key-settings that are tied to user saved filter preferences.
        /// </summary>
        public static class FilterSetting
        {
            public static readonly string IncludeIgnored = "Include Ignored";
        }
    }
}