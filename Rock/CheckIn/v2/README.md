# Instructions

This file contains instructions for Copilot related to Rock RMS Check-in.

When validating the structure of options, don't list options that are correct. Only list ones that need to be fixed. Spelling errors should also be reported.

When reporting on consistency and things like that, don't report things that are already good. Only things that need improvement. When reporting issues, inline a line number if possible.

There are a few primary goals to having this file:

* Consistent UI elements such as labels, descriptive text, tooltip help text, etc. They obviously don't need to be exact names as each other, but we want the tone, level of understanding, punctuation, and style to be consistent. It should feel like all the UI text was written by a single person.
* Concise but easy to understand. We have limited UI space so we want to be concise, and being concise tends to help things be more understandable. But at the same time, we don't want to be so concise that the person isn't clear what it means or does.
* Make sure configuration options are in the right sections. The names and descriptions of the sections should make sense for all the options inside that section. If they don't make sense we need to either change the section name/description or move the configuration option to another section.
* Capitalization and spelling of things should be consistent across all configuration options.

## UI Formatting Rules

The following rules should be enforced for any text that is displayed in the UI (labels, titles, descriptions, help text, etc.):

* Labels and titles should be in title-case, with no trailing period. Preposition words should be lower case.
* Descriptions, help text and other long form text must include a trailing period.
* Assume the person using the UI to be a high school graduate, but may or may not have a college degree. They are likely to have very low technical knowledge and rely primarily on help text and documentation. Trying out changes to configuration options probably scares them because they are afraid of breaking something.

## Sample People

We have a sample family we use when talking about these things. Ted and Cindy Decker are the parents. Noah is their son and Alex (full name Alexis) is their daughter. Noah is 11 and in 6th grade. Alex is 8 and in 3rd grade.

## Process Overview

The check-in system handles attendance related check-in. When people arrive they will go to a kiosk to check-in. Sometimes the kiosk will be manned (attended check-in) and other times they will be self service. Either way the general flow is the same, it just comes down to who is interacting with the screen.

The first thing they need to do is search for their family. Depending on configuration options this can be done by phone number or last name. Then a list of matching families is displayed and they pick one.

They are then taken to a screen that shows all the family members as well as friends of the family (these are people that are in other families but they are authorized to check-in). Depending on the configuration they will either pick a single person to check-in, or multiple people to check-in.

Next they go through a series of screens in order to select the Schedule (service time), Area, Group, and Location. If the person is young enough, they will also see a screen asking about their ability level (crawling, walking, etc).

Finally they will see a final status screen. This will show them who was checked in, what locations they checked into, any celebrations, and also handle printing of their labels.

In the screen descriptions above, some screens may be automatically skipped if there is only a single selection available. For example, if there is only 1 location available for them to check into then that screen will be skipped and the single option automatically selected.


## Terminology

* Kiosk: A physical computer station, almost always using a touch screen. This may be a full computer with monitor, or a tablet like an iPad.
* Schedule: A database record that describes a service time, such as "every sunday at 9:00am".
* Area: A broad categorization of one or more groups such as "Elementary" or "Nursery". These contain a limited amount of configuration data such as which labels to print and any load balancing requirements on locations.
* Group: A group provides primary filtering such as age, gender, grade range, etc. A grade must belong to one and only one area. A group contains one or more locations that people will be placed into.
* Location: A database representation of a room. Configuration such as max capacity exists here. A location can be associated to multiple groups.
* Presence: The check-in system has an optional feature called "presence". This is used to determine if somebody is physically in the room. In this mode, when somebody checks in they are put in a kind of pending state until they arrive at the room at which point they are updated to "present in room". This is used to help provide additional safety checks. For example in an evacuation, you don't want to be looking for a missing kid that was never dropped off in your room to begin with.
* Supervisor/Supervising: This is essentially a special set of screens on the kiosk. They require an access code to pull up and allow a staff worker to do things like see an overall picture of location counts, close or open locations, and perform some other administrative type tasks.
* Checkout: There are two uses cases for this term. One is the actual act of checking out. This could be triggered by either a manual action or by some automatic action, such as a service ending. The other is a mode supported by the kiosk. When the mode of checkout is enabled, an additional UI screen will display showing family members currently checked in and allow them to be checked out. The mode is probably a bit on the rare side to be used since nothing prevents a person from being checked out by a parent and then left in the room anyway.
* Security Code: These are used to help during checkout. A parent or guardian will be given a sticker with a security code on it. When picking up the child, this code will be checked in the system to make sure it is the right code for the child. This helps ensure somebody doesn't take a kid they are not authorized to take. Often these are also used on small display signs in service to let the parent know they are needed without being a disruption to the whole audience.


