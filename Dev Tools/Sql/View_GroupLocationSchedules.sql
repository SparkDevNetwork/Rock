SELECT S.Name 'Schedule', G.Name 'Group', L.Name 'Location'
FROM [Schedule] S
	INNER JOIN GroupLocationSchedule GLS ON GLS.ScheduleId = S.Id
	INNER JOIN GroupLocation GL ON GL.Id = GLS.GroupLocationId
	INNER JOIN [Group] G ON G.Id = GL.GroupId
	INNER JOIN [Location] L ON L.Id = GL.LocationId
ORDER BY S.Name, G.Name, L.Name

SELECT D.Id 'Device Id', D.Name 'Device', L.Id 'Location Id', L.Name 'Location'
FROM [Device] D
INNER JOIN [DeviceLocation] DL ON DL.DeviceId = D.Id
INNER JOIN [Location] L ON L.Id = DL.LocationId
