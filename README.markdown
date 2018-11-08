<div align="center">

Rock RMS (NewPointe Edition)
=======

![NewPointe + Rock RMS](https://newpointe.blob.core.windows.net/newpointe-webassets/Default/940fb09cf280462f997d556af951a608_np_rock_small.png)

[![Website][website-badge]][website-link]
[![Build][build-badge]][build-link]
[![Tests][tests-badge]][tests-link]

![Campuses][campuses-badge]
![attendance][attendance-badge]
![Trend][trend-badge]
![ERAs][era-badge]

</div>


[NewPointe Community Church](https://newpointe.org) is a large, non-denominational church with multiple locations across Northeast Ohio. Our mission is to lead people to realize and reach their full potential in Jesus Christ.

[Rock RMS](https://rockrms.com) is the open source Church Management System (ChMS/RMS/CMS) behind most of our technical endeavors. It runs everything from Sunday morning check-in to the public-facing website. It's built on [ASP.NET Webforms](https://www.asp.net/web-forms) and uses [Entity Framework 6](https://docs.microsoft.com/en-us/ef/ef6/), [jQuery](https://jquery.com/), [Bootstrap 3](https://getbootstrap.com/docs/3.3/), and many other open source libraries.

## Dev Environment Setup

Note: remind me to make a pre-built VM image for this sometime.

### Prerequisites

- A Windows PC or Windows Virtual Machine.

  Rock is built on a Windows Server stack, and uses Windows tools for development. If you don't have a Windows machine available, don't worry! There are tools like VirtualBox (free) or VMWare ($$) that you can use to run Windows in a Virtual Machine. If you have a Mac you can also use solutions like Bootcamp or Parallels. Recommended specs: An SSD with at least 50GB of free space, a quad core processor, and 8gb of RAM.

- [Visual Studio Community 2017 or later][download-visual-studio]

  Most internal development is done in Visual Studio. Make sure to select the "ASP.NET and web development" workload when installing it.

- [SQL Server 2017 Developer or later][download-sql-server]

  You'll want some kind of database for testing and debugging. If you already have SQL Server installed somewhere you can use that. If not, you should download and install the free developer edition. Note: SQL Server 2017 and later also have a Linux version available if you want to mess with it - my brief testing shows that it will work with Rock (and might even be a bit faster) but it has not been thoroughly stress tested so I can't give you any guarantees.
  
- [SQL Server Management Studio][download-sql-server-management-studio] OR [Azure Data Studio (formerly known as SQL Operations Studio)][download-azure-data-studio]

  In order to directly interact with your test databases and configure your development server, you're going to need one of Microsoft's management solutions. SQL Server Management Studio (SSMS) is the standard full-featured solution, and the one I'd recommend. Azure Data Studio is a newer, cross-platform option for those who want to try it.
  
- Git

  We use Git for all of our version control. While I highly recommend learning and using the standard [Git command-line](https://git-scm.com) program, those who like clicking on things can use alternatives such as [GitKraken](https://www.gitkraken.com/git-client) or [SmartGit](https://www.syntevo.com/smartgit/). There are even some editors like Visual Studio and VS Code that have git built in!

### Basic Setup

#### SQL Server
1. Enable SQL Server authentication:
   - Open SSMS and log into your SQL Server.
   - Right click on your server in the Object Explorer tree and select Properties.
   - In the Properties dialog, open the "Security" page and make sure Server Authentication is set to "SQL Server and Windows Authentication mode"
   - Click OK to save any changes.
2. Add a new user login for testing and development:
   - Open up your server in SSMS.
   - In the Object Explorer tree, go to [Your Server] -> Security -> Logins.
   - Right click on the Logins folder and choose New Login...
   - Give your new login a nice name, something like "rock-dev-user".
   - Select the "SQL Server authentication" option, give it a good alphanumeric password, and uncheck the "Enforce password policy" box.
   - Open the "Server Roles" page and check the "dbcreator".
   - Click OK to save the new login.

#### Rock
1. Clone the repository into a local folder using Git:

   ```sh
   git clone https://github.com/NewPointe/Rock.git
   # or
   git clone git@github.com:NewPointe/Rock.git
   ```
2. Go get some lunch, that's going to take a while.
3. Open up `/RockWeb/web.ConnectionStrings.template.config`, fill in the template with your SQL Server information, and then save it as `/RockWeb/web.ConnectionStrings.config`. For the Database name, use something simple like "rock-test-db". Don't worry about creating the actual database just yet, we'll do that in the next section.

   It should look something like this when you're done:
   ```xml
   <?xml version="1.0"?>
   <connectionStrings>
     <add name="RockContext" connectionString="Data Source=localhost;Initial Catalog=rock-test-db; User Id=rock-dev-user; password=hunter2;MultipleActiveResultSets=true" providerName="System.Data.SqlClient"/>
   </connectionStrings>
   ```

#### Visual Studio

1. Open the Rock.sln file in Visual Studio.
2. Right click on the RockWeb project in the Solution Explorer and select "Set as StartUp Project"
3. Open up the Tools menu, got to the NuGet Package Manager Section and open the Package Manager Console.
4. At the top of the Package Manager Console, change the Default Project to "Rock.Migrations"
5. Then, inside the console, type "Update-Database" and hit enter to run the database update command. This should create a new database on your SQL Server and run all of the migrations needed for a brand new Rock install.
6. Go get some more lunch.

At this point you should be able to go to the Debug menu and click Start Debugging to build and open a local instance of Rock in your browser. Depending on how fast your computer's hard drive is, this will also probably take a while.

## Building and Deployment

All commits are automatically built and tested by [AppVeyor][build-link]. New changes are deployed in two stages: First, all changes are merged into the `np-beta` branch. Once AppVeyor has built that branch and verified that tests pass, you can use an AppVeyor deployment to deploy the build on the Beta server. Then, once you have thoroughly tested it on the Beta server, you can merge it into the protected `np-production` branch. Once AppVeyor has built the production branch you can wait for the next maintenance window and deploy it to the Production server.

## Rock Developer Resources

Rock has a lot of developer resources to help you get started:

* Rock [Developer Guides](https://www.rockrms.com/Developer)
* [Developer Q&A](https://www.rockrms.com/Rock/Ask/Developing) Forms
* [Community Slack](https://www.rockrms.com/slack)

## License
Rock released under the [Rock Community License](http://www.rockrms.com/license).

## Crafted By

A community of developers led by the [Spark Development Network](http://www.sparkdevnetwork.com/).


<!-- Link References -->

[website-badge]: https://img.shields.io/website-up-down-green-red/https/newpointe.org.svg?label=newpointe.org
[website-link]: https://newpointe.org

[build-badge]: https://img.shields.io/appveyor/ci/NewPointe/Rock.svg
[build-link]: https://ci.appveyor.com/project/NewPointe/Rock

[tests-badge]: https://img.shields.io/appveyor/tests/NewPointe/Rock.svg
[tests-link]: https://ci.appveyor.com/project/NewPointe/Rock

[campuses-badge]: https://img.shields.io/badge/dynamic/json.svg?label=campuses&url=https://newpointe.org/Webhooks/Lava.ashx/gh/stats&query=$.campusCount&colorB=lightgrey
[era-badge]: https://img.shields.io/badge/dynamic/json.svg?label=rock%20ERAs&url=https://newpointe.org/Webhooks/Lava.ashx/gh/stats&query=$.eraCount&colorB=lightgrey
[attendance-badge]: https://img.shields.io/badge/dynamic/json.svg?label=average%20attendance&url=https://newpointe.org/Webhooks/Lava.ashx/gh/stats&query=$.averageAttendance&colorB=lightgrey
[trend-badge]: https://img.shields.io/badge/dynamic/json.svg?label=attendance%20trend&url=https://newpointe.org/Webhooks/Lava.ashx/gh/stats&query=$.attendanceTrend&colorB=lightgrey

[download-visual-studio]: https://visualstudio.microsoft.com/downloads/
[download-sql-server]: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
[download-sql-server-management-studio]: https://docs.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
[download-azure-data-studio]: https://docs.microsoft.com/en-us/sql/azure-data-studio/download
