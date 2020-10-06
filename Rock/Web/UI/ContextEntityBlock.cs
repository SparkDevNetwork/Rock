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
using Rock.Data;
using Rock.Model;

namespace Rock.Web.UI
{
    /// <summary>
    /// A Block displaying information about a particular entity
    /// </summary>
    [ContextAware]
    public abstract class ContextEntityBlock : RockBlock
    {
        /// <summary>
        /// The current entity being viewed
        /// </summary>
        public IEntity Entity { get; set; }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            base.OnInit( e );
            Entity = ContextEntity();

            // If there is no context entity, then it may not have been set in the block setting. A good assumption
            // is that the context is supposed to be a person, so attempt to get the person directly, or by checking
            // page parameters
            if ( Entity == null )
            {
                Entity = ContextEntity<Person>();
            }
        }
    }
}