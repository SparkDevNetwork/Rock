# Rock UI and Data Recipes

These are techniques learned while running Rock verification plans in a browser and against SQL Server; add new ones at the end of the relevant section.

## Logging in and users

- **Browser login and logout**: log in at `/page/3` (internal login) and log out with `/Login?logout=True`. This is the quickest way to switch between admin, staff and test users in one browser.
- **One cookie jar per browser**: tabs of the in-app browser share cookies, so two tabs are not two users. Use a second browser profile, or headless HTTP sessions, when two users must be signed in at once.
- **Headless login**: `POST /api/Auth/Login` with `{ "Username": "...", "Password": "...", "Persisted": true }` and keep the returned cookie in a session object per user (for example a PowerShell `WebRequestSession`). That cookie is all Obsidian block actions need.
- **Restarts end sessions**: a site restart (web.config touch, DLL copy) invalidates browser and API sessions, so log in again afterwards.
- **The admin login's person may be "Admin Admin"**: on local databases the `admin` login's Person record can be named Admin Admin, not Alisha Marble. A form that matches people by name and email (for example Registration Entry) may then create a new "Alisha Marble" person instead of using the signed in one.
- **Person ids differ between databases**: do not hard code Ted or Pete ids from a test plan. Look them up through the login: `SELECT u.UserName, u.PersonId FROM UserLogin u WHERE u.UserName IN ('pfoster','tdecker')`.
- **Primary alias**: the primary PersonAlias is the row where `AliasPersonId = PersonId`: `SELECT Id FROM PersonAlias WHERE PersonId = <id> AND AliasPersonId = <id>`.
- **Admin proves scope, not authorization**: the full administrator passes every security check, so use it for golden paths and for "value must belong to this page's entity" checks. Use a staff user with limited rights to prove an authorization check blocks someone.
- **A ready VIEW-only user**: on the stock databases Ted Decker (WEB - Administration) has VIEW but not EDIT on most Admin Tools pages, which makes him a convenient VIEW-only user there. Some Admin Tools pages (for example the General Settings page) allow only RSR - Rock Administration, so grant VIEW explicitly if the user cannot open the page.
- **Admin may lack rights on restricted types**: RSR - Rock Administration does not automatically get VIEW on everything. For example General Person Document can be restricted to staff roles, so admin is not offered that document type. Run such tests as a person who has VIEW on the type.
- **Kiosk manager login**: the check-in Welcome manager screen accepts a PIN login. Create a temporary PIN UserLogin for a test person with SQL (PIN authentication entity type) and delete it afterwards.

## Security and permissions setup

- **Grant through the lock icon**: page, block, group, calendar, data view, metric, template and most entity security is edited through the item's lock (Security) button. Add a person (not a role) with Allow or Deny for one action.
- **Person rules need a low Auth Order**: stock role rules (for example RSR - Staff Workers Allow Edit) often come first, so a new person Deny never wins. Give person rules a negative `[Order]` (for example -2 and -3) so they are evaluated first, then clear the cache.
- **Add an explicit Deny on the "other" record**: rights inherit from group types, categories and parents (Staff Workers have Edit on the Serving Team group type and on calendars; a category grant covers every child). When a test needs "can edit A but not B", add an explicit Deny Edit for the person on B.
- **Page VIEW on the whole chain**: a limited user needs VIEW on the page and its parents. Check the person's Security tab and the page security before assuming a block check failed.
- **Block EDIT versus ADMINISTRATE**: many actions gate on block Edit, but some (reorder on certain lists, Run Now on metrics, Link to Existing Person in SMS Conversations) need Administrate or both. Read the block's C# to see which.
- **Restricting a record to admins only**: add View Allow for RSR - Rock Administration and then View Deny for All Users on the item.
- **Auth rows changed with SQL are cached**: after inserting or deleting `Auth` rows by SQL, clear the cache or restart before testing. Prefer the lock icon UI, which refreshes the cache itself.
- **Stale form technique**: a check behind a button that is not rendered for the user cannot be reached by posting (event validation rejects it). Grant the right, open the form as that user, revoke the right from a second session, then submit the already open form.
- **Revoke security grant tokens**: Admin Tools > Settings > System Configuration, Experimental Settings, Revoke Grants stores `core_SecurityGrantTokenEarliestDate`; tokens issued before it then return 401.

## Pages, blocks and cache

