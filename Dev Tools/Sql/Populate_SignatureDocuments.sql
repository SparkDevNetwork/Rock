
/* =============================================================================================
 Author:        Nick w/GPT Code Copilot
 Created:       2025-09-02
 Description:   Populate N fake [dbo].[SignatureDocument] rows AND create a corresponding
                fake PDF [dbo].[BinaryFile]/[dbo].[BinaryFileData] per document.
                - AppliesToPersonAliasId = a CHILD in the family (GroupRoleId = 4)
                - AssignedToPersonAliasId + SignedByPersonAliasId = an ADULT in the same family (GroupRoleId = 3)
                - SignatureDocumentTemplateId comes from @TemplateId or the existing templates pool
                - Each SignatureDocument gets BinaryFileId pointing to a fake PDF
                - BinaryFileType is looked up by Guid '40871411-4E2D-45C2-9E21-D9FCBA5FC340'
                - Batch is tagged via ForeignKey for easy cleanup

 Safety:
   * Set @DoCommit = 0 to preview (ROLLBACK). Set to 1 to COMMIT inserts.

 ============================================================================================= */

/* ===============================
   Configuration
   =============================== */
DECLARE @DocumentCount INT = 5000;                       -- how many docs to insert
DECLARE @TemplateId INT = NULL;                          -- force a single template id; NULL = use pool
DECLARE @MinSignatureDocumentTemplate INT = NULL;        -- optional range filter
DECLARE @MaxSignatureDocumentTemplate INT = NULL;        -- optional range filter
DECLARE @CreatedDateStart DATE = '2025-01-01';           -- random Created/Signed within this window
DECLARE @CreatedDateEnd   DATE = '2025-08-31';
DECLARE @PercentSigned TINYINT = 90;                     -- 0..100 => % that will be Status = 2 (Signed)
DECLARE @AssignAdultAsAssignee BIT = 1;                  -- also set AssignedToPersonAliasId to adult
DECLARE @DoCommit BIT = 1;                               -- 0 = ROLLBACK (preview); 1 = COMMIT

-- Constants
DECLARE @GroupTypeFamilyId INT = 10;
DECLARE @RoleAdultId INT = 3;
DECLARE @RoleChildId INT = 4;
DECLARE @StatusPending INT = 1;
DECLARE @StatusSigned INT  = 2;                          -- From the SignatureDocumentStatus enum
DECLARE @ForeignKey NVARCHAR(100) = N'Populate_SignatureDocuments'; -- useful when wanting to delete

-- BinaryFileType (Digitally Signed Documents) guid (well-known)
DECLARE @PdfBinaryFileTypeId INT = (
    SELECT TOP (1) Id FROM dbo.BinaryFileType WHERE [Guid] = '40871411-4E2D-45C2-9E21-D9FCBA5FC340'
);
DECLARE @StorageEntityTypeId INT = (
    SELECT TOP (1) Id FROM dbo.EntityType WHERE [Guid] = '0AA42802-04FD-4AEC-B011-FEB127FC85CD'
);


/* ===============================
   Preconditions
   =============================== */
