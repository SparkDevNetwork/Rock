SELECT s.Id AS SiteId, s.Name AS Site, p.Id AS PageId, p.PageTitle AS Page
FROM Page p
JOIN Layout l ON p.LayoutId = l.Id
JOIN Site s ON l.SiteId = s.Id
WHERE p.RequiresEncryption = 0
AND s.RequiresEncryption = 0
ORDER BY s.Name, p.PageTitle