- **Test pages go under Installed Plugins**: add test pages as children of Admin Tools > Installed Plugins (page 420 on the stock database) via Admin Tools > Settings > Pages, then add blocks through the page's zone editor.
- **Stock page map (stock database ids)**: login 3, internal home 12, Zone Blocks dialog 16 (`/ZoneBlocks/{EditPage}/{ZoneName}`), Child Pages dialog 29 (`/Pages/{EditPage}`), Page Properties dialog 37 (`/PageProperties/{id}`), Pages 103 (`/admin/cms/pages?Page=<id>`), Global Attributes 51, Person profile 93, Group Viewer 113 (`?GroupId=`), Defined Types 119, Group Member Detail 140, Data Views 145, Reports 149, New Communication 168, Edit Person 183, Give Now 186, New Family 188, Attribute Categories 189, Check-in Configuration 201, Add Transaction 256, Metrics 280, Metric Value 281, Business Detail 283, Check-in Manager person 301, Manage Giving Profiles 321, Edit Giving Profile 322, Content Channel Detail 341, Calendar Detail 395, Event Detail 396, Occurrence 402, Registration Instance 404, Linkage 406, Connection Request 408, Connection Type 410, Opportunity 411, Registrant 415, Prayer Requests 431, File Manager 433, Check-in label editor 452, Tag Categories 497, Subscribe 498, Cache Manager 533, Workflow Import/Export 547, Step Entry 574, Mobile Application 582, Mobile Pages 584, System Communication 595, Documents 607, Connection Board 639, Edit Reminder 759.
- **Dialog pages open directly**: Rock's admin dialogs are real pages. Open `/PageProperties/<id>`, `/Pages/<id>` or `/ZoneBlocks/<id>/Main` in their own tab, which makes the hidden fields easy to reach. Page Properties opens straight in edit mode for a user with page EDIT and read only without it.
- **Find which page hosts a WebForms block**: `SELECT p.Id, b.Id FROM Block b JOIN BlockType bt ON bt.Id = b.BlockTypeId JOIN Page p ON p.Id = b.PageId WHERE bt.Path = '~/Blocks/<Area>/<Name>.ascx'`.
- **Find an Obsidian block's page and block Guids**: `SELECT p.Guid, b.Guid FROM Block b JOIN Page p ON p.Id = b.PageId JOIN BlockType bt ON bt.Id = b.BlockTypeId JOIN EntityType et ON et.Id = bt.EntityTypeId WHERE et.Name = 'Rock.Blocks.<Area>.<BlockName>'`.
- **Obsidian blocks share names with WebForms blocks**: the block picker lists both under one name. Insert the Obsidian block with SQL by BlockType Guid, then clear the cache:
  ```sql
  INSERT INTO Block (IsSystem, PageId, BlockTypeId, Zone, [Order], Name, OutputCacheDuration, Guid, CreatedDateTime, ModifiedDateTime)
  SELECT 0, <testPageId>, Id, 'Main', 0, Name, 0, NEWID(), GETDATE(), GETDATE() FROM BlockType WHERE Guid = '<blockTypeGuid>';
  ```
- **Some BlockType rows appear only after startup**: an Obsidian block can have an EntityType but no BlockType row until the site has started once. Restart before looking for it.
- **Pages made with SQL need SiteId**: set `Page.SiteId` by hand. A NULL SiteId makes the Zone Blocks dialog throw and makes some Obsidian actions return 500 about SiteId. Cloning an existing page row (for example Installed Plugins) is the easiest way to get every required column.
- **Block settings by SQL**: block settings are `AttributeValue` rows keyed by the block attribute and `EntityId = <block id>`, for example `SELECT av.Value FROM AttributeValue av JOIN Attribute a ON a.Id = av.AttributeId WHERE a.[Key] = 'FilterId' AND av.EntityId = <block id>`. Restart or clear the cache after changing them; some settings are cached until an app restart.
- **Clear cache after SQL changes**: block settings, attribute values, Auth rows, pages, blocks, schedules, group types, device attributes and entity type flags are cached. Use Admin Tools > Settings > System Settings > Clear Cache (or Cache Manager, or the admin bar cache button), or restart.
- **Restart by touching web.config**: touch `RockWeb\web.config`, then wait 30 to 45 seconds (until `GET /` answers). If SQL Server is busy the start can fail with EF timeouts; wait and touch it again.
- **Do not click Cache Manager's Enable Statistics casually**: it rewrites the `CacheManagerEnableStatistics` app setting in web.config, which restarts Rock.
- **Page context parameters**: to give a block an entity context, edit the page properties, Advanced > Context Parameters (for example `Rock.Model.PrayerRequest` = `PrayerRequestId`), or add a `PageContext` row with SQL and clear the cache.
- **Campus context without a setter block**: the `CampusId` page parameter sets the campus context for context aware blocks. The context is remembered in a cookie, so pass the parameter explicitly on every check.
- **IdKey and integer URLs both work**: most detail pages accept the IdKey or the integer id in the query string (for example `/page/408?ConnectionOpportunityId=<IdKey or id>`), unless the site disables predictable ids.
- **Clear the cache from a script**: the System Information block's `ClearCache` block action clears everything without opening an admin page. Find its Guids with `SELECT p.Guid, b.Guid FROM Block b JOIN BlockType bt ON bt.Id = b.BlockTypeId JOIN EntityType et ON et.Id = bt.EntityTypeId JOIN Page p ON p.Id = b.PageId WHERE et.Name = 'Rock.Blocks.Administration.SystemInformation'`, then POST `{"__context":{"pageParameters":{}}}` to `/api/v2/BlockActions/<pageGuid>/<blockGuid>/ClearCache` from an admin session. Using a scripted admin session (see `techniques.md`) works even while the browser is logged in as another user.

