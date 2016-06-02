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
    [MigrationNumber( 9, "1.0.14" )]
    public class BaptismIsPrivate : Migration
    {
        public override void Up()
        {
            Sql( @"
            IF NOT EXISTS(SELECT * FROM sys.columns 
            WHERE Name = N'IsPrivateBaptism' AND Object_ID = Object_ID(N'[_com_centralaz_Baptism_Baptizee]'))
BEGIN
            ALTER TABLE [_com_centralaz_Baptism_Baptizee]
            ADD IsPrivateBaptism BIT
END
            " );
        }

        public override void Down()
        {

        }
    }
}