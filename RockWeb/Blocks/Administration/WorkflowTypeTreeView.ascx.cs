//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using Rock;
using Rock.Attribute;
using Rock.Model;
using Rock.Web.UI;

namespace RockWeb.Blocks.Administration
{
    /// <summary>
    /// 
    /// </summary>
    [DetailPage]
    public partial class WorkflowTypeTreeView : RockBlock
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            base.OnLoad( e );

            string itemId = PageParameter( "workflowTypeId" );
            string entityTypeName = "workflowType";
            if ( string.IsNullOrWhiteSpace( itemId ) )
            {
                itemId = PageParameter( "categoryId" );
                entityTypeName = "category";
            }

            if ( !string.IsNullOrWhiteSpace( itemId ) )
            {
                hfInitialItemId.Value = itemId;
                hfInitialEntityTypeName.Value = entityTypeName;
                List<string> parentIdList = new List<string>();

                Category category = null;
                if ( entityTypeName.Equals( "workflowType" ) )
                {
                    WorkflowType workflowType = new WorkflowTypeService().Get( int.Parse( itemId ) );
                    if ( workflowType != null )
                    {
                        category = workflowType.Category;
                        if ( category != null )
                        {
                            parentIdList.Insert( 0, category.Id.ToString() );
                        }
                    }
                }
                else
                {
                    category = new CategoryService().Get( int.Parse( itemId ) );
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
    }
}