## WebForms controls

- **Hidden field tampering**: `document.querySelector('input[type=hidden][id$="_hfFieldName"]').value = '123';` then click the page's normal button. The server id ends with the field name.
- **Scope the selector when names repeat**: an uploader has its own `_hfBinaryFileId`, so exclude it (`[id$="_hfBinaryFileId"]:not([id*="fsFile"])`) or target the uploader (`[id$="fsFile_hfBinaryFileId"]`). Repeater rows carry the same field name per row; use `querySelectorAll(...)[n]` or a row container selector.
- **Control id prefixes differ per user**: `ctl14` for one user can be `ctl35` for another because blocks render differently. Use `[id$=...]` selectors and copy postback targets from the page as the acting user, never from another user's page.
- **Call `__doPostBack` directly**: read the button's `href` and call `__doPostBack('<target>', '<argument>')` yourself. Running the href through `eval` in a timeout did nothing in one case.
- **Direct postbacks skip client confirms**: calling the Delete postback directly bypasses the client side confirm dialog. Use this when the confirm is purely client side.
- **Find postback arguments in the page**: right click > Inspect the button, or search the page source for the argument prefix (`EditPerson^`, `DeleteDevice^`, `Pray^`, `update-preference`, `FileUploaded`). Update panel postbacks look like `__doPostBack('ctl00$main$...$upnlContent', 'Action^<id>')`.
- **Some blocks compare `__EVENTTARGET` with the ClientID**: for blocks that read `Request["__EVENTTARGET"]` themselves (Mobile Page Detail's `lbDragCommand`, the Connection Board's `lbJavaScriptCommand`) pass the ClientID with underscores: `__doPostBack(document.querySelector('[id$="_lbJavaScriptCommand"]').id, 'view|<id>')`. The `$` UniqueID form fails ASP.NET event validation.
- **Event validation blocks unrendered controls**: posting to a button or hidden field that was not rendered for the user throws an "Invalid postback" error page before block code runs. Report that as "not exercised", not as a pass of the block's own check, and use the stale form technique if the check must be reached.
- **Injecting a missing field**: when a field is simply not in the DOM at that moment (for example a hidden field outside the open edit panel, or a campus picker that is not rendered when only one campus is active), adding `<input type="hidden" name="<posted name>">` to the form can carry the value. If the server control is `Visible = false`, event validation rejects it.
- **Drag and drop is not scriptable**: dragula drags do not respond to synthetic mouse events. Send the postback the drop handler builds instead.
- **Grid reorder postback**: `__doPostBack('<grid UniqueID>', 're-order:<id>;<fromIndex>;<toIndex>')`. Copy the grid id from the reorder script in the page source.
- **Page zone editor reorder**: send the `re-order-panel-widget` postback to the `upPages` update panel. The Move dialog's zone list is `ddlMoveToZoneList`.
- **Mobile page block add and reorder**: `__doPostBack(<lbDragCommand ClientID>, 'add-block|<blockTypeId>|Main|0')` and `'reorder-block|<Zone>|<blockId>|<order>'`. The block type list only accepts types from the category currently shown, so switch the category first.
- **Workflow and SMS pipeline action add**: the page's `lbDragCommand` takes `add-action|<component>|<order>`.
- **Selects**: set them with JavaScript by index or value. ASP.NET ignores a posted value that is not in the server side option list and silently uses the first option, so adding an `Option` from the console does not select a value the server never offered.
- **AutoPostBack dropdowns**: changing the value in JS does not fire the server event. Call `__doPostBack('<dropdown UniqueID>', '')` (for example `dlgAddChild$ddlAddExistingItemChannel`) so dependent lists fill.
- **Checkbox lists that save on click**: some lists (Communication List Subscribe, schedule toolbox sign up) post back as soon as a checkbox is clicked; there is no Save button.
- **Rock item pickers**: clicking a tree node only sets hidden fields on the client. Server events fire from the picker's Select button (`[id$="_btnSelect"]`), so click it after choosing a node. To set a picker directly, write its `_hfItemId` (category, merge template) or `_hfSelectedItemId` (button dropdown pickers) and then click the related button (for example Open on the label editor).
- **Person picker**: write the person id to `pp<Name>_hfPersonId` before Save.
- **Modal saves**: a Rock modal's Save is the link `a[id$="<modalId>_serverSaveLink"]`; fill the modal's fields and click it (or run the `__doPostBack` in its href).
- **Confirm dialogs are bootbox modals**: click the OK button inside `.bootbox`. Overriding `window.confirm` does nothing. Some deletes (mobile application) ask twice. Tamper the hidden field after the confirm opens and before clicking its Delete.
- **Collapsed panel widgets still hold their inputs**: you can fill them without expanding.
- **Ace code editors**: the ZPL label editor and similar code fields are Ace: `ace.edit('<editor element id>').setValue('...')`.
- **Summernote HTML editors**: `$('<textarea selector>').summernote('code', '<p>html</p>')`. The "Details" Summernote may be a different field than its label suggests, so confirm in the database.
- **File uploads from JavaScript**: build the file and assign it to the uploader's file input, then fire change:
  ```js
  const dt = new DataTransfer();
  dt.items.add(new File([bytes], 'name.zip', { type: 'application/zip' }));
  input.files = dt.files;
  input.dispatchEvent(new Event('change', { bubbles: true }));
  ```
  This works for Rock file and image uploaders and zip uploaders (bytes decoded from base64).
