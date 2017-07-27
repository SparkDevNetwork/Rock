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
    public partial class BinaryFileSize : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.BinaryFile", "FileSize", c => c.Long());

            Sql( @"
    UPDATE bf SET [FileSize] = datalength(bfd.Content)
    FROM [BinaryFile] bf
    INNER JOIN [BinaryFileData] bfd ON bfd.[Id] = bf.[Id]
    WHERE [FileSize] IS NULL
" );
            // MP: Metric Source Lava Defined Value
            RockMigrationHelper.UpdateDefinedValue( "D6F323FF-6EF2-4DA7-A82C-61399AC1D798", "Lava", "The Metric Values are populated from custom Lava", "2868A3E8-4632-4966-84CD-EDB8B775D66C" );

            // DT: New job settings for site crawler
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", SystemGuid.FieldType.TEXT, "Class", "Rock.Jobs.IndexRockSite", "Login Id", "The login to impersonate when navigating to secured pages. Leave blank if secured pages should not be indexed.", 1, "", "71751F8A-1ECF-4AC6-88B0-B6236150DBEC" );
            RockMigrationHelper.UpdateEntityAttribute( "Rock.Model.ServiceJob", SystemGuid.FieldType.TEXT, "Class", "Rock.Jobs.IndexRockSite", "Password", "The password associated with the Login Id.", 2, "", "E30D75EA-3B7D-4A19-87FC-3C857AA0CB81" );
            RockMigrationHelper.UpdateAttributeQualifier( "71751F8A-1ECF-4AC6-88B0-B6236150DBEC", "ispassword", "False", "CBC91D74-F74B-47E3-808F-78EF062C8458" );
            RockMigrationHelper.UpdateAttributeQualifier( "E30D75EA-3B7D-4A19-87FC-3C857AA0CB81", "ispassword", "True", "03A4795F-446B-4E38-90C9-F7931FC996F5" );
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.BinaryFile", "FileSize");
        }
    }
}
