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
    /// Field Type POCO Service class
    /// </summary>
    public partial class FieldTypeService
    {
        /// <summary>
        /// Gets Field Types by Name
        /// </summary>
        /// <param name="name">Name.</param>
        /// <returns>An enumerable list of FieldType objects.</returns>
        public IEnumerable<FieldType> GetByName( string name )
        {
            return Repository.Find( t => t.Name == name );
        }
        
        /// <summary>
        /// Gets Field Types by Guid
        /// </summary>
        /// <param name="guid">Guid.</param>
        /// <returns>FieldType object.</returns>
        public Rock.Model.FieldType GetByGuid( Guid guid )
        {
            return Repository.FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets a list of ISecured entities (all models) that have not yet been registered and adds them
        /// as an entity type.
        /// </summary>
        /// <param name="physWebAppPath">the physical path of the web application</param>
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
