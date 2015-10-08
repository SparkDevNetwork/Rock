// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
        public IQueryable<FieldType> GetByName( string name )
        {
            return Queryable().Where( t => t.Name == name );
        }
        
        /// <summary>
        /// Returns a <see cref="Rock.Model.FieldType"/> by its Guid identifier.
        /// </summary>
        /// <param name="guid">A <see cref="System.Guid"/> representing the Guid identifier of the <see cref="Rock.Model.FieldType"/> to retrieve.</param>
        /// <returns>The <see cref="Rock.Model.FieldType"/> with a Guid identifier that matches the specified value.</returns>
        public Rock.Model.FieldType GetByGuid( Guid guid )
        {
            return Queryable().FirstOrDefault( t => t.Guid == guid );
        }

        /// <summary>
        /// Gets a list of all <see cref="Rock.Model.FieldType">FieldTypes</see> (all items that implement the <see cref="Rock.Field.IFieldType"/> interface) and registers the 
        /// <see cref="Rock.Model.FieldType">FieldTypes</see> that have not been previously registered.
        /// </summary>
        /// <param name="physWebAppPath">A <see cref="System.String"/> representing the physical path of the web application.</param>
        public static void RegisterFieldTypes( string physWebAppPath )
        {
            var fieldTypes = new Dictionary<string, EntityType>();

            using ( var rockContext = new RockContext() )
            {
                var fieldTypeService = new FieldTypeService( rockContext );

                var existingFieldTypes = fieldTypeService.Queryable().ToList();

                foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Field.IFieldType ) ) )
                {
                    string assemblyName = type.Value.Assembly.GetName().Name;
                    string className = type.Value.FullName;

                    if ( !existingFieldTypes.Where( f =>
                        f.Assembly == assemblyName &&
                        f.Class == className ).Any() )
                    {
                        string fieldTypeName = type.Value.Name.SplitCase();
                        if ( fieldTypeName.EndsWith( " Field Type" ) )
                        {
                            fieldTypeName = fieldTypeName.Substring( 0, fieldTypeName.Length - 11 );
                        }
                        var fieldType = new FieldType();
                        fieldType.Name = fieldTypeName;
                        fieldType.Assembly = assemblyName;
                        fieldType.Class = className;
                        fieldType.IsSystem = false;
                        fieldTypeService.Add( fieldType );
                    }
                }

                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Gets the Guid for the FieldType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = Rock.Web.Cache.FieldTypeCache.Read( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }
    }
}