- **List uploaders save on their own postback**: for uploaders that add a row to a list (benevolence documents, transaction images), set the hidden file id and then run the uploader's own postback from the page source (`postbackScript: '__doPostBack(...,'FileUploaded')'` or `'ImageUploaded'`).
- **Downloads without saving to disk**: replay the export postback with `fetch(location.href, { method: 'POST', body: formData })`, where `formData = new FormData(document.forms[0])` plus the clicked button's `name` and `value`. The response body is the file (check `content-disposition`); an HTML body means no file was sent.
- **Prove the save ran**: a required field you did not notice (Category on a system communication, Order Items By on a channel view filter, a marital status on New Family) stops the save with only a validation summary. Change a harmless field (Description, title) in the same save as a marker and confirm it in the database.
- **Grid row clicks vary**: some grids open the editor from a specific cell (Child Pages opens from the Id or Layout cell because the name is a link); some use a pencil postback while the row click navigates elsewhere (Tag Categories).
- **Answering assessments quickly**: click the first radio of each radio group name on the page and press Next or Finish; repeat per page.

## Obsidian blocks and controls

- **Block action endpoint**: `POST /api/v2/BlockActions/{pageGuid}/{blockGuid}/{ActionName}` with JSON `{ "__context": { "pageParameters": { ... } }, ...arguments }`. Only the login cookie is required, so a same origin `fetch` from the console works:
  ```js
  await fetch('/api/v2/BlockActions/<pageGuid>/<blockGuid>/<ActionName>', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ __context: { pageParameters: { } } })
  }).then(async r => r.status + ' ' + await r.text());
  ```
