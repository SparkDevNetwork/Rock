We have two types of tests here. Test that require and don't require a database.  
If your test doesn't require a database, just read item 0 below.
To get tests that require a db context to work do the following:  

   0) If you don't see a Test Explorer tab next to your Solution Explorer, select the "Test" option 
      from the main VS menu. Then select Windows > Test Explorer. You should then see the tab. Once
      build the test project, each test method (marked with a [Fact] attribute) should appear in the
      explorer.

      NOTE: If you don't see them you may need to delete your %TEMP%\VisualStudioTestExplorerExtensions
      folder (while VS is shut down) to get the tests to show up. 

   1) In the Rock.Tests folder, edit the app.config and add your local connection 
   string -- db connection string -- to it.

   2) Remove the "Skip" from one of the person model tests you want to run.

   3) Right-click "debug selected tests" option to run the test of your choice.

   4) When you're finished testing, add a Skip property to your test's Fact attribute 
      like this: [Fact( Skip = "Requires a db" )]

	  ...so these tests are not run in other people's CI environments which don't have a db
	  connection.  (At some point in the future, our plan is to resolve this so that these tests
	  will be executed in everyone's CI environments where tests are run.)


When this is working properly, you can even do something like this to load a real person object from the db:

    var ps = new PersonService( new Data.RockContext() );
    var person = ps.Get( 1 );

You are responsible for the contents of the db. Your tests can ONLY assume a stock 
seeded database has been loaded with the Sample Data. Adding new data, deleting, 
cleaning up, etc. is up to you.
