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

*** The rest of the content is truncated. ***
