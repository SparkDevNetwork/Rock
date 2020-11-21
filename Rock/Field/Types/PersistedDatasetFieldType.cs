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
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Stored as PersistedDataset.Guid
    /// </summary>
    public class PersistedDatasetFieldType : FieldType, ICachedEntitiesFieldType
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
        public override string FormatValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            var persistedDatasetGuid = value.AsGuidOrNull();
            if ( persistedDatasetGuid.HasValue )
            {
                return PersistedDatasetCache.Get( persistedDatasetGuid.Value )?.Name;
            }

            return null;
        }

        /// <summary>
        /// Returns the value using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object ValueAsFieldType( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var persistedDatasetGuid = value.AsGuidOrNull();
            if ( persistedDatasetGuid.HasValue )
            {
                return PersistedDatasetCache.Get( persistedDatasetGuid.Value ).ResultDataObject;
            }

            return null;
        }

        /// <summary>
        /// Returns the value that should be used for sorting, using the most appropriate datatype
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">The value.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override object SortValue( System.Web.UI.Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var persistedDatasetGuid = value.AsGuidOrNull();
            if ( persistedDatasetGuid.HasValue )
            {
                return PersistedDatasetCache.Get( persistedDatasetGuid.Value ).Name;
            }

            return null;
        }

        #endregion

        #region Edit Control

        /// <summary>
        /// Creates the control(s) necessary for prompting user for a new value
        /// </summary>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id"></param>
        /// <returns>
        /// The control
        /// </returns>
        public override System.Web.UI.Control EditControl( System.Collections.Generic.Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            var ddlPersistedDataset = new RockDropDownList { ID = id };
            ddlPersistedDataset.Items.Clear();
            ddlPersistedDataset.Items.Add( new ListItem() );
            foreach ( var persistedDataset in PersistedDatasetCache.All().OrderBy( a => a.Name ) )
            {
                ddlPersistedDataset.Items.Add( new ListItem( persistedDataset.Name, persistedDataset.Guid.ToString() ) );
            }

            return ddlPersistedDataset;
        }

        /// <summary>
        /// Gets the edit value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var ddlPersistedDataset = control as RockDropDownList;
            return ddlPersistedDataset?.SelectedValue;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="value">The value.</param>
        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var ddlPersistedDataset = control as RockDropDownList;
            if ( ddlPersistedDataset != null )
            {
                ddlPersistedDataset.SetValue( value );
            }
        }

        /// <summary>
        /// Gets the cached entities as a list.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public List<IEntityCache> GetCachedEntities( string value )
        {
            var result = new List<IEntityCache>();

            List<PersistedDatasetCache> list = value.SplitDelimitedValues().AsGuidList().Select( g => PersistedDatasetCache.Get( g ) ).ToList();

            result.AddRange( list );

            return result;
        }

        #endregion
    }
}