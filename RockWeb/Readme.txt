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
