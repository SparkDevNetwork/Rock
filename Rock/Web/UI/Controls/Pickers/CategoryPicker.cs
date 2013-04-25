//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System.Collections.Generic;
using System.Linq;
using Rock.Model;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class CategoryPicker : ItemPicker
    {
        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="category">The category.</param>
        public void SetValue( Rock.Model.Category category )
        {
            if ( category != null )
            {
                ItemId = category.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = category.ParentCategory;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new char[] { ',' } );
                ItemName = category.Name;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="categories">The categories.</param>
        public void SetValues( IEnumerable<Category> categories )
        {
            var theCategories = categories.ToList();

            if ( theCategories.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var category in theCategories )
                {
                    if ( category != null )
                    {
                        ids.Add( category.Id.ToString() );
                        names.Add( category.Name );
                        var parentCategory = category.ParentCategory;

                        while ( parentCategory != null )
                        {
                            parentCategoryIds += parentCategory.Id.ToString() + ",";
                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemIds = ids;
                ItemNames = names;
            }
            else
            {
                ItemId = Constants.None.IdValue;
                ItemName = Constants.None.TextHtml;
            }
        }

        /// <summary>
        /// Sets the value on select.
        /// </summary>
        protected override void SetValueOnSelect()
        {
            var item = new CategoryService().Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var items = new CategoryService().Queryable().Where( i => ItemIds.Contains( i.Id.ToString() ) );
            this.SetValues( items );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/categories/getchildren/"; }
        }

        /// <summary>
        /// Sets the type of the category entity.
        /// </summary>
        /// <value>
        /// The type of the category entity.
        /// </value>
        public string CategoryEntityTypeName
        {
            set
            {
                ItemRestUrlExtraParams = "/" + value + "/false";
            }
        }
    }
}