using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Operations;

namespace Rock.CoreShims
{
    public interface ILegacyMigration
    {
        MigrationBuilder MigrationBuilder { get; set; }

        void Up();

        void Down();
    }

    /// <summary>
    /// EF Core cannot run EF 6 migrations directly for various reasons. This shim acts as a wrapper
    /// </summary>
    /// <seealso cref="Microsoft.EntityFrameworkCore.Migrations.Migration" />
    public abstract class MigrationShim : Migration
    {
        protected ILegacyMigration _realMigration;

        public MigrationShim( string realMigrationType )
        {
            var type = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany( a => a.GetTypes() )
                .Where( t => t.FullName == realMigrationType )
                .Single();
            _realMigration = ( ILegacyMigration ) Activator.CreateInstance( type );
        }

        protected override void Up( MigrationBuilder migrationBuilder )
        {
            throw new NotImplementedException(); /* This method should never be called */
        }

        protected override void Down( MigrationBuilder migrationBuilder )
        {
            throw new NotImplementedException(); /* This method should never be called */
        }

        public override IReadOnlyList<MigrationOperation> UpOperations
        {
            get
            {
                _realMigration.MigrationBuilder = new MigrationBuilder( ActiveProvider );
                _realMigration.Up();

                var operations = _realMigration.MigrationBuilder.Operations;

                //
                // Need to re-order things a bit and move any foreign key creation
                // to after any create table or add column operations.
                //
                var addForeignKeyOperations = operations.Where( o => o.GetType() == typeof( AddForeignKeyOperation ) ).ToList();

                foreach ( AddForeignKeyOperation foreignKeyOp in addForeignKeyOperations )
                {
                    var lastCreateTableOp = operations
                        .Where( o => o.GetType() == typeof( CreateTableOperation ) )
                        .Cast<CreateTableOperation>()
                        .Where( o => o.Schema == foreignKeyOp.PrincipalSchema && o.Name == foreignKeyOp.PrincipalTable )
                        .LastOrDefault();
                    var lastAddColumnOp = operations
                        .Where( o => o.GetType() == typeof( AddColumnOperation ) )
                        .Cast<AddColumnOperation>()
                        .Where( o => o.Schema == foreignKeyOp.PrincipalSchema && o.Table == foreignKeyOp.PrincipalTable && foreignKeyOp.PrincipalColumns.Contains( o.Name ) )
                        .LastOrDefault();

                    var lastIndex = Math.Max( operations.IndexOf( lastCreateTableOp ), operations.IndexOf( lastAddColumnOp ) );
                    var currentIndex = operations.IndexOf( foreignKeyOp );

                    if ( currentIndex < lastIndex )
                    {
                        operations.Remove( foreignKeyOp );
                        operations.Insert( lastIndex, foreignKeyOp );
                    }
                }

                return operations;
            }
        }

        public override IReadOnlyList<MigrationOperation> DownOperations
        {
            get
            {
                _realMigration.MigrationBuilder = new MigrationBuilder( ActiveProvider );
                _realMigration.Down();

                return _realMigration.MigrationBuilder.Operations;
            }
        }
    }
}