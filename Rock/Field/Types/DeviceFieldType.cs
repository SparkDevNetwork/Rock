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

using Rock.Attribute;
using Rock.ViewModels.Utility;
using Rock.Web.Cache;

namespace Rock.Field.Types
{
    /// <summary>
    /// Picker for selecting a single Device.
    /// </summary>
    [DefinedValueField( "Device Type",
        Description = "When set the list of devices will be limited to those that match this device type.",
        IsRequired = false,
        DefinedTypeGuid = Rock.SystemGuid.DefinedType.DEVICE_TYPE,
        Key = AttributeKey.DeviceType )]

    [Rock.SystemGuid.FieldTypeGuid( "d7f5d737-bdc9-4656-951e-08325d0543fd" )]
    public class DeviceFieldType : UniversalItemPickerFieldType
    {
        #region Keys

        private class AttributeKey
        {
            public const string DeviceType = "DeviceType";
        }

        #endregion

        /// <inheritdoc/>
        protected sealed override List<ListItemBag> GetItemBags( IEnumerable<string> values, Dictionary<string, string> privateConfigurationValues )
        {
            return GetListItems( privateConfigurationValues )
                .Where( bag => values.Contains( bag.Value ) )
                .ToList();
        }

        /// <inheritdoc/>
        protected sealed override List<ListItemBag> GetListItems( Dictionary<string, string> privateConfigurationValues )
        {
            var deviceTypeGuid = privateConfigurationValues.GetValueOrNull( AttributeKey.DeviceType ).AsGuidOrNull();
            var deviceTypeId = deviceTypeGuid.HasValue
                ? DefinedValueCache.GetId( deviceTypeGuid.Value )
                : null;

            return DeviceCache.All()
                .Where( d => d.IsActive
                    && ( !deviceTypeId.HasValue || d.DeviceTypeValueId == deviceTypeId.Value ) )
                .OrderBy( d => d.Name )
                .ToListItemBagList();
        }

        /// <inheritdoc/>
        protected override bool GetEnhanceForLongLists( Dictionary<string, string> privateConfigurationValues )
        {
            return true;
        }
    }
}
