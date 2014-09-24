+ Update the DatePicker and DateRangePicker controls to display dates in the format of the current culture (Fixes #527)
+ Add the option to configure the order of person attributes for each category on person profile page (Fixes #424).
+ Fix issue with address country not defaulting to the organization's country (Fixes #529)
+ Fix the "Country" attribute of the "State" defined type to use the correct defined type (Fixes #528).
+ Add options to workflow form attributes to allow hiding the label and/or specifing pre/post Html content to be rendered before or afer the field when displaying form to user.
+ Several updates to the RockUpdate system to make it more resilient.
+ Enable default RequestValidation for form values ( all other content type was already being validated ) and update the HTMLEditor and CodeEditor controls to HTML-encode their content prior to posting back to server ( Fixes #532 ).
+ Fix grid page size so that a user's selection is remembered across pages and login sessions
+ Add ability to select all visible rows at once when grid has a selection column ( includes Person Search Results, Photo Verification, Financial Batch List and Financial Transaction List )