- **Capture first, then replay**: perform the action once in the UI with the Network tab open and copy the request URL and body. Change only the value under test so the request shape is right.
- **Page parameters live in `__context`**: actions read page parameters from `__context.pageParameters`, so a replay must send the same parameters the page had (and can change them for a test).
- **Entity context is a request header**: blocks that use context entities (campus, location, person) read them from `X-EntityContext-<Type>` request headers (for example `X-EntityContext-Campus`) in replays, not from page parameters.
- **Edit returns `entity`, Save takes `bag`**: many detail blocks return `{ entity, validProperties }` from Edit but Save expects a ValidPropertiesBox `{ box: { bag, validProperties } }`. Build the box from the Edit response's entity plus its validProperties.
- **validProperties must list what you change**: Save only applies properties named in `box.validProperties`; a missing list gives 400 "Invalid data." Copy the casing the capture uses (some blocks use flat PascalCase names such as `Name`, `GroupMemberAttributes`).
- **Clone a real bag instead of hand building**: bags missing fields the server expects (`attributeValues`, `address`, `searchKeys`, placement attribute dictionaries, `plannedVisitDate`, `startDateTime`) can throw a NullReference (500), and some of those happen after rows were already created. Take the bag from the Edit response or a related action (for example kiosk `EditFamily`) and modify it.
- **New records**: call Edit with an empty key, then Save with `idKey` empty (or `BinaryFileId=0` and similar); success usually returns 201. A new attribute in an attribute list needs `guid: null` (a guid means "edit existing") and `entityTypeQualifierColumn` and `entityTypeQualifierValue` as empty strings.
- **Always send `key`**: if `key` is left out of the JSON the framework answers 400 "Parameter 'key' is required." before the block's own check. Send `""` or `null` to exercise a "new record" path.
- **Picker and file values are ListItemBags**: send pickers, files, groups and defined values as `{ "value": "<Guid>", "text": "x" }`.
- **Find IdKeys in responses**: grid blocks return row `idKey` values from `GetGridData`; detail blocks put IdKeys and tokens in the block configuration in the page source (search for `idKey`, `securityGrantToken`, `experienceToken`).
- **Read permission flags from block data**: list and detail blocks expose flags such as `isEditable`, `isDeleteEnabled`, `isAddEnabled` in their configuration, which tells you what the server thinks the user may do without relying on a screenshot.
- **Controls API calls need a security grant token**: `/api/v2/Controls/<ControlAction>` requests (asset manager, group member requirement card) carry the page's `securityGrantToken`. Copy it from the captured request or the block configuration; some Save responses return a refreshed token that keeps working without a reload.
- **Some actions need a realtime connection id**: placement detach and similar actions expect `connectionId` in the body; without it the change can be made but the action returns 500.
- **Captcha blocks headless replays**: set the block setting Disable Captcha Support to Yes for the test (and back afterwards). Agents cannot complete a captcha.
- **AI chat responses are streams**: `SendMessage` returns a server sent event stream; read it as text. Debug logs appear only for block administrators.
- **Lowercase where Rock compares strings**: a stored selected-number person preference was ignored until its value was lowercase, and step status mapping keys must be lowercase Guids. When a Guid keyed value is silently ignored, try lowercase.
- **Person preferences can be set by SQL**: block person preferences use keys such as `block-<blockId>-<setting>`; letting the page create the preference once and then editing it is safer than inventing the key.
- **Mobile blocks without an app**: `Rock.Blocks.Types.Mobile.*` blocks can be placed on an internal test page and called through the same BlockActions endpoint.
- **Mobile Check-in Launcher**: use IdKeys (not integer ids) in `SaveAttendance` selections, set Disable Location Services on the block, and restart after schedule changes. The stock "4:30 (test)" schedule is always open and already linked to the group locations.
- **Kiosk actions take IdKeys or Guids** for the template, kiosk device and family; the device attributes (Allow Add Family Member, Allow Adding Individuals) gate the family and individual registration actions.
- **Grid row buttons**: Obsidian grid rows are `.grid-row` elements (not table rows), and the delete button is `.grid-row button[title="Delete"]`. Wide grids scroll sideways inside `.grid-obsidian`, so set `document.querySelector('.grid-obsidian').scrollLeft = 1e6` before clicking a button in the last column. Delete asks for confirmation in a modal with OK and Cancel buttons.

## Finding ids, IdKeys and Guids

- **Keys accepted by actions**: `Service.Get( string )` accepts IdKeys, Guids and plain integer ids unless the site disables predictable ids. Some actions or page parameters accept only IdKeys (some kiosk and giving actions), and a few read a page parameter only as an integer, so check the C# when a valid key is rejected.
- **IdKey from Lava**: render `{{ 123 | ToIdHash }}` in an HTML Content block on a test page (a Lava Tester block exists only if a plugin provides it).
- **IdKey from the API**: `/api/Lava/RenderTemplate` renders the same Lava but needs an admin session.
- **IdKey in a script**: IdKeys are Hashids of the id using the site's `DataEncryptionKey` with minimum length 10; a small Python Hashids helper reproduces them.
- **WebForms hidden fields may hold IdKeys**: on newer versions some hidden fields (for example the metric id on Metric Detail) hold an IdKey rather than an integer; read the field before tampering.
- **Useful system Guids** (from `Rock/SystemGuid`): Family group type `790E3215-3B10-442B-AF69-616C0DCB998E` (Id 10 on the stock databases), Known Relationships owner role `7BC6C12E-0CD1-4DFD-8D5B-1B35AE714C42`, Nameless Person record type `721300ED-1267-4DA0-B4F2-6C6B5B17B1C5`, Default binary file type `C1142570-8CD6-4A20-83B1-ACB47C1CD377`.
- **Entity type ids by name**: `SELECT Id FROM EntityType WHERE Name = 'Rock.Model.Page'` (Auth rows and attributes reference entity types by id, and the ids differ per database).
- **Person ids from logins** and **primary aliases**: see the Logging in section; never trust ids copied from another database.

