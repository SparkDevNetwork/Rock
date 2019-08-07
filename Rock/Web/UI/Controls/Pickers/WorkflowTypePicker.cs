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
using System.Web.UI.WebControls;

using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowTypePicker : ItemPicker
    {
        /// <summary>
        /// Gets or sets a value indicating whether [show in active].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in active]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInactive { get; set; } = true;

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "?getCategorizedItems=true&showUnnamedEntityItems=true&showCategoriesThatHaveNoChildren=false";
            ItemRestUrlExtraParams += "&entityTypeId=" + EntityTypeCache.Get( Rock.SystemGuid.EntityType.WORKFLOW_TYPE.AsGuid() ).Id;
            ItemRestUrlExtraParams += "&includeInactiveItems=" + ShowInactive + "&lazyLoad=false";
            this.IconCssClass = "fa fa-cogs";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="workflowType">The Workflow Type.</param>
        public void SetValue( WorkflowType workflowType )
        {
            if ( workflowType != null )
            {
                ItemId = workflowType.Id.ToString();

                string parentCategoryIds = string.Empty;
                var parentCategory = workflowType.CategoryId.HasValue ? CategoryCache.Get( workflowType.CategoryId.Value ) : null;
                while ( parentCategory != null )
                {
                    parentCategoryIds = parentCategory.Id + "," + parentCategoryIds;
                    parentCategory = parentCategory.ParentCategory;
                }

                InitialItemParentIds = parentCategoryIds.TrimEnd( new[] { ',' } );
                ItemName = workflowType.Name;
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
        /// <param name="workflowTypes">The schedules.</param>
        public void SetValues( IEnumerable<WorkflowType> workflowTypes )
        {
            var workflowTypeList = workflowTypes.ToList();

            if ( workflowTypeList.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentCategoryIds = string.Empty;

                foreach ( var workflowType in workflowTypeList )
                {
                    if ( workflowType != null )
                    {
                        ids.Add( workflowType.Id.ToString() );
                        names.Add( workflowType.Name );
                        CategoryCache parentCategory = null;
                        if ( workflowType.CategoryId.HasValue )
                        {
                            parentCategory = CategoryCache.Get( workflowType.CategoryId.Value );
                        }

                        if ( parentCategory != null )
                        {
                            // We need to get all of the categories the selected workflowtype is nested in order to expand them
                            parentCategoryIds += string.Join( ",", GetWorkflowTypeCategoryAncestorIdList( parentCategory.Id ) ) + ",";
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
            var workflowType = new WorkflowTypeService( new RockContext() ).Get( int.Parse( ItemId ) );
            SetValue( workflowType );
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var workflowTypes = new WorkflowTypeService( new RockContext() ).Queryable().Where( g => ItemIds.Contains( g.Id.ToString() ) );
            this.SetValues( workflowTypes );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/Categories/GetChildren/"; }
        }

        /// <summary>
        /// Gets the workflow type category ancestor identifier list.
        /// </summary>
        /// <param name="childCategoryId">The child category identifier.</param>
        /// <returns></returns>
        private List<int> GetWorkflowTypeCategoryAncestorIdList( int childCategoryId )
        {
            CategoryService categoryService = new CategoryService( new RockContext() );
            var parentCategories = categoryService.GetAllAncestors( childCategoryId );

            List<int> parentCategoryIds = parentCategories.Select( p => p.Id ).ToList();

            return parentCategoryIds;
        }
    }
}