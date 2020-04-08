/*
 Script to populate a Rock RMS database with sample data for the Steps module.

 This script assumes the following:
 - The Person table is populated.
 - Step Types are arranged so that prerequisite steps occur first in the sort order.
*/

-- Parameters
declare
    -- Set this flag to indicate if Step Program data should be deleted.
    -- Enabling this option will remove all existing data from the Steps tables.
    @deleteExistingData bit = 1
    -- Set this flag to indicate if Step Participant sample data should be deleted.
    -- Enabling this option will remove only those Step Participicants that were added by this script.
    ,@deleteExistingSteps bit = 1
    -- Set this flag to indicate if sample data should be added.
    ,@addSampleData bit = 1
    -- Set this value to the number of people for whom steps will be created.
    ,@maxPersonCount int = 100
    -- Set this value to the maximum number of days between consecutive step achievements.
    ,@maxDaysBetweenSteps int = 120
    -- Set this value to the percentage chance that a given person will complete their most recently attempted step.
    ,@percentChanceOfLastStepCompletion int = 70
    -- Set this value to place a tag in the ForeignKey field of the sample data records for easier identification.
    ,@stepSampleDataForeignKey nvarchar(100) = 'Steps Sample Data'

-- Local variables
declare
    @createdDateTime datetime = (select convert(date, SYSDATETIME()))
    ,@createdByPersonAliasId int = (select pa.id from PersonAlias pa inner join Person p on pa.PersonId = p.Id where p.FirstName = 'Admin' and p.LastName = 'Admin');

set nocount on

print N'Create Steps Sample Data: started.';

--
-- Create Step Programs with associated Step Types.
--
declare @adultCategoryGuid AS uniqueidentifier = '43DC43A8-420B-4012-BAA0-0A0DDF2D4A9A';
declare @youthCategoryGuid AS uniqueidentifier = 'EAB10217-F288-4B29-B56F-50BD4BA5FB08';

declare @sacramentsProgramGuid AS uniqueidentifier = '2CAFBB12-901F-4880-A3E4-848F25CAF1A6';
declare @baptismStepTypeGuid AS uniqueidentifier = '23E73F78-587A-483F-99EF-855FD6AD1B11';
declare @eucharistStepTypeGuid AS uniqueidentifier = '5EA01E79-5D17-4E87-A94F-8C4DD22131B5';
declare @annointingStepTypeGuid AS uniqueidentifier = '48141631-38F6-40C0-8470-5253208CEA9A';
declare @confessionStepTypeGuid AS uniqueidentifier = '5F754006-7F8C-4BED-94A8-FC61CEEC6B43';
declare @marriageStepTypeGuid AS uniqueidentifier = 'D03B3C65-C128-4918-A300-509B94B90175';
declare @holyOrdersStepTypeGuid AS uniqueidentifier = '0099C701-6C1E-418E-A94F-C247A2FE4BA5';
declare @confirmationStepTypeGuid AS uniqueidentifier = 'F109169F-C1F6-46ED-9091-274540E3F3E2';
declare @baptismTedGuid AS uniqueidentifier = '02BB71C9-5FE9-45B8-B230-51C7A8475B6B';
declare @marriage1TedGuid AS uniqueidentifier = '414EBE88-2CD2-40E1-893B-216DEA2CB25E';
declare @marriage2TedGuid AS uniqueidentifier = '314F5303-C803-442B-AE39-DAA7BC30CCEE';
declare @tedPersonAliasId AS INT = (SELECT pa.Id FROM PersonAlias pa JOIN Person p ON p.Id = pa.PersonId WHERE p.Guid = '8FEDC6EE-8630-41ED-9FC5-C7157FD1EAA4');
declare @sacramentsStatusSuccessGuid AS uniqueidentifier = 'A5C2A14F-9ED9-4DF4-A1C8-8ADF75E18833';
declare @sacramentsStatusDangerGuid AS uniqueidentifier = 'B591240C-4D4D-49DA-82E3-F8C1738B8EC6';
declare @prereqGuid AS uniqueidentifier = 'D96A2C67-2D76-4697-838F-3514CA11485E';