## Spelling

The following should be used when spelling words for display in the UI:

* check-in
* checkout
* auto-select

# Template Configuration Options

The child headings are divided up into the following levels.

The first level represents a UI panel with a label that matches the heading title. These panels are collapsable and meant to represent broad categories of configuration options.

The second level represents a UI section within the panel. These have a UI label that matches the name of the heading title. They also have a UI description that will be identified by any paragraph beginning with the phrase "Description:". Any other text should be considered internal details that provide more context about the section but wouldn't be displayed in the UI. For example, we might indicate which settings are conditional and only display depending on other values. This is meant for internal information and information to the LLM, but not to be displayed directly.

The third level represents the UI configuration options in that section. The headings are the UI labels of the configuration setting. The content of the section will contain additional information on how this setting is used. Each line should be prefixed by one of the terms in the following list. If a line is not prefixed, it is an error and should be pointed out. All the items below are optional, but if given they should follow the order of the items below. If something is out of order, it is considered an error and should be pointed out. Any unknown prefixes here should also be considered and error.

* Type: The type of UI control that will represent this option. Valid values are: Single Selection; Multiple Selection; Text Input; Number Input; Checkbox.
* Values: If the UI control is one that only provides a hard coded set of values that cannot change, they will be listed here separated by semicolon. If no values are provided that means the input type is freeform, such as a text input, or a list of dynamic items driven by other database configuration.
* Depends On: This specifies another option's value that determines if this option should be visible. The item is also visibly indented to help show that is is related to the option it depends on. If you can't figure out what known configuration option or value this is referring to, then consider it an error.
* Help: This indicates the following text is used for tooltip help text in the UI.
Mode: This should be a value of either "Essentials" or "Trailblazer" and must be present on all third level configuration options.
* Note: This provides context information about the setting. It won't be displayed in the UI directly, but can be used to improve the quality of the help text or label. Don't assume the end-user knows about this information. We sometimes intentionally use "note:" inside of help text (the "Help:" prefix) to call something out to the user, so don't confuse the two notes.
* Developer Note: This provides context to the developer who will be implementing this feature. It should be ignored by the language model.
* Example: This provides a real world example of how it would be used.


## General

Note: This should contain things that are truly general. It should not be a catch all just because we couldn't come up with a better place to put something.

### Check-in Type Flow

Description: Control how the screen should flow depending on if a family or an individual is checking in. A family check-in type will give additional settings for the flow.

#### Check-in Type

Type: Single Selection
Values: Family; Individual
Mode: Essentials

Help: The type of check-in experience to use for this type. Family check-in allows more than one person in the family to be checked in at a time.

#### Family Member Auto-Select Mode

Type: Single Selection
Values: People Only; People & Their Area/Group/Location
Depends On: Check-in Type == Family
Mode: Essentials

Help: The options that should be pre-selected if a family member has previously checked in. 'People Only' will only select the person and then require you to select the placement options. 'People & Their Area/Group/Location' will automatically pre-select all placement options.

Developer Note: Originally called "Auto Select Options"

#### Days Back for Auto-Selecting Family Members

Type: Number Input
Depends On: Check-in Type == Family
Mode: Essentials

Help: The number of days back to look for a previous check-in for each person in the family (or related person). If they have previously checked within this number of days, they will automatically be selected during the Family check-in process.

Note: The UI input field should have an automatic suffix of "Days" to provide additional clarity.

Developer Note: Originally called "Auto Select Days Back".

#### Reuse Selections for Additional Service Times

Type: Checkbox
Depends On: Check-in Type == Family
Mode: Essentials

Help: When a family member is checking into multiple services, reuse their first service's selections (area, group, location) for each additional service.

Developer Note: Originally called "Use Same Service Options".

#### Prevent Duplicate Check-in

Type: Checkbox
Depends On: Check-in Type == Family
Mode: Essentials

Help: Should people be prevented from checking into a specific service time (schedule) more than once?

#### Use Same Code for Family

Type: Checkbox
Depends On: Check-in Type == Family
Mode: Essentials

