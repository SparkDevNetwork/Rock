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

 * Don't create Selenium Core extensions and use them in your tests.

 * Add a `setSpeed` command to set the speed of your test to about 500ms

	<table>
		<tr>
			<td>Command</td><td>Target</td><td>Value</td>
		</tr>
		<tr>
			<td>setSpeed</td><td>500</td><td></td>
		</tr>
	</table>

When developing new tests, you may find these selectors
are less fragile than the ones than the Selenium IDE chooses.  Consider replacing 

 * If you need something like a random number, use the `storeEval` command like this:
	
	<table>
		<tr>
			<td>Command</td><td>Target</td><td>Value</td>
		</tr>
		<tr>
			<td>storeEval</td><td>new Date().getTime()</td><td>x</td>
		</tr>
	</table>

	This will store a number (eg. 1431551879224) that can be used later with `${x}`

 * a input field via CSS ends-with trick:

    `css=input[id$='_tbUserName']`
    
 * Select something in the tree view by name "Foundational Views":
    
	`//div[@id='treeview-content']/ul/li[span[contains(text(), 'Foundational Views')]]/span`

 * Select a `<select>` under a `<div>` based on it having an option with a certain known value ("Women of the Bible"):

    `//div/select[option[contains(text(), 'Women of the Bible')]]/`    

 * Selecting a complex `<select>` via CSS that has an unpredictable middle section of the id:
    
	`css=select[id^='ctl00_main_ctl33_ctl01_ctl06_fg'][id$='_BackgroundCheckDate_dtPicker']`
