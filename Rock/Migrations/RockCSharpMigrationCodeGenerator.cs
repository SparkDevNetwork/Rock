//
// THIS WORK IS LICENSED UNDER A CREATIVE COMMONS ATTRIBUTION-NONCOMMERCIAL-
// SHAREALIKE 3.0 UNPORTED LICENSE:
// http://creativecommons.org/licenses/by-nc-sa/3.0/
//

using System;
using System.Collections.Generic;
using System.Data.Entity.Migrations.Design;
using System.Linq;
using System.Reflection;
using Rock.Data;

namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    public class RockCSharpMigrationCodeGenerator : CSharpMigrationCodeGenerator
    {
        #region internal custom methods
        /// <summary>
        /// 
        /// </summary>
        private Assembly rockAssembly = null;

        /// <summary>
        /// 
        /// </summary>
        private Dictionary<string, Type> tableNameLookup = new Dictionary<string, Type>();

        /// <summary>
        /// Initializes a new instance of the <see cref="RockCSharpMigrationCodeGenerator" /> class.
        /// </summary>
        public RockCSharpMigrationCodeGenerator()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            rockAssembly = Assembly.Load( "Rock" );
            foreach ( var e in rockAssembly.GetTypes().Where( a => a.CustomAttributes.Any( x => x.AttributeType.Name.Equals( "TableAttribute" ) ) ).ToList() )
            {
                var attrib = e.CustomAttributes.FirstOrDefault( a => a.AttributeType.Name.Equals( "TableAttribute" ) );
                if ( attrib != null )
                {
                    tableNameLookup.Add( attrib.ConstructorArguments[0].Value.ToString(), e );
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

            var tableType = tableNameLookup[tableName];
            if ( tableType != null )
            {
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
            result += base.Generate( operations, @namespace, className );

            result = result.Replace( ": DbMigration", ": RockMigration" );

            result = result.Replace( "public partial class",
  @"/// <summary>
    /// 
    /// </summary>
    public partial class" );

            result = result.Replace( "public override void Up()",
      @"/// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()" );

            result = result.Replace( "public override void Down()",
      @"/// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()" );

            return result;
        }
        #endregion
    }
}