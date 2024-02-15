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

using Rock.Utility.Settings;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select 0 or 1 content channel item
    /// Stored as ContentChannelItem.Guid
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class ContentChannelItemFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannelItemFieldAttribute" /> class.
        /// </summary>
        /// <param name="contentChannelGuid">The content channel GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        public ContentChannelItemFieldAttribute( string contentChannelGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.ContentChannelItemFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( contentChannelGuid ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( contentChannelGuid, out guid ) && RockInstanceConfig.DatabaseIsAvailable )
                {
                    var contentChannel = ContentChannelCache.Get( guid );
                    if ( contentChannel != null )
                    {
                        var configValue = new Field.ConfigurationValue( contentChannel.Id.ToString() );
                        FieldConfigurationValues.Add( "contentchannel", configValue );

                        if ( string.IsNullOrWhiteSpace( Name ) )
                        {
                            Name = contentChannel.Name;
                        }

                        if ( string.IsNullOrWhiteSpace( Key ) )
                        {
                            Key = Name.Replace( " ", string.Empty );
                        }
                    }
                }
            }
        }
    }
}
