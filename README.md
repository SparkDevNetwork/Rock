![Rock RMS](https://raw.githubusercontent.com/SparkDevNetwork/Rock/develop/Images/github-banner.png)

[![Build status](https://ci.appveyor.com/api/projects/status/om3ddkynyoobdnpf/branch/master?svg=true)](https://ci.appveyor.com/project/NewSpring/rock/branch/master)

Rock RMS is an open source Relationship Management System (RMS) and Application 
Framework. While Rock specializes in serving the unique needs of churches it's
useful in a wide range of service industries.  Rock is an ASP.NET 4.5 C# web application
that uses Entity Framework 6.0, jQuery, Bootstrap 3, and many other open source libraries.

Quick links: [Getting Started](#getting-started) [Running Rock](#running-rock) [New Environments](#new-environments) [Learn More](#learn-more) [Community](#community) [License](#license)

## Getting Started

Take a peek at the Rock [Developer 101](http://www.rockrms.com/Rock/Developer/BookContent/16/16) book to get started.   Some more information can be found on the [wiki](../../wiki).

## Running Rock

The VMDK file you should've received can be run in VirtualBox, Fusion, or Parallels.

The default Rock install is hosted at http://rock.dev and you can use IIS Manager to control access to it.

If you need to debug some code, open Visual Studio and attach to `w3wp.exe`.  If you don't need to debug anything, you can develop themes or change Block code in the running site: `C:\Code\Rock\RockWeb`.

### Connections

`C:\Rock\RockWeb\web.connectionstrings.config` defines attachments to multiple servers.  The only one you'll probably need is local, which is `.\SQL` (shorthand for `localhost\SQL`).  

```
Note: If you get an error running Nuget or starting the project, make sure your website is set as the startup project (Solution Explorer will display it in bold).  
```

Use SQL Management Studio to connect to local or remote servers for queries or adjusting SQL settings.

### Migrations

If you need to build or update a new database, look for the Nuget Package Manager Console.  You can either remember this command every time: `update-database -ProjectName 'Rock.Migrations'`. Alternatively, [follow the guide](http://shouldertheboulder.com/Article?id=368) to set your own shortcut.  This will use the connection defined in `web.connectionstrings.config` to build or upgrade your database.

### Maintenance

Eventually you'll need to run `C:\Code\FreeSpace.bat` to clear out old files and keep your Windows install trimmed down.  This will also defrag your unused space, which allows you to shrink the disk from Fusion/VB after you shut the OS down.

## New Environments

This guide assumes you'll be running Rock inside Windows.  If that's not the case, [talk to us](https://twitter.com/newspringweb) because we'd be verrry interested.

- Create a new Windows 10 x64 machine
- Select an existing hard disk (VMDK)
- Set minimum CPU processors to 2 
- Set minimum RAM at 4GB
- Update your [git config](https://git-scm.com/book/en/v2/Getting-Started-First-Time-Git-Setup) using Git Bash
- Create your first Pull Request 
- Visual Studio Community (free)
- SQL Server: LocalDB (free), Express (free), or Standard (not free)
- Install some recommended plugins: [CodeMaid](https://visualstudiogallery.msdn.microsoft.com/76293c4d-8c16-4f4a-aee6-21f83a571496), [GhostDoc](https://visualstudiogallery.msdn.microsoft.com/46A20578-F0D5-4B1E-B55D-F001A6345748) and [Gulp](https://webtooling.visualstudio.com/task-runners/gulp/)

If you haven't already experienced the joy of developing with IIS, we'd highly recommend it: faster build times, easier debugging, cross-platform browsing, and bypassing the occasional VS insanity.   You'll need to set up an IIS site pointed at `C:\Code\Rock\RockWeb` (or wherever you code from), your database has to [be configured](http://logicalread.solarwinds.com/sql-server-express-as-a-production-database/) for IIS requests, and Visual Studio has to run as Administrator (so you can attach to IIS).

If you don't want to go the IIS route or prefer to use Visual Studio for everything, you can still get cross-platform browsing with some [trickery](http://www.hanselman.com/blog/WorkingWithSSLAtDevelopmentTimeIsEasierWithIISExpress.aspx).

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