## Data setup and cleanup with SQL

- **sqlcmd flags**: `sqlcmd -S . -d <db> -E -W -I`. Always pass `-I` (QUOTED_IDENTIFIER on) or deletes on tables with filtered indexes fail. Add `-b` so an error stops the batch and start with `SET NOCOUNT ON;`. A failed statement inside `BEGIN TRAN` without `-b` can leave the transaction open until the connection closes, so verify the data instead of assuming it committed.
- **Record baselines**: before a block's tests note counts and highest ids of the tables you will touch (`SELECT COUNT(*) FROM Person`, `MAX(Id)`), and compare after cleanup.
- **Use canaries and throwaway rows**: target throwaway records or canary files for anything a failed check could damage, and make negative tests discriminating (a tampered value that would visibly change something), with one control request using a legitimate value.
- **Rows that need more than the obvious columns**: `GroupMember` needs `GroupTypeId`; `ConnectionRequest` needs `ConnectionTypeId`; `SmsAction` needs `IsInteractionLoggedAfterProcessing`; a `Document` needs its own BinaryFile (two documents sharing one file cause an EntityReference error); `RelatedEntity` has a unique index, so duplicate rows must target different entities; a SQL created business needs `Person.GivingId` (`P<id>`) or giving actions return 404.
- **Values that mean something else than they look**: schedule RSVP Unknown is 3 (0 is No), so pending rows need `RSVP = 3`. Financial batch status 1 is Open, 0 Pending, 2 Closed; sample batches are closed, so create an open batch for transaction tests.
- **Workflow rows need a persisted type**: a workflow launched for a type without `IsPersisted = 1` (and stopped at a form) stores no `Workflow` row; turn it on temporarily to count launches.
- **Entities that will not save without a category or default**: a data view needs a category; a site needs a Default Page; a system communication needs a category; an achievement type needs attribute values; a connection request needs a connection status. Set them by SQL if the UI path does not.
- **Finance transactions by SQL**: give them a `FinancialPaymentDetail` with a currency type, or a refund fails while creating its batch ("The Name field is required").
- **Test Gateway payments**: card 4111111111111111 (or 4242424242424242), expiry typed as mm/yy, CVV 123. Next shows a confirm step and Finish gives a confirmation code. Replayed `ProcessTransaction` calls accepted a token of the form `token_<guid>`.
- **All-day schedule for check-in and experiences**: `iCalendarContent` with `DTSTART:20130501T000000`, `DTEND:20130501T235900` and `RRULE:FREQ=DAILY`, linked to group locations with `GroupLocationSchedule`.
- **Check-in kiosk recipe**: on `/checkin` pick a kiosk device and configuration and tick all real areas (the "Check in by Age / Ability Level / Grade" group types are base configurations, not areas). Add an all-day schedule. Edit Family needs device attribute Registration Mode = True and Kiosk Allow Editing Families; Roster Check Out and Staying need the Present tab, the group type's Allow Checkout attributes and the block's Enable Staying Button. Restart after these changes.
- **Mobile application recipe**: create it through the UI (Admin Tools > Settings > Mobile Applications, add, then the Pages tab). An app built with SQL made Mobile Page Detail fail with "Value cannot be null. Parameter name: s". Deleting the app from the grid leaves its REST user Person and UserLogin behind; delete those with SQL.
- **LMS data by SQL**: programs, courses, classes (`LearningClass.Id` is the group id), activities and participants can be inserted directly. `LearningParticipant.Id` is the GroupMember id and has no created or modified columns. Activity types are entity types (`Rock.Lms.FileUploadComponent`, and so on). Student completion rows may be needed before `CompleteActivity` works.
- **Interactive experiences**: an experience, a Short Answer action, an all-day schedule and an `InteractiveExperienceSchedule` row are enough; occurrences are created automatically while the schedule is active. The participant token is in the Live Experience page source.
- **Event Wizard**: New Event appears only with the block setting Allow Creating New Calendar Events on, and a template chosen without the picker's Select button leads to a NullReference later.
- **Registration tests**: if the instance has ended or not opened (Summer Camp 2019 on sample data), move its dates temporarily and restore them. A pending registration session can be made with a `RegistrationSession` row holding JSON registration data.
- **Assessments**: retaking needs `AssessmentType.MinimumDaysToRetake = 0` (temporarily); a pending request is an `Assessment` row with `Status = 0`.
- **Reminders**: the reminder bell lists only reminders that are not complete and are dated in the past, so create test reminders with a past `ReminderDate`.
- **Decline reasons and RSVP**: the RSVP decline reason list shows only when the occurrence has `ShowDeclineReasons` and `DeclineReasonValueIds` set; RSVP lists only show groups with at least one occurrence.
- **Communication list drafts**: the communication list's Hide Drafts filter is on by default; turn it off to see drafts.
- **Email sink**: point the SMTP transport at `localhost` port 25 and run a tiny SMTP sink (a Python `socketserver` script answering 220, 250 and 354 and logging `To:`, `Subject:` and attachment names). It stops with the session, so restart it before email tests. Send Test also needs the sender's `IsEmailActive = 1`. The SMS Test transport covers SMS.
- **Universal Search (Lucene)**: make the Lucene component Active through its settings UI, enable indexing per entity type in the Universal Search Control Panel, run the bulk load, and set `IsIndexEnabled` on the group type for groups. An empty result with no warning usually means one of these is missing.
- **File browser zip upload**: the encrypted `ZipUploaderEnabled` parameter comes from the File Manager block when its Zip Uploader Enabled setting is on; open that iframe URL directly. The HTML editor file browser can be opened as `/page/259?rootFolder=<encrypted root copied from an HTML editor>`.
- **Workflow export and import without disk**: export with the fetch download recipe, then upload the JSON body through `FileUploader.ashx` and set the import uploader to it.
- **Uploading files by HTTP**: multipart `POST` to `ImageUploader.ashx` or `FileUploader.ashx?isBinaryFile=T&fileTypeGuid=<guid>&isTemporary=True` returns `{Id, Guid, FileName}` for a temporary file created by the session's user.
- **Victim files for upload checks**: use a permanent file (`IsTemporary = 0`) uploaded by someone else. Seed person photos are often temporary files with no creator, which the upload rules deliberately accept, so they make misleading victims.
- **Photo replacement deletes the old file**: replacing a photo marks or cleans up the previous file, so a genuine upload on a real person or record can destroy the original. Use throwaway records, or note the original file so it can be restored.
- **Form Builder cleanup order**: delete the form's `WorkflowActionFormAttribute` and `WorkflowActionFormSection` rows, the `WorkflowActionType`, `WorkflowActionForm`, `WorkflowActivityType`, the attributes qualified by the workflow type, then the `WorkflowType`.
- **Connection request cleanup order**: `ConnectionRequestActivity`, then `ConnectionRequestWorkflow`, then `ConnectionRequest`, then clear the cache.
- **Restoring [Order]**: many seed rows share the same `[Order]` value, so record the original values and restore them by SQL rather than by replaying reorder actions.
- **Do not publish to the content library**: content library uploads go to the real, shared library. Never run that path on a local database.
- **Open batch with transactions (tested)**: the required columns are `FinancialBatch` (`Name`, `Status` 1 for Open, `ControlAmount`, `Guid`; `IsAutomated` defaults to 0), `FinancialPaymentDetail` (`CurrencyTypeValueId`, `Guid`), `FinancialTransaction` (`TransactionTypeValueId`, `FinancialPaymentDetailId`, `AuthorizedPersonAliasId`, `BatchId`, `TransactionDateTime`, `Guid`) and `FinancialTransactionDetail` (`TransactionId`, `AccountId`, `Amount`, `Guid`). Contribution is DefinedValue `2D607262-52D6-4724-910D-5C6E8FB89ACC` and Cash is `F3ADC889-1EE8-4EB6-B3FD-8C10F3C8AF93`. Put a `Verif <name>` marker in `TransactionCode` so the rows are easy to find and clean up. Deleting a transaction through Rock also deletes its `FinancialPaymentDetail`; deleting with SQL does not, so delete payment details yourself in cleanup.
- **Batches are not cached**: status and `IsAutomated` changes made with SQL take effect on the next page load without a cache clear.
- **Deletes write History rows**: deleting a transaction through the UI or a block action adds History rows on the batch and on the transaction (and an account line delete adds rows on the transaction and a MODIFY row on the batch). Cleanup should delete those rows too: note `MAX(Id)` of `History` before the tests and remove the new rows that reference your `Verif` records.

