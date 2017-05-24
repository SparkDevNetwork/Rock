![Rock RMS](./Images/newspring-banner.jpg)

[![Build status](https://ci.appveyor.com/api/projects/status/om3ddkynyoobdnpf/branch/master?svg=true)](https://ci.appveyor.com/project/NewSpring/rock/branch/master)

Rock RMS is a Relationship Management System (RMS) and plugin-friendly framework
written as an ASP.NET 4.5 C# web application. It uses Entity Framework 6.1, 
jQuery, Bootstrap 3, and other open source libraries.

The following guide documents how NewSpring Web uses and deploys Rock.

Quick links: 

- [Getting Started](#getting-started) 
- [Primary Differences](#primary-differences)
- [New Environments](#new-environments)
- [Running Rock](#running-rock) 
- [Deploy Process](#deploy-process)
- [Learn More](#learn-more) 
- [Community](#community) 
- [License](#license)

## Getting Started

Take a peek at the Rock [Developer 101](http://www.rockrms.com/Rock/Developer/BookContent/16/16) book to get started.   Some more information can be found on the [wiki](../../wiki).

If you're completely new to .NET, C# and SQL Server, you may want to read through [this overview](http://www.hanselman.com/blog/WhatNETDevelopersOughtToKnowToStartIn2017.aspx).  If you're completely new to Github and Git, read through the [Contribution](./CONTRIBUTING.md) guidelines.


## Primary Differences

We try our best to stay in sync with the [base repository](https://github.com/SparkDevNetwork/Rock); however, we have a few primary differences:

#### Repo Size

If you clone the base repository, you'll notice that there are a  _LOT_ of binary files.  This doesn't work so well for quick build and deployment times (even with caching certain folders).   So we've removed the following folders to reduce the overall size:

- `./Applications`
- `./Checkin Labels`
- `./Installers`
- `./packages`

Applications and Installers can be downloaded through the Rock Shop (for production) or from the base repo (for debugging/development).  We don't use the stock Check-in Labels; all our labels can be found [here](https://github.com/NewSpring/rock-attended-checkin/tree/master/Checkin%20Labels).  Packages are automatically downloaded in production or development via Nuget.

#### Deploy Configuration

We use [Appveyor](https://www.appveyor.com/) to do continuous deployment to different environments.  It's fast, free (for one build at a time), and relatively easy to set up.  We depend on [Node/Norma](https://github.com/NewSpring/Norma) to add plugins to the build and [MSBuild](https://msdn.microsoft.com/en-us/library/dd393573.aspx) to compile the entire solution.   You'll notice the following files in our repo:

- [./appveyor.yml](./appveyor.yml)

  This file contains all the build settings for AppVeyor.  You can use Appveyor's UI instead, but you won't be able to track changes (especially helpful if something breaks).  Formatting wise, Appveyor requires all spaces in this file (no tabs!).

- [./Norma](./Norma)

  This file contains the build settings for Norma, such as plugins to download, files to copy, etc. 

- [./package.json](./package.json)

  This file contains all the dependencies for Norma.

- [./RockWeb/app_offline-template.htm](./RockWeb/app_offline-template.htm)

  This file displays a helpful user message while Rock is upgrading or rebooting.  This file exists in core but has been heavily customized with NewSpring branding.

- [./RockWeb/before-deploy.ps1](./RockWeb/before-deploy.ps1)

  This file specifies what should happen before a deploy, such as saving web.config and setting the "Rock is restarting" message.

- [./RockWeb/deploy.ps1](./RockWeb/deploy.ps1)

  This file specifies what should happen during a deploy, such as setting permissions, and restoring web.config.

- [./RockWeb/web.config](./RockWeb/web.config)

  For security reasons our deploy process ignores any Github changes to the web.config.  If there's an update in the Spark repo, you'll need to manually copy that change to each server.  Keep in mind that any change to `web.config` will restart IIS.

#### Dev Environment

We have a lot of systems running in production: some we're proud of, and some we're not.  We also have a [native app](https://github.com/NewSpring/Holtzman) for iOS, Android, Windows, and others.  That means we often need to test systems (including Rock) from non-Windows environments.   We've customized a couple things to make testing a little more friendly: 

- [./Rock.sln](./Rock.sln)

  This file contains links to all the projects necessary for Rock to run, as well as links to any [plugin projects](#custom-plugins).  Note that if a plugin isn't downloaded locally, Visual Studio will gracefully ignore the reference.  `Rock.sln` also specifies the name and URL of the web project.

  `Note: We changed the Rock website name and URL to be http://rock.dev instead of RockWeb with a URL of http://localhost:6229.  This allows us to load and debug Rock cross-platform.  See `[New Environments](#new-environments)` for more details.`

#### Custom Plugins

We've written a [few plugins](https://github.com/NewSpring?utf8=✓&q=rock-) for Rock, including a [CyberSource](https://github.com/NewSpring/rock-cybersource) gateway, a [CacheBreak](https://github.com/NewSpring/rock-cache-sync) project for API's, and an [Attended Check-in](https://github.com/NewSpring/rock-attended-checkin) module.  We store plugins at the same folder level as Rock, then reference them in [./Rock.sln](./Rock.sln) with a relative path (`..\rock-cache-sync\cc.newspring.CacheBreak.csproj`).

The following list details other minor blocks or packages we've added:

- [Plugins\All Staff Live](./RockWeb/Plugins/cc_newspring/Blocks/AllStaffLive)

  This block displays a video feed on our internal homepage for weekly staff meetings.

- [Plugins\Dashboard Widget](./RockWeb/Plugins/cc_newspring/Blocks/Dashboards)

  This block displays metric values using a Liquid template for staff dashboards.

- [Plugins\Headings](./RockWeb/Plugins/cc_newspring/Blocks/Headings)

  This block adds headings to a layout with the option to set a column size.

- [Plugins\Metrics](./RockWeb/Plugins/cc_newspring/Blocks/Metrics)

  This block displays metric values or comparisons on a dashboard based on Campus, Group, Date, or Schedule context.

- [Plugins\Ooyala](./RockWeb/Plugins/cc_newspring/Blocks/Video)

  This block loads the Ooyala video player for a specific video content ID.

- [Plugins\Sentry Test](./RockWeb/Plugins/cc_newspring/Blocks/Sentry)

  We use [Sentry](https://sentry.io) for additional error reporting from Rock, and this block allows you to send a custom error to test your config.  The specific C# library we use is called [SharpRaven](https://www.nuget.org/packages/SharpRaven).  We've added dependencies for Sentry/SharpRaven throughout Rock, including [Rock.csproj](./Rock/Rock.csproj), [RockBlock.cs](./Rock/Web/UI/RockBlock.cs), and the [API Exception Log](.//Rock.Rest/Controllers/ExceptionLogsController.Partial.cs).

- [Plugins\Workflow Alert](./RockWeb/Plugins/cc_newspring/Blocks/WorkflowAlert)

  This block displays a [bell icon](http://fontawesome.io/icon/bell-o/) next for each logged-in user when they have workflows to complete.

#### WebAPI Upgrade

In order to fully support Twilio and other API calls, we upgraded the `Microsoft.AspNet.WebApi` packages to version 5.2.3.  The core version of Rock is at 5.2.2.


## New Environments

If you were already given a VMDK, most of these steps should be completed.  Also, this guide assumes you'll be running Rock inside a Windows environment, either as a VM or dual-boot.  If that's not the case, [talk to us](https://twitter.com/newspringweb) because we'd love to optimize our development/deployment strategy.

- Create a new Windows 10 x64 machine
- Select an existing hard disk (VMDK)
- Set minimum CPU processors to 2 
- Set minimum RAM at 4GB
- Download [Git](https://git-scm.com/downloads) (includes Git Bash ❤️)
- Update your [git config](https://git-scm.com/book/en/v2/Getting-Started-First-Time-Git-Setup) using Git Bash
- Create your first [Pull Request](../../pulls) 
- Download [Visual Studio Community](https://www.visualstudio.com/vs/community/) (free)
- Import the recommended [Visual Studio settings](./.vs/VisualStudio.vssettings)
- Download SQL Server: LocalDB (free), [Express](https://www.microsoft.com/en-us/sql-server/sql-server-editions-express) (free), or Standard (not free)
- Download [SQL Server Management Studio](https://msdn.microsoft.com/en-us/library/mt238290.aspx) (free)
- Install some helpful plugins: [CodeMaid](https://visualstudiogallery.msdn.microsoft.com/76293c4d-8c16-4f4a-aee6-21f83a571496), [GhostDoc](https://visualstudiogallery.msdn.microsoft.com/46A20578-F0D5-4B1E-B55D-F001A6345748) and [Gulp](https://webtooling.visualstudio.com/task-runners/gulp/)

If you haven't already experienced the joy of developing in Visual Studio with IIS, we'd highly recommend it: faster build times, easier debugging, cross-platform browsing, and bypassing the occasional VS insanity.   You'll need to set up an IIS site pointed at [./RockWeb](./RockWeb), your database has to [be configured](http://logicalread.solarwinds.com/sql-server-express-as-a-production-database/) for IIS requests, and Visual Studio has to run as Administrator (so you can attach to IIS when debugging).

If you don't want to go the IIS route or prefer to use Visual Studio for everything, you can still get cross-platform browsing with some [trickery](http://www.hanselman.com/blog/WorkingWithSSLAtDevelopmentTimeIsEasierWithIISExpress.aspx).  You'll also want to set a static IP on your VM and add a `hosts` reference to `rock.dev` with that IP.


## Running Rock

The VMDK file you should've received can be run in VirtualBox, Fusion, or Parallels.  If you're wondering what settings you should use, go back to [New Environments](#new-environments).

Our Rock version is hosted at `http://rock.dev` and you can use IIS Manager to control access to it.

If you need to debug code, open Visual Studio and attach to `w3wp.exe`.  If you don't need to debug, you can develop themes or change Block code in the running site: [./RockWeb](./RockWeb).

#### Connections

`./RockWeb/web.connectionstrings.config` defines your database attachments, although only one can be active (comment out the others).  The only connection you'll probably need is `localhost\SQL`.  If you're using SQL Server LocalDB, the Server property will be something like `(localdb)\ProjectsV13`.

`Note: If you get an error running Nuget or starting the Rock project, make sure RockWeb is set as the startup project (Solution Explorer will display it in bold).`

Here's a sample connection string:
```
<?xml version="1.0"?>
<connectionStrings>
  <!--<add name="RockContext" connectionString="Server=productionserver.azure.com; Initial Catalog=Rock;
        User Id=RockAdmin; password=SomeRidiculousPasswordAPasswordManagerCreated; MultipleActiveResultSets=true"
        providerName="System.Data.SqlClient" />-->
  <add name="RockContext" connectionString="Server=.\SQL; Initial Catalog=Rock;
        Integrated Security=true; MultipleActiveResultSets=true" providerName="System.Data.SqlClient" />
</connectionStrings>
```

#### Migrations

If you need to build or update a new database, look for the Nuget Package Manager Console and enter this command: `update-database -ProjectName 'Rock.Migrations'`.  Note that if you had any Nuget updates while Visual Studio was open, you'll need to reload Visual Studio to successfully run the Package Manager Console.

Alternatively, [follow the guide](http://shouldertheboulder.com/Article?id=368) to set your own migration shortcut.  This will use the connection defined in `web.connectionstrings.config` to build or upgrade your database.

#### Maintenance

Eventually you'll need to run [./FreeSpace.bat](./FreeSpace.bat) to clear out old files and keep your Windows install trimmed down.  This will also defrag your unused space, which allows you to shrink the disk from Fusion/VirtualBox after you shut the OS down.


## Deploy Process

We actively develop against our Rock fork and occasionally "cherry pick" changes from core, instead of waiting for the official or beta releases.  While this does mean we get faster features or bugfixes, we're also some of the first to find out about breaking changes.

We follow the below process to verify changes don't cause issues in production:

####1) Review recent changes in [Spark/pre-alpha-release](https://github.com/SparkDevNetwork/Rock/tree/pre-alpha-release).  
  
This branch is typically the last phase in the development cycle before a Rock release (develop -> pre-alpha-spark -> pre-alpha-release -> hotfix / master release).   Click [here](../../compare/pre-alpha-release...SparkDevNetwork:pre-alpha-release) to start a PR to [NewSpring/pre-alpha-release](../../tree/pre-alpha-release).

You'll want to be sensitive to the following changes: 
- Migrations (will require downtime and typically can't be rolled back)
- Check-in or workflow actions (could make for Sunday surprises, and not the good kind)
- Model or ModelService updates (can affect performance)

####2) Fix code merge conflicts and test any migrations against a local database.  

Since we've modified [multiple files](#primary-differences), you'll need to make sure changes (theirs or ours) aren't accidentally overwritten.  You'll also want to test migrations against a recent copy of our database so that any Sites, Blocks, or Attributes we may have tweaked don't cause dependency problems.
  
####3) Merge [NewSpring/pre-alpha-release](../../tree/pre-alpha-release) into [NewSpring/alpha](../../tree/alpha).  
 
Alpha is our "did anything break?" environment and is set to automatically deploy when code is committed.  Click [here](../../compare/alpha...NewSpring:pre-alpha-release) to start the PR to [NewSpring/alpha](../../tree/alpha).
   
####4) Merge [NewSpring/alpha](../../tree/alpha) into [NewSpring/beta](../../tree/beta) and deploy.

Beta is our "does everything work?" environment and does *not* automatically deploy when code is committed.  Click [here](../../compare/beta...NewSpring:alpha) to start the PR to [NewSpring/beta](../../tree/beta).

####5) Document all feature changes for the Web Operations team to review.  

If you reviewed well in step one, you'll have a list of changes to send for testing on Beta.  You'll want to document the default behavior as well as expected error states.  You may also need to clarify where the new or updated feature lives.

####6) After Operations signoff, merge [NewSpring/beta](../../tree/beta) into [NewSpring/master](../../tree/master) and deploy.

Master is our "go live" environment and does *not* automatically deploy.  After a successful build, you'll need to trigger a deploy from Appveyor during planned or co-ordinated downtime.

####7) Clear cache on newly deployed servers.

It's a good idea to clear cache on the Rock and Check-in servers after a deploy, so that any changes to Sites, Blocks, or Attributes are immediately present.  Theoretically IIS should pick up those changes after a restart, but it occasionally doesn't.


## Learn More

Jump over to our [Rock website](http://www.rockrms.com/) to find out more. You can also:

- [Read our blog](http://www.rockrms.com/Rock/Connect)
- [Follow us on Twitter](http://www.twitter.com/therockrms)
- [Like us on Facebook](http://www.facebook.com/therockrms)
- [Read the community Q & A](http://www.rockrms.com/Rock/Ask)
- [Subscribe to our newsletter](http://www.rockrms.com/Rock/Subscribe)


## Community

Rock is crafted by a community of developers led by the [Spark Development Network](http://www.sparkdevnetwork.com/).  You can join us on the forums at [Rock RMS](http://www.rockrms.com/Rock/Ask) or on our [Slack channel](http://rockrms.com/slack).


## License

Rock is released under the open-source [Rock Community License](http://www.rockrms.com/license).
