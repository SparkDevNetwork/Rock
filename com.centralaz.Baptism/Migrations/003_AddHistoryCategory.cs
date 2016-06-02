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
    [MigrationNumber( 3, "1.0.14" )]
    public class AddHistoryCategory : Migration
    {
        public override void Up()
        {
            // Add a Baptism Changes category (for the 'history' entity)
            RockMigrationHelper.UpdateCategory( "546D5F43-1184-47C9-8265-2D7BF4E1BCA5", "Baptism Changes", "fa fa-tint", "Anything related to a person's baptism.", com.centralaz.Baptism.SystemGuid.Category.HISTORY_PERSON_BAPTISM_CHANGES );
        }

        public override void Down()
        {
            // Delete the Baptism Changes category
            RockMigrationHelper.DeleteCategory( com.centralaz.Baptism.SystemGuid.Category.HISTORY_PERSON_BAPTISM_CHANGES );
        }
    }
}