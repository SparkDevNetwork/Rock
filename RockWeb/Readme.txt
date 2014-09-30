+ Fixed tags to allow special characters (Fixes: #548) reference http://stackoverflow.com/questions/24688639/wcf-rejecting-request-with-400-bad-request-when-their-is-a-colon-in-the-url-da
+ Update code generation
+ Updated the communication 'Channel' to communication 'Medium' to avoid confusion with Content Channels.
+ Merge branch 'feature-dt-content' into develop
+ The Rock Updater now requires the oldest update package be installed first and each must be installed one at a time. It also provides a convenient Restart button after a successful update.
+ DataFilters now work on DateTime, Time, Numeric Attributes
+ Modified the CheckScannerUtility build process to support file paths that contain spaces.
+ Single-Select Attributes can now be used as a Filter in Dataviews ( fixes #539)
- fix issue that would cause a Javascript error in DataViews if an Attribute Name had special chars
- fix issue in DataViews that would cause a SQL error if a large number of records matched an Attribute Filter
+ The person who created a group, dataview, or report can now administrate it
+ Update the Dynamic Data block so that it does not HTML encode any content returned by the selected query.
+ Merge branch 'release-1.0.14-ccv' into develop
+ Fixes for CCV
<br/><h4><span title="This application needs to be updated. - Updated Check Scanner Installer" class="label label-warning">Check Scanner] - Updated Check Scanner Installer</span></h4>