IF OBJECT_ID('dbo.SignatureDocument') IS NULL
BEGIN
    RAISERROR('dbo.SignatureDocument not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('dbo.SignatureDocumentTemplate') IS NULL
BEGIN
    RAISERROR('dbo.SignatureDocumentTemplate not found.', 16, 1);
    RETURN;
END;

IF OBJECT_ID('dbo.BinaryFile') IS NULL OR OBJECT_ID('dbo.BinaryFileData') IS NULL
BEGIN
    RAISERROR('dbo.BinaryFile or dbo.BinaryFileData not found.', 16, 1);
    RETURN;
END;

IF @PdfBinaryFileTypeId IS NULL
BEGIN
    RAISERROR('BinaryFileType (Digitally Signed Documents) not found.', 16, 1 );
    RETURN;
END;

IF @StorageEntityTypeId IS NULL
BEGIN
    RAISERROR('StorageEntityTypeId (Rock.Storage.Provider.Database) not found.', 16, 1 );
    RETURN;
END;

IF (SELECT COUNT(1) FROM dbo.Person) < 50
BEGIN
    RAISERROR('Not enough people exist. Please populate the [Person] table first.', 16, 1);
    RETURN;
END;

/* ===============================
   Template Pool
   =============================== */
DECLARE @Templates TABLE (Id INT PRIMARY KEY, [Name] NVARCHAR(250) NULL);

IF @TemplateId IS NOT NULL
BEGIN
    INSERT INTO @Templates(Id, [Name])
    SELECT t.Id, t.[Name]
    FROM dbo.SignatureDocumentTemplate t
    WHERE t.Id = @TemplateId;
END
ELSE
BEGIN
    INSERT INTO @Templates(Id, [Name])
    SELECT t.Id, t.[Name]
    FROM dbo.SignatureDocumentTemplate t
    WHERE (@MinSignatureDocumentTemplate IS NULL OR t.Id >= @MinSignatureDocumentTemplate)
      AND (@MaxSignatureDocumentTemplate IS NULL OR t.Id <= @MaxSignatureDocumentTemplate);
END

IF NOT EXISTS (SELECT 1 FROM @Templates)
BEGIN
    RAISERROR('No SignatureDocumentTemplate rows available (check @TemplateId or table contents).', 16, 1);
    RETURN;
END;

/* ===============================
   Child/Adult Pairs within same Primary Family
   =============================== */
IF OBJECT_ID('tempdb..#Pairs') IS NOT NULL DROP TABLE #Pairs;

CREATE TABLE #Pairs
(
    FamilyGroupId INT NOT NULL,
    ChildPersonId INT NOT NULL,
    ChildAliasId  INT NOT NULL,
    ChildFullName NVARCHAR(200) NOT NULL,
    AdultPersonId INT NOT NULL,
    AdultAliasId  INT NOT NULL,
    AdultFullName NVARCHAR(200) NOT NULL,
    AdultEmail    NVARCHAR(250) NULL
);

;WITH Children AS (
    SELECT DISTINCT
        g.Id            AS FamilyGroupId,
        pc.Id           AS ChildPersonId,
        pac.Id          AS ChildAliasId,
        COALESCE(pc.NickName, pc.FirstName, N'') + N' ' + COALESCE(pc.LastName, N'') AS ChildFullName
    FROM dbo.[Group] g
    JOIN dbo.[Person] pc
        ON pc.PrimaryFamilyId = g.Id
    JOIN dbo.[GroupMember] gmc
        ON gmc.GroupId = g.Id AND gmc.PersonId = pc.Id AND gmc.GroupRoleId = @RoleChildId
    JOIN dbo.PersonAlias pac
        ON pac.PersonId = pc.Id AND pac.PersonId = pac.AliasPersonId
    WHERE g.GroupTypeId = @GroupTypeFamilyId
), Adults AS (
    SELECT DISTINCT
        g.Id            AS FamilyGroupId,
        pa.Id           AS AdultPersonId,
        paa.Id          AS AdultAliasId,
        COALESCE(pa.NickName, pa.FirstName, N'') + N' ' + COALESCE(pa.LastName, N'') AS AdultFullName,
        pa.Email        AS AdultEmail
    FROM dbo.[Group] g
    JOIN dbo.[GroupMember] gma
        ON gma.GroupId = g.Id AND gma.GroupRoleId = @RoleAdultId
    JOIN dbo.Person pa
        ON pa.Id = gma.PersonId
    JOIN dbo.PersonAlias paa
        ON paa.PersonId = pa.Id AND paa.PersonId = paa.AliasPersonId
    WHERE g.GroupTypeId = @GroupTypeFamilyId
)
INSERT INTO #Pairs (FamilyGroupId, ChildPersonId, ChildAliasId, ChildFullName, AdultPersonId, AdultAliasId, AdultFullName, AdultEmail)
SELECT c.FamilyGroupId,
       c.ChildPersonId,
       c.ChildAliasId,
       c.ChildFullName,
       a.AdultPersonId,
       a.AdultAliasId,
       a.AdultFullName,
       a.AdultEmail
FROM Children c
CROSS APPLY (
    SELECT TOP (1) *
    FROM Adults a
    WHERE a.FamilyGroupId = c.FamilyGroupId
    ORDER BY NEWID()
) a;

IF NOT EXISTS (SELECT 1 FROM #Pairs)
BEGIN
    RAISERROR('No eligible child/adult pairs found. Ensure families (GroupTypeId=10) have at least one child (RoleId=4) and one adult (RoleId=3).', 16, 1);
    RETURN;
END;

/* ===============================
   Candidates (Pairs x Templates) with randomized fields
   =============================== */
IF OBJECT_ID('tempdb..#Candidates') IS NOT NULL DROP TABLE #Candidates;

CREATE TABLE #Candidates
(
    CandidateId INT IDENTITY(1,1) PRIMARY KEY,
    SignatureDocumentTemplateId INT NOT NULL,
    [Name] NVARCHAR(250) NOT NULL,
    AppliesToPersonAliasId INT NULL,
    AssignedToPersonAliasId INT NULL,
    [Status] INT NOT NULL,
    LastStatusDate DATETIME NULL,
    BinaryFileId INT NULL,
    SignedByPersonAliasId INT NULL,
    CreatedDateTime DATETIME NULL,
    ModifiedDateTime DATETIME NULL,
    [Guid] UNIQUEIDENTIFIER NOT NULL,
    ForeignId INT NULL,
    ForeignGuid UNIQUEIDENTIFIER NULL,
    ForeignKey NVARCHAR(100) NULL,
    LastInviteDate DATETIME NULL,
    InviteCount INT NOT NULL,
    SignedDocumentText NVARCHAR(MAX) NULL,
    SignedName NVARCHAR(250) NULL,
    SignedClientIp NVARCHAR(128) NULL,
    SignedClientUserAgent NVARCHAR(MAX) NULL,
    SignedDateTime DATETIME NULL,
    SignedByEmail NVARCHAR(75) NULL,
    CompletionEmailSentDateTime DATETIME NULL,
    EntityTypeId INT NULL,
    EntityId INT NULL,
    SignatureVerificationHash NVARCHAR(40) NULL,
    SignatureDataEncrypted NVARCHAR(MAX) NULL
);

DECLARE @Days INT = DATEDIFF(DAY, @CreatedDateStart, @CreatedDateEnd);
IF @Days < 0 SET @Days = 0;

INSERT INTO #Candidates (
    SignatureDocumentTemplateId
    ,[Name]
    ,AppliesToPersonAliasId
    ,AssignedToPersonAliasId
    ,[Status]
    ,LastStatusDate
    ,BinaryFileId
    ,SignedByPersonAliasId
    ,CreatedDateTime
    ,ModifiedDateTime
    ,[Guid]
    ,ForeignId
    ,ForeignGuid
    ,ForeignKey
    ,LastInviteDate
    ,InviteCount
    ,SignedDocumentText
    ,SignedName
    ,SignedClientIp
    ,SignedClientUserAgent
    ,SignedDateTime
    ,SignedByEmail
    ,CompletionEmailSentDateTime
    ,EntityTypeId
    ,EntityId
    ,SignatureVerificationHash
    ,SignatureDataEncrypted
)
SELECT TOP (@DocumentCount)
    t.Id AS SignatureDocumentTemplateId,
    -- Document name: "{Child} ({TemplateName} {yyyy})"
    LEFT(
        COALESCE(p.ChildFullName, N'Child') + N' (' + COALESCE(t.[Name], CONCAT(N'Template #', CONVERT(NVARCHAR(12), t.Id))) + N' '
        + CONVERT(NVARCHAR(4), YEAR(DATEADD(DAY, ABS(CHECKSUM(NEWID())) % (CASE WHEN @Days=0 THEN 1 ELSE @Days END), @CreatedDateStart))) + N')'
    , 250) AS [Name],
    p.ChildAliasId   AS AppliesToPersonAliasId,
    CASE WHEN @AssignAdultAsAssignee = 1 THEN p.AdultAliasId ELSE NULL END AS AssignedToPersonAliasId,
    CASE WHEN (ABS(CHECKSUM(NEWID())) % 100) < @PercentSigned THEN @StatusSigned ELSE @StatusPending END AS [Status],
    NULL AS LastStatusDate,
    NULL AS BinaryFileId,
    NULL AS SignedByPersonAliasId,
    NULL AS CreatedDateTime,
    NULL AS ModifiedDateTime,
    NEWID() AS [Guid],
    NULL AS ForeignId,
    NULL AS ForeignGuid,
    @ForeignKey AS ForeignKey,
    NULL AS LastInviteDate,
    ABS(CHECKSUM(NEWID())) % 3 AS InviteCount, -- 0..2
    NULL AS SignedDocumentText,
    NULL AS SignedName,
    N'127.0.0.1' AS SignedClientIp,
    N'Populate_SignatureDocuments.sql' AS SignedClientUserAgent,
    NULL AS SignedDateTime,
    LEFT(p.AdultEmail, 75) AS SignedByEmail,
    NULL AS CompletionEmailSentDateTime,
    NULL AS EntityTypeId,
    NULL AS EntityId,
    NULL AS SignatureVerificationHash,
    NULL AS SignatureDataEncrypted
FROM #Pairs p
CROSS JOIN @Templates t
ORDER BY NEWID();

-- Post-populate PASS 1: randomized dates
UPDATE c
SET 
    c.CreatedDateTime = DATEADD(DAY, CASE WHEN @Days = 0 THEN 0 ELSE ABS(CHECKSUM(NEWID())) % (@Days + 1) END, CAST(@CreatedDateStart AS DATETIME))
  , c.SignedByPersonAliasId = CASE WHEN c.[Status] = @StatusSigned THEN p.AdultAliasId ELSE NULL END
  , c.SignedByEmail = CASE WHEN c.[Status] = @StatusSigned THEN c.SignedByEmail  ELSE NULL END
  , c.SignedClientIp = CASE WHEN c.[Status] = @StatusSigned THEN c.SignedClientIp  ELSE NULL END
  , c.SignedName = CASE WHEN c.[Status] = @StatusSigned THEN p.AdultFullName ELSE NULL END
  , c.BinaryFileId = CASE WHEN c.[Status] = @StatusSigned THEN c.BinaryFileId  ELSE NULL END
  , c.SignedDocumentText = CASE WHEN c.[Status] = @StatusSigned THEN N'Signed electronically by ' + p.AdultFullName + N' for ' + p.ChildFullName ELSE NULL END
FROM #Candidates c
JOIN #Pairs p
  ON p.ChildAliasId = c.AppliesToPersonAliasId;  -- AppliesTo is child alias

-- Post-populate PASS 2: signed fields for candidates
UPDATE c
SET 
    c.SignedDateTime  = CASE WHEN c.[Status] = @StatusSigned THEN DATEADD(MINUTE, ABS(CHECKSUM(NEWID())) % 1440, c.CreatedDateTime) ELSE NULL END
  , c.LastStatusDate  = COALESCE(c.SignedDateTime, c.CreatedDateTime)
  , c.ModifiedDateTime = COALESCE(c.SignedDateTime, c.CreatedDateTime)
  , c.LastInviteDate = DATEADD(DAY, -1, c.CreatedDateTime)
FROM #Candidates c
JOIN #Pairs p
  ON p.ChildAliasId = c.AppliesToPersonAliasId;  -- AppliesTo is child alias

--                           SELECT * FROM #Candidates
--                           RAISERROR('TESTING', 16, 1);
--                           RETURN;

/* ===============================
   Create a fake PDF BinaryFile + BinaryFileData for EACH candidate (SQL Server 2016 compatible)
   =============================== */
-- Minimal/fake PDF content
DECLARE @Pdf VARBINARY(MAX) = 0x255044462D312E340A25C7EC8FA20A342030206F626A0A3C3C2F4C696E656172697A656420312F4C20343130372F485B2031333036203133375D2F4F20362F4520313330362F4E20312F5420333938363E3E0A656E646F626A0A202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020200A787265660A342031300A30303030303030303135203030303030206E200A30303030303030353038203030303030206E200A30303030303030363231203030303030206E200A30303030303030373831203030303030206E200A30303030303031313138203030303030206E200A30303030303031313337203030303030206E200A30303030303031313738203030303030206E200A30303030303031323435203030303030206E200A30303030303031323735203030303030206E200A30303030303031333036203030303030206E200A0A747261696C65720A3C3C2F53697A652031342F496E666F2031203020522F526F6F742035203020522F49445B3C36443941323132363041453530414330444130393237453232443432333335433E3C36443941323132363041453530414330444130393237453232443432333335433E5D2F5072657620333937383E3E0A7374617274787265660D0A300A2525454F460A20202020200A352030206F626A0A3C3C2F54797065202F436174616C6F67202F50616765732032203020520A2F506167654C61796F75742F53696E676C65506167650A2F506167654D6F64652F5573654E6F6E650A2F5061676520310A2F4D657461646174612033203020520A3E3E0A656E646F626A0A362030206F626A0A3C3C2F547970652F506167652F4D65646961426F78205B30203020353935203834325D0A2F526F7461746520302F506172656E742032203020520A2F5265736F75726365733C3C2F50726F635365745B2F504446202F546578745D0A2F457874475374617465203131203020520A2F466F6E74203132203020520A3E3E0A2F436F6E74656E74732037203020520A3E3E0A656E646F626A0A372030206F626A0A3C3C2F4C656E6774682038203020522F46696C746572202F466C6174654465636F64653E3E0A73747265616D0A789CED50B14EC4300CDDF315D948906A6C374EE21501036200940D31A0D3DD8144254EB7F0F9A4ADCA45626180ADB16439F17B2FF63B5804628B632CC56630178FC9EE8F06EDDE1C4C867E3C53AFAD3783BD2C159A6D25969D4150D540538FACA24DA2503B837972C523C480C8D1BDFA0E8135680AEECD770C1445B23BFA8E805935393B2228A4A4FC0BC48BEFA14A2D4FA27D8B1C467E10555980EF27C5A66C15B733F0B3AA20F448DFD446EC63DEA252A595D92EC32864C944EEDE77A17A4694DC9527901402BB1B2F105368A61EBFDD79868C12A95DFA87367270304D46C252059ECBADA11EB22D77A65ECFE654CE57D3FFD9F4D5ECD5ECD5ECD5EC3F33FB94AE8B79A8F105A8FA8AA6656E6473747265616D0A656E646F626A0A382030206F626A0A3236370A656E646F626A0A392030206F626A0A3C3C2F547970652F4578744753746174650A2F4F504D20313E3E656E646F626A0A31302030206F626A0A3C3C2F42617365466F6E742F54696D65732D526F6D616E2F547970652F466F6E740A2F537562747970652F54797065313E3E0A656E646F626A0A31312030206F626A0A3C3C2F52370A39203020523E3E0A656E646F626A0A31322030206F626A0A3C3C2F52380A3130203020523E3E0A656E646F626A0A31332030206F626A0A3C3C2F4C656E67746820202020202020202036360A2F5320202020202020202033363E3E0A73747265616D0A000000000000026D00010000000000010000030D000100000151000100000001000100010000000000000000000000070000000700000001000000000000000100000A656E6473747265616D0A656E646F626A0A312030206F626A0A3C3C2F50726F6475636572285C3337365C3337375C303030505C303030445C303030465C303030435C303030725C303030655C303030615C303030745C3030306F5C303030725C303030205C303030325C3030302E5C303030305C3030302E5C303030325C3030302E5C30303030290A2F4372656174696F6E4461746528443A32303135303732363030323532322B313027303027290A2F4D6F644461746528443A32303135303732363030323532322B313027303027290A2F5469746C65285C3337365C3337375C303030445C3030306F5C303030635C303030755C3030306D5C303030655C3030306E5C303030745C30303031290A2F417574686F72285C3337365C3337375C303030415C303030645C3030306D5C303030695C3030306E5C303030695C303030735C303030745C303030725C303030615C303030745C3030306F5C30303072290A2F5375626A65637428290A2F4B6579776F72647328290A2F43726561746F72285C3337365C3337375C303030505C303030445C303030465C303030435C303030725C303030655C303030615C303030745C3030306F5C303030725C303030205C303030325C3030302E5C303030305C3030302E5C303030325C3030302E5C30303030293E3E656E646F626A0A322030206F626A0A3C3C202F54797065202F5061676573202F4B696473205B0A36203020520A5D202F436F756E7420310A3E3E0A656E646F626A0A332030206F626A0A3C3C2F547970652F4D657461646174610A2F537562747970652F584D4C2F4C656E67746820313536383E3E73747265616D0A3C3F787061636B657420626567696E3D27EFBBBF272069643D2757354D304D7043656869487A7265537A4E54637A6B633964273F3E0A3C3F61646F62652D7861702D66696C74657273206573633D2243524C46223F3E0A3C783A786D706D65746120786D6C6E733A783D2761646F62653A6E733A6D6574612F2720783A786D70746B3D27584D5020746F6F6C6B697420322E392E312D31332C206672616D65776F726B20312E36273E0A3C7264663A52444620786D6C6E733A7264663D27687474703A2F2F7777772E77332E6F72672F313939392F30322F32322D7264662D73796E7461782D6E73232720786D6C6E733A69583D27687474703A2F2F6E732E61646F62652E636F6D2F69582F312E302F273E0A3C7264663A4465736372697074696F6E207264663A61626F75743D27757569643A37623766663465372D333533342D313165352D303030302D3164303665366439653032322720786D6C6E733A7064663D27687474703A2F2F6E732E61646F62652E636F6D2F7064662F312E332F273E3C7064663A50726F64756365723E50444643726561746F7220322E302E322E303C2F7064663A50726F64756365723E0A3C7064663A4B6579776F7264733E3C2F7064663A4B6579776F7264733E0A3C2F7264663A4465736372697074696F6E3E0A3C7264663A4465736372697074696F6E207264663A61626F75743D27757569643A37623766663465372D333533342D313165352D303030302D3164303665366439653032322720786D6C6E733A786D703D27687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F273E3C786D703A4D6F64696679446174653E323031352D30372D32365430303A32353A32322B31303A30303C2F786D703A4D6F64696679446174653E0A3C786D703A437265617465446174653E323031352D30372D32365430303A32353A32322B31303A30303C2F786D703A437265617465446174653E0A3C786D703A43726561746F72546F6F6C3E50444643726561746F7220322E302E322E303C2F786D703A43726561746F72546F6F6C3E3C2F7264663A4465736372697074696F6E3E0A3C7264663A4465736372697074696F6E207264663A61626F75743D27757569643A37623766663465372D333533342D313165352D303030302D3164303665366439653032322720786D6C6E733A7861704D4D3D27687474703A2F2F6E732E61646F62652E636F6D2F7861702F312E302F6D6D2F27207861704D4D3A446F63756D656E7449443D27757569643A37623766663465372D333533342D313165352D303030302D316430366536643965303232272F3E0A3C7264663A4465736372697074696F6E207264663A61626F75743D27757569643A37623766663465372D333533342D313165352D303030302D3164303665366439653032322720786D6C6E733A64633D27687474703A2F2F7075726C2E6F72672F64632F656C656D656E74732F312E312F272064633A666F726D61743D276170706C69636174696F6E2F706466273E3C64633A7469746C653E3C7264663A416C743E3C7264663A6C6920786D6C3A6C616E673D27782D64656661756C74273E446F63756D656E74313C2F7264663A6C693E3C2F7264663A416C743E3C2F64633A7469746C653E3C64633A63726561746F723E3C7264663A5365713E3C7264663A6C693E41646D696E6973747261746F723C2F7264663A6C693E3C2F7264663A5365713E3C2F64633A63726561746F723E3C64633A6465736372697074696F6E3E3C7264663A416C743E3C7264663A6C6920786D6C3A6C616E673D27782D64656661756C74273E3C2F7264663A6C693E3C2F7264663A416C743E3C2F64633A6465736372697074696F6E3E3C2F7264663A4465736372697074696F6E3E0A3C2F7264663A5244463E0A3C2F783A786D706D6574613E0A2020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020200A2020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020200A3C3F787061636B657420656E643D2777273F3E0A656E6473747265616D0A656E646F626A0A202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020202020787265660A3020340A303030303030303030302036353533352066200A30303030303031343433203030303030206E200A30303030303031393231203030303030206E200A30303030303031393830203030303030206E200A747261696C65720A3C3C2F53697A6520343E3E0A7374617274787265660A3134360A2525454F460A

BEGIN TRAN;
DECLARE @Inserted INT = 0, @InsertedFiles INT = 0, @InsertedFileData INT = 0;
BEGIN TRY
    /* Insert BinaryFile rows (one per candidate). Use Candidate Guid as BinaryFile.ForeignGuid
       so we can join back without relying on OUTPUT of source aliases (2016-safe). */
    INSERT INTO dbo.BinaryFile (
          IsTemporary, IsSystem, BinaryFileTypeId, FileName, MimeType, Description,
          StorageEntityTypeId, [Guid], CreatedDateTime, ModifiedDateTime,
          CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey,
          ContentLastModified, StorageEntitySettings, [Path], ForeignGuid, ForeignId,
          FileSize, [Width], [Height], AdditionalInformation, ParentEntityTypeId, ParentEntityId
    )
    SELECT 
          0 AS IsTemporary
        , 0 AS IsSystem
        , @PdfBinaryFileTypeId AS BinaryFileTypeId
        , LEFT(REPLACE(c.[Name], '/', '-'), 240) + '.pdf' AS FileName
        , 'application/pdf' AS MimeType
        , 'Fake PDF for SignatureDocument population' AS [Description]
        , @StorageEntityTypeId AS StorageEntityTypeId
        , NEWID() AS [Guid]
        , c.CreatedDateTime
        , c.ModifiedDateTime
        , NULL, NULL
        , @ForeignKey
        , c.ModifiedDateTime
        , NULL, NULL
        , c.[Guid] AS ForeignGuid -- link to candidate/document guid
        , NULL AS ForeignId
        , DATALENGTH(@Pdf) AS FileSize
        , NULL, NULL, NULL, NULL, NULL
    FROM #Candidates c
    WHERE c.[Status] = @StatusSigned;

    SET @InsertedFiles = @@ROWCOUNT;

    -- Wire BinaryFileId back to candidates via ForeignKey + ForeignGuid
    UPDATE c
    SET c.BinaryFileId = bf.Id
    FROM #Candidates c
    JOIN dbo.BinaryFile bf
      ON bf.ForeignKey = @ForeignKey
     AND bf.ForeignGuid = c.[Guid]
    WHERE c.[Status] = @StatusSigned;

    -- Insert BinaryFileData for those files
    INSERT INTO dbo.BinaryFileData (
          Id, [Content], [Guid], CreatedDateTime, ModifiedDateTime,
          CreatedByPersonAliasId, ModifiedByPersonAliasId, ForeignKey, ForeignGuid, ForeignId
    )
    SELECT bf.Id, @Pdf, NEWID(), c.CreatedDateTime, c.ModifiedDateTime,
           NULL, NULL, @ForeignKey, c.[Guid], NULL
    FROM dbo.BinaryFile bf
    JOIN #Candidates c
      ON bf.ForeignKey = @ForeignKey
     AND bf.ForeignGuid = c.[Guid]
     WHERE c.[Status] = @StatusSigned;

    SET @InsertedFileData = @@ROWCOUNT;

    /* ===============================
       Insert SignatureDocument rows
       =============================== */
    INSERT INTO dbo.SignatureDocument (
          SignatureDocumentTemplateId
        , [Name]
        , DocumentKey
        , AppliesToPersonAliasId
        , AssignedToPersonAliasId
        , [Status]
        , LastStatusDate
        , BinaryFileId
        , SignedByPersonAliasId
        , CreatedDateTime
        , ModifiedDateTime
        , CreatedByPersonAliasId
        , ModifiedByPersonAliasId
        , [Guid]
        , ForeignId
        , ForeignGuid
        , ForeignKey
        , LastInviteDate
        , InviteCount
        , SignedDocumentText
        , SignedName
        , SignedClientIp
        , SignedClientUserAgent
        , SignedDateTime
        , SignedByEmail
        , CompletionEmailSentDateTime
        , EntityTypeId
        , EntityId
        , SignatureVerificationHash
        , SignatureDataEncrypted
    )
    SELECT 
          c.SignatureDocumentTemplateId
        , c.[Name]
        , NULL AS DocumentKey
        , c.AppliesToPersonAliasId
        , c.AssignedToPersonAliasId
        , c.[Status]
        , c.LastStatusDate
        , c.BinaryFileId
        , c.SignedByPersonAliasId
        , c.CreatedDateTime
        , c.ModifiedDateTime
        , NULL AS CreatedByPersonAliasId
        , NULL AS ModifiedByPersonAliasId
        , c.[Guid]
        , c.ForeignId
        , NULL AS ForeignGuid
        , c.ForeignKey
        , c.LastInviteDate
        , c.InviteCount
        , c.SignedDocumentText
        , c.SignedName
        , c.SignedClientIp
        , c.SignedClientUserAgent
        , c.SignedDateTime
        , c.SignedByEmail
        , c.CompletionEmailSentDateTime
        , c.EntityTypeId
        , c.EntityId
        , c.SignatureVerificationHash
        , c.SignatureDataEncrypted
    FROM #Candidates c;

    SET @Inserted = @@ROWCOUNT;

    IF (@DoCommit = 1)
    BEGIN
        COMMIT TRAN;
        PRINT CONCAT('Inserted SignatureDocument rows: ', @Inserted);
        PRINT CONCAT('Inserted BinaryFile rows: ', @InsertedFiles);
        PRINT CONCAT('Inserted BinaryFileData rows: ', @InsertedFileData);
    END
    ELSE
    BEGIN
        ROLLBACK TRAN;
        PRINT CONCAT('Preview only. Would have inserted SignatureDocument rows: ', @Inserted);
        PRINT CONCAT('Preview only. Would have inserted BinaryFile rows: ', @InsertedFiles);
        PRINT CONCAT('Preview only. Would have inserted BinaryFileData rows: ', @InsertedFileData);
    END
END TRY
BEGIN CATCH
    DECLARE @ErrMsg NVARCHAR(4000) = ERROR_MESSAGE();
    IF XACT_STATE() <> 0 ROLLBACK TRAN;
    RAISERROR('Populate_SignatureDocuments failed: %s', 16, 1, @ErrMsg);
    RETURN;
END CATCH;

/* ===============================
   Output Samples / Cleanup Hint
   =============================== */
PRINT '25 Example SignatureDocument rows:';
SELECT TOP (25)
    sd.Id, sd.SignatureDocumentTemplateId, sd.[Name], sd.AppliesToPersonAliasId, sd.AssignedToPersonAliasId, sd.[Status],
    sd.SignedByPersonAliasId, sd.CreatedDateTime, sd.SignedDateTime, sd.BinaryFileId, sd.ForeignKey
FROM dbo.SignatureDocument sd WITH (NOLOCK)
WHERE sd.ForeignKey = @ForeignKey
ORDER BY sd.Id DESC;

PRINT '25 Example BinaryFile rows:';
SELECT TOP (25)
    bf.Id, bf.FileName, bf.MimeType, bf.BinaryFileTypeId, bf.FileSize, bf.ForeignKey
FROM dbo.BinaryFile bf WITH (NOLOCK)
WHERE bf.ForeignKey = @ForeignKey
ORDER BY bf.Id DESC;

-- Cleanup for this batch only:
-- DELETE FROM dbo.SignatureDocument WHERE ForeignKey = @ForeignKey;
-- DELETE FROM dbo.BinaryFile WHERE ForeignKey = @ForeignKey; -- cascades to BinaryFileData