declare @alphaProgramGuid AS uniqueidentifier = 'F7C2BA07-579B-4800-BBE1-B14B73E21E12';
declare @attenderStepTypeGuid AS uniqueidentifier = '24A70E5C-9871-452E-9403-3B43C003AA87';
declare @volunteerStepTypeGuid AS uniqueidentifier = '31DB3478-840B-4541-972D-059690AD623E';
declare @leaderStepTypeGuid AS uniqueidentifier = '95DF3AC6-AFBF-4BEA-AF86-805335A1C4CB';
declare @alphaStatusStartedGuid AS uniqueidentifier = 'F29CF8A1-76A9-4436-9726-810DF0BC95C7';
declare @alphaStatusCompletedGuid AS uniqueidentifier = '7BA5F14D-BB38-4AAB-AB69-3A7F49494A55';

DECLARE @colorGreen NVARCHAR(10) = '#0f0';
DECLARE @colorRed NVARCHAR(10) = '#f00';
DECLARE @colorBlue NVARCHAR(10) = '#00f';

if ( @deleteExistingData = 1 )
begin
    print N'Removing existing programs...';

    delete from StepWorkflowTrigger
    delete from StepType
    delete from StepProgram
    delete from Category
        where EntityTypeId In ( SELECT Id FROM EntityType WHERE Name = 'Rock.Model.StepProgram' )

    delete from Step
end

if ( @deleteExistingSteps = 1 )
begin
    print N'Removing existing steps...';

    delete from Step where [ForeignKey] = @stepSampleDataForeignKey;
end

