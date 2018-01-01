IF OBJECT_ID(N'[dbo].[AnalyticsDimAttendanceLocation]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimAttendanceLocation
GO

CREATE VIEW AnalyticsDimAttendanceLocation
AS
SELECT l.Id [LocationId]
	,l.Name
	 ,l.Street1 [Street1]
    ,l.Street2 [Street2]
    ,l.City [City]
    ,l.County [County]
    ,l.[State] [State]
    ,l.Country [Country]
    ,l.PostalCode [PostalCode]
    ,l.GeoPoint [GeoPoint]
    ,l.GeoFence [GeoFence]
    ,l.GeoPoint.Lat [Latitude]
    ,l.GeoPoint.Long [Longitude]
FROM [Location] l
