set statistics time on
/* CREATE the table using this template and helper sql to help construct the CREATE TABLE

Template:
CREATE TABLE [dbo].[Analytics_Dim_Person](
	[PersonKey] [int] IDENTITY(1,1) NOT NULL,
	[PersonId] [int] NOT NULL,
	[HistoryHash] nvarchar(128) null,
	[FirstName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
[attribute_1stTimeCard&Book] datetime null,
[attribute_AbilityLevel] nvarchar(max) null,
[attribute_AdaptiveC] nvarchar(max) null,
[attribute_AdaptiveD] nvarchar(max) null,
[attribute_AdaptiveI] nvarchar(max) null,
[attribute_AdaptiveS] nvarchar(max) null,
[attribute_ADC] nvarchar(max) null,
[attribute_Agesofchildrenwillingtoreceive1-3-1] nvarchar(max) null,
[attribute_Agesofchildrenwillingtoreceive13-14-1] nvarchar(max) null,
[attribute_Agesofchildrenwillingtoreceive15-17-1] nvarchar(max) null,
[attribute_Agesofchildrenwillingtoreceive4-7-1] nvarchar(max) null,
[attribute_Agesofchildrenwillingtoreceive8-12-1] nvarchar(max) null,
[attribute_Agesofchildrenwillingtoreceiveunder1-1] nvarchar(max) null,
[attribute_Allergy] nvarchar(max) null,
[attribute_ApprovalLevel] nvarchar(max) null,
[attribute_Arena-13-451] nvarchar(max) null,
[attribute_AttemptDate] datetime null,
[attribute_AttendedRockSolidClass] datetime null,
[attribute_AttendsCCV] nvarchar(max) null,
[attribute_BabyDedicationDate] datetime null,
[attribute_BackgroundCheckDate] datetime null,
[attribute_BackgroundCheckDocument] nvarchar(max) null,
[attribute_BackgroundChecked] nvarchar(max) null,
[attribute_BackgroundCheckResult] nvarchar(max) null,
[attribute_BadgeIssued] nvarchar(max) null,
[attribute_BaptismDate] datetime null,
[attribute_BaptismPhoto] nvarchar(max) null,
[attribute_BaptizedHere] nvarchar(max) null,
[attribute_BehavioralChallenges] nvarchar(max) null,
[attribute_bestwaytofollowup] nvarchar(max) null,
[attribute_BibleOrdered] nvarchar(max) null,
[attribute_BibleReceived] nvarchar(max) null,
[attribute_BirthCertificate] nvarchar(max) null,
[attribute_BLSALSDocument] nvarchar(max) null,
[attribute_Bucket] nvarchar(max) null,
[attribute_BuildingLocation] nvarchar(max) null,
[attribute_C/C] nvarchar(max) null,
[attribute_CallingCampaignWorker] nvarchar(max) null,
[attribute_CampBus] nvarchar(max) null,
[attribute_CampCoach] nvarchar(max) null,
[attribute_CCVCertifiedDriver] nvarchar(max) null,
[attribute_CCVCetrifiedDriveDate] datetime null,
[attribute_CCWDocument] nvarchar(max) null,
[attribute_CCWExpirationDate] datetime null,
[attribute_ChildAbuseReport] nvarchar(max) null,
[attribute_ChildAbuseReported] nvarchar(max) null,
[attribute_CLICKDiet] nvarchar(max) null,
[attribute_CLICKImportantInfo] datetime null,
[attribute_CLICKMedicalConcerns] nvarchar(max) null,
[attribute_CLICKPermissiontotakeanynecessaryaction] nvarchar(max) null,
[attribute_CLICKPermissiontotakepicturesCCVdatabase] nvarchar(max) null,
[attribute_CLICKSeizures] nvarchar(max) null,
[attribute_CLICKTrachSuction] nvarchar(max) null,
[attribute_CoachELicense] nvarchar(max) null,
[attribute_CoachFLicense] nvarchar(max) null,
[attribute_CoachingStatus] nvarchar(max) null,
[attribute_CoachMisc.License] nvarchar(max) null,
[attribute_CoachMiscLicense] nvarchar(max) null,
[attribute_CoachMiscLicense1] nvarchar(max) null,
[attribute_CodeofConduct] nvarchar(max) null,
[attribute_com.sparkdevnetwork.DLNumber] nvarchar(max) null,
[attribute_Comments] nvarchar(max) null,
[attribute_CommitmentCardDate] datetime null,
[attribute_CommitmentLocation] nvarchar(max) null,
[attribute_CompletedFoundations] datetime null,
[attribute_ContactMade] nvarchar(max) null,
[attribute_ContactMade1] nvarchar(max) null,
[attribute_core_CurrentlyAnEra] nvarchar(max) null,
[attribute_core_EraEndDate] datetime null,
[attribute_core_EraFirstCheckin] datetime null,
[attribute_core_EraFirstGave] datetime null,
[attribute_core_EraLastCheckin] datetime null,
[attribute_core_EraLastGave] datetime null,
[attribute_core_EraStartDate] datetime null,
[attribute_core_EraTimesGiven52Wks] nvarchar(max) null,
[attribute_core_EraTimesGiven6Wks] nvarchar(max) null,
[attribute_core_TimesCheckedIn16Wks] nvarchar(max) null,
[attribute_CountryofMinistry] nvarchar(max) null,
[attribute_Covenant] nvarchar(max) null,
[attribute_CovenantDetailsVerified] nvarchar(max) null,
[attribute_CovenantNotSigned] nvarchar(max) null,
[attribute_CovenantNotTurnedIn] nvarchar(max) null,
[attribute_CPR/AEDExpirationDate] datetime null,
[attribute_CRPAEDDocument] nvarchar(max) null,
[attribute_CurrentlyanERA] nvarchar(max) null,
[attribute_CurrentMedication] nvarchar(max) null,
[attribute_DataClean-upComplete] nvarchar(max) null,
[attribute_DateAttendedStartingPoint] datetime null,
[attribute_DateBasicTrainingCompleted] datetime null,
[attribute_DateCertified] datetime null,
[attribute_DateCompletedPSMAPPtraining] datetime null,
[attribute_DateCPRTrainingcompleted] datetime null,
[attribute_DateFingerprintClearancepassed] datetime null,
[attribute_DateofLicensure/Certification] datetime null,
[attribute_DateofMembership] datetime null,
[attribute_DateOrientationCompleted] datetime null,
[attribute_DatePassedBackgroundCheck] datetime null,
[attribute_DatePassedHomeInspection] datetime null,
[attribute_DateTrainingModulescompletedonline] datetime null,
[attribute_DelayMembership] nvarchar(max) null,
[attribute_Department] nvarchar(max) null,
[attribute_DeviceAssetIdComputer] nvarchar(max) null,
[attribute_DeviceAssetIDHardPhone] nvarchar(max) null,
[attribute_DeviceAssetIDHeadset] nvarchar(max) null,
[attribute_DeviceAssetIdiPad] nvarchar(max) null,
[attribute_Devices] nvarchar(max) null,
[attribute_Divorced] nvarchar(max) null,
[attribute_DonotprocessNCOA] nvarchar(max) null,
[attribute_DoyouhaveahomeChurch] nvarchar(max) null,
[attribute_Employer] nvarchar(max) null,
[attribute_EMTDocument] nvarchar(max) null,
[attribute_EMTExpirationDate] datetime null,
[attribute_eRAFirstAttended] datetime null,
[attribute_eRALastAttended] datetime null,
[attribute_eRALastGave] datetime null,
[attribute_eRALost] datetime null,
[attribute_eRATimesAttended(16wks)] nvarchar(max) null,
[attribute_eRATimesGiven(52wks)] nvarchar(max) null,
[attribute_eRATimesGiven(6wks)] nvarchar(max) null,
[attribute_ExpressedNeeds] nvarchar(max) null,
[attribute_ExpressedNeedsComplete] nvarchar(max) null,
[attribute_Facebook] nvarchar(max) null,
[attribute_FirstActivity] datetime null,
[attribute_FirstVisit] datetime null,
[attribute_FoodHandlerCard] datetime null,
[attribute_FosterCareNotes] nvarchar(max) null,
[attribute_FosterCareNotes2] nvarchar(max) null,
[attribute_FosterCareNotes4] nvarchar(max) null,
[attribute_FosterCareNotes5] nvarchar(max) null,
[attribute_FundDistribution] nvarchar(max) null,
[attribute_GenderofchildIamwillingtoreceive] nvarchar(max) null,
[attribute_GivingInLast12Months] nvarchar(max) null,
[attribute_Global/Local] nvarchar(max) null,
[attribute_GO360OutreachTrips] nvarchar(max) null,
[attribute_GO360Updates] nvarchar(max) null,
[attribute_HireDate] datetime null,
[attribute_HomeChurch] nvarchar(max) null,
[attribute_HowJoined] nvarchar(max) null,
[attribute_Iamactiveanyreadytoacceptachild] nvarchar(max) null,
[attribute_Iaminactiveandnotacceptingchildren] nvarchar(max) null,
[attribute_Icurrenthaveaplacement] nvarchar(max) null,
[attribute_Icurrentlyhaveaplacement] nvarchar(max) null,
[attribute_InfoinSystemCorrect] nvarchar(max) null,
[attribute_Instagram] nvarchar(max) null,
[attribute_InterestedInNHGroup] nvarchar(max) null,
[attribute_InviteOnePersonaMonth] nvarchar(max) null,
[attribute_JoinaNeighborhoodGroup] nvarchar(max) null,
[attribute_Kids&StudentsMinistry] nvarchar(max) null,
[attribute_LanguagesSpoken] nvarchar(max) null,
[attribute_Languagesspokeninthehome] nvarchar(max) null,
[attribute_LastDiscRequestDate] datetime null,
[attribute_LastSaveDate] datetime null,
[attribute_LegalNotes] nvarchar(max) null,
[attribute_LocationBasicTrainingCompleted] nvarchar(max) null,
[attribute_LocationOrientationCompleted] nvarchar(max) null,
[attribute_MedicalCerExpirationDate] datetime null,
[attribute_MedicalOtherDocument] nvarchar(max) null,
[attribute_MedicalSituation] nvarchar(max) null,
[attribute_MembershipDate] datetime null,
[attribute_MinistryPartnerFormSigned] datetime null,
[attribute_MissionAgencyName] nvarchar(max) null,
[attribute_MVDDriver'sClearanceDate] datetime null,
[attribute_MyChildenjoystheseactivities] nvarchar(max) null,
[attribute_MyFamilyCoach] nvarchar(max) null,
[attribute_MyLocalNeighborhoodSchool] nvarchar(max) null,
[attribute_MyLocalNeighborhoodSchool2] nvarchar(max) null,
[attribute_Mysupportteamiscomplete] nvarchar(max) null,
[attribute_NameofBiologicalFamily] nvarchar(max) null,
[attribute_NameTagIssued] datetime null,
[attribute_NaturalC] nvarchar(max) null,
[attribute_NaturalD] nvarchar(max) null,
[attribute_NaturalI] nvarchar(max) null,
[attribute_NaturalS] nvarchar(max) null,
[attribute_NeedstoMeetAgeRequirement] nvarchar(max) null,
[attribute_NeighborhoodMinistry] nvarchar(max) null,
[attribute_NewInfoforSystem] nvarchar(max) null,
[attribute_NotAttendingCCVOtherReason] nvarchar(max) null,
[attribute_NumberofchildrenIamwillingtohost] nvarchar(max) null,
[attribute_NursingLic] nvarchar(max) null,
[attribute_OldCovenant] datetime null,
[attribute_OtherCertificationDate] datetime null,
[attribute_OtherExpressedNeed] nvarchar(max) null,
[attribute_PayrollDepartment] nvarchar(max) null,
[attribute_PayrollEmployeeID] nvarchar(max) null,
[attribute_PCO_ID] nvarchar(max) null,
[attribute_PCO_Password] nvarchar(max) null,
[attribute_PDIDDocument] nvarchar(max) null,
[attribute_PendindStartingPoint] nvarchar(max) null,
[attribute_PendingBaptism] nvarchar(max) null,
[attribute_PenPalAssigned] nvarchar(max) null,
[attribute_PeopleKeysDISC] nvarchar(max) null,
[attribute_PersonalityType] nvarchar(max) null,
[attribute_PhotoProcessed] nvarchar(max) null,
[attribute_PlacementEndDate] datetime null,
[attribute_PlacementStartDate] datetime null,
[attribute_Position] nvarchar(max) null,
[attribute_PrayerConfidentiallySigned] datetime null,
[attribute_PreferredT-ShirtSize] nvarchar(max) null,
[attribute_PreviousChurch] nvarchar(max) null,
[attribute_PreviousMemberStatus] nvarchar(max) null,
[attribute_Prison] nvarchar(max) null,
[attribute_PrisonMinistryNotes] nvarchar(max) null,
[attribute_ProjectType] nvarchar(max) null,
[attribute_PublicEmail] nvarchar(max) null,
[attribute_PublicPhoto] nvarchar(max) null,
[attribute_ReadthroughtheBible] nvarchar(max) null,
[attribute_RegionofMinistry] nvarchar(max) null,
[attribute_RelationshipWithChrist] nvarchar(max) null,
[attribute_ReleaseDate] datetime null,
[attribute_ReleaseFormForTalent] datetime null,
[attribute_ResidencyProgram] nvarchar(max) null,
[attribute_Safety&Security] nvarchar(max) null,
[attribute_School] nvarchar(max) null,
[attribute_SecondVisit] datetime null,
[attribute_SecurityClearanceDate] datetime null,
[attribute_SecurityClearanceProcess] nvarchar(max) null,
[attribute_SecurityOtherDocument] nvarchar(max) null,
[attribute_SentBaptismPhotoEmail] nvarchar(max) null,
[attribute_SentPostBaptismEmail] nvarchar(max) null,
[attribute_ServeOneHouraWeek] nvarchar(max) null,
[attribute_SignedCovenantDate] datetime null,
[attribute_Skype] nvarchar(max) null,
[attribute_SO] nvarchar(max) null,
[attribute_SocialMediaRelease] nvarchar(max) null,
[attribute_SourceofVisit] nvarchar(max) null,
[attribute_SpecialNote] nvarchar(max) null,
[attribute_SpecialPoints] nvarchar(max) null,
[attribute_SpecialSituation] nvarchar(max) null,
[attribute_StaffBGCheckStatus] nvarchar(max) null,
[attribute_StaffBGPassedDate] datetime null,
[attribute_StaffBGSubmissionDate] datetime null,
[attribute_STARS] nvarchar(max) null,
[attribute_StartDate] datetime null,
[attribute_Status] nvarchar(max) null,
[attribute_Strength1] nvarchar(max) null,
[attribute_Strength2] nvarchar(max) null,
[attribute_Strength3] nvarchar(max) null,
[attribute_Strength4] nvarchar(max) null,
[attribute_Strength5] nvarchar(max) null,
[attribute_SupportAmountForCurrentYear] nvarchar(max) null,
[attribute_Team] nvarchar(max) null,
[attribute_ThirdVisit] datetime null,
[attribute_TimeCommitmentagreedupon] nvarchar(max) null,
[attribute_TrustGodbyTithing] nvarchar(max) null,
[attribute_Twitter] nvarchar(max) null,
[attribute_UniformSize] nvarchar(max) null,
[attribute_Unit] nvarchar(max) null,
[attribute_VolunteerApplication] datetime null,
[attribute_VolunteerOpportunitiesinGlobalOutreach] nvarchar(max) null,
[attribute_Waiver(s)] nvarchar(max) null,
[attribute_Website] nvarchar(max) null,
[attribute_WhichAgencyhaveyouchosen] nvarchar(max) null,
[attribute_Widowed] nvarchar(max) null,
[attribute_WorkingCampus] nvarchar(max) null,
[attribute_YouthNameTagIssued] datetime null,
[attribute_YouthPCO_ID] nvarchar(max) null,
[attribute_YouthPCO_Password] nvarchar(max) null,
[attribute_YouthVolunteerApplication] datetime null,
	CONSTRAINT [PK_dbo.Analytics_Dim_Person] PRIMARY KEY CLUSTERED 
	(
		[PersonKey] ASC
	)
	)

-- *helper sql
SELECT CONCAT (
        '[attribute_'
        ,a.[Key]
        ,'] '
        ,CASE a.FieldTypeId
            WHEN 11
                THEN 'datetime null'
            ELSE 'nvarchar(max) null'
            END
        ,','
        )
FROM Attribute a
WHERE a.EntityTypeId = 15
ORDER BY a.[Key]
*/

