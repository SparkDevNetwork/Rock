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
namespace Rock.Migrations
{
    using System;
    using System.IO;

    /// <summary>
    ///
    /// </summary>
    public partial class AddIsActiveToDefinedValueType : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.DefinedValue", "IsActive", c => c.Boolean( nullable: false, defaultValue: true ) );
            AddColumn( "dbo.DefinedType", "IsActive", c => c.Boolean( nullable: false, defaultValue: true ) );

            MoveOriginalCssOverrides();
        }

        /// <summary>
        /// Moves the original CSS and Variable LESS overrides from Rock Theme to RockOriginal Theme
        /// </summary>
        private static void MoveOriginalCssOverrides()
        {
            if ( !System.Web.Hosting.HostingEnvironment.IsHosted )
            {
                // We only want to backup if this is a production environment, so if this migration was run from Package Manager Console, don't copy the files
                return;
            }

            // Copy the existing (old) _css-overrides.less to RockOriginal to preserve any customizations that were made, then replace it with a blank one for the new v8 Rock Theme
            var rockCssOverridesPath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Themes\\Rock\\Styles\\_css-overrides.less" );
            var rockOriginalCssOverridesPath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Themes\\RockOriginal\\Styles\\_css-overrides.less" );

            if ( !Directory.Exists( Path.GetDirectoryName( rockOriginalCssOverridesPath ) ) )
            {
                // The RockOriginal theme doesn't exist, so nothing to do
                return;
            }

            if ( !File.Exists( rockOriginalCssOverridesPath ) )
            {
                if ( File.Exists( rockCssOverridesPath ) )
                {
                    File.Copy( rockCssOverridesPath, rockOriginalCssOverridesPath );
                    File.WriteAllText( rockCssOverridesPath, string.Empty );
                }
                else
                {
                    File.WriteAllText( rockOriginalCssOverridesPath, string.Empty );
                }
            }

            // Copy the existing (old) _variable-overrides.less to RockOriginal to preserve any customizations that were made, then replace it with a blank one for the new v8 Rock Theme
            var rockVariableOverridesPath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Themes\\Rock\\Styles\\_variable-overrides.less" );
            var rockOriginalVariableOverridesPath = System.IO.Path.Combine( AppDomain.CurrentDomain.BaseDirectory, "Themes\\RockOriginal\\Styles\\_variable-overrides.less" );

            if ( !File.Exists( rockOriginalVariableOverridesPath ) )
            {
                if ( File.Exists( rockVariableOverridesPath ) )
                {
                    File.Copy( rockVariableOverridesPath, rockOriginalVariableOverridesPath );
                    File.WriteAllText( rockVariableOverridesPath, string.Empty );
                }
                else
                {
                    File.WriteAllText( rockOriginalVariableOverridesPath, string.Empty );
                }
            }
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn( "dbo.DefinedType", "IsActive" );
            DropColumn( "dbo.DefinedValue", "IsActive" );
        }
    }
}
