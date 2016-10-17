![Rock RMS](https://raw.githubusercontent.com/SparkDevNetwork/Rock/develop/Images/github-banner.png)

[![Build status](https://ci.appveyor.com/api/projects/status/om3ddkynyoobdnpf/branch/master?svg=true)](https://ci.appveyor.com/project/NewSpring/rock/branch/master)

Rock RMS is an open source Relationship Management System (RMS) and Application 
Framework. While Rock specializes in serving the unique needs of churches it's
useful in a wide range of service industries.  Rock is an ASP.NET 4.6 C# web application
that uses Entity Framework 6.0, jQuery, Bootstrap 3, and many other open source libraries.

Our main developer starting point site is [the wiki](https://github.com/SparkDevNetwork/Rock/wiki).

## Learn More

Jump over to our [Rock website](http://www.rockrms.com/) to find out more. Keep up to date by:

* [Reading our blog](http://www.rockrms.com/Rock/Connect)
* [Following us on Twitter](http://www.twitter.com/therockrms)
* [Liking us on Facebook](http://www.facebook.com/therockrms)
* [Reading the community Q & A](http://www.rockrms.com/Rock/Ask)
* [Subscribing to our newsletter](http://www.rockrms.com/Rock/Subscribe)

## License
Rock released under the [Rock Community License](http://www.rockrms.com/license).

## Crafted By

A community of developers led by the [Spark Development Network](http://www.sparkdevnetwork.com/).

## Running the VM

The VMDK file you should've received can be run in VirtualBox, Fusion, or Parallels.

The default Rock install is hosted at http://rock.dev and you can use IIS Manager to control access to it.

If you need to debug some code, open Visual Studio and click the green arrow to compile and attach the web project.

If you don't need to debug anything, you can develop themes or change code in the running site: `C:\Code\Rock\RockWeb`.

## Connections

C:\Rock\RockWeb has a `web.connectionstrings.config` that defines attachments to multiple servers.  The only one you'll probably need is local, which is `.\SQL` (shorthand for `localhost\SQL`).  

``` Note: If you get an error running Nuget or starting the project, make sure your website is set as the startup project (Solution Explorer will display it in bold).  ```

Use SQL Management Studio to connect to local or remote servers for queries or adjusting SQL settings.

## Maintenance

From time to time you should probably run `C:\Code\FreeSpace.bat` to clear out old files and keep your Windows install trimmed down.  This will also defrag your unused space, which allows you to shrink the disk from Fusion/VB after you shut the OS down.

## VM Checklist

- [ ] Create a new Windows 10 x64 machine
- [ ] Select an existing hard disk (VMDK)
- [ ] Set minimum CPU processors to 2 
- [ ] Set minimum RAM at 4GB
- [ ] Update your [git config](https://git-scm.com/book/en/v2/Getting-Started-First-Time-Git-Setup) using Git Bash
- [ ] Create your first Pull Request 
- [ ] Start with some plugins: [CodeMaid](https://visualstudiogallery.msdn.microsoft.com/76293c4d-8c16-4f4a-aee6-21f83a571496), [GhostDoc](https://visualstudiogallery.msdn.microsoft.com/46A20578-F0D5-4B1E-B55D-F001A6345748) and the [Gulp docs](https://webtooling.visualstudio.com/task-runners/gulp/)

## MOAR?

Take a peek at the Rock [Developer 101](http://www.rockrms.com/Rock/Developer/BookContent/16/16) book to get started.
