+ Added CreatedByPersonName, ModifiedByPersonName, CreatedByPersonId, and ModifiedByPersonId as available lava merge fields for all models.
+ Updated security on groups so that a group can use both it's parent group and it's group type as a parent authority. Also improved security handling in the Group Viewer block (Fixes #718).
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
+ Fixed the group member status field label on bulk update block to have the correct label(Fixes #708).
+ Added block properties to the Register block (AccountEntry.ascx) to set the Connection Status and Record Status used when creating new individuals. Default values are 'Web Prospect' and 'Pended' (Fixes #699).
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
+ Fixsd bug that was preventing GlobalAttributes from being included in new account creation block emails (Fixes #649).
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
+ Updated how lava include files are referenced. Previously they always needed to be in the current theme's Assets/Liquid folder, and required an underscore prefix and suffix of 'liquid'. Now they are referenced using the actual path and file name relative to the the website's root folder.  For example a previous include of {% include 'PageNav' %} would now be {% include '~~/Assets/Lava/PageNav.lava' %}. All of Rock's Liquid include statements have been updated to reflect this change.
+ Updated Content Channel functionality to allow channel-specific item attributes.
+ Updated the item list on channel view block to include columns for the attributes that have been configured with the 'Display in Grid' option enabled
+ Added a new workflow action and webhook for processing background checks.
+ Updated the System Info block's clear cache option to also refresh the list of EntityTypes, FieldTypes, and BlockTypes.
+ Added new Encrypted Field Type and Attribute that can be used to store sensitive attribute values as an encrypted value in database (e.g. Passwords, SSN, etc ).
+ Added option to RockTextBox field type for using password mode on rendered textboxs and update SMTP settings and Payment Gateway settings to use password mode (Fixes #426).
+ Added a 'Delay' workflow action that will delay successful execution of action until a specified number of minutes have passed.
+ Updated the Content Channel Dynamic block to allow sorting items by attribute values in addition to item properties.
+ Added optional setting to Group Member list to allow the Member Count column to be hidden