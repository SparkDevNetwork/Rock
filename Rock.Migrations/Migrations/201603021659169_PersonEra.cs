// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class PersonEra : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
          
            // todo
            // - add stored proc

            // create person attributes
            RockMigrationHelper.UpdatePersonAttributeCategory( "eRA", "fa fa-shield", "Person attributes related to Rock's eRA features.", SystemGuid.Category.PERSON_ATTRIBUTES_ERA );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.BOOLEAN, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "Currently an eRA", "core_CurrentlyAnEra", "", "", 1, "false", SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "eRA Start Date", "core_EraStartDate", "", "", 2, "", SystemGuid.Attribute.PERSON_ERA_START_DATE );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "eRA End Date", "core_EraEndDate", "", "", 3, "", SystemGuid.Attribute.PERSON_ERA_END_DATE );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "First Attended", "core_EraFirstAttended", "", "", 4, "", SystemGuid.Attribute.PERSON_ERA_FIRST_ATTENDED );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "Last Attended", "core_EraLastAttended", "", "", 5, "", SystemGuid.Attribute.PERSON_ERA_LAST_ATTENDED );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "Last Gave", "core_EraLastGave", "", "", 6, "", SystemGuid.Attribute.PERSON_ERA_LAST_GAVE );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.DATE, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "First Gave", "core_EraFirstGave", "", "", 6, "", SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.INTEGER, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "Times Attended (16 wks)", "core_EraTimesAttended16Wks", "", "", 7, "", SystemGuid.Attribute.PERSON_ERA_TIMES_ATTENDED_16 );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.INTEGER, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "Times Given (52 wks)", "core_EraTimesGiven52Wks", "", "", 8, "", SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_52 );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.INTEGER, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "Times Given (6 wks)", "core_EraTimesGiven6Wks", "", "", 9, "", SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_6 );

            // add security to person attributes
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_START_DATE );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_END_DATE );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_FIRST_ATTENDED );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_LAST_ATTENDED );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_LAST_GAVE );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_ATTENDED_16 );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_52 );
            AddSecurityToAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_6 );

            // add attributes block to the person profile page
            RockMigrationHelper.AddBlock( SystemGuid.Page.EXTENDED_ATTRIBUTES, "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "eRA Attributes", "SectionB3", "", "", 0, "CDE833AA-159D-30B5-419F-CD6EFBB05887" );
            RockMigrationHelper.AddBlockAttributeValue( "CDE833AA-159D-30B5-419F-CD6EFBB05887", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", SystemGuid.Category.PERSON_ATTRIBUTES_ERA );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete security for attribites
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_START_DATE );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_END_DATE );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_FIRST_ATTENDED );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_LAST_ATTENDED );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_LAST_GAVE );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_FIRST_GAVE );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_ATTENDED_16 );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_52 );
            DeleteSecurityFromAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_6 );

            // delete attributes
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_START_DATE );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_END_DATE );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_FIRST_ATTENDED );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_LAST_ATTENDED );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_LAST_GAVE );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_ATTENDED_16 );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_52 );
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PERSON_ERA_TIMES_GIVEN_6 );

            // delete category
            RockMigrationHelper.DeleteCategory( SystemGuid.Category.PERSON_ATTRIBUTES_ERA );

        }

        /// <summary>
        /// Adds the security to attribute.
        /// </summary>
        /// <param name="attributeGuid">The attribute unique identifier.</param>
        private void AddSecurityToAttribute( string attributeGuid )
        {
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 0, "View", true, SystemGuid.Group.GROUP_ADMINISTRATORS, 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 1, "View", true, SystemGuid.Group.GROUP_STAFF_MEMBERS, 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 2, "View", true, SystemGuid.Group.GROUP_STAFF_LIKE_MEMBERS, 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 3, "View", false, "", (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );

            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 0, "Edit", true, SystemGuid.Group.GROUP_ADMINISTRATORS, 0, Guid.NewGuid().ToString() );
            RockMigrationHelper.AddSecurityAuthForAttribute( attributeGuid, 1, "Edit", false, "", (int)Model.SpecialRole.AllUsers, Guid.NewGuid().ToString() );
        }

        private void DeleteSecurityFromAttribute(string attributeGuid )
        {
            RockMigrationHelper.DeleteSecurityAuthForAttribute( attributeGuid );
        }
    }
}
