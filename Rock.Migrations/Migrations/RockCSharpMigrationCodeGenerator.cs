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
using System.Data.Entity.Migrations.Design;
using System.Data.Entity.Migrations.Infrastructure;
using System.Data.Entity.Migrations.Model;
using System.Linq;

namespace Rock.Migrations
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">Target DbContext to be migrated</typeparam>
    public class RockCSharpMigrationCodeGenerator<T> : CSharpMigrationCodeGenerator where T : System.Data.Entity.DbContext, new()
    {
        #region overridden methods

        /// <summary>
        /// Set <see cref="IsFrozen"/> to true if migrations are frozen in this branch, and update <see cref="MigrationTokenOwner"/>
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance is frozen; otherwise, <c>false</c>.
        /// </value>
        public bool IsFrozen => true;
        private const string MigrationTokenOwner = "release-1.11.0";

        /// <summary>
        /// </summary>
        /// <param name="migrationId"></param>
        /// <param name="operations"></param>
        /// <param name="sourceModel"></param>
        /// <param name="targetModel"></param>
        /// <param name="namespace"></param>
        /// <param name="className"></param>
        /// <returns></returns>
        /// <inheritdoc />
        public override ScaffoldedMigration Generate( string migrationId, IEnumerable<MigrationOperation> operations, string sourceModel, string targetModel, string @namespace, string className )
        {
            var result = base.Generate( migrationId, operations, sourceModel, targetModel, @namespace, className );

            var migrationTypes = Rock.Reflection.SearchAssembly( this.GetType().Assembly, typeof( Rock.Migrations.RockMigration ) ).ToList();
            var migrationTypeInstances = migrationTypes.Select( a => Activator.CreateInstance( a.Value ) as IMigrationMetadata ).ToList();
            var lastMigrationWithSameTarget = migrationTypeInstances.Where( a => a.Id != migrationId && a.Target == targetModel ).OrderBy( a => a.Id ).FirstOrDefault();

            if ( lastMigrationWithSameTarget != null )
            {
                result.Resources.Clear();
                result.DesignerCode = result.DesignerCode.Replace( $"new ResourceManager(typeof({className}));", $"new ResourceManager(typeof({lastMigrationWithSameTarget.GetType().Name}));" );
            }

            return result;
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
            if ( IsFrozen )
            {
                throw new System.Exception( $"Cannot add migration. Migrations are frozen in this branch. The Migration owner is currently {MigrationTokenOwner}" );
            }

            string result = string.Empty;

            result += @"// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the ""License"");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an ""AS IS"" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
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