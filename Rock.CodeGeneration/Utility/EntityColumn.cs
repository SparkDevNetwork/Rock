using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

using Rock.CodeGeneration.Lava;
using Rock.Data;
using Rock.Model;

namespace Rock.CodeGeneration.Utility
{
    /// <summary>
    /// Various bits of logic for dealing with columns that are to be passed
    /// to the Lava templates for processing.
    /// </summary>
    public class EntityColumn : LavaDynamic
    {
        #region Fields

        /// <summary>
        /// The simple primitive types.
        /// </summary>
        private static readonly Type[] _primitiveTypes = new[]
        {
            typeof( bool ),
            typeof( bool? ),
            typeof( int ),
            typeof( int? ),
            typeof( long ),
            typeof( long? ),
            typeof( decimal ),
            typeof( decimal? ),
            typeof( double ),
            typeof( double? ),
            typeof( string ),
            typeof( Guid ),
            typeof( Guid? )
        };

        /// <summary>
        /// The numeric types.
        /// </summary>
        private static readonly Type[] _numericTypes = new[]
        {
            typeof( int ),
            typeof( int? ),
            typeof( long ),
            typeof( long? ),
            typeof( decimal ),
            typeof( decimal? ),
            typeof( double ),
            typeof( double? )
        };

        /// <summary>
        /// The simple date types.
        /// </summary>
        private static readonly Type[] _dateTypes = new[]
        {
            typeof( DateTime ),
            typeof( DateTime? ),
            typeof( DateTimeOffset ),
            typeof( DateTimeOffset? )
        };

        #endregion

        #region Properties

        /// <summary>
        /// Gets the property information.
        /// </summary>
        /// <value>The property information.</value>
        public PropertyInfo PropertyInfo { get; }

        /// <summary>
        /// Gets the type of the property.
        /// </summary>
        /// <value>The type of the property.</value>
        public Type PropertyType => PropertyInfo.PropertyType;

        /// <summary>
        /// Gets the name of the property.
        /// </summary>
        /// <value>The name of the property.</value>
        public string Name => PropertyInfo.Name;

        /// <summary>
        /// Gets the friendly name of the column.
        /// </summary>
        /// <value>The friendly name of the column.</value>
        public string FriendlyName
        {
            get
            {
                // Strip "Value" off end of defined value navigations.
                if ( PropertyType == typeof( Rock.Model.DefinedValue ) && Name.EndsWith( "Value" ) )
                {
                    return Name.Substring( 0, Name.Length - 5 );
                }

                // Strip "PersonAlias" off end of person alias navigations.
                if ( PropertyType == typeof( Rock.Model.PersonAlias ) && Name.EndsWith( "PersonAlias" ) )
                {
                    return Name.Substring( 0, Name.Length - 11 );
                }

                // Strip "Alias" off end of person alias navigations.
                if ( PropertyType == typeof( Rock.Model.PersonAlias ) && Name.EndsWith( "Alias" ) )
                {
                    return Name.Substring( 0, Name.Length - 5 );
                }

                return Name;
            }
        }

        /// <summary>
        /// Gets the code to add the field to the builder.
        /// </summary>
        /// <value>The code to add the field to the builder.</value>
        public string AddFieldCode => GetAddFieldCode();

        /// <summary>
        /// Gets the column template code.
        /// </summary>
        /// <value>The column template code.</value>
        public string TemplateCode => GetTemplateCode();

        /// <summary>
        /// Gets the name of the imports from the Grid package.
        /// </summary>
        /// <value>The name of the imports from the Grid package.</value>
        public IEnumerable<string> GridImports => GetGridImports();

        /// <summary>
        /// Gets a value indicating whether this property is an entity.
        /// </summary>
        /// <value><c>true</c> if this property is an entity; otherwise, <c>false</c>.</value>
        public bool IsEntity => typeof( Data.IEntity ).IsAssignableFrom( PropertyType );

        #endregion

