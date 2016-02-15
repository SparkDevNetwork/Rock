/* ====================================================== */
-- CodeGen to script devices

-- Scripts devices into sql inserts so they can be added to NewSpring 13

/* ====================================================== */

IF OBJECT_ID('tempdb..#TempCode') IS NOT NULL DROP TABLE #TempCode;

-- STEP 1: Generate code to insert devices
SELECT
	CONCAT(
		'INSERT [Device] ([Name], [Description], [DeviceTypeValueId], [IPAddress], [PrinterDeviceId], [PrintFrom], [PrintToOverride], [Guid]) VALUES (''',
		REPLACE(d.Name, '''', ''''''),
		''', ',
		CASE WHEN d.[Description] IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(d.[Description], '''', ''''''),
				''''
			)
		END,
		', ',
		CASE WHEN dv.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM DefinedValue WHERE [Guid] = ''',
				dv.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		CASE WHEN d.IPAddress IS NULL THEN
			'NULL'
		ELSE
			CONCAT(
				'''',
				REPLACE(d.IPAddress, '''', ''''''),
				''''
			)
		END,
		', ',
		CASE WHEN printer.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM Device WHERE [Guid] = ''',
				printer.[Guid],
				'''), '
			)
		ELSE
			'NULL, '
		END,
		d.PrintFrom,
		', ',
		d.PrintToOverride,
		', ''',
		d.[Guid],
		'''',
		');'
	) AS Code
INTO #TempCode
FROM 
	Device d
	LEFT JOIN Device printer ON printer.Id = d.PrinterDeviceId
	LEFT JOIN DefinedValue dv ON d.DeviceTypeValueId = dv.Id
ORDER BY
	d.PrinterDeviceId;

-- Step 2 - Generate code for device locations
INSERT INTO #TempCode
SELECT
	CONCAT(
		'INSERT [DeviceLocation] ([LocationId], [DeviceId]) VALUES (',
		CASE WHEN loc.Id IS NOT NULL THEN
			CONCAT(
				'( SELECT l.Id FROM DeviceLocation dl JOIN Location l ON l.Id = dl.LocationId LEFT JOIN Location pl ON pl.Id = l.ParentLocationId WHERE l.[Guid] = ''',
				loc.[Guid],
				''' OR ( l.Name = ''',
				loc.Name,
				''' AND ( ( LEN(''',
				ploc.Name,
				''') = 0 AND pl.Id IS NULL ) OR ( LEN(''',
				ploc.Name,
				''') > 0 AND pl.Id IS NOT NULL ANd pl.Name = ''',
				ploc.Name,
				''' ) ) ) ), '
			)
		ELSE
			'NULL, '
		END,
		CASE WHEN d.Id IS NOT NULL THEN
			CONCAT(
				'(SELECT Id FROM Device WHERE [Guid] = ''',
				d.[Guid],
				''') '
			)
		ELSE
			'NULL '
		END,
		');'
	) AS Code
FROM
	DeviceLocation dl
	JOIN Device d ON d.Id = dl.DeviceId
	JOIN Location loc ON loc.Id = dl.LocationId
	LEFT JOIN Location ploc ON ploc.Id = loc.ParentLocationId;

SELECT * FROM #TempCode;