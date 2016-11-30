using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace church.ccv.Datamart.Migrations
{
    [Rock.Plugin.MigrationNumber( 3, "1.3.0" )]
    public class RemoveAdultxPercentagesColumns : Rock.Plugin.Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            // Drop Columns
            Sql( @"
if exists(select * from sys.columns 
            where Name = N'AdultsInGroupsPercentage' and Object_ID = Object_ID(N'_church_ccv_Datamart_Neighborhood'))
begin
    alter table [dbo].[_church_ccv_Datamart_Neighborhood] drop column AdultsInGroupsPercentage
end

if exists(select * from sys.columns 
            where Name = N'AdultsBaptizedPercentage' and Object_ID = Object_ID(N'_church_ccv_Datamart_Neighborhood'))
begin
    alter table [dbo].[_church_ccv_Datamart_Neighborhood] drop column AdultsBaptizedPercentage
end

if exists(select * from sys.columns 
            where Name = N'AdultMembersInGroupsPercentage' and Object_ID = Object_ID(N'_church_ccv_Datamart_Neighborhood'))
begin
    alter table [dbo].[_church_ccv_Datamart_Neighborhood] drop column AdultMembersInGroupsPercentage
end

if exists(select * from sys.columns 
            where Name = N'AdultAttendeesInGroupsPercentage' and Object_ID = Object_ID(N'_church_ccv_Datamart_Neighborhood'))
begin
    alter table [dbo].[_church_ccv_Datamart_Neighborhood] drop column AdultAttendeesInGroupsPercentage
end" );

            // Add Columns
            Sql( @"
if not exists(select * from sys.columns 
            where Name = N'AdultsTakenStartingPoint' and Object_ID = Object_ID(N'_church_ccv_Datamart_Neighborhood'))
begin
    alter table [dbo].[_church_ccv_Datamart_Neighborhood] add AdultsTakenStartingPoint nvarchar(max) null
end

if not exists(select * from sys.columns 
            where Name = N'AdultsServing' and Object_ID = Object_ID(N'_church_ccv_Datamart_Neighborhood'))
begin
    alter table [dbo].[_church_ccv_Datamart_Neighborhood] add AdultsServing int null
end

if not exists(select * from sys.columns 
            where Name = N'LastPublicNote' and Object_ID = Object_ID(N'_church_ccv_Datamart_Person'))
begin
    alter table [dbo]._church_ccv_Datamart_Person add LastPublicNote nvarchar(max) null
end
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
