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

using Rock.Configuration;
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
        [Obsolete( "Use the constructor that takes only contentChannelGuid and name." )]
        [RockObsolete( "20.0" )]
        public ContentChannelItemFieldAttribute( string contentChannelGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.CONTENT_CHANNEL_ITEM.AsGuid(), name, description, required, defaultValue, category, order, key )
        {
            if ( !string.IsNullOrWhiteSpace( contentChannelGuid ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( contentChannelGuid, out guid ) && RockApp.Current.IsDatabaseAvailable() )
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentChannelItemFieldAttribute" /> class.
        /// </summary>
        /// <param name="contentChannelGuid">The content channel GUID.</param>
        /// <param name="name">The name.</param>
        /// <remarks>
        /// This is essentially a temporary constructor. Once the constructor
        /// takes multiple parameters is removed, this constructor can be marked
        /// as obsolete and a new constructor that takes only a name parameter
        /// can be added to match the pattern of all other field attributes.
        /// We can't go directly to a single name parameter because it would
        /// conflict with the original constructor that takes the content channel
        /// guid as the first parameter.
        /// </remarks>
        public ContentChannelItemFieldAttribute( string contentChannelGuid, string name )
            : base( SystemGuid.FieldType.COMPONENT.AsGuid(), name )
        {
            ContentChannelGuid = contentChannelGuid;
        }

        /// <summary>
        /// The unique identifier of the content channel to limit the selection to.
        /// </summary>
        /// <remarks>
        /// It is unusual, but this property requires the database to be available
        /// in order to work. The ContentChannelItem field type uses the integer
        /// identifier of the content channel rather than the guid.
        /// </remarks>
        public string ContentChannelGuid
        {
            get
            {
                var configValue = FieldConfigurationValues.GetValueOrNull( "contentchannel" );

                if ( int.TryParse( configValue, out var id ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var contentChannel = ContentChannelCache.Get( id );

                    if ( contentChannel != null )
                    {
                        return contentChannel.Guid.ToString();
                    }
                }

                return null;
            }
            set
            {
                if ( Guid.TryParse( value, out var guid ) && RockApp.Current.IsDatabaseAvailable() )
                {
                    var contentChannel = ContentChannelCache.Get( guid );

                    if ( contentChannel != null )
                    {
                        var configValue = new Field.ConfigurationValue( contentChannel.Id.ToString() );
                        FieldConfigurationValues.Add( "contentchannel", configValue );
                    }
                }
            }
        }
    }
}
