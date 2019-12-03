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
using Rock.Web.Cache;

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
        public BlockTemplateFieldAttribute( string templateBlockValueGuid = "", string name = "", string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base( name, description, required, defaultValue, category, order, key, typeof( Rock.Field.Types.BlockTemplateFieldType ).FullName )
        {
            if ( !string.IsNullOrWhiteSpace( templateBlockValueGuid ) )
            {
                Guid guid = Guid.Empty;
                if ( Guid.TryParse( templateBlockValueGuid, out guid ) )
                {
                    var definedValue = DefinedValueCache.Get( guid );
                    if ( definedValue != null )
                    {
                        var configValue = new Field.ConfigurationValue( definedValue.Id.ToString() );
                        FieldConfigurationValues.Add( Rock.Field.Types.BlockTemplateFieldType.TEMPLATE_BLOCK_KEY, configValue );

                    }
                }
            }
        }
    }
}