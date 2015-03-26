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