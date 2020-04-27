using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.bemaservices.MinistrySafe.Migrations
{
    [MigrationNumber( 1, "1.8.5" )]
    public class Data : Migration
    {
        public override void Up()
        {
            Sql( @"
                CREATE TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser](
	                [Id] [int] IDENTITY(1,1) NOT NULL,
                    [UserId] [int] NOT NULL,
                    [FirstName] [nvarchar](max) NULL,
                    [LastName] [nvarchar](max) NULL,
	                [PersonAliasId] [int] NOT NULL,
	                [Score] [int] NULL,
                    [UserType] [nvarchar](max) NULL,
                    [SurveyCode] [nvarchar](max) NULL,
                    [DirectLoginUrl] [nvarchar](max) NULL,
	                [CompletedDateTime] [datetime] NULL,
	                [RequestDate] [datetime] NULL,
	                [ResponseDate] [datetime] NULL,
	                [WorkflowId] [int] NULL,
	                [Guid] [uniqueidentifier] NOT NULL,
	                [CreatedDateTime] [datetime] NULL,
	                [ModifiedDateTime] [datetime] NULL,
	                [CreatedByPersonAliasId] [int] NULL,
	                [ModifiedByPersonAliasId] [int] NULL,
	                [ForeignKey] [nvarchar](50) NULL,
                    [ForeignGuid] [uniqueidentifier] NULL,
                    [ForeignId] [int] NULL,
                 CONSTRAINT [PK__com_bemaservices_MinistrySafe_MinistrySafeUser] PRIMARY KEY CLUSTERED 
                (
	                [Id] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
                ) ON [PRIMARY]

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_WorkflowId] FOREIGN KEY([WorkflowId])
                REFERENCES [dbo].[Workflow] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] CHECK CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_WorkflowId]

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_PersonAliasId] FOREIGN KEY([PersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] CHECK CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_PersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_CreatedByPersonAliasId] FOREIGN KEY([CreatedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] CHECK CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_CreatedByPersonAliasId]

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser]  WITH CHECK ADD CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_ModifiedByPersonAliasId] FOREIGN KEY([ModifiedByPersonAliasId])
                REFERENCES [dbo].[PersonAlias] ([Id])

                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] CHECK CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_ModifiedByPersonAliasId]

