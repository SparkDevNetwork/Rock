using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

#if NET5_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
#endif

using Rock;
using Rock.Data;

namespace TestApp
{
    public static class EntityTester
    {
        public static string CountAllEntities( IEnumerable<Type> entityTypes )
        {
            var sb = new StringBuilder();

            foreach ( var entityType in entityTypes.OrderBy( a => a.Name ) )
            {
                using ( var rockContext = new RockContext() )
                {
                    try
                    {
                        var service = Rock.Reflection.GetServiceForEntityType( entityType, rockContext );
                        var qry = GetEntityQueryable( entityType, rockContext );

                        var items = qry.ToList();

                        sb.AppendLine( $"{entityType.Name}: {items.Count} items." );
                    }
                    catch ( Exception ex )
                    {
                        sb.AppendLine( $"{entityType.Name}: {ex.Message}" );
                    }
                }
            }

            return sb.ToString();
        }

        public static Dictionary<string, List<object>> DumpEntities( IEnumerable<Type> entityTypes )
        {
            var entityTypeHolder = new Dictionary<string, List<object>>();

            foreach ( var entityType in entityTypes.OrderBy( a => a.Name ) )
            {
                var rockContext = new RockContext();

                var entityHolder = new List<object>();
                entityTypeHolder.Add( entityType.Name, entityHolder );

                var properties = GetProperties( entityType, rockContext ).ToList();
                var navigationProperties = GetNavigationProperties( entityType, rockContext ).ToList();

                try
                {
                    var service = Rock.Reflection.GetServiceForEntityType( entityType, rockContext );
                    var qry = GetEntityQueryable( entityType, rockContext );

                    var items = qry.OrderBy( a => a.Id ).ToList();

                    foreach ( var item in items )
                    {
                        entityHolder.Add( DumpEntity( item, properties, navigationProperties, rockContext ) );
                    }
                }
                catch ( Exception ex )
                {
                    entityHolder.Add( new Dictionary<string, string> { ["Error"] = ex.Message } );
                }

                rockContext.Dispose();
            }

            return entityTypeHolder;
        }

        public static IEnumerable<IEntity> GetEntityQueryable( Type entityType, RockContext rockContext )
        {
            var service = Rock.Reflection.GetServiceForEntityType( entityType, rockContext );

            var queryableMethod = service.GetType().GetMethod( "Queryable",
                BindingFlags.Instance | BindingFlags.Public,
                null,
                new Type[0],
                null );

            var qry = ( IQueryable ) queryableMethod.Invoke( service, null );

            foreach ( object obj in qry )
            {
                yield return ( IEntity ) obj;
            }
        }

#if NET5_0_OR_GREATER
        private static IEnumerable<PropertyInfo> GetProperties( Type entityType, RockContext rockContext )
        {
            var entityModel = rockContext.Model.FindEntityType( entityType );

            return entityModel.GetProperties().Select( p => p.PropertyInfo );
        }

        private static IEnumerable<PropertyInfo> GetNavigationProperties( Type entityType, RockContext rockContext )
        {
            var entityModel = rockContext.Model.FindEntityType( entityType );

            return entityModel.GetNavigations().Select( p => p.PropertyInfo );
        }
#else
        private static IEnumerable<PropertyInfo> GetProperties( Type entityType, RockContext rockContext )
        {
            var workspace = ( ( System.Data.Entity.Infrastructure.IObjectContextAdapter ) rockContext ).ObjectContext.MetadataWorkspace;
            var itemCollection = ( System.Data.Entity.Core.Metadata.Edm.ObjectItemCollection ) ( workspace.GetItemCollection( System.Data.Entity.Core.Metadata.Edm.DataSpace.OSpace ) );
            var entityTypeModel = itemCollection.OfType<System.Data.Entity.Core.Metadata.Edm.EntityType>().Single( e => itemCollection.GetClrType( e ) == entityType );
            foreach ( var property in entityTypeModel.Properties )
            {
                yield return entityType.GetProperty( property.Name );
            }
        }

        private static IEnumerable<PropertyInfo> GetNavigationProperties( Type entityType, RockContext rockContext )
        {
            var workspace = ( ( System.Data.Entity.Infrastructure.IObjectContextAdapter ) rockContext ).ObjectContext.MetadataWorkspace;
            var itemCollection = ( System.Data.Entity.Core.Metadata.Edm.ObjectItemCollection ) ( workspace.GetItemCollection( System.Data.Entity.Core.Metadata.Edm.DataSpace.OSpace ) );
            var entityTypeModel = itemCollection.OfType<System.Data.Entity.Core.Metadata.Edm.EntityType>().Single( e => itemCollection.GetClrType( e ) == entityType );
            foreach ( var navigationProperty in entityTypeModel.NavigationProperties )
            {
                yield return entityType.GetProperty( navigationProperty.Name );
            }
        }
#endif

        public static Dictionary<string, object> DumpEntity( IEntity entity, ICollection<PropertyInfo> properties, ICollection<PropertyInfo> navigationProperties, RockContext rockContext )
        {
            var entityType = entity.GetType();

            if ( entityType.IsDynamicProxyType() )
            {
                entityType = entityType.BaseType;
            }

            var dump = new Dictionary<string, object>();

            foreach ( var property in properties.OrderBy( p => p.Name ) )
            {
                var value = property.GetValue( entity );

                if ( value != null && value.GetType().IsPrimitive )
                {
                    dump.Add( property.Name, value );
                }
                else if ( value != null )
                {
                    dump.Add( property.Name, value.ToString() );
                }
                else
                {
                    dump.Add( property.Name, null );
                }
            }

            foreach ( var navigationProperty in navigationProperties.OrderBy( p => p.Name ) )
            {
                var value = navigationProperty.GetValue( entity );

                if ( value is IEntity navigationEntity )
                {
                    dump.Add( navigationProperty.Name, $"{navigationEntity.Id}:{navigationEntity.Guid}" );
                }
                else
                {
                    dump.Add( navigationProperty.Name, null );
                }
            }

            return dump;
        }
    }
}
