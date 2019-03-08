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
using System.Linq;

using Rock.Data;
using Rock.Model;
using Rock.Web.UI.Controls;

namespace Rock.Field.Types
{
    class AssetStorageProviderFieldType : FieldType
    {
        public override string FormatValue( Control parentControl, string value, Dictionary<string, ConfigurationValue> configurationValues, bool condensed )
        {
            string formattedValue = string.Empty;

            Guid? assetStorageProviderGuid = value.AsGuidOrNull();
            if ( assetStorageProviderGuid.HasValue )
            {
                using ( var rockContext = new RockContext() )
                {
                    var assetStorageProvider = new AssetStorageProviderService( rockContext ).Get( assetStorageProviderGuid.Value );
                    if ( assetStorageProvider != null )
                    {
                        formattedValue = assetStorageProvider.Name;
                    }
                }
            }

            return base.FormatValue( parentControl, formattedValue, null, condensed );
        }

        public override Control EditControl( Dictionary<string, ConfigurationValue> configurationValues, string id )
        {
            return new AssetStorageProviderPicker { ID = id, ShowAll = false };
        }

        public override string GetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues )
        {
            var picker = control as AssetStorageProviderPicker;
            if ( picker != null )
            {
                int? itemId = picker.SelectedValue.AsIntegerOrNull();
                Guid? itemGuid = null;
                if ( itemId.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemGuid = new AssetStorageProviderService( rockContext ).Queryable().Where( a => a.Id == itemId.Value ).Select( a => ( Guid? ) a.Guid ).FirstOrDefault();
                    }
                }

                return itemGuid?.ToString();
            }

            return null;
        }

        public override void SetEditValue( Control control, Dictionary<string, ConfigurationValue> configurationValues, string value )
        {
            var picker = control as AssetStorageProviderPicker;
            if ( picker != null )
            {
                int? itemId = null;
                Guid? itemGuid = value.AsGuidOrNull();
                if ( itemGuid.HasValue )
                {
                    using ( var rockContext = new RockContext() )
                    {
                        itemId = new AssetStorageProviderService( rockContext ).Queryable().Where( a => a.Guid == itemGuid.Value ).Select( a => ( int? ) a.Id ).FirstOrDefault();
                    }
                }

                picker.SetValue( itemId );
            }
        }
    }
}
