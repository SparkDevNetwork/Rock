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
using System.Web.UI;

using Rock.Data;

namespace Rock.Field
{
    /// <summary>
    /// Fields Types that are used to edit a Rock entity
    /// </summary>
    public interface IEntityFieldType : IFieldType
    {
        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        int? GetEditValueAsEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues );

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        void SetEditValueFromEntityId( Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id );

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        IEntity GetEntity( string value);

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        IEntity GetEntity( string value, RockContext rockContext );
    }
}
