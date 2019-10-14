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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 54, "1.8.1" )]
    public class MigrationRollupsForV8_2 : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
//            // SK: Update group view lava template to display schedule when exist (ISSUE #3188)
//            // Update the default template
//            Sql( @"		  
//		  UPDATE 
//	[GroupType]
//SET 
//	[GroupViewLavaTemplate] = REPLACE([GroupViewLavaTemplate],'{{ Group.Schedule.ToString()','{{ Group.Schedule.FriendlyScheduleText')
//WHERE 
//	[GroupViewLavaTemplate] Like '%{{ Group.Schedule.ToString()%'
//" );

//            // Update the system setting 
//            Sql( @"UPDATE 
//	[Attribute]
//SET 
//	[DefaultValue] = REPLACE([DefaultValue],'{{ Group.Schedule.ToString()','{{ Group.Schedule.FriendlyScheduleText')
//WHERE 
//	[Key]='core_templates_GroupViewTemplate'" );

//            // SK: Added Other to Currency Type Defined Type
//            RockMigrationHelper.UpdateDefinedValue( Rock.SystemGuid.DefinedType.FINANCIAL_CURRENCY_TYPE, "Other", "The currency type is other.", SystemGuid.DefinedValue.CURRENCY_TYPE_OTHER, true );

//            // NA: Enable AllowGroupSync for Communication List type groups (Fixes #3223).
//            Sql( string.Format( "UPDATE [GroupType] SET [AllowGroupSync] = 1 WHERE [AllowGroupSync] = 0 AND [Guid] = '{0}'", Rock.SystemGuid.GroupType.GROUPTYPE_COMMUNICATIONLIST ) );

//            // NA: Make Personal Device Platform Defined Values IsSystem=true
//            // Fix Personal Device Platform Defined Values to IsSystem=true
//            Sql( string.Format( "UPDATE [DefinedValue] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [Guid] = '{0}'", Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_ANDROID ) );
//            Sql( string.Format( "UPDATE [DefinedValue] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [Guid] = '{0}'", Rock.SystemGuid.DefinedValue.PERSONAL_DEVICE_PLATFORM_IOS ) );

//            // NA: Fix FrontPorch Device Platform Defined Values IsSystem=true
//            // Fix FrontPorch Device Platform Defined Values IsSystem=true
//            // NOTE: These do not have a well-known GUIDs so we will target them via their value and description.
//            Sql( string.Format( "UPDATE [DefinedValue] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [DefinedTypeId] = ( SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}' ) AND [Value] = 'Windows' AND [Description] = 'A Windows device'", Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM ) );
//            Sql( string.Format( "UPDATE [DefinedValue] SET [IsSystem] = 1 WHERE [IsSystem] = 0 AND [DefinedTypeId] = ( SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{0}' ) AND [Value] = 'Mac' AND [Description] = 'A Macintosh device'", Rock.SystemGuid.DefinedType.PERSONAL_DEVICE_PLATFORM ) );
        }


        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
