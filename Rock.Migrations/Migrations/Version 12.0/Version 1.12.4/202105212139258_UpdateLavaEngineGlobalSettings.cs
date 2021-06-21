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
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class UpdateLavaEngineGlobalSettings : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            ModifyLavaEngineSystemSettingsUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            //
        }

        private void ModifyLavaEngineSystemSettingsUp()
        {
            // These values are declared as global constants in v13.
            var v13_GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK = "9CBDD352-A4F5-47D6-9EFE-6115774B2DFE";
            var v13_LAVA_ENGINE_LIQUID_FRAMEWORK = "core_LavaEngine_LiquidFramework";

            // Update the setting description.
            RockMigrationHelper.UpdateGlobalAttribute(
                v13_LAVA_ENGINE_LIQUID_FRAMEWORK,
                string.Empty,
                string.Empty,
                v13_GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK,
                Rock.SystemGuid.FieldType.SINGLE_SELECT,
                "Lava Engine Liquid Framework",
                @"The Liquid rendering framework used by the Lava Engine to parse and render templates. Changes to this setting will not take effect until Rock is restarted.",
                0
            );

            // Update the setting available options.
            RockMigrationHelper.UpdateAttributeQualifier( v13_GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK, "values", "DotLiquid^DotLiquid,Fluid^Fluid,FluidVerification^DotLiquid (with Fluid verification)", "E11456FF-F57B-4964-877E-85468971C238" );

            // Reset the setting value if it is not one of the available options, to prevent the Lava Engine logging a configuration error during startup.
            Sql( $@"
                DECLARE @AttributeId INT = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '{v13_GLOBAL_LAVA_ENGINE_LIQUID_FRAMEWORK}' )
                UPDATE [AttributeValue] 
            	SET [Value] = ''
            	WHERE [AttributeId] = @AttributeId AND [Value] NOT IN ('DotLiquid','Fluid','FluidVerification')
                
                UPDATE [Attribute] SET [DefaultValue] = '' WHERE [Id] = @AttributeId"
            );
        }
    }
}
