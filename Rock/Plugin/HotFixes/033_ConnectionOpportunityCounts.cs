﻿// <copyright>
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

namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Rock.Plugin.Migration" />
    [MigrationNumber( 33, "1.6.9" )]
    public class ConnectionOpportunityCounts : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            RockMigrationHelper.DeleteSecurityAuthForPage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD" );
            RockMigrationHelper.AddSecurityAuthForPage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD", 3, "View", false, "", 1, "21EF9B65-A9BA-4C9C-8DBB-3247F668768B" ); // Page:Fundraising Matching
            RockMigrationHelper.AddSecurityAuthForPage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD", 2, "View", true, "2539CF5D-E2CE-4706-8BBF-4A9DF8E763E9", 0, "5310F062-147F-4FDD-8671-10516FD94251" ); // Page:Fundraising Matching
            RockMigrationHelper.AddSecurityAuthForPage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD", 1, "View", true, "6246A7EF-B7A3-4C8C-B1E4-3FF114B84559", 0, "6FA07B13-7382-4988-B7FD-F506D3EDE7D8" ); // Page:Fundraising Matching
            RockMigrationHelper.AddSecurityAuthForPage( "A3EF32AC-B0FE-4140-A6F4-134FDD247CBD", 0, "View", true, "628C51A8-4613-43ED-A18D-4A6FB999273E", 0, "7BE99C1B-B96E-4EDF-9D3F-8A8247A2B04F" ); // Page:Fundraising Matching

            Sql( @"
    DECLARE @EntityTypeId int = ( SELECT TOP 1 [Id] FROM [EntityType] WHERE [Name] = 'Rock.Model.Block' )
    DECLARE @BlockTypeId int = ( SELECT TOP 1 [Id] FROM [BlockType] WHERE [Path] = '~/Blocks/Connection/MyConnectionOpportunities.ascx' )
    DECLARE @AttributeId int = ( SELECT TOP 1 [Id] FROM [Attribute] WHERE [EntityTypeId] = @EntityTypeId AND [EntityTypeQualifierColumn] = 'BlockTypeId' AND [EntityTypeQualifierValue] = CAST( @BlockTypeId AS varchar ) AND [Key] = 'OpportunitySummaryTemplate' )
    UPDATE [AttributeValue] SET [Value] = 
	    '<span class=""item-count"" title=""There are {{ ''active connection'' | ToQuantity:OpportunitySummary.TotalRequests }} in this opportunity."">{{ OpportunitySummary.TotalRequests | Format:''#,###,##0'' }}</span>' 
	    + [Value]
    WHERE [AttributeId] = @AttributeId
    AND [Value] NOT LIKE '<span class=""item-count""%'
" );

        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
        }
    }
}
