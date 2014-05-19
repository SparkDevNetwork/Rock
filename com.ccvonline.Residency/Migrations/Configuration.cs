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
namespace com.ccvonline.Residency.Migrations
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Reflection;

    // This is the Configuration class specifically for your Plugin.  
    // When doing "Add-Migration" and "Update-Database" operations, you might need to add the "-ConfigurationTypeName:Configuration" parameter 
    internal sealed class Configuration : DbMigrationsConfiguration<com.ccvonline.Residency.Data.ResidencyContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Configuration"/> class.
        /// </summary>
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            CodeGenerator = new Rock.Migrations.RockCSharpMigrationCodeGenerator<com.ccvonline.Residency.Data.ResidencyContext>(true);
        }

        /// <summary>
        /// Seeds the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        protected override void Seed(com.ccvonline.Residency.Data.ResidencyContext context)
        {
            //  This method will be called after migrating to the latest version.
        }
    }

    /*
    // If you update the Rock.dll often, this RockCoreConfiguration will help you do Update-Database for the Rock Core Tables
    // To do this, run "Update-Database -ConfigurationTypeName:RockCoreConfiguration" using Package Manager Console
    internal sealed class RockCoreConfiguration : DbMigrationsConfiguration<Rock.Data.RockContext>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RockCoreConfiguration"/> class.
        /// </summary>
        public RockCoreConfiguration()
        {
            this.MigrationsAssembly = typeof( Rock.Data.RockContext ).Assembly;
            this.MigrationsNamespace = "Rock.Migrations";
            this.ContextKey = "Rock.Migrations.Configuration";
        }
    }
     */ 
}