## Environment and platform limits

- **Windows on ARM cannot load Rock spatial types**: Rock's `Microsoft.SqlServer.Types` package ships only x86 and x64 native binaries, so geofence saves (Dynamic Heat Map, location polygons) silently leave `GeoFence` NULL on ARM. Run those on Intel or AMD. A SQL query is not a valid check because SQL Server handles spatial types itself.
- **Queued work runs on a 60 second timer or at shutdown**: workflow launches, search index requests and similar transactions may not appear in a short test. Wait over a minute or restart the site (shutdown drains the queue) before counting.
- **Unreachable printers are slow, not broken**: label printing to an unreachable printer takes about 30 seconds per attempt with "Could not connect to printer" messages; a print error can stand in for "the check passed".
- **External services are often missing**: AI providers, SMS numbers and transports, Google Maps keys, the content library connection, kiosk printers and Font Awesome Pro packages. Without a Google Maps key, post map values directly instead of drawing.
- **AI chat needs a linked provider**: `StartNewSession` returns 500 "uriString" null when no AI provider is configured.
- **Deploying a fix to the running site**: build the changed project (for example `dotnet build Rock.Blocks/Rock.Blocks.csproj`). The SDK-style projects set `CopyToRockWeb` to True, and `Directory.Build.targets` then copies the built DLL and PDB into `RockWeb\Bin` automatically, so there is nothing to copy by hand. The new DLL restarts the site (90 seconds or more). Obsidian front ends build with `npm run build` in `Rock.JavaScript.Obsidian.Blocks` (writes to `RockWeb/Obsidian/Blocks`) and in `Rock.JavaScript.Obsidian`.
- **Sample data is thin**: local databases often lack saved accounts, open batches, scheduled transactions, mobile apps, kiosks, prayer requests, reminders, LMS data, signature documents, personal devices, cache tags, Form Builder forms and communication flows. Plan to create them.
- **ViewState values cannot be tampered**: values kept in ViewState are MAC protected, so no browser tamper test is possible for them.
- **sqlcmd scripts from Git Bash**: `sqlcmd -i` with a short Windows temp path (for example `C:\Users\DANIEL~1\...`) fails from Git Bash with the misleading error "The -E and the -U/-P options are mutually exclusive." Run the script from PowerShell (`sqlcmd ... -i (Join-Path $env:TEMP '...')`) or copy it to a path without `~`.

