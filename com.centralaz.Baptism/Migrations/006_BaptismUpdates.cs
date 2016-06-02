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

using Rock;
using Rock.Plugin;
using Rock.Web.Cache;
namespace com.centralaz.Baptism.Migrations
{
    [MigrationNumber( 6, "1.0.14" )]
    public class BaptismUpdates : Migration
    {
        public override void Up()
        {
            Sql( @"
        --Baptisms Page
        UPDATE [Page] SET
            [BreadCrumbDisplayName] = 0 
        WHERE [GUID] = 'B248D7E3-AD38-4E83-9E3C-3CC6D3814AB4'

UPDATE [dbo].[GroupType]
   SET [ShowInNavigation] = 0    
 WHERE [GUID] = '32F8592C-AE11-44A7-A053-DE43789811D9'


" );

        }

        public override void Down()
        {

        }
    }
}