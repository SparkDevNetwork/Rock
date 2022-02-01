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
using System.Reflection;
using System.Runtime.Serialization;

using Rock.Attribute;
using Rock.Data;
using Rock.Lava;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Information about a fieldType that is required by the rendering engine.
    /// This information will be cached by the engine
    /// </summary>
    [Serializable]
    [DataContract]
    public class FieldTypeCache : ModelCache<FieldTypeCache, FieldType>
    {
        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether this instance is system.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is system; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IsSystem { get; private set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        [DataMember]
        public string Name { get; private set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [DataMember]
        public string Description { get; private set; }

        /// <summary>
        /// Gets or sets the assembly.
        /// </summary>
        /// <value>
        /// The assembly.
        /// </value>
        [DataMember]
        public string Assembly { get; private set; }

        /// <summary>
        /// Gets or sets the class.
        /// </summary>
        /// <value>
        /// The class.
        /// </value>
        [DataMember]
        public string Class { get; private set; }

        /// <summary>
        /// Gets the CSS class to use when displaying an icon that represents
        /// this field type.
        /// </summary>
        /// <value>
        /// The CSS class to use when displaying an icon that represents this
        /// field type.
        /// </value>
        public string IconCssClass { get; private set; }

        /// <summary>
        /// Gets the ways this field type can be used and presented in the system.
        /// If the field is not available then it will be assumed to be Advanced.
        /// </summary>
        /// <value>
        /// The ways this field type can be used and presented in the system.
        /// </value>
        public Rock.Field.FieldTypeUsage Usage { get; private set; }

        /// <summary>
        /// Gets the front-end platform(s) that this field type supports. All
        /// field types should fallback to a "Text" field type for view and edit
        /// mode if they are not supported. This property in particular
        /// specifies which platforms support creating/configurating this field
        /// type.
        /// </summary>
        /// <value>
        /// The front-end platform(s) that this field type supports.
        /// </value>
        public Utility.RockPlatform Platform { get; private set; }

        /// <summary>
        /// Gets the field.
        /// </summary>
        /// <value>
        /// The field.
        /// </value>
		[LavaVisible]
        public Field.IFieldType Field
        {
            get
            {
                if ( _field == null )
                {
                    _field = Rock.Field.Helper.InstantiateFieldType( Assembly, Class );
                }
                return _field;
            }
        }
        private Field.IFieldType _field = null;

        #endregion

        #region Public Methods

        /// <summary>
        /// Set's the cached objects properties from the model/entities properties.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var fieldType = entity as FieldType;
            if ( fieldType == null )
            {
                return;
            }

            IsSystem = fieldType.IsSystem;
            Name = fieldType.Name;
            Description = fieldType.Description;
            Assembly = fieldType.Assembly;
            Class = fieldType.Class;

            // Can't use the "Field" property because it will return a TextFieldType
            // if the real field type implementation cannot be found.
            var fieldTypeImplementationType = Type.GetType( $"{fieldType.Class}, {fieldType.Assembly}" );

            // If we found the field C# type check for the custom attributes. Do not
            // use inherited attributes since that could have unintended consequences.
            if ( fieldTypeImplementationType != null )
            {
                IconCssClass = fieldTypeImplementationType.GetCustomAttribute<IconCssClassAttribute>( false )?.IconCssClass ?? string.Empty;

                // Default to Advanced if the field type does not specify its usage.
                Usage = fieldTypeImplementationType.GetCustomAttribute<FieldTypeUsageAttribute>( false )?.Usage ?? Rock.Field.FieldTypeUsage.Advanced;

                Platform = fieldTypeImplementationType.GetCustomAttribute<RockPlatformSupportAttribute>( false )?.Platform ?? 0;
            }
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <returns></returns>
        public new static List<FieldTypeCache> All()
        {
            // use 'new' to override the base All since we want to sort field types
            return ModelCache<FieldTypeCache, FieldType>.All().OrderBy( a => a.Name ).ToList();
        }

        /// <summary>
        /// Gets all the instances of this type of model/entity that are currently in cache.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <returns></returns>
        public new static List<FieldTypeCache> All( RockContext rockContext )
        {
            // use 'new' to override the base All since we want to sort field types
            return ModelCache<FieldTypeCache, FieldType>.All( rockContext ).OrderBy( a => a.Name ).ToList();
        }

        /// <summary>
        /// Gets this instance.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static FieldTypeCache Get<T>() where T : Field.IFieldType
        {
            return All().FirstOrDefault( a => a.Class == typeof( T ).FullName );
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public static string GetName( int id )
        {
            var fieldType = Get( id );
            return fieldType?.Name;
        }

        #endregion
    }
}