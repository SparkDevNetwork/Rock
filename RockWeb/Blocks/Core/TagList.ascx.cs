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
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for viewing list of tags
    /// </summary>
    [DisplayName( "Tag List" )]
    [Category( "Core" )]
    [Description( "Block for viewing a list of tags." )]

    [LinkedPage( "Detail Page" )]
    public partial class TagList : Rock.Web.UI.RockBlock
    {
        #region Fields

        private bool _canConfigure = false;

        #endregion

        #region Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            _canConfigure = IsUserAuthorized( Authorization.ADMINISTRATE );

            // Load Entity Type Filter
            new EntityTypeService( new RockContext() ).GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );

            ppOwner.Visible = _canConfigure;

            rFilter.ApplyFilterClick += rFilter_ApplyFilterClick;
            rFilter.DisplayFilterValue += rFilter_DisplayFilterValue;

            rGrid.DataKeyNames = new string[] { "Id" };
            rGrid.Actions.ShowAdd = true;
            rGrid.Actions.AddClick += rGrid_Add;
            rGrid.GridReorder += rGrid_GridReorder;
            rGrid.GridRebind += rGrid_GridRebind;
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
        /// Handles the Add event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_Add( object sender, EventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "tagId", "0" );
            parms.Add( "entityTypeId", rFilter.GetUserPreference( "EntityType" ).AsIntegerOrNull().ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "tagId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( "DetailPage", parms );
        }

        /// <summary>
        /// Handles the Delete event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var rockContext = new RockContext();
            var tagService = new Rock.Model.TagService( rockContext );
            var tag = tagService.Get( e.RowKeyId );

            if ( tag != null )
            {
                string errorMessage;
                if ( !tagService.CanDelete( tag, out errorMessage ) )
                {
                    mdGridWarning.Show( errorMessage, ModalAlertType.Information );
                    return;
                }

                tagService.Delete( tag );
                rockContext.SaveChanges();
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridReorder event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GridReorderEventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var tags = GetTags();
            if ( tags != null )
            {
                using ( var rockContext = new RockContext() )
                {
                    var tagService = new TagService( rockContext );
                    tagService.Reorder( tags.ToList(), e.OldIndex, e.NewIndex );
                    rockContext.SaveChanges();
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the SelectedIndexChanged event of the rblScope control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rblScope_SelectedIndexChanged( object sender, EventArgs e )
        {
            if ( rblScope.SelectedValue == "Personal" && CurrentPerson != null )
            {
                ppOwner.SetValue( CurrentPerson );
                ppOwner.Visible = _canConfigure;
            }
            else
            {
                ppOwner.SetValue( null );
                ppOwner.Visible = false;
            }

        }

        /// <summary>
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "EntityType":
                    e.Value = EntityTypeCache.Read( int.Parse( e.Value ) ).FriendlyName;
                    break;
                case "Owner":
                    int? personId = e.Value.AsIntegerOrNull();
                    if ( personId.HasValue )
                    {
                        var person = new PersonService( new RockContext() ).Get( personId.Value );
                        if ( person != null )
                        {
                            e.Value = person.FullNameReversed;
                        }
                        else
                        {
                            e.Value = string.Empty;
                        }
                    }
                    else
                    {
                        e.Value = string.Empty;
                    }
                    break;
            }
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the rFilter control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void rFilter_ApplyFilterClick( object sender, EventArgs e )
        {
            rFilter.SaveUserPreference( "EntityType", ddlEntityType.SelectedValue );
            rFilter.SaveUserPreference( "Scope", rblScope.SelectedValue );

            if (rblScope.SelectedValue == "Personal" && !ppOwner.PersonId.HasValue && CurrentPerson != null )
            {
                ppOwner.SetValue( CurrentPerson );
            }

            rFilter.SaveUserPreference( "Owner", ppOwner.PersonId.HasValue ? ppOwner.PersonId.Value.ToString() : string.Empty );

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            if ( string.IsNullOrWhiteSpace( rFilter.GetUserPreference( "EntityType" ) ) )
            {
                rFilter.SaveUserPreference( "EntityType", EntityTypeCache.Read( "Rock.Model.Person" ).Id.ToString() );
            }
            ddlEntityType.SelectedValue = rFilter.GetUserPreference( "EntityType" );

            if (string.IsNullOrWhiteSpace(rFilter.GetUserPreference("Scope")))
            {
                rFilter.SaveUserPreference("Scope", "Personal");
            }
            rblScope.SelectedValue = rFilter.GetUserPreference( "Scope" );

            if ( rblScope.SelectedValue == "Personal" )
            {
                Person owner = CurrentPerson;
                if ( _canConfigure && !string.IsNullOrWhiteSpace( rFilter.GetUserPreference( "Owner" ) ) )
                {
                    int? savedOwnerId = rFilter.GetUserPreference( "Owner" ).AsIntegerOrNull();
                    if ( savedOwnerId.HasValue )
                    {
                        var person = new PersonService( new RockContext() ).Queryable()
                            .Where( p => p.Id == savedOwnerId.Value )
                            .FirstOrDefault();

                        if ( person != null )
                        {
                            owner = person;
                        }
                    }
                }
                if ( owner != null )
                {
                    rFilter.SaveUserPreference( "Owner", owner.Id.ToString() );
                    if ( _canConfigure )
                    {
                        ppOwner.SetValue( owner );
                    }
                }

                ppOwner.Visible = _canConfigure;
            }
            else
            {
                rFilter.SaveUserPreference( "Owner", string.Empty );
                ppOwner.SetValue( null );
                ppOwner.Visible = false;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            rGrid.Columns[0].Visible = true;

            var tags = GetTags();
            if (tags != null)
            {
                rGrid.DataSource = tags.Select( t => new
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    EntityTypeName = t.EntityType.FriendlyName,
                    EntityTypeQualifierColumn = t.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = t.EntityTypeQualifierValue,
                    Owner = ( t.OwnerPersonAlias != null && t.OwnerPersonAlias.Person != null ) ? 
                        t.OwnerPersonAlias.Person.LastName + ", " + t.OwnerPersonAlias.Person.NickName : 
                        "Organization"
                } ).ToList();

                rGrid.EntityTypeId = EntityTypeCache.Read<Tag>().Id;
                rGrid.DataBind();
            }
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Tag> GetTags()
        {
            int? entityTypeId = rFilter.GetUserPreference( "EntityType" ).AsIntegerOrNull();
            if ( entityTypeId.HasValue )
            {
                var queryable = new Rock.Model.TagService( new RockContext() ).Queryable().
                    Where( t => t.EntityTypeId == entityTypeId.Value );

                if ( rFilter.GetUserPreference( "Scope" ) == "Organization" )
                {
                    // Only allow sorting of public tags if authorized to Administer
                    rGrid.Columns[0].Visible = _canConfigure;
                    queryable = queryable.Where( t => !t.OwnerPersonAliasId.HasValue );
                }
                else
                {
                    int? personId = rFilter.GetUserPreference( "Owner" ).AsIntegerOrNull();
                    if ( _canConfigure && personId.HasValue )
                    {
                        queryable = queryable.Where( t => t.OwnerPersonAlias != null && t.OwnerPersonAlias.PersonId == personId.Value );
                    }
                    else
                    {
                        queryable = queryable.Where( t => t.OwnerPersonAlias != null && t.OwnerPersonAlias.PersonId == CurrentPersonId );
                    }
                }

                return queryable.OrderBy( t => t.Order );

            }

            return null;
        }

        #endregion

}
}