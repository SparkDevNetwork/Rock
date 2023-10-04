// <copyright>
// Copyright Southeast Christian Church
//
// Licensed under the  Southeast Christian Church License (the "License");
// you may not use this file except in compliance with the License.
// A copy of the License shoud be included with this file.
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
using Rock.Plugin;

namespace org.secc.PastoralCare.Migrations
{
    [MigrationNumber( 4, "1.2.0" )]
    class Fix_Nursing_Homes : Migration
    {
        public override void Up()
        {     
            Sql( "UPDATE [AttributeValue] SET Value = '4573E600-4E00-4BE9-BA92-D17093C735D6' WHERE Value = '89C2E347-BDEF-4BF2-8A25-9D4EE2E9B405'" );
        }
        public override void Down()
        {
            Sql( "UPDATE [AttributeValue] SET Value = '89C2E347-BDEF-4BF2-8A25-9D4EE2E9B405' WHERE Value = '4573E600-4E00-4BE9-BA92-D17093C735D6'" );
        }
    }
}

