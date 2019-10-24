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
using System.Runtime.Serialization;
using Rock.Communication.SmsActions;
using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about an SMS Action that is required by communications engine.
    /// </summary>
    [Serializable]
    [DataContract]
    public class SmsActionCache : ModelCache<SmsActionCache, SmsAction>
    {
        #region Properties

        /// <summary>
        /// Gets or sets the name of the action.
        /// </summary>
        /// <value>
        /// The name of the action.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is active.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is active; otherwise, <c>false</c>.
        /// </value>
        public bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the order of this action in the system.
        /// </summary>
        /// <value>
        /// The order of this action in the system.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the identifier for the entity type that handles this action's logic.
        /// </summary>
        /// <value>
        /// The identifier for the entity type that handles this action's logic.
        /// </value>
        public int SmsActionComponentEntityTypeId { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether further actions should be processed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if further actions should be processed; otherwise, <c>false</c>.
        /// </value>
        public bool ContinueAfterProcessing { get; set; }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
		[LavaInclude]
        public SmsActionComponent SmsActionComponent
        {
            get
            {
                if ( _smsActionComponent == null )
                {
                    var entityTypeCache = EntityTypeCache.Get( SmsActionComponentEntityTypeId );

                    if ( entityTypeCache != null )
                    {
                        _smsActionComponent = ( SmsActionComponent ) Activator.CreateInstance( entityTypeCache.GetEntityType() );
                    }
                }

                return _smsActionComponent;
            }
        }
        private SmsActionComponent _smsActionComponent = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var smsAction = entity as SmsAction;
            if ( smsAction == null )
            {
                return;
            }

            Name = smsAction.Name;
            IsActive = smsAction.IsActive;
            Order = smsAction.Order;
            SmsActionComponentEntityTypeId = smsAction.SmsActionComponentEntityTypeId;
            ContinueAfterProcessing = smsAction.ContinueAfterProcessing;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <returns></returns>
        public new static List<SmsActionCache> All()
        {
            // use 'new' to override the base All since we want to sort actions
            return ModelCache<SmsActionCache, SmsAction>.All().OrderBy( a => a.Order ).ToList();
        }

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public new static List<SmsActionCache> All( RockContext rockContext )
        {
            // use 'new' to override the base All since we want to sort actions
            return ModelCache<SmsActionCache, SmsAction>.All( rockContext ).OrderBy( a => a.Order ).ToList();
        }

        #endregion
    }
}