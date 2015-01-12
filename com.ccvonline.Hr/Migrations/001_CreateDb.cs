using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Hr.Migrations
{
    [MigrationNumber( 1, "1.2.0" )]
    public class CreateDb : Migration
    {
        public override void Up()
        {
            // Create Tables
            Sql( @"
create table [dbo].[_com_ccvonline_Hr_TimeCard] (
    [Id] [int] not null identity,
    [TimeCardPayPeriodId] [int] not null,
    [PersonAliasId] [int] not null,
    [TimeCardStatus] [int] not null,
    [SubmittedToPersonAliasId] [int] null,
    [SubmittedDateTime] [datetime] null,
    [ApprovedByPersonAliasId] [int] null,
    [ApprovedDateTime] [datetime] null,
    [ExportedDateTime] [datetime] null,
    [CreatedDateTime] [datetime] null,
    [ModifiedDateTime] [datetime] null,
    [CreatedByPersonAliasId] [int] null,
    [ModifiedByPersonAliasId] [int] null,
    [Guid] [uniqueidentifier] not null,
    [ForeignId] [nvarchar](50) null,
    primary key ([Id])
);
create table [dbo].[_com_ccvonline_Hr_TimeCardDay] (
    [Id] [int] not null identity,
    [TimeCardId] [int] not null,
    [StartDateTime] [datetime] not null,
    [LunchStartDateTime] [datetime] null,
    [LunchEndDateTime] [datetime] null,
    [EndDateTime] [datetime] null,
    [TotalWorkedDuration] [decimal](18, 2) null,
    [PaidHolidayHours] [decimal](18, 2) null,
    [PaidVacationHours] [decimal](18, 2) null,
    [PaidSickHours] [decimal](18, 2) null,
    [Notes] [nvarchar](200) null,
    [CreatedDateTime] [datetime] null,
    [ModifiedDateTime] [datetime] null,
    [CreatedByPersonAliasId] [int] null,
    [ModifiedByPersonAliasId] [int] null,
    [Guid] [uniqueidentifier] not null,
    [ForeignId] [nvarchar](50) null,
    primary key ([Id])
);
create table [dbo].[_com_ccvonline_Hr_TimeCardHistory] (
    [Id] [int] not null identity,
    [TimeCardId] [int] not null,
    [HistoryDateTime] [datetime] not null,
    [TimeCardStatus] [int] not null,
    [StatusPersonAliasId] [int] not null,
    [Notes] [nvarchar](200) null,
    [CreatedDateTime] [datetime] null,
    [ModifiedDateTime] [datetime] null,
    [CreatedByPersonAliasId] [int] null,
    [ModifiedByPersonAliasId] [int] null,
    [Guid] [uniqueidentifier] not null,
    [ForeignId] [nvarchar](50) null,
    primary key ([Id])
);
create table [dbo].[_com_ccvonline_Hr_TimeCardPayPeriod] (
    [Id] [int] not null identity,
    [StartDate] [date] not null,
    [EndDate] [date] not null,
    [CreatedDateTime] [datetime] null,
    [ModifiedDateTime] [datetime] null,
    [CreatedByPersonAliasId] [int] null,
    [ModifiedByPersonAliasId] [int] null,
    [Guid] [uniqueidentifier] not null,
    [ForeignId] [nvarchar](50) null,
    primary key ([Id])
);
" );

            // Add Constraints
            Sql( @"
alter table [dbo].[_com_ccvonline_Hr_TimeCard] add constraint [_com_ccvonline_Hr_TimeCard_ApprovedByPersonAlias] foreign key ([ApprovedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCard] add constraint [_com_ccvonline_Hr_TimeCard_CreatedByPersonAlias] foreign key ([CreatedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCard] add constraint [_com_ccvonline_Hr_TimeCard_ModifiedByPersonAlias] foreign key ([ModifiedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCard] add constraint [_com_ccvonline_Hr_TimeCard_PersonAlias] foreign key ([PersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCard] add constraint [_com_ccvonline_Hr_TimeCard_SubmittedToPersonAlias] foreign key ([SubmittedToPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCard] add constraint [_com_ccvonline_Hr_TimeCard_TimeCardPayPeriod] foreign key ([TimeCardPayPeriodId]) references [dbo].[_com_ccvonline_Hr_TimeCardPayPeriod]([Id]) on delete cascade;
alter table [dbo].[_com_ccvonline_Hr_TimeCardDay] add constraint [_com_ccvonline_Hr_TimeCardDay_CreatedByPersonAlias] foreign key ([CreatedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCardDay] add constraint [_com_ccvonline_Hr_TimeCardDay_ModifiedByPersonAlias] foreign key ([ModifiedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCardDay] add constraint [_com_ccvonline_Hr_TimeCardDay_TimeCard] foreign key ([TimeCardId]) references [dbo].[_com_ccvonline_Hr_TimeCard]([Id]) on delete cascade;
alter table [dbo].[_com_ccvonline_Hr_TimeCardHistory] add constraint [_com_ccvonline_Hr_TimeCardHistory_CreatedByPersonAlias] foreign key ([CreatedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCardHistory] add constraint [_com_ccvonline_Hr_TimeCardHistory_ModifiedByPersonAlias] foreign key ([ModifiedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCardHistory] add constraint [_com_ccvonline_Hr_TimeCardHistory_TimeCard] foreign key ([TimeCardId]) references [dbo].[_com_ccvonline_Hr_TimeCard]([Id]) on delete cascade;
alter table [dbo].[_com_ccvonline_Hr_TimeCardPayPeriod] add constraint [_com_ccvonline_Hr_TimeCardPayPeriod_CreatedByPersonAlias] foreign key ([CreatedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
alter table [dbo].[_com_ccvonline_Hr_TimeCardPayPeriod] add constraint [_com_ccvonline_Hr_TimeCardPayPeriod_ModifiedByPersonAlias] foreign key ([ModifiedByPersonAliasId]) references [dbo].[PersonAlias]([Id]);
" );
        }

        public override void Down()
        {
            //
        }
    }
}
