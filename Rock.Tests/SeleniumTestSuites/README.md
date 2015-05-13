Selenium Test Notes
===================
These Selenium tests are intended to QA some standard functionality in Rock and
relies on the standard data that comes with Rock and the Rock sample data.

Each component (DataViews, Groups, New Family) should have its own folder
containing a test suite and all the individual test cases.


##Startup Steps:

 1. If running for the first time, be sure to load the Rock Sample Data under Power Tools. You may also need to set your Base URL to something like "http://localhost:6229/".
 2. Set the run speed to medium (because sometimes a screen has not finished loading and Selenium gets moving a bit too fast.
 3. Load a test suite and press the "Play entire test suite" button.
 4. If you get too many spurious errors, try slowing the test run down a bit more.
 

## Notes for Test Builders
When developing new tests, you may find these selectors
are less fragile than the ones than the Selenium IDE chooses.  Consider replacing 

 * a input field via CSS endswith trick:

    css=input[id$='_tbUserName']
    
 * Select something in the tree view by name "Foundational Views":
    
	//div[@id='treeview-content']/ul/li[span[contains(text(), 'Foundational Views')]]/span

 * Selecting a complex `<SELECT>` via CSS that has an unpredictible middle section of the id:
    
	css=select[id^='ctl00_main_ctl33_ctl01_ctl06_fg'][id$='_BackgroundCheckDate_dtPicker']
