//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.UI;

using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// User control for managing the attributes that are available for a specific entity
    /// </summary>
    [BlockProperty( 0, "Entity", "Entity", "Entity Name", false, "" )]
    [BlockProperty( 1, "Entity Qualifier Column", "Entity", "The entity column to evaluate when determining if this attribute applies to the entity", false, "" )]
    [BlockProperty( 2, "Entity Qualifier Value", "Entity", "The entity column value to evaluate.  Attributes will only apply to entities with this value", false, "" )]
    [BlockProperty( 3, "Global Tags", "GlobalTags", "Entity", "Edit global tags (vs. personal tags)?", false, "false", "Rock", "Rock.Field.Types.Boolean" )]
    [System.ComponentModel.Description( "Block for administrating the tags for a given entity type" )]
    public partial class Tags : Rock.Web.UI.RockBlock
    {
        #region Fields

        protected int _entityTypeId;
        protected string _entityQualifierColumn = string.Empty;
        protected string _entityQualifierValue = string.Empty;
        protected int? _ownerId;
        
        private bool _canConfigure = false;

        #endregion

        /// <summary>
        /// Gets a list of any context entities that the block requires.
        /// </summary>
        public override List<string> ContextTypesRequired
        {
            get 
            {
                var requiredContext = base.ContextTypesRequired;

                if ( !Convert.ToBoolean( AttributeValue( "GlobalTags" ) ) )
                    requiredContext.Add( "Rock.Model.Person" );

                return requiredContext;
            }
        }

        #region Control Methods

        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );

            try
            {
                string entityType = AttributeValue( "Entity" );
                if ( string.IsNullOrWhiteSpace( entityType ) )
                {
                    entityType = PageParameter( "Entity" );
                }

                if ( string.IsNullOrWhiteSpace( AttributeValue( "Entity" ) ) )
                {
                    throw new Exception( "Entity is required" );
                }

                _entityTypeId = Rock.Web.Cache.EntityTypeCache.Read( entityType ).Id;

                _entityQualifierColumn = AttributeValue( "EntityQualifierColumn" );
                if ( string.IsNullOrWhiteSpace( _entityQualifierColumn ) )
                    _entityQualifierColumn = PageParameter( "EntityQualifierColumn" );

                _entityQualifierValue = AttributeValue( "EntityQualifierValue" );
                if ( string.IsNullOrWhiteSpace( _entityQualifierValue ) )
                    _entityQualifierValue = PageParameter( "EntityQualifierValue" );

                _canConfigure = CurrentPage.IsAuthorized( "Configure", CurrentPerson );

                if ( !Convert.ToBoolean( AttributeValue( "GlobalTags" ) ) )
                {
                    Rock.Data.IEntity model = CurrentPage.GetCurrentContext( "Rock.Model.Person" );
                    if ( model != null )
                        _ownerId = model.Id;
                    else
                        _ownerId = CurrentPersonId;
                }

                if ( _canConfigure )
                {
                    rGrid.DataKeyNames = new string[] { "id" };
                    rGrid.Actions.IsAddEnabled = true;

                    rGrid.Actions.AddClick += rGrid_Add;
                    rGrid.GridReorder += rGrid_GridReorder;
                    rGrid.GridRebind += rGrid_GridRebind;
                }
                else
                {
                    DisplayError( "You are not authorized to configure these tags" );
                }
            }
            catch ( SystemException ex )
            {
                DisplayError( ex.Message );
            }
        }

        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack && _canConfigure )
                BindGrid();

            base.OnLoad( e );
        }

        #endregion

        #region Events

        protected void rGrid_Edit( object sender, RowEventArgs e )
        {
            ShowEdit( ( int )rGrid.DataKeys[e.RowIndex]["id"] );
        }

        protected void rGrid_Delete( object sender, RowEventArgs e )
        {
            var tagService = new Rock.Model.TagService();
            var tag = tagService.Get( (int)rGrid.DataKeys[e.RowIndex]["id"] );

            if ( tag != null )
            {
                tagService.Delete( tag, CurrentPersonId );
                tagService.Save( tag, CurrentPersonId );
            }

            BindGrid();
        }

        protected void rGrid_Add( object sender, EventArgs e )
        {
            ShowEdit( 0 );
        }

        void rGrid_GridReorder( object sender, GridReorderEventArgs e )
        {
            var tagService = new Rock.Model.TagService();
            var queryable = tagService.Queryable().
                Where( t => t.EntityTypeId == _entityTypeId &&
                    ( t.EntityTypeQualifierColumn ?? string.Empty ) == _entityQualifierColumn &&
                    ( t.EntityTypeQualifierValue ?? string.Empty ) == _entityQualifierValue );

            if ( _ownerId.HasValue )
                queryable = queryable.Where( t => t.OwnerId == _ownerId.Value );
            else
                queryable = queryable.Where( t => t.OwnerId == null );

            var items = queryable
                .OrderBy( t => t.Order )
                .ToList();

            tagService.Reorder( items, e.OldIndex, e.NewIndex, CurrentPersonId );

            BindGrid();
        }

        void rGrid_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        protected void btnSave_Click( object sender, EventArgs e )
        {
            using ( new Rock.Data.UnitOfWorkScope() )
            {
                var tagService = new Rock.Model.TagService();

                Rock.Model.Tag tag;

                int tagId = 0;
                if ( hfId.Value != string.Empty && !Int32.TryParse( hfId.Value, out tagId ) )
                    tagId = 0;

                if ( tagId == 0 )
                {
                    tag = new Rock.Model.Tag();
                    tag.IsSystem = false;
                    tag.EntityTypeId = _entityTypeId;
                    tag.EntityTypeQualifierColumn = _entityQualifierColumn;
                    tag.EntityTypeQualifierValue = _entityQualifierValue;
                    tag.OwnerId = _ownerId;
                    tagService.Add( tag, CurrentPersonId );
                }
                else
                {
                    tag = tagService.Get( tagId );
                }

                tag.Name = tbName.Text;

                tagService.Save( tag, CurrentPersonId );

            }

            BindGrid();

            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        protected void btnCancel_Click( object sender, EventArgs e )
        {
            pnlDetails.Visible = false;
            pnlList.Visible = true;
        }

        #endregion

        #region Methods

        private void BindGrid()
        {
            var queryable = new Rock.Model.TagService().Queryable().
                Where( t => t.EntityTypeId == _entityTypeId &&
                    ( t.EntityTypeQualifierColumn ?? string.Empty ) == _entityQualifierColumn &&
                    ( t.EntityTypeQualifierValue ?? string.Empty ) == _entityQualifierValue );
                    
            if (_ownerId.HasValue)
                queryable = queryable.Where(t => t.OwnerId == _ownerId.Value);
            else
                queryable = queryable.Where(t => t.OwnerId == null);

            rGrid.DataSource = queryable.OrderBy( t => t.Order ).ToList();
            rGrid.DataBind();
        }

        protected void ShowEdit( int tagId )
        {
            var tag =  new Rock.Model.TagService().Get( tagId );

            if ( tag != null )
            {
                lAction.Text = "Edit";
                hfId.Value = tag.Id.ToString();

                tbName.Text = tag.Name;
            }
            else
            {
                lAction.Text = "Add";
                hfId.Value = string.Empty;

                tbName.Text = string.Empty;
            }

            pnlList.Visible = false;
            pnlDetails.Visible = true;
        }

        private void DisplayError( string message )
        {
            pnlMessage.Controls.Clear();
            pnlMessage.Controls.Add( new LiteralControl( message ) );
            pnlMessage.Visible = true;
        }

        #endregion


    }
}