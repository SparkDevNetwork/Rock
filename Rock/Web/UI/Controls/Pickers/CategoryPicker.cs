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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

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
            this.AllowCategorySelection = true;
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
        /// Gets or sets the root category.  This will be the topmost category that can be selected.
        /// </summary>
        /// <value>
        /// The root category identifier.
        /// </value>
        public int? RootCategoryId
        {
            get
            {
                return ViewState["RootCategoryId"] as int?;
            }

            set
            {
                ViewState["RootCategoryId"] = value;
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

                var parentCategoryIds = new List<string>();
                var parentCategory = category.ParentCategory;
                while ( parentCategory != null )
                {
                    if ( !parentCategoryIds.Contains( parentCategory.Id.ToString() ) )
                    {
                        parentCategoryIds.Insert( 0, parentCategory.Id.ToString() );
                    }
                    else
                    {
                        // infinite recursion
                        break;
                    }

                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.AsDelimited( "," );
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
                var parentCategoryIds = new List<string>();

                foreach ( var category in theCategories )
                {
                    if ( category != null )
                    {
                        ids.Add( category.Id.ToString() );
                        names.Add( category.Name );
                        var parentCategory = category.ParentCategory;

                        while ( parentCategory != null )
                        {
                            if ( !parentCategoryIds.Contains( parentCategory.Id.ToString() ) )
                            {
                                parentCategoryIds.Insert( 0, parentCategory.Id.ToString() );
                            }
                            else
                            {
                                // infinite recursion
                                break;
                            }

                            parentCategory = parentCategory.ParentCategory;
                        }
                    }
                }

                InitialItemParentIds = parentCategoryIds.AsDelimited( "," );
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
                EntityTypeId = EntityTypeCache.Get( value ).Id;
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
            get
            {
                return ViewState["EntityTypeId"] as int? ?? 0;
            }

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
            get
            {
                return ViewState["EntityTypeQualifierColumn"] as string;
            }

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
            get
            {
                return ViewState["EntityTypeQualifierValue"] as string;
            }

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
            string parms = $"?getCategorizedItems=false&entityTypeId={EntityTypeId}";

            parms += EntityTypeQualifierColumn.IsNotNullOrWhiteSpace() ? $"&entityQualifier={EntityTypeQualifierColumn}" : string.Empty;
            parms += EntityTypeQualifierValue.IsNotNullOrWhiteSpace() ? $"&entityQualifierValue={EntityTypeQualifierValue}" : string.Empty;
            parms += ExcludedCategoryIds.IsNotNullOrWhiteSpace() ? $"&excludedCategoryIds={ExcludedCategoryIds}" : string.Empty;

            if ( RootCategoryId.HasValue )
            {
                var rootCategory = CategoryCache.Get( RootCategoryId.Value );
                parms += rootCategory.EntityTypeId == EntityTypeId ? $"&rootCategoryId={rootCategory.Id}" : string.Empty;
            }

            parms += "&lazyLoad=false";

            ItemRestUrlExtraParams = parms;
        }
    }
}