        #region Constructors

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityColumn"/> class.
        /// </summary>
        /// <param name="propertyInfo">The property to be represented by this instance.</param>
        public EntityColumn( PropertyInfo propertyInfo )
        {
            PropertyInfo = propertyInfo;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Gets the C# code that will handle adding the field to the builder.
        /// </summary>
        /// <returns>A string that represents the C# code.</returns>
        public string GetAddFieldCode()
        {
            // Check if date type.
            if ( _dateTypes.Contains( PropertyType ) )
            {
                return $".AddDateTimeField( \"{FriendlyName.CamelCase()}\", a => a.{Name} )";
            }

            // Check if Person.
            if ( PropertyType == typeof( Rock.Model.PersonAlias ) )
            {
                return $".AddPersonField( \"{FriendlyName.CamelCase()}\", a => a.{Name}?.Person )";
            }

            // Check if generic entity.
            if ( typeof( IEntity ).IsAssignableFrom( PropertyType ) )
            {
                if ( PropertyType.GetProperty( "Name" ) != null )
                {
                    return $".AddTextField( \"{FriendlyName.CamelCase()}\", a => a.{Name}?.Name )";
                }
                else if ( PropertyType.GetProperty( "Title" ) != null )
                {
                    return $".AddTextField( \"{FriendlyName.CamelCase()}\", a => a.{Name}?.Title )";
                }
                else if ( PropertyType == typeof( Rock.Model.DefinedValue ) )
                {
                    return $".AddTextField( \"{FriendlyName.CamelCase()}\", a => a.{Name}?.Value )";
                }
                else
                {
                    return $".AddTextField( \"{FriendlyName.CamelCase()}\", a => throw new NotSupportedException() )";
                }
            }

            // Check for string type.
            if ( PropertyType == typeof( string ) )
            {
                return $".AddTextField( \"{FriendlyName.CamelCase()}\", a => a.{Name} )";
            }

            // Check if it is a simple primitive type.
            if ( _primitiveTypes.Contains( PropertyType ) )
            {
                return $".AddField( \"{FriendlyName.CamelCase()}\", a => a.{Name} )";
            }

            return $".AddField( \"{FriendlyName.CamelCase()}, a => throw new NotSupportedException() )";
        }

        /// <summary>
        /// Gets the C# code that will handle the code for the column template
        /// definition in the Obsidian file.
        /// </summary>
        /// <returns>A string that represents the C# code.</returns>
        public string GetTemplateCode()
        {
            // Check for string types.
            if ( PropertyType == typeof( string ) || PropertyType == typeof( Guid ) || PropertyType == typeof( Guid? ) )
            {
                return $@"
        <TextColumn name=""{FriendlyName.CamelCase()}""
                    title=""{FriendlyName.SplitCase()}""
                    field=""{FriendlyName.CamelCase()}""
                    :filter=""textValueFilter""
                    visiblePriority=""xs"" />".Trim();
            }

            // Check for numeric types.
            if ( _numericTypes.Contains( PropertyType ) )
            {
                return $@"
        <NumberColumn name=""{FriendlyName.CamelCase()}""
                       title=""{FriendlyName.SplitCase()}""
                       field=""{FriendlyName.CamelCase()}""
                       :filter=""numberValueFilter""
                       visiblePriority=""xs"" />".Trim();
            }

            // Check for boolean types.
            if ( PropertyType == typeof( bool ) || PropertyType == typeof( bool? ) )
            {
                return $@"
        <BooleanColumn name=""{FriendlyName.CamelCase()}""
                       title=""{FriendlyName.SplitCase()}""
                       field=""{FriendlyName.CamelCase()}""
                       visiblePriority=""xs"" />".Trim();
            }

            // Check for date types.
            if ( _dateTypes.Contains( PropertyType ) )
            {
                return $@"
        <DateColumn name=""{FriendlyName.CamelCase()}""
                    title=""{FriendlyName.SplitCase()}""
                    field=""{FriendlyName.CamelCase()}""
                    :filter=""dateValueFilter""
                    visiblePriority=""xs"" />".Trim();
            }

            // Check for Person types.
            if ( PropertyType == typeof( PersonAlias ) )
            {
                return $@"
        <PersonColumn name=""{FriendlyName.CamelCase()}""
                      title=""{FriendlyName.SplitCase()}""
                      field=""{FriendlyName.CamelCase()}""
                      :filter=""pickExistingValueFilter""
                      visiblePriority=""xs"" />".Trim();
            }

            // Check for generic entity types.
            if ( typeof( IEntity ).IsAssignableFrom( PropertyType ) )
            {
                return $@"
        <TextColumn name=""{FriendlyName.CamelCase()}""
                    title=""{FriendlyName.SplitCase()}""
                    field=""{FriendlyName.CamelCase()}""
                    :filter=""textValueFilter""
                    visiblePriority=""xs"" />".Trim();
            }

            return $@"
        <Column name=""{FriendlyName.CamelCase()}""
                title=""{FriendlyName.SplitCase()}""
                visiblePriority=""xs"">
            <template #format=""{{ row }}"">
                {{{{ row.{FriendlyName.CamelCase()} }}}}
            </template>
        </Column>
".Trim();
        }

        /// <summary>
        /// Gets the template column name.
        /// </summary>
        /// <returns>A collection of strings that contains the import names.</returns>
        private IEnumerable<string> GetGridImports()
        {
            // Check for string types.
            if ( PropertyType == typeof( string ) || PropertyType == typeof( Guid ) || PropertyType == typeof( Guid? ) )
            {
                return new[] { "TextColumn", "textValueFilter" };
            }

            // Check for numeric types.
            if ( _numericTypes.Contains( PropertyType ) )
            {
                return new[] { "NumberColumn", "numberValueFilter" };
            }

            // Check for boolean types.
            if ( PropertyType == typeof( bool ) || PropertyType == typeof( bool? ) )
            {
                return new[] { "BooleanColumn" };
            }

            // Check for date types.
            if ( _dateTypes.Contains( PropertyType ) )
            {
                return new[] { "DateColumn", "dateValueFilter" };
            }

            // Check for Person types.
            if ( PropertyType == typeof( PersonAlias ) )
            {
                return new[] { "PersonColumn", "pickExistingValueFilter" };
            }

            // Check for generic entity types.
            if ( typeof( IEntity ).IsAssignableFrom( PropertyType ) )
            {
                return new[] { "TextColumn", "textValueFilter" };
            }

            return new[] { "Column" };
        }

        /// <summary>
        /// Determines whether the type is one that is supported for normal
        /// code generation operations.
        /// </summary>
        /// <param name="type">The type to be checked.</param>
        /// <returns><c>true</c> if the type is supported; otherwise, <c>false</c>.</returns>
        public static bool IsSupportedPropertyType( Type type )
        {
            // If the type is one of the few supported entity types or one of
            // the known primitive types then it is considered supported.
            if ( typeof( IEntity ).IsAssignableFrom( type ) )
            {
                return true;
            }
            else if ( _primitiveTypes.Contains( type ) )
            {
                return true;
            }
            else if ( _dateTypes.Contains( type ) )
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}