## Things that look like failures but are not

- **No message on the page**: after a refused WebForms postback the warning is often in a hidden panel or not in the DOM at all. Judge by the database (unchanged values and ModifiedDateTime), not by the absence of a message.
- **"Invalid postback" error page**: that is ASP.NET event validation, not the block's check. Nothing changed, but the block's own guard was not exercised.
- **Disabled buttons still rendered**: some lists still draw a Delete column with `aspNetDisabled` buttons and no postback link for users without rights; they are not usable.
- **Copy only opens a form**: several Copy buttons open a pre-filled "... - Copy" form and create the record only when that form is saved.
- **HTTP 200 with `isSuccess: false`**: some actions report refusals in the body rather than with a 4xx status; others return 200 with nothing changed (for example `movedCount: 0`). Always confirm in the database.
- **Error text differs from the plan**: messages may have different wording, a stray `$` (from a plain string containing `${...}`), or a missing word. Treat the data outcome as the result and mention the text.
- **A 500 from your own request**: a NullReference or foreign key 500 from a hand built body (missing bag fields, missing `connectionId`, no `entityType`) is usually the replay's fault. Retry with a captured or cloned body before calling it a defect.
- **Empty lookup values**: if a SQL lookup for a victim Guid returns nothing, the replay sends an empty value and the result means nothing. Echo the values you substitute.
- **Stale cache after SQL**: "no services", old settings or rules that seem ignored right after a SQL change are usually cache; clear the cache or restart and retest.
- **Rows that appear late**: workflow rows and index results can take a minute or more (queued transactions); a send through the message bus may not reach the SMTP sink even though the communication was created.
- **Seed data and ids differ from the plan**: page ids, block ids, person ids and group ids vary per database, and some plan assumptions (which accounts are offered, which roles a user has) do not hold. Look the values up and record the substitution in the notes.
- **Pre-existing bugs**: when a check fails, use `git log`, `git blame -L` and `git show <commit>^:<path>` to see whether the behavior predates the change under test, and reproduce through the normal UI before reporting it as caused by the change.
- **Blank page right after a cache clear**: the first page load after clearing the cache re-registers entity, field and block types and can take about 10 seconds, showing only the page title meanwhile. Wait for the content (for example poll for `.grid-row`) before judging the page.
