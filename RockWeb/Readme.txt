+ Fixed a bug with the photo upload that was preventing people from uploading their photo.
+ Add Communications, Group Member changes and Logins to the list of items that is logged in person history log (Fixes #382).
+ Added new report field called "Person Link" that can be added to a Person report. It will generate a link to the person's profile
+ The Age and Grade fields for Person reports are now sortable by clicking on the column header
- added ability to add Sorting support to a DataSelectComponent
- fixed Report Field picker not having things in alpha order
+ Merge branch 'feature-dt-caching' into develop
+ Update DefinedType and SystemEmail entities to use Categories from Category table instead of free-form text
+ Updated install package creation steps+ Added styling to required fields to help note that they are required.
+ moved settings for Full and Light CKEditor toolbars to RockWeb/Scripts/Rock/Controls/htmlEditor.js
- stylecop FirstContributionDateFilter
+ added Total Giving Amount data select to reports which will show the total giving for each person with criteria of Date, Account(s), Amount
+ Added First Contribution field to Person reports to go along with the existing Last Contribution field
+ Merge
+ Fix Migration
+ added new First Contribution Date data filter for Person dataviews
+ added new Total Amount field to reports on Transactions
- Hide ForeignId columns from Reporting
+ added Total Amount dataview filter for Financial Transactions
+ added Dev Tools\Sql\Tool_FinancialTransactions_ClearCheckMicrHash.sql
+ Added shared project file allowing Rock.Mobile to access Rock Models.
+ Added new block for sending photo requests in bulk to a large set of people based on their family role, age, connection status, etc.
+ Add Home Phone, Cell Phone, and Email to the Add Family block
+ Add the ability to update information about several people at once
+ modified boolean to lower for mandrill header
+ Changed the 'Safe Sender Domain' setting to be a Defined Type instead of a Global Attribute
+ Merge branch 'feature-mp-transactionscanner' into develop
