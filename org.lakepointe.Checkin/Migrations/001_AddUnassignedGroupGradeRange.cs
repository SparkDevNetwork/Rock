using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Rock;
using Rock.Data;
using Rock.Model;
using Rock.Plugin;


namespace org.lakepointe.Checkin.Migrations
{
    [MigrationNumber(1, "1.9.4.1")]
    class AddUnassignedGroupGradeRange : Migration
    {
        public override void Up()
        {
            /* Grade School & Soar */

            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Group", Rock.SystemGuid.FieldType.DEFINED_VALUE_RANGE, "GroupTypeId", "439", "Unassigned Grade Range",
                 "Allows a student to check in to an 'unassigned' group based on their grade when they are not eligible to check in to a group when the Already Belongs group rule is used.",
                 1073, "", "46d03740-c18b-425d-848c-b6ae6b2848a3", key: "UnassignedGradeRange");

            RockMigrationHelper.UpdateAttributeQualifier("46d03740-c18b-425d-848c-b6ae6b2848a3", "definedtype", "24e5a79f-1e62-467a-ad5d-0d10a2328b4d", "09f10345-c5b8-43b6-a678-d3aaa1a31d79");
            RockMigrationHelper.UpdateAttributeQualifier("46d03740-c18b-425d-848c-b6ae6b2848a3", "displaydescription", "True", "9c6768c4-235e-4222-ac27-11a87b3fab46");

