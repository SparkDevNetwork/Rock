Rock McKinley 7.4

+ Added a new badge that shows interactions for a specific channel.
+ Added a new check-in workflow action that allows filtering groups by gender.
+ Added a new Data Automation job and a block to configure how data is automatically updated. This includes the inactivating/reactivating of person records, changing family campus, and moving adult children into their own new families.
+ Added a new 'Decrypt' Lava filter.
+ Added a new 'Family Registration' block for families to pre-register before visiting for the first time.
+ Added a new Gender field type.
+ Added a new option to the Prayer Request List block that can be used to limit the list of prayer requests to only those that are public.
+ Added a Previous button to the Transaction Detail block.
+ Added additional data view filters and report selects for Benevolence, Prayer Requests, and Scheduled Transactions.
+ Added an additional merge field to the Interaction Component List block to return the number of interactions by component.
+ Added an option to hide the internal login option on the Login block.
+ Added an option to the Login Block so that it will go immediately to the external auth url if that is the login option.
+ Added an option to the Person detail screen in Check-in Manager to enable showing related people who are allowed to check-in the selected person, in addition to only family members.
+ Added an optional append order to the Lava Fallback text filter.
+ Added CC and BCC support to the Email Form block.
+ Added Connection Status as a possible Person Field when building Registration Templates.
+ Added email address to the information displayed on the Prayer Request Detail block.
+ Added filters for the Interactions List block.
+ Added Lava shortcode to embed videos from Vimeo.
+ Added Person ID to the Excel file when exporting from the Transaction List block.
+ Added Spark integration reference number to Payflow Pro gateway. This will help Payflow Pro identify transactions that came from Rock.
+ Added support for plugins to use custom SQL Functions in LINQ queries.
+ Added the ability to filter interaction blocks by a specific person through the query string.
+ Added the ability to set the connection opportunity via a block setting on the Connection Opportunity Sign-up block.
+ Fixed a bug in the Calendarlava block where switching between week and month without clicking on the calendar first would select the first week then the previous month.
+ Fixed a history point creation exception on event registration.
+ Fixed an exception that would occur in the Group Sync job when attempting to create a login for a person with no name.
+ Fixed an issue preventing user-drawn geofences from filtering groups and people.
+ Fixed an issue that kept inline Lava shortcodes from having recursive capabilities.
+ Fixed an issue that was causing the 'send signature document reminder' to create a new document instead of using the existing one.
+ Fixed an issue when impersonating another user that would result in the Restore button not always being visible in the administrator toolbar.
+ Fixed an issue where copying a communication template would not reset IsSystem to false.
+ Fixed an issue where text and SMS messages created through a workflow, job, etc., would not resolve Lava commands successfully.
+ Fixed an issue where the URL value on Redirect workflow actions was getting lost.
+ Fixed an issue with communication recipient fields not working for a dataview when used by a Dynamic Report block.
+ Fixed an issue with not being able to scroll vertically on an IOS device from a tree view (i.e. Group Tree, Account Tree, Page Map, etc).
+ Fixed an issue with several list blocks where filtering the list by an attribute's default value would not return all of the correct items.
+ Fixed Attribute Matrix editor not enforcing validation in some situations. This fixes an issue where validation would not be enforced when an attribute matrix was used in the Person Profile attributes.
+ Fixed bug in Purchased Plugins page that would throw exception if a previously purchased plugin was not installed.
+ Fixed check-in so that the age comparisons are only as precise as the specified number of digits, and will no longer require entering extremely precise age ranges with a large number of decimal places.
+ Fixed check-out success message to correctly show the schedule name instead of the frequency.
+ Fixed Communication Entry to create recipient correctly when linked to a person from another page (i.e., from Person Profile).
+ Fixed confirmation email for registration never being received. Changed email send logic to better fall back on global org email for the from address if one is not specified in the email object.
+ Fixed error in the batch list that prevented batches in the grid list from being opened or closed using the drop down list. (Fixes #2977)
+ Fixed error that could occur when using the Attribute Editor to edit attributes with different field types and at least one of the field types had multiple configuration controls.
+ Fixed error when adding a person to a new family in a workflow.
+ Fixed event registration so that it displays fees in the correct order.
+ Fixed exception that would occur if creating a communication on a page who's URL was longer than 200 characters (i.e., a shared attendance analytics link).
+ Fixed exceptions that would occur if an attribute's value was null (possibly caused from migrating data).
+ Fixed FileUploader to pass correct arguments when using a grid to store the control.
+ Fixed Group Edit showing Security Role checkbox for users with Edit privileges but not Administrator. Group Edit will only show the Security Role checkbox if the current user is a Rock Administrator.
+ Fixed issue in Fundraising Matching (Transaction Entity Matching) where transactions could be falsely matched after saving changes to one batch, switching to another batch, then saving changes on that batch. (The selection would sometimes be stuck on the selections from the first batch for the corresponding row.)
+ Fixed issue that would result in having to login again after restoring an impersonated user if you never navigated to another page after impersonating them.
+ Fixed issue where communication approval emails did not include the correct link to the communication needing approval.
+ Fixed issue where escaped HTML gets displayed in a grid.
+ Fixed issue where new Content Channel Items were not added to the bottom of the grid if manually reordered.
+ Fixed issue where site's default page was being rendered instead of a 404 page when requesting an invalid page/route.
+ Fixed issue where the registrant's campus was not showing up on the Registrants tab, WaitList tab, and GroupPlacement tab in the Registration Instance Detail block.
+ Fixed issue with not being able to check in a person using the manager override option.
+ Fixed issue with not being able to create a personal communication template from My Settings.
+ Fixed issue with several HTML content areas showing a warning in the code editor.
+ Fixed issue with the pager in the footer of the Podcast Series List for the external example themes.
+ Fixed issue with the RockListBlock saving correct values when an item value included a space.
+ Fixed label security codes not to use variations of '666' for numerical increments.
+ Fixed link paths to send the entire path in notification emails.
+ Fixed Simple Communication Entry block to show SMS medium correctly if that was the only medium configured.
+ Fixed sort ordering on the Notes Lava filter. Both asc and desc are now working as expected.
+ Fixed the ability to link a Registration Instance to an existing Calendar Item.
+ Fixed the age formatting on the My Account page so that "old" displays correctly now after "yrs" instead of after the birth date.
+ Fixed the Benevolence statement to show correct case worker address while printing.
+ Fixed the Communication List block so that it no longer causes an exception when a query error occurs.
+ Fixed the Group Context Setting to allow the Clear Selection option to be blank to match the help text description of the feature.
+ Fixed the Group Context Setting to allow the 'Clear Selection' option to be blank to match the help text description of the feature.
+ Fixed the In Registration Instance data view filter to include anyone who registered multiple times.
+ Fixed the Interaction Session List block so that it does not timeout with a large number of page views.
+ Fixed the Key Value List to correctly display the ^ character.
+ Fixed the links for sharing items to be encoded correctly.
+ Fixed the 'Right' Lava filter to not throw an exception if the request length is  greater than the string length.
+ Fixed the SetPageTitle Lava filter to also update the breadcrumbs.
+ Fixed the standard account confirmation messages for the Account Confirmation block.
+ Fixed the way Person is retrieved in the Lava filters.
+ Fixed Transaction List block to filter transactions correctly when using the 'Campus (of Batch)' filter.
+ Fixed workflow email issue where action buttons would not work.
+ Improved performance of the Batch List block.
+ Removed hyphens from "Thank-you" and "on-going" throughout Rock.
+ Updated EventItemOccurrencesSearchLava, EventDetailWithOccurrencesSearchLava and EventItemOccurrenceListLava to include 'All Campuses' event occurrences when filtering by campus. This makes the functionality consistent with the CalendarLava and EventItemOccurrenceListByAudienceLava block, which include 'All Campuses' events when filtering by campus.
+ Updated exception logging to always log to the RockExceptions.csv file during startup.
+ Updated Giving Amount data filter to include people who didn't have any transactions when doing a Less Than filter.
+ Updated how person and family history records are created so that it is now only created in one shared place instead of each block having to create the history records.
+ Updated Number box to display integers, and Currency box to display with two decimal places. iOS keyboards default to the correct input.
+ Updated the Account Picker control so that it defaults to using the account Name (instead of Public Name) and added a new property to control whether the public name should be used.
+ Updated the Attendance Analytics block so that it can be specific to an individual group.
+ Updated the attendance block to add an additional merge field (AttendanceDate) to the roster merge document for passing in the date that the attendance is for.
+ Updated the attribute editor so that when it is used inside a modal dialog it will display the id of the attribute being edited in the title of the dialog.
+ Updated the Auto Select Days Back option in Family Check-in so that a value of zero will never auto select anyone.
+ Updated the Bulk Update block to improve performance when updating a large number of people.
+ Updated the Calendar Lava block to accept date parameters from the query string.
+ Updated the Check-in Schedule Builder block to support configuring schedules for group types that support check-in, but are not configured through the check-in areas.
+ Updated the Communication Entry Wizard block to remember the template category selection a user selects and default to that same category the next time they create a communication.
+ Updated the Communication Entry wizard not to use hard-coded table widths in order to assist with media queries.
+ Updated the Communication Wizard to indicate that CC/BCC recipients will recieve an email for every recipient in the communication.
+ Updated the configuration of Event Registration so if a '$' character is included with fee cost(s), it will no longer cause an issue.
+ Updated the Connection Request Detail block to hide workflows that are inactive or the user is not authorized to view.
+ Updated the custom Rock check-in font. If you've installed this on your printers, you may want to update it. In some cases, the icons on this font will not accept the proper placement information from the label. See the related Github issue for more details.
+ Updated the Dynamic Data block to support decrypting encrypted values returned by the query.
+ Updated the editor in the new Communication Wizard block so that the controls toolbar is fixed and does not scroll off the screen when scrolling the email content down.
+ Updated the Email Preference Entry block to allow user the option of updating their email address, and added configuration option for customizing which options are displayed.
+ Updated the Family Pre-Registration block to use the Date Part Picker instead of the Date Picker for birth dates.
+ Updated the File Browser to actually disable the Rename, Move and Delete buttons when they are disabled.
+ Updated the Financial Transaction List block to include Batch Id when exporting to excel.
+ Updated the Giving Analytics block to support filtering transactions by Transaction Type.
+ Updated the Grade Transition Date global attribute to use a month/day picker, and updated the grade and graduation date logic so it works consistently regardless of culture.
+ Updated the Group Attendance Detail block to allow people with Manage Members security to update attendance.
+ Updated the Group Finder block to only consider 'Active' group members when calculating capacity (to be consistent with all the other places capacity is checked).
+ Updated the Help text on the pledge blocks to clarify the meaning of the total amount and pledge frequency.
+ Updated the HtmlEditor to prevent an issue where switching to WYSIWYG mode would rearrange the text if there were Lava commands. Now Rock will detect if there are Lava commands and automatically start in code editor mode. It will also display a warning and prevent the editor from going to WYSIWYG mode if there are any Lava commands (e.g., if the code contains a {% for %} loop around an HTML Table Row [tr], Rock will detect the Lava and prevent WYSIWYG mode).
+ Updated the Interaction reporting for Short Links to be more readable and show the fields that one would be interested in when looking at short links.
+ Updated the Line Chart Dashboard Widget block to support getting more than one metric partition context from the page.
+ Updated the location services to automatically enable the Smarty Streets integration if no other location services were enabled. Now that Smarty Streets is free it will be default ON. You can turn it off if you wish and it will not be enabled again in the future. Also, removed the Administrator Check-list item for enabling location services.
+ Updated the person badges to align better with Font Awesome 5.
+ Updated the Person Merge block to show unlisted phone numbers with an indicator that it is unlisted. This makes it easier to determine the correct phone number to use.
+ Updated the 'Set Defaults' text on the Dynamic Report block to say 'Reset Filters' so that it better indicates what the link will do.
+ Updated the site's 404 page to display with a result status of 404. Before it would show the page 404 page, but not set the HTTP status to be 404.
+ Updated the Summary of Contributions panel on the Contributions tab of the Person Profile to allow showing less after the user has selected to show more.
+ Updated the text of the Next button on family check-in to be configurable.
+ Updated the Transaction Matching block to remove the unused 'Show Selected Accounts Only' block setting. This functionality is actually controlled by a filter setting and is specific to each user.
+ Updated the Universal Search site crawler to process a robots.txt file correctly and fixed a possible stack overflow issue that would occur if page hierarchy was deep enough.
+ Updated the way SMS Response Codes are generated to prevent infinite loops.


Rock McKinley 7.3

+ Added 'Order Top-Level Accounts' option to Accounts configuration.
+ Fixed an issue where a person could be removed from a synced group (or security role).
+ Fixed an issue in the Communication Entry blocks where Approvers wouldn't be able to edit a communication that was pending approval.
+ Fixed an issue where Font Awesome font weights were not calculating correctly, causing checkboxes to appear solid.
+ Fixed an issue where the Workflow Entry block would get stuck in a loop if it was passed a command value in the query string.
+ Fixed an issue with pages added using the new Pages editor not defaulting the Show Child Pages property to 'true'. This fixes an issue where the Page Menu block appeared not to be working.
+ Fixed Dynamic Report not showing Labels.
+ Fixed Giving Type Context Picker not setting the current person context correctly in some instances.  This fixes an issue of the contribution statement list lava sometimes not displaying any available statements.
+ Fixed issue where a person could be removed from a synced group (or security role).
+ Fixed issue where Communication Admins and Communication Approvers were not able to see pending communications.
+ Fixed issue where 'Not in Existing Dataview' returned incorrect results.
+ Fixed issue where Scheduled Transactions couldn't be added using the Schedule Transaction List block.
+ Fixed issue with Phone Number field on Reports showing the phone number for mobile regardless of the phone type selected.
+ Fixed matrix attributes being hidden in view mode when used as person attributes.
+ Fixed Merge Template Entry only working if Unsecured file type had a storage type of 'Database'.
+ Fixed Merge Templates not showing Lava Help correctly for GroupMember, etc., when exporting from a Group Member list.
+ Fixed Person Duplicate job taking a long time on some databases.
+ Fixed Person Has Note data filter so that leaving the note type blank will return records that have any type of note.
+ Fixed public calendar showing inactive campuses when the Campus Filter option is enabled.
+ Fixed Send SMS workflow action throwing an exception when the recipient was a group with multiple people in it.
+ Fixed styling of the icons for External Applications to make it more friendly to raster images.
+ Fixed the Attendance Analytics block so that the columns displayed include the same dates as the attendance being calculated. For example, if using a date range of 01/01/18 through 01/31/18, a column will no longer be displayed for 02/04/18 and a date of 02/04/18 will not affect the calculated attendance rate.
+ Updated Copy Communication so that it only requires VIEW security instead of EDIT. This fixes an issue where the Copy Communication button in communication history was not visible to some people.


Rock McKinley 7.2

+ Fixed communications so that a person's email preference does not affect sending of SMS messages.
+ Updated Bio block on Person Profile page to have a 'Communication Page' block option that defaults to the simple communication page.
+ Fixed Registration Instance List Block by removing unused field from the grid.
+ Fixed issue where the Active Registration Instance List block did not show instances when blank dates were present.


Rock McKinley 7.1

+ Added a delete confirmation when deleting members from the Leader Toolbox.
+ Added a new Attribute type of 'Lava Field', which allows a Lava template to be added to entities and will render its contents as the value. This will be helpful in creating more descriptive settings and configuration panels.
+ Added a new block setting that can be used to prevent entering a future date in the Group Attendance Detail block.
+ Added a new configuration option to the Attendance Analytics block to limit the available data views in the filter to specific category(s).
+ Added a new 'Label' field type. This makes it easier to add a label to a specific person and/or group that will always print when that person checks in, or when anyone checks into that group. This is an advanced configuration done outside the normal Check-in Configuration block.
+ Added a new Manage Members security action to the group and grouptype models that can be used to grant users the ability to add, edit, and delete members in a group without having the ability to edit or delete the group.
+ Added new blocks for viewing interaction data.
+ Fixed an exception that would occur in the statement generator if a pledge exists with a maximum end date value (12/31/9999).
+ Fixed an issue where the required field indicators were not showing on the New Family control.
+ Fixed an issue where workflows would process the wrong User Entry Form if multiple forms existed in the same Activity or a single Activity was activated multiple times.
+ Fixed an issue with Check-in not displaying the correct message or the countdown timer when the kiosk was closed but had an upcoming schedule for the same day.
+ Fixed an issue with Check-in using the people/group/location auto check-in option, which had allowed people to check-in again to the same service despite the option to prevent this being selected.
+ Fixed an issue with Font Awesome brand icons on the Login block.
+ Fixed an issue with NMI transactions not saving the Name On Card value after processing a transaction.
+ Fixed an issue with not being able to select items when using an iOS device.
+ Fixed Attribute Editor to allow editing the default values of field types that have multiple entry fields (i.e., Group Role).
+ Fixed the EnumField and EnumsField attributes so that they support being used more than once on a block.
+ Fixed various field types so that attribute values of the types can be reverted back to the default values.
+ Fixed workflow processing so that if a redirect action occurs with remaining workflow entry actions, the redirect still happens.
+ Updated communications to fix an exception that would occur if certain communication fields (e.g., subject, from name, etc.) were too long.
+ Updated the Attributes block to prevent  multiple global attributes being created with the same key.
+ Updated the Bootstrap Button to support a completed text and/or message that can be displayed for a set duration after the post back completes.
+ Updated the Bulk Update block to allow setting a new Communication Preference.
+ Updated the CodeEditor control to display correctly even if it is not initially set to be visible on a block.
+ Updated the Communication List block to use the communication name when subject is empty (i.e., for SMS communications).
+ Updated the Create Labels check-in action to look for labels configured on the person, group and/or location objects rather than only the group type object.
+ Updated the device search by hostname functionality in Check-in to match against the IPAddress field.
+ Updated the editing of family attributes so that updates are logged to history.
+ Updated the Group Member List block to display any group member note values when hovering the mouse over the note icon.  Also added an option to display the notes in a separate column instead of only showing an icon.
+ Updated the merge document process to no longer delete word fields from the template when merging data.
+ Updated the Metric Detail block to display the ID of the metric to assist in Lava development using metrics.
+ Updated the Person Property Update workflow action to support setting a person's communication preference value.
+ Updated the Schedule Detail block to support viewing and editing attributes associated with schedules.


Rock McKinley 7.0

+ Added a block setting to the Group Detail block to toggle the display of the address below the map. This keeps you from having to edit the group to see the actual screen address of the group's location.
+ Added a configurable logo to Communication Templates.
+ Added a data filter to filter people who have registered for registration instances of a particular template(s).
+ Added a feature to the Scheduled Transaction List and TransactionEntry block that allows scheduled transactions (that use an 'old' gateway) to be transferred to use the new gateway.
+ Added a new "Right" Lava filter which returns the rightmost part of a string of the given length.
+ Added a new block to display communications that are currently queued to be sent.
+ Added a new Communication List Subscribe block where a person's communication list subscriptions can be managed.
+ Added a new Communication Wizard for creating communications.
+ Added a new control, field type, and attribute type for selecting one or more data views.
+ Added a new data view filter for Person data views for filtering based on whether a person is included in a Personal Device data view.
+ Added a new EntityAttributeValues block that can be used to view and edit attribute values for entities that have an existing UI (though the UI does not currently have support for editing attribute values).
+ Added a new FileSize property to the BinaryFile model that storage providers can use to store the size of a file when it is saved, and updated the database and filesystem providers to update the property.
+ Added a new Giving Type context picker that can be used on a Giving History page. This allows a person that has businesses to either choose themselves or one of their businesses when viewing giving history.
+ Added a new IRockOwinStartup interface for allowing plugins to implement OWIN functionality.
+ Added a new Javascript Lava command for adding scripts to your page. This is especially helpful with the new Lava Shortcuts.
+ Added a new job that will monitor the communication queue and send a notification if communications are not getting sent in a timely matter.
+ Added a new Lava command to allow you to place a stylesheet into the page head with support for Less compiling and caching. https://www.rockrms.com/Lava/Stylesheet
+ Added a new Lava filter for determining if the person has signed a particular digital document.
+ Added a new Lava filter that adds a CSS link to the page.
+ Added a new Lava filter to add a script link to the page.
+ Added a new Lava Filter to get the End Date from an iCal feed.
+ Added a new Lava webhook that can return results that are configured through Lava and vary based on the URL and HTTP method used to make the request.
+ Added a new 'No Account Text' setting to the Login block to allow customizing of the message displayed when an invalid user login is entered.
+ Added a new option to the Actions list on person profile to allow downloading a vCard.
+ Added a new Person History Following event type to detect and send a notice when specific changes to a person's demographic history are made for someone who is being followed.
+ Added a new Pledge Data View filter to allow filtering people by their pledges.
+ Added a new social security number control and field type.
+ Added a new Summary text field to the workflow type to display a friendly summary of the status of a workflow.
+ Added a new Webhook to activate a Workflow.
+ Added a new workflow action for setting any attribute value on any entity.
+ Added a new workflow action for setting any property value on any entity.
+ Added a new workflow action that can be used to add a following record.
+ Added a new workflow action that can set a File Attribute using Lava.
+ Added a new workflow action to create short links.
+ Added a new workflow action to set the connector on a connection request.
+ Added a setting to display groups only under a selected group.
+ Added a setting to FundraisingDonationEntry to allow for automatic selection when only a single active participant exists.
+ Added a 'Transaction Details' mode to the Transaction List
+ Added a User Login related data view filter for Person data views.
+ Added ability to configure multiple group types/roles/statuses for connection opportunities
+ Added ability to set security on Group Attributes.
+ Added Account filter block setting to various Finance blocks so they can be configured to only allow specific accounts.
+ Added Account Public Name in PledgeSummary for ContributionStatementLava.
+ Added additional settings to New Family block to allow optionally requiring Birthdate, Address, and a Phone Number.
+ Added an additional "Register" security action that can be used to give the ability to add/edit/delete registrations and registrants without the ability to edit the registration instances.
+ Added an attribute to Fundraising Opportunity Types that allows overriding the Donate button text.
+ Added an enhanced interface for drop down lists and as an alternative to check box lists. This uses the "chosen" jquery plug-in from Harvest.
+ Added an option to scheduled transactions for changing the account allocation. This works regardless of the gateway associated with the scheduled transaction.
+ Added an option to search by security code to the Check-In Manager.
+ Added an option to select the connector when transferring a connection request.
+ Added an option to show a CampusFilter on the GroupMap. To enable, edit the block settings.
+ Added an option to unsubscribe from a communication list.
+ Added an optional Custom Content block setting to the person Bio block that allows adding additional content.
+ Added 'Batch Id' as a column in Transaction List.
+ Added BI Analytics to include a Campus Dimension with support for Campus Attributes.
+ Added block for listing active registration instances.
+ Added block to view the fundraising progress for all people in a fundraising group.
+ Added Campus filter to Group Tree View settings panel and related data calls, and added a Group search box to the Group Tree View settings panel.
+ Added Click to Call to HTML of Stark GroupDetail.lava.
+ Added 'Edit Connection Status' and 'Edit Record Status' as security actions on the Edit Person block.
+ Added FontAwesome 5. Be sure to see the documentation for updated information on it's usage.
+ Added framework for linking Rock to phone systems (PBX). The features added allow for plug-ins to be created for specific phone systems to allow for features like creating interactions from call detail records and click to call.
+ Added 'Image Tag Template' option to Image Attributes so that the resulting IMG tag can be customized through Lava.
+ Added interaction cache objects and updated Interaction Add workflow action to use cached objects.
+ Added job that will complete workflows older than a certain age.
+ Added Last Attended and First Attended as options on the InGroups person datafilter.
+ Added 'Lava' as a Metric SourceType.
+ Added logic to Fundraising Progress block to function on a single participant.
+ Added new {{ 'Lava' | Debug }} Lava filter which will show details about the available merge fields.
+ Added new Attended Group of Type person badge.
+ Added new block to allow rapidly entering attendance data (such as for a worship service).
+ Added new data transformations that can be used in data views to transform a list of people to their mothers, fathers, grandparents, or grandchildren.
+ Added new dataview filter to search for Person records that have a note containing specified text.
+ Added new EventOccurrencesSearchLava block.
+ Added new functionality for creating short links and tracking when people use them.
+ Added new Giving person badge.
+ Added new GroupById and GroupByGuid Lava filters to retrieve a group by its Id and Guid respectively.
+ Added new job for sending notices for expiring credit cards.
+ Added new Lava filter FamilySaluation (http://www.rockrms.com/lava/person#familysalutation).
+ Added new Lava filter for determining the number of days in a month.
+ Added new Lava Filter for getting information about the client. https://www.rockrms.com/page/565#client
+ Added new Lava filter for listing Notes.
+ Added new Lava filter to get a unique identifier (a guid).
+ Added new Lava filter to retrieve the value of a page property.
+ Added new Lava shortcode feature.
+ Added new Location data view filter to make filtering by address easier.
+ Added new Redirect and ShowHtml workflow actions.
+ Added new Rock job to automate SQL Server maintenance. Note if you have created a custom maintenance plan you'll want to either disable the job or your custom plan. [docs]
+ Added new Spouse DataView Transform.
+ Added new 'Workflow' field/attribute type and added new workflow action to set status on another workflow.
+ Added option to Group Member list to allow filtering group members by registration instance.
+ Added option to have a predefined list of Batch Names when entering a new batch.
+ Added option to prompt for middle name in the New Family Block.
+ Added option to reassign saved bank accounts when reassigning transactions.
+ Added option to restrict Group Context by the Campus Context.
+ Added option to set the WorkflowType of the Activate Workflow action from an attribute.
+ Added option to the Date Field type to have a Future Years setting when using the date parts picker.
+ Added option to the Group Member Remove from URL to inactivate the person instead of removing them completely. Also allowed the group to be passed by Guid if desired.
+ Added options to the redirect block to only redirect when on/not on specific networks. This will allow you to keep people in certain roles from accessing pages/sites outside of the organization's network.
+ Added Payment Detail entity attributes to the Transaction Detail and Transaction Matching blocks.
+ Added property to the page to add a CSS class to the body tag. The theme must define the body tags as <body id="body" runat="server"> in order for this to work.
+ Added PublicName and Description to AccountSummary on the Contribution Statement.
+ Added REST endpoint for sending a communication.
+ Added REST endpoint to get Rock version (/api/Utility/GetRockSemanticVersionNumber).
+ Added security to Communications based on the Communication Template that was used.
+ Added security verification block under Admin Tools > Security to show the effective permissions on a specific entity.
+ Added settings to allow changing Image and Document paths on new communications for use with toolboxes.
+ Added some additional configuration options to new family block to control which fields are displayed.
+ Added some additional options to Content Channel Types: Option to disable the Content field, a new 'No Dates' date range type, an option to disable the priority field, and an option to disable the status field (which treats all content as Approved).
+ Added support for configuring content channels to allow tagging of items.
+ Added support for generic XValues in Line charts inside the Dynamic Chart block and for friendly formatted tooltip values in Dynamic Chart blocks (YValueFormatted).
+ Added support for Lava commands to the CalendarLava block.
+ Added support for Lava commands to the Email Form block.
+ Added support for plugins to specify inherited attributes on custom entities.
+ Added support for viewing, editing and filtering on Financial Batch attribute values.
+ Added support for word clouds.
+ Added Swagger UI which will help users visualize and interact with Rock's REST API. It can be viewed at ~/api/docs.
+ Added the ability to add and remove people to security roles directly from their profile record.
+ Added the ability to add site specific Page attributes. These can be configured in the site details and edited on the page details.
+ Added the ability to categorize and secure tags, and updated tag blocks to support additional entities.
+ Added the ability to configure a favicon image specific to each site.
+ Added the ability to copy a Communication Template.
+ Added the ability to easily add schedule exceptions to all schedules within the same category.
+ Added the ability to have chapters in the Contribution Statement Generator.
+ Added the ability to order campuses.
+ Added the ability to save WiFi Presence information about a device and the person associated with that device.
+ Added the ability to search by Birthdate.
+ Added the ability to select existing Content Channel Items when configuring Calendar Event Occurrences.
+ Added the ability to set a link URL on an image in the Summernote editor.
+ Added the ability to set and view the description of categories. This will allow users to set the description of categories for things such as DataViews and Report categories.
+ Added the ability to set default frequency and dollar amounts for accounts passed in the URL to Transaction Entry block.
+ Added the ability to set MEF component attribute values from the web.config file.
+ Added the ability to set Transaction attributes from the Payment object. 
+ Added the ability to use the Prayer Request List and Entry blocks on a Person Profile page.
+ Added the ability to view Lava-based contribution report on the person profile page.
+ Added the 'count' parameter for Lava Entity Commands.
+ Added the following additional financial security actions that can be secured separately from the normal view/edit/administrate actions: Batch Delete; Transaction Refund; Filtering Transaction List by Person.
+ Added the option for login cookies to be able to span subdomains.
+ Added the option to give as a business to the Transaction Entry block.
+ Added the option to have a block show up on all pages of a site for a specific zone.
+ Added three new Lava filters to Add/Get/Delete user preferences. This allows your Lava to save settings about a person to retrieve on future runs. For instance, you may want to save the last time the Lava ran to filter data by on future runs. Additional documentation can be found at http://rockrms.com/lava.
+ Added Transaction History block and better logging of transaction changes.
+ Added transaction settled information if it is available from processor, and updated transaction detail block to require a valid batch Id when adding new transactions.
+ Added two new Lava filters for reading cache objects and resolving the application path.
+ Added UI support for Financial Pledge Attributes.
+ Added URL Lava filter to parse a URL into individual component parts.
+ Added workflow action to start a Job.
+ Added workflow action to write to interactions table.
+ Added Workflow settings to automatically remove old logs and completed workflows.
+ Added Workflow type cache objects to help improve performance of workflow processing.
+ Changed BirthdayPicker to validate (prevent) future dates and added AllowFutureDateSelection option to the DatePicker to prevent selection of future dates.
+ Changed the default cache time on the HTML block to be 0 (none) instead of 3600 seconds.
+ Changed the default ordering on the group list block to be by group name.
+ Extended event registration discount codes to have additional qualifiers.
+ Fixed a line break issue in the Contact Information column of the Business List block.
+ Fixed admin bar to allow editing of footer in DashboardStark.
+ Fixed an exception that would occur when trying to merge a person who does not have a family group.
+ Fixed an issue in the ResolveRockUrl filter where in some cases the theme directory was not being appended to the URL when using ~~/.
+ Fixed an issue that was preventing certain windows from being able to be scrolled using the mouse on touch-enabled devices.
+ Fixed an issue with NMI transactions getting saved without a credit card type.
+ Fixed AttendanceAnalytics issue where first and second visit data was not shown if a check-in group was inside a sub area.
+ Fixed bug where the idle count of the My Connection Opportunities block showed items that were not idle.
+ Fixed controls to scroll correctly when using a mobile/touch device.
+ Fixed displaying of corrupt images in the file browser. Before it would produce an out of memory exception. Now it returns a placeholder image noting that the image was corrupt.
+ Fixed event registrations so that they will no longer occasionally create a duplicate address on a family.
+ Fixed Group Member Remove Workflow Action removing all groups.
+ Fixed issue where CSS files were not getting fingerprinted when using RockPage.
+ Fixed issue with a person's grade being calculated incorrectly when using a non en-US client culture.
+ Fixed issue with check-in welcome screen hanging at 0:00 countdown.
+ Fixed issue with new account entry block that would prevent existing people with a PIN number login from creating a normal database login.
+ Fixed issue with several jobs and a few blocks (and transactions) that were incorrectly using a non-existent "ExternalApplicationRoot" instead of the correct "PublicApplicationRoot" global attribute (which is used in the content of emails).
+ Fixed issue with the Attribute List grid where it would break if the attribute value contained HTML that included </td></tr></table>.
+ Fixed Mailgun webhook bounced email event.
+ Fixed performance issue with transaction download processing when processing a large number of transactions.
+ Fixed Remove Person from Group Using Attribute Workflow Action removing all groups.
+ Fixed SignNow issue with document getting sent to 'Applies To' person instead of 'Assigned to' and an exception that would occur when sending a required registration document.
+ Fixed the connection request Transfer feature to honor the default connector if the new opportunity has a default set for the request's campus.
+ Fixed the Lava Date filter to be precise down to the millisecond. Before, it was only down to the second.
+ Fixed the personal and organizational tags to appear under the correct headers.
+ Fixed the public Event Details block to show the contact person's name correctly when using the default Lava template.
+ Fixed unnecessary creation of Rock database context objects to improve performance.
+ Fixed various controls not honoring the Required property.
+ Improved performance of Content Channel View block by defaulting to not supporting the legacy global attribute Lava syntax, and by allowing output to be cached in addition to the content items.
+ Improved the Group List block to allow filtering on a Group Type's Purpose.
+ Improved the performance of Attendance and Giving Analytics reporting.
+ Improved the performance of the group picker control when selecting a large number of groups.
+ Improved the Person Merge block to handle the selection and merging of family attribute values.
+ Improved the Rock Cleanup job to delete attributes that are associated with an entity that has been deleted.
+ Improved the System Information dialog to show the full Rock version number.
+ Set up XUnit and created sample tests.
+ Updated Account List block so that account attributes marked as 'Show In Grid' will now appear in the grid.
+ Updated Batch Detail block to save new batches with status of 'Open'.
+ Updated BI Analytics to include any Family Attributes that are marked as IsAnalytic.
+ Updated blocks that use 'enable Debug' and removed code related to 'Enable Debug'.
+ Updated Bulk Update to allow launching one or more workflow types for each of the selected people.
+ Updated Communication Template Editor to allow custom Lava fields that can be easily edited, and an advanced mode where Lava fields can be created and integrated into the template.
+ Updated Connection Request to require a follow-up date when state is set to Future Follow Up.
+ Updated Connection Request Transfers to prompt for who the new connector person should be. It now has the option to keep the current connector, use the default, selecting a specific person, or selecting no connector.
+ Updated Connections to allow setting security on Connection Opportunities and update blocks to use security consistently.
+ Updated credit card labels to not assume 'credit'.
+ Updated Data View Detail block to show any groups that use the Data View for group sync.
+ Updated Database Maintenance Job to use an 80% fill factor for index rebuilds, and also have an option for online index rebuild.
+ Updated Date Picker control to make keyboard entry easier.
+ Updated Event Item Occurrences block to support editing attribute values.
+ Updated Group finder to show an info window if no detail page.
+ Updated Group Member Attributes datafilter to prompt for GroupType first and fixed issue that would prevent Group Member attribute filters from working correctly.
+ Updated Group Member List block to include option for displaying date added as an additional column.
+ Updated Group Placement with Events to only hide individuals who are already Active members of an Active group.
+ Updated GroupList block to have an AdditionalColumns block setting that can be used to add additional columns to the grid using Lava for the column value.
+ Updated Lava to cache parsed templates in order to improve performance.
+ Updated Lava to use compiled regular expressions to improve performance.
+ Updated Less compile on the Themes page to now return an error message if a compile error occurs.
+ Updated metric security so that they inherit permissions from their category.
+ Updated My Connection Opportunities block to hide inactive Opportunities that have no open requests and include an 'inactive-item' class when inactive.
+ Updated New Family to be responsive on mobile.
+ Updated payment downloads to report failed one-time transactions (typically ACH) in addition to payments associated with a scheduled transaction.
+ Updated Person Bio to include person's title if they have a formal title such as 'Dr.', 'Rev.', or 'Cpt.'
+ Updated Person Directory block to allow linking to a person profile page.
+ Updated Person Picker to allow it to show more than 20 results, and be displayed in a scroller. Now it defaults to have a max result of 60, but can be configured by changing the "Person Picker Fetch Count" global attribute.
+ Updated Person Tokens to have options of a specific page, a max usage, and expiration date.
+ Updated PostAttendanceToGroup action to add Campusid and allow attendance without group membership.
+ Updated Reassign action in the TransactionList so that Transactions can be reassigned to Businesses.
+ Updated Registration Entry block so that it will resend a confirmation anytime someone edits their registration (previously it would only send on initial registration).
+ Updated Registration Entry to update an existing group member to Active and to not lose family selection on postback.
+ Updated Relationships block so that a custom Relationships group type can be used.
+ Updated Rock Jobs so they will send notification emails.
+ Updated Send SMS Workflow action to allow an attachment.
+ Updated Tags block so it can be put on any page for any Entity.
+ Updated tags to support security and categories.
+ Updated the Attendance Analytics group filter to exclude inactive groups
+ Updated the Background Check to use the new SSN field type rather than the encrypted text field when storing the SSN number, and updated the workflow to blank out the value when done rather than setting it to xxx-xx-xxxx.
+ Updated the Bing location verification service to only consider a match if the entity type returned is 'Address' vs. a 'PostalCode' or 'Neighborhood' type match.
+ Updated the Communication Wizard so that an image or other file can be attached to an SMS message.
+ Updated the CompleteWorkflow action to allow specifying a status to set workflow to when completing (rather than always overwriting with hard-coded 'Completed' status)
+ Updated the custom content area on bio bar to render correctly.
+ Updated the DatePicker control to allow setting a minimum date (so it can be used to prevent past dates).
+ Updated the Device blocks to support editing and viewing device attribute values.
+ Updated the Dynamic Data block and Reports to be able to specify alternate column(s) to use as the recipient for communications, and updated reports to be able to select column values to be included as merge fields on a communication.
+ Updated the Email Form block to provide some additional styling options.
+ Updated the Family View/Edit to respect attribute security.
+ Updated the File Browser to allow downloading of existing files.
+ Updated the Financial Account Detail page to reflect parent/child relationships.
+ Updated the Following By Entity block to support deleting a following record.
+ Updated the Giving Analytics filter to include option to specify that inactive and/or non tax-deductible accounts should be available for filter.
+ Updated the Group Detail block to allow copying a group (security role) and all of its associated authorization rules.
+ Updated the Group Finder block to optionally show the Day Of Week filter as a multi-select checkbox list.
+ Updated the Group Member Add workflow action to allow setting the status of a member when added.
+ Updated the Group Picker to correctly select all children when the child nodes in the tree have not yet been loaded.
+ Updated the internal Registration detail block to accept payments when using a three step gateway (NMI).
+ Updated the Less compiler in Rock to allow for variable overrides. Basically this means that the last implementation of the variable will be the accepted value. This more closely aligns to the pattern of Less.js.
+ Updated the logging of URLs (page view interactions, short links, email clicks, etc.) to also obfuscate impersonation parameters (rckipid) even when it is part of a page route (in addition to when it is used as a query string parameter).
+ Updated the MyWorkflows block to include a new block setting for limiting workflows to specific categories.
+ Updated the name of the MyWorkflowsLiquid block type to MyWorkflowsLava.
+ Updated the Page Settings to show the blocks requesting the page context parameter.
+ Updated the Person Attribute Select field used when adding a person attribute to a registration template form to also indicate the Id and key as a tooltip in order to help identify multiple attributes with the same name.
+ Updated the Person Bio block to allow disabling the following functionality.
+ Updated the Person Follow Add workflow action to support following any entity type, and also fixed a bug where the Lava in the field was not being run before the value was being used.
+ Updated the Plugin install process so that all cached items are cleared after installing the plugin and running its migrations (or install.sql file).
+ Updated the Public Profile Edit block to allow disabling of the First and Last Name fields.
+ Updated the Redirect block to support using Lava for the URL value.
+ Updated the Registration Instance block to show registration start and end dates when viewing details.
+ Updated the Registration Template tree view to allow hiding of inactive templates.
+ Updated the Safe Sender check when sending emails to allow for the option of not updating the From Address if all recipients belong to a Safe Sender domain.
+ Updated the Schedule Builder block to allow selecting all or none of the items in the column.
+ Updated the Send Email workflow action so that sending to a group or security role can be limited to people in a specific role.
+ Updated the Send SMS workflow action to allow using an attribute that is a Memo field type in addition to Text field type.
+ Updated the sending of communications to an email address so that it will track opens/clicks.
+ Updated the server-based check-in label printing to support specifying port number override with 0.0.0.0:9100 syntax.
+ Updated the SMS Communication Entry to check if current user has a number associated with them and if so default to that number.
+ Updated the Sort and SortByAttribute Lava Filters to support sorting in ascending or descending order.
+ Updated the SQL Lava command to allow the results to be used with the other Array filters like Where, Select, Property, etc.
+ Updated the Tooltip text on the family attendance badge to indicate that attendance is only for child when viewing a child's record.
+ Updated the Total Giving report select field to optionally be able to query analytics tables to improve performance.
+ Updated the Transaction Detail block so it does not consider other transactions with same check number (transaction code) as a related transaction or when calculating the default refund amount.
+ Updated the transaction download process to actually create a zero-dollar transaction whenever a scheduled payment fails to process, and added the option to launch a workflow and/or to send an email to the person who created the schedule when this happens.
+ Updated the Transaction download process to evaluate all transactions (not just those associated with a Scheduled Transaction). This allows the download to create status updates for all transactions and allows creating offsetting reversals if necessary for any transaction.
+ Updated the Transaction Entry block to display a warning if the Test Gateway is being used.
+ Updated the Transaction List block to show an account summary when used on a batch detail page, and added a block setting that can be used to hide the account summary.
+ Updated the Universal Search site crawler to be able to crawl pages that have been secured (require login Id and password).
+ Updated the Unsubscribe (Email Preference Entry) to give an option to unsubscribe from any communication lists that the user belongs to. This option will only show if the user is unsubscribing from email that was sent to a communication list.
+ Updated the ValueList field type to support using a SQL statement as its source for custom values in dropdown.
+ Updated the workflow entry block to evaluate route parameter values (in addition to just query string values) when attempting to match workflow attribute values to set.
+ Updated to make a child who is 18+ an adult in their own family when they are moved to a new family.
+ Updated transaction list to display and support filtering of attribute values and the foreign key field.


Rock McKinley 6.10

+ Added a new "In Group(s)" filter for person dataviews that allows filtering just by the group and updates existing filter to be "In Group(s) Advanced".
+ Added a new following event for when a person submits a new public prayer request.
+ Added a new following suggestion type that allows following people that are in any groups that are followed.
+ Added a new Group Attribute report field for Group Member reports.
+ Added a warning to the Protect My Ministry configuration page when configured to use a non-secure URL.
+ Added an option on registration group placement that can be used to set the value of group member attributes when a registrant is placed in a group.
+ Added an option to have new prayer requests default to being public.
+ Added Group Placement Filters to Event Registration.
+ Added new Campus filters for person data views that allow filtering based only on active campuses (existing filters include inactive campuses).
+ Added the ability to filter the calendar Lava block using page parameters.
+ Fixed a bug in Registration List Lava block that would cause exception on the My Account page.
+ Fixed an exception that would occur in Connection Request Detail block if no query string parameters are provided.
+ Fixed an issue that prevented following when multiple followable items exist on single page.
+ Fixed an issue with Transaction Entry block incorrectly determining if saved accounts should be enabled when making a future one-time transaction.
+ Fixed check-in label details to not erase existing merge codes when re-uploading file.
+ Fixed confusing help text on site domains.
+ Fixed Event Template Details from showing duplicate forms in certain situations.
+ Fixed exception that would occur in the Group Registration block if a spouse phone number was left blank.
+ Fixed GroupList block to respect security.
+ Fixed issue with text not being aligned correctly in the ButtonDropDownList control.
+ Fixed issue with the Public Profile Edit block not saving person attribute values when adding new people.
+ Fixed Prayer Request Detail block to set default category correctly.
+ Fixed Scheduled Transaction Summary so that it does not always show a recurring NMI schedule as having a next payment of 'Today'.
+ Fixed the Email Validation on new prayer requests.
+ Fixed the fact that logging of the Twilio transport was on by default. If using Twilio you may want to delete the log file (~/App_Data/Logs/TwilioLog.txt) to regain some disk space on your web server.
+ Fixed the Financial Pledge block to prevent a negative pledge.
+ Fixed the Fundraising Matching block to not display inactive groups.
+ Fixed the Fundraising Participant block to not throw an exception when donor does not have an address.
+ Fixed the Group Finder block to link to detail page correctly.
+ Fixed the Group Finder to display the search button correctly when configured to only filter by campus.
+ Fixed the Group Finder to handle groups with a "\" character in their name.
+ Fixed the My Connection Opportunities to correctly hide the "Last Activity Note" column when configured to not show the note.
+ Fixed the PageListAsBlocks lava template in the Flat and Stark themes.
+ Fixed the required field validation for the RockRating Control.
+ Fixed the Rock REST API missing some 'special' fields when an $expand clause is specified. This fixes an issue where PrimaryAliasId was not included when using GET ~/api/People with an $expand clause.
+ Fixed the Rock REST API to include attributes when an $expand clause and loadAttributes are specified.
+ Updated the ToCssClass Lava filter to handle complex string that could contain special characters. 
+ Updated Attendance Analytics to exclude inactive campuses by default in the campus filter (This can be changed to include inactive campuses using a new block setting).
+ Updated check-in type configuration to only display phone number details if a search type of phone number is configured.
+ Updated DynamicReport to include the DataFilters from any parent dataviews that it uses.
+ Updated exception logging to obfuscate sensitive form fields or any field name that contains 'nolog'.
+ Updated File Uploader to clean invalid characters from filenames before storing on file system.
+ Updated following suggestions so that it does not send an additional reminder if the number of days for a reminder is left blank.
+ Updated My Connections block to display a total count of each opportunity type.
+ Updated Person Search to better handle businesses with a comma in their name.
+ Updated the Add Family block to optionally require a birthdate for children, a phone number for adults, and be able to launch workflows for each adult, child, and or family.
+ Updated the admin toolbar to allow those with edit rights on a page to edit the page properties.
+ Updated the Attendance Analytics block so it can optionally be configured to use a group picker instead of listing every group with a checkbox.
+ Updated the Campus field type to use the "Include Inactive" configuration setting when generating the UI for filtering attributes of this type.
+ Updated the Campus Pickers to load campuses automatically (with option to override list).
+ Updated the Communication Entry block so that it does not allow removing all recipients when in simple mode.
+ Updated the Connection Opportunity Details block to allow selecting multiple groups at a time when configuring placement groups.
+ Updated the Connection Request block to optionally display the total number of requests for the given campus and opportunity types that user is authorized to view.
+ Updated the Content Channel detail block to prevent adding Item attributes with same key that the content channel type has defined.
+ Updated the Content Channel View block to check security and not allow viewing of an item that doesn't belong to configured channel.
+ Updated the ContentChannelView block to require a value for Channel.
+ Updated the Contribution Lava Statement block to more accurately show pledge data.
+ Updated the cron expression for the 'Download Transction' job to run every day at 5am. Before it only ran on weekdays.
+ Updated the Group Detail block to optionally be able to prevent setting the campus to an inactive campus.
+ Updated the Group Finder block to optionally be able to display a campus column for the groups in the grid.
+ Updated the Group List Block to sort correctly on Date Added.
+ Updated the Group Picker control to allow easily selecting all groups, or all child groups.
+ Updated the Group Registration block to only count active members when determining over capacity.
+ Updated the Group Type block to prevent adding a circular reference for inherited group types.
+ Updated the Login block to honor the locked/unconfirmed settings for external accounts.
+ Updated the Person Merge block so that the invalidating of logins when people with different emails are merged can be disabled.
+ Updated the Person Picker to show the person's age and spouse name when hovering over the name.
+ Updated the PostAttendanceToGroup workflow action to add campusid and allow attendance without group membership.
+ Updated the Public Profile block to support a View Only mode.
+ Updated the Tooltip text on the family attendance badge to indicate that attendance is only for child when viewing a child's record.
+ Updated the Transaction Detail block to optionally show transaction detail attributes in the list of accounts for the transaction.
+ Updated the transaction entry block so that it does not default to 0.00 and uses place-holder text instead.
+ Updated the Transaction List block to optionally be able to display the Total Results account summary even when used within context of a batch, person, registration or scheduled transaction.
+ Updated the Transaction Matching block to support only showing accounts that already have amounts allocated to them by default.
+ Updated the Workflow Entry block so that it will reset browser view to top of page on each form when workflow has multiple user entry forms.
+ Updated work flow processing to not process any workflows/actions that are configured to be inactive.


Rock McKinley 6.9

+ Added GivingId to the Attendance Analytics Excel Export
+ Updated the active users block to show hints at color dot meanings
+ Fixed an exception that would occur in Attendance History block if a valid group id was not used.
+ Fixed an exception that would occur when attempting to configure Transaction Entry block with only an ACH gateway.
+ Fixed issue with not being able to logout when the current URL contains an Encoded Key parameter.
+ Fixed misspelled word in description of field in Check-in Label.
+ Added 'ReopenBatch' as a security action on Batch Detail
+ Fixed the "My Assigned Workflow" lists so they would not show a duplicate workflow when an assigned activity had more than one active form.
+ Fixed the IdleRedirect block so that it will not redirect prior to the configured idle seconds.
+ Fixed the person picker to correctly display 'Pending' record status.
+ Fixed the Pledge List block so that when exporting to Excel it includes all of the columns displayed in the list.
+ Fixed incorrect HTML in the default podcast series detail Lava file.
+ Fixed an issue with the Check-in Details page (under Attendance Analytics), sometimes not showing all of the correct groups.
+ Improved the GroupDetailLava block so that when editing an existing group member, the person picker field is disabled.
+ Fixed the Content Channel View block so that if Query Parameter Filtering is enabled, the items are not cached (regardless of Cache Duration setting). The Lava template will still be cached based on Cache Duration setting.
+ Fixed the NMI payment gateway to prompt for the "Name on Account" field correctly when adding a bank account (ACH) transaction.
+ Added new My Connection Opportunities Lava block
+ Added block setting to allow prayer requests to be public by default.
+ Added option to Batch List to hide the Accounts column
+ Added Campus Filter to the top of My Connections block that will filter both the Summary and the Grid by Campus
+ Fixed Date Attribute showing a required validation error when 'Use Current' is checked.
+ Fixed select columns on grids so that their selection will be persisted through a postback.
+ Fixed the date range filter in Pledge List block.
+ Updated Connection Requests to show the status dots for each connection request in the grid.
+ Added optional 'PersonDetailPage' to the New Family block that can be used to navigate to a custom page instead of the default ~/Person/{PersonId} route


Rock McKinley 6.8

+ Fixed an issue with an early version of the v6.7 install missing an updated file.


Rock McKinley 6.7
 
+ Fixed exceptions that would appear on default home page after installing Rock v6 due to an invalid filter on the content blocks (Fixes #2155).
+ Fixed issue with event details being duplicated on the Calendar Event Detail block when using the Stark theme (Fixes #2245).
+ Updated the ZebraPhoto Lava Filter so that it can be used multiple times on the same label.
+ Added 'Save Then Add' and 'Save Then View Batch' as buttons when adding a new transaction to a batch.
+ Updated Registrations to use Sliding Date Range filters for Registrations, Registrants, and Payments. This also fixed an issue where the date range filters were sometimes not using the correct dates and not including recent registrations and payments.
+ Updated the Login block so that when a user logs in using an external provider such as Facebook or Google, their login will be remembered and they won't need to login again on every visit.
+ Added the ability to check-in using any type of device that supports keyboard wedge. This includes bar code scanners, proximity card readers, etc.
+ Added Campus (of Account) as a filter for Transaction List.
+ Added new features to Check-in that will automatically select options based on the person's last check-in (with ability for user to change the selection).
+ Added the ability for people to check-out.
+ Added option to have transaction matching so that additional optional accounts can be easily added per transaction.
+ Improved check-in so that when searching by name, the name field will have focus when screen is displayed (Fixes #2222).
+ Fixed issue with check-in that was preventing people from checking in when used with a culture that formats date as dd/mm/yyyy like en-GB or en-AU (Fixes #2212).


Rock McKinley 6.6
 
+ Updated the Registration Entry block so that it no longer inserts nicknames into incorrect form fields (Fixes #2040).
+ Updated Registration Instance list to include the registration's confirmation email when exporting to Excel (Fixes #2209).
+ Fixed an exception that would occur if a person without a valid record type tried to register for an event.
+ Updated the reassign action in the TransactionList so that transactions can be reassigned to businesses.
+ Fixed an issue with downloaded transactions getting assigned to incorrect schedule if multiple PayFlowPro accounts are being used for contributions (Fixes #2234).
+ Added an option to have a predefined list of Batch Names when creating a new batch.
+ Fixed giving badge not filtering by account correctly.
+ Fixed an exception that would occur in the ContributionStatementLava if no pledges were given, and updated DateRange logic to be consistent.
+ Updated the ContributionStatemementLava so that GivingGroupId logic is consistent for Pledges and Contributions.
+ Updated transaction matching so that accounts that have non-zero amounts will always show regardless of account filter.
+ Added a 'Select All' action to the Account Picker control.
+ Updated the group detail block so that unlisted phone numbers are not displayed (Fixes #2085).
+ Fixed the GroupMemberAttendanceAddWorkflow action to use the schedule attribute value setting (Fixes #2125).
+ Fixed an issue with the new account entry block that would prevent existing people with a PIN number from being able to create a normal database login (Fixes #2204).
+ Fixed an issue with Bulk Update not updating deceased people (Fixes #2108).
+ Fixed group member max count role enforcement not enforcing correctly when adding a new family.
+ Added 'Edit Connection Status' and 'Edit Record Status' as security actions on the edit person block.
+ Added an additional security action to person edit block to control who can edit the combine giving value.
+ Fixed MyAccount block throwing an exception when trying to edit/crop the person image (Fixes #2001).
+ Fixed MyAccount block adding Demographic changes twice to history (Fixes #2078).
+ Updated the workflow SetAttributeValue action to save an encrypted field type value correctly (Fixes #2167).
+ Fixed the incorrect reporting page showing when canceling or adding a category in reports (Fixes #2056).
+ Fixed ContentChannelItem data view filters not showing attributes that are defined at the content channel.
+ Updated the ContentChannelView block to use new decluttered storage for Lava commands (Fixes EntityCommands not working in ContentChannelView).
+ Added support for the "suppress-bounce" event type in the Mailgun webhook (Fixes #2082).
+ Updated SignNow integration to handle an invalid filename (Fixes #2207).
+ Removed links to missing images from Stark theme.
+ Upgraded FontAwesome to v4.7. Been looking forward to the bathtub icon... http://fontawesome.io/icon/bath/


Rock McKinley 6.5

+ Fixed an issue with ValueList and KeyValueList block settings not allowing user to add new values.
+ Fixed a backward-compatibility issue with migration helper method that was affecting plugin installs.
+ Fixed an exception that would occur when saving a change to a family.


Rock McKinley 6.4

+ Updated the Edit Family block to create history correctly when adding a new person (Fixes #1726).
+ Fixed mispelled 'amount' in NumberBox validation message.
+ Fixed business photo reference.
+ Fixed the GroupRegistration block to not erase matched person's data if the user did not fully fill out the form.
+ Fixed issue with not being able to delete a non note (Fixes #2123)
+ Fixed SignNow issue with document getting sent to "Applies To" person instead of "Assigned to" and an exception that would occur when sending a required registration document (Fixes #2176, Fixes #2177)
+ Updated the Communication Entry block to validate the future send date/time (Fixes #1999)
+ Updated giving analytics to display accounts in configured order rather than alphabetically.
+ Added option to show NickName when adding a new family
+ Update the Facebook authentication to deal with their "[Oauth Access Token] Format" change (Fixes #2117).
+ Fixed blocks and stock lava that use the Google Static Map API to include the Google API Key since it is now required (Fixes #1991).
+ Updated the Global Attribute Filter so that it passes original merge fields when resolving contents of attribute (Fixes #2162).
+ Updated check-in to support numeric-only security codes.
+ Added option to display phone numbers, gender, and/or spouse on the person search results.
+ Added optional phone and email fields to the new person dialog when editing an existing family. These are not displayed by default.
+ Updated transaction matching to include 'Finish' button that will mark batch as being open instead of pending, and added a campus filter to limit selected accounts by campus.


Rock McKinley 6.3

+ Updated the Protect My Ministry integration to include email address when sending a request to PMM.
+ Updated PayFlowPro to update the saved transaction code whenever a payment is made using a saved payment method. This is because PayPal only allows transaction codes that are less than 12 months old.
+ Update Relationships block so that a custom Relationships group type can be used.
+ Added new Giving person badge.
+ Added new Attended Group of Type person badge.
+ Updated the Transaction Entry block to allow anonymous person to still give as a business.
+ Added Features to the Transaction Entry Block.
+ Fixed issue with not being able to unselect all the phone types on the directory block (Fixes #1983).
+ Added logic to the Registration Entry block to prevent a registration if login is associated with a business (only occurs if bad data was created during import) (fixes #1772).
+ Fixed issue with not being able to do a Place Elsewhere workflow on a member who is associated with a registration (Fixes #2022).
+ Updated Registration Entry block to persist the discount code when hitting Previous button (Fixes #2015).
+ Fixed issue with Current Family Member option persisting incorrectly (Fixes #2074).
+ Updated Lava commands to clean up Lava variable space to keep things less cluttered for the user.
+ Added new group data view filter that pulls all groups that meet the criteria of a group type data view.
+ Update Actions to use helper method for saving attribute values so that it works with either a workflow or activity attribute.
+ Update Sliding Date Range picker to clear controls when set to a null value.
+ Update Benevolence Request action to set attribute value correctly.
+ Add workflow action to create Benevolence Result.
+ Added additional merge fields available to check-in labels that summarize the schedule/group type/group/location selected for each person.
+ Fixed registration issue when using NMI that would result in not being able to submit a payment if first attempt was unsuccessful (i.e. wrong card number entered).
+ Added the ability to search by Birthdate.
+ Added the ability to use the Prayer Request List and Entry blocks on a Person Profile page.
+ Updated the Person Directory block to display families in order by family name rather than by id.
+ Updated the Protect My Ministry block so that when changes are saved, it does not requires a Rock restart, or editing of background check components before changes are used. Also fixed issue with error message not being displayed correctly in workflow entry form.


Rock McKinley 6.2

+ Fixed issue with icons printing incorrectly when using ipad checkin and client printing (fixes #1971).
+ Changed the list of event's registrants (shown in RegistrationInstanceDetail block) to display Grade field (when used) as Grade instead of Graduation Year (fixes #1946).
+ Updated the Registration Template detail block so that it will no longer change a person attribute to a registration attribute (fixes #1856).
+ Fixed the regsitration entry block to set the person's group member status to that of the registration template even if they are already in the group (previously it only set it when adding a new group member).
+ Added two additional block settings to allow restricting the Group Registration block even further (fixes #1799).
+ Changed campus field to be optional on the Background Check Request workflow and created Cancel type button (fixes #1701).
+ Changed Group Attendance Detail block's lava template setting to be optional (fixes #1952).
+ Updated Statement Generator to use Public Account Name vs the Standard Account Name.
+ Updated Person Attributes (Adults and Children) in Block Settings to not have a default checked when none selected (fixes #1880).
+ Updated Address Detail in person profile to hide Threshold fields (fixes #1665).
+ Fixed Address not saving on public person profile block.
+ Changed EditGroup and EditPerson block to mark family group inactive if all members people's record status are inactive (fixes #1103).
+ Changed EditGroup block to prevent a person's status from changing if they were formerly inactive/deceased (fixes #1887).
+ Changed EditPerson block to inactivate a person's family groups if, when changing the person to inactive, the family has no members except inactive people (fixes #1103).
+ Corrected "or" operator to be valid Lava (fixes #1912).
+ Added option to configure check-in areas/groups by Birth Date in addition to Age and/or Grade.


Rock McKinley 6.1

+ Changed lava conditional to use the 'or' (||) operator in the PodcastMessageDetail lava (fixes #1912).
+ Added a block setting to the Contribution Statement Lava to exclude transactions from a specific currency type. Default is to show all. This allows you to remove 'Non-cash' transactions from your on-line contribution statements.
+ Data View category actions are now visible when permissions/rights are inherited by a parent category (fixes #1777).
+ Corrected Family Analytics job to update first/second visit dates when the Set Visit Dates flag is enabled (fixes #1833).
+ Changed label editor to allow for height and width to be specified in decimal inches as well as it now uses PW and LL values to determine initial width and height (fixes #1846).
+ Benevolence workers can now upload files to requests (fixes #1877).
+ The Related Transactions (shown on the transaction details screen) with the same transaction code now only appear if they are from the same person (fixes #1766).
+ Used the same SVG width and height for children.
+ Corrected Range Slider control to use correct index (fixes #1845)
+ Added a job to send emails to a dataview.
+ Pressing the browser's back button after merging duplicate records from the duplicate finder no longer throws an exception (fixes #1787).
+ Viewing the Registrants of an event no longer defaults to filtering by 12th grade if the registration instance had a Grade option on the form (fixes #1816).
+ Fixed problem where 12th grade would not stay selected when entering a new child via the New Family operation (fixes #1834).
+ Fixed issue with wrong attribute values being used when exporting selected grid rows (vs exporting all rows)
+ Enhanced Person Search performance (#1798).
+ Improved error reporting in the payment download job.
+ Updated Group Registration block to allow linking by guid (instead of id), and update it to not allow registering for a security role.
+ Fixed issue with attendance date calculations in Family Analytics procedures.


Rock McKinley 6.0

CALENDAR
+ Updated the Event Item Occurrence List By Audience lava file to use the external url link if it exists.
+ Added an .ics download button to the event detail page.
+ Updated calendar detail block lava to provide well formatted shareable summaries for Facebook and Twitter. 

CHECK-IN
+ Fixed scrolling issues in check-in for windows 10 multi-touch devices (fixes #1660).
+ Added the ability to order GroupLocations (namely for check-in) in the event that a check-in workflow action wants to make decisions based on their order.
+ Added new block for editing label files that allows easily viewing and printing of incremental changes.
+ Updated check-in to save the value that was entered to search for family, and the family that was selected when saving the attendance record.
+ Added options to check-in for hiding photos and excluding inactive people.
+ Updated the Welcome check-in block to allow changing the 'Check In' button text with a block setting and CSS.
+ Updated label printing so that if person is checking into two or more grouptypes that share the same label configured to be printed once per person, that it will only print once per person. Note if grouptypes have different labels, both types will still be printed. (Fixes #1755).
+ Updated check-in to add option of preventing people from checking into the same service time more than once.
+ Updated attendance model to allow more fields be accessible through Lava.

CMS
+ Added ability to have site-specific routes.
+ Added new setting on the site that will require all pages on the site to load as HTTPS and will redirect if the page is loaded under HTTP.
+ Prevented PageView records from being created when the user does not have security rights to the page and they are redirected to the login page.
+ Added a Person Directory block.
+ Addded a block to allow editing of user's account and family from the home page.
+ Updated the list of acceptable security protocols for external SSL connections. 
+ Added page copying to the Page Map block.
+ Upgraded Bootstrap to 3.3.7.
+ Fixed issue with Site Domain list not being cleared when cache is cleared.
+ Added property to the page to add a CSS class to the body tag. 

CONTENT CHANELLS
+ Updated Content Channel Items to support hierarchy so that items can have children and parent items from the same or other configured channels.
+ Added additional 'Request Assigned' and 'Request Transferred' connection workflow trigger types, and fixed issues with workflows getting launched when they shouldn't have.
+ Added block settings to the content channel items list to show/hide various columns.
+ Added content channel feed to ContentChannelItem Detail and List to provide alternative to query parameters.
+ Fixed 'content channel view' block to no longer break when invalid Lava is provided (now it politely tells you that your Lava doesn't make the grade).

FINANCIAL
+ Added the ability to view Lava based contribution report on the person profile page.
+ Fixed Giving Analytics query when using "did not give during date range" pattern filter with no accounts selected.
+ Added a Person filter to Transaction List.
+ Updated Giving Analytics to display transactions from any TaxDeductible account rather than just transactions that have a type of "contribution".
+ Pledge List block now has a setting to allow it to only show only pledges for the currently logged in person.
+ Added Printable Benevolence Request Summary (#1684).
+ Updated the transaction list block to filter accounts shown in the totals/summary at the bottom based on the accounts selected in the filter. This allows you to see a total at the bottom for just the accounts you selected in the filters.
+ Added the whole transaction entity as a merge field option for receipts.
+ Disabled the word merge button on the Transaction Report block. This block is used on the external site where Word merging is not needed or desired.
+ Added new features to Benevolence including: Reorganizing the screen; Adding the ability to have attributes on benevolence requests; Adding the ability to attach up to 6 documents on a request; Adding a new 'Provided Next Steps' field
+ Updated StatementGenerator and CheckScanner to work with Tls 1.2. (Fixes #1611)
+ Updated Giving Analytics block to improve performance.

GROUPS
+ Updated GroupFinder and GroupRegistration blocks to honor a group's GroupCapacity and GroupTypeRole MaxCount (Fixes #1275).
+ Added ability to Group Attendance detail block to select/unselect all members at once by clicking 'Member' header.
+ Added campus filter option to group finder.
+ Fixed Lava used for the Group Toolbox. The edit member feature was not working as intended.
+ Updated the Member Attended Group workflow trigger type on groups and group types to also optionally set an attendance date attribute.
+ Add option to GroupAttendanceDetail to allow a Lava template to be used for rendering

LAVA
+ Added new Entity, Execute, SQL, and WebRequest Lava Commands.
+ Added new REST endpoint for resolving Lava.
+ Added New Lava filter for checking the security of a model "HasRightsTo'. 
+ Removed LavaIgnore on AttributeValue EntityId.
+ Made Metric Partitions accessible via Lava (fixes #1644).
+ Added new Lava filter 'WithFallback' to eliminate the need for conditional testing of null or empty variables.
+ Added new Break and Continue tags to Lava.

PERSON/FAMILY
+ Added ability to have Family attributes.
+ Added optional 'Birthdate' column to the person search results.
+ Fixed exception that would occur if searching for person with blank string.
+ Grades can now be set when adding new family members to the PublicProfileEdit block.

REGISTRATION
+ Updated Registration Entry block to support signing required digital documents inline during registration.
+ Fixed issue with event registration where discount code and amount paid are not cleared if user navigates backwards and unselects optional fees.
+ Added the ability to specify a workflow to launch when a registration is completed. Can be configured on the registration template or instance.
+ Fixed security on the registration instance detail block to stop people from adding/deleting registrants when they did not have proper security (Fixes #1732).
+ Added additional fields to the event registration registrant Excel export (First Name, Last Name, Datetime Created, and Home Address).
+ Fixed issue that prevented being able to move a registration to a different instance if the target instance did not have a group configured and selected in the move.
+ Fixed security issue with allowing people to edit payments on a registration.
+ Added validation to registration entry block to ensure person applies a discount code they enter and added server-side validation to prevent paying an amount greater than the balance due.
+ Added option to event registration templates to optionally allow registrants to select existing family members when registering for an event.

REPORTING
+ Updated Metrics so that a MetricValueDateTime can be specified when using SQL as the Source. (Fixes #1666).
+ Fixed the Giving Amount person data view filter to not include children when combining giving.
+ Added new Group Member Report Select for Group Campus.

WORKFLOW
+ Added Lava capabilities to Pre/Post HTML of workflow entry attributes.
+ Added option to SQL workflow action to allow processing to continue even if SQL results in an error.
+ Added Activate Activity in Other Workflow action and Activate Activity in Other Workflow on match action.
+ Added a Workflow Action to Trigger a New Workflow.
+ Added a Workflow Action to add a Benevolence Request.
+ Added the ability for workflows to have sequential ids based on a prefix associated with the type of workflow ( for example, IT requests could have ids like 'IT00001', 'IT00002' )
+ Added a new workflow action to run Lava in a better UI.
+ Added better error handling to the Process Workflows job so that one exception does not stop the job from processing additional workflows.
+ Fixed issue with the Group Member Attendance Add action not doing anything if an attendance date attribute was not selected.
+ Fixed the delay workflow to delay correctly when datetime or day of week values are used for the delay

MISC
+ Fixed the communication recipient block to include emails that were marked as being opened in addition to those that were just marked delivered.
+ Added ability to display notifications from Spark.
+ Updated the address control to use placeholders instead of labels and moved country to be first field (fixes #1628).
+ Added the ability to customize status bar and opportunity summary tiles using Lava in the My Connection Opportunities block.
+ Updated the database authentication provider to use BCrypt when hashing new passwords, and to convert existing HMACSHA1 hashed passwords to BCrypt next time each user logs in.
+ Added new Startup interface that custom plugins can use to run custom code during Rock startup.
+ Fixed the currency field type to return a formatted value that is a currency.


Rock McKinley 5.5

+ Updated the RockUpdate block to deal with servers that are reporting the .Net framework incorrectly.


Rock McKinley 5.4

+ Fixed issue with CalendarLava block not using correct date range when the Date Range filter is used (Fixes #1771).
+ Fixed issue with a new communication not requiring an approval when recipients are added manually (Fixes #1768).
+ Added better error handling to the Process Workflow job so that one exception does not stop the job from processing additional workflows.
+ Updated check-in label printing so that if person is checking into two or more grouptypes that share the same label configured to be printed once per person, that it will only print once per person (Fixes #1755).
+ Fixed security on the registration instance detail block to stop people from adding/deleting registrants when they did not have proper security (Fixes #1732).
+ Updated the FilterGroupsByGradeAndAge so that it does not remove/exclude groups that don't have a defined age or grade range.
+ Fixed exception that would occur when renaming a block from the zone block list.
+ Fixed issue with check-in not refreshing attendance cache correctly when used to calculate threshold values.
+ Updated giving/registration to strip any non-numeric characters from credit card number before submitting (Fixes #1728).
+ Updated the Rock Update block to require that Microsoft .NET Framework 4.5.2 or greater be installed before Rock can be updated to next version.


Rock McKinley 5.3

+ Fixed the date filter on pledge analytics block (Fixes #1617).
+ Fixed bug with 'Filter Groups by LastName' check-in workflow action which occurred when a group did not have the necessary attributes.
+ Fixed memory issue with check-in welcome screen.
+ Made Entity TypeId and TypeName accessible in Lava (#1691).
+ Fixed issue with attribute filters incorrectly filtering results and not showing correctly when used in simple view mode.
+ Fixed one-time future payments with NMI to use correct frequency and number of payments.
+ Fixed exception that would occur when using NMI gateway and trying to create a scheduled transaction with a monthly frequency.
+ Updated phone number model to force number being saved correctly whenever it is added/updated.
+ Removed LavaIgnore on AttributeValue EntityId.
+ Fixed 'content channel view' block to no longer break when invalid Lava is provided (now it politely tells you that your Lava doesn't make the grade).
+ Fixed scrolling issue with windows 10 multi-touch devices in check-in (fixes #1660).
+ Fixed issue with Family Checkin not displaying the current service time when selecting area/group/location.
+ Updated the list of acceptable security protocols for external SSL connections. 
+ Fixed issues with the Connection workflow actions (Fixes #1649).
+ Updated Dynamic Data block to hide grid filters when more than one grid is displayed. (Fixes #1642).
+ Fixed issue with some workflow action type attributes not saving value correctly if using the RockTextOrDropDownList and multiple row textbox.
+ Fixed BirthDate showing a year of 0001 if a person's record doesn't have a BirthYear specified.
+ Disabled the word merge button on the Transaction Report block. This block is used on the external site where Word merging is not needed or desired.
+ Fixed exception that would occur if multiple less variables with the same name existed.
+ Fixed Group security so that all users do not have View access by default.
+ Updated Event Item Occurrence List By Audience lava file to use the external url link if it exists.


Rock McKinley 5.2

+ Fixed bug in individual check-in that would cause Rock to restart if user clicks the Back button (due to stack overflow error).


Rock McKinley 5.1

+ Fixed issue with Family check-in incorrectly navigating to individual check-in page when clicking 'Back' (Fixes #1620).
+ Fixed issue with workflow triggers not getting started (Fixes #1623).
+ Fixed display of hierarchical group types in check-in manager.
+ Fixed display of group type path on the location schedule screen in check-in manager mode (Fixes #121).
+ Fixed positioning of Save button on check-in admin schedule view (Fixes #1609).
+ Updated the saving of attributes so that it does not clear foreign key values and the created by information (Fixes #1596).
+ Fixed new route for check-in to use correct page.
+ Updated the Check-in Manager to allow changing the location threshold value.
+ Updated family check-in to fix issue with schedules not getting selected correctly when only one schedule option is available.
+ Fixed issue with family checkin that would result in first service's selection getting cleared if checking child into multiple services and each service had different location options.
+ Fixed issue with family check-in that would result in ability level being asked twice if selecting multiple service times, and different locations options were available for each service.
+ Fixed issue with View possibly getting created with wrong schema name.
+ Fixed the check-in Grade Required attribute to have correct entity qualifier
+ Fixed exception that would occur if any PreHtml or PostHtml text included any opening and closing brackets (i.e. Lava or Style definitions).


Rock McKinley 5.0

API
+ Added two new REST Endpoints: ~/api/Groups/ByLocation and ~api/Groups/ByLatLong.
+ Added file upload REST endpoint for BinaryFilesController.

CALENDAR
+ Added ability to group event occurrences in the calendar by event.
+ Added ability to follow an Event Item.
+ Updated core Calendar.lava and EventItemList.lava to display the date range of an Event instead of just the first date/time if the event occurrence spans multiple days.
+ Updated CalendarItem.lava to show all the dates of an event instead of just the first one if the event occurs on multiple dates.
+ Added context object to EventItemListLava block.
+ Updated EventItemListLava block so that campuses is not required.
+ Updated calendar to no longer lose selected date when switching between view modes (Fixes #1392).
+ Added 'External Website Ads', and 'Service Bulletin' content channels to public calendar configuration if no content channels are already configured.
+ Fixed exception that would occur when using the Event Item Occurrence List By Audience Lava block without a date range filter (and recurring events without an end date exist).

CHECK-IN
+ Added 'Family' check-in functionality to allow checking in multiple family members at the same time.
+ Update Check-in Configuration to make it easier to modify settings and edit a large number of areas/groups.
+ Updated the check-in welcome message that is displayed when there are not any more active schedules so that it reflects that it is only considering schedules for the current day.
+ Updated the check-in manager to exclude people who have checked out
+ Updated Check-in Manager to display the Attendance Code for each person currently checked into a location
+ Updated the Check-in Manager's person profile check-in history to include the security code for each check-in.
+ Added 'GroupMembers' as an available merge field on check-in labels that will contain the group member record for the groups that person is checking into if they are a member of those groups. This Lava would print the role of person in group: {% for groupMember in GroupMembers %}{{ groupMember.GroupRole.Name }}{% endfor %}
+ Refactored Attendance Analytics to improve performance
+ Updated Attendance Analytics to support selecting a specific set of group types to display and added campus filter the Check-in Group block.
+ Updated Attendance Analytics to include the location (room) that person last checked into.
+ Added birthdate as field that is included when exporting Attendance Analytics.
+ Added schedule as an additional optional filter in Attendance Analytics block
+ Fixed issues with attendance history filters not persisting
+ Updated Attendance History block to only consider check-in start date when using a date-range filter (Fixes #1524).
+ Added new FilterGroupsByGradeAndAge workflow action that will match groups first by grade, and only check age if group or person does not have a grade.

CMS
+ Added a new 'Link List Lava' block that makes it easy to add/edit/secure and format a list of links.
+ Added a new block for building forms that allows a logged in user to enter person attribute information and optionally launch a workflow.
+ Added new DynamicChart block which will display a linechart based on the results of a SQL query.
+ Added a utility block that allows you to logout a user when they visit a page.
+ Added a block that allows you to remove a person from a group based on query parameters.
+ Updated the EmailForm block to allow using Lava merge fields in the From, From Name, and Subject fields.
+ Added option to Login block to customize the text that is shown on the "New Account" button.
+ Added new Person Context Setter that can be used to set a Person context for a Page or Site.
+ Added setting to the Campus Context setter to default to the current person's campus (default is disabled).
+ Added new fields to allow sites to add content to the page header and also to enable/disable site indexing.
+ Added the ability to control the X-Frame-Options and Content-Security-Policy HTTP headers per Site so that trusted domains can be allowed to embed the site (such as iframes).
+ Fixed issue with routes not getting removed from routing table when a page is deleted (Fixes #1555).
+ Added the ability to set page and block security from the Page Map (Admin Tools > CMS Settings > Page Map). No more locking yourself out...
+ Added the ability to set email preference in the AccountEdit block.
+ Added "Next Available Username" message to the User Logins block to help an admin figure out the next available username in situations.  For example: jsmith1, jsmith2, jsmith3...
+ Added the ability to require phone number on Account Entry block.
+ Updated security so that inactive security groups/roles are ignored when checking security.
+ Updated FontAwesome to v 4.6.1
+ Upgraded google analytics tracker to latest.

CONNECTION OPPORTUNITIES
+ Added ability to add comment to a new Connection Request.
+ Added 'Days Until Request Idle' property to ConnectionTypes so that each one can have its own idle time.
+ Added feature that will scroll the screen to the connections grid when a connection type is selected and the grid is not visible.
+ Added ability to require a placement group before connecting a connection request.
+ Changed the MyConnectionOpportunity block to show different notification counts based on the state/status of the requests.
+ Added the ability to add Lava to the Connection Summary and Descriptions fields for display on the external site.
+ Added option to My Connection Opportunities to show last activity note.
+ Added a last activity filter to My Connection Opportunities.
+ Fixed checking of connection state in 'Connected' workflow trigger.
+ Updated connection request detail to display current requests placement group as an option even if the connection type no longer is configured for that group.

CONTENT CHANNELS
+ Fixed Content Channel Item List to show attribute columns when attribute is associated with the channel (vs just the channel type).

FINANCIAL
+ Updated Rock to support using a three-step payment gateway (NMI) for registration and transaction entry.
+ Added the ability to filter transactions by campus.
+ Added option to transaction list on person profile to allow reassigning transactions to another person.
+ Updated transaction detail block to prevent editing a transaction if it belongs to a batch that is closed.
+ Added support to Transaction Detail block for updating/editing FinancialTransaction attributes.
+ Updated Scheduled Transaction Summary to include all schedules with same giving leader id.
+ Updated the recurring transaction download job (and block) to automatically create an offsetting reversal transaction if a failed transaction is downloaded and that transaction was previously downloaded as a successful transaction. Also added a new workflow that will be launched whenever a reversal transaction is created.
+ Updated Transaction Entry block to save the comment field value to the transaction summary field when creating new transactions.
+ Removed the Bank Account Name field from Transaction Entry screen as it is not needed to process ACH transactions.
+ Updated Giving and Attendance chart data grids to display descriptive header text instead of "SeriesId". Also included GL Code in grid when graphing by Account in Giving Analytics grid.
+ Updated Transaction Matching block to only display and allow selecting of active accounts.
+ Updated Giving Analytics block to hide the 'Is First Gift' and 'First Gift' columns when viewing first time givers.
+ Updated Giving Analytics block so that when creating a merge document from list of people, the documents are created correctly (with data).
+ Added Last Gift dates to Giving Analytics.
+ Updated Pledge Analytics block to include any child accounts when calculating contributions made to an account.

GROUPS
+ Added option to Group Tree so show child group counts or member counts.
+ Added the ability to set unique group capacities.
+ Added flag to group type to indicate that attendance in a group of that type counts as attending a weekend service.
+ Added option to GroupMap to show all child groups in the map.
+ Added real person photos to the group member list block. This is not only an enhancement but also fixes what appears to be a UI bug when no photo exists.
+ Added block setting to show/hide campus filter on group member list.
+ Added the option to filter the  group member list block by gender.
+ Added birthdate and age to the Group Member list Excel export.
+ Added optional First/Last attendance dates to group member list if group takes attendance and block setting is configured to show dates.
+ Updated Group Member List block to exclude inactive members when sending communication.
+ Added option to Group Member List block to hide the 'Move to another group' button.
+ Updated groups so that group member status changes are logged to person history.
+ Added option to GroupDetailLava to hide the 'Inactive' option for Group Member Status
+ Added an option to GroupFinder to sort the results by Distance and limit number of groups with a PageSize option.
+ Added the option to filter the group list block to a specific group type id.
+ Changed default date range for Group Attendance to 3 months instead of a year (date filter can still be changed per user).
+ Added campus filter to group attendance list. This will filter the attendance count and rate values by the people whose campus is the selected value.
+ Added an optional campus filter to the Group Attendance Detail block.
+ Fixed issue with Role specific Group Requirements still showing warning icon for members of group with a different role.
+ Updated Group Finder to prevent adding attribute filters for attribute field types that do not support filtering (Fixes #1561).
+ Fixed display of Group Name when using option to display group path and selecting a specific group type (Fixes #1525).

LAVA
+ Added new SortByAttribute Lava filter.
+ Added new FromMarkdown Lava Filter.
+ Added new RegExMatch Lava filter for providing regular expression matches in Lava.
+ Added new SundayDate Lava filter.
+ Added new Campus Lava Filter which will return the Campus or Campuses that a person belongs to.
+ Added new FromJSON Lava Filter.
+ Added new EscapeDataString Lava Filter.
+ Added new PersonByGuid Lava filter.
+ Deprecated the ZebraPhoto Lava filter in favor for new ZebraPersonPhoto which has new optional fields to adjust image brightness and contrast.
+ Added an option to include all dates when using the DatesFromICal Lava filter.
+ Added Lava filter to return group member info if a person is in a specific group.

PERSON/FAMILY
+ Added new badge for showing if a person is in a group with a specific group type purpose (e.g. where the purpose is 'Serving'.)
+ Updated the AddFamily, FamilyMembers, and EditFamily blocks to work with other group types besides family. This allows being able to add and edit groups on person profile page in addition to, or instead of just the family group.
+ Added setting to display the middle name of a person on the bio block.
+ Updated formatted age display to work properly for people less than one year old.
+ Added option to the Notes block to limit the entry/display to a specific subset of note types for the configured entity type.
+ Updated Notes so they will automatically render any Markdown in the Text into HTML.  Also, any text that looks like an http link will be rendered as a clickable link
+ Update Person History block so that history items are not displayed for any category that current user does not have authorization to view.
+ Changed "Combine Giving With" to show the family name along with the list of names of the family members when editing a person
+ Allow the toggling of showing inactive campuses on the Add Family block using a block setting.
+ Updated saving of a person record to check if anniversary date was updated and if so make same update to spouse's record.

PRAYER
+ The PrayerRequestEntry block's Save Success Text can now use Lava in order to display different messages based on the prayer request (or its Category) and the block can also initiate a Workflow with the PrayerRequest as the Entity if configured.

REGISTRATION
+ Updated Event Registration to allow each instance of a template to specify a different cost.
+ Updated registration detail block to allow moving registration to an inactive instance, and also allow optionally moving registrants to new group.
+ Updated registration detail block to allow specifying the person who is making a payment rather than automatically assuming it is same person who did the registration.
+ Added Grade as an available field in event registration forms.
+ Added ability to copy EventItemOccurrences.
+ Added an "In Group" filter to Event Registrants grid to help identify registrants that haven't been added to the target group yet.
+ Updated Registration list so that registrations can be sorted by Total Cost or Balance Due without causing an exception.
+ Added the ability to set a batch name prefix on an event registration.
+ Added new feature to keep saved registrations from being updated on the external website.
+ Updated Registration Entry block so that it will always create a person record for the registrant (instead of just when there is a cost), and updated the registration list on the Registration Instance detail block to be a person grid which allows emailing the people who did the registration.
+ Added the ability to follow a Registration Instance.
+ Fixed an issue preventing Email, Campus and Fee columns from not exporting into Excel.
+ Added Group Placement functionality to registration instance that allows for easily adding registrants to a set of child groups.
+ Added checkbox to registration entry summary that allows a logged-in user to specify whether the email on their account should be updated with the confirmation email address that they enter when registering.

REPORTING
+ Added new dataview filters and report fields to help create dataviews and reports off of the workflow table.
+ Fixed the person giving filter and select in reporting so that the end date range is inclusive (Fixes #1490).
+ Added additional 'Date Added' criteria to the In Group(s) Person Data Filter.
+ Added 'Not In Other Dataview' filter.
+ Fixed date filters to be inclusive of the entire day of the upper and lower limit.
+ Added Report Field 'LastLogin'.
+ Added Report Field 'Parent's Email Address'.
+ Added Parent's Email data select.
+ Updated Total Giving Amount report select, Giving Amount datafilter, and First Contribution datafilter to use a Sliding Date Range picker instead of a regular date picker.
+ Added Campus and GroupType to the list of available report fields for Groups.
+ Added options to Member List data select for groups to limit by GroupRole and status.
+ Fixed performance issue in "In Group of GroupType" data filter to be around 2x faster.
+ Fixed performance issue in "Distance From" data filter to be around 3x faster.
+ Updated the dynamic data block with better filters.
+ Updated dynamic data filter controls to default true and moved to custom settings.

WORKFLOW
+ Improved performance of workflow type editing.
+ Updated workflow action names to follow a more consistent pattern. If you use workflows a lot and have become accustomed to the action names you may hate this before you LOVE it.
+ Updated workflow type editor so that new workflow types default to processing every 8 hours.
+ Added additional workflow detail/entry pages below the My Dashboard pages so that breadcrumb trail is more intuitive.
+ Added a 'Regular Expression' option for use with Workflow Action Filters (i.e. Run If) to give more control with text matching.
+ Added support for datetime comparisons to the workflow action compares.
+ Added new workflow action that will set a Person attribute value based on First Name, Last Name, and Email values. If only one person in database is found matching those fields, that person will be used, otherwise a new person will be created.
+ Added new workflow action to set the properties of a person (name, birthdate, etc.)
+ Added new workflow action to get the spouse of a person.
+ Added new workflow actions for setting person phone and address.
+ Added new workflow action to add a note to the group member.
+ Added new workflow action to set GroupMember Note.
+ Added new workflow action to send email with events that will send an email and then activate actions based on recipient opening and/or clicking items in email.
+ Added two new workflow actions to remove a person from a group.
+ Updated the Set Person Attribute workflow action so that it correctly writes person attribute changes to the person's history.
+ Updated the background check workflow action to check for an error or failure that might have occurred when submitting background check and display error to user with option to resubmit.
+ Updated the DISC request workflow action to warn if the individual does not have an email and persist the workflow only if it's about to send (there by not keeping a lot of unneeded workflows around). Also updated the DISC badge to highlight if someone has already request a DISC from the individual in the last 30 days.

MISC
+ Fixed person search to allow better searching on complex names like Mary Ann Van Dellen (Fixes #1518).
+ Updated CommunicationDetail block to show which page the communication originated from if it was generated from a grid somewhere.
+ Enabled custom mobile keyboards for mobile devices when using NumberBoxes, UrlLinkBoxes, and EmailBoxes.
+ Updated the Email Exception Filter so that exceptions can be filtered by the exception type, source, message and stacktrace in addition to any http request server variable.
+ Added new option for requiring a user login to change their password on next visit.
+ Added new search service that allows searching for businesses.
+ Added the ability to bulk update Tags.
+ Fixed issue in Merge Templates that was causing Header, Footer, and any remaining global lava fields in the Body not to get merged (Fixes #1396).
+ Added "Campuses" and "Context" as available merge fields for Doc and HTML Merge Templates.
+ Fixed copying of existing communication so that the recipient's additional merge values are copied as well.
+ Updated the Communication block's support for additional merge fields (Lava) to support objects in addition to just string values.
+ Update Communication Entry block so that if a different template is selected, the From, From Email, etc fields are not cleared.
+ Updated the Transaction Entry and Registration Entry blocks to validate new passwords against configured pattern when user is creating a new login in order to save their payment account information (Fixes #1371).
+ Added the ability to copy and share selected filter criteria for the giving and attendance analytics blocks.
+ Improved performance of the Person Duplicate Finder.
+ Increased the query timeout values for attendance and giving analytic reports.
+ Updated the FileSystem binary file type provider to save the relative path to file when saving a new binary file using this provider.
+ Added logic to check for any new person names or addresses being entered with all lower or all upper case characters and if so, fix casing so that first character is uppercased and remaining characters are lowercased.
+ Updated following notifications to exclude deceased people (Fixes #1558).
+ Added new SendGroupEmail Job.
+ Added setting to Group Sync job to enable forced password resets on new logins that are created during the sync.
+ Updated the address verification process to preserve first service's standardization results even if second service is used for geocoding.
+ Updated the address verification job to correctly retry previously failed verifications and to differentiate between an error calling a service and a non-match returned by the service.
+ Updated Smarty Streets address verification to save the barcode value.
+ Update cleanup job so that it does not delete unconfirmed logins.
+ Updated Campus, Campuses, and Connection Opportunity attributes/fields to exclude inactive items, and added optional config option to include inactive items (Fixes #1413, Fixes #1540).
+ Added new Phone Number field type.
+ Added new Address Field Type.
+ Added new Address Attribute.
+ Fixed issue with radio button lists automatically selecting first value when they are set using an empty/null value (Fixes #1334).
+ Updated KeyValueList field type to display values correctly when not using a defined type (Fixes #1522).
+ Added 'Warning' property to Rock Controls similar to 'Help' Property.
+ Noted individuals who are inactive on the person picker search.
+ Added new Markdown Field Attribute that can be used to render Markdown into HTML (see http://commonmark.org).
+ Added new Binary File Types Field Attribute and Field Type.
+ Updated person picker results to display a person's connection status.
+ Added option to the Person Attribute/FieldType so that the 'Enable Self Detection' option of the person picker can be enabled.
+ Added ability to set qualifier column/value on the Categories block to further filter entities that categories belong to.


Rock McKinley 4.6

+ Fixed the display of line breaks in Rock Shop ratings.
+ Fixed the person giving filter and select in reporting so that the end date range is inclusive (Fixes #1490).
+ Fixed issue with not being able to remove a workflow type from a connection opportunity if requests have already been created that use that workflow type (Fixes #1488).
+ Fixed issue with check-in configuration not clearing cache for the areas (i.e. which labels are associated with area).
+ Updated Connection Request so that members of a campus-specific connection group can now add connection requests (Fixes #1446).
+ Fixed column headers not being responsive.
+ Updated Mailgun component to deal with bounced events having incorrect formatting (Fixes #1478).
+ Updated the Device Detail block to prevent and warn when attempting to add a new device with same name as an existing device instead of causing an error (Fixes #1437).
+ Fixed exception that would occur when trying to delete a group type that was a child of another group type (Fixes #1415).
+ Updated the Benevolence Request Detail block to list case workers in alphabetical order (Fixes #1494).
+ Fixed the message that is displayed on the Merge People page when user navigates directly to page (instead of initiating request from a grid). Message will not indicate that they are not authorized to submit a request (Fixes #1501).
+ Fixed Spouse name field on reports to exclude any deceased spouses (Fixes #1498).
+ Updated Group Member List block so that the Group setting is not required (typically group is passed on the query string) Fixes #1500.
+ Fixed issue when adding a known relationship to an inactive person and it not creating the inverse relationship.
+ Fixed issue with new family block saving phone numbers without a country code and added migration to fix all missing country code values.
+ Fixed issue where only first recipient would receive email when it included one or more attachments and the default binary file type was using a custom storage provider.


Rock McKinley 4.5

+ Updated the Protect My Ministry (PMM) background check integration to check for a valid response from PMM when submitting the initial request and add logging of both the request and the reply.
+ Updated workflow processing so that any errors that occur during a workflow action are always written to the workflow log regardless of log settings.
+ Fixed exception that would occur if new person was trying to signup for a connection request and organization only has one campus.
+ Updated the code editor to correctly escape html values.
+ Updated the email preferences block so that it logs any changes a user makes to their preference (including record status/reason changes) to their history.
+ Fixed an issue with cropping person images when using a custom storage provider.


Rock McKinley 4.4

+ Updated Check-in to correctly cache device configuration data so that it is not specific to a particular point in time.
+ Fixed paging on grid.
+ Fixed Facebook login for organizations using a newer Facebook App API Version v2.4 or v2.5. This change was also tested to be backwards compatible with their API Version as old as v2.0 (Fixes #1419).
+ Fixed exception that would occur when adding a new location (Fixes #1408).
+ Fixed issue with deleted channels causing exception on content page (Fixes #1406).
+ Added id header text to fix export to Excel issue (Fixes #1391).
+ Fixed issue with security when changing if a group is a security role or not (Fixes #1399).
+ Fixed issue with registration saving a blank person/registrant if user navigated away from registration and then used browser navigation to return before submitting the registration.
+ Fixed Tag Report so that after sorting it deletes the correct item (Fixes #1398).
+ Fix issue with Group Requirements when the Expire In Days setting has not been set (Fixes #1389).
+ Fixed error in the People REST endpoint (Fixes #1388).
+ Updated Lava debug display to only display each object type/id once, and limit iterations to first two items (Fixes #1365).
+ Changed z-index of the admin toolbar to keep it from being hidden when editing page zones. (Fixes #1397)
+ Changed the ConnectionRequestDetail block to allow a request to be Connected even if there is no placement group.
+ Fixed Communication Entry block so that the Subject is cleared if a selected template does not specify a subject (Fixes #1393).
+ Fixed issue where creating a new communication from a dataview caused the block to timeout before the configured database timeout occurred. (Fixes #909).
+ Update communication send job so that it ignores any inactive mediums and/or transports (Fixes #1527).
+ Fixed issue when trying to add group attendance and location filter is set to a parent location (i.e. campus/building) vs. an actual room location.
+ Fixed Registration Entry so that it does not create person notes until registration and payment was successful.
+ Updated the Group Attendance Detail block so that if adding attendance for a particular schedule, the attendance record gets created with the correct start time.
+ Updated the Connection Search block so that it now has access to the complete opportunity model including the opportunity attributes. Note, there is a small change in the Lava for this update. If you use a custom theme you'll want to grab a new copy of 'OpportunitySearch.lava' as you should use the property of 'PublicName' instead of 'Name'.


Rock McKinley 4.3

+ Fixed issue with check-in "randomly" not allowing check-ins
+ Fixed issue with registration saving a blank person/registrant if user navigated away from registration and then used browser navigation to return before submitting the registration.
+ Fixed Registration Entry so that it does not create person notes until registration and payment was successful.
+ Fixed Tag Report so that after sorting it deletes the correct item (Fixes #1398).
+ Fixed location details not saving attribute values.
+ Fixed issue with security when changing if a group is a security role or not (Fixes #1399).
+ Fix issue with Group Requirements when the Expire In Days setting has not been set (Fixes #1389).
+ Fixed error in the People REST endpoint (Fixes #1388).
+ Updated Check-in to correctly cache device configuration data so that it is not specific to a particular point in time.
+ Updated Lava debug display to only display each object type/id once, and limit iterations to first two items (Fixes #1365).
+ Changed z-index of the admin toolbar to keep it from being hidden when editing page zones. (Fixes #1397)
+ Changed the ConnectionRequestDetail block to allow a request to be Connected even if there is no placement group. In the future this can be controlled via a new 'RequiresPlacementGroupToConnect' flag on the opportunity type.
+ Fixed Communication Entry block so that the Subject is cleared if a selected template does not specify a subject (Fixes #1393).
+ Fixed IE double-tap zoom issue in check-in themes
+ Fixed issue where creating a new communication from a dataview caused the block to timeout before the configured database timeout occurred. (Fixes #909).
+ Fixed issue when trying to add group attendance and the location filter is set to a parent location (i.e. campus/building) vs. an actual room location.
+ Updated the Group Attendance Detail block so that if adding attendance for a particular schedule, the attendance record gets created with the correct start time.
+ Connection Search block now has access to the complete opportunity model including the opportunity attributes. Note, there is a small change in the Lava for this update. If you use a custom theme you'll want to grab a new copy of 'OpportunitySearch.lava' as you should use the property of 'PublicName' instead of 'Name'.


Rock McKinley 4.2

+ Updated Registration Entry so that discount code is still applied correctly when user returns to make a payment on existing registration.
+ Updated Registration Entry to correctly delete a registration if it was just created but an error occurred during save of registrants or attributes.
+ Fixed exception that would occur when copying a registration template that included an additional form(s) (in addition to the default form) and that form contained a person or group attribute (Fixes #1356).
+ Fixed registration issue that would result in registered person getting added to target group even if registration failed due to payment issue.
+ Fixed issue with group attendance not recording attendance correctly and continuing to send reminders.
+ Fixed issue where a person's tags would not always survive a merge.
+ Fixed performance issue with the Giving Amount dataview filter due to how it generated its query.
+ Fixed so that Connection Opportunities in the Connection Request transfer window are now ordered.
+ Added real person photos to the group member list block. This is not only an enhancement but also fixes what appears to be a UI bug when no photo exists.
+ Updated the Workflow Type Detail block so that it does not time out when trying to remove an activity type or action from an existing workflow type that has a significant number of workflows already created.
+ Updated the Rock Shop configuration to work with usernames and/or passwords that may have special characters.
+ Updated the Send Email and the Send System Email workflow actions to allow Email attribute field types for the from and to addresses (in addition to text or person field types)
+ Removed unnecessary clearing of authorization cache whenever a group member was added/edited/removed from a security group.
+ Updated the Group Type Detail so that if a group member attribute is removed, it is also removed from any registration template that was configured to use that attribute.
+ Updated the Twilio webhook so that if it gets an IOException when writing to log file that it will wait and try again a few times before causing an error.
+ Fixed issues with numeric attribute field comparisons on data views (Fixes #1377).
+ Fixed issue with Group and Role picker not saving the selected role (Fixes #1380).
+ Updated the Transaction Entry and Registration Entry blocks to validate new passwords against configured pattern when user is creating a new login in order to save their payment account information (Fixes #1371).
+ Fixed bug where event name not showing in page title (Fixes #1373)
+ Fixed issue with attendance block not saving the location or schedule for a new attendance being entered.
+ Fixed issue in Statement Generator where some transactions might not show up if it is split into multiple accounts and at least one of those accounts wasn't included in the filter.


Rock McKinley 4.1

+ Updated communication entry block so that binary file attachments are not stored as temporary files (and possibly deleted up before an email is sent in the future).
+ Fixed issue with payment details getting cleared when a scheduled transaction is updated.
+ Fixed issue with a reporting lava column being blank when exporting to excel (Fixes #1349).
+ Updated the cache timeout on check-in location attendance counts. Was previously changed from 1 min to an hour, but an hour is too long to accurately reflect current attendance in a location.
+ Fixed the Defined Value field type filter when the option to display multiple values is enabled.
+ Updated REST methods so they do not pass their rockcontext object (without proxy creation enabled) to the cache methods that rely on being able to use navigation properties to load child collection properties.
+ Updated Registration Entry so that it does not block existing registrations from editing an existing registration when the instance is full.
+ Fixed exception that would occur when attempting to add a new registrant who already belonged to the group associated with the registration.
+ Fixed exception that would occur if a fee was deleted from a registrant template that had existing registrants already using the fee. It will now remove the fee from the registrants.
+ Fixed exception that would occur when unselecting a fee from a registrant.
+ Fixed baptism badge html markup.
+ Fixed an issue with Connection signup where new records were being created even when logged in.
+ Updated how registration person field values are serialized so that objects can be unserialized correctly.
+ Fixed security issue on metric detail that allowed non-authorized users to edit/delete a metric (Fixes #1338).
+ Fixed issue with undelivered emails appearing in a person's communication history list (Fixes #1344).
+ Fixed Pledge Analytics to display business names (Fixes #1335).
+ Fixed issue with date picker not working correctly on New Family block (Fixes #1333).
+ Fixed the workflow SMS Send action to support using 'Person' merge field when recipient is a person, group, or security role attribute (Fixes #1309).
+ Fixed issue with transaction matching when masked account number was blank.
+ Fixed an issue preventing Email, Campus and Fee columns from exporting to Excel.


Rock McKinley 4.0

+ Added support for processing refunds.
+ Added Twitter Authentication Service.
+ Added Alternate Placement feature to groups.
+ Added following to Groups.
+ Added Connection Management feature to help with the process of finding and placing volunteers into teams.
+ Added a new core Group Requirement Type and Data View for Background Checks
+ Added new Attendance Analysis attendee grid that shows detailed information on a person's attendance, which can be filtered in some handy ways
+ Added Google Login provider
+ Refactored giving to allow multiple financial gateways to be configured using the same payment processor with different settings.
+ Added setting to display the middle name of a person on the bio block. By default the setting is set to false.
+ Changed default security on new sites so that they do not require user to login.
+ Updated content channel items to correctly sort items when sorting on numeric and/or date attribute values.
+ Updated the attendance list/detail blocks to allow deleting and changing the date of any occurrence that is not tied to a specific schedule
+ Added the ability to remove a person from a family (vs. moving them to a new family) if they are already in another family.
+ Updated the category tree view (used by data views, reports, etc.) to allow a user to edit items if they have security for a specific category rather than requiring edit rights to the block or page.
+ Updated the person merge to remove any duplicate previous names after merging records.
+ Added the option for administrators to disable verification services for a particular location on the person profile page (Fixes #1286).
+ Added additional Lava to output a phone number given a person input.
+ Added Lava filters to get the parents or children of a given person.
+ Updated the Content Channel View block to support filtering item by a route parameter as well as a query string parameter.
+ Updated the ContentChannelView blocks to only do query string parameter filtering if option is set on block (Fixes #1267).
+ Added option to Group Type to show each group member's Connection Status in the group member list.
+ Added option to the header of the TransactionList block to show images or summary. The option can be made visible in block options.
+ Added 'Inactivate Child Groups' feature to Group.
+ Added REST EndPoint GetGraduationYear for getting a graduation year from a grade offset.
+ Added Inactive Reason Note to EditPerson page.
+ Fixed person search for people with two first names (e.g. Mary Ann Smith).
+ Added options for sending a contribution receipt whenever someone gives a onetime gift online, or when a recurring transaction is downloaded for their scheduled transaction.
+ Added option to the group list to display the full path to group.
+ Added new Last Note data select for Person reports.
+ Added "Move Group Member" feature to Group Member Detail.
+ Added new File Manager block that can be used to manage and upload files.
+ Added Campus Filter to Group Member List.
+ Updated the display of admin bars to only show if current user has administrative rights to the page or a block on page, or edit rights to a block that has custom settings (HTML Detail, Content Channel View), instead of always showing the bar if user has edit rights to any block on page.
+ Added new workflow action to allow setting the initiator from an attribute value
+ Updated the communication recipient list on person history to allow senders to view any communications they created.
+ Added account summary to the transaction list block.
+ Added new "Format as Link" option to Image field types which will format the image to navigate to a full size image when the image is clicked.
+ Fixed issue with not being able to move transactions from one batch to another on Firefox.
+ Added option to Statement Generator to limit statements to a Dataview.
+ Added new badge that will display if a note exists on a person's record that is marked 'Is Alert'.
+ Updated content channel view block to refresh the view after cancelling out of the edit dialog (Fixes #1245).
+ Fixed issue with payment downloads updating the scheduled transaction detail amounts instead of the transaction detail amounts (Fixes #1250)
+ Fixed updating of scheduled transaction to not create account detail records with $0.0 amounts.
+ Removed the Output Cache Duration option from block properties as this functionality is not fully functional (Fixes #1096).
+ Added the ability for communication approvers to edit a communication before approving it.
+ Added new PrayerRequestListLava block type.
+ Updated the Send Email and Send System Email workflow actions to support multiple "to" addresses.
+ Added account list to the scheduled transaction list block.
+ Added PageRedirect lava filter.
+ Fixed database locking contention that would occur when saving a workflow with a new attribute and a user entry form (and database was configured with snapshot isolation turned on).
+ Fixed the Transaction History block to show transactions correctly for the correct person (not everyone's) when the current person does not give with their family (Fixes #1243).
+ Updated grid export to excel to export numeric values correctly (not as text values).
+ Fixed Dynamic Data Report issue when Selection URL contained more than one column name (Fixes #1238).
+ Renamed the ForeignId field to ForeignKey (string field) and added new ForeignId (int field) and ForeignGuid (Guid field) fields to every table/model.
+ Added column (and filter) to Attendance History block to indicate whether a person attended the group or not (Fixes #1184).
+ Updated SMS communications to require From and Message values (Fixes #1171).
+ Updated Bulk Update to allow selecting the note type when adding a new note for people (Fixes #1165).
+ Updated the processing of scheduled payments to also update the status (Fixes #1145).
+ Added a link to Report Detail to navigate to the associated data view (if user has edit rights to report).
+ Updated the Security dialog to read values from DB instead of cache (Fixes #903).
+ Updated the Transaction Entry block to use public account names on confirmation and success views (Fixes #1220).
+ Updated the family edit to allow adding new people with unknown gender.
+ Added new Person Merge Requests page under Data Integrity.
+ PrayerRequestEntry block can now accept the text (if URL encoded) of a request via the Request parameter.
+ Fixed BirthdayPicker to display controls in correct order for ShortDateFormats that include day/month format specifiers other than "dd" or "MM" (Fixes #942).
+ Updated Person Merge so that it shows a "Merge Request Submitted" if the person is only authorized to View.
+ Added Saved Account List block that can be used by users to delete their saved accounts.
+ Fixed issue with ordering of accounts (and their amounts) on transaction entry block when accounts are added by user in different order than the default account order (Fixes #1219).
+ Modified date picker to work more like the time picker. You can now click on the calendar icon and it will open. Also added logic that if you click the calendar icon it will select the text of the date which allows you to start typing right away to change the date. This should now be consistent with the time picker. (Fixes #1189).
+ Added toggle button to Show/Hide Data View and Report Results.
+ Updated the merge fields available to exception notification system email to include a 'Person' field for the current person.
+ Added a feature to the data view detail block to show what reports and other data views are in use.
+ Updated the testing of workflow action filters to correctly compare numerical values (Fixes #1205).
+ Added setting to the transaction list block to limit transactions to one or more transaction types (i.e. only contributions on person contribution tab ).
+ Added additional filters and ability to sort to the scheduled transaction list.
+ Updated Check Scanner so that the user chooses the Transaction Source on the scanning prompt page instead of the options page. This list is limited to the Transaction Source defined values that have 'Show In Check Scanner' set to true.
+ Added setting to transaction list that allows limiting accounts in filter to be only active accounts.
+ Added column to batch list that summarizes account totals, and fixed batch notes to display on batch list.
+ Fixed the Delete Workflow action.
+ Added workflow action to create a connection request and added new controls, field types, and attributes for selecting connection opportunity, request, and status.
+ Added new Floor and Ceil Lava filters for rounding (Fixes #1193).
+ Updated email communication medium to only allow valid email address in From Address field, and updated communication send job to continue processing communications when one communication has an issue.
+ Added new Children transform for dataviews that will transform a list of parents to a list of the children of those parents
+ Updated the person save so that when a person is inactivated, their group members status will be updated to inactive also for all the groups they are members of unless the group's group type has the new 'Don't Inactivate Members' option set.
+ Added framework for adding following events and suggestions.
+ Added a Copy command for Reports.
+ Added a Copy command for Data Views.
+ Added new Lava filters for getting a person object from a PersonId and PersonAliasId
+ Added additional Payment details model/table to store the details of a payment such as currency type, credit card type, and name on card, expiration date, and billing address so that in the future functionality can be added to allow users to update their information without having to recreate scheduled transactions.
+ Added mobile redirect options to site configuration to allow automatically redirecting mobile clients to a different page or external url.
+ Updated workflow processing so that if an action fails on initial processing the workflow would still be persisted if configured to do so.
+ Added new EnableSelfSelection option to the Person Picker
+ Fixed issue with attribute value having html encoded text when they shouldn't (e.g. global attribute in email subject)
+ Updated parent check-in label to include the last name in addition to nick name
+ Modified the allergy and legal notes on check-in label to truncate instead of writing over itself.
+ Updated the grid's page setting size preference to be grid specific instead of for the entire site
+ Upgraded to FontAwesome 4.4 (66 New Fonts)
+ Updated the sending of system and workflow emails to also optionally write a communication history to the recipients profile record.
+ Fixed Exception List showing incorrect "Last 7 days" count for exceptions with empty Site Name. (Fixes #1174).
+ Fixed issue with Twilio transport sending to the same recipient more than once (Fixes #1077).
+ Rock Duplicate Finder no longer displays duplicates following merge (Fixes #985).
+ Known Relationships with deceased people now show in the Known Relationships block (Fixes #1164).
+ Added Graph By Location option to Attendance Analytics
+ Added new Communication History block to the person profile page
+ Changes to the order of person profile badges now update the badge bar without having to clear the cache (Fixes #933).
+ Google Maps link now searches for a named location's address rather than its name (Fixes #1093).
+ Added setting to the Category Tree View block to allow setting the icon to display for items that do not have an IconCssClass property.
+ Fixed issue with recurring specific dates not saving start times (Fixes #1147).
+ SendEmail workflow action now works with attributes of type Group (Fixes #1167).
+ Merging a person with a security role now refreshes that role (Fixes #1137).
+ Added ability to delete a batch and all its transactions from the Check Scanner utility.
+ Fixed issue with pre/post delete triggers not firing when a column qualifier was specified.
+ Added new Lava filter to return the possessive form of the string (Ted -&gt; Ted's, Charles -&gt; Charles').
+ Changed the Group Details block to require 'Administrate' access to view/edit Group Member Attributes.
+ New Lava filter for returning a set of next date/times from an iCal string
+ Made Batch Id visible in Check Scanner Utility
+ Added ability to delete a scanned transaction in the Check Scanner Utility
+ Added new Lava filter for returning information about the current page.
+ Added navigation improvements to MyWorkflows block
+ Added new Lava filter for returning information about the current page.
+ updated Check Scanning to prompt the user when a Bad MICR read or duplicate is detected, which the option of Uploading it as-is.
+ Added public description html and is public fields to accounts.
+ Fixed the MonthYearPicker so that it maintains current selection through postbacks.
+ Fixed GroupList Active Status filter and display to consider both the Group Status and the Membership Status when determining if a Group Member is active. Also modified to show inactive Memberships in a lighter-weight font and sorted last by default. (Fixes #1140).
+ Updated Edit Family to automatically save the address if an address is in edit mode when you save the family.
+ Workflow triggers can now be fired when a qualifier changes-to or changes-from a value for pre-save trigger types.
+ Added new lava filter PluralizeForQuantity
+ The Communications Entry block will now send an email to Communication Approvers when a new communication requires approval.
+ Add 'Between' as an option for all date dataview filters that lets you choose a date range or a sliding date range
+ Changed IsDeceased on Person from a Nullable to Not Null. Any records that have a NULL value for IsDeceased will be marked as IsDeceased=False
+ Users can now search for businesses in PersonPicker (Fixes #1047).
+ Sites can now be deleted via an extra confirmation (Fixes #937).
+ Added Spatial Indexes to the Location table which should significantly speed up the DistanceFrom datafilter plus any other GeoSpatial queries
+ Added options to the "In Group" person data filters that let you specify "Include Child Groups" with options of "Including Selected Groups" and "Include All Descendants" (which default to true if "include child groups" is selected)
+ Datafilters for booleans now have an "Equal To" and "Not Equal To" comparison option. Handy for nullable booleans and boolean attributes. For example, if you want to filter on 'Has a Pet Cat', 'Not Equal to True' will include people that either have False or don't have a value for 'Has a Pet Cat'
+ Added new REST endpoints for editing Attribute Values.
+ Added new REST endpoints: api/People/AddNewPersonToFamily and api/People/AddExistingPersonToFamily
+ Added display of current room counts to the check-in manager login screen.
+ Updated attendance analytics block to include DataView filter.
+ Added optional setting to FindFamilies checkin workflow action that allows phone search to only include families with a phone number that ENDS WITH the value entered during check-in rather than all the families with a phone number that CONTAINS the value entered.
+ Updated new family control to use correct phone type labels rather than hard-coded 'home' and 'cell' phone labels (Fixes #941).
+ Updated checkin to persist the theme selection in browser's local storage so that if theme is changed, it is remembered on subsequent starts.
+ Added new Lava filter to transform a property into a key value pair.
+ Added workflow action to post attendance to a group.
+ User Logins can now be added from the Security &gt; User Accounts page (Fixes #888).
+ Refactored note types so that new (and existing) note types can be configured and secured.
+ Added new workflow action to set an attribute for a group member.
+ Added new workflow action to set a person attribute to the leader of a provided group.
+ PersonName linked field in Report now appears correctly and is an Excel hyperlink (Fixes #1063).
+ Added a calendar feature for events.
+ Added Gender, Age, and Grade to the Check-in Manager person profile display.
+ Fixed issue with Check-in Manager that would cause navigation options to disappear when switching campus context.
+ Updated checkin to prevent multiple options (family, person, group type, etc.) to be clicked on.
+ Added new workflow action to create a person note.
+ Improved check-in performance.
+ Improved performance of Check-in manager.
+ Turned off auditing by default (Can be turned on by adding a boolean 'EnableAuditing' global attribute).
+ Added 'Public' option for Groups that can be set to false to make the group a private group, which will prevent it from showing in the group finder.
+ Updated Transaction List, Batch List, DataViews and Reports so that the grid will only fetch the records it needs. This will help the grids load quite a bit faster.
+ Improve performance of attendance list/detail blocks
+ Add Location path to attendance list/detail blocks
+ Add option to add new attendees to attendance detail block
+ PersonPicker now provides some feedback if the user is unauthorized to search (Fixes #1042).
+ Added Lava filter 'DateAdd' to add a number of days/hours/minutes/seconds to a date.
+ Added new PIN Authentication that can be used as a special "PIN Only" login for check-in management.
+ Changed Group Tree View so groups will be shown regardless of the 'Show In Navigation' group type setting if specific group types are configured in the block settings.
+ Updated group editor to prevent changing parent group type to a group that does not allow the current group's type.
+ Updated check-in so that people without a birth date cannot check-in to groups that have an age range requirement (this can be reverted through a new 'Require Age' workflow action setting.)
+ Added the option to the group type check-in block to optionally automatically select all group types and then display the groups for all matching group types.
+ Added Lava filter for randomizing arrays (useful for shuffling ads).
+ Added SetPageTitle Lava filter.
+ Added the ability to change the page title from the Dynamic Data block.
+ Updated the Transaction List Liquid block to update status of a giving profile correctly when inactivating the profile.
+ Added full location path to location column in Check-in Schedule configuration.
+ Fixed communications through SMTP and Twilio to clean up any special MS Word characters that might have been pasted into the Subject line, or Body, or Message.
+ Added a special Attendance Grid to AttendanceAnalysis that will show a person's attendance per Week/Month/Year as checkboxes.
+ Added new Lava filter to get groups that a person has attended.
+ Added ReplaceLast Lava filter useful for replacing the last comma with 'and'.
+ Added HtmlDecode Lava filter.
+ Added new Lava filter to return the group(s) of a specific type that person lives within the geofenced boundary of.
+ Added new Lava filter to return the members of a particular role in the group of a specific type that person lives within the geofenced boundary of (i.e. neighborhood area leader).
+ Added new Lava filter to return the nearest group for a person.
+ Modified Group Detail block to only display attributes with "Show in Grid" flag enabled.
+ A person's previous last names can now be edited in edit person.
+ Moved Giving Group and Previous Names UI to be in an "Advanced Settings" panel in edit person.
+ Searching for a person will now look for any previous last names that the person has had.
+ Added 'Date Added' to group members.
+ Added new Send Birthday Email job.
+ Added Jobs to calculate Metrics and calculate Group Requirements
+ Added new 'Format' Lava filter to help format numbers.
+ Fixed issue where Data View comparison "Is Not Blank" incorrectly returns Null values as matches. (Fixes #948).
+ Added new Transaction Yearly Summary Lava block that can be used as standalone or as a person profile block.
+ Added the group leader as a tooltip on the Geofenced By Group person badge.
+ Fixed issue where Excel Export from Grid incorrectly shows Defined Value Fields as Guids (Fixes #935).
+ For REST, changed DateTimeZoneHandling to Unspecified instead of the default of RoundTripKind since Rock doesn't store dates in a time zone aware format. Since Rock doesn't do time zones, we don't want Transmission of DateTimes to specify TimeZone either. This fixes an issue where the batch date of scanned checks could be off by a few hours in certain edge situations, and prevents similar issues from happening.
+ Return the created entity after a REST POST to improve the efficiency of 3rd party software using the REST endpoint
+ Added an Active|All toggle filter to the Group Tree View that will either show only Active (the default) or All groups. The filter can be hidden in the Group Tree View block settings which will hide the filter and only show Active groups in the treeview.
+ Added a new person badge type that can display any groups of a particular type that have a geo-fence location that surrounds the person's family address (geopoint).
+ Added a new 'Volunteer Check-in Area' to make it easier to configure serving teams for check-in.
+ Updated the check-in configuration block's schedule and group/location editors to work more consistently with groups that are also configured and managed using the normal group viewer.
+ Added new Workflow Actions that can be used to add a Person to a Group
+ Added a new Lava filter for person objects to get all groups of a selected type that user belongs to.
+ Updated the Workflow Detail block to check both the block security settings and the workflow security to determine if user is authorized to edit a workflow (If user has edit rights on either, they will be allowed to edit the workflow)
+ Fixed Entity Type filtering on attribute category block and updated attribute list block to correctly display categories when adding/editing an attribute.
+ Updated workflow form processing so that if user has edit rights to workflow type, they will be able to process workflow forms even if they are not the one that form is assigned to.
+ Updated group attendance list/detail blocks to support groups with multiple locations and schedules (i.e. groups used for check-in)
+ Added a binary file picker attribute
+ Updated notes block to allow backdating notes
+ Updated Group Type Detail block to display child group type list and picker in correct group type order.
+ Updated prayer request comment list default sort order to be descending date order instead of ascending (show most recent first).
+ Updated Communication List block to use Create Date for date range filtering instead of Reviewed Date.
+ added a PATCH method to the Rock REST API. Patch allows any entity's properties or a subset of those properties to be updated.
+ Improved the performance of the Communication History block
+ Timepicker now figures out the time if you type in the time such as 2p, 11a, 345p, 1155a
+ Added Page Parameters as merge fields to the Dynamic Data block. For example {{ PageParameter.GroupId }} would give you the GroupId from the query string.
+ Added a javascript helper method for setting Rock Context and added support for setting Rock Context through a query string parameter (e.g. Campus Context).
+ Fixed Transaction Matching not showing the Person Details if the person was selected using the Person Picker. Also made it more obvious which person the transaction is going to be assigned to if a new person is selected instead of the automatically assigned person and vice-versa.
+ Updated transaction matching so that pressing Enter will navigate to the next record.


Rock McKinley 3.5

+ Fixed issue with occasional website hang
+ Fixed issue with ordering of accounts (and their amounts) on transaction entry block when accounts are added by user in different order than the default account order (Fixes #1219)
+ SendEmail workflow action now works with attributes of type Group (Fixes #1167).
+ Merging a person with a security role now refreshes that role (Fixes #1137).


Rock McKinley 3.4

+ Improved performance of caching.
+ Improved performance of Check-in manager blocks.
+ Improved performance of attendance list and detail blocks.
+ Turned off auditing by default (can be turned back on by changing the value of 'Enable Auditing' global attribute).
+ Updated PersonPicker control to provides some feedback if the user is unauthorized to search (Fixes #1042).
+ Updated performance of First Contribution Date report filter (Fixes #972).
+ Updated Group Finder block to not display inactive groups (Fixes #1019).
+ Fixed Pledge List block so that it correctly applies the Account filter when no accounts are selected (Fixes #1032).
+ Added support for financial gateway providers to specify whether they support Saved account functionality for a given currency type ( credit card, ACH ) and updated UI to render options based on setting (Fixes #1031).
+ Fixed Family group members getting added with inactive status (Fixes #1013).
+ Improved performance of Rock Cleanup job (Fixes #1060).


Rock McKinley 3.3

+ Fixed exception in Batch Detail when batch contained transactions without a currency value (Fixes #998)
+ Fixed issue with person history causing an exception after manual attendance was entered (Fixes #999).
+ Fixed exception on Page List when attempting to sort by Layout column (Fixes #1004)
+ Fixed issue with address auto-compute not working correctly on the person search (Fixes #994)
+ Fixed issue with 'First Contribution Date' report filter taking a very long time to load (Fixes #972).
+ Updated the link in group attendance email to include the impersonated person link so that group leader does not need to login when entering attendance (Fixes #1000).
+ Updated Template Detail block to use 'SMS' instead of 'Sms' (Fixes #891).
+ Updated Person Following List to show proper home/mobile phone in heading based on value of the home/mobile defined type (Fixes #1003).
+ Updated Person Following list so that phone and spouse columns are not sortable (Fixes #1001).
+ Updated check-in to print labels in configured order (Fixes #1007).
+ Updated the person merge so that it does not set external logins to be unconfirmed (Fixes #1005).


Rock McKinley 3.2

+ Fixed Facebook login to match with existing person correctly (Fixes #983).
+ Fixed exception that would occur when trying to activate a new activity on an existing workflow (Fixes #978).
+ Updated the Checkin Schedule to have it's 'StartTime' property available to Lava (Fixes #981).
+ Updated the default ordering of transactions in Contributions tab of person profile be datetime of the transaction (Fixes #980).
+ Fixed issue with My Workflows block not showing the workflow type.
+ Fixed "Group Type" Report Field not showing any results (Fixes #977).
+ Added validation to prevent duplicate routes being created for different pages.
+ Updated person object so that PrimaryAlias property is available to be used by Lava (Fixes #973).
+ Fixed issue with Add Family block adding children as adults (Fixes #964).
+ Fixed issue with prayer approval not updating request correctly, resulting in prayer not being visible to prayer session (Fixes #969).
+ Updated communication processing so that communication job will not send duplicate emails to same recipients.
+ Fixed issue with occasional deadlocks.
+ Added job for processing scheduled metrics.
+ Fixed attribute block to escape HTML in the values column (Fixes #965).
+ Added access keys to edit the individual (Alt+I) and family (Alt+O) on the person details page.
+ Changed the access key for edit from Alt+E to Alt+M (modify) since Chrome already used Alt+E. (Fixes #946).


Rock McKinley 3.1

+ Improved performance of check-in's Find Families action.
+ Added a setting to the check-in Find Families action to limit the number of families returned by search (default is 100).
+ Added a phone number index to improve check-in performance.
+ Fixed filtering issue when filtering on defined value attributes that allow multiple values (Fixes #910).
+ Updated the Transaction Entry block so that the 'Next' button does not remain disabled after a validation error (Fixes #899).
+ Fixed issue with not being able to send communications that included an attachment.
+ Fixed issue with displaying known and implied relationships on the person detail page (Fixes #930).
+ Fixed spacing of the 'In Group of Type' badge (Fixes #908).
+ Fixed issue with existing people creating a new login and not being able to login (Fixes #928).
+ Improved how Rock manages shared connections to database.
+ Fixed issue with tag report.
+ Added additional indexes to improve performance of person merge.
+ Fixed issue with not being able to update a binary file person attribute (Fixes #874).
+ Fixed issue with SMS replies being sent to the wrong person.


Rock McKinley 3.0

+ Updated the PayFlowPro transaction download to exclude 'declined' transactions
+ Fixed inaccurate check-in countdown due to time not using ISO 8601 format (Fixes #878).
+ Made some changes to improve performance and accuracy of check-in random code generation.
+ Fixed BusinessDetail block so that an exception does not occur when trying to delete contacts from a business (Fixes #867).
+ Added a job and system email for sending reminders to group leaders about entering attendance for their group meetings.
+ Updated SampleData block and xml data to handle adding small group topics as  defined values, and to handle setting up a small group's schedule.
+ Updated the Add Family, Edit Person, and Edit Family blocks to have a more consistent UI for adding new people (Fixes #849, Fixes #850, Fixes #855)
+ Fixed issue with effective end date not being updated when saving a recurring schedule.
+ Updated the zone block list so that it only copies authorization from page when a new block is added and not when an existing block is edited.
+ Updated LoginStatus block to only show "Login" when user not logged in, and not an empty dropdown
+ Updated person following and person attribute filtering to use person alias id instead of person id.
+ Updated the Transaction Detail block to facilitate easier keyboard adding of transactions.
+ Changed PrayerRequestDetail block to not encode HTML prior to storage as per https://github.com/SparkDevNetwork/Rock/wiki/Security-Considerations (Fixes #846).
+ Updated the communication send process to use the the person who created the communication when resolving merge fields so that secured attribute values can actually be included in a communication (Fixes #802).
+ Fixed display of transaction list/detail when user is authorized to view, but not to edit (Fixes #805)
+ Updated Bing address verification to handle a null postalcode value returned for valid international addresses (Fixes #845).
+ Updated the boolean field type to render a drop-down list with a blank option instead of a radio-button. This allows specifying a none/true/false value. If attribute is configured to be required, user would still need to select true or false (Fixes #776, Fixes #784)
+ Updated Family Edit to set email active when adding new family members (Fixes #820).
+ Updated the HTML Editor's merge field picker to use current person's security when determining the attributes to display and include in list ( vs. showing only the attributes available to everyone) (Fixes #802).
+ Fixed issue with note container only showing first 10 comments and not displaying the 'Show More' option (Fixes #808).
+ Added a new api/Groups/SaveAddress REST endpoint for updating the address associated with a group/family. This method takes care of checking for existing addresses, updating address pointers correctly, and writing change to family history.
+ Updated the api/Peoples REST POST endpoint to create all the needed relationships for a new person (i.e. Family Member, Person Alias, Known Relationships, Implied Relationships)
+ Fixed the TransactionEntry block to update the phone numbers and/or email address for an existing person.
+ Updated the Communication Entry block to support a 'Simple' mode that can be used on an external site for members to initiate a communication.  Also updated Site to include a Communication Page setting.
+ Changed directory names in the content folder to not have spaces.
+ Added a 'Schedule' field to the group detail that allows a group to have either a simple weekly schedule, a custom schedule, or use any one of the named schedules
+ Updated the 'First-time Icon' merge field for check-in labels to use a new Person.FirstTime field that is true if person has ever attended anything. This replaces previous merge field that only checked if person had previously checked into a group with same group type.
+ Added regex option to the search screen of check-in to allow applying a regex pattern to the user's input. For example the expression '[0](\d*)' would strip off a leading 0 from the input. This example would be needed in the UK where Rock does not store the leading 0 in the database but many individuals would be used to searching using it. #MakingUpForTheSinsOfTheNSA
+ Added option to include people without a mailing address in the Statement Generator. Also, if an individual person is selected, it will include them regardless of mailing address.
+ Updated Statement Generator to recommend a 600x200px logo, and to ship with a standard logo at 600x200px.
+ Add new 'My Settings' page which lives under the 'Login Status' drop-down in the upper right of the internal pages. This page will be a user's one stop shop for all of their personal settings and configurations. For now it contains links to change their password and manage their communication templates.
+ Attribute and AttributeValues can now be included in a REST response by specifying ?LoadAttributes='simple' or ?LoadAttributes='expanded' in the request parameters. In 'simple' mode, only the most commonly needed properties of Attributes and AttributeValues are serialized in a REST response.
+ Updated the 'Set Attribute From Entity' workflow action to support setting a value based on a Lava template that can parse the entity passed in for processing.
+ Moved the Content Channel Type and Content Channel admin screens from under the 'Communications' page to the 'CMS Configuration Page'. This will simplify the security access since these pages will be used more by Web Administrators.
+ Moved the 'Photo Request' page under 'Admin &gt; Communications'. This helps limit who can send out bulk photo requests and helps to clean up the 'People' menu item.
+ Adjusted the security under 'Admin Tools' to allow the 'WEB - Administration' security role to see the 'CMS Configuration' pages and the 'RSG - Communications Administration' role to see the 'Communications' pages.
+ Added edit and administrate access for the 'WEB - Administration' role to the External Website Ads content channel.
+ In an effort to help organizations create an organized system for managing their security roles we've implemented a sample naming system. You can read more about our recommendations in the updated Admin Hero Guide.
+ Added new DateTime field type for attributes.
+ Fixes DISC Last Save Date not storing in international date format (Fixes #828).
+ Upgraded to FontAwesome 4.3.0 (Fixes #814)
+ Added the ability to filter group members by specific attribute values
+ Added additional checking in the PhotoSendRequest block to verify the communication template and certain fields in the template (Fixes #798)
+ Fix new REST key not confirmed on create and UserDetail block showing incorrectly that it was. Fixes https://github.com/SparkDevNetwork/Rock/issues/825.
+ Added Block Property to GroupList to filter by Active Status. If this is set, it overrides the user filter for Active Status
+ Fixed check scanner allowing checks without a check number through when using a Ranger scanner
+ Added option in Address data select to specify what portion of the Address to show in report.
+ Added an option to the 'Memo' field type to optionally allow html to be entered in field without causing a Validation Request error.
+ Added an option in Service Job list to "Run Now".
+ Updated the grid export to resolve defined value ids with their values when exporting to Excel.
+ Changed Statement Generator to display a breakdown of the Accounts for each Transaction
+ Changed the DISC service to work even if someone changes the DISC category name (Fixes #792).
+ Changed reverse order and format of the graduation and grade labels on the person profile (Bio block)
+ Added "Combine Giving" option to Total Giving Amount data select in reporting. When enabled, the total giving for the person will include amounts given by members of their giving group (or just the total for the person if they are not in a giving group)
+ Rearranged the Content Channel Item View block to make it more intuitive. (Fixes #787)
+ Fixed issue with Location Picker control not maintaining state when toggling between different location types on the group details location editor.
+ Added keyboard shortcuts to speed up several operations in Rock. These include:
   - Alt + Q = Quick Search (sets focus to the search box at the top of the page)
   - Alt + S = Save (presses the save button on the given page)
   - Alt + E = Edit (presses the edit button on the given page)
   - Alt + N = New ( presses the add button on any grid on the page)
   - Alt + C = Cancel (presses the cancel button on the given page)
+ Added new Lava filter to create postback links in Lava.
+ Fixed the Delete workflow action so that it does not cause an exception when processing.
+ Updated the workflow detail block so that only users with edit access to the block see the advanced options for editing a workflow.
+ Removed the double borders around the grid when used inside of a grid-panel.
+ Fixed an issue with Categories block that prevented categories from being able to be ordered.
+ Added new block attribute types for using the KeyValueList and ValueList field types
+ Added a new rating control and field type. An attribute can use this field type to display a group of stars that can be selected to rate an item.
+ Add new note types for use in workflows.
+ Updated the 'Set Attribute From Entity' workflow action to include an option for determining if workflow should continue processing or not when the entity is missing or invalid.
+ Fixed bug in transaction detail block where the cancel button would not take you back to the parent batch when clicked.
+ Made changes to the transaction details block to make entering transactions faster. There is now a new 'Add New Transaction' button to enter an additional sibling transaction and the person picker now expands when entering a new transaction.
+ Changed the default sort order of financial transactions to be in the order they where entered.
+ Changed person picker to set the focus to the search box when opened.
+ Added an option to reuse the code per family (disabled by default).
+ Updated the Binary File URL property to return a full path instead of a relative path when file type is using Database or File System storage provider.
+ Updated check-in labels so that attribute values are not HTML encoded.
+ Added helper method to SavedAccount model to create a new reference transaction
+ Added an "Authorize" method to financial gateway components.
+ Fixed issue that enabled the keyboard to display on touch devices (Fixes #777).
+ Updated content feed to sort content items in descending order. Also added filters to respect the content item's date ranges.
+ Updated workflow administration so that users with edit security on a workflow type can manage workflows of that type (Fixes #771).
+ Updated workflow list to allow filtering by workflow attribute values
+ Added feature to the GetChannelFeed handler to resolve any relative links to absolute links.
+ Added block setting to the Bio block used on the Person Profile page to enable prepending the country code to phone numbers. Helpful for internationalization.
+ Updated block rendering so that the Pre/Post text is rendered outside of all the block wrapping divs
+ Updated the Lava "Attribute" filter to use the current person when checking security rather than defaulting to anonymous user (Fixes #760).
+ Fixed the 'Forgot User Name' block to include Active Directory user names (Fixes #765)
+ Updated the FinancialPersonSavedAccount to also allow being associated to a group in addition to a person
+ Fixed issue with static text being replaced with security code on check-in labels (Fixes #766).
+ Updated the address verification services so that a second active service will attempt to standardize/geocode a given address even when previous active service attempted to standardize or geocode the address but failed.
+ Updated the Defined Value field type to optionally display defined value descriptions instead of the values.
+ Added ability to add notes to a workflow
+ Fixed exception that would occur when manually activating a new activity on an existing workflow.
+ Fixed the workflow processing job so that it will still process workflows when the workflow type has a blank processing interval.
+ Updated the logging of exceptions so that the source and stack trace information get logged correctly for inner exceptions.
+ Added 'Exclude Group Type' block setting to Group Tree View and Group Detail
+ Added new group type for tracking internal staff organizational structures with a new page for viewing them.
+ Added the ability to specify root document/image folders and the option to optionally force user-specific root folders for attributes that use the HTML field type
+ Fixed the 'Delay' workflow action to correctly calculate the time that delay was initiated.
+ Changed Google Map API so it only loads when it is needed vs loading on every page. If you have any custom blocks that need the Google Maps API, call this.LoadGoogleMapsApi() in your OnInit.
+ Added option to CategoryTreeView to set the Root Category
+ Updated the Rock Grid control to handle separate DataKey and PersonId columns correctly.
+ Added option to use CurrentDate plus/minus a number of days in DataView date filters
+ Updated the Workflow Entry block so that it will interrogate the query string parameters and set any workflow attributes with same key to the value passed in query string. This results in the Activate Workflow block no longer being needed, so it has been deprecated and will be removed in a future update.
+ Updated workflow type configuration block to include the attribute field types in grid of workflow and activity attributes.
+ Add support for the ExcludedByFilter flag to RemoveEmptyPeople workflow action
+ Updated the Facebook authentication provider to update the person's Facebook person attribute value whenever they login.
+ Added the ability to specify the kiosk id and group type ids as a query string (route) parameter so that the check-in admin screen can by bypassed.
+ Added Lava Address filter option to provide a template for the address return.
+ Add Scan Settings to BinaryFileType
+ Updated the 'Attribute' lava merge field to return an object (instead of string) and added an additional 'Property' filter so that these can be used to navigate complex object/attribute structures when outputing lava content.
+ Updated blocks that provide 'Enable Debug' setting for Lava to only show the debug information if the logged in user also has Edit access to the block. This allows an Admin to enable debug on a production page without everyone seeing the debug info.
+ Fixed the DISC assessment block which was not working in IE 10 (Fixes #732).
+ Added responsive table support to the Rock Grid
+ Added column to GroupMember list for linking to profile detail (and removed 'View Profile' button from group member detail)
+ Added new Lava filter 'Default' that returns the passed default value if the value is undefined or empty, otherwise the value of the variable.
+ Updated several of the Lava string filters (Replace, ReplaceFirst, Remove, RemoveFirst, Append, Prepend to accept objects instead of strings. This allows these filters to be used on numbers also.
+ Updated the Facebook authentication provider so that it works with the new Facebook API.  If person does not have existing photo in Rock it will now also add a photo to Rock from Facebook, and will add a new 'Facebook Friend' known relationships for any of the user's Facebook friends that have also previously logged into Rock using Facebook (Fixes #675).
+ Updated the Memo field type so that attributes of this type retain their line breaks when displayed on page (i.e. workflow entry screen, workflow detail, etc.)


Rock McKinley 2.0

+ Fixed the EmailHeader and EmailFooter global attributes to have correctly formatted HTML content (Fixes #740).
+ Fixed the Check-in countdown until active display when there is an earlier schedule for the same day that is no longer active (Fixes #739).
+ Fixed an issue with not being able to update a Multi-Select attribute value in the Bulk Update block (Fixes #734).
+ Fixed the Attended Duration badge to correctly include Sunday attendances.
+ Fixed the weekly attendance grouping in Attendance Analysis so that Sunday attendances are grouped to the correct weekend.
+ Fixed attendance and DISC badges in IE.
+ Fixed a bug in the lava debug display that kept some attribute help from showing the parent level object.
+ Fixed the DISC assessment block which was not working in IE 10 (Fixes #732).
+ Added CreatedByPersonName, ModifiedByPersonName, CreatedByPersonId, and ModifiedByPersonId as available lava merge fields for all models.
+ Updated security on groups so that a group can use both its parent group and its group type as a parent authority. Also improved security handling in the Group Viewer block (Fixes #718).
+ Fixed security so that if a user is not allowed to view a Group type, they cannot view/edit groups of that type in the Group Viewer block (Fixes #719).
+ Updated the Day of Week attribute field type to allow setting a default of none (Fixes #726).
+ Updated Person Merge and Bulk Update to include deceased people when navigating from grid of people (Fixes #727).
+ Updated the Social Media person attributes to use a URL field type instead of just text (Fixes #728).
+ Updated all emails ( system, workflow, etc) to check that sender is from a "Safe Sender" domain.
+ Updated all the places that use "Person" as an available lava merge field for the current person to use "CurrentPerson" instead.
+ Updated the lava fields used by the PageMenu block to have the same format as all other lava merge fields in Rock. They are now mixed case fields. This required all Page Menu templates and template include files to be updated (update should automatically fix all of the templates).
+ Fixed issue with row selection not working on Dynamic Data Block.
+ Added ability to delete channel items from the Tools &gt; Content page.
+ Updated Twilio SMS transport to report back messages that are undeliverable (Fixes #715).
+ Fixed HTMLEditor issue in IE when toggling between source and WYSIWYG modes not saving content (Fixes #588).
+ Added "Include Child Groups" option to the "In Group" and "Not in Group" Person dataview filters.
+ Updated several workflow action fields used to select attributes to only display attributes that have the specific field types allowed by the action.
+ Updated the location and naming convention of the Lava include files to be RockWeb/Themes/[Theme]/Assets/Lava/[Template].lava instead of RockWeb/Themes/[Theme]/Assets/Liquid/_[Template].liquid.
+ Updated prayer requests so that new prayer requests that were not auto-approved will now show up in the Prayer Requests list without having to check 'Show Expired Requests' (Fixes #685).
+ Added DISC Result block for viewing results after clicking the DISC badge, and added the DISC Request person profile action with corresponding Workflow.
+ Added additional 'Text Option' settings to the financial Transaction Entry and Scheduled Transaction Edit blocks to make more of the text captions configurable.
+ Updated the Send Email, Send SMS, and Send System Email workflow action types so that the recipient field supports Lava.
+ Fixed attribute category ordering on person profile page (Fixes #716).
+ Updated the System Info block's 'Clear Cache' option to also delete all the files in the App_Data/Cache folder.
+ Renamed the 'Send Email Template' workflow action to 'Send System Email' since it is used to send a system email, and not a communication template.
+ Fixed issue with workflow entry form notifications including fields that are not marked visible.
+ Updated the Rock REST API so that exceptions that occur when using the API will now return an error and log the exception to the Rock Exception Log.
+ Updated communication templates to correctly use 'Communication.MediumData.*' instead of 'Communication.ChannelData.*' lava merge fields.
+ Fixed the group member status field label on bulk update block to have the correct label (Fixes #708).
+ Added block properties to the Register block (AccountEntry.ascx) to set the Connection Status and Record Status used when creating new individuals. Default values are 'Web Prospect' and 'Pending' (Fixes #699).
+ Fixed the 'Copy Communication' option in Communication History so that it correctly copies all the channel data (e.g. email body text) to the new communication.
+ Subject value in email templates can now be removed (Fixes #692).
+ Updated workflow entry screen so that inactive workflow types cannot be used to start a new workflow. They will still appear, so that they can be managed, but will not link to the entry form (Fixes #695).
+ Fixed issue where public account names were not being used on the transaction entry page (Fixes #696).
+ Updated Lava templating engine to support enumerations correctly (i.e. Gender) (Fixes #689).
+ Fixed several problems associated with Global Attributes not being properly merged in communication templates for email exceptions, account confirmation, and account creation during new giving transaction (Fixes #684).
+ Updated the Group Tree View block so that it only shows groups of the selected type when specific types are selected in the settings (Fixes #671).
+ Dataview filter selector now shows tooltip with the description of the filter.
+ Report field selector now shows tooltip with the description of what the field is (if it has a description).
+ Changed "Person Link" report field to "Person Name" and moved it to show in the Common fields. It also now has the option to show as a link, and whether to show 'FirstName Lastname', or 'LastName, Firstname'
+ Fixed issue with adding multiple existing people to a family not displaying the correct tab in add person dialog window (Fixes #666).
+ Updated the check-in application so that when the search page is displayed, the number field already has focus (Fixes #623).
+ Fixed issue with check-in codes not printing on labels.
+ Added a way to set the requester of the prayer request on the Prayer Request detail block. Also now shows the requester if one was set.
+ Fixed communication copy so that it uses the current datetime instead of retaining the original create date (Fixes #651).
+ Fixed bug that was preventing GlobalAttributes from being included in new account creation block emails (Fixes #649).
+ Updated Rock REST API to use latest version OData V3. Now there is support for most of the standard OData V3 Query Types and also an endpoint at api/$metadata to see the schema
+ Fixed issue that was preventing prayer requests with comments from being deleted until each comment was removed first (Fixes #644).
+ Fixed a few REST endpoints that were not configured to check authentication correctly (Fixes #647).
+ Added workflow actions to add and remove a person from an organization tag.
+ Fixed exception that would occur on person bio block if person had a phone number with an invalid phone number type
+ Added new Video and Audio field types that can be used to display Video and Audio content.
+ Updated the 'Set Attribute From Entity' workflow action to support any entity type instead of just person or group.
+ Fixed issue with attribute values becoming unavailable when a cached item is reloaded from a model
+ Updated the Memo field type to have a configurable number of rows (instead of always being 3).
+ Added a new DISC Person Profile Badge.
+ Removed the 'Current Date' check box option from the Date field type and added it to a new FilterDate field type. This field type is intended to only be used for report filters when specifying date comparisons (Fixes #627).
+ Added "Not in Group" and "Not in Group of Group Type" dataview filters.
+ Added new "Media File" BinaryFileType as the File Type for Video and Audio field types.
+ Added new AudioFieldType that can be used to add Audio content.
+ Added New VideoFieldType that can be used to upload and include video in content pages.
+ Updated the MyWorkflows block so that when the 'All Types' option is used, only the workflow types that user is authorized to edit are displayed (in addition to any types that have active activities assigned to current user).
+ Updated the 'Person/{PersonId}/StaffDetails' route for the person profile security page to be 'Person/{PersonId}/Security' instead.
+ Removed the 'Blocs' page route as it is no longer needed.
+ Updated how lava include files are referenced. Previously they always needed to be in the current theme's Assets/Liquid folder, and required an underscore prefix and suffix of 'liquid'. Now they are referenced using the actual path and file name relative to the website's root folder.  For example a previous include of {% include 'PageNav' %} would now be {% include '~~/Assets/Lava/PageNav.lava' %}. All of Rock's Liquid include statements have been updated to reflect this change.
+ Updated Content Channel functionality to allow channel-specific item attributes.
+ Updated the item list on channel view block to include columns for the attributes that have been configured with the 'Display in Grid' option enabled
+ Added a new workflow action and webhook for processing background checks.
+ Updated the System Info block's clear cache option to also refresh the list of EntityTypes, FieldTypes, and BlockTypes.
+ Added new Encrypted Field Type and Attribute that can be used to store sensitive attribute values as an encrypted value in database (e.g. Passwords, SSN, etc ).
+ Added option to RockTextBox field type for using password mode on rendered textboxes and update SMTP settings and Payment Gateway settings to use password mode (Fixes #426).
+ Added a 'Delay' workflow action that will delay successful execution of action until a specified number of minutes have passed.
+ Updated the Content Channel Dynamic block to allow sorting items by attribute values in addition to item properties.
+ Added optional setting to Group Member list to allow the Member Count column to be hidden


Rock McKinley 1.2

+ Fix report data view filters that use an attribute date filter (Fixes #629)
+ Include last name of family members if the last name is different than the current person being viewed (Fixes #618).
+ Fixed issue with StatementGenerator that was causing it to only work on x64 machines (Fixes #613).
+ Fix person age calculation to not be dependent on current culture being in mm/dd/yy format (Fixes #607).
+ Fix the ordering of Content Channel Dynamic items when retrieving items from cache.
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


Rock McKinley 1.1

+ Added the option for specifying 'Current Date' when adding data view or content channel item filters on date fields or attributes.
+ Updated default format on content channel item detail block to check for missing image (Fixes #580)
+ Added the option to support additional filtering based on query string parameter values to the Content Channel Dynamic block
+ Fixed the Social Media person attribute category to have the correct qualifier (Fixes #579)
+ Added communication template content that was deleted in the v1.0 update
+ Fixed issue with content channel items getting saved with incorrect status when user is authorized to approve channel items. (Fixes #576)
+ Updated check-in administration screen to exclude any group types that do not have the 'Takes Attendance' option enabled when selecting the 'Check-in Areas' to use for check-in. (Fixes #559)
+ Added preview dialog to Html Content Approval
+ Added support for merging liquid fields in the RSS channel HTTP hander.
+ Added new content channel type of 'Blogs' along with a sample blog on the external website. Also added the 'Bulletins' channel type.
+ Added new Lava filter 'ToString' to convert integers to strings.
+ Removed the homepage link from the stark theme as the logo already acts as a home button.
+ Added security to the 'Web Ads' content channel to allow 'Communication Administrators' to be able to Edit/Administrate/Approve. (Fixes #568)
+ Fixed issue that was keeping content from showing in the ContentChannelDynamic block if you were not logged in. (Fixes #567)
+ Fixed typo changing 'Nick Name' to 'Nickname'. (Fixes #570)
+ Fixed issue that limited the editing of attributes on the Person Profile screen in Firefox. . (Fixes #573)
+ Added the ability to change the page title, override the meta description, add an RSS autodiscover tag and add meta images on the ContentChannelDynamic block.
+ Added ability to sort reports on any column (except for Liquid).
+ Fixed DataView filters to work on MultiSelect Attributes.  For example, if the Attribute is "Favorite Colors" with options of "Red,Blue,Green", a data filter of "Favorite Colors is Blue or Red" will work.
+ Update HTML Content Detail block to include merge fields even when there is not a logged in person.
+ Update Dynamic Data Block to support column sorting, and multiple result sets.
