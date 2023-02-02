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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// At the time of this writing the Created Date Time on the Rock Instance Id Attribute is set to NULL.
    /// We needed a way to set the Default Value of the Lava Engine Liquid Framework to Fluid only if the Rock installation is new
    /// The way we check if the installation is new is by checking if the Created Date Time on the Rock Instance Id is not set to NULL
    /// If the value is NULL, the installation is old and Default Value of the Lava Engine Liquid Attribute should not be updated
    /// Otherwise, set the Default Value to Fluid
    /// We plan to make the change of setting the Created Date Time of the Rock Instance Id Attribute shortly after writing this.
    /// </summary>
    [MigrationNumber( 166, "1.14.1" )]
    public class SetLavaEngineLiquidFrameworkAttributeDefaultValueToFluid : Migration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
    UPDATE 
        [Attribute] 
    SET 
        [DefaultValue] = 'Fluid' 
    WHERE [Key] = 'core_LavaEngine_LiquidFramework' 
        AND ( EXISTS (
                SELECT * FROM [Attribute] WHERE [Key] = 'RockInstanceId' AND [CreatedDateTime] IS NOT NULL
            )
        );" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // Down migrations are not yet supported in plug-in migrations.
        }
    }
}
