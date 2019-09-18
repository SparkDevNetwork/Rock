/* Use this script to remove most of the unneeded blocks from Page/12. This can increase startup time by 10+ seconds or so */

DELETE
FROM [Block]
WHERE [Guid] IN (
		'62B1DBE6-B3D9-4C0B-BD12-1DD8C4F2C6EB'
		,-- Install Checklist
		'CB8F9152-08BB-4576-B7A1-B0DDD9880C44'
		,-- Active Users
		'03FCBF5A-42E0-4F45-B670-BC8E324BD573'
		,-- Active Users
		'2C90BDF8-48FF-4A7C-AA70-97B7E3780177'
		,-- My Workflows Liquid
		'4AA6DB52-D44E-43FB-8A6F-43BEC93AA341'
		,-- My Workflows Liquid
		'60469A41-5180-446F-9935-0A09D81CD319'
		,-- Notification List 
		'879BC5A7-3CE2-43FC-BEDB-B93B0054F417'
		,-- Internal Communication View
		'F337823D-BA5D-49F8-9BC4-1EF48C9000CE' -- Dev Links
		)