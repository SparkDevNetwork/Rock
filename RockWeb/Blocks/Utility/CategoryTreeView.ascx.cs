//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;

using Rock;
using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Utility
{
    /// <summary>
    /// 
    /// </summary>
    [DetailPage]
    [EntityType("Entity Type", "The types of entities to display categories for")]
    [TextField( "Page Parameter Key", "The page parameter to look for" )]
    public partial class CategoryTreeView : RockBlock
    {
        /// <summary>
        /// The entity type name
        /// </summary>
        protected string EntityTypeName;

        /// <summary>
        /// The page parameter name
        /// </summary>
        protected string PageParameterName;
        
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            // Get EntityTypeName
            string entityTypeName = GetAttributeValue( "EntityType" );
            int entityTypeId = Rock.Web.Cache.EntityTypeCache.Read( entityTypeName ).Id;

            var cachedEntityType = Rock.Web.Cache.EntityTypeCache.Read( entityTypeId );
            if ( cachedEntityType != null )
            {
                EntityTypeName = cachedEntityType.Name;
                lbAddItem.ToolTip = "Add " + cachedEntityType.FriendlyName;
            }

            PageParameterName = GetAttributeValue( "PageParameterKey" );
            string itemId = PageParameter( PageParameterName );
            string selectedEntityType = EntityTypeName;
            if ( string.IsNullOrWhiteSpace( itemId ) )
            {
                itemId = PageParameter( "categoryId" );
                selectedEntityType = "category";
            }

            lbAddCategory.Visible = true;
            lbAddItem.Visible = false;

            if ( !string.IsNullOrWhiteSpace( itemId ) )
            {
                hfInitialItemId.Value = itemId;
                hfInitialEntityIsCategory.Value = (selectedEntityType == "category").ToString();
                List<string> parentIdList = new List<string>();

                Category category = null;
                if ( selectedEntityType.Equals( "category" ) )
                {
                    category = new CategoryService().Get( int.Parse( itemId ) );
                    lbAddItem.Visible = true;
                }
                else
                {
                    int id = 0;
                    if ( int.TryParse( itemId, out id ) )
                    {
                        if ( cachedEntityType != null )
                        {
                            Type entityType = cachedEntityType.GetEntityType();
                            if ( entityType != null )
                            {
                                Type serviceType = typeof( Rock.Data.Service<> );
                                Type[] modelType = { entityType };
                                Type service = serviceType.MakeGenericType( modelType );
                                var serviceInstance = Activator.CreateInstance( service );
                                var getMethod = service.GetMethod( "Get", new Type[] { typeof( int ) } );
                                ICategorized entity = getMethod.Invoke( serviceInstance, new object[] { id } ) as ICategorized;

                                if ( entity != null )
                                {
                                    lbAddCategory.Visible = false;
                                    category = entity.Category;
                                    if ( category != null )
                                    {
                                        parentIdList.Insert( 0, category.Id.ToString() );
                                    }
                                }
                            }
                        }
                    }
                }
                
                while ( category != null )
                {
                    category = category.ParentCategory;
                    if ( category != null )
                    {
                        parentIdList.Insert( 0, category.Id.ToString() );
                    }

                }

                hfInitialCategoryParentIds.Value = parentIdList.AsDelimited( "," );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddCategory control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddCategory_Click( object sender, EventArgs e )
        {
            int parentCategoryId = 0;
            if ( Int32.TryParse( hfSelectedCategoryId.Value, out parentCategoryId ) )
            {
                NavigateToDetailPage( "CategoryId", 0, "parentCategoryId", parentCategoryId );
            }
            else
            {
                NavigateToDetailPage( "CategoryId", 0 );
            }
        }

        /// <summary>
        /// Handles the Click event of the lbAddItem control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        protected void lbAddItem_Click( object sender, EventArgs e )
        {
            int parentCategoryId = 0;
            if ( Int32.TryParse( hfSelectedCategoryId.Value, out parentCategoryId ) )
            {
                NavigateToDetailPage( PageParameterName, 0, "parentCategoryId", parentCategoryId );
            }
        }
    }
}