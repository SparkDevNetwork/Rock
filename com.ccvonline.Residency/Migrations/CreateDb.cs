using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rock.Plugin;

namespace com.ccvonline.Residency.Migrations
{
    [MigrationNumber( 1, "1.0.8" )]
    public class CreateDb : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            Sql( @"
CREATE TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] (
    [Id] [int] NOT NULL IDENTITY,
    [CompetencyPersonProjectAssessmentId] [int] NOT NULL,
    [ProjectPointOfAssessmentId] [int] NOT NULL,
    [Rating] [int],
    [RatingNotes] [nvarchar](max),
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_CompetencyPersonProjectAssessmentId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]([CompetencyPersonProjectAssessmentId])
CREATE INDEX [IX_ProjectPointOfAssessmentId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]([ProjectPointOfAssessmentId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment] (
    [Id] [int] NOT NULL IDENTITY,
    [CompetencyPersonProjectId] [int] NOT NULL,
    [AssessorPersonId] [int],
    [AssessmentDateTime] [datetime],
    [OverallRating] [decimal](2, 1),
    [RatingNotes] [nvarchar](max),
    [ResidentComments] [nvarchar](max),
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_CompetencyPersonProjectId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment]([CompetencyPersonProjectId])
CREATE INDEX [IX_AssessorPersonId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment]([AssessorPersonId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProject] (
    [Id] [int] NOT NULL IDENTITY,
    [CompetencyPersonId] [int] NOT NULL,
    [ProjectId] [int] NOT NULL,
    [MinAssessmentCount] [int],
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_CompetencyPersonProject] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_CompetencyPersonId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProject]([CompetencyPersonId])
CREATE INDEX [IX_ProjectId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProject]([ProjectId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProject]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProject]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_CompetencyPersonProject]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_CompetencyPerson] (
    [Id] [int] NOT NULL IDENTITY,
    [CompetencyId] [int] NOT NULL,
    [PersonId] [int] NOT NULL,
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_CompetencyPerson] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_CompetencyId] ON [dbo].[_com_ccvonline_Residency_CompetencyPerson]([CompetencyId])
CREATE INDEX [IX_PersonId] ON [dbo].[_com_ccvonline_Residency_CompetencyPerson]([PersonId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPerson]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_CompetencyPerson]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_CompetencyPerson]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_Competency] (
    [Id] [int] NOT NULL IDENTITY,
    [TrackId] [int] NOT NULL,
    [TeacherOfRecordPersonId] [int],
    [FacilitatorPersonId] [int],
    [Goals] [nvarchar](max),
    [CreditHours] [int],
    [SupervisionHours] [int],
    [ImplementationHours] [int],
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](max),
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_Competency] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_TrackId] ON [dbo].[_com_ccvonline_Residency_Competency]([TrackId])
CREATE INDEX [IX_TeacherOfRecordPersonId] ON [dbo].[_com_ccvonline_Residency_Competency]([TeacherOfRecordPersonId])
CREATE INDEX [IX_FacilitatorPersonId] ON [dbo].[_com_ccvonline_Residency_Competency]([FacilitatorPersonId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Competency]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Competency]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_Competency]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_Project] (
    [Id] [int] NOT NULL IDENTITY,
    [CompetencyId] [int] NOT NULL,
    [MinAssessmentCountDefault] [int],
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](max),
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_Project] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_CompetencyId] ON [dbo].[_com_ccvonline_Residency_Project]([CompetencyId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Project]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Project]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_Project]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment] (
    [Id] [int] NOT NULL IDENTITY,
    [ProjectId] [int] NOT NULL,
    [PointOfAssessmentTypeValueId] [int],
    [AssessmentOrder] [int] NOT NULL,
    [AssessmentText] [nvarchar](max) NOT NULL,
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_ProjectId] ON [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment]([ProjectId])
CREATE INDEX [IX_PointOfAssessmentTypeValueId] ON [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment]([PointOfAssessmentTypeValueId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_Track] (
    [Id] [int] NOT NULL IDENTITY,
    [PeriodId] [int] NOT NULL,
    [DisplayOrder] [int] NOT NULL,
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](max),
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_Track] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_PeriodId] ON [dbo].[_com_ccvonline_Residency_Track]([PeriodId])
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Track]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Track]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_Track]([Guid])
CREATE TABLE [dbo].[_com_ccvonline_Residency_Period] (
    [Id] [int] NOT NULL IDENTITY,
    [StartDate] [date],
    [EndDate] [date],
    [Name] [nvarchar](100) NOT NULL,
    [Description] [nvarchar](max),
    [CreatedDateTime] [datetime],
    [ModifiedDateTime] [datetime],
    [CreatedByPersonAliasId] [int],
    [ModifiedByPersonAliasId] [int],
    [Guid] [uniqueidentifier] NOT NULL,
    [ForeignId] [nvarchar](50),
    CONSTRAINT [PK_dbo._com_ccvonline_Residency_Period] PRIMARY KEY ([Id])
)
CREATE INDEX [IX_CreatedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Period]([CreatedByPersonAliasId])
CREATE INDEX [IX_ModifiedByPersonAliasId] ON [dbo].[_com_ccvonline_Residency_Period]([ModifiedByPersonAliasId])
CREATE UNIQUE INDEX [IX_Guid] ON [dbo].[_com_ccvonline_Residency_Period]([Guid])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo._com_ccvonline_Residency_CompetencyPerson] FOREIGN KEY ([CompetencyPersonProjectAssessmentId]) REFERENCES [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessmentPointOfAssessment_dbo._com_ccvonline_Residency_ProjectPointOfAs] FOREIGN KEY ([ProjectPointOfAssessmentId]) REFERENCES [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment] ([Id]) ON DELETE CASCADE
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo.Person_AssessorPersonId] FOREIGN KEY ([AssessorPersonId]) REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo._com_ccvonline_Residency_CompetencyPersonProject_Competenc] FOREIGN KEY ([CompetencyPersonProjectId]) REFERENCES [dbo].[_com_ccvonline_Residency_CompetencyPersonProject] ([Id]) ON DELETE CASCADE
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProjectAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProjectAssessment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProject] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo._com_ccvonline_Residency_CompetencyPerson_CompetencyPersonId] FOREIGN KEY ([CompetencyPersonId]) REFERENCES [dbo].[_com_ccvonline_Residency_CompetencyPerson] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProject] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProject] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPersonProject] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPersonProject_dbo._com_ccvonline_Residency_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[_com_ccvonline_Residency_Project] ([Id]) ON DELETE CASCADE
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPerson] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo._com_ccvonline_Residency_Competency_CompetencyId] FOREIGN KEY ([CompetencyId]) REFERENCES [dbo].[_com_ccvonline_Residency_Competency] ([Id]) ON DELETE CASCADE
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPerson] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPerson] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_CompetencyPerson] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_CompetencyPerson_dbo.Person_PersonId] FOREIGN KEY ([PersonId]) REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Competency] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Competency_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Competency] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Competency_dbo.Person_FacilitatorPersonId] FOREIGN KEY ([FacilitatorPersonId]) REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Competency] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Competency_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Competency] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Competency_dbo.Person_TeacherOfRecordPersonId] FOREIGN KEY ([TeacherOfRecordPersonId]) REFERENCES [dbo].[Person] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Competency] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Competency_dbo._com_ccvonline_Residency_Track_TrackId] FOREIGN KEY ([TrackId]) REFERENCES [dbo].[_com_ccvonline_Residency_Track] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Project] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Project_dbo._com_ccvonline_Residency_Competency_CompetencyId] FOREIGN KEY ([CompetencyId]) REFERENCES [dbo].[_com_ccvonline_Residency_Competency] ([Id]) ON DELETE CASCADE
ALTER TABLE [dbo].[_com_ccvonline_Residency_Project] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Project_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Project] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Project_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo.DefinedValue_PointOfAssessmentTypeValueId] FOREIGN KEY ([PointOfAssessmentTypeValueId]) REFERENCES [dbo].[DefinedValue] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_ProjectPointOfAssessment] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_ProjectPointOfAssessment_dbo._com_ccvonline_Residency_Project_ProjectId] FOREIGN KEY ([ProjectId]) REFERENCES [dbo].[_com_ccvonline_Residency_Project] ([Id]) ON DELETE CASCADE
ALTER TABLE [dbo].[_com_ccvonline_Residency_Track] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Track_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Track] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Track_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Track] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Track_dbo._com_ccvonline_Residency_Period_PeriodId] FOREIGN KEY ([PeriodId]) REFERENCES [dbo].[_com_ccvonline_Residency_Period] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Period] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Period_dbo.PersonAlias_CreatedByPersonAliasId] FOREIGN KEY ([CreatedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
ALTER TABLE [dbo].[_com_ccvonline_Residency_Period] ADD CONSTRAINT [FK_dbo._com_ccvonline_Residency_Period_dbo.PersonAlias_ModifiedByPersonAliasId] FOREIGN KEY ([ModifiedByPersonAliasId]) REFERENCES [dbo].[PersonAlias] ([Id])
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
