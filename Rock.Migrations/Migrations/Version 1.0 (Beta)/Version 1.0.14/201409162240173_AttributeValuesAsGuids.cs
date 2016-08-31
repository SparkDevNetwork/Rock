// <copyright>
// Copyright by the Spark Development Network
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
namespace Rock.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    ///
    /// </summary>
    public partial class AttributeValuesAsGuids : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            /* Convert BinaryFile,File,Image attribute values from int to guid */
            Sql( @"
DECLARE 
    @fieldTypeBinaryFileGuid UNIQUEIDENTIFIER = 'C403E219-A56B-439E-9D50-9302DFE760CF' -- BinaryFile
    ,@fieldTypeFileGuid UNIQUEIDENTIFIER = '6F9E2DD0-E39E-4602-ADF9-EB710A75304A' -- File (BinaryFile)
    ,@fieldTypeImageGuid UNIQUEIDENTIFIER = '97F8157D-A8C8-4AB3-96A2-9CB2A9049E6D' -- Image (BinaryFile) 

update Attribute set DefaultValue = bf.Guid
FROM Attribute a
JOIN FieldType ft ON ft.id = a.FieldTypeId
left outer join BinaryFile bf on bf.Id = a.DefaultValue
where len(a.DefaultValue) < 15
and ISNUMERIC(a.DefaultValue) = 1
and ft.Guid in (@fieldTypeBinaryFileGuid, @fieldTypeFileGuid, @fieldTypeImageGuid)

update AttributeValue set Value = bf.Guid
FROM AttributeValue av
JOIN Attribute a on av.AttributeId = a.Id
JOIN FieldType ft ON ft.id = a.FieldTypeId
left outer join BinaryFile bf on bf.Id = av.Value
where len(av.Value) < 15
and ISNUMERIC(av.Value) = 1
and ft.Guid in (@fieldTypeBinaryFileGuid, @fieldTypeFileGuid, @fieldTypeImageGuid)

" );
            /* 
            * Convert Campus attribute values from int to guid 
            * NOTE: Don't attempt to convert "Campuses" attribute values (a list of ints).  If there happens to be any. 
            */
            Sql( @"
declare
    @fieldTypeCampusGuid UNIQUEIDENTIFIER = '1B71FEF4-201F-4D53-8C60-2DF21F1985ED' -- Campus
    ,@fieldTypeCampusesGuid uniqueidentifier = '69254F91-C97F-4C2D-9ACB-1683B088097B' -- Campuses

update Attribute set DefaultValue = e.Guid
FROM Attribute a
JOIN FieldType ft ON ft.id = a.FieldTypeId
left outer join Campus e on e.Id = a.DefaultValue
where len(a.DefaultValue) < 15
and ISNUMERIC(a.DefaultValue) = 1
and ft.Guid in (@fieldTypeCampusGuid)

update AttributeValue set Value = e.Guid
FROM AttributeValue av
JOIN Attribute a on av.AttributeId = a.Id
JOIN FieldType ft ON ft.id = a.FieldTypeId
left outer join Campus e on e.Id = av.Value
where len(av.Value) < 15
and ISNUMERIC(av.Value) = 1
and ft.Guid in (@fieldTypeCampusGuid)

" );

            // Convert Group attribute values from int to guid 
            Sql( @"
declare
   @fieldTypeGroupGuid UNIQUEIDENTIFIER = 'F4399CEF-827B-48B2-A735-F7806FCFE8E8'

update Attribute set DefaultValue = e.Guid
FROM Attribute a
JOIN FieldType ft ON ft.id = a.FieldTypeId
left outer join [Group] e on e.Id = a.DefaultValue
where len(a.DefaultValue) < 15
and ISNUMERIC(a.DefaultValue) = 1
and ft.Guid in (@fieldTypeGroupGuid)

update AttributeValue set Value = e.Guid
FROM AttributeValue av
JOIN Attribute a on av.AttributeId = a.Id
JOIN FieldType ft ON ft.id = a.FieldTypeId
left outer join [Group] e on e.Id = av.Value
where len(av.Value) < 15
and ISNUMERIC(av.Value) = 1
and ft.Guid in (@fieldTypeGroupGuid)
" );

            // add a Command Timeout attribute for RunSQL job types
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "Class", "Rock.Jobs.RunSQL", "Command Timeout", "Maximum amount of time (in seconds) to wait for the SQL Query to complete. Leave blank to use the SQL default (30 seconds).", 1, "180", "FF66ABF1-B01D-4AE7-814E-95D842B2EA99" );

            // set the commandtimeout for the Calculate Person Duplicates job to 5 minutes
            Sql( @"
                DECLARE 
                    @AttributeId int,
                    @EntityId int

                SET @AttributeId = (SELECT TOP 1 [Id] FROM [Attribute] WHERE [Guid] = 'FF66ABF1-B01D-4AE7-814E-95D842B2EA99')
                SET @EntityId = (SELECT TOP 1 [ID] FROM [ServiceJob] where [Guid] = 'C386528C-3AC6-44E8-884E-A57B571B65D5')

                DELETE FROM [AttributeValue] WHERE [Guid] = '2BE0AA51-DFED-4755-BC13-24AD1E0EFE14'

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],[Value],[Guid])
                VALUES(
                    1,@AttributeId,@EntityId,'300','2BE0AA51-DFED-4755-BC13-24AD1E0EFE14')
" );

            // Fix workflow buttons
            Sql( @"  UPDATE [AttributeValue]
	SET [Value] = REPLACE([Value], '<i class=""fa fa-check""></i>', '')
	WHERE [Guid] in (
			'8CC9689C-E811-4708-97E4-B506212905EC'
			,'F0EF8638-0F0B-4AEC-9304-98E617A89669')" );

            // edit dupe page icon for communication template
            Sql( @"  UPDATE [Page]
  SET [IconCssClass] = 'fa fa-list-alt'
  WHERE [Guid] = '39F75137-90D2-4E6F-8613-F19344767594'" );

        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
        }
    }
}
