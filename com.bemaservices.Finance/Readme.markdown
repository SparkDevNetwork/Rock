# Finance Reports

## Continuing Giving Online Report
- Import Workflows into Rock
Upload both .json files in Assets folder as workflows.
Filters is a workflow that modifies the report page parameters.
Send Online Giving Email is a workflow that sends a generic email to a giving leader with information on how to give online.

- Create Page under Finance to add dynamic report and workflow entry to
Add workflow entry block and dynamic data block to page. 
Select the filter workflow in workflow entry. 
Import the SQL from ContinuingGivingOnline.sql into the dynamic data block

- Modify the SQL in Dynamic Data block
Change the first declare statements to match the environment 
