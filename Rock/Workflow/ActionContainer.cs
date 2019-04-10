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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Extension;
using Rock.Web.Cache;

namespace Rock.Workflow
{
    /// <summary>
    /// MEF Container class for Binary File Action Components
    /// </summary>
    public class ActionContainer : Container<ActionComponent, IComponentData>
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        private static readonly Lazy<ActionContainer> instance =
            new Lazy<ActionContainer>( () => new ActionContainer() );

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public static ActionContainer Instance
        {
            get { return instance.Value; }
        }

        /// <summary>
        /// Gets the categories.
        /// </summary>
        /// <value>
        /// The categories.
        /// </value>
        public ConcurrentDictionary<int, string> Categories
        {
            get 
            {
                if ( _categories == null )
                {
                    _categories = new ConcurrentDictionary<int, string>();
                    var categories = new List<string>();
                    foreach ( var action in Instance.Dictionary.Select( d => d.Value.Value ) )
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

                        categories.Add( categoryName );
                    }

                    // Get unique and in order
                    categories = categories.Distinct().OrderBy( c => c ).ToList();
                    int key = 0 - categories.Count;
                    foreach ( var category in categories )
                    {
                        _categories.TryAdd( key++, category );
                    }
                }
                return _categories;
            }
        }
        private ConcurrentDictionary<int, string> _categories;

        /// <summary>
        /// Gets the component with the matching Entity Type Name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static ActionComponent GetComponent( string entityType )
        {
            return Instance.GetComponentByEntity( entityType );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        /// <returns></returns>
        public static string GetComponentName( string entityType )
        {
            return Instance.GetComponentNameByEntity( entityType );
        }

        /// <summary>
        /// Gets or sets the MEF components.
        /// </summary>
        /// <value>
        /// The MEF components.
        /// </value>
        [ImportMany( typeof( ActionComponent ) )]
        protected override IEnumerable<Lazy<ActionComponent, IComponentData>> MEFComponents { get; set; }

        /// <summary>
        /// Refreshes this instance.
        /// </summary>
        public override void Refresh()
        {
            base.Refresh();
            _categories = null;
        }

        /// <summary>
        /// Updates the attributes.
        /// </summary>
        public void UpdateAttributes()
        {
            var entityType = EntityTypeCache.Get<Rock.Model.WorkflowActionType>( false );
            if ( entityType != null )
            {
                foreach ( var component in this.Components )
                {
                    using ( var rockContext = new Rock.Data.RockContext() )
                    {
                        var actionComponent = component.Value.Value;
                        var type = actionComponent.GetType();
                        Rock.Attribute.Helper.UpdateAttributes( type, entityType.Id, "EntityTypeId", EntityTypeCache.GetId( type.FullName ).ToString(), rockContext );
                    }
                }
            }
        }
    }
}
