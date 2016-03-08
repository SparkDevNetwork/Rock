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

            // add job service
            Sql( @"  INSERT INTO [ServiceJob]
                          ([IsSystem], [IsActive], [Name], [Description], [Class], [CronExpression], [NotificationStatus], [Guid])
                          VALUES 
                          (0, 0, 'Family Analytics', 'Job that populates Rock''s family analytics', 'Rock.Jobs.CalculateFamilyAnalytics', '0 0 20 ? * TUE *', 1, '623F4751-C654-FEB7-45B7-59685B1F60AE')" );

            // create person attributes
            RockMigrationHelper.UpdatePersonAttributeCategory( "Family Analytics", "fa fa-shield", "Person attributes related to Rock's family analytics features.", SystemGuid.Category.PERSON_ATTRIBUTES_ERA );
            RockMigrationHelper.UpdatePersonAttribute( SystemGuid.FieldType.BOOLEAN, SystemGuid.Category.PERSON_ATTRIBUTES_ERA, "Currently an eRA", "core_CurrentlyAnEra", "", "", 1, "", SystemGuid.Attribute.PERSON_ERA_CURRENTLY_AN_ERA );
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
            RockMigrationHelper.AddBlock( SystemGuid.Page.EXTENDED_ATTRIBUTES, "", "D70A59DC-16BE-43BE-9880-59598FA7A94C", "Family Analytics", "SectionB3", "", "", 0, "CDE833AA-159D-30B5-419F-CD6EFBB05887" );
            RockMigrationHelper.AddBlockAttributeValue( "CDE833AA-159D-30B5-419F-CD6EFBB05887", "EC43CF32-3BDF-4544-8B6A-CE9208DD7C81", SystemGuid.Category.PERSON_ATTRIBUTES_ERA );

            // reorder the current blocks
            Sql( @"  UPDATE [Block]
	                SET [Order] = 1 
	                WHERE [Guid] = '46D254C2-5A36-4F99-97A3-45DA8A49DB90'

                  UPDATE [Block]
	                SET [Order] = 2 
	                WHERE [Guid] = 'FFC9DF57-3E18-492C-B622-3EA167D7EBA1'" );

            // add era badge
            Sql( @"DECLARE @LavaBadgeEntityTypeId int = (SELECT TOP 1 [Id] FROM [EntityType] WHERE [Guid] = '95912004-62B5-4460-951F-D752427D44FE')
  DECLARE @LavaAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = '01C9BA59-D8D4-4137-90A6-B3C06C70BBC3')
  DECLARE @BadgeEntityId int 
  

  INSERT INTO [PersonBadge]
  ([Name], [Description], [EntityTypeId], [Order], [Guid])
  VALUES
	('eRA', 'Shows if someone is an eRA.', @LavaBadgeEntityTypeId, 10, '7FC986B9-CA1E-CBB7-4E63-C79EAC34F39D')
SET @BadgeEntityId = SCOPE_IDENTITY()


INSERT INTO [AttributeValue]
([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
VALUES
(0, @LavaAttributeId, @BadgeEntityId, '{% assign isEra = Person | Attribute:''core_CurrentlyAnEra'' %}

{% if isEra == ''Yes'' %}

<div class=""badge badge-era"" data-toggle=""tooltip"" data-original-title=""{{ Person.NickName}} become an eRA on {{ Person | Attribute:''core_EraStartDate''}}"">
    <span>eRA</span>
</div>

{% endif %}', newid())

DECLARE @BadgeListAttributeId int = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'F5AB231E-3836-4D52-BD03-BF79773C237A')
DECLARE @BadgeBlockId int = (SELECT TOP 1 [Id] FROM [Block] WHERE [Guid] = '98A30DD7-8665-4C6D-B1BB-A8380E862A04')
DECLARE @BadgeGuid nvarchar(50) = (SELECT TOP 1 convert(nvarchar(50),[Guid]) FROM [PersonBadge] WHERE [Id] = @BadgeEntityId)

IF EXISTS (SELECT *  FROM  [AttributeValue] WHERE [AttributeId] = @BadgeListAttributeId AND [EntityId] =  @BadgeBlockId)
	UPDATE [AttributeValue]
		SET [Value] = [Value] + ',' + @BadgeGuid
	WHERE
		[AttributeId] = @BadgeListAttributeId AND [EntityId] =  @BadgeBlockId
ELSE
	INSERT INTO [AttributeValue]
		([IsSystem], [AttributeId], [EntityId], [Value], [Guid])
	VALUES
		(0, @BadgeListAttributeId, @BadgeBlockId, @BadgeGuid, newid())" );
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            // delete job
            Sql( @"DELETE FROM [ServiceJob] WHERE [Guid] = '623F4751-C654-FEB7-45B7-59685B1F60AE'" );
            
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
