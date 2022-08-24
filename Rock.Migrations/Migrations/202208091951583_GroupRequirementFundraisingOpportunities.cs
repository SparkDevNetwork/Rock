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
    public partial class GroupRequirementFundraisingOpportunities : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            // Get the GroupTypeId for Fundraising Opportunity.
            var groupReq_GroupTypeIdSql = $"SELECT TOP 1 [Id] FROM GroupType WHERE [Guid] = '{SystemGuid.GroupType.GROUPTYPE_FUNDRAISINGOPPORTUNITY}'";
            var groupReq_GroupTypeId = SqlScalar( groupReq_GroupTypeIdSql ).ToIntSafe();
            if ( groupReq_GroupTypeId > 0 )
            {
                RockMigrationHelper.AddOrUpdateEntityAttribute(
                    "Rock.Model.Group", SystemGuid.FieldType.SINGLE_SELECT, "GroupTypeId",
                    groupReq_GroupTypeId.ToString(),
                    "Participation Type",
                    "Participation Type",
                    @"The type of participation in this group.", 18, @"1", SystemGuid.Attribute.PARTICIPATION_TYPE, "ParticipationType" );

                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "values", @"1^Individual,2^Family", "4B50E240-74D5-4953-B9B3-99DC4EEB5C84" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "fieldtype", @"ddl", "93A56DAE-2672-43F4-8AC9-C144B0AB84B3" );
                // Qualifier for attribute: ParticipationType
                RockMigrationHelper.UpdateAttributeQualifier( SystemGuid.Attribute.PARTICIPATION_TYPE, "repeatColumns", @"", "04D5573C-4671-45B7-A591-13EC6EA0FF99" );

            }
            // Update the Fundraising Entry Transaction Header
            string oldAssignParticipantReference;
            string newAssignParticipantReference;

            string oldParticipantReference;
            string newParticipantReference;

            // This is for the Participant Lava.
            oldAssignParticipantReference = "{% assign groupMember = TransactionEntity %}";
            newAssignParticipantReference = @"{% assign groupMember = TransactionEntity %}
{% assign participationType = PageParameter[''ParticipationMode''] %}";

            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{oldAssignParticipantReference}','{newAssignParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );


            oldParticipantReference = "{{ groupMember.Person.FullName }}";
            newParticipantReference = @"
                 {% if participationType == ''2'' %}
                   {{ groupMember.Person.PrimaryFamily.Name }}
                 {% else %}
                   {{ groupMember.Person.FullName }}
                 {% endif  %}
                    ";

            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{oldParticipantReference}','{newParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );


            // Rock.Model.RequirementCheckType.Manual enum value is '2'.
            Sql( $@"UPDATE gmr
                    SET gmr.[WasManuallyCompleted] = 1, gmr.[ManuallyCompletedDateTime] = gmr.[RequirementMetDateTime]
                        FROM [dbo].[GroupRequirementType] grt INNER JOIN [dbo].[GroupRequirement] gr ON grt.[Id] = gr.[GroupRequirementTypeId] 
                        INNER JOIN [dbo].[GroupMemberRequirement] gmr ON gr.[Id] = gmr.[GroupRequirementId]
                        WHERE (gmr.[RequirementMetDateTime] IS NOT NULL AND grt.[RequirementCheckType] = 2" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            string oldAssignParticipantReference;
            string newAssignParticipantReference;

            string oldParticipantReference;
            string newParticipantReference;

            // Reverse the changes that were made.
            oldAssignParticipantReference = "{% assign groupMember = TransactionEntity %}";
            newAssignParticipantReference = @"{% assign groupMember = TransactionEntity %}
{% assign participationType = PageParameter[''ParticipationMode''] %}";

            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{newAssignParticipantReference }','{oldAssignParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );


            oldParticipantReference = "{{ groupMember.Person.FullName }}";
            newParticipantReference = @"
                 {% if participationType == ''2'' %}
                   {{ groupMember.Person.PrimaryFamily.Name }}
                 {% else %}
                   {{ groupMember.Person.FullName }}
                 {% endif  %}
                    ";

            Sql( $@"
                    UPDATE [AttributeValue]
                    SET [Value] = REPLACE([Value],'{newParticipantReference}','{oldParticipantReference}')
                    WHERE [Id] IN (SELECT av.[Id]
                            FROM [dbo].[AttributeValue] av
                            INNER JOIN [Attribute] a ON a.[Id] = av.[AttributeId]
                            WHERE a.[Guid] = '{SystemGuid.Attribute.FUNDRAISING_TRANSACTION_HEADER}')
                " );

            // Rock.Model.Group: Participation Type.
            RockMigrationHelper.DeleteAttribute( SystemGuid.Attribute.PARTICIPATION_TYPE );
        }
    }
}
