using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// This hotfix update is to address security concerns in the file uploader and is being retroactivly applied to v7.6 and >.
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber(66,"1.8.6")]
    public class MigrationRollupsForV8_7_3 :Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //UpdateStatementGenerator();
            //UpdateCalendarEventField();
            //UpdatePersonTokenCreate();
            //UpdatePersonTokenCreateAttribute();
           
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            
        }

        /// <summary>Updates the Statement Generator.
        ///SK: Updated Statement Generator lava template to check for null StreetAddress2
        /// </summary>
        private void UpdateStatementGenerator()
        {
            Sql( @"
DECLARE @StatementDefinedTypeId int = ( SELECT TOP 1 [Id] FROM [DefinedType] WHERE [Guid] = '74A23516-A20A-40C9-93B5-1AB5FDFF6750' )
DECLARE @LavaTemplatAttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [Key]='LavaTemplate' AND  [EntityTypeQualifierColumn]='DefinedTypeId' AND [EntityTypeQualifierValue]=@StatementDefinedTypeId )
select * from [AttributeValue] WHERE 
	[AttributeId]=@LavaTemplatAttributeId
UPDATE 
	[AttributeValue]
SET 
	[Value]=REPLACE([Value],'{% if StreetAddress2 != '''' %}','{% if StreetAddress2 != null && StreetAddress2 != '''' %}')
WHERE 
	[AttributeId]=@LavaTemplatAttributeId
" );
        }

        /// <summary>Updates the Calendar Event field.
        ///SK: Updated Calendar Event Field to have correct default value in block setting.
        /// </summary>
        private void UpdateCalendarEventField()
        {
            Sql( @"
UPDATE
	[Attribute]
SET
	[DefaultValue]='8A444668-19AF-4417-9C74-09F842572974'
 WHERE
  [Guid] 
  IN
  ('D4CD78A2-E893-46D3-A68A-2F4D0EFCA97A','BBD2DDE8-EDA0-4A54-8895-F56D55D6A450')
" );
        }

        /// <summary>Updates the PersonTokenCreate.
        /// SK: update the existing usage of PersonTokenCreate to use PersonActionIdentifier in several places.(Spilt the SQL into two(JM)
        /// This is specifically updating the Photo Upload workflow's Send Email Action body value
        /// </summary>
        private void UpdatePersonTokenCreate()
        {
            Sql( @"
                DECLARE @ActionTypeId int = (SELECT [Id] FROM [WorkflowActionType] WHERE [Guid] = '2C88DD58-40D3-4927-8A12-7968092C4929')
                DECLARE @AttributeId int = (SELECT [Id] FROM [Attribute] WHERE [Guid] = '4D245B9E-6B03-46E7-8482-A51FBA190E4D')
                DECLARE @AttributeValue nvarchar(MAX)

                IF @ActionTypeId IS NOT NULL AND @AttributeId IS NOT NULL
                BEGIN
                    SET @AttributeValue = (SELECT [Value] FROM [AttributeValue]  WHERE
                        [AttributeId] = @AttributeId
                    AND [EntityId] = @ActionTypeId)
                    
                    IF @ActionTypeId IS NOT NULL AND @AttributeId IS NOT NULL
                    SET @AttributeValue = Replace(@AttributeValue,'OptOut/{{ Person.UrlEncodedKey }}','OptOut/{{ Person | PersonActionIdentifier:''OptOut'' }}')
                    SET @AttributeValue = Replace(@AttributeValue,'Unsubscribe/{{ Person.UrlEncodedKey }}','Unsubscribe/{{ Person | PersonActionIdentifier:''Unsubscribe'' }}')
                    BEGIN
                        UPDATE
                            [AttributeValue]
                        SET
                           [Value]=@AttributeValue
                        WHERE
                            [AttributeId] = @AttributeId AND [EntityId] = @ActionTypeId
                    END
                END
" );

           
        }

        /// <summary>Updates the PersonTokenCreate Attribute.
        ///SK: update the existing usage of PersonTokenCreate to use PersonActionIdentifier in several places.(Spilt the SQL into two(JM))
        ///This is specifically updating the 'Unsubscribe HTML' and 'Non-HTML Content' attribute values for the Rock.Communication.Medium.Email
        /// </summary>
        private void UpdatePersonTokenCreateAttribute()
        {
            Sql( @"
                    UPDATE Attribute
                    SET DefaultValue = Replace(DefaultValue,'PersonTokenCreate:43200,3','PersonActionIdentifier:''Unsubscribe''')
                    WHERE [Guid] = '2942EFCB-9BCF-4A16-9A78-D6149E2EAAD3'

                    UPDATE AttributeValue 
                    SET [Value] = Replace([Value],'PersonTokenCreate:43200,3','PersonActionIdentifier:''Unsubscribe''')
                    WHERE AttributeId in (select Id from Attribute where [Guid] = '2942EFCB-9BCF-4A16-9A78-D6149E2EAAD3')

                    Update Attribute
                    SET DefaultValue = Replace(DefaultValue,'PersonTokenCreate:43200,3','PersonActionIdentifier:''Unsubscribe''')
                    where 
                    [Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88'

                    Update AttributeValue
                    SET [Value] = Replace([Value],'PersonTokenCreate:43200,3','PersonActionIdentifier:''Unsubscribe''')
                    where
                    AttributeId in (select Id from Attribute where [Guid] = 'FDB3E4EB-DE16-4A43-AE92-B4EAA3D5DF88')
" );
        }

    }
}
