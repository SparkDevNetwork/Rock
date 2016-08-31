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
    public partial class AddMissingBlockTypeCategories : Rock.Migrations.RockMigration4
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            Sql( @"
            UPDATE [BlockType] SET [Category] = 'CRM' WHERE [Guid] = '9B274A75-1D9B-4533-9849-7892F10A7672' --Person Merge' - ~/Blocks/Crm/PersonMerge.ascx
            UPDATE [BlockType] SET [Category] = 'Check-in' WHERE [Guid] = '21FFA70E-18B3-4148-8FC4-F941100B49B8' --Attendance History' - ~/Blocks/Checkin/AttendanceHistoryList.ascx
            UPDATE [BlockType] SET [Category] = 'CMS' WHERE [Guid] = 'CD3C0C1D-2171-4FCC-B840-FC6E6F72EEEF' --Layout Block List' - ~/Blocks/Cms/LayoutBlockList.ascx
            UPDATE [BlockType] SET [Category] = 'Administration' WHERE [Guid] = 'E2D423B8-10F0-49E2-B2A6-D62892379429' --System Configuration' - ~/Blocks/Administration/SystemConfiguration.ascx
            UPDATE [BlockType] SET [Category] = 'Core' WHERE [Guid] = '468B99CE-D276-4D30-84A9-7842933BDBCD' --Location Tree View' - ~/Blocks/Core/LocationTreeView.ascx
            UPDATE [BlockType] SET [Category] = 'Core' WHERE [Guid] = '08189564-1245-48F8-86CC-560F4DD48733' --Location Detail' - ~/Blocks/Core/LocationDetail.ascx
            UPDATE [BlockType] SET [Category] = 'Security' WHERE [Guid] = 'B37DC24D-9DE4-4E94-B8E1-9BCB03A835F1' --My Account' - ~/Blocks/Security/MyAccount.ascx
            UPDATE [BlockType] SET [Category] = 'Security' WHERE [Guid] = 'F501AB3F-1F41-4C06-9BC2-57C42E702995' --Edit My Account' - ~/Blocks/Security/EditMyAccount.ascx
            UPDATE [BlockType] SET [Category] = 'Communication' WHERE [Guid] = '82B00455-B8CF-4673-ACF5-641B961DF59F' --System Email Detail' - ~/Blocks/Communication/SystemEmailDetail.ascx
            UPDATE [BlockType] SET [Category] = 'Communication' WHERE [Guid] = '2645A264-D5E5-43E8-8FE2-D351F3D5435B' --System Email List' - ~/Blocks/Communication/SystemEmailList.ascx
" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
