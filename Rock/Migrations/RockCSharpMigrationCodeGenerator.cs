//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Model;
using System.IO;
using System.Linq;
using System.Reflection;
using Rock.Data;

namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Target DbContext to be migrated</typeparam>
    public class RockCSharpMigrationCodeGenerator<T> : CSharpMigrationCodeGenerator where T : DbContext
    {
        #region internal custom methods

        /// <summary>
        /// 
        /// </summary>
        private Assembly contextAssembly = null;

        /// <summary>
        /// Gets or sets a value indicating whether [limit operations to context assembly]
        /// Default is True
        /// </summary>
        /// <value>
        /// <c>true</c> if [limit operations to context assembly]; otherwise, <c>false</c>.
        /// </value>
        public bool LimitOperationsToContextAssembly { get; set; }

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Type> contextAssemblyTables = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCSharpMigrationCodeGenerator" /> class.
        /// </summary>
        /// <param name="limitOperationsToContextAssembly">if set to <c>true</c> [limit operations to context assembly].</param>
        public RockCSharpMigrationCodeGenerator( bool limitOperationsToContextAssembly = true )
            : base()
        {
            contextAssembly = typeof( T ).Assembly;
            LimitOperationsToContextAssembly = limitOperationsToContextAssembly;

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
                // probably not found if this is the Down() migration
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
            base.Generate( createTableOperation, writer );

            foreach ( var column in createTableOperation.Columns )
            {
                GenerateCustomColumnCode( writer, createTableOperation.Name, column.Name );
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
            List<string> skippedOperationComments = new List<string>();
            IEnumerable<MigrationOperation> includedOperations;
            if ( LimitOperationsToContextAssembly )
            {
                includedOperations = GetFilteredOperations( operations, out skippedOperationComments );
            }
            else
            {
                includedOperations = operations;
            }

            result += base.Generate( includedOperations, @namespace, className );

            result = result.Replace( ": DbMigration", ": Rock.Migrations.RockMigration" );

            result = result.Replace( "public partial class", "/// <summary>\r\n    ///\r\n    /// </summary>\r\n    public partial class" );
            result = result.Replace( "public override void Up()", "/// <summary>\r\n        /// Operations to be performed during the upgrade process.\r\n        /// </summary>\r\n        public override void Up()" );
            result = result.Replace( "public override void Down()", "/// <summary>\r\n        /// Operations to be performed during the downgrade process.\r\n        /// </summary>\r\n        public override void Down()" );

            if ( skippedOperationComments.Count > 0 )
            {
                result += string.Format( "/* Skipped Operations for tables that are not in the assembly {0}: Review these comments to verify the proper things were skipped */" + Environment.NewLine, Path.GetFileName( contextAssembly.Location ) );
                foreach ( var comment in skippedOperationComments )
                {
                    result += comment + Environment.NewLine;
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the filtered operations.
        /// </summary>
        /// <param name="operations">The operations.</param>
        /// <param name="skippedOperationComments">The skipped operation comments.</param>
        /// <returns></returns>
        private List<MigrationOperation> GetFilteredOperations( IEnumerable<System.Data.Entity.Migrations.Model.MigrationOperation> operations, out List<string> skippedOperationComments )
        {
            List<MigrationOperation> includedOperations = new List<System.Data.Entity.Migrations.Model.MigrationOperation>();
            skippedOperationComments = new List<string>();

            // make comments on the skipped Down() operations and add to includedOperations ones that are not skipped
            skippedOperationComments.Add( "" );
            skippedOperationComments.Add( "// Up()..." );
            foreach ( MigrationOperation operation in operations )
            {
                TableColumnInfo tableColumnInfo = GetOperationTableColumnInfo( operation );

                if ( !string.IsNullOrWhiteSpace( tableColumnInfo.TableName ) && !contextAssemblyTables.ContainsKey( tableColumnInfo.TableName ) )
                {
                    // not found if Migration is trying to gen code for a table in another DbContext of a Multiple Context project
                    // probably not found if this is the Down() migration

                    string columnInfo = string.Empty;
                    if ( !string.IsNullOrWhiteSpace( tableColumnInfo.ColumnName ) )
                    {
                        columnInfo = ", column " + tableColumnInfo.ColumnName;
                    }

                    skippedOperationComments.Add( string.Format( "// {0} for TableName {1}{2}.", operation.GetType().Name, tableColumnInfo.TableName, columnInfo ) );
                }
                else
                {
                    includedOperations.Add( operation );
                }
            }

            // make comments on the skipped Down() operations
            skippedOperationComments.Add( "" );
            skippedOperationComments.Add( "// Down()..." );
            foreach ( MigrationOperation operation in operations.Select( a => a.Inverse ).Where( a => a != null ).Reverse() )
            {
                TableColumnInfo tableColumnInfo = GetOperationTableColumnInfo( operation );

                if ( !string.IsNullOrWhiteSpace( tableColumnInfo.TableName ) && !contextAssemblyTables.ContainsKey( tableColumnInfo.TableName ) )
                {
                    // not found if Migration is trying to gen code for a table in another DbContext of a Multiple Context project
                    // probably not found if this is the Down() migration

                    string columnInfo = string.Empty;
                    if ( !string.IsNullOrWhiteSpace( tableColumnInfo.ColumnName ) )
                    {
                        columnInfo = ", column " + tableColumnInfo.ColumnName;
                    }

                    skippedOperationComments.Add( string.Format( "// {0} for TableName {1}{2}.", operation.GetType().Name, tableColumnInfo.TableName, columnInfo ) );
                }
            }

            if ( skippedOperationComments.Count == 4 )
            {
                // if there are exactly four comments, nothing was skipped
                skippedOperationComments.Clear();
            }

            return includedOperations;
        }

        /// <summary>
        /// 
        /// </summary>
        private class TableColumnInfo
        {
            /// <summary>
            /// Gets or sets the name of the table.
            /// </summary>
            /// <value>
            /// The name of the table.
            /// </value>
            public string TableName { get; set; }

            /// <summary>
            /// Gets or sets the name of the column.
            /// </summary>
            /// <value>
            /// The name of the column.
            /// </value>
            public string ColumnName { get; set; }
        }


        /// <summary>
        /// Gets the operation table column info.
        /// </summary>
        /// <param name="operation">The operation.</param>
        /// <returns></returns>
        private static TableColumnInfo GetOperationTableColumnInfo( MigrationOperation operation )
        {
            string tableName = string.Empty;
            string columnName = string.Empty;

            if ( operation is AddColumnOperation )
            {
                tableName = ( operation as AddColumnOperation ).Table;
                columnName = ( operation as AddColumnOperation ).Column.Name;
            }
            else if ( operation is AlterColumnOperation )
            {
                tableName = ( operation as AlterColumnOperation ).Table;
                columnName = ( operation as AlterColumnOperation ).Column.Name;
            }
            else if ( operation is CreateTableOperation )
            {
                tableName = ( operation as CreateTableOperation ).Name;
            }
            else if ( operation is DropColumnOperation )
            {
                tableName = ( operation as DropColumnOperation ).Table;
                columnName = ( operation as DropColumnOperation ).Name;
            }
            else if ( operation is DropTableOperation )
            {
                tableName = ( operation as DropTableOperation ).Name;
            }
            else if ( operation is ForeignKeyOperation )
            {
                tableName = ( operation as ForeignKeyOperation ).PrincipalTable;
                columnName = ( operation as ForeignKeyOperation ).DependentColumns.ToList().AsDelimited( "," );
            }
            else if ( operation is HistoryOperation )
            {
                // just a HistoryOperation, don't filter
            }
            else if ( operation is IndexOperation )
            {
                tableName = ( operation as IndexOperation ).Table;
                columnName = ( operation as IndexOperation ).Columns.ToList().AsDelimited( "," );
            }
            else if ( operation is MoveTableOperation )
            {
                tableName = ( operation as MoveTableOperation ).Name;
            }
            else if ( operation is PrimaryKeyOperation )
            {
                tableName = ( operation as PrimaryKeyOperation ).Table;
            }
            else if ( operation is RenameColumnOperation )
            {
                tableName = ( operation as RenameColumnOperation ).Table;
                columnName = ( operation as RenameColumnOperation ).NewName;
            }
            else if ( operation is RenameTableOperation )
            {
                tableName = ( operation as RenameTableOperation ).NewName;
            }
            else if ( operation is SqlOperation )
            {
                // some SQL operation, don't filter
            }
            else
            {
                // unexpected operation, don't filter
            }

            tableName = tableName.Replace( "dbo.", string.Empty );

            return new TableColumnInfo { TableName = tableName, ColumnName = columnName };
        }
        #endregion
    }
}