/* ##Initial Populate##
*/
--use these helpers
-- FROM clauses (NOTE: TOP 1 is faster than MAX() in this case)
SELECT CONCAT (
        'OUTER APPLY ( SELECT TOP 1 '
        ,CASE 
            WHEN [ft].[Class] IN (
                    'Rock.Field.Types.DateFieldType'
                    ,'Rock.Field.Types.DateTimeFieldType'
                    )
                THEN 'av.ValueAsDateTime'
            ELSE 'av.Value'
            END
        ,' FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = '
        ,a.[Id]
        ,') attribute_'
        ,a.[Id]
        )
FROM Attribute a
JOIN FieldType ft ON a.FieldTypeId = ft.Id
WHERE a.EntityTypeId = 15
order by a.[Key]


-- SELECT fields for attributes
SELECT CONCAT (
        ',attribute_'
        ,a.Id
        ,CASE 
            WHEN [ft].[Class] IN (
                    'Rock.Field.Types.DateFieldType'
                    ,'Rock.Field.Types.DateTimeFieldType'
                    )
                THEN '.ValueAsDateTime'
            ELSE '.Value'
            END
        ,' as [attribute_'
        ,a.[Key]
        ,']'
        )
FROM Attribute a
JOIN FieldType ft ON a.FieldTypeId = ft.Id
WHERE a.EntityTypeId = 15
ORDER BY a.[Key]

