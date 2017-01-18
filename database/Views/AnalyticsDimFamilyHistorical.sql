IF OBJECT_ID(N'[dbo].[AnalyticsDimFamilyHistorical]', 'V') IS NOT NULL
    DROP VIEW AnalyticsDimFamilyHistorical
GO

CREATE VIEW AnalyticsDimFamilyHistorical
AS
SELECT asfh.*
    ,isnull(c.NAME, 'None') [CampusName]
    ,isnull(c.ShortCode, 'None') [CampusShortCode]
    ,mailingLocation.Street1 [MailingAddressStreet1]
    ,mailingLocation.Street2 [MailingAddressStreet2]
    ,mailingLocation.City [MailingAddressCity]
    ,mailingLocation.County [MailingAddressCounty]
    ,mailingLocation.[State] [MailingAddressState]
    ,mailingLocation.Country [MailingAddressCountry]
    ,mailingLocation.PostalCode [MailingAddressPostalCode]
    ,mailingLocation.GeoPoint [MailingAddressGeoPoint]
    ,mailingLocation.GeoFence [MailingAddressGeoFence]
    ,mailingLocation.GeoPoint.Lat [MailingAddressLatitude]
    ,mailingLocation.GeoPoint.Long [MailingAddressLongitude]
    ,mappedLocation.Street1 [MappedAddressStreet1]
    ,mappedLocation.Street2 [MappedAddressStreet2]
    ,mappedLocation.City [MappedAddressCity]
    ,mappedLocation.County [MappedAddressCounty]
    ,mappedLocation.[State] [MappedAddressState]
    ,mappedLocation.Country [MappedAddressCountry]
    ,mappedLocation.PostalCode [MappedAddressPostalCode]
    ,mappedLocation.GeoPoint [MappedAddressGeoPoint]
    ,mappedLocation.GeoFence [MappedAddressGeoFence]
    ,mappedLocation.GeoPoint.Lat [MappedAddressLatitude]
    ,mappedLocation.GeoPoint.Long [MappedAddressLongitude]
FROM AnalyticsSourceFamilyHistorical asfh
LEFT JOIN Campus c ON asfh.CampusId = c.Id
LEFT JOIN Location mailingLocation ON mailingLocation.Id = asfh.MailingAddressLocationId
LEFT JOIN Location mappedLocation ON mappedLocation.Id = asfh.MappedAddressLocationId