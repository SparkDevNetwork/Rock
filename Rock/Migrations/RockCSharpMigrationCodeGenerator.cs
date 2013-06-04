//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations.Design;
using System.Linq;
using System.Reflection;
using Rock.Data;

namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Target DbContext to be migrated</typeparam>
    public class RockCSharpMigrationCodeGenerator<T> : CSharpMigrationCodeGenerator where T:DbContext
    {
        #region internal custom methods

        /// <summary>
        /// 
        /// </summary>
        private Assembly contextAssembly = null;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Type> contextAssemblyTables = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCSharpMigrationCodeGenerator"/> class.
        /// </summary>
        public RockCSharpMigrationCodeGenerator()
            : base()
        {
            contextAssembly = typeof( T ).Assembly; 

            foreach ( var e in contextAssembly.GetTypes().Where( a => a.CustomAttributes.Any( x => x.AttributeType.Name.Equals( "TableAttribute" ) ) ).ToList() )
            {
                var attrib = e.CustomAttributes.FirstOrDefault( a => a.AttributeType.Name.Equals( "TableAttribute" ) );
                if ( attrib != null )
                {
                    contextAssemblyTables.Add( attrib.ConstructorArguments[0].Value.ToString(), e );
                }
            }
        }

        /// <summary>
        /// Handles the AssemblyResolve event of the CurrentDomain control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="args">The <see cref="ResolveEventArgs" /> instance containing the event data.</param>
        /// <returns></returns>
        protected Assembly CurrentDomain_AssemblyResolve( object sender, ResolveEventArgs args )
        {
            return AppDomain.CurrentDomain.Load( args.Name );
        }

        /// <summary>
        /// Generates the custom column code.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="fullTableName">Full name of the table.</param>
        /// <param name="columnName">Name of the column.</param>
        private void GenerateCustomColumnCode( System.Data.Entity.Migrations.Utilities.IndentedTextWriter writer, string fullTableName, string columnName )
        {
            string tableName = fullTableName.Replace( "dbo.", string.Empty );

            if ( contextAssemblyTables.ContainsKey( tableName ) )
            {
                Type tableType = contextAssemblyTables[tableName];

                MemberInfo mi = tableType.GetMember( columnName ).FirstOrDefault();

                if ( mi != null )
                {
                    AlternateKeyAttribute attribute = mi.GetCustomAttribute<AlternateKeyAttribute>();
                    if ( attribute != null )
                    {
                        writer.WriteLine( "CreateIndex( \"" + fullTableName + "\", \"" + columnName + "\", true );" );
                    }
                }
                else
                {
                    // probably not found if this is the Down() migration
                }
            }
            else
            {
                // not found if Migration is trying to gen code for a table in another DbContext of a Multiple Context project
                // probably not found if this is the Down() migration
                writer.WriteLine( string.Format( "// Skipping GenerateCustomColumnCode for Table {0}. Not found in context ", tableName ) );
            }
        }

        #endregion

        #region overridden methods

        /// <summary>
        /// Generates code to perform a <see cref="T:System.Data.Entity.Migrations.Model.CreateTableOperation" />.
        /// </summary>
        /// <param name="createTableOperation">The operation to generate code for.</param>
        /// <param name="writer">Text writer to add the generated code to.</param>
        protected override void Generate( System.Data.Entity.Migrations.Model.CreateTableOperation createTableOperation, System.Data.Entity.Migrations.Utilities.IndentedTextWriter writer )
        {
            string tableName = createTableOperation.Name.Replace( "dbo.", string.Empty );

            if ( contextAssemblyTables.ContainsKey( tableName ) )
            {
                base.Generate( createTableOperation, writer );

                foreach ( var column in createTableOperation.Columns )
                {
                    GenerateCustomColumnCode( writer, createTableOperation.Name, column.Name );
                }
            }
            else
            {
                // not found if Migration is trying to gen code for a table in another DbContext of a Multiple Context project
                // probably not found if this is the Down() migration
                writer.WriteLine( string.Format( "// Skipping Create Table for TableName {0}.  Not found in context. ", tableName ) );
            }
        }

        /// <summary>
        /// Generates code to perform an <see cref="T:System.Data.Entity.Migrations.Model.AddColumnOperation" />.
        /// </summary>
        /// <param name="addColumnOperation">The operation to generate code for.</param>
        /// <param name="writer">Text writer to add the generated code to.</param>
        protected override void Generate( System.Data.Entity.Migrations.Model.AddColumnOperation addColumnOperation, System.Data.Entity.Migrations.Utilities.IndentedTextWriter writer )
        {
            base.Generate( addColumnOperation, writer );
            GenerateCustomColumnCode( writer, addColumnOperation.Table, addColumnOperation.Column.Name );
        }

        /// <summary>
        /// Generates code to perform a <see cref="T:System.Data.Entity.Migrations.Model.DropForeignKeyOperation" />.
        /// </summary>
        /// <param name="dropForeignKeyOperation">The operation to generate code for.</param>
        /// <param name="writer">Text writer to add the generated code to.</param>
        protected override void Generate( System.Data.Entity.Migrations.Model.DropForeignKeyOperation dropForeignKeyOperation, System.Data.Entity.Migrations.Utilities.IndentedTextWriter writer )
        {
            string tableName = dropForeignKeyOperation.PrincipalTable.Replace( "dbo.", string.Empty );

            if ( contextAssemblyTables.ContainsKey( tableName ) )
            {
                base.Generate( dropForeignKeyOperation, writer );
            }
            else
            {
                // not found if Migration is trying to gen code for a table in another DbContext of a Multiple Context project
                // probably not found if this is the Down() migration
                writer.WriteLine( string.Format( "// Skipping Drop FK for TableName {0}.  Not found in context. ", tableName ) );
            }
        }

        /// <summary>
        /// Generates code to perform a <see cref="T:System.Data.Entity.Migrations.Model.DropIndexOperation" />.
        /// </summary>
        /// <param name="dropIndexOperation">The operation to generate code for.</param>
        /// <param name="writer">Text writer to add the generated code to.</param>
        protected override void Generate( System.Data.Entity.Migrations.Model.DropIndexOperation dropIndexOperation, System.Data.Entity.Migrations.Utilities.IndentedTextWriter writer )
        {
            string tableName = dropIndexOperation.Table.Replace( "dbo.", string.Empty );

            if ( contextAssemblyTables.ContainsKey( tableName ) )
            {
                base.Generate( dropIndexOperation, writer );
            }
            else
            {
                // not found if Migration is trying to gen code for a table in another DbContext of a Multiple Context project
                // probably not found if this is the Down() migration
                writer.WriteLine( string.Format( "// Skipping Drop Index for TableName {0}.  Not found in context. ", tableName ) );
            }
        }

        /// <summary>
        /// Generates code to perform a <see cref="T:System.Data.Entity.Migrations.Model.DropTableOperation" />.
        /// </summary>
        /// <param name="dropTableOperation">The operation to generate code for.</param>
        /// <param name="writer">Text writer to add the generated code to.</param>
        protected override void Generate( System.Data.Entity.Migrations.Model.DropTableOperation dropTableOperation, System.Data.Entity.Migrations.Utilities.IndentedTextWriter writer )
        {
            string tableName = dropTableOperation.Name.Replace( "dbo.", string.Empty );

            if ( contextAssemblyTables.ContainsKey( tableName ) )
            {
                base.Generate( dropTableOperation, writer );
            }
            else
            {
                // not found if Migration is trying to gen code for a table in another DbContext of a Multiple Context project
                // probably not found if this is the Down() migration
                writer.WriteLine( string.Format( "// Skipping Drop Table for TableName {0}.  Not found in context. ", tableName ) );
            }
        }

        /// <summary>
        /// Generates the primary code file that the user can view and edit.
        /// </summary>
        /// <param name="operations">Operations to be performed by the migration.</param>
        /// <param name="namespace">Namespace that code should be generated in.</param>
        /// <param name="className">Name of the class that should be generated.</param>
        /// <returns>
        /// The generated code.
        /// </returns>
        protected override string Generate( IEnumerable<System.Data.Entity.Migrations.Model.MigrationOperation> operations, string @namespace, string className )
        {
            string result = string.Empty;

            result += @"//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//
";
            result += base.Generate( operations, @namespace, className );

            result = result.Replace( ": DbMigration", ": Rock.Migrations.RockMigration" );

            result = result.Replace( "public partial class", "/// <summary>\r\n    ///\r\n    /// </summary>\r\n    public partial class" );
            result = result.Replace( "public override void Up()", "/// <summary>\r\n        /// Operations to be performed during the upgrade process.\r\n        /// </summary>\r\n        public override void Up()" );
            result = result.Replace( "public override void Down()", "/// <summary>\r\n        /// Operations to be performed during the downgrade process.\r\n        /// </summary>\r\n        public override void Down()" );

            return result;
        }
        #endregion
    }
}