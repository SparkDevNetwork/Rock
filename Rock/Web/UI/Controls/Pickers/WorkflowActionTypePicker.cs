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

using Rock.Web.Cache;
using Rock.Workflow;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class WorkflowActionTypePicker : ItemPicker
    {
        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            ItemRestUrlExtraParams = "";
            this.IconCssClass = "fa fa-folder";
            base.OnInit( e );
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="entityType">Type of the entity.</param>
        public void SetValue( EntityTypeCache entityType )
        {
            ItemId = Constants.None.IdValue;
            ItemName = Constants.None.TextHtml;

            if ( entityType != null )
            {
                ItemId = entityType.Id.ToString();
                ItemName = ActionContainer.GetComponentName(entityType.Name);

                var action = ActionContainer.GetComponent( entityType.Name );
                if ( action != null )
                {
                    var actionType = action.GetType();
                    var obj = actionType.GetCustomAttributes( typeof( ActionCategoryAttribute ), true ).FirstOrDefault();
                    if ( obj != null )
                    {
                        var actionCategory = obj as ActionCategoryAttribute;
                        if ( actionCategory != null )
                        {
                            var categoryEntry = ActionContainer.Instance.Categories.Where( c => c.Value == actionCategory.CategoryName ).FirstOrDefault();
                            InitialItemParentIds = categoryEntry.Key.ToString();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the values.
        /// </summary>
        /// <param name="entityTypes">The entity types.</param>
        /// <exception cref="System.NotSupportedException"></exception>
        public void SetValues( IEnumerable<EntityTypeCache> entityTypes )
        {
            var theEntityTypes = entityTypes.ToList();
            if ( theEntityTypes.Any() )
            {
                var ids = new List<string>();
                var names = new List<string>();
                var parentIds = new List<string>();

                foreach( var entityType in theEntityTypes )
                {
                    ids.Add( entityType.Id.ToString() );
                    names.Add(ActionContainer.GetComponentName(entityType.Name));

                    var action = ActionContainer.GetComponent( entityType.Name );
                    if ( action != null )
                    {
                        var actionType = action.GetType();
                        var obj = actionType.GetCustomAttributes( typeof( ActionCategoryAttribute ), true ).FirstOrDefault();
                        if ( obj != null )
                        {
                            var actionCategory = obj as ActionCategoryAttribute;
                            if ( actionCategory != null )
                            {
                                parentIds.Add( string.Format( "'{0}'", actionCategory.CategoryName.EscapeQuotes() ) );
                            }
                        }
                    }
                }

                InitialItemParentIds = parentIds.AsDelimited( "," );
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
            int? entityTypeId = ItemId.AsIntegerOrNull();
            if ( entityTypeId.HasValue )
            {
                var entityType = EntityTypeCache.Get( entityTypeId.Value );
                SetValue( entityType );
            }
        }

        /// <summary>
        /// Sets the values on select.
        /// </summary>
        protected override void SetValuesOnSelect()
        {
            var entityTypes = new List<EntityTypeCache>();
            foreach ( var itemId in ItemIds )
            {
                int? entityTypeId = itemId.AsIntegerOrNull();
                if ( entityTypeId.HasValue )
                {
                    entityTypes.Add( EntityTypeCache.Get( entityTypeId.Value ) );
                }
            }

            this.SetValues( entityTypes );
        }

        /// <summary>
        /// Gets the item rest URL.
        /// </summary>
        /// <value>
        /// The item rest URL.
        /// </value>
        public override string ItemRestUrl
        {
            get { return "~/api/WorkflowActionTypes/Getchildren/"; }
        }

    }
}