Help: Should the same security code be used for each person from the same family that is checking in at the same time?

Note: This applies to a single check-in session.

Example: If this is turned on and Ted Decker checks himself and his son Noah in, they will both get the same security code, such as "ABC123". If he then realizes he forgot to check his wife Cindy and daughter Alexis in and goes to check them in, they will get a different code but they will both get the same code as each other, such as "ABC456". So this really applies to a check-in session, which is a single pass through the check-in system.

### Kiosk Features

Description: These settings control the basic kiosk features for check-in and checkout.

#### Enable Self Checkout at Kiosk

Type: Checkbox
Mode: Essentials

Help: Allows individuals to checkout using the kiosks.

Developer Note: Originally called "Enable Check-out at Kiosk".

#### Allow Removing Guests from Family

Type: Checkbox
Mode: Trailblazer

Help: Only supported in next-gen check-in. Allows individuals to remove guests from their family at the kiosk without supervisor approval. A guest is a non-family member connected by a relationship. This will remove all relationships that are marked as "Can Check-in".

Developer Note: Originally called "Enable Remove From Family at Kiosk".

#### Enable Presence Confirmation

Type: Checkbox
Mode: Trailblazer

Help: When enabled, the attendance record will not be marked as being 'present' until the individual is set to 'Present' by the assistant using the Check-in Manager application.

Developer Note: Enable Presence

### Display

Description: Set what kind of content to display during check-in.

#### Promotions Content Channel

Type: Single Selection
Mode: Trailblazer

Help: The content channel to use for displaying promotions on the kiosk welcome screen. Only supported by next-gen check-in.

Note: This provides content that is displayed on the kiosk. When on the welcome screen and idle, it will cycle through the content items contained in the selected channel.

#### Hide Individual Photos

Type: Checkbox
Mode: Essentials

Help: Select this option if photos should not be displayed when selecting people from the family during check-in.

Developer Note: Originally called "Hide Photos"

#### Display Location Occupancy Count

Type: Checkbox
Mode: Trailblazer

Help: Should the room location selection buttons include a count of how many people are currently checked into that location?

Note: These counts are displayed when selecting which location to place a person into.

Developer Note: Originally called "Display Location Count".


#### Celebration Achievement Types

Type: Multiple Selection
Mode: Trailblazer

Help: Select achievement types that will be used for check-in celebrations.

Developer Note: Originally called "Achievement Types".

## Supervising Check-in

### Supervision Capabilities

Description: Configure the capabilities for check-in that supervisors and check-in managers have access to.

#### Enable Supervisor Mode

Type: Checkbox
Mode: Essentials

Help: Show an option on the check-in welcome screen that allows an individual to view the supervison screen (after entering a passcode).

Developer Note: Originally called "Enable Manager Option".

#### Enable Supervisor Override

Type: Checkbox
Depends On: Enable Supervisor Mode == True
Mode: Essentials

Help: Show an option on the check-in supervisor screen that allows a supervisor to check-in a person and ignore any specific group requirements, such as age and grade.

Note: An override allows a supervisor to check somebody into a room they wouldn't normally be allowed into.

Example: If a younger sibling is scared to go alone, sometimes a supervisor will use an override to allow the older sibling to check into the same room, even if they are too old, so that the younger sibling is calmer.

Developer Note: Orginally called "Enable Override".

#### Enable Checkout in Manager

Type: Checkbox
Depends On: Enable Supervisor Mode == True
Mode: Essentials

Help: Allows checkout to be enabled in the Check-in Manager.

Developer Note: Originally called "Enable Check-out in Manager".


## Search & Security Codes

### Search

Description: Configure how people are searched and matched during the check-in process.

#### Search Type

Type: Single Selection
Values: Phone Number; Name; Name & Phone
Mode: Essentials

Help: The type of search that is available after the person clicks the check-in button on the check-in Welcome screen. Note, the individual can also always check-in using a scanned barcode, fingerprint, RFID card, etc. if the scanner is attached and configured for keyboard wedge mode.

Note: This selection changes the UI when searching for a family. If the selection is Phone Number only, then an on-screen keypad will be displayed so that the search can happen without a keyboard.

#### Phone Number Minimum Length

Type: Number Input
Depends On: Search Type == Phone Number or Name & Phone
Mode: Essentials

