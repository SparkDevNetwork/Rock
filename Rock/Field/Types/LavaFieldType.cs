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
using Rock.Model;

namespace Rock.Field.Types
{
    /// <summary>
    /// Lava Field Type.  Stored as text
    /// </summary>
    public class LavaFieldType : CodeEditorFieldType
    {
        #region Formatting

        /// <summary>
        /// Formats the value.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValue( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string newValue = value;

            if ( value.HasMergeFields() )
            {
                var mergeFields = Rock.Lava.LavaHelper.GetCommonMergeFields( null );
                if ( entityTypeId.HasValue && entityId.HasValue )
                {
                    var entity = new EntityTypeService( new RockContext() ).GetEntity( entityTypeId.Value, entityId.Value );
                    if ( entity != null )
                    {
                        mergeFields.Add( "Entity", entity );
                    }
                }
                newValue = value.ResolveMergeFields( mergeFields ).Trim();
            }

            return base.FormatValue( parentControl, entityTypeId, entityId, newValue, configurationValues, condensed );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="entityTypeId">The entity type identifier.</param>
        /// <param name="entityId">The entity identifier.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, int? entityTypeId, int? entityId, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // implementing this as the base method encodes the HTML and we don't want to do that with the Lava control
            return FormatValue( parentControl, entityTypeId, entityId, value, configurationValues, condensed );
        }

        /// <summary>
        /// Formats the value as HTML.
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">if set to <c>true</c> [condensed].</param>
        /// <returns></returns>
        public override string FormatValueAsHtml( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed = false )
        {
            // implementing this as the base method encodes the HTML and we don't want to do that with the Lava control
            return FormatValue( parentControl, value, configurationValues, condensed );
        }

        #endregion
    }
}
