/* ====================================================== */
-- NewSpring Script #25: 

-- Updates family groups from LEX to be at COL
-- Adds a note to people from LEX of "Formerly LEX Campus 10/24/16"
-- Adds a foreign key of "Formerly LEX Campus 10/24/16" to families from LEX

-- VARS
-- G.GroupTypeId = 10 (family groups)
-- G.campusId = 13 (Lexington)
-- NoteTypeId = 4 (General Note)
-- CampusId = 8 (Columbia)

/* ====================================================== */

SELECT DISTINCT P.ID
INTO #LEXFamilies
FROM [Group] as G
    INNER JOIN [GroupMember] as M
        ON M.GroupId = G.Id
    INNER JOIN [Person] as P
        ON P.Id = M.PersonId
WHERE G.GroupTypeId = 10 AND G.CampusId = 13

INSERT INTO [dbo].[Note] (
    [NoteTypeId],
    [EntityId],
    [Text],
    [Guid],
    [IsPrivateNote],
    [IsSystem]
) 
SELECT
    4,
    Id,
    'Formerly LEX Campus 10/24/16',
    NEWID(),
    0,
    0
FROM #LEXFamilies

UPDATE [dbo].[Group]
SET CampusId = 8, ForeignKey = 'Formerly LEX Campus 10/24/16'
WHERE [Group].CampusId = 13 and [Group].GroupTypeId = 10
