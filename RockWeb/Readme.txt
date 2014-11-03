+ Update the Content Channel Dynamic block so that it does not default to a specific channel when first added to a page.
+ Update ordering of defined type attributes to clear the attribute cache so that editing values immediately reflects the updated order.
+ Turn off form validation for the field that is returned by Mandrill to Rock's Mandrill webhook so that exceptions do not occur when Mandrill posts this event data.
+ Fixed issue with the date picker when used at the bottom of screens (Fixes #592).
+ Update the Content Channel Dynamic block to set the correct liquid template folder prior to looking for liquid template files.
+ Update Check-in Configuration block to save new group types (Areas) with correct default options for being used by check-in application (Fixes #610).
+ Made the Dynamic Content Channel feed feature work off of liquid templates. NOTE: The address of the handler changed from GetChannelRss.ashx to GetChannelFeed.ashx to make it more reusable. There is also a new defined type for storing liquid templates in. See documentation for more information.
+ Update the Content Channel Dynamic block to allow sorting items by attribute values in addition to item properties.
+ Update the Zone editor so that it refreshes the page after adding, removing, or changing the order of blocks on a page.
+ Fix the age and anniversary on person profile page, and the birthday picker on person edit page to be culture aware when formatting date display (Fixes #607).
+ Correct bug that was not saving the first attribute value on new group members (Fixes #606).
+ Family can now be deleted if there is only one member and that member is in at least one other family (Fixes #581).
+ Update the System Information's Clear Cache option to also clear all the cached security authorization rules.
+ Fix issue with attribute images being saved as temporary images (and therefore subsequently being deleted by clean-up job).
+ Update how cache items are cleared in the System Information's Clear Cache option. It now deletes the entire caching object instead of attempting to remove individual items.
+ Fix the Assign Activity to Group workflow action to correctly assign the activity to the selected group.
+ Fix exception caused when viewing a single select field with no value (Fixes #602).
+ Update the Person Profile page to check for 'Edit' security before allowing edits to the person, family, and relationships.  Also add Staff and Staff-Like roles to have the ability to edit by default (Fixes #597).
+ Update Google maps to load over both http and https (Fixes #593).
+ Update check-in location active time, and mandrill notices to handle time zone time comparisons correctly (Fixes #594).
+ Fix the workflow text/attribute drop down control to allow html in the text portion of the control (Fixes #591).
+ Fix issue that would cause 404 page to fail XSS validation test (Fixes #563).
+ Fix GivingGroup not getting set in EditPerson (Fixes #599).
+ fix address control getting an error if the Country doesn't have any States defined (Fixes #587).
+ Updated the Content Channel Dynamic blocks on the external site to use the themes liquid files (as an include file).
+ Updated the Group Detail block so that GroupType can only be edited when adding a new group (Fixes #605).
+ Fixed auto discovery link for RSS channels
