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
using System.Collections.Generic;
using System.Linq;

using Rock.Rest.Filters;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;
using Rock.Workflow;

namespace Rock.Rest.Controllers
{
    /// <summary>
    /// 
    /// </summary>
    public partial class WorkflowActionTypesController
    {

        /// <summary>
        /// Gets the children.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        [Authenticate, Secured]
        [System.Web.Http.Route( "api/WorkflowActionTypes/GetChildren/{id}" )]
        public IQueryable<TreeViewItem> GetChildren( string id )
        {
            var list = new List<TreeViewItem>();

            int? idAsInt = id.AsIntegerOrNull();

            if ( string.IsNullOrWhiteSpace( id ) || ( idAsInt.HasValue && idAsInt.Value == 0 ) )
            {
                // Root
                foreach( var category in ActionContainer.Instance.Categories )
                {
                    var item = new TreeViewItem();
                    item.Id = category.Key.ToString();
                    item.Name = category.Value;
                    item.HasChildren = true;
                    item.IconCssClass = "fa fa-folder";
                    list.Add( item );
                }
            }
            else
            {
                if ( idAsInt.HasValue && idAsInt.Value < 0 )
                {
                    // Category
                    if ( ActionContainer.Instance.Categories.ContainsKey( idAsInt.Value ) )
                    {
                        string categoryName = ActionContainer.Instance.Categories[idAsInt.Value];
                        var categorizedActions = GetCategorizedActions();
                        if ( categorizedActions.ContainsKey( categoryName ) )
                        {
                            foreach ( var entityType in categorizedActions[categoryName].OrderBy( e => e.FriendlyName ) )
                            {
                                var item = new TreeViewItem();
                                item.Id = entityType.Id.ToString();
                                item.Name = ActionContainer.GetComponentName(entityType.Name);
                                item.HasChildren = false;
                                item.IconCssClass = "fa fa-cube";
                                list.Add( item );
                            }
                        }
                    }
                }
            }

            return list.OrderBy( i => i.Name ).AsQueryable();
        }

        /// <summary>
        /// Gets the categorized actions.
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, List<EntityTypeCache>> GetCategorizedActions()
        {
            var categorizedActions = new Dictionary<string, List<EntityTypeCache>>();

            foreach ( var action in ActionContainer.Instance.Dictionary.Select( d => d.Value.Value ) )
            {
                string categoryName = "Uncategorized";

                var actionType = action.GetType();
                var obj = actionType.GetCustomAttributes( typeof( ActionCategoryAttribute ), true ).FirstOrDefault();
                if ( obj != null )
                {
                    var actionCategory = obj as ActionCategoryAttribute;
                    if ( actionCategory != null )
                    {
                        categoryName = actionCategory.CategoryName;
                    }
                }

                categorizedActions.AddOrIgnore( categoryName, new List<EntityTypeCache>() );
                categorizedActions[categoryName].Add( action.EntityType );

            }

            return categorizedActions;
        }
    }
}