Help: The minimum number of digits that needs to be entered for a phone number search (default is 4).

Note: This refers to the minimum length of a numeric phone number search that can be performed. It also disables the Search button in the UI until this many digits have been entered.

Developer Note: Originally called "Minimum Phone Number Length".

#### Phone Number Maximum Length

Type: Number Input
Depends On: Search Type == Phone Number or Name & Phone
Mode: Essentials

Help: The maximum number of digits that can be entered for a phone number search (default is 10).

Note: This refers to the maximum length of a numeric phone number search that can be performed.

Developer Note: Originally called "Minimum Phone Number Length".

#### Phone Number Comparison Method

Type: Single Selection
Values: Contains; Ends With
Depends On: Search Type == Phone Number or Name & Phone
Mode: Trailblazer

Help: Determines how an individual's phone number is matched against the digits they enter during check-in.

Developer Note: Originally called "Phone Search Type".

#### Maximum Number of Results

Type: Number Input
Mode: Trailblazer

Help: The maximum number of search results to return when searching for families (default is 100).

#### Regular Expression Filter

Type: Text Input
Mode: Trailblazer

Help: An optional regular expression that will be run against any search input before the search is performed. This is useful for removing any special characters.

### Security Codes

Description: Configure the length and format of security codes printed on labels. Rock will generate alpha-numeric characters first, followed by alphabetic, then numeric codes. The length of the code will be the sum of all three character counts. Note: A 3-character alpha-numeric setting provides 13,744 unique codes per day. Larger churches should consider using at least 4 characters to avoid duplicates.

#### Alpha-Numeric Characters

Type: Number Input
Mode: Essentials

Help: The number of alpha-numeric characters to include. This includes most uppercase letters and numbers, but some are intentionally excluded for clarity and security.

Developer Note: Originally called "Alpha-Numeric".

#### Alpha Characters

Type: Number Input
Mode: Trailblazer

Help: The number of alpha characters to include. This includes most uppercase letters, but some are intentionally excluded for clarity and security.

Developer Note: Originally called "Alpha".

#### Numeric Characters

Type: Number Input
Mode: Trailblazer

Help: The number of numeric characters to include. This includes most numbers, but some are intentionally excluded for clarity and security.

Developer Note: Originally called "Numeric".

#### Random Numeric Values

Type: Checkbox
Mode: Essentials

Help: Should the numbers be randomized (vs. generated in order).


## Check-in Registration

### General Registration Settings

Description: Configure general features and default settings for new individuals.

#### Default Person Connection Status

Type: Single Selection
Mode: Essentials

Help: When adding a new person, this will be the connection status they are created with.

#### Enable Check-in After Registration

Type: Checkbox
Mode: Essentials

Help: This determines if the family should continue on the check-in path after being registered, or if they should be directed to a different kiosk after registration (take them back to search).

#### Display SMS Option for Phone Numbers

Type: Checkbox
Mode: Essentials

Help: Display an option to enable SMS messaging for the individual's phone number.

Developer Note: Originally called "Display SMS Enabled Selection for Phone Number".

#### Enable SMS by Default

Type: Checkbox
Depends On: Display SMS Option for Phone Numbers
Mode: Essentials

Help: Enables the SMS messaging option for the new phone numbers by default.

Developer Note: Originally called "Set the SMS Enabled for the phone number by default".

#### Display Name Suffix

Type: Single Selection
Values: None; Adults & Children; Adults Only; Children Only
Mode: Trailblazer

Help: Display the name suffix field (such as Jr or Sr) when editing an individual's information.

Developer Note: Originally called "Display Suffix".

### Adult Registration Fields

Description: Configure the types of information to collect from adults during registration.

#### Required Attributes for Adults

Type: Multiple Selection
Mode: Trailblazer

Help: The person attributes that will be displayed as required input fields when adding or updating adults.

#### Optional Attributes for Adults

Type: Multiple Selection
Mode: Trailblazer

Help: The person attributes that will be displayed as optional input fields when adding or updating adults.

#### Display Birthdate for Adults

Type: Single Selection
Values: Hide; Optional; Required
Mode: Essentials

Help: Controls how the birthdate input field should be displayed for adults.

Developer Note: Originally called "Display Birthdate on Adults".

#### Display Race for Adults

Type: Single Selection
Values: Hide; Optional; Required
Mode: Trailblazer

