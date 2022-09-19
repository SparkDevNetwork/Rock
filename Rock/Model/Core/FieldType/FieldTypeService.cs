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

using Rock;
using Rock.Data;
using Rock.Web.Cache;

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
        /// Gets a list of all <see cref="Rock.Model.FieldType">FieldTypes</see> (all items that implement the <see cref="Rock.Field.IFieldType" /> interface) and registers the
        /// <see cref="Rock.Model.FieldType">FieldTypes</see> that have not been previously registered.
        /// </summary>
        /// <param name="physWebAppPath">The physical web application path.</param>
        [Obsolete( "Use the RegisterFieldTypes() that doesn't have any parameters (physWebAppPath is never used)" )]
        [RockObsolete( "1.11" )]
        public static void RegisterFieldTypes( string physWebAppPath )
        {
            RegisterFieldTypes();
        }

        /// <summary>
        /// Gets a list of all <see cref="Rock.Model.FieldType">FieldTypes</see> (all items that implement the <see cref="Rock.Field.IFieldType" /> interface) and registers the
        /// <see cref="Rock.Model.FieldType">FieldTypes</see> that have not been previously registered.
        /// </summary>
        public static void RegisterFieldTypes()
        {
            var fieldTypes = new Dictionary<string, EntityType>();

            bool changesMade = false;

            using ( var rockContext = new RockContext() )
            {
                var fieldTypeService = new FieldTypeService( rockContext );

                var existingFieldTypes = fieldTypeService.Queryable().ToList();

                foreach ( var type in Rock.Reflection.FindTypes( typeof( Rock.Field.IFieldType ) ).Values )
                {
                    string assemblyName = type.Assembly.GetName().Name;
                    string className = type.FullName;

                    var fieldType = existingFieldTypes.FirstOrDefault( t => t.Assembly == assemblyName && t.Class == className );

                    if ( fieldType == null )
                    {
                        fieldType = new FieldType();

                        fieldType.IsSystem = false;
                        string fieldTypeName = type.Name.SplitCase();
                        if ( fieldTypeName.EndsWith( " Field Type" ) )
                        {
                            fieldTypeName = fieldTypeName.Substring( 0, fieldTypeName.Length - 11 );
                        }

                        fieldType.Name = fieldTypeName;
                        fieldType.Assembly = assemblyName;
                        fieldType.Class = className;
                        fieldTypeService.Add( fieldType );

                        changesMade = true;
                    }

                    var fieldTypeGuidFromAttribute = type.GetCustomAttribute<Rock.SystemGuid.FieldTypeGuidAttribute>( inherit: false )?.Guid;
                    if ( fieldTypeGuidFromAttribute.HasValue && fieldType.Guid != fieldTypeGuidFromAttribute.Value )
                    {
                        fieldType.Guid = fieldTypeGuidFromAttribute.Value;
                        changesMade = true;
                    }
                }

                if ( changesMade )
                {
                    try
                    {
                        rockContext.SaveChanges();
                    }
                    catch ( Exception thrownException )
                    {
                        // if the exception was due to a duplicate Guid, throw as a duplicateGuidException. That'll make it easier to troubleshoot.
                        var duplicateGuidException = Rock.SystemGuid.DuplicateSystemGuidException.CatchDuplicateSystemGuidException( thrownException, null );
                        if ( duplicateGuidException != null )
                        {
                            throw duplicateGuidException;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the Guid for the FieldType that has the specified Id
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns></returns>
        public override Guid? GetGuid( int id )
        {
            var cacheItem = FieldTypeCache.Get( id );
            if ( cacheItem != null )
            {
                return cacheItem.Guid;
            }

            return null;

        }
    }
}
