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
using System.Web.UI;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type to select a single (or null) Connection Request Activity
    /// Stored as ConnectionRequestActivity.Guid
    /// </summary>
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    public class ConnectionRequestActivityFieldType : FieldType, IEntityFieldType
    {

        #region Formatting

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = value;

            ConnectionRequestActivity connectionRequestActivity = null;

            using ( var rockContext = new RockContext() )
            {
                Guid? guid = value.AsGuidOrNull();
                if ( guid.HasValue )
                {
                    connectionRequestActivity = new ConnectionRequestActivityService( rockContext ).GetNoTracking( guid.Value );
                }

                if ( connectionRequestActivity != null &&
                    connectionRequestActivity.Note != null )
                {
                    formattedValue = connectionRequestActivity.Note;
                }
            }

            return base.FormatValue( parentControl, formattedValue, configurationValues, condensed );
        }

        #endregion

        #region Edit Control

        // simple text box implemented by base FieldType

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            Guid guid = GetEditValue( control, configurationValues ).AsGuid();
            using ( var rockContext = new RockContext() )
            {
                return new ConnectionRequestActivityService( rockContext ).GetId( guid );
            }
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId( System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id )
        {
            using ( var rockContext = new RockContext() )
            {
                var itemGuid = new ConnectionRequestActivityService( rockContext ).GetGuid( id ?? 0 );
                string guidValue = itemGuid.HasValue ? itemGuid.ToString() : string.Empty;
                SetEditValue( control, configurationValues, guidValue );
            }
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value )
        {
            return GetEntity( value, null );
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity( string value, RockContext rockContext )
        {
            rockContext = rockContext ?? new RockContext();
            Guid? guid = value.AsGuidOrNull();
            if ( guid.HasValue )
            {
                return new ConnectionRequestActivityService( rockContext ).Get( guid.Value );
            }

            return null;
        }

        #endregion

    }
}