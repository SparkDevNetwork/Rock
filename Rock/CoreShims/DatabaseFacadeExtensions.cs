using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Rock
{
    public static class DatabaseFacadeExtensions
    {
        /// <summary>
        /// Executes raw query with parameters and maps returned values to column property names of Model provided.
        /// Not all properties are required to be present in model (if not present - null)
        /// </summary>
        public static IEnumerable<T> SqlQuery<T>( this DatabaseFacade database, string query, params object[] parameters )
        {
            using var command = database.GetDbConnection().CreateCommand();

            command.CommandText = query;
            command.CommandType = CommandType.Text;

            if ( parameters != null )
            {
                foreach ( var parameter in parameters )
                {
                    command.Parameters.Add( parameter );
                }
            }

            database.OpenConnection();

            using var reader = command.ExecuteReader();

            if ( typeof( T ).IsPrimitive )
            {
                while ( reader.Read() )
                {
                    object val = reader.IsDBNull( 0 ) ? null : reader[0];

                    if ( val == null )
                    {
                        yield return default;
                    }
                    else
                    {
                        yield return ( T ) Convert.ChangeType( val, typeof( T ) );
                    }
                }
            }
            else
            {
                List<PropertyInfo> columns = typeof( T )
                    .GetProperties( BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic )
                    .ToList();

                while ( reader.Read() )
                {
                    var newObject = Activator.CreateInstance<T>();

                    for ( int i = 0; i < reader.FieldCount; i++ )
                    {
                        string name = reader.GetName( i );
                        var prop = columns.FirstOrDefault( a => a.Name.Equals( name ) );

                        if ( prop == null )
                        {
                            continue;
                        }

                        object val = reader.IsDBNull( i ) ? null : reader[i];

                        prop.SetValue( newObject, val, null );
                    }

                    yield return newObject;
                }
            }
        }
    }
}
