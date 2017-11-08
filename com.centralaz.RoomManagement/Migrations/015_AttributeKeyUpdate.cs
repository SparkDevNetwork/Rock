// <copyright>
// Copyright by the Central Christian Church
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

namespace com.centralaz.RoomManagement.Migrations
{
    [MigrationNumber( 15, "1.6.0" )]
    public class AttributeKeyUpdate : Migration
    {
        public override void Up()
        {
            Sql( @"
                Update a
                Set a.[Key] = innerTable.NewKey
                From Attribute a 
                inner join
                (
                Select 'Q'+Cast(a.[Order] as Nvarchar(20))+'_ResourceId'+Cast(ResourceId as nvarchar(20)) as 'NewKey',
                a.Id as 'AttributeId'
                From Attribute a
                Join _com_centralaz_RoomManagement_Question q on a.Id = q.AttributeId
                Where ResourceId is not null
                ) as innerTable 
                on innerTable.AttributeId = a.Id


                Update a
                Set a.[Key] = innerTable.NewKey
                From Attribute a 
                inner join
                (
                Select 'Q'+Cast(a.[Order] as Nvarchar(20))+'_LocationId'+Cast(LocationId as nvarchar(20)) as 'NewKey',
                a.Id as 'AttributeId'
                From Attribute a
                Join _com_centralaz_RoomManagement_Question q on a.Id = q.AttributeId
                Where LocationId is not null
                ) as innerTable 
                on innerTable.AttributeId = a.Id
                " );
        }
        public override void Down()
        {

        }
    }
}
