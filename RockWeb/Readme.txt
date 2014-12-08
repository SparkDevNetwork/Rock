+ Add CreatedByPersonName, ModifiedByPersonName, CreatedByPersonId, and ModifiedByPersonId as available lava merge fields for all models
+ Update security on groups so that a group can have both the parent group and it's group type as a parent authority, and improve security handling in the Group Viewer block (Fixes #718)
+ Fix security so that if user is not allowed to view a Group type, they cannot view/edit groups of that type in the Group Viewwer (Fixes #719).
+ Update the Day of Week attribute field type to allow setting a default of none (Fixes #726)
+ Update Person Merge and Bulk Update to include deceased people when navigating from grid of people (Fixes #727).
+ Update the Social Media person attributes to use a URL field type instead of just text (Fixes #728)
+ Update all Emails ( system, workflow, etc) to check that sender is from a "Safe Sender" domain.
+ Update all the places that use "Person" as an available lava merge field for the current person to use "CurrentPerson" instead
+ Update the lava ( liquid ) fields used by the PageMenu block to have the same format as all other lava merge fields in Rock. They are all now mixed case fields. This requires all Page Menu templates and template include files to be updated. Migration should fix all of them.
+ Fix issue with row selection not working on Dynamic Data Block.
+ Update the 'CurrentPerson' lava merge field in workflows to be just 'Person' so that it is consitent with all other areas in rock that provide currently logged in person as a 'Person' merge field.
+ Add ability to delete channel items from the Tools &gt; Content page
+ Twilio SMS transport will now report back messages that are undeliverable. (Fixes #715)
+ Fix HTMLEditor issue in IE when toggling between source and WYSIWYG modes (Fixes #588)
+ Merge branch 'feature-ns-modal-changes' into develop
+ added "Include Child Groups" option to the "In Group" and "Not in Group" Person dataview filters
+ Update the WorkflowAttribute and WorkflowTextOrAttribute attributes to be able to filter the list of available attributes by those that have specific field types allowed by the action that uses them.
+ Merge branch 'pre-alpha' into develop
+ Update the location and naming convention of the Lava (liquid) include files to  be RockWeb/Themes/[Theme]/Assets/Lava/[Template].lava instead of RockWeb/Themes/[Theme]/Assets/Liquid/_[Template].liquid.
+ New prayer requests that were not auto-approved will now show up in the Prayer Requests list without having to check 'Show Expired Requests' (Fixes #685).
+ Added DISC Result block for viewing results after clicking the DISC badge, and added the DISC Request person profile action with corresponding Workflow.
+ Add additional 'Text Option' settings to the financial Transaction Entry and Scheduled Transaction Edit blocks to make more of the text captions configurable.
+ Update the Send Email, Send SMS, and Send System Email workflow action types so that the recipient field supports Lava.
+ Fix attribute category ordering (Fixes #716).
+ Update the System Info block's 'Clear Cache' option to also delete all the files in the App_Data/Cache folder.
+ Rename the 'Send Email Template' workflow action to 'Send System Email' since it actually allows sending to a system email, and not a communication template.
+ Fix issue with workflow entry form notifications including fields that are not marked visible
+ Exceptions that occur when using the Rock API will now return an error and log the exception to the Rock ExceptionLog
+ Update communication templates to correctly use 'Communication.MediumData.*' instead of 'Communication.ChannelData.*' lava merge fields.
+ Fixed mis-labeled dropdown on bulk update block (Fixes #708)
+ Added block properties to the Register block (AccountEntry.ascx) to set the Connection Status and Record Status used when creating new individuals. Default values are 'Web Prospect' and 'Pended' (Fixes #699).
+ Merge pull request #704 from tcavaletto/develop

Merging Central's latest DISC code+ Fix the 'Copy Communication' option in Communication History so that it correctly copies all the channel data (e.g. email body text) to the new communication.
+ Subject value in email templates can now be removed (Fixes #692).
+ Fixed issue where inactive workflow types were linked. Inactive items will now still show, so that they can be managed, but will not link to the entry form. (Fixes #695)
+ Fixed issue where public account names were not being used on the transaction entry page (Fixes #696)
+ Update Lava templating engine to support enumerations correctly (i.e. Gender) (Fixes #689)
+ fixes several problems associated with Global Attributes not being properly merged in communication templates for email exceptions, account confirmation, and account creation during new giving transaction (Fixes #684).
+ Update the Group Tree View block so that it only shows groups of the selected type when specific types are selected in the settings (Fixes #671).
+ Dataview filter selector now shows tooltip with the description of the filter
+ Report field selector now shows tooltip with the description of what the field is (if it has a description)
+ "Person Link" report field is now called "Person Name" and is listed in the Common fields.  It now has the option to show as a link, and whether to show 'FirstName Lastname', or 'LastName, Firstname'
+ Fix issue with adding multiple existing people to a family not displaying the correct tab in add person dialog window (Fixes #666).
+ Update the checkin app so that when search page is displayed, the number field already has focus (Fixes #623).
+ Fix issue with check-in codes not printing on labels
+ Added a way to set the requester of the prayer request on the Prayer Request detail block.  Also now shows the requester if one was set.
+ Corrects bug where creating a copy of a communication retained the original create date instead of using the current datetime (Fixes #651).
+ Fix bug that was preventing GlobalAttributes from being included in new account creation block emails (Fixes #649).
+ Updated Rock REST API to use latest version OData V3. Now there is support for most of the standard OData V3 Query Types and also an endpoint at api/$metadata to see the schema
+ Correct bug that was preventing prayer requests with comments from being deleted until each comment was removed first (Fixes #644).
+ Fixed a few REST endpoints that were not configured to check authentication correctly (Fixes #647).
+ Start migration for background checks
+ Added workflow actions to add and remove a person from an organization tag.
+ Fix exception that would occur on person bio block if person had a phone number with an invalid phone number type
+ new Video and Audio field types that can be used to display Video and Audio content.
- added Rock/Scripts/Rock/Controls/mediaPlayer.js that video and audio uses
+ Update the 'Set Attribute From Entity' workflow action to support any entity type instead of just person or group.
+ Fix issue with attribute values becoming unavailable when a cached item is reloaded from a model
+ Update the Memo field type to have a configurable number of rows (instead of always being 3).
+ DISC Person Profile Badge complete.
+ Removed the 'Current Date' checkbox option from the Date field type, and added it to a new FilterDate field type. This field type is intended to only be used for report filters when specifying date comparisons (Fixes #627).
+ added "Not in Group" and "Not in Group of Group Type" dataview filters
+ Added new BinaryFileType of Media File that can be used as the File Type for Video and Audio
- fixed up BinaryFileType of FileSystem to have a RootPath attribute
+ DISC PersonProfile Badge created.
+ Merge branch 'hotfix-1.1.2' into develop

Conflicts:
	Applications/Wpf/StatementGenerator/StatementGenerator.csproj
	Applications/Wpf/WpfProjects.sln
+ new AudioFieldType that can be used to add Audio content
+ New VideoFieldType that can be used to upload and include video in content pages
- big refactor of BinaryFile and Storage Provider to use Stream instead of byte[]
+ Update the MyWorkflows block so that when the 'All Types' option is used, only the workflow types that user is authorized to edit are displayed (in addition to any types that have active activities assigned to current user).
+ Update the 'Person/{PersonId}/StaffDetails' route for the person profile security page to be 'Person/{PersonId}/Security' instead.
+ Remove the 'Blocs' page route as it is no longer needed.
+ Updated how Liquid (Lava) include files are referenced. Previously they always needed to be in the current theme's Assets/Liquid folder, and required an underscore prefix and suffix of 'liquid'. Now they are referenced using the actual path and file name relative to the the website's root folder.  For example a previous include of {% include 'PageNav' %} would now be {% include '~~/Assets/Liquid/_PageNav.liquid' %}. All of Rock's Liquid include statements have been updated to reflect this change.
+ Update Content Channel functionality to allow channel-specific item attributes
+ Update the item list on channel view block to include columns for the attributes that have been configured with the 'Display in Grid' option enabled
+ Add workflow action and webhook for background check
+ Update the System Info block's clear cache option to also refresh the list of EntityTypes, FieldTypes, and BlockTypes.
+ Add new Encrypted Field Type and Attribute that can be used to store sensitive attribute values as an encrypted value in database (e.g. Passwords, SSN, etc ).
+ Add option to RockTextBox field type for using password mode on rendered textbox and update SMTP settings and Payment Gateway settings to use password mode ( Fixes #426 )

Merge branch 'feature-dt-password' into develop
+ Add a 'Delay' workflow action that will delay successful execution of action until a specified number of minutes have passed.
+ Update the Content Channel Dynamic block to allow sorting items by attribute values in addition to item properties.
+ Merge pull request #608 from tcavaletto/develop

As mentioned in my email to community@sparkdevnetwork, this allows the Member Count column to be hidden
