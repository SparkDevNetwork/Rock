// <copyright>
// Copyright by LCBC Church
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

namespace com.lcbcchurch.Groups.Migrations
{
    [MigrationNumber( 2, "1.0.14" )]
    class ModifyKidCrewMemberAttributes : Migration
    {
        public override void Up()
        {
            // Remove 'Environment' Group Member Attributes from first migration
            // kidCrew Adult Volunteers
            RockMigrationHelper.DeleteAttribute( "A68B61CF-1D10-4331-B4F2-69E80421DCAC" );    // GroupType - Group Member Attribute, kidCrew Adult Volunteers: Environment  
            // kidCrew Student Volunteers
            RockMigrationHelper.DeleteAttribute( "9D8AE363-D5DA-4759-9686-837D8069A942" );    // GroupType - Group Member Attribute, kidCrew Student Volunteers: Environment  

            // GroupType: kidCrew Adult Volunteers // Add Group Type Member Attribute: Environment
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "093DAFC7-2F18-46D3-99F9-C3BEC4A8C07F", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Environment", @"", 2, "", "8E245EF6-5D65-441B-BF2D-C6B1CCFDDD64" );
            RockMigrationHelper.AddAttributeQualifier( "8E245EF6-5D65-441B-BF2D-C6B1CCFDDD64", "values", "Sunrise Pointe,Oyster Bay,Treasure Cove,The Pier,Wheelhouse,Wheelhouse Jr", "19ABB37D-D3EE-47CE-81F1-E5C79E9ACAAE" );
            RockMigrationHelper.AddAttributeQualifier( "8E245EF6-5D65-441B-BF2D-C6B1CCFDDD64", "enhancedselection", "False", "5ED6B813-30A6-4B32-AFBA-39765D743E8A" );

            // GroupType: kidCrew Student Volunteers // Add Group Type Member Attribute: Environment
            RockMigrationHelper.AddGroupTypeGroupMemberAttribute( "ABD85A23-17F5-4EE0-A0C6-A88B6F8DFD77", "BD0D9B57-2A41-4490-89FF-F01DAB7D4904", "Environment", @"", 2, "", "55CF7957-8E1D-4428-9901-29B74F5FD988" );
            RockMigrationHelper.AddAttributeQualifier( "55CF7957-8E1D-4428-9901-29B74F5FD988", "values", "Sunrise Pointe,Oyster Bay,Treasure Cove,The Pier,Wheelhouse,Wheelhouse Jr", "7B056A11-6556-4B6A-B5B7-45BFAF6EAFB0" );
            RockMigrationHelper.AddAttributeQualifier( "55CF7957-8E1D-4428-9901-29B74F5FD988", "enhancedselection", "False", "EFCB9337-6A0E-46EB-A3C8-7FBFC16CE983" );

            // Set the new Group Member Attributes to be grid columns
            Sql( @" Update Attribute
                    Set IsGridColumn = 1
                    Where Guid in (
                        '8E245EF6-5D65-441B-BF2D-C6B1CCFDDD64',
                        '55CF7957-8E1D-4428-9901-29B74F5FD988')
            " );
        }

        public override void Down()
        {
            throw new NotImplementedException();
        }
    }
}
