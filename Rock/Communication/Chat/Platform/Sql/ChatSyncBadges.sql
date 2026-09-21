-- The badges section: the church's configured badge list, a few rows.
--
-- The sort order is the position the administrator put the badge in, not anything the Data View
-- carries, so that every client renders the church's own order rather than each one sorting by
-- whatever it happens to hold.
--
-- The highlight colour is returned raw. The pair of colours the wire carries is derived from it
-- before it is sent, so the contrast decision is made once here rather than three times in three
-- clients that would each reach a different answer.

SELECT
    [DV].[Guid] AS [badge_key],
    [DV].[Name] AS [name],
    [DV].[IconCssClass] AS [icon_css],
    [DV].[HighlightColor] AS [highlight_color],
    [BV].[SortOrder] AS [sort_order]
FROM #BadgeViews AS [BV]
INNER JOIN [DataView] AS [DV] ON [DV].[Guid] = [BV].[DataViewGuid]
WHERE [DV].[PersistedScheduleIntervalMinutes] IS NOT NULL
ORDER BY [BV].[SortOrder];