" );          
            RockMigrationHelper.UpdateEntityType( "com.bemaservices.MinistrySafe.Model.MinistrySafeUser", "F6261353-1C34-4CE0-8E8C-201AB1A1CA94", true, true );

            RockMigrationHelper.AddGlobalAttribute( "9C204CD0-1233-41C5-818A-C5DA439445AA", "", "", "Ministry Safe API Token", "", 0, "", "C3F952C7-F515-4950-B0EB-8737A013CD85" );

            // Defined Types
            RockMigrationHelper.AddDefinedType( "Global", "Ministry Safe Training Types", "", "95EF81D2-C192-4B9E-A7A3-5E1E90BDA3CE" );
            RockMigrationHelper.AddDefinedType( "Global", "Ministry Safe User Types", "", "559E79C6-2EAB-4A0D-A16F-59D9B63F002F" );

            // Defined Values
            RockMigrationHelper.AddDefinedValue( "95EF81D2-C192-4B9E-A7A3-5E1E90BDA3CE", "standard", "Standard Sexual Abuse Awareness Training", "C8E8E22A-1D27-4179-AF30-313D2EE896EA" );
            RockMigrationHelper.AddDefinedValue( "95EF81D2-C192-4B9E-A7A3-5E1E90BDA3CE", "youth", "Youth Sports Sexual Abuse Awareness Training", "E62E5933-2196-4D73-AC7C-061BFC349072" );
            RockMigrationHelper.AddDefinedValue( "95EF81D2-C192-4B9E-A7A3-5E1E90BDA3CE", "camp", "Camp-Focused Sexual Abuse Awareness Training", "1F6FF1C7-6C90-4026-B631-1572A5BA5FF0" );
            RockMigrationHelper.AddDefinedValue( "95EF81D2-C192-4B9E-A7A3-5E1E90BDA3CE", "spanish", "Spanish Sexual Abuse Awareness Training", "B00AF2D3-6BB2-463E-A82F-E9D78B0F0FB3" );
            RockMigrationHelper.AddDefinedValue( "559E79C6-2EAB-4A0D-A16F-59D9B63F002F", "employee", "Employee", "63E94DA6-DC22-45E9-9374-11BE3ED509DD" );
            RockMigrationHelper.AddDefinedValue( "559E79C6-2EAB-4A0D-A16F-59D9B63F002F", "volunteer", "Volunteer", "6DBED334-45B7-433F-828D-95EFDD23F464" );

            // Person Attributes
            RockMigrationHelper.UpdatePersonAttributeCategory( "MinistrySafe", "fa fa-lock", "", "CB481AB7-E0F9-4A3E-B846-0F5E5C94C038" );
            RockMigrationHelper.UpdatePersonAttribute( "59D5A94C-94A0-4630-B80A-BB25697D74C7", "CB481AB7-E0F9-4A3E-B846-0F5E5C94C038", "Training Type", "TrainingType", "", "", 0, "", "05E50A5A-DFB8-4656-9210-9027565D7864" );
            RockMigrationHelper.UpdatePersonAttribute( "FE95430C-322D-4B67-9C77-DFD1D4408725", "CB481AB7-E0F9-4A3E-B846-0F5E5C94C038", "Training Date", "TrainingDate", "", "", 0, "", "0B1607AF-6900-406C-8F7F-8DC03FC253F3" );
            RockMigrationHelper.UpdatePersonAttribute( "A75DFC58-7A1B-4799-BF31-451B2BBE38FF", "CB481AB7-E0F9-4A3E-B846-0F5E5C94C038", "Training Score", "TrainingScore", "", "", 0, "", "937C7D10-74DD-4512-9E0D-79B5B989DEB7" );
            RockMigrationHelper.UpdatePersonAttribute( "7525C4CB-EE6B-41D4-9B64-A08048D5A5C0", "CB481AB7-E0F9-4A3E-B846-0F5E5C94C038", "Training Result", "TrainingResult", "", "", 0, "", "C19F3842-7CEE-4772-B2ED-86B7968E2879" );

            // MinistrySafeTrainingType
            var definedTypeId = SqlScalar( "Select Top 1 Id From DefinedType Where Guid = '95EF81D2-C192-4B9E-A7A3-5E1E90BDA3CE'" ).ToString();
            RockMigrationHelper.UpdateAttributeQualifier( "05E50A5A-DFB8-4656-9210-9027565D7864", "allowmultiple", @"False", "DFEBD204-BD71-48CB-BF1B-B0DD6873D18E" );
            RockMigrationHelper.UpdateAttributeQualifier( "05E50A5A-DFB8-4656-9210-9027565D7864", "definedtype", definedTypeId, "5FB837BB-2072-481F-8A1C-7A4BA2BF5150" );
            RockMigrationHelper.UpdateAttributeQualifier( "05E50A5A-DFB8-4656-9210-9027565D7864", "displaydescription", @"True", "FD10024C-D776-4F05-B535-F67026DD5C67" );
            RockMigrationHelper.UpdateAttributeQualifier( "05E50A5A-DFB8-4656-9210-9027565D7864", "enhancedselection", @"False", "0C180AC2-9108-4875-B39F-96FAF29082F5" );
            RockMigrationHelper.UpdateAttributeQualifier( "05E50A5A-DFB8-4656-9210-9027565D7864", "includeInactive", @"False", "9F13948A-7DBA-4019-BACF-38DBE4A99B67" );

            // TrainingDate
            RockMigrationHelper.UpdateAttributeQualifier( "0B1607AF-6900-406C-8F7F-8DC03FC253F3", "datePickerControlType", @"Date Picker", "B799A5F5-E256-4AAD-A4D2-CA530885AE14" );
            RockMigrationHelper.UpdateAttributeQualifier( "0B1607AF-6900-406C-8F7F-8DC03FC253F3", "displayCurrentOption", @"False", "19F62F74-DEDF-4D41-BE9D-205A0D38877E" );
            RockMigrationHelper.UpdateAttributeQualifier( "0B1607AF-6900-406C-8F7F-8DC03FC253F3", "displayDiff", @"False", "9B364D96-4AAE-45DF-84C7-CEC765A139B3" );
            RockMigrationHelper.UpdateAttributeQualifier( "0B1607AF-6900-406C-8F7F-8DC03FC253F3", "format", @"", "DFA365F9-FB7C-4110-AFDE-73347EBD18A1" );
            RockMigrationHelper.UpdateAttributeQualifier( "0B1607AF-6900-406C-8F7F-8DC03FC253F3", "futureYearCount", @"", "A53FC126-C0BA-4CB1-A223-5F2A31DAADA5" );

            // TrainingResult
            RockMigrationHelper.UpdateAttributeQualifier( "C19F3842-7CEE-4772-B2ED-86B7968E2879", "fieldtype", @"ddl", "3B41FC8D-2709-4F38-B4CF-6E6B8866CF4C" );
            RockMigrationHelper.UpdateAttributeQualifier( "C19F3842-7CEE-4772-B2ED-86B7968E2879", "repeatColumns", @"", "A57054D1-814D-44F8-8D11-A55922531056" );
            RockMigrationHelper.UpdateAttributeQualifier( "C19F3842-7CEE-4772-B2ED-86B7968E2879", "values", @"Pass,Fail,Pending", "5C9AB718-6B8C-4D19-AFDC-86E1E7755505" );

            // Security Role
            RockMigrationHelper.AddSecurityRoleGroup( "RSR - Ministry Safe Training Administration", "The group of people responsible for approving Ministry Safe Trainings.", "22AD73FD-B267-49C3-ABF3-DF8805898E9C" );

            // Person Profile Badge
            UpdateBadge( "Ministry Safe Badge", "Shows whether someone has taken a Ministry Safe Training, as well as their score.", "Rock.Badge.Component.Liquid", 0, "9E9B9FAF-C7B8-40AA-B0C9-24177058943B" );
            AddBadgeAttributeValue( "9E9B9FAF-C7B8-40AA-B0C9-24177058943B", "01C9BA59-D8D4-4137-90A6-B3C06C70BBC3", @"{% assign trainingDate = Person | Attribute:'TrainingDate' %} 
            {% assign trainingScore = Person | Attribute:'TrainingScore' %}
            {% assign trainingResult = Person | Attribute:'TrainingResult' %}
            {% assign isDisabled = false %}
            {% assign badgeColor = '#939393' %}

            {% if trainingResult == 'Pending' %}
                {% capture tooltipText %}{{ Person.NickName }} is currently taking a Ministry Safe Training.{% endcapture %}
                {% capture badgeColor %}yellow{% endcapture %}
            {% elseif trainingResult == 'Fail' %}
                {% capture tooltipText %}{{ Person.NickName }} has failed their Ministry Safe Training.{% endcapture %}
                {% capture badgeColor %}red{% endcapture %}
            {% else %}
                {% assign daysSinceTraining = 'Now' | DateDiff:trainingDate,'d' %}
                {% if daysSinceTraining < -730 or trainingDate == blank %}
                    {% assign isDisabled = true %}
                    {% if daysSinceTraining < -730 %}
                        {% capture tooltipText %}{{ Person.NickName }}'s Ministry Safe Training has expired.{% endcapture %}
                    {% elseif trainingDate == blank %}
                        {% capture tooltipText %}{{ Person.NickName }} has not completed a Ministry Safe Training.{% endcapture %}
                    {% endif %}
                {% else %}
                    {% capture tooltipText %}{{ Person.NickName }} has completed their Ministry Safe Training.{% endcapture %}
                    {% capture badgeColor %}green{% endcapture %}
                {% endif %}
            {% endif %}
            <div class='badge badge-lastvisitonsite {% if isDisabled == true %}badge-disabled{% endif %}' data-toggle='tooltip' data-original-title='{{ tooltipText }}'>
                <div class='badge-content'>
                    <i class='badge-icon fa fa-chalkboard {% if isDisabled == true %}badge-disabled{% endif %}' style='color: {{badgeColor}};'></i>
                    <span class='duration {% if isDisabled == true %}badge-disabled{% endif %}'  style='color: {{badgeColor}};'>{{ trainingScore }}</span>
                </div>
            </div>" );
        }
        public override void Down()
        {
            RockMigrationHelper.DeleteEntityType( "F6261353-1C34-4CE0-8E8C-201AB1A1CA94" );

            Sql( @"              
                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] DROP CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_WorkflowId]
                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] DROP CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_PersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] DROP CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_ModifiedByPersonAliasId]
                ALTER TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser] DROP CONSTRAINT [FK__com_bemaservices_MinistrySafe_MinistrySafeUser_CreatedByPersonAliasId]
                DROP TABLE [dbo].[_com_bemaservices_MinistrySafe_MinistrySafeUser]
