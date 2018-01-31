IF OBJECT_ID(N'[dbo].[AnalyticsDimCampus]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimCampus
GO

CREATE VIEW [dbo].AnalyticsDimCampus
AS
SELECT sc.*
	,CONCAT (
		lp.NickName
		,' '
		,lp.LastName
		) [LeaderFullName]
	,lp.Id [LeaderPersonId]
	,l.Street1 [AddressStreet1]
	,l.Street2 [AddressStreet2]
	,l.City [AddressCity]
	,l.County [AddressCounty]
	,l.[State] [AddressState]
	,l.Country [AddressCountry]
	,l.PostalCode [AddressPostalCode]
	,l.GeoPoint [AddressGeoPoint]
	,l.GeoFence [AddressGeoFence]
	,l.GeoPoint.Lat [AddressLatitude]
	,l.GeoPoint.Long [AddressLongitude]
	,CONCAT (
		l.[Street1]
		,' '
		,+ l.[Street2]
		,' '
		,l.[City]
		,', '
		,l.[State]
		,' '
		,l.[PostalCode]
		) [AddressFull]
FROM AnalyticsSourceCampus sc
LEFT JOIN PersonAlias lpa ON sc.LeaderPersonAliasId = lpa.Id
LEFT JOIN Person lp ON lpa.PersonId = lp.Id
LEFT JOIN [Location] l ON sc.LocationId = l.Id