Help: Controls how the race input field should be displayed for adults.

Developer Note: Originally called "Display Race on Adults".

#### Display Ethnicity for Adults

Type: Single Selection
Values: Hide; Optional; Required
Mode: Trailblazer

Help: Controls how the ethnicity input field should be displayed for adults.

Developer Note: Originally called "Display Ethnicity on Adults".

#### Display Alternate ID Field for Adults

Type: Checkbox
Mode: Trailblazer

Help: Controls if the alternate identifier input field should be displayed for adults. The alternate identifier is used when searching for a scanned barcode tag.

### Children Registration Fields

Description: Configure the types of information to collect from children during registration.

#### Required Attributes for Children

Type: Multiple Selection
Mode: Trailblazer

Help: The person attributes that will be displayed as required input fields when adding or updating children.

#### Optional Attributes for Children

Type: Multiple Selection
Mode: Trailblazer

Help: The person attributes that will be displayed as required input fields when adding or updating children.

#### Display Birthdate for Children

Type: Single Selection
Values: Hide; Optional; Required
Mode: Essentials

Help: Controls how the birthdate input field should be displayed for children.

Developer Note: Originally called "Display Birthdate on Children".

#### Display Grade for Children

Type: Single Selection
Values: Hide; Optional; Required
Mode: Essentials

Help: Controls how the grade input field should be displayed for children.

Developer Note: Originally called "Display Grade on Children".

#### Display Mobile Phone for Children

Type: Single Selection
Values: Hide; Optional; Required
Mode: Essentials

Help: Controls how the mobile phone input field should be displayed for children.

Developer Note: Originally called "Display Mobile Phone on Children".

#### Display Race for Children

Type: Single Selection
Values: Hide; Optional; Required
Mode: Trailblazer

Help: Controls how the race input field should be displayed for children.

Developer Note: Originally called "Display Race on Children".

#### Display Ethnicity for Children

Type: Single Selection
Values: Hide; Optional; Required
Mode: Trailblazer

Help: Controls how the ethnicity input field should be displayed for children.

Developer Note: Originally called "Display Ethnicity on Children".

#### Display Alternate ID Field for Children

Type: Checkbox
Mode: Trailblazer

Help: Controls if the alternate identifier input field should be displayed for children. The alternate identifier is used when searching for a scanned barcode tag.

#### Require Relationship Type Selection for Children

Type: Checkbox
Mode: Trailblazer

Help: Requires the individual to select a relationship type from the list. 'Child' will not be pre-selected automatically when turned on.

Developer Note: Originally called "Force Selection of Known Relationship Type".

#### Minimum Age for Grade Confirmation

Type: Number Input
Mode: Trailblazer

Help: Sets the minimum age at which the grade confirmation dialog appears while adding or editing a child when no grade has been entered. Use decimals for partial years (e.g., 5.5). Only applies if grade is optional. Leave blank to disable the dialog.

Note: This should not have a "Display" prefix in the title. That implies a conditional state, but the input is always visible.

Developer Note: Originally called "Grade Confirmation Age".

### Family Registration Fields

Description: Configure the types of information to collect from families during registration.

#### Required Attributes for Families

Type: Multiple Selection
Mode: Trailblazer

Help: The family attributes that will be displayed as required input fields when adding or updating family records.

#### Optional Attributes for Families

Type: Multiple Selection
Mode: Trailblazer

Help: The family attributes that will be displayed as optional input fields when adding or updating family records.

### Child Relationship Settings

Description: Configure the relationship types that are displayed when adding a child. You can also specify which of those will add the child to the parent's family and which ones will add them to a new family with a relationship back to the parent's family.

#### Relationship Types

Type: Multiple Selection
Mode: Trailblazer

Help: The known relationships to display in the child's 'Relationship to Adult' field.

Developer Note: Originally called "Known Relationship Types".

#### Add Child to Parent's Family for These Relationship Types

Type: Multiple Selection
Mode: Trailblazer

Help: Choose which relationship types will add a new child to the same family as the adults.

Developer Note: Originally called "Same Family Known Relationship Types".

#### Add Child to New Family for These Relationship Types

Type: Multiple Selection
Mode: Trailblazer

Help: Choose which relationship types will add a new child to a separate family, with a 'Can Check-in' relationship back to the adults in the primary family.

Developer Note: Originally called "Can Check-in Known Relationship Types".

