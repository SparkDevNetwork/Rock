//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            SetExtraRestParams();
            this.IconCssClass = "fa fa-folder-open";
            base.OnInit( e );
        }

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
            var ids = this.SelectedValuesAsInt().ToList();
            var items = new CategoryService().Queryable().Where( i => ids.Contains( i.Id ) );
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
        public string EntityTypeName
        {
            set
            {
                EntityTypeId = Rock.Web.Cache.EntityTypeCache.Read( value ).Id;
            }
        }

        /// <summary>
        /// Gets or sets the entity type id.
        /// </summary>
        /// <value>
        /// The entity type id.
        /// </value>
        public int EntityTypeId
        {
            get { return ViewState["EntityTypeId"] as int? ?? 0; }
            set 
            { 
                ViewState["EntityTypeId"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Gets or sets the entity type qualifier column.
        /// </summary>
        /// <value>
        /// The entity type qualifier column.
        /// </value>
        public string EntityTypeQualifierColumn
        {
            get { return ViewState["EntityTypeQualifierColumn"] as string; }
            set 
            { 
                ViewState["EntityTypeQualifierColumn"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Gets or sets the entity type qualifier value.
        /// </summary>
        /// <value>
        /// The entity type qualifier value.
        /// </value>
        public string EntityTypeQualifierValue
        {
            get { return ViewState["EntityTypeQualifierValue"] as string; }
            set 
            { 
                ViewState["EntityTypeQualifierValue"] = value;
                SetExtraRestParams();
            }
        }

        /// <summary>
        /// Sets the extra rest params.
        /// </summary>
        private void SetExtraRestParams()
        {
            var parms = new StringBuilder();
            parms.AppendFormat( "/false/{0}", EntityTypeId );
            if ( !string.IsNullOrEmpty( EntityTypeQualifierColumn ) )
            {
                parms.AppendFormat( "/{0}", EntityTypeQualifierColumn );

                if ( !string.IsNullOrEmpty( EntityTypeQualifierValue ) )
                {
                    parms.AppendFormat( "/{0}", EntityTypeQualifierValue );
                }
            }

            ItemRestUrlExtraParams = parms.ToString();
        }

    }
}