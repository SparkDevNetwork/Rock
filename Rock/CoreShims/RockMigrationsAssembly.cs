using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Rock.CoreShims
{
    /// <summary>
    /// Provides information to EF Core about what migrations are available.
    /// EFTODO: Eventually need to mix in both EF6 and EF Core migrations into one.
    /// </summary>
    /// <seealso cref="IMigrationsAssembly" />
    public class RockMigrationsAssembly : IMigrationsAssembly
    {
        private readonly IMigrationsIdGenerator _idGenerator;
        private readonly Type _contextType;

        public RockMigrationsAssembly( ICurrentDbContext currentContext, IDbContextOptions options, IMigrationsIdGenerator idGenerator, IDiagnosticsLogger<DbLoggerCategory.Migrations> logger )
        {
            _idGenerator = idGenerator;
            _contextType = currentContext.Context.GetType();
            Assembly = Assembly.Load( new AssemblyName( "Rock.Migrations" ) );
        }

        public virtual IReadOnlyDictionary<string, TypeInfo> Migrations
        {
            get
            {
                if ( _migrations == null )
                {
                    var migrations = new SortedList<string, TypeInfo>();

                    var assemblyName = new AssemblyName( Guid.NewGuid().ToString() );
                    var asmBuilder = AssemblyBuilder.DefineDynamicAssembly( assemblyName, AssemblyBuilderAccess.Run );
                    var modBuilder = asmBuilder.DefineDynamicModule( assemblyName.Name );

                    var baseType = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany( a => a.GetTypes() )
                        .Single( t => t.FullName == "Rock.Migrations.RockMigration" );

                    var migrationTypes = this.Assembly.GetTypes()
                        .Where( t => t.IsSubclassOf( baseType ) && !t.IsAbstract )
                        .Where( t => t.GetCustomAttribute<MigrationAttribute>() == null )
                        .Where( t => t.GetCustomAttribute<DbContextAttribute>() == null )
                        .ToList();
                    foreach ( var migrationType in migrationTypes )
                    {
                        var tBuilder = modBuilder.DefineType( migrationType.Name, TypeAttributes.Public, typeof( CoreShims.MigrationShim ) );

                        var attrBuilder = new CustomAttributeBuilder( typeof( DbContextAttribute ).GetConstructor( new[] { typeof( Type ) } ), new[] { _contextType } );
                        tBuilder.SetCustomAttribute( attrBuilder );

                        var inst = ( System.Data.Entity.Migrations.Infrastructure.IMigrationMetadata ) Activator.CreateInstance( migrationType );
                        attrBuilder = new CustomAttributeBuilder( typeof( MigrationAttribute ).GetConstructor( new[] { typeof( string ) } ), new[] { inst.Id } );
                        tBuilder.SetCustomAttribute( attrBuilder );

                        var ctorBuilder = tBuilder.DefineConstructor( MethodAttributes.Public, CallingConventions.Standard, new Type[] { } );
                        var il = ctorBuilder.GetILGenerator();
                        il.Emit( OpCodes.Ldarg_0 ); // Push "this"
                        il.Emit( OpCodes.Ldstr, migrationType.FullName ); // Push the first parameter
                        il.Emit( OpCodes.Call, typeof( CoreShims.MigrationShim ).GetConstructor( new[] { typeof( string ) } ) );
                        il.Emit( OpCodes.Ret );

                        migrations.Add( inst.Id, tBuilder.CreateType().GetTypeInfo() );
                    }

                    _migrations = migrations;
                }

                return _migrations;
            }
        }
        private IReadOnlyDictionary<string, TypeInfo> _migrations;

        public virtual ModelSnapshot ModelSnapshot
        {
            get
            {
                var type = Assembly.GetTypes()
                    .Where( t => t.IsSubclassOf( typeof( ModelSnapshot ) ) )
                    .Where( t => t.GetCustomAttribute<DbContextAttribute>()?.ContextType == _contextType )
                    .FirstOrDefault();

                if ( type != null )
                {
                    return ( ModelSnapshot ) Activator.CreateInstance( type.GetTypeInfo().AsType() );
                }

                return null;
            }
        }

        public virtual Assembly Assembly { get; }

        public virtual string FindMigrationId( string nameOrId )
        {
            if ( _idGenerator.IsValidId( nameOrId ) )
            {
                return Migrations.Keys.FirstOrDefault( id => string.Equals( id, nameOrId, StringComparison.OrdinalIgnoreCase ) );
            }
            else
            {
                return Migrations.Keys.FirstOrDefault( id => string.Equals( _idGenerator.GetName( id ), nameOrId, StringComparison.OrdinalIgnoreCase ) );
            }
        }

        public virtual Migration CreateMigration( TypeInfo migrationClass, string activeProvider )
        {
            var migration = ( Migration ) Activator.CreateInstance( migrationClass.AsType() );
            migration.ActiveProvider = activeProvider;

            return migration;
        }
    }

}