### Registration Workflows

Description: Configure optional workflows to run when a new family or person is registered.

#### New Family Workflow Types

Type: Multiple Selection
Mode: Trailblazer

Help: The workflow types that should be launched when a family is added.

#### New Person Workflow Types

Type: Multiple Selection
Mode: Trailblazer

Help: The workflow types that should be launched when a person is added.


## Advanced Settings

### Additional Filters & Settings

Description: Configure requirements like age, grade, or ability level, enable proximity check-in, and filter who can check in.

#### Ability Level Determination

Type: Single Selection
Values: Ask; Don't Ask; Don't Ask If There Is No Ability Level; Don't Ask If There Is An Ability Level
Mode: Trailblazer

Help: Determines how check-in should gather the individual's current ability level. "Ask" means that the individual will be asked as a part of each check-in. "Don't Ask" will trust that there is another process in place to gather ability level information and the individual will not be asked for their level during check-in. "Don't Ask If There Is No Ability Level" will only ask if the individual does <b>not</b> already have an ability level. This will allow a person's ability level to be updated during the check-in process but not initially set. "Don't Ask If There Is An Ability Level" will only ask if the individual <b>does</b> already have an ability level (this is only supported in next-gen check-in).

Note: In the list of values, "An" is capitalized because it is an auto-generated value, we don't have control over it.

Developer Note: When putting the help text in the UI, Use HTML to make each of the four options their own bullet points.

#### Grade & Age Matching Behavior

Type: Single Selection
Values: Grade and Age Must Match; Age Match Not Required; Prioritize Grade Over Age
Mode: Trailblazer

Help: Only supported in next-gen check-in. Defines how the Grade Range and Age Range/Birthdate Range values are used (blank ranges are still allowed in all cases). See documentation for edge cases not described here. "Grade And Age Must Match" means Grade Range and one of Age Range or Birthdate Range must match. "Age Match Not Required" means that if a Grade Range is specified on the group and the person has a matching Grade then that will be considered sufficient and neither Age Range nor Birthdate Range will be checked even if they are specified. "Prioritize Grade Over Age" will use the same filtering logic as Age Match Not Required. After that, if any groups were matched by grade then all groups that were not matched by grade will be excluded.

Developer Note: Originally called "Grade and Age Matching Behavior".

#### Age Restriction

Type: Single Selection
Values: Show All; Hide Adults; Hide Children
Mode: Trailblazer

Help: Limits the ages of people that will be considered for check-in when using this configuration. This also limits who will be displayed on the person select screen. Only applies to next-gen check-in.

#### Enable Proximity Check-in

Type: Checkbox
Mode: Trailblazer

Help: Makes this check-in configuration and all areas and groups available for proximity check-in with a native Rock Mobile application.

#### Check-in Notification Template

Type: Text Input
Depends On: Enable Proximity Check-in == True
Mode: Trailblazer

Help: The Lava template used to generate the push notification message when an individual is successfully checked in via proximity. This message will be sent to the associated device. Only used when proximity check-in is enabled.

Developer Note: The text input should be a Code Editor with Editor Mode set to Lava.

#### Hide Inactive People

Type: Checkbox
Mode: Essentials

Help: When enabled, individuals who are marked as inactive will not be shown in the results or allowed to check-in.

Developer Note: Originally called "Prevent Inactive People".

#### Age is Required

Type: Checkbox
Mode: Essentials

Help: If an area and/or group has an age requirement, check this option to prevent people without an age from checking into that area/group.

#### Grade is Required

Type: Checkbox
Mode: Essentials

Help: If an area and/or group has a grade requirement, check this option to prevent people without a grade from checking into that area/group.

### Special Needs

Description: Configure check-in settings to accommodate individuals with special needs.

#### Hide Special Needs Groups

Type: Checkbox
Mode: Essentials

Help: A special needs group will only be available if the person is marked as special needs.

Note: At check-in time, a list of groups will be processed that is configured elsewhere. If the Person is not marked as Special Needs and this setting is enabled, then any group marked as Special Needs will be removed from consideration.
Note: It is okay that this label is similar to the "Hide Non-Special Needs Groups".

Developer Note: Originally called "Remove Special Needs Groups".

#### Hide Non-Special Needs Groups

Type: Checkbox
Mode: Essentials

Help: A non-special needs group will only be available if the person is not marked as special needs.

