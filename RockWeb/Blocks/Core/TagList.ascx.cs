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
using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.Cache;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Core
{
    /// <summary>
    /// Block for viewing list of tags
    /// </summary>
    [DisplayName( "Tag List" )]
    [Category( "Core" )]
    [Description( "Block for viewing a list of tags." )]

    [BooleanField( "Show Qualifier Columns",
        Description = "Should the 'Qualifier Column' and 'Qualifier Value' fields be displayed in the grid?",
        DefaultValue = "false",
        Order = 0,
        Key = AttributeKey.ShowQualifierColumns )]

    [LinkedPage( "Detail Page",
        Key = AttributeKey.DetailPage )]

    public partial class TagList : RockBlock, ICustomGridColumns
    {
        public static class AttributeKey
        {
            public const string ShowQualifierColumns = "ShowQualifierColumns";
            public const string DetailPage = "DetailPage";
        }

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

            _canConfigure = IsUserAuthorized( Authorization.EDIT );

            // Load Entity Type Filter
            ddlEntityType.Items.Add( new System.Web.UI.WebControls.ListItem() );
            new EntityTypeService( new RockContext() ).GetEntityListItems().ForEach( l => ddlEntityType.Items.Add( l ) );

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
            else
            {
                string personalFormId = string.Format( "{0}$1", cblScope.UniqueID );
                if ( Request.Form["__EVENTTARGET"] == personalFormId )
                {
                    if ( Request.Form[personalFormId] != null && Request.Form[personalFormId] == "Personal" )
                    {
                        if ( _canConfigure && !ppOwner.Visible )
                        {
                            ppOwner.SetValue( CurrentPerson );
                        }
                        ppOwner.Visible = _canConfigure;
                    }
                    else
                    {
                        ppOwner.SetValue( null );
                        ppOwner.Visible = false;
                    }
                }
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
            parms.Add( "TagId", "0" );
            parms.Add( "CategoryId", cpCategory.SelectedValueAsId().ToString() );
            parms.Add( "EntityTypeId", rFilter.GetUserPreference( "EntityType" ).AsIntegerOrNull().ToString() );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
        }

        /// <summary>
        /// Handles the Edit event of the rGrid control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs"/> instance containing the event data.</param>
        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( "TagId", e.RowKeyValue.ToString() );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
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
            var tags = GetTags( true );
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
        /// Rs the filter_ display filter value.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        protected void rFilter_DisplayFilterValue( object sender, GridFilter.DisplayFilterValueArgs e )
        {
            switch ( e.Key )
            {
                case "Category":
                    var category = CategoryCache.Get( int.Parse( e.Value ) );
                    e.Value = category != null ? category.Name : string.Empty;
                    break;
                case "EntityType":
                    var entityType = EntityTypeCache.Get( int.Parse( e.Value ) );
                    e.Value = entityType != null ? entityType.FriendlyName : string.Empty;
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
            rFilter.SaveUserPreference( "Category", cpCategory.SelectedValue );
            rFilter.SaveUserPreference( "EntityType", ddlEntityType.SelectedValue );
            rFilter.SaveUserPreference( "Scope", cblScope.SelectedValues.AsDelimited( "," ) );
            rFilter.SaveUserPreference( "Owner",
                ( _canConfigure && cblScope.SelectedValues.Contains( "Personal" ) && ppOwner.PersonId.HasValue ) ?
                ppOwner.PersonId.Value.ToString() : string.Empty );

            BindGrid();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            cpCategory.SetValue( rFilter.GetUserPreference( "Category" ).AsIntegerOrNull() );
            ddlEntityType.SelectedValue = rFilter.GetUserPreference( "EntityType" );
            cblScope.SetValues( rFilter.GetUserPreference( "Scope" ).SplitDelimitedValues().ToList() );

            if ( _canConfigure && cblScope.SelectedValues.Contains( "Personal" ) )
            {
                Person owner = CurrentPerson;
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

                ppOwner.SetValue( owner ?? CurrentPerson );
                ppOwner.Visible = true;
            }
            else
            {
                ppOwner.Visible = false;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            bool ordered = _canConfigure && !rFilter.GetUserPreference( "Scope" ).SplitDelimitedValues().ToList().Contains( "Personal" );
            rGrid.ColumnsOfType<ReorderField>().First().Visible = ordered;

            bool showQualifierCols = GetAttributeValue( AttributeKey.ShowQualifierColumns ).AsBoolean();
            rGrid.ColumnsOfType<RockBoundField>().Where( c => c.DataField.StartsWith( "EntityTypeQualifier" ) ).ToList().ForEach( c => c.Visible = showQualifierCols );

            var tags = GetTags( ordered );
            if ( tags != null )
            {
                rGrid.DataSource = tags.Select( t => new
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    EntityTypeName = t.EntityType != null ? t.EntityType.FriendlyName : "<All>",
                    EntityTypeQualifierColumn = t.EntityTypeQualifierColumn,
                    EntityTypeQualifierValue = t.EntityTypeQualifierValue,
                    Owner = ( t.OwnerPersonAlias != null && t.OwnerPersonAlias.Person != null ) ?
                        t.OwnerPersonAlias.Person.LastName + ", " + t.OwnerPersonAlias.Person.NickName : "",
                    Scope = ( t.OwnerPersonAlias != null ) ? "Personal" : "Organization",
                    EntityCount = t.TaggedItems.Count(),
                    IsActive = t.IsActive
                } ).ToList();

                rGrid.EntityTypeId = EntityTypeCache.Get<Tag>().Id;
                rGrid.DataBind();
            }
        }

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <returns></returns>
        private IQueryable<Tag> GetTags( bool ordered )
        {
            var queryable = new TagService( new RockContext() ).Queryable();

            int? categoryId = cpCategory.SelectedValueAsId();
            if ( categoryId.HasValue )
            {
                queryable = queryable.Where( t => t.CategoryId == categoryId.Value );
            }

            int? entityTypeId = rFilter.GetUserPreference( "EntityType" ).AsIntegerOrNull();
            if ( entityTypeId.HasValue )
            {
                queryable = queryable.Where( t => t.EntityTypeId == entityTypeId.Value );
            }

            string personFlag = string.Empty;     // Space = None, 0 = All, Integer = Specific Person
            var scopes = rFilter.GetUserPreference( "Scope" ).SplitDelimitedValues().ToList();
            if ( scopes.Contains( "Personal" ) )
            {
                if ( _canConfigure )
                {
                    personFlag = ( rFilter.GetUserPreference( "Owner" ).AsIntegerOrNull() ?? 0 ).ToString();
                }
                else
                {
                    personFlag = CurrentPersonId.HasValue ? CurrentPersonId.Value.ToString() : string.Empty;
                }
            }

            bool includeOrgs = scopes.Contains( "Organization" );

            switch ( personFlag )
            {
                // No people
                case "":
                    queryable = includeOrgs ?
                        queryable.Where( t => !t.OwnerPersonAliasId.HasValue ) :
                        queryable;
                    break;

                // All people
                case "0":
                    queryable = includeOrgs ?
                        queryable :
                        queryable.Where( t => t.OwnerPersonAliasId.HasValue );
                    break;

                // Specific Person
                default:
                    int personId = personFlag.AsInteger();
                    queryable = includeOrgs ?
                        queryable.Where( t => t.OwnerPersonAlias == null || t.OwnerPersonAlias.PersonId == personId ) :
                        queryable.Where( t => t.OwnerPersonAlias != null && t.OwnerPersonAlias.PersonId == personId );
                    break;
            }

            rGrid.ColumnsOfType<ReorderField>().First().Visible = _canConfigure && includeOrgs && personFlag == string.Empty;

            if ( ordered )
            {
                return queryable.OrderBy( t => t.Order ).ThenBy( t => t.Name );
            }
            else
            {
                return queryable.OrderBy( t => t.Name );
            }
        }

        #endregion

    }
}