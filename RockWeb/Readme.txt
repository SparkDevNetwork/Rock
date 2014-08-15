+ Added new pages to "Admin Tools &gt; Check-in Settings" to eliminate the need to also go to "Admin Tools &gt; General Settings &gt; Defined Types" to set some specific defined types related to check-in (Ability Levels, Label Merge Fields, etc.)
+ Added logic to New Communication to prevent sending communications to any recipient that is marked as deceased.
+ Fixed issue where images on the person profile page were not showing in Firefox (Fixes #500)
+ Add a test (dummy) financial gateway that is active by default so that financial transaction pages can render content by default
+ New VerifyPhoto and PhotoOptOut blocks for use with new Request Photo communication template.
+ Updated person profile updates to update the IsDeceased flag when a person is marked as Inactive with a reason of "Deceased".  (Fixes #475)
+ Added additional giving pages to the external website for managing giving profiles (aka Scheduled Transactions) and viewing previous gifts (aka Transactions). This should make the external giving tools 'feature complete' for 1.0.
+ Fixed typo in the Twilio integration where Twilio was spelled Twillio. IMPORTANT: if you setup your webhook with Twilio you will need to update it to fix the typo. Webhook address should be http://&lt;yourserver&gt;/Webhooks/Twilio.ashx
+ Update Html Content block to support using merge fields for any object currently in context for the page.
+ Add an option to the check-in admin screen to select the theme to use
+ Moved check-in configuration pages to their own parent page under "Admin Tools". Also changed the route for the check-in manager from "/managecheckin" to "/checkinmanager".
+ Update Family Edit block so that when adding a new family member, the option to add a new person is displayed first instead of the option to add existing person
+ Update locations (and maps) on group detail to display in the order defined by the Group Location Type defined type.
+ If an sms message comes in and the sender is not in the database the sender will now get a error message noting that their message did not go through. #476
+ Add job and block to download any scheduled financial transactions that have been processed by payment gateway during a specific date range.
+ Updated to latest Bootstrap (3.2.0)</releaseNotes>
