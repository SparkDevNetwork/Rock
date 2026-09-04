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

using Rock.Enums.Cms;
using Rock.Field;
using Rock.Web.Cache;

namespace Rock.Attribute
{
    /// <summary>
    /// A class Attribute that can be used by any object that inherits from <see cref="Rock.Attribute.IHasAttributes"/> to specify what attributes it needs.  The 
    /// Framework provides methods in the <see cref="Rock.Attribute.Helper"/> class to create, read, and update the attributes
    /// </summary>
    /// <remarks>
    /// </remarks>
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = true )]
    public abstract class FieldAttribute : System.Attribute
    {
        private string _fieldTypeClass;
        private string _fieldTypeAssembly;
        private Guid? _fieldTypeGuid;

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAttribute" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        /// <param name="fieldTypeAssembly">The field type assembly.</param>
        /// <param name="fieldTypeClass">The field type class.</param>
        [Obsolete( "Use the constructor that takes a fieldTypeGuid instead." )]
        [RockObsolete( "20.0" )]
        public FieldAttribute( string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null, string fieldTypeClass = null, string fieldTypeAssembly = "Rock" )
            : base()
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                Key = name.Replace( " ", string.Empty );
            }
            else
            {
                Key = key;
            }

            if ( string.IsNullOrWhiteSpace( fieldTypeClass ) )
            {
                fieldTypeClass = "Rock.Field.Types.TextFieldType";
            }

            Name = name;
            Category = category;
            Description = description;
            IsRequired = required;
            DefaultValue = defaultValue;
            Order = order;
            FieldTypeAssembly = fieldTypeAssembly;
            FieldTypeClass = fieldTypeClass;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAttribute" /> class.
        /// </summary>
        /// <param name="fieldTypeGuid">The unique identifier of the field type.</param>
        /// <param name="name">The name.</param>
        /// <param name="description">The description.</param>
        /// <param name="required">if set to <c>true</c> [required].</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="category">The category.</param>
        /// <param name="order">The order.</param>
        /// <param name="key">The key.</param>
        [Obsolete( "Use the constructor that takes a fieldTypeGuid and name only." )]
        [RockObsolete( "20.0" )]
        public FieldAttribute( Guid fieldTypeGuid, string name, string description = "", bool required = true, string defaultValue = "", string category = "", int order = 0, string key = null )
            : base()
        {
            if ( string.IsNullOrWhiteSpace( key ) )
            {
                Key = name.Replace( " ", string.Empty );
            }
            else
            {
                Key = key;
            }

            Name = name;
            Category = category;
            Description = description;
            FieldTypeGuid = fieldTypeGuid;
            IsRequired = required;
            DefaultValue = defaultValue;
            Order = order;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FieldAttribute" /> class.
        /// </summary>
        /// <param name="fieldTypeGuid">The unique identifier of the field type.</param>
        /// <param name="name">The name.</param>
        public FieldAttribute( Guid fieldTypeGuid, string name )
        {
            Category = string.Empty;
            DefaultValue = string.Empty;
            Description = string.Empty;
            FieldTypeGuid = fieldTypeGuid;
            IsRequired = true;
            Key = name.Replace( " ", string.Empty );
            Name = name;
            Order = 0;
        }

        /// <summary>
        /// Gets or sets the user-friendly name of the attribute
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public virtual string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the attribute
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public virtual string Description { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is required.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is required; otherwise, <c>false</c>.
        /// </value>
        public virtual bool IsRequired { get; set; }

        /// <summary>
        /// Gets or sets the default value of the attribute.  This is the value that will be used if a specific value has not yet been created
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public virtual string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        /// <value>
        /// The category.
        /// </value>
        public virtual string Category { get; set; }

        /// <summary>
        /// Gets or sets the order of the attribute.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public virtual int Order { get; set; }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        /// <value>
        /// The key.
        /// </value>
        public virtual string Key { get; set; }

        /// <summary>
        /// Gets or sets the assembly name of the <see cref="Rock.Field.IFieldType"/> to be used for the attribute
        /// </summary>
        /// <value>
        /// The field type assembly.
        /// </value>
        [Obsolete( "Use FieldTypeGuid instead." )]
        [RockObsolete( "20.0" )]
        public virtual string FieldTypeAssembly
        {
            get
            {
                if ( _fieldTypeAssembly == null )
                {
                    PopulateLegacyClassAndAssembly();
                }

                return _fieldTypeAssembly ?? string.Empty;
            }
            set
            {
                // The obsolete constructor writes FieldTypeAssembly and
                // FieldTypeClass separately in sequence, so this setter
                // must not clobber _fieldTypeClass here. We null only
                // _fieldTypeGuid so the next FieldTypeGuid read re-resolves
                // against the (assembly, class) pair. Mixing legacy setter
                // writes with FieldTypeGuid writes is not a supported
                // pattern; if you need to change the field type, set
                // FieldTypeGuid instead.
                _fieldTypeAssembly = value;
                _fieldTypeGuid = null;
            }
        }

        /// <summary>
        /// Gets or sets the class name of the <see cref="Rock.Field.IFieldType"/> to be used for the attribute.
        /// </summary>
        /// <value>
        /// The field type class.
        /// </value>
        [Obsolete( "Use FieldTypeGuid instead." )]
        [RockObsolete( "20.0" )]
        public virtual string FieldTypeClass
        {
            get
            {
                if ( _fieldTypeClass == null )
                {
                    PopulateLegacyClassAndAssembly();
                }

                return _fieldTypeClass ?? string.Empty;
            }
            set
            {
                // The obsolete constructor writes FieldTypeAssembly and
                // FieldTypeClass separately in sequence, so this setter
                // must not clobber _fieldTypeAssembly here. We null only
                // _fieldTypeGuid so the next FieldTypeGuid read re-resolves
                // against the (assembly, class) pair. Mixing legacy setter
                // writes with FieldTypeGuid writes is not a supported
                // pattern; if you need to change the field type, set
                // FieldTypeGuid instead.
                _fieldTypeClass = value;
                _fieldTypeGuid = null;
            }
        }

        /// <summary>
        /// The unique identifier of the field type that will handle the
        /// configuration and UI for this attribute.
        /// </summary>
        /// <remarks>
        /// This is a string instead of a Guid so that it can be used in
        /// C# attribute constructors.
        /// </remarks>
        public Guid FieldTypeGuid
        {
            get
            {
                if ( _fieldTypeGuid == null )
                {
                    // Legacy fallback: this path only runs when the
                    // attribute was constructed via the obsolete
                    // fieldTypeClass constructor. Full-cache walks and
                    // cache-miss retries are acceptable here because this
                    // is not a hot path in normal Rock usage.
                    try
                    {
                        // Only check the class and ignore the assembly because
                        // the assembly might have changed since the attribute
                        // was created.
                        var fieldType = FieldTypeCache.All()
                            .FirstOrDefault( c => c.Class == _fieldTypeClass );

                        if ( fieldType == null )
                        {
                            // Don't set _fieldTypeGuid to Guid.Empty here
                            // because that would prevent future attempts to
                            // look up the field type by class name once the
                            // cache is warm.
                            return Guid.Empty;
                        }

                        _fieldTypeGuid = fieldType.Guid;
                    }
                    catch
                    {
                        // Don't set _fieldTypeGuid to Guid.Empty here
                        // because that would prevent future attempts to
                        // look up the field type by class name once the
                        // cache is available.
                        return Guid.Empty;
                    }
                }

                return _fieldTypeGuid ?? Guid.Empty;
            }
            set
            {
                _fieldTypeGuid = value;
                _fieldTypeAssembly = null;
                _fieldTypeClass = null;
            }
        }

        /// <summary>
        /// The site types this attribute will be displayed on. This is currently
        /// only valid for block settings. If not set then all sites will be
        /// considered valid.
        /// </summary>
        public SiteTypeFlags SiteTypes { get; set; }

        /// <summary>
        /// Gets or sets the field configuration values.
        /// </summary>
        /// <value>
        /// The field configuration values.
        /// </value>
        public virtual Dictionary<string, ConfigurationValue> FieldConfigurationValues
        {
            get
            {
                return fieldConfigurationValues;
            }
            set
            {
                FieldConfigurationValues = value;
            }
        }
        private Dictionary<string, ConfigurationValue> fieldConfigurationValues = new Dictionary<string, ConfigurationValue>();

        /// <summary>
        /// Populate the <strong>FieldTypeClass</strong> and
        /// <strong>FieldTypeAssembly</strong> properties based on the
        /// <see cref="FieldTypeGuid"/> value.
        /// </summary>
        /// <remarks>
        /// FieldTypeClass and FieldTypeAssembly are legacy properties and
        /// are not expected to be read at runtime in normal Rock code paths.
        /// They exist only for backwards compatibility with plugins that
        /// reflect on FieldAttribute. On cache miss (which can happen during
        /// early attribute reflection before FieldTypeCache is warm), we
        /// deliberately leave the backing fields null so a subsequent read
        /// after the cache warms up will retry the lookup. Caching a miss
        /// as string.Empty would permanently return an empty value even
        /// once the field type becomes resolvable.
        /// </remarks>
        [Obsolete( "This is a legacy support method and should be removed when FieldTypeClass is removed." )]
        [RockObsolete( "20.0" )]
        private void PopulateLegacyClassAndAssembly()
        {
            // Route through the FieldTypeGuid property so the class-name
            // fallback fires when only FieldTypeClass has been set.
            var guid = FieldTypeGuid;

            if ( guid == Guid.Empty )
            {
                return;
            }

            try
            {
                var fieldType = FieldTypeCache.Get( guid );

                if ( fieldType == null )
                {
                    return;
                }

                _fieldTypeAssembly ??= fieldType.Assembly;
                _fieldTypeClass ??= fieldType.Class;
            }
            catch
            {
                // Intentionally ignore exceptions since this is only used
                // for backwards compatibility with the old constructor.
                // Leave the backing fields null so a subsequent read once
                // the cache is available can succeed.
            }
        }
    }
}