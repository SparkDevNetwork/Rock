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

namespace org.fellowshipnwa.Autobuild
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
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/FLCH/Event/RegistrationCart.ascx',
                        Category = 'BEMA Services > Event'
                    Where Path = '~/Plugins/com_bemaservices/Event/RegistrationCart.ascx'
            " );
            
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/FLCH/Reporting/TestSQLConnectivity.ascx',
                        Category = 'BEMA Services > Reporting'
                    Where Path = '~/Plugins/com_bemaservices/Testing/TestSQLConnectivity.ascx'
            " );
            
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/FLCH/Event/EventLink.ascx',
                        Category = 'BEMA Services > Event'
                    Where Path = '~/Plugins/com_bemadev/Other/EventLink.ascx'
            " );
            
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/FLCH/Finance/BatchListWithGLExport.ascx',
                        Category = 'BEMA Services > Finance'
                    Where Path = '~/Plugins/com_bemadev/Finance/BatchListWithGLExport.ascx'
            " );
            
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Checkin/Manager/Locations.ascx',
                        Category = 'BEMA Services > Check-in'
                    Where Path = '~/Plugins/com_bemaservices/Checkin/Manager/Locations.ascx'
            " );
            
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Event/CalendarLava.ascx',
                        Category = 'BEMA Services > Event'
                    Where Path = '~/Plugins/com_bemaservices/Event/CalendarLava.ascx'
            " );
            
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Checkin/Search.ascx',
                        Category = 'BEMA Services > Check-in'
                    Where Path = '~/Plugins/com_bemaservices/Checkin/Search.ascx'
            " );
            
            Sql( @" Update BlockType
                    Set Path = '~/Plugins/com_bemaservices/CustomBlocks/BEMA/Core/CampusContextSetter.ascx',
                        Category = 'BEMA Services > Core'
                    Where Path = '~/Plugins/com_bemaservices/Core/CampusContextSetter.ascx'
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

