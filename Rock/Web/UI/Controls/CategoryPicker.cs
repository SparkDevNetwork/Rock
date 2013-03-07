//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
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
                ItemId = Rock.Constants.None.IdValue;
                ItemName = Rock.Constants.None.TextHtml;
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
        /// Gets the item rest URL extra params.
        /// </summary>
        /// <value>
        /// The item rest URL extra params.
        /// </value>
        public override string ItemRestUrlExtraParams
        {
            get { return "/" + ViewState["CategoryEntityTypeName"] as string + "/false"; }
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
                ViewState["CategoryEntityTypeName"] = value;
            }
        }
    }
}