Note: At check-in time, a list of groups will be processed that is configured elsewhere. If the Person is marked as Special Needs and this setting is enabled, then any group not also marked as Special Needs will be removed from consideration.
Note: It is okay that this label is similar to the "Hide Special Needs Groups".

Developer Note: Originally called "Remove Non-Special Needs Groups".

### Custom Attributes

Description: Configure any custom settings defined on your system.

Mode: Trailblazer

Developer Note: This is where any custom attributes should go. If there are no custom attributes then this entire section should be hidden. We will need to filter out "known" attributes just like we do in the legacy block.


## Classic Check-in Settings

### Display

Description: Control how often the kiosk refreshes its check-in configuration and what is displayed on the success screen.

#### Refresh Interval

Type: Number Input
Mode: Trailblazer

Help: How often (seconds) should the welcome page automatically refresh and check for updated configuration information?

Note: The UI input field should have an automatic suffix of "Seconds" to provide additional clarity.

#### Success Template Display Mode

Type: Single Selection
Values: Never; Replace; Append
Mode: Trailblazer

Help: 'Never' will hide the custom success template. 'Replace' will replace the current success content with the template. 'Append' will place the success template content under the existing content.

Note: This label is confusing, but it is a legacy setting so we are going to leave it as-is.

#### Success Template

Type: Text Input
Depends On: Success Template Display Mode == Replace or Append
Mode: Trailblazer

Help: The lava template to use when rendering the Success result on the Success Block.

Note: Allows custom HTML content to replace or be appended to the normal success screen content.

### Templates

Description: Customize the content shown on the start page, family and person selection screens, and the success screen.

#### Start Screen Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use when rendering the Start button on the Welcome Block.

Developer Note: Originally called "Start Template".

#### Family Select Screen Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use when rendering each family button on the Family Select.

Developer Note: Originally called "Family Select Template".

#### Person Select Screen Template

Type: Text Input
Mode: Trailblazer

Help: The lava template used to append additional information to each person button on the Person Select & Multi-Person Select Check-in blocks.

Developer Note: Originally called "Person Select Template".

### Custom Header Text

Description: Configure the header text for each step of the check-in process.

#### Action Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Action Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.

Developer Note: Originally called "Action Select".

#### Checkout Person Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Checkout Person Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.

Developer Note: Originally called "Checkout Person Select".

#### Person Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Person Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.

Developer Note: Originally called "Person Select".

#### Multi-Person Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Multi Person Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.

Developer Note: Originally called "Multi Person Select".

#### Group Type Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Group Type Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.<br>{{ Individual }} which is a Person object and is the current selected person.<br>{{ SelectedSchedule }} is a Schedule object and is the current selected schedule.

Developer Note: Originally called "Group Type Select".

#### Time Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Time Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.<br>{{ SelectedIndividuals }} is a list of Person objects which contains all of the currently selected persons.<br>{{ CheckinType }} is the type of check-in given as a string which will be either 'Family' or 'Individual'.<br>{{ SelectedGroup }} is a Group object and corresponds to the selected check-in group listed in Areas and Groups. This only applies for individual check-in types.<br>{{ SelectedLocation }} is a Location and corresponds to the selected location for the group. This only applies for individual check-in types.

Developer Note: Originally called "Time Select".

#### Ability Level Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Ability Level Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.<br>{{ Individual }} which is a Person object and is the current selected person.<br>{{ SelectedArea }} is a GroupType object and corresponds to the selected check-in Area listed in Areas and Groups.

Developer Note: Originally called "Ability Level Select".

#### Location Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Location Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.<br>{{ Individual }} which is a Person object and is the current selected person.<br>{{ SelectedGroup }} is a Group object and corresponds to the selected check-in group listed in Areas and Groups.<br>{{ SelectedSchedule }} is a Schedule object and is the current selected schedule.

Developer Note: Originally called "Location Select".

#### Group Select Header Template

Type: Text Input
Mode: Trailblazer

Help: The lava template to use for the 'Group Select' check-in block header. The available merge fields are:<br>{{ Family }} which is a Group object and is the current family.<br>{{ Individual }} which is a Person object and is the current selected person.<br>{{ SelectedArea }} is a GroupType object and corresponds to the selected check-in Area listed in Areas and Groups.<br>{{ SelectedSchedule }} is a Schedule object and is the current selected schedule.

Developer Note: Originally called "Group Select".
