Declare @ValueTable table(SeriesName varchar(250), year int, YValue int) 
-- Previous Years 
INSERT Into @ValueTable 
SELECT Concat(Yr-1,' - ',Yr), 
        Yr, 
        COUNT(A.Id) As YValue 
FROM ( 
    SELECT p.Id, 
    CASE When (Month(av.[ValueAsDateTime]) < 9) Then Year(av.[ValueAsDateTime]) Else Year(DateAdd(year,1,av.[ValueAsDateTime])) End As Yr 
    FROM Person p 
    JOIN AttributeValue av on av.AttributeId = 174 And p.Id = av.EntityId
    ) As A 
-- Change this 
INNER Join ( 
    SELECT MIN(g.CampusId) As CampusId, 
            p2.Id 
    FROM Person p2 
    INNER Join GroupMember gm on gm.PersonId = p2.Id 
    INNER Join [Group] g on g.Id = gm.GroupId And g.GroupTypeId = 10 
    GROUP By p2.Id 
    ) As B on A.Id = B.Id 
GROUP By Yr 

SELECT SeriesName, convert(datetime,convert(nvarchar(max),[Year])+'-01-01') as [DateTime], convert(numeric,YValue) as YValue
FROM @ValueTable 
WHERE [Year] >= 2000 
ORDER By [Year]