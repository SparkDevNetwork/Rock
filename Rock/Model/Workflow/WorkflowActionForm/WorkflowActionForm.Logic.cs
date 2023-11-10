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
using Rock.Lava;
using Rock.Web.Cache;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;

namespace Rock.Model
{
    /// <summary>
    /// WorkflowActionForm Logic
    /// </summary>
    public partial class WorkflowActionForm
    {
        #region ICacheable

        /// <summary>
        /// Gets the cache object associated with this Entity
        /// </summary>
        /// <returns></returns>
        public IEntityCache GetCacheObject()
        {
            return WorkflowActionFormCache.Get( this.Id );
        }

        /// <summary>
        /// Updates any Cache Objects that are associated with this entity
        /// </summary>
        /// <param name="entityState">State of the entity.</param>
        /// <param name="dbContext">The database context.</param>
        public void UpdateCache( EntityState entityState, Rock.Data.DbContext dbContext )
        {
            WorkflowActionFormCache.UpdateCachedEntity( this.Id, entityState );
        }

        #endregion ICacheable

        #region Action Buttons

        /// <summary>
        /// Gets or sets the buttons.
        /// </summary>
        /// <value>
        /// The buttons.
        /// </value>
        [NotMapped]
        [LavaVisible]
        public virtual List<LiquidButton> Buttons
        {
            get
            {
                return GetActionButtons( Actions );
            }
        }

        /// <summary>
        /// Gets the action buttons.
        /// </summary>
        /// <param name="actions">The actions.</param>
        /// <returns></returns>
        public static List<LiquidButton> GetActionButtons( string actions )
        {
            var buttonList = new List<LiquidButton>();

            foreach ( var actionButton in actions.Split( new char[] { '|' }, StringSplitOptions.RemoveEmptyEntries ) )
            {
                var button = new LiquidButton();
                var details = actionButton.Split( new char[] { '^' } );
                if ( details.Length > 0 )
                {
                    button.Name = details[0];

                    if ( details.Length > 1 )
                    {
                        var definedValue = DefinedValueCache.Get( details[1].AsGuid() );
                        if ( definedValue != null )
                        {
                            button.Html = definedValue.GetAttributeValue( "ButtonHTML" );
                            button.EmailHtml = definedValue.GetAttributeValue( "ButtonEmailHTML" );
                        }
                    }
                }

                buttonList.Add( button );
            }

            return buttonList;
        }

        #endregion Action Buttons
    }
}