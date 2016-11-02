/* ====================================================== */
-- NewSpring Script #26: 

-- Sets GroupMemberStatus to be 0 (inactive) for all LEX campus
-- groups that are inactive (G.IsActive == 0) 
-- Added a foreign key of 'Deactivate LEX GroupMembers 10/30' to find later if necessary

-- VARS
-- G.campusId = 13 (Lexington)

/* ====================================================== */

UPDATE [GroupMember] 
SET 
  GroupMemberStatus = '0', 
  ModifiedDateTime=GETDATE(), 
  ForeignKey='Deactivate LEX GroupMembers 10/30'
FROM [Group] as G
    INNER JOIN [GroupMember] as GM
        ON GM.GroupId = G.Id
WHERE
    G.IsActive = '0' AND G.CampusId = '13'