if ( @addSampleData = 1 )
begin
    print N'Creating Programs...';

    -- Add Categories
    IF NOT EXISTS (SELECT * FROM Category WHERE Guid = @adultCategoryGuid)
    BEGIN
        INSERT INTO Category (
            IsSystem,
            EntityTypeId,
            Name,
            Guid,
            [Order]
        ) VALUES (
            1,
            ( SELECT Id FROM EntityType WHERE Name = 'Rock.Model.StepProgram' ),
            'Adult',
            @adultCategoryGuid,
            1
        );
    END
    IF NOT EXISTS (SELECT * FROM Category WHERE Guid = @youthCategoryGuid)
    BEGIN
        INSERT INTO Category (
            IsSystem,
            EntityTypeId,
            Name,
            Guid,
            [Order]
        ) VALUES (
            1,
            ( SELECT Id FROM EntityType WHERE Name = 'Rock.Model.StepProgram' ),
            'Youth',
            @youthCategoryGuid,
            2
        );
    END

    -- Add Step Programs
    declare @sqlInsertStepProgram nvarchar(1000)
    set @sqlInsertStepProgram = '
    BEGIN
        IF NOT EXISTS (SELECT * FROM StepProgram WHERE Guid = @programGuid)
        BEGIN
            INSERT INTO StepProgram (
                Guid
                ,Name
                ,Description
                ,IconCssClass
                ,IsActive
                ,[Order]
                ,CategoryId
                ,DefaultListView
                ,CreatedDateTime
                ,CreatedByPersonAliasId
                ,ForeignKey
            ) VALUES (
                @programGuid
                ,@programName
                ,@description
                ,@iconCssClass
                ,@isActive
                ,@order
                ,( SELECT Id FROM Category WHERE Guid = @categoryGuid )
                ,0
                ,''' + convert(varchar, @createdDateTime, 120) + ''',' + convert(varchar, @createdByPersonAliasId) + ',''' + @stepSampleDataForeignKey + '''' + '
            );
        END
    END
    ';
    declare @sqlInsertStepProgramParams nvarchar(1000)
    set @sqlInsertStepProgramParams = N'@programGuid uniqueidentifier, @programName nvarchar(250), @description nvarchar(max), @categoryGuid uniqueidentifier, @order int, @iconCssClass nvarchar(100), @isActive bit = 1'

    execute sp_executesql @sqlInsertStepProgram, @sqlInsertStepProgramParams
        ,@programGuid = @sacramentsProgramGuid
        ,@programName = 'Sacraments'
        ,@description = 'The sacraments represent significant milestones in the Christian faith journey.'
        ,@categoryGuid = @adultCategoryGuid
        ,@order = 1
        ,@iconCssClass = 'fa fa-bible'
    execute sp_executesql @sqlInsertStepProgram, @sqlInsertStepProgramParams
        ,@programGuid = @alphaProgramGuid
        ,@programName = 'Alpha'
        ,@description = 'Alpha is a series of interactive sessions that freely explore the basics of the Christian faith.'
        ,@categoryGuid = @adultCategoryGuid
        ,@order = 1
        ,@iconCssClass = 'fa fa-question'

    -- Add Step Statuses
    IF NOT EXISTS (SELECT * FROM StepStatus WHERE Guid = @sacramentsStatusSuccessGuid)
    BEGIN
        INSERT INTO StepStatus (
            StepProgramId,
            Name,
            IsCompleteStatus,
            StatusColor,
            IsActive,
            [Order],
            Guid
        ) VALUES (
            ( SELECT Id FROM StepProgram WHERE Guid = @sacramentsProgramGuid ),
            'Completed',
            1,
            @colorGreen,
            1,
            1,
            @sacramentsStatusSuccessGuid
        );
    END

    IF NOT EXISTS (SELECT * FROM StepStatus WHERE Guid = @sacramentsStatusDangerGuid)
    BEGIN
        INSERT INTO StepStatus (
            StepProgramId,
            Name,
            IsCompleteStatus,
            StatusColor,
            IsActive,
            [Order],
            Guid
        ) VALUES (
            ( SELECT Id FROM StepProgram WHERE Guid = @sacramentsProgramGuid ),
            'Pending',
            0,
            @colorRed,
            1,
            2,
            @sacramentsStatusDangerGuid
        );
    END

    IF NOT EXISTS (SELECT * FROM StepStatus WHERE Guid = @alphaStatusStartedGuid)
    BEGIN
        INSERT INTO StepStatus (
            StepProgramId,
            Name,
            IsCompleteStatus,
            StatusColor,
            IsActive,
            [Order],
            Guid
        ) VALUES (
            ( SELECT Id FROM StepProgram WHERE Guid = @alphaProgramGuid ),
            'Started',
            0,
            @colorGreen,
            1,
            1,
            @alphaStatusStartedGuid
        );
    END
    IF NOT EXISTS (SELECT * FROM StepStatus WHERE Guid = @alphaStatusCompletedGuid)
    BEGIN
        INSERT INTO StepStatus (
            StepProgramId,
            Name,
            IsCompleteStatus,
            StatusColor,
            IsActive,
            [Order],
            Guid
        ) VALUES (
            ( SELECT Id FROM StepProgram WHERE Guid = @alphaProgramGuid ),
            'Completed',
            1,
            @colorGreen,
            1,
            1,
            @alphaStatusCompletedGuid
        );
    END

    -- Add Step Types
    declare @sqlInsertStepType nvarchar(1000)
    set @sqlInsertStepType = '
    BEGIN
        IF NOT EXISTS (SELECT * FROM StepType WHERE Guid = @stepTypeGuid)
        BEGIN
            INSERT INTO StepType (
                StepProgramId
                ,Name
                ,AllowMultiple
                ,HasEndDate
                ,IsActive
                ,[Order]
                ,Guid
                ,IconCssClass
                ,AllowManualEditing
                ,ShowCountOnBadge
                ,CreatedDateTime
                ,CreatedByPersonAliasId
                ,ForeignKey
            ) VALUES (
                ( SELECT Id FROM StepProgram WHERE Guid = @programGuid )
                ,@stepTypeName
                ,@allowMultiple
                ,@hasEndDate
                ,@isActive
                ,@order
                ,@stepTypeGuid
                ,@iconCssClass
                ,0
                ,1
                ,''' + convert(varchar, @createdDateTime, 120) + ''',' + convert(varchar, @createdByPersonAliasId) + ',''' + @stepSampleDataForeignKey + '''' + '
            );
        END
    END
    ';
    declare @sqlInsertStepTypeParams nvarchar(1000)
    set @sqlInsertStepTypeParams = N'@stepTypeGuid uniqueidentifier, @programGuid uniqueidentifier, @stepTypeName nvarchar(250), @order int, @iconCssClass nvarchar(100), @allowMultiple bit = 0, @hasEndDate bit = 0, @isActive bit = 1'

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @baptismStepTypeGuid
        ,@programGuid =  @sacramentsProgramGuid
        ,@stepTypeName = 'Baptism'
        ,@order = 1
        ,@iconCssClass = 'fa fa-tint'

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @confirmationStepTypeGuid
        ,@programGuid =  @sacramentsProgramGuid
        ,@stepTypeName = 'Confirmation'
        ,@order = 2
        ,@iconCssClass = 'fa fa-bible'

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @eucharistStepTypeGuid
        ,@programGuid =  @sacramentsProgramGuid
        ,@stepTypeName = 'Eucharist'
        ,@order = 3
        ,@iconCssClass = 'fa fa-cookie'

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @confessionStepTypeGuid
        ,@programGuid =  @sacramentsProgramGuid
        ,@stepTypeName = 'Confession'
        ,@order = 4
        ,@iconCssClass = 'fa fa-praying-hands'

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @annointingStepTypeGuid
        ,@programGuid =  @sacramentsProgramGuid
        ,@stepTypeName = 'Annointing of the Sick'
        ,@order = 5
        ,@iconCssClass = 'fa fa-comment-medical'

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @marriageStepTypeGuid
        ,@programGuid =  @sacramentsProgramGuid
        ,@stepTypeName = 'Marriage'
        ,@order = 6
        ,@iconCssClass = 'fa fa-ring'
        ,@allowMultiple = 1
        ,@hasEndDate = 1

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @holyOrdersStepTypeGuid
        ,@programGuid =  @sacramentsProgramGuid
        ,@stepTypeName = 'Holy Orders'
        ,@order = 7
        ,@iconCssClass = 'fa fa-cross'

    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @attenderStepTypeGuid
        ,@programGuid =  @alphaProgramGuid
        ,@stepTypeName = 'Attender'
        ,@order = 1
        ,@iconCssClass = 'fa fa-user'
        ,@allowMultiple = 1
        ,@hasEndDate = 1
    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @volunteerStepTypeGuid
        ,@programGuid =  @alphaProgramGuid
        ,@stepTypeName = 'Volunteer'
        ,@order = 2
        ,@iconCssClass = 'fa fa-hand-paper'
        ,@allowMultiple = 1
        ,@hasEndDate = 1
    execute sp_executesql @sqlInsertStepType, @sqlInsertStepTypeParams
        ,@stepTypeGuid = @leaderStepTypeGuid
        ,@programGuid =  @alphaProgramGuid
        ,@stepTypeName = 'Leader'
        ,@order = 3
        ,@iconCssClass = 'fa fa-user-graduate'
        ,@allowMultiple = 1
        ,@hasEndDate = 1

    -- Add data for specific test scenarios.
    IF NOT EXISTS (SELECT * FROM Step WHERE Guid = @baptismTedGuid)
    BEGIN
        INSERT INTO Step (
            StepTypeId,
            PersonAliasId,
            CompletedDateTime,
            Guid,
            [Order],
            StepStatusId
        ) VALUES (
            ( SELECT Id FROM StepType WHERE Guid = @baptismStepTypeGuid ),
            @tedPersonAliasId,
            '12/4/1996',
            @baptismTedGuid,
            1,
            NULL
        );
    END

    IF NOT EXISTS (SELECT * FROM Step WHERE Guid = @marriage1TedGuid)
    BEGIN
        INSERT INTO Step (
            StepTypeId,
            PersonAliasId,
            CompletedDateTime,
            Guid,
            [Order],
            StepStatusId
        ) VALUES (
            ( SELECT Id FROM StepType WHERE Guid = @marriageStepTypeGuid ),
            @tedPersonAliasId,
            '12/4/1980',
            @marriage1TedGuid,
            1,
            ( SELECT Id FROM StepStatus WHERE Guid = @sacramentsStatusDangerGuid )
        );
    END

    IF NOT EXISTS (SELECT * FROM Step WHERE Guid = @marriage2TedGuid)
    BEGIN
        INSERT INTO Step (
            StepTypeId,
            PersonAliasId,
            CompletedDateTime,
            Guid,
            [Order],
            StepStatusId
        ) VALUES (
            ( SELECT Id FROM StepType WHERE Guid = @marriageStepTypeGuid ),
            @tedPersonAliasId,
            '12/4/2005',
            @marriage2TedGuid,
            2,
            ( SELECT Id FROM StepStatus WHERE Guid = @sacramentsStatusSuccessGuid )
        );
    END

    IF NOT EXISTS (SELECT * FROM StepTypePrerequisite WHERE Guid = @prereqGuid)
    BEGIN
        INSERT INTO StepTypePrerequisite (
            StepTypeId,
            PrerequisiteStepTypeId,
            Guid,
            [Order]
        ) VALUES (
            ( SELECT Id FROM StepType WHERE Guid = @holyOrdersStepTypeGuid ),
            ( SELECT Id FROM StepType WHERE Guid = @confirmationStepTypeGuid ),
            @prereqGuid,
            1
        )
    END

    -- Add a randomized set of Steps.
    print N'Creating Steps...';

    declare @programIds table ( id Int )
    declare @programIdProcessList table ( id Int );
    declare @stepTypeIds table ( id Int, rowNo Int );
    declare @stepStatusIds table ( id int,stepProgramId int,isCompleteStatus bit );
    declare @personAliasIds table ( id Int not null );

    declare
         @stepTable table (
            [StepTypeId] [int] NOT NULL,
            [StepStatusId] [int] NULL,
            [PersonAliasId] [int] NULL,
            [CampusId] [int] NULL,
            [CompletedDateTime] [datetime] NULL,
            [StartDateTime] [datetime] NOT NULL,
            [EndDateTime] [datetime] NULL,
            [Note] [nvarchar](max) NULL,
            [Order] [int] NOT NULL,
            [Guid] [uniqueidentifier] NOT NULL,
            [CreatedDateTime] [datetime] NULL,
            [ModifiedDateTime] [datetime] NULL,
            [CreatedByPersonAliasId] [int] NULL,
            [ModifiedByPersonAliasId] [int] NULL,
            [ForeignKey] [nvarchar](100) NULL
        );

    begin
        -- Get a complete set of active Step Types ordered by Program and structure order.
        insert into @programIds
            select Id
            from [StepProgram] sp
            where exists (select top 1 Id from StepType where StepProgramId = sp.Id )
            order by [Order];

        insert into @stepStatusIds
            select Id
                    ,StepProgramId
                    ,IsCompleteStatus
            from [StepStatus]

        declare @maxProgramCount int;

        set @maxProgramCount = ( select count(*) from @programIds )

        if ( @maxProgramCount = 0 )
            throw 50000, 'Populate Steps data failed. There are no configured Step Programs in this database.', 1;

        -- Get a random selection of people that are not system users or specific users for which test data already exists.
        insert into @personAliasIds
            select top (@maxPersonCount)
                   pa.Id
            from PersonAlias pa
                 inner join Person p on pa.PersonId = p.Id
            where p.IsSystem = 0
                  and p.LastName NOT IN ('Anonymous')
                  and pa.Id NOT IN ( @tedPersonAliasId )
            order by newid();

    declare
        @stepCounter int = 0
        ,@personAliasId int
        ,@stepTypeId int
        ,@stepProgramId int
        ,@startDateTime datetime = SYSDATETIME()
        ,@newStepDateTime datetime
        ,@campusId int
        ,@maxStepTypeCount int
        ,@stepsToAddCount int
        ,@programsToAddCount int
        ,@nextStepTypeId int = 0
        ,@statusId int = 0
        ,@offsetDays int
        ,@personCounter int = 0
        ,@completedDateTime datetime
        ,@isCompleted bit

        -- Loop through the set of people, adding at least 1 step for each person.
        set @personAliasId =  (select top 1 Id from @personAliasIds)

        while @personAliasId is not null
        begin
            set @personCounter += 1;

            -- Randomly select the Programs that this person will participate in, from 1 to @maxProgramCount.
            set @programsToAddCount = (SELECT FLOOR(RAND()*(@maxProgramCount)+1))

            insert into @programIdProcessList
                    select top (@programsToAddCount)
                    Id
                    from @programIds
                    order by newid()

            set @stepProgramId = (select top 1 Id from @programIds)

            while ( @stepProgramId is not null)
            begin
                set @newStepDateTime = @startDateTime
                set @campusId = (select top 1 Id from Campus order by newid())

                set @maxStepTypeCount = (SELECT COUNT(Id) from StepType where StepProgramId = @stepProgramId);

                -- Randomly select the Steps that this person will achieve in the Program, from 1 to @maxStepTypeCount.
                -- This creates a distribution weighted toward achievement of earlier Steps, which is the likely scenario for most Programs.
                -- Add a row number so that the Steps can be added from last to first, to ensure that some data always exists in the current year.
                set @stepsToAddCount = (SELECT FLOOR(RAND()*(@maxStepTypeCount)+1))

                print N'Addings Steps: PersonAliasId: ' + CAST(@personAliasId AS nvarchar(10)) + ', ProgramId=' + CAST(@stepProgramId AS nvarchar(10)) + ', Steps='+ CAST(@stepsToAddCount AS nvarchar(10));

                insert into @stepTypeIds
                        select top (@stepsToAddCount)
                        Id
                        ,ROW_NUMBER() OVER(order by [Order] DESC) rowNo
                        from StepType
                        where StepProgramId = @stepProgramId order by [Order]

                set @stepTypeId = (select top 1 Id from @stepTypeIds order by rowNo)

                while ( @stepTypeId is not null)
                begin
                    if (@stepCounter > 0 and @stepCounter % 100 = 0)
                    begin
                        print N'--> (' + CAST(@stepCounter AS nvarchar(10)) + ' added)...';
                    end

                    -- Get the next Step Type to process.
                    delete from @stepTypeIds
                        where Id = @stepTypeId

                    set @nextStepTypeId = (select top 1 Id from @stepTypeIds order by [rowNo])

                    -- Set the step status. If not the last step, make sure the status represents a completion.
                    if ( @nextStepTypeId is null )
                        set @isCompleted = 1;
                    else
                        set @isCompleted = IIF((SELECT FLOOR(RAND()*100) ) <= @percentChanceOfLastStepCompletion, 1, 0 );

                    set @statusId = ( select top 1 Id from @stepStatusIds where stepProgramId = @stepProgramId and isCompleteStatus = @isCompleted order by newid() );

                    -- Subtract a random number of days from the current step date to get a suitable date for the preceding step in the program.
                    set @offsetDays = (SELECT FLOOR(RAND()*(@maxDaysBetweenSteps)+1))
                    set @newStepDateTime = DATEADD(DAY, -1 * @offsetDays, @newStepDateTime);

                    -- Set the completion date for a completed step.
                    if ( @isCompleted = 1 )
                        set @completedDateTime = @newStepDateTime
                    else
                        set @completedDateTime = null

                    print N'Add Step: PersonAliasId=' + CAST(@personAliasId AS nvarchar(10)) + ', ProgramId=' + CAST(@stepProgramId AS nvarchar(10)) + ', StepTypeId='+ CAST(@stepTypeId AS nvarchar(10)) + ', Status=' + CAST(@statusId AS nvarchar(10));

                    insert into @stepTable
                                ([StepTypeId]
                                ,[StepStatusId]
                                ,[PersonAliasId]
                                ,[CampusId]
                                ,[StartDateTime]
                                ,[CompletedDateTime]
                                ,[Order]
                                ,[Guid]
                                ,[CreatedDateTime]
                                ,[CreatedByPersonAliasId]
                                ,[ForeignKey])
                            values (@stepTypeId
                                ,@statusId
                                ,@personAliasId
                                ,@campusId
                                ,@newStepDateTime
                                ,@completedDateTime
                                ,0
                                ,NEWID()
                                ,@createdDateTime
                                ,@createdByPersonAliasId
                                ,@stepSampleDataForeignKey)

                    set @stepCounter += 1;
                    set @stepTypeId = @nextStepTypeId;
                end

                -- Get the next Program to process.
                delete from @programIdProcessList where Id = @stepProgramId
                set @stepProgramId = (select top 1 Id from @programIdProcessList)
            end

            -- Get the next person.
            delete from @personAliasIds where Id = @personAliasId
            set @personAliasId = (select top 1 Id from @personAliasIds)
        end

        print N'--> Created ' + CAST(@stepCounter AS nvarchar(10)) + ' steps for ' + CAST(@personCounter AS nvarchar(10)) + ' people.';

        insert into Step
                    ([StepTypeId]
                    ,[StepStatusId]
                    ,[PersonAliasId]
                    ,[CampusId]
                    ,[CompletedDateTime]
                    ,[StartDateTime]
                    ,[Order]
                    ,[Note]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[CreatedByPersonAliasId]
                    ,[ForeignKey])
            select  [StepTypeId]
                    ,[StepStatusId]
                    ,[PersonAliasId]
                    ,[CampusId]
                    ,[CompletedDateTime]
                    ,[StartDateTime]
                    ,[Order]
                    ,[Note]
                    ,[Guid]
                    ,[CreatedDateTime]
                    ,[CreatedByPersonAliasId]
                    ,[ForeignKey]
            from @stepTable
            order by [PersonAliasId], [CompletedDateTime]
    end

end

print N'Create Steps Sample Data: completed.';
