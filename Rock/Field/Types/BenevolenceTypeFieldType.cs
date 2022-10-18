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
using System.Data.Entity;
using System.Linq;
using System.Web.UI;
using System.Web.UI.WebControls;

using Rock.Attribute;
using Rock.Data;
using Rock.Model;
using Rock.Web.Cache;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    /// <summary>
    /// Field Type used to display a dropdown list of benevolence types
    /// </summary>
    [Serializable]
    [RockPlatformSupport( Utility.RockPlatform.WebForms )]
    [Rock.SystemGuid.FieldTypeGuid( "7BD3C3A3-DF4A-41EB-BF13-29EDB166078B")]
    public class BenevolenceTypeFieldType : FieldType, IEntityFieldType, IEntityReferenceFieldType
    {
        #region Formatting

        /// <inheritdoc/>
        public override string GetTextValue( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();
            if ( guid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var type = new BenevolenceTypeService( rockContext ).GetNoTracking( guid.Value );
                    if ( type != null )
                    {
                        return type.Name;
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns the field's current value(s)
        /// </summary>
        /// <param name="parentControl">The parent control.</param>
        /// <param name="value">Information about the value</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="condensed">Flag indicating if the value should be condensed (i.e. for use in a grid column)</param>
        /// <returns></returns>
        public override string FormatValue(Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed)
        {
            return !condensed
                ? GetTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) )
                : GetCondensedTextValue( value, configurationValues.ToDictionary( cv => cv.Key, cv => cv.Value.Value ) );
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
        public override Control EditControl(Dictionary<string, ConfigurationValue> configurationValues, string id)
        {
            var editControl = new RockDropDownList { ID = id };
            editControl.Items.Add(new ListItem());

            var types = new BenevolenceTypeService(new RockContext())
                .Queryable().AsNoTracking()
                .OrderBy(o => o.Name)
                .Select(o => new
                {
                    o.Guid,
                    o.Name,
                })
                .ToList();

            if (types.Any())
            {
                foreach (var type in types)
                {
                    var listItem = new ListItem(type.Name, type.Guid.ToString().ToUpper());
                    editControl.Items.Add(listItem);
                }

                return editControl;
            }

            return null;
        }

        /// <summary>
        /// Reads new values entered by the user for the field
        /// </summary>
        /// <param name="control">Parent control that controls were added to in the CreateEditControl() method</param>
        /// <param name="configurationValues"></param>
        /// <returns></returns>
        public override string GetEditValue(Control control, Dictionary<string, ConfigurationValue> configurationValues)
        {
            var picker = control as DropDownList;
            if (picker != null)
            {
                // picker has value as BenevolenceType.Guid
                return picker.SelectedValue;
            }

            return null;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues"></param>
        /// <param name="value">The value.</param>
        public override void SetEditValue(Control control, Dictionary<string, ConfigurationValue> configurationValues, string value)
        {
            var editControl = control as ListControl;
            if (editControl != null)
            {
                editControl.SetValue(value);
            }
        }

        #endregion

        #region Entity Methods

        /// <summary>
        /// Gets the edit value as the IEntity.Id
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <returns></returns>
        public int? GetEditValueAsEntityId(System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues)
        {
            Guid guid = GetEditValue(control, configurationValues).AsGuid();
            var item = new BenevolenceTypeService(new RockContext()).Get(guid);
            return item != null ? item.Id : (int?)null;
        }

        /// <summary>
        /// Sets the edit value from IEntity.Id value
        /// </summary>
        /// <param name="control">The control.</param>
        /// <param name="configurationValues">The configuration values.</param>
        /// <param name="id">The identifier.</param>
        public void SetEditValueFromEntityId(System.Web.UI.Control control, Dictionary<string, ConfigurationValue> configurationValues, int? id)
        {
            var item = new BenevolenceTypeService(new RockContext()).Get(id ?? 0);
            string guidValue = item != null ? item.Guid.ToString() : string.Empty;
            SetEditValue(control, configurationValues, guidValue);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public IEntity GetEntity(string value)
        {
            return GetEntity(value, null);
        }

        /// <summary>
        /// Gets the entity.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public IEntity GetEntity(string value, RockContext rockContext)
        {
            Guid? guid = value.AsGuidOrNull();
            if (guid.HasValue)
            {
                rockContext = rockContext ?? new RockContext();
                return new BenevolenceTypeService(rockContext).Get(guid.Value);
            }

            return null;
        }

        #endregion

        #region IEntityReferenceFieldType

        /// <inheritdoc/>
        List<ReferencedEntity> IEntityReferenceFieldType.GetReferencedEntities( string privateValue, Dictionary<string, string> privateConfigurationValues )
        {
            Guid? guid = privateValue.AsGuidOrNull();

            if ( !guid.HasValue )
            {
                return null;
            }

            using ( var rockContext = new RockContext() )
            {
                var benevolenceTypeId = new BenevolenceTypeService( rockContext ).GetId( guid.Value );

                if ( !benevolenceTypeId.HasValue )
                {
                    return null;
                }

                return new List<ReferencedEntity>
                {
                    new ReferencedEntity( EntityTypeCache.GetId<BenevolenceType>().Value, benevolenceTypeId.Value )
                };
            }
        }

        /// <inheritdoc/>
        List<ReferencedProperty> IEntityReferenceFieldType.GetReferencedProperties( Dictionary<string, string> privateConfigurationValues )
        {
            return new List<ReferencedProperty>
            {
                new ReferencedProperty( EntityTypeCache.GetId<BenevolenceType>().Value, nameof( BenevolenceType.Name ) )
            };
        }

        #endregion
    }
}