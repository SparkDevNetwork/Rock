![Rock RMS](./Images/newspring-banner.jpg)

[![Build status](https://ci.appveyor.com/api/projects/status/om3ddkynyoobdnpf/branch/master?svg=true)](https://ci.appveyor.com/project/NewSpring/rock/branch/master)

Rock RMS is an open source Relationship Management System (RMS) and Application 
Framework. While Rock specializes in serving the unique needs of churches it's
useful in a wide range of service industries.  Rock is an ASP.NET 4.5 C# web application
that uses Entity Framework 6.0, jQuery, Bootstrap 3, and many other open source libraries.

Quick links: 

- [Getting Started](#getting-started) 
- [Running Rock](#running-rock) 
- [New Environments](#new-environments) 
- [Primary Differences](#primary-differences)
- [Learn More](#learn-more) 
- [Community](#community) 
- [License](#license)

## Getting Started

Take a peek at the Rock [Developer 101](http://www.rockrms.com/Rock/Developer/BookContent/16/16) book to get started.   Some more information can be found on the [wiki](../../wiki).

If you're completely new to .NET, C# and SQL Server, you may want to read through [this overview](http://www.hanselman.com/blog/WhatNETDevelopersOughtToKnowToStartIn2017.aspx).

## Running Rock

The VMDK file you should've received can be run in VirtualBox, Fusion, or Parallels.  If you're setting up a new machine, skip to [New Environments](#new-environments).

The default Rock install is hosted at http://rock.dev and you can use IIS Manager to control access to it.

If you need to debug some code, open Visual Studio and attach to `w3wp.exe`.  If you don't need to debug anything, you can develop themes or change Block code in the running site: `C:\Code\Rock\RockWeb`.

### Connections

`C:\Rock\RockWeb\web.connectionstrings.config` defines attachments to multiple servers.  The only one you'll probably need is local, which is `.\SQL` (shorthand for `localhost\SQL`).  

```
Note: If you get an error running Nuget or starting the project, make sure your website is set as the startup project (Solution Explorer will display it in bold).  
```

Use SQL Management Studio to connect to local or remote servers for queries or adjusting SQL settings.

### Migrations

If you need to build or update a new database, look for the Nuget Package Manager Console.  You can either remember this command every time: `update-database -ProjectName 'Rock.Migrations'`. 

Alternatively, [follow the guide](http://shouldertheboulder.com/Article?id=368) to set your own migration shortcut.  This will use the connection defined in `web.connectionstrings.config` to build or upgrade your database.

### Maintenance

Eventually you'll need to run `C:\Code\FreeSpace.bat` to clear out old files and keep your Windows install trimmed down.  This will also defrag your unused space, which allows you to shrink the disk from Fusion/VB after you shut the OS down.

## New Environments

This guide assumes you'll be running Rock inside Windows.  If that's not the case, [talk to us](https://twitter.com/newspringweb).

- Create a new Windows 10 x64 machine
- Select an existing hard disk (VMDK)
- Set minimum CPU processors to 2 
- Set minimum RAM at 4GB
- Update your [git config](https://git-scm.com/book/en/v2/Getting-Started-First-Time-Git-Setup) using Git Bash
- Create your first [Pull Request](../../pulls) 
- Visual Studio Community (free)
- SQL Server: LocalDB (free), Express (free), or Standard (not free)
- Install some recommended plugins: [CodeMaid](https://visualstudiogallery.msdn.microsoft.com/76293c4d-8c16-4f4a-aee6-21f83a571496), [GhostDoc](https://visualstudiogallery.msdn.microsoft.com/46A20578-F0D5-4B1E-B55D-F001A6345748) and [Gulp](https://webtooling.visualstudio.com/task-runners/gulp/)

If you haven't already experienced the joy of developing with IIS, we'd highly recommend it: faster build times, easier debugging, cross-platform browsing, and bypassing the occasional VS insanity.   You'll need to set up an IIS site pointed at `C:\Code\Rock\RockWeb` (or wherever you code from), your database has to [be configured](http://logicalread.solarwinds.com/sql-server-express-as-a-production-database/) for IIS requests, and Visual Studio has to run as Administrator (so you can attach to IIS).

If you don't want to go the IIS route or prefer to use Visual Studio for everything, you can still get cross-platform browsing with some [trickery](http://www.hanselman.com/blog/WorkingWithSSLAtDevelopmentTimeIsEasierWithIISExpress.aspx).

## Primary Differences

We try our best to stay in sync with the [base repository](https://github.com/SparkDevNetwork/Rock); however, we have a few primary differences:

### Repo Size

If you clone the base repository, you'll notice that there are a  _LOT_ of binary files.  This doesn't work so well for quick build and deployment times (even with caching certain folders).   So we've removed the following folders to reduce the overall size:

- ./Applications
- ./Checkin Labels
- ./Installers
- ./packages

Applications and Installers can be downloaded through the Rock Shop (for production) or from the base repo (for debugging/development).  We don't use the stock Check-in Labels; all our labels can be found [here](https://github.com/NewSpring/rock-attended-checkin/tree/master/Checkin%20Labels).  Packages are automatically downloaded in production or development via Nuget.

### Deploy Process

We use [Appveyor](https://www.appveyor.com/) to do continuous deployment to different environments.  It's fast, free (for one build at a time), and relatively easy to set up.  We depend on [Node/Norma](https://github.com/NewSpring/Norma) to add plugins to the build and [MSBuild](https://msdn.microsoft.com/en-us/library/dd393573.aspx) to compile the entire solution.   You'll notice the following files in our repo:

- ./appveyor.yml

  This file contains all the build settings for AppVeyor.  You can use the UI instead, but you won't be able to track changes (especially helpful if something breaks).

- ./Norma

  This file contains the build settings for Norma, such as plugins to download, files to copy, etc. 

- ./package.json

  This file contains all the dependencies for Norma.

- ./RockWeb/app_offline-template.htm

  This file displays a helpful user message while Rock is upgrading or rebooting.  This file exists in core but has been heavily customized with NewSpring branding.

- ./RockWeb/before-deploy.ps1

  This file specifies what should happen before a deploy, such as saving web.config and setting the "Rock is restarting" message.

- ./RockWeb/deploy.ps1

  This file specifies what should happen during a deploy, such as setting permissions, and restoring web.config.

  ./RockWeb/web.config

  For security reasons our deploy process ignores any Github changes to the web.config.  If there's an update in the Spark repo, you'll need to manually copy that change to each server.  Keep in mind that any change to `web.config` will restart IIS.

### Dev Environment

We have a lot of systems running in production: some we're proud of, and some we're not.  We also have a [native app](https://github.com/NewSpring/Holtzman) for iOS, Android, Windows, and others.  That means we often need to test systems (including Rock) in non-Microsoft environments.   We've customized a couple things to make testing a little more friendly: 

- ./Rock.sln

  This file contains links to all the projects necessary for Rock to run, as well as links to any [plugin projects](#custom-plugins).  It also specifies settings for the web project, such as URL.  

  We changed our local web project to be http://rock.dev instead of `RockWeb` with a URL of http://localhost:6229.  This allows us to load and debug Rock cross-platform.  See [New Environments](#new-environments) for more details.

### Custom Plugins

We've written a [few plugins](https://github.com/NewSpring?utf8=✓&q=rock-) for Rock, including a [CyberSource](https://github.com/NewSpring/rock-cybersource) gateway, a [CacheBreak](https://github.com/NewSpring/rock-cache-sync) project for API's, and an [Attended Check-in](https://github.com/NewSpring/rock-attended-checkin) module.  The following list details other minor blocks or packages we've added:

- All Staff Live

  This block displays a video feed on our internal homepage for weekly staff meetings.

- Dashboard Widget

  This block displays metric values using a Liquid template for staff dashboards.

- Headings

  This block adds headings to a layout with the option to set a column size.

- Metrics

  This block displays metric values or comparisons on a dashboard based on Campus, Group, Date, or Schedule context.

- Ooyala

  This block loads the Ooyala video player for a specific video content ID.

- Raven

  We use [Sentry](https://sentry.io) for additional error reporting from Rock.  The specific C# library we use is called [SharpRaven](https://www.nuget.org/packages/SharpRaven).  We've added dependencies for SharpRaven throughout Rock, including Rock.csproj, RockBlock.cs, and ExceptionLog.cs.

- Sentry Test

  This block allows you to send a custom error to Sentry to test your config.

- Workflow Alert

  This block displays a [bell icon](http://fontawesome.io/icon/bell-o/) next for each logged-in user when they have workflows to complete.

### WebAPI Upgrade

In order to fully support Twilio and other API calls, we upgraded the `Microsoft.AspNet.WebApi` packages to version 5.2.3.  The core version of Rock is at 5.2.2.



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