            Sql(@"
                DECLARE @AttributeId INT =  (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '46d03740-c18b-425d-848c-b6ae6b2848a3')
                DECLARE @CategoryId INT = (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = 'c8e0fd8d-3032-4acd-9db9-ff70b11d6bcc' )

                IF (@AttributeId IS NOT NULL AND @CategoryId IS NOT NULL)
                BEGIN
                    IF NOT EXISTS (SELECT AttributeId FROM [dbo].[AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId )
                    BEGIN
                        INSERT INTO [dbo].[AttributeCategory]([AttributeId], [CategoryId]) VALUES (@AttributeId, @CategoryId)
                    END
                END
            ");

            // Fuel Group Type
            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Group", Rock.SystemGuid.FieldType.DEFINED_VALUE_RANGE, "GroupTypeId", "342", "Unassigned Grade Range",
                 "Allows a student to check in to an 'unassigned' group based on their grade when they are not eligible to check in to a group when the Already Belongs group rule is used.",
                 1074, "", "1dc781e2-e98d-4a9c-8387-d3009444737b", key: "UnassignedGradeRange");

            RockMigrationHelper.UpdateAttributeQualifier("1dc781e2-e98d-4a9c-8387-d3009444737b", "definedtype", "24e5a79f-1e62-467a-ad5d-0d10a2328b4d", "47c4173f-d11e-4abb-a9bc-56b57906775d");
            RockMigrationHelper.UpdateAttributeQualifier("1dc781e2-e98d-4a9c-8387-d3009444737b", "displaydescription", "True", "a676ab64-625a-4109-b227-554a3c26d26a");

            Sql(@"
                DECLARE @AttributeId INT =  (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '1dc781e2-e98d-4a9c-8387-d3009444737b')
                DECLARE @CategoryId INT = (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = 'c8e0fd8d-3032-4acd-9db9-ff70b11d6bcc' )

                IF (@AttributeId IS NOT NULL AND @CategoryId IS NOT NULL)
                BEGIN
                    IF NOT EXISTS (SELECT AttributeId FROM [dbo].[AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId )
                    BEGIN
                        INSERT INTO [dbo].[AttributeCategory]([AttributeId], [CategoryId]) VALUES (@AttributeId, @CategoryId)
                    END
                END
            ");

            //Outreach Group Type

            RockMigrationHelper.UpdateEntityAttribute("Rock.Model.Group", Rock.SystemGuid.FieldType.DEFINED_VALUE_RANGE, "GroupTypeId", "279", "Unassigned Grade Range",
                "Allows a student to check in to an 'unassigned' group based on their grade when they are not eligible to check in to a group when the Already Belongs group rule is used.",
                1075, "", "762256ec-c063-4464-804f-0e63ff68e478", key: "UnassignedGradeRange");

            RockMigrationHelper.UpdateAttributeQualifier("762256ec-c063-4464-804f-0e63ff68e478", "definedtype", "24e5a79f-1e62-467a-ad5d-0d10a2328b4d", "2d36682c-affb-4cda-89e9-be0bbf09ac65");
            RockMigrationHelper.UpdateAttributeQualifier("762256ec-c063-4464-804f-0e63ff68e478", "displaydescription", "True", "c31b6802-8b01-4178-8370-816ac030130c");

            Sql(@"
                DECLARE @AttributeId INT =  (SELECT [Id] FROM [dbo].[Attribute] WHERE [Guid] = '762256ec-c063-4464-804f-0e63ff68e478')
                DECLARE @CategoryId INT = (SELECT [Id] FROM [dbo].[Category] WHERE [Guid] = 'c8e0fd8d-3032-4acd-9db9-ff70b11d6bcc' )

                IF (@AttributeId IS NOT NULL AND @CategoryId IS NOT NULL)
                BEGIN
                    IF NOT EXISTS (SELECT AttributeId FROM [dbo].[AttributeCategory] WHERE [AttributeId] = @AttributeId AND [CategoryId] = @CategoryId )
                    BEGIN
                        INSERT INTO [dbo].[AttributeCategory]([AttributeId], [CategoryId]) VALUES (@AttributeId, @CategoryId)
                    END
                END
            ");
        }

        public override void Down()
        {
            // Grade School and Soar
            Sql(@"
                DECLARE @AttributeId INT = (SELECT[Id] FROM[dbo].[Attribute] WHERE[Guid] = '46d03740-c18b-425d-848c-b6ae6b2848a3')
                DECLARE @CategoryId INT = (SELECT[Id] FROM[dbo].[Category] WHERE[Guid] = 'c8e0fd8d-3032-4acd-9db9-ff70b11d6bcc' )

                IF(@AttributeId IS NOT NULL AND @CategoryId IS NOT NULL)
                BEGIN
                    DELETE FROM[dbo].[AttributeCategory] WHERE[AttributeId] = @AttributeId AND[CategoryId] = @CategoryId
                END
            ");

            RockMigrationHelper.DeleteAttribute("46d03740-c18b-425d-848c-b6ae6b2848a3");

            // Fuel Weekend
            Sql(@"
                DECLARE @AttributeId INT = (SELECT[Id] FROM[dbo].[Attribute] WHERE[Guid] = '1dc781e2-e98d-4a9c-8387-d3009444737b')
                DECLARE @CategoryId INT = (SELECT[Id] FROM[dbo].[Category] WHERE[Guid] = 'c8e0fd8d-3032-4acd-9db9-ff70b11d6bcc' )

                IF(@AttributeId IS NOT NULL AND @CategoryId IS NOT NULL)
                BEGIN
                    DELETE FROM[dbo].[AttributeCategory] WHERE[AttributeId] = @AttributeId AND[CategoryId] = @CategoryId
                END
            ");

            RockMigrationHelper.DeleteAttribute("1dc781e2-e98d-4a9c-8387-d3009444737b");

            // Outreach
            Sql(@"
                DECLARE @AttributeId INT = (SELECT[Id] FROM[dbo].[Attribute] WHERE[Guid] = '762256ec-c063-4464-804f-0e63ff68e478')
                DECLARE @CategoryId INT = (SELECT[Id] FROM[dbo].[Category] WHERE[Guid] = 'c8e0fd8d-3032-4acd-9db9-ff70b11d6bcc' )

                IF(@AttributeId IS NOT NULL AND @CategoryId IS NOT NULL)
                BEGIN
                    DELETE FROM[dbo].[AttributeCategory] WHERE[AttributeId] = @AttributeId AND[CategoryId] = @CategoryId
                END
            ");

            RockMigrationHelper.DeleteAttribute("762256ec-c063-4464-804f-0e63ff68e478");

        }

    }
}
