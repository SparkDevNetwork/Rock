// <copyright>
// Copyright by Central Christian Church
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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock.Plugin;
namespace com.lcbcchurch.Reporting.Migrations
{
    [MigrationNumber( 6, "1.0.14" )]
    public class MoveAssimilationAbsenteePages : Migration
    {
        public override void Up()
        {
            // Move Assimilation and Absentee Report Pages to the 'Reporting' section 

            Sql( @"DECLARE @ParentPageId INT
                    SET @ParentPageId = (SELECT TOP 1 [Page].Id FROM [Page] WHERE [Guid] = 'bb0acd18-24fb-42ba-b89a-2ffd80472f5b' )

                    UPDATE [Page]
                    SET ParentPageId = @ParentPageId
                    WHERE Guid IN ( '82A4A145-4E72-41DA-AA05-E4B31A3290FF', '23B27982-B2F2-4D18-A4BF-598E41DC8FFC')" );


        }
        public override void Down()
        {
            
        }
    }
}
