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

namespace Rock.Attribute
{
    /// <summary>
    /// Field Attribute to select group and role filtered by a group type
    /// Stored as "GroupType.Guid|Group.Guid"
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true, Inherited = true )]
    public class BlockTemplateFieldAttribute : FieldAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BlockTemplateFieldAttribute" /> class.
        /// </summary>
        /// <param name="templateBlockValueGuid">The template block defined value GUID.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a name only." )]
        [RockObsolete( "20.0" )]
        public BlockTemplateFieldAttribute( string name, string description = "", string templateBlockValueGuid = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( SystemGuid.FieldType.BLOCK_TEMPLATE.AsGuid(), name )
        {
            Category = category;
            DefaultValue = defaultValue;
            Description = description;
            IsRequired = required;
            Order = order;
            TemplateBlockValueGuid = templateBlockValueGuid;

            if ( key.IsNotNullOrWhiteSpace() )
            {
                Key = key;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BlockTemplateFieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BlockTemplateFieldAttribute( string name )
            : base( SystemGuid.FieldType.BLOCK_TEMPLATE.AsGuid(), name )
        {
        }

        /// <summary>
        /// Gets or sets the template block unique identifier.
        /// </summary>
        /// <value>
        /// The template block unique identifier.
        /// </value>
        public string TemplateBlockValueGuid
        {
            get
            {
                var definedTypeGuid = FieldConfigurationValues.GetValueOrNull( "templateblock" ).AsGuidOrNull();
                if ( definedTypeGuid.HasValue )
                {
                    return definedTypeGuid.Value.ToString();
                }
                else
                {
                    return null;
                }
            }
            set
            {
                var definedTypeGuid = value.AsGuidOrNull();
                if ( !definedTypeGuid.HasValue )
                {
                    definedTypeGuid = Guid.Empty;
                }

                FieldConfigurationValues.AddOrReplace( "templateblock", new Field.ConfigurationValue( definedTypeGuid.Value.ToString() ) );
            }
        }
    }
}