" );
        }

        public void UpdateBadge( string name, string description, string entityTypeName, int order, string guid )
        {
            Sql( string.Format( @"
                    DECLARE @BadgeComponentEntityTypeId int = (SELECT [ID] FROM [EntityType] WHERE [Name] = '{2}')
                    DECLARE @EntityTypeId int = (SELECT [ID] FROM [EntityType] WHERE [Name] = 'Rock.Model.Person')

                    IF EXISTS ( SELECT * FROM [Badge] where [Guid] = '{4}')
                    BEGIN
                        UPDATE [Badge] set
                            [Name] = '{0}',
                            [Description] = '{1}',
                            [BadgeComponentEntityTypeId] = @BadgeComponentEntityTypeId,
                            [EntityTypeId] = @EntityTypeId,
                            [Order] = {3}
                        WHERE [Guid] = '{4}'

                    END
                    ELSE
                    BEGIN
                        INSERT INTO [Badge] ([Name],[Description],[BadgeComponentEntityTypeId],[EntityTypeId],[Order],[Guid])
                            VALUES ('{0}', '{1}', @BadgeComponentEntityTypeId,@EntityTypeId, {3}, '{4}')
                    END

",
                    name.Replace( "'", "''" ),
                    description.Replace( "'", "''" ),
                    entityTypeName,
                    order,
                    guid )
            );
        }

        public void AddBadgeAttributeValue( string personBadgeGuid, string attributeGuid, string value )
        {
            Sql( string.Format( @"

                DECLARE @PersonBadgeId int
                SET @PersonBadgeId = (SELECT [Id] FROM [Badge] WHERE [Guid] = '{0}')

                DECLARE @AttributeId int
                SET @AttributeId = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '{1}')

                -- Delete existing attribute value first (might have been created by Rock system)
                DELETE [AttributeValue]
                WHERE [AttributeId] = @AttributeId
                AND [EntityId] = @PersonBadgeId

                INSERT INTO [AttributeValue] (
                    [IsSystem],[AttributeId],[EntityId],
                    [Value],
                    [Guid])
                VALUES(
                    1,@AttributeId,@PersonBadgeId,
                    '{2}',
                    NEWID())
",
                    personBadgeGuid,
                    attributeGuid,
                    value.Replace( "'", "''" )
                )
            );
        }
    }
}
