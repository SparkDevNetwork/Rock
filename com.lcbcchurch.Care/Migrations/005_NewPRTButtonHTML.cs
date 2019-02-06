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
namespace com.lcbcchurch.Care.Migrations
{
    [MigrationNumber( 5, "1.0.14" )]
    public class NewPRTButtonHTML : Migration
    {
        public override void Up()
        {
            //Find page Id for Care PRT request page (created in 001_AddCarePages.cs)
            string newPRTRequestPage = SqlScalar( "SELECT TOP 1 P.Id FROM [Page] P WHERE P.[Guid] = '0AFB6A46-50F3-4E82-939B-88D4248A657B'" ).ToString().Trim();

            string connectionOpportunityId = SqlScalar( "SELECT TOP 1 CO.Id FROM [ConnectionOpportunity] CO WHERE CO.[Guid] = '0F1B2894-4BF5-455C-AA5C-4FFDC6440D24'" ).ToString().Trim();

            //Update HTML block on Care person page with the button content
            RockMigrationHelper.UpdateHtmlContentBlock( "3DFB18A8-909F-435F-AE5A-409338F498CC", @"<a href="" / page /  " + newPRTRequestPage + @"  ? ConnectionRequestId = 0 & ConnectionOpportunityId = " + connectionOpportunityId + @" & PersonId ={ { ''Global'' | PageParameter:''PersonId'' } }"" class=""btn btn-primary"" style=""margin - bottom: 15px; ""> < i class=""fa fa-heart"" style=""margin-right: 8px; font-size: 12px;""></i>New PRT Request </a> ", "9B3ED278-95B1-4509-8427-0569F0225EE3" );
            
        }
        public override void Down()
        {
            
        }
    }
}
