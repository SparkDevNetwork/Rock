// <copyright>
// Copyright by BEMA Information Technologies
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
using Rock.Plugin;

namespace com.citizenschurch.Autobuild
{
    [MigrationNumber( 1, "1.9.4" )]
    public class InitialSetup : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/CheckIn/MultiPersonSelect.ascx'
                    Where Path = '~/Plugins/com_bemaservices/CheckIn/MultiPersonSelect.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/CICH/Groups/GroupMap.ascx',
                        Category = 'BEMA Services > Groups'
                    Where Path = '~/Plugins/com_bemaservices/Groups/GroupMap.ascx'
            " );

            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/CICH/Groups/GroupTypesMap.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemaservices/Groups/GroupTypesMap.ascx'
            " );
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {

        }
    }
}

