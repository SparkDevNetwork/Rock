-- Calendar Detail Page
UPDATE [Page] SET 
    [InternalName] = 'Event Calendar',
    [PageTitle] = 'Event Calendar',
    [BrowserTitle] = 'Event Calendar',
    [IconCssClass] = 'fa fa-calendar',
    [BreadCrumbDisplayName] = 0 
WHERE [GUID] = 'B54725E1-3640-4419-B580-2AF77DAF6568'

-- Calendar Item Detail Page
UPDATE [Page] SET 
    [InternalName] = 'Calendar Item',
    [PageTitle] = 'Calendar Item',
    [BrowserTitle] = 'Calendar Item',
    [IconCssClass] = 'fa fa-calendar-o',
    [BreadCrumbDisplayName] = 0 
WHERE [GUID] = '7FB33834-F40A-4221-8849-BB8C06903B04'

-- Calendar Detail
UPDATE [BlockType] SET
	[Path] = '~/Blocks/Event/CalendarDetail.ascx',
	[Name] = 'Calendar Detail',
	[Description] = 'Displays the details of the given Event Calendar.',
	[Category] = 'Event'
WHERE [Path] = '~/Blocks/Calendar/EventCalendarDetail.ascx'

-- Calendar Item Detail
UPDATE [BlockType] SET
	[Path] = '~/Blocks/Event/CalendarItemDetail.ascx',
	[Name] = 'Calendar Item Detail',
	[Description] = 'Displays the details of the given calendar item.',
	[Category] = 'Event'
WHERE [Path] = '~/Blocks/Calendar/EventItemDetail.ascx'

-- Calendar Item List
UPDATE [BlockType] SET
	[Path] = '~/Blocks/Event/CalendarItemList.ascx',
	[Name] = 'Calendar Item List',
	[Description] = 'Lists all the items in the given calendar.',
	[Category] = 'Event'
WHERE [Path] = '~/Blocks/Calendar/EventCalendarItemList.ascx'

-- Calendar Lava
UPDATE [BlockType] SET
	[Path] = '~/Blocks/Event/CalendarLava.ascx',
	[Name] = 'Calendar Lava',
	[Description] = 'Renders a particular calendar using Lava.',
	[Category] = 'Event'
WHERE [Path] = '~/Blocks/Calendar/ExternalCalendarLava.ascx'

-- Calendar Types
UPDATE [BlockType] SET
	[Path] = '~/Blocks/Event/CalendarTypes.ascx',
	[Name] = 'Calendar Types',
	[Description] = 'Displays the calendars that user is authorized to view.',
	[Category] = 'Event'
WHERE [Path] = '~/Blocks/Calendar/EventCalendarList.ascx'