/*
Example:
insert into Analytics_Dim_Person 
SELECT
	p.Id AS [PersonId]
	,null [HistoryHash]
    ,p.FirstName
	,p.LastName
,attribute_1167.ValueAsDateTime as [attribute_1stTimeCard&Book]
,attribute_553.Value as [attribute_AbilityLevel]
,attribute_1947.Value as [attribute_AdaptiveC]
,attribute_1944.Value as [attribute_AdaptiveD]
,attribute_1945.Value as [attribute_AdaptiveI]
,attribute_1946.Value as [attribute_AdaptiveS]
,attribute_1140.Value as [attribute_ADC]
,attribute_1278.Value as [attribute_Agesofchildrenwillingtoreceive1-3-1]
,attribute_1281.Value as [attribute_Agesofchildrenwillingtoreceive13-14-1]
,attribute_1282.Value as [attribute_Agesofchildrenwillingtoreceive15-17-1]
,attribute_1279.Value as [attribute_Agesofchildrenwillingtoreceive4-7-1]
,attribute_1280.Value as [attribute_Agesofchildrenwillingtoreceive8-12-1]
,attribute_1277.Value as [attribute_Agesofchildrenwillingtoreceiveunder1-1]
,attribute_676.Value as [attribute_Allergy]
,attribute_1188.Value as [attribute_ApprovalLevel]
,attribute_5704.Value as [attribute_Arena-13-451]
,attribute_15223.ValueAsDateTime as [attribute_AttemptDate]
,attribute_1168.ValueAsDateTime as [attribute_AttendedRockSolidClass]
,attribute_15225.Value as [attribute_AttendsCCV]
,attribute_1331.ValueAsDateTime as [attribute_BabyDedicationDate]
,attribute_2072.ValueAsDateTime as [attribute_BackgroundCheckDate]
,attribute_2074.Value as [attribute_BackgroundCheckDocument]
,attribute_2071.Value as [attribute_BackgroundChecked]
,attribute_2073.Value as [attribute_BackgroundCheckResult]
,attribute_1160.Value as [attribute_BadgeIssued]
,attribute_174.ValueAsDateTime as [attribute_BaptismDate]
,attribute_2627.Value as [attribute_BaptismPhoto]
,attribute_714.Value as [attribute_BaptizedHere]
,attribute_1169.Value as [attribute_BehavioralChallenges]
,attribute_30404.Value as [attribute_bestwaytofollowup]
,attribute_1143.Value as [attribute_BibleOrdered]
,attribute_1144.Value as [attribute_BibleReceived]
,attribute_1052.Value as [attribute_BirthCertificate]
,attribute_2718.Value as [attribute_BLSALSDocument]
,attribute_1318.Value as [attribute_Bucket]
,attribute_1156.Value as [attribute_BuildingLocation]
,attribute_1145.Value as [attribute_C/C]
,attribute_15236.Value as [attribute_CallingCampaignWorker]
,attribute_16679.Value as [attribute_CampBus]
,attribute_16678.Value as [attribute_CampCoach]
,attribute_1313.Value as [attribute_CCVCertifiedDriver]
,attribute_1314.ValueAsDateTime as [attribute_CCVCetrifiedDriveDate]
,attribute_2719.Value as [attribute_CCWDocument]
,attribute_1105.ValueAsDateTime as [attribute_CCWExpirationDate]
,attribute_31203.Value as [attribute_ChildAbuseReport]
,attribute_28768.Value as [attribute_ChildAbuseReported]
,attribute_1173.Value as [attribute_CLICKDiet]
,attribute_1176.ValueAsDateTime as [attribute_CLICKImportantInfo]
,attribute_1170.Value as [attribute_CLICKMedicalConcerns]
,attribute_1175.Value as [attribute_CLICKPermissiontotakeanynecessaryaction]
,attribute_1174.Value as [attribute_CLICKPermissiontotakepicturesCCVdatabase]
,attribute_1171.Value as [attribute_CLICKSeizures]
,attribute_1172.Value as [attribute_CLICKTrachSuction]
,attribute_15151.Value as [attribute_CoachELicense]
,attribute_15385.Value as [attribute_CoachFLicense]
,attribute_15222.Value as [attribute_CoachingStatus]
,attribute_15386.Value as [attribute_CoachMisc.License]
,attribute_17085.Value as [attribute_CoachMiscLicense]
,attribute_17086.Value as [attribute_CoachMiscLicense1]
,attribute_1053.Value as [attribute_CodeofConduct]
,attribute_12496.Value as [attribute_com.sparkdevnetwork.DLNumber]
,attribute_1323.Value as [attribute_Comments]
,attribute_1062.ValueAsDateTime as [attribute_CommitmentCardDate]
,attribute_1201.Value as [attribute_CommitmentLocation]
,attribute_1101.ValueAsDateTime as [attribute_CompletedFoundations]
,attribute_15224.Value as [attribute_ContactMade]
,attribute_15230.Value as [attribute_ContactMade1]
,attribute_16728.Value as [attribute_core_CurrentlyAnEra]
,attribute_16730.ValueAsDateTime as [attribute_core_EraEndDate]
,attribute_16731.ValueAsDateTime as [attribute_core_EraFirstCheckin]
,attribute_16734.ValueAsDateTime as [attribute_core_EraFirstGave]
,attribute_16732.ValueAsDateTime as [attribute_core_EraLastCheckin]
,attribute_16733.ValueAsDateTime as [attribute_core_EraLastGave]
,attribute_16729.ValueAsDateTime as [attribute_core_EraStartDate]
,attribute_16736.Value as [attribute_core_EraTimesGiven52Wks]
,attribute_16737.Value as [attribute_core_EraTimesGiven6Wks]
,attribute_16735.Value as [attribute_core_TimesCheckedIn16Wks]
,attribute_1320.Value as [attribute_CountryofMinistry]
,attribute_2717.Value as [attribute_Covenant]
,attribute_1130.Value as [attribute_CovenantDetailsVerified]
,attribute_1166.Value as [attribute_CovenantNotSigned]
,attribute_1162.Value as [attribute_CovenantNotTurnedIn]
,attribute_1103.ValueAsDateTime as [attribute_CPR/AEDExpirationDate]
,attribute_2720.Value as [attribute_CRPAEDDocument]
,attribute_2533.Value as [attribute_CurrentlyanERA]
,attribute_1177.Value as [attribute_CurrentMedication]
,attribute_15801.Value as [attribute_DataClean-upComplete]
,attribute_1309.ValueAsDateTime as [attribute_DateAttendedStartingPoint]
,attribute_1220.ValueAsDateTime as [attribute_DateBasicTrainingCompleted]
,attribute_1271.ValueAsDateTime as [attribute_DateCertified]
,attribute_1225.ValueAsDateTime as [attribute_DateCompletedPSMAPPtraining]
,attribute_1226.ValueAsDateTime as [attribute_DateCPRTrainingcompleted]
,attribute_1232.ValueAsDateTime as [attribute_DateFingerprintClearancepassed]
,attribute_1245.ValueAsDateTime as [attribute_DateofLicensure/Certification]
,attribute_1031.ValueAsDateTime as [attribute_DateofMembership]
,attribute_1218.ValueAsDateTime as [attribute_DateOrientationCompleted]
,attribute_1228.ValueAsDateTime as [attribute_DatePassedBackgroundCheck]
,attribute_1244.ValueAsDateTime as [attribute_DatePassedHomeInspection]
,attribute_1270.ValueAsDateTime as [attribute_DateTrainingModulescompletedonline]
,attribute_1070.Value as [attribute_DelayMembership]
,attribute_1058.Value as [attribute_Department]
,attribute_23834.Value as [attribute_DeviceAssetIdComputer]
,attribute_24376.Value as [attribute_DeviceAssetIDHardPhone]
,attribute_24375.Value as [attribute_DeviceAssetIDHeadset]
,attribute_24374.Value as [attribute_DeviceAssetIdiPad]
,attribute_23833.Value as [attribute_Devices]
,attribute_1137.Value as [attribute_Divorced]
,attribute_1069.Value as [attribute_DonotprocessNCOA]
,attribute_1333.Value as [attribute_DoyouhaveahomeChurch]
,attribute_740.Value as [attribute_Employer]
,attribute_2721.Value as [attribute_EMTDocument]
,attribute_1102.ValueAsDateTime as [attribute_EMTExpirationDate]
,attribute_2535.ValueAsDateTime as [attribute_eRAFirstAttended]
,attribute_2536.ValueAsDateTime as [attribute_eRALastAttended]
,attribute_2539.ValueAsDateTime as [attribute_eRALastGave]
,attribute_2540.ValueAsDateTime as [attribute_eRALost]
,attribute_2534.Value as [attribute_eRATimesAttended(16wks)]
,attribute_2538.Value as [attribute_eRATimesGiven(52wks)]
,attribute_2537.Value as [attribute_eRATimesGiven(6wks)]
,attribute_15234.Value as [attribute_ExpressedNeeds]
,attribute_15818.Value as [attribute_ExpressedNeedsComplete]
,attribute_1379.Value as [attribute_Facebook]
,attribute_1056.ValueAsDateTime as [attribute_FirstActivity]
,attribute_717.ValueAsDateTime as [attribute_FirstVisit]
,attribute_1033.ValueAsDateTime as [attribute_FoodHandlerCard]
,attribute_1231.Value as [attribute_FosterCareNotes]
,attribute_1251.Value as [attribute_FosterCareNotes2]
,attribute_1272.Value as [attribute_FosterCareNotes4]
,attribute_1289.Value as [attribute_FosterCareNotes5]
,attribute_1324.Value as [attribute_FundDistribution]
,attribute_1276.Value as [attribute_GenderofchildIamwillingtoreceive]
,attribute_15033.Value as [attribute_GivingInLast12Months]
,attribute_1317.Value as [attribute_Global/Local]
,attribute_1151.Value as [attribute_GO360OutreachTrips]
,attribute_1159.Value as [attribute_GO360Updates]
,attribute_1061.ValueAsDateTime as [attribute_HireDate]
,attribute_1117.Value as [attribute_HomeChurch]
,attribute_1032.Value as [attribute_HowJoined]
,attribute_1273.Value as [attribute_Iamactiveanyreadytoacceptachild]
,attribute_1274.Value as [attribute_Iaminactiveandnotacceptingchildren]
,attribute_1249.Value as [attribute_Icurrenthaveaplacement]
,attribute_1283.Value as [attribute_Icurrentlyhaveaplacement]
,attribute_15232.Value as [attribute_InfoinSystemCorrect]
,attribute_1381.Value as [attribute_Instagram]
,attribute_1080.Value as [attribute_InterestedInNHGroup]
,attribute_1125.Value as [attribute_InviteOnePersonaMonth]
,attribute_1126.Value as [attribute_JoinaNeighborhoodGroup]
,attribute_1210.Value as [attribute_Kids&StudentsMinistry]
,attribute_9991.Value as [attribute_LanguagesSpoken]
,attribute_1212.Value as [attribute_Languagesspokeninthehome]
,attribute_16264.ValueAsDateTime as [attribute_LastDiscRequestDate]
,attribute_1952.ValueAsDateTime as [attribute_LastSaveDate]
,attribute_715.Value as [attribute_LegalNotes]
,attribute_1219.Value as [attribute_LocationBasicTrainingCompleted]
,attribute_1217.Value as [attribute_LocationOrientationCompleted]
,attribute_1104.ValueAsDateTime as [attribute_MedicalCerExpirationDate]
,attribute_2722.Value as [attribute_MedicalOtherDocument]
,attribute_1055.Value as [attribute_MedicalSituation]
,attribute_906.ValueAsDateTime as [attribute_MembershipDate]
,attribute_1066.ValueAsDateTime as [attribute_MinistryPartnerFormSigned]
,attribute_1315.Value as [attribute_MissionAgencyName]
,attribute_1158.ValueAsDateTime as [attribute_MVDDriver'sClearanceDate]
,attribute_1178.Value as [attribute_MyChildenjoystheseactivities]
,attribute_1261.Value as [attribute_MyFamilyCoach]
,attribute_1214.Value as [attribute_MyLocalNeighborhoodSchool]
,attribute_1259.Value as [attribute_MyLocalNeighborhoodSchool2]
,attribute_1233.Value as [attribute_Mysupportteamiscomplete]
,attribute_1285.Value as [attribute_NameofBiologicalFamily]
,attribute_1022.ValueAsDateTime as [attribute_NameTagIssued]
,attribute_1951.Value as [attribute_NaturalC]
,attribute_1948.Value as [attribute_NaturalD]
,attribute_1949.Value as [attribute_NaturalI]
,attribute_1950.Value as [attribute_NaturalS]
,attribute_1184.Value as [attribute_NeedstoMeetAgeRequirement]
,attribute_1209.Value as [attribute_NeighborhoodMinistry]
,attribute_15233.Value as [attribute_NewInfoforSystem]
,attribute_15231.Value as [attribute_NotAttendingCCVOtherReason]
,attribute_1275.Value as [attribute_NumberofchildrenIamwillingtohost]
,attribute_2723.Value as [attribute_NursingLic]
,attribute_1035.ValueAsDateTime as [attribute_OldCovenant]
,attribute_1106.ValueAsDateTime as [attribute_OtherCertificationDate]
,attribute_15235.Value as [attribute_OtherExpressedNeed]
,attribute_1310.Value as [attribute_PayrollDepartment]
,attribute_1208.Value as [attribute_PayrollEmployeeID]
,attribute_1120.Value as [attribute_PCO_ID]
,attribute_1121.Value as [attribute_PCO_Password]
,attribute_2724.Value as [attribute_PDIDDocument]
,attribute_1164.Value as [attribute_PendindStartingPoint]
,attribute_1163.Value as [attribute_PendingBaptism]
,attribute_1147.Value as [attribute_PenPalAssigned]
,attribute_29563.Value as [attribute_PeopleKeysDISC]
,attribute_2125.Value as [attribute_PersonalityType]
,attribute_17434.Value as [attribute_PhotoProcessed]
,attribute_1287.ValueAsDateTime as [attribute_PlacementEndDate]
,attribute_1284.ValueAsDateTime as [attribute_PlacementStartDate]
,attribute_741.Value as [attribute_Position]
,attribute_1129.ValueAsDateTime as [attribute_PrayerConfidentiallySigned]
,attribute_1081.Value as [attribute_PreferredT-ShirtSize]
,attribute_716.Value as [attribute_PreviousChurch]
,attribute_1063.Value as [attribute_PreviousMemberStatus]
,attribute_1142.Value as [attribute_Prison]
,attribute_1146.Value as [attribute_PrisonMinistryNotes]
,attribute_1321.Value as [attribute_ProjectType]
,attribute_15194.Value as [attribute_PublicEmail]
,attribute_15192.Value as [attribute_PublicPhoto]
,attribute_1124.Value as [attribute_ReadthroughtheBible]
,attribute_1319.Value as [attribute_RegionofMinistry]
,attribute_1116.Value as [attribute_RelationshipWithChrist]
,attribute_1141.ValueAsDateTime as [attribute_ReleaseDate]
,attribute_1150.ValueAsDateTime as [attribute_ReleaseFormForTalent]
,attribute_1498.Value as [attribute_ResidencyProgram]
,attribute_1332.Value as [attribute_Safety&Security]
,attribute_739.Value as [attribute_School]
,attribute_718.ValueAsDateTime as [attribute_SecondVisit]
,attribute_1186.ValueAsDateTime as [attribute_SecurityClearanceDate]
,attribute_1185.Value as [attribute_SecurityClearanceProcess]
,attribute_2725.Value as [attribute_SecurityOtherDocument]
,attribute_2519.Value as [attribute_SentBaptismPhotoEmail]
,attribute_2517.Value as [attribute_SentPostBaptismEmail]
,attribute_1128.Value as [attribute_ServeOneHouraWeek]
,attribute_1064.ValueAsDateTime as [attribute_SignedCovenantDate]
,attribute_1077.Value as [attribute_Skype]
,attribute_1118.Value as [attribute_SO]
,attribute_11356.Value as [attribute_SocialMediaRelease]
,attribute_719.Value as [attribute_SourceofVisit]
,attribute_1057.Value as [attribute_SpecialNote]
,attribute_1059.Value as [attribute_SpecialPoints]
,attribute_1068.Value as [attribute_SpecialSituation]
,attribute_1155.Value as [attribute_StaffBGCheckStatus]
,attribute_1154.ValueAsDateTime as [attribute_StaffBGPassedDate]
,attribute_1153.ValueAsDateTime as [attribute_StaffBGSubmissionDate]
,attribute_1187.Value as [attribute_STARS]
,attribute_1060.ValueAsDateTime as [attribute_StartDate]
,attribute_1072.Value as [attribute_Status]
,attribute_1190.Value as [attribute_Strength1]
,attribute_1191.Value as [attribute_Strength2]
,attribute_1192.Value as [attribute_Strength3]
,attribute_1193.Value as [attribute_Strength4]
,attribute_1194.Value as [attribute_Strength5]
,attribute_1325.Value as [attribute_SupportAmountForCurrentYear]
,attribute_2003.Value as [attribute_Team]
,attribute_1034.ValueAsDateTime as [attribute_ThirdVisit]
,attribute_1286.Value as [attribute_TimeCommitmentagreedupon]
,attribute_1127.Value as [attribute_TrustGodbyTithing]
,attribute_1380.Value as [attribute_Twitter]
,attribute_1051.Value as [attribute_UniformSize]
,attribute_2002.Value as [attribute_Unit]
,attribute_1020.ValueAsDateTime as [attribute_VolunteerApplication]
,attribute_1152.Value as [attribute_VolunteerOpportunitiesinGlobalOutreach]
,attribute_1136.Value as [attribute_Waiver(s)]
,attribute_1078.Value as [attribute_Website]
,attribute_1221.Value as [attribute_WhichAgencyhaveyouchosen]
,attribute_1119.Value as [attribute_Widowed]
,attribute_5385.Value as [attribute_WorkingCampus]
,attribute_1073.ValueAsDateTime as [attribute_YouthNameTagIssued]
,attribute_1311.Value as [attribute_YouthPCO_ID]
,attribute_1312.Value as [attribute_YouthPCO_Password]
,attribute_1071.ValueAsDateTime as [attribute_YouthVolunteerApplication]
FROM dbo.Person p
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1167) attribute_1167
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 553) attribute_553
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1947) attribute_1947
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1944) attribute_1944
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1945) attribute_1945
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1946) attribute_1946
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1140) attribute_1140
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1278) attribute_1278
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1281) attribute_1281
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1282) attribute_1282
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1279) attribute_1279
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1280) attribute_1280
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1277) attribute_1277
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 676) attribute_676
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1188) attribute_1188
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 5704) attribute_5704
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15223) attribute_15223
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1168) attribute_1168
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15225) attribute_15225
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1331) attribute_1331
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2072) attribute_2072
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2074) attribute_2074
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2071) attribute_2071
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2073) attribute_2073
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1160) attribute_1160
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 174) attribute_174
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2627) attribute_2627
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 714) attribute_714
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1169) attribute_1169
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 30404) attribute_30404
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1143) attribute_1143
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1144) attribute_1144
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1052) attribute_1052
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2718) attribute_2718
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1318) attribute_1318
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1156) attribute_1156
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1145) attribute_1145
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15236) attribute_15236
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16679) attribute_16679
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16678) attribute_16678
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1313) attribute_1313
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1314) attribute_1314
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2719) attribute_2719
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1105) attribute_1105
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 31203) attribute_31203
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 28768) attribute_28768
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1173) attribute_1173
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1176) attribute_1176
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1170) attribute_1170
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1175) attribute_1175
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1174) attribute_1174
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1171) attribute_1171
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1172) attribute_1172
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15151) attribute_15151
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15385) attribute_15385
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15222) attribute_15222
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15386) attribute_15386
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 17085) attribute_17085
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 17086) attribute_17086
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1053) attribute_1053
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 12496) attribute_12496
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1323) attribute_1323
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1062) attribute_1062
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1201) attribute_1201
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1101) attribute_1101
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15224) attribute_15224
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15230) attribute_15230
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16728) attribute_16728
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16730) attribute_16730
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16731) attribute_16731
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16734) attribute_16734
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16732) attribute_16732
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16733) attribute_16733
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16729) attribute_16729
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16736) attribute_16736
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16737) attribute_16737
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16735) attribute_16735
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1320) attribute_1320
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2717) attribute_2717
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1130) attribute_1130
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1166) attribute_1166
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1162) attribute_1162
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1103) attribute_1103
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2720) attribute_2720
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2533) attribute_2533
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1177) attribute_1177
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15801) attribute_15801
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1309) attribute_1309
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1220) attribute_1220
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1271) attribute_1271
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1225) attribute_1225
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1226) attribute_1226
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1232) attribute_1232
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1245) attribute_1245
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1031) attribute_1031
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1218) attribute_1218
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1228) attribute_1228
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1244) attribute_1244
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1270) attribute_1270
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1070) attribute_1070
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1058) attribute_1058
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 23834) attribute_23834
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 24376) attribute_24376
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 24375) attribute_24375
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 24374) attribute_24374
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 23833) attribute_23833
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1137) attribute_1137
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1069) attribute_1069
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1333) attribute_1333
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 740) attribute_740
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2721) attribute_2721
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1102) attribute_1102
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2535) attribute_2535
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2536) attribute_2536
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2539) attribute_2539
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2540) attribute_2540
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2534) attribute_2534
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2538) attribute_2538
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2537) attribute_2537
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15234) attribute_15234
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15818) attribute_15818
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1379) attribute_1379
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1056) attribute_1056
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 717) attribute_717
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1033) attribute_1033
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1231) attribute_1231
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1251) attribute_1251
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1272) attribute_1272
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1289) attribute_1289
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1324) attribute_1324
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1276) attribute_1276
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15033) attribute_15033
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1317) attribute_1317
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1151) attribute_1151
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1159) attribute_1159
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1061) attribute_1061
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1117) attribute_1117
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1032) attribute_1032
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1273) attribute_1273
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1274) attribute_1274
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1249) attribute_1249
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1283) attribute_1283
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15232) attribute_15232
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1381) attribute_1381
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1080) attribute_1080
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1125) attribute_1125
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1126) attribute_1126
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1210) attribute_1210
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 9991) attribute_9991
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1212) attribute_1212
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 16264) attribute_16264
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1952) attribute_1952
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 715) attribute_715
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1219) attribute_1219
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1217) attribute_1217
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1104) attribute_1104
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2722) attribute_2722
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1055) attribute_1055
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 906) attribute_906
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1066) attribute_1066
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1315) attribute_1315
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1158) attribute_1158
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1178) attribute_1178
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1261) attribute_1261
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1214) attribute_1214
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1259) attribute_1259
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1233) attribute_1233
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1285) attribute_1285
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1022) attribute_1022
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1951) attribute_1951
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1948) attribute_1948
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1949) attribute_1949
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1950) attribute_1950
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1184) attribute_1184
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1209) attribute_1209
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15233) attribute_15233
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15231) attribute_15231
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1275) attribute_1275
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2723) attribute_2723
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1035) attribute_1035
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1106) attribute_1106
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15235) attribute_15235
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1310) attribute_1310
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1208) attribute_1208
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1120) attribute_1120
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1121) attribute_1121
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2724) attribute_2724
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1164) attribute_1164
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1163) attribute_1163
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1147) attribute_1147
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 29563) attribute_29563
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2125) attribute_2125
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 17434) attribute_17434
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1287) attribute_1287
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1284) attribute_1284
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 741) attribute_741
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1129) attribute_1129
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1081) attribute_1081
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 716) attribute_716
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1063) attribute_1063
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1142) attribute_1142
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1146) attribute_1146
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1321) attribute_1321
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15194) attribute_15194
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 15192) attribute_15192
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1124) attribute_1124
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1319) attribute_1319
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1116) attribute_1116
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1141) attribute_1141
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1150) attribute_1150
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1498) attribute_1498
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1332) attribute_1332
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 739) attribute_739
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 718) attribute_718
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1186) attribute_1186
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1185) attribute_1185
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2725) attribute_2725
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2519) attribute_2519
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2517) attribute_2517
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1128) attribute_1128
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1064) attribute_1064
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1077) attribute_1077
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1118) attribute_1118
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 11356) attribute_11356
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 719) attribute_719
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1057) attribute_1057
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1059) attribute_1059
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1068) attribute_1068
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1155) attribute_1155
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1154) attribute_1154
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1153) attribute_1153
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1187) attribute_1187
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1060) attribute_1060
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1072) attribute_1072
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1190) attribute_1190
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1191) attribute_1191
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1192) attribute_1192
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1193) attribute_1193
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1194) attribute_1194
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1325) attribute_1325
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2003) attribute_2003
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1034) attribute_1034
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1286) attribute_1286
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1127) attribute_1127
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1380) attribute_1380
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1051) attribute_1051
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 2002) attribute_2002
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1020) attribute_1020
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1152) attribute_1152
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1136) attribute_1136
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1078) attribute_1078
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1221) attribute_1221
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1119) attribute_1119
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 5385) attribute_5385
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1073) attribute_1073
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1311) attribute_1311
OUTER APPLY ( SELECT TOP 1 av.Value FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1312) attribute_1312
OUTER APPLY ( SELECT TOP 1 av.ValueAsDateTime FROM dbo.AttributeValue av WHERE av.EntityId = p.Id and AttributeId = 1071) attribute_1071
*/ 


/* update the HistoryHash
update Analytics_Dim_Person set [HistoryHash] = CONVERT(varchar(max), HASHBYTES('SHA2_512', (select top 1 FirstName, LastName, attribute_AbilityLevel /* ... */ from Analytics_Dim_Person where PersonId = adp.PersonId for xml raw)), 2)
from Analytics_Dim_Person adp where [HistoryHash] is null
*/



