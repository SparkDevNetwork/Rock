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
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Data;
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
        /// Gets or sets the excluded category ids (comma delimited) 
        /// </summary>
        /// <value>
        /// The excluded category ids.
        /// </value>
        public string ExcludedCategoryIds
        {
            get
            {
                return ViewState["ExcludedCategoryIds"] as string ?? string.Empty;
            }

            set
            {
                ViewState["ExcludedCategoryIds"] = value;
                SetExtraRestParams();
            }
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
            var item = new CategoryService( new RockContext() ).Get( int.Parse( ItemId ) );
            this.SetValue( item );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var ids = this.SelectedValuesAsInt().ToList();
            var items = new CategoryService( new RockContext() ).Queryable().Where( i => ids.Contains( i.Id ) );
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
            string parms = "?getCategorizedItems=false";
            parms += string.Format( "&entityTypeId={0}", EntityTypeId );

            if ( !string.IsNullOrEmpty( EntityTypeQualifierColumn ) )
            {
                parms += string.Format( "&entityQualifier={0}", EntityTypeQualifierColumn );

                if ( !string.IsNullOrEmpty( EntityTypeQualifierValue ) )
                {
                    parms += string.Format( "&entityQualifierValue={0}", EntityTypeQualifierValue );
                }
            }

            if ( !string.IsNullOrEmpty( ExcludedCategoryIds ) )
            {
                parms += string.Format( "&excludedCategoryIds={0}", ExcludedCategoryIds);
            }

            ItemRestUrlExtraParams = parms;
        }
    }
}