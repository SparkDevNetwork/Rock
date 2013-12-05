//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Rock;
using Rock.Data;

namespace Rock.Model
{
    /// <summary>
    /// Service/Data Access class for <see cref="Rock.Model.FieldType"/> entity objects.
    /// </summary>
    public partial class FieldTypeService
    {
        /// <summary>
        /// Returns an enumerable collection of <see cref="Rock.Model.FieldType">FieldTypes</see> by Name.
        /// </summary>
        /// <param name="name">A <see cref="System.String"/> represents the Name of the <see cref="Rock.Model.FieldType">FieldType(s)</see> to retrieve.</param>
        /// <returns>An enumerable collection of <see cref="Rock.Model.FieldType">FieldTypes</see> with a name that matches the specified value.</returns>
        public IEnumerable<FieldType> GetByName( string name )
        {
            return Repository.Find( t => t.Name == name );
        }
        
        /// <summary>
        /// Returns a <see cref="Rock.Model.FieldType"/> by it's Guid identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.FieldType"/> to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.FieldType"/> with a Guid identifier that matches the specified value.</returns>
        public Rock.Model.FieldType GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets a list of all <see cref="Rock.Model.FieldType">FieldTypes</see> (all items that implement the <see cref="Rock.Field.IFieldType"/> interface) and registers the 
        /// <see cref="Rock.Model.FieldType">FieldTypes</see> that have not been previously registered.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String"/> representing the physical path of the web application.</param>
        public void RegisterFieldTypes( string physWebAppPath )
        {
            var fieldTypes = new Dictionary<string, EntityType>();

            foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Field.IFieldType ) ) )
            {
                string assemblyName = type.Value.Assembly.GetName().Name;
                string className = type.Value.FullName;

                if ( !this.Queryable().Where( f =>
                    f.Assembly == assemblyName &&
                    f.Class == className ).Any() )
                {
                    string fieldTypeName = type.Value.Name.SplitCase();
                    if (fieldTypeName.EndsWith(" Field Type"))
                    {
                        fieldTypeName = fieldTypeName.Substring( 0, fieldTypeName.Length - 11 );
                    }
                    var fieldType = new FieldType();
                    fieldType.Name = fieldTypeName;
                    fieldType.Assembly = assemblyName;
                    fieldType.Class = className;
                    fieldType.IsSystem = false;
                    this.Add( fieldType, null );
                    this.Save( fieldType, null );
                }
            }
        }
    }
}
