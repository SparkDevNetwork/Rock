<%@ Page Language="C#" AutoEventWireup="true" CodeFile="KeepAlive.aspx.cs" Inherits="KeepAlive" %>

<!DOCTYPE html>

<html>
<head>
    <title>Rock ChMS - Keep Alive</title>
</head>
<body>

    <div>
        <h1>I'm Alive!</h1>
        You've run across the Rock ChMS Keep Alive page.  This page is called internally by the Rock code to keep the website loaded
        in Microsoft IIS (the webserver).  This is important for two reasons:
        <ol>
            <li>Keeping the website loaded makes it faster.</li>
            <li>Rock relies on serveral tasks to run behind the scenes to manage the data and keep things ship-shape.  Out of the box these
                tasks run inside of IIS.  It's important that IIS keeps the site 'awake' so these tasks can be launched.
                <p>
                    <i>For Advanced Users:</i>
                    <br />
                    These tasks can be run outside of IIS as a Windows Service.  If you're database is large, or you have several
                    custom tasks/jobs, you should look into running the Job Scheduler as a Windows Service.  See the documentation
                    on how to configure this.
                </p></li>
        </ol>

        <p>Well... this is all probably more than you wanted to know.  So for most of you... move along... there's nothing to see here.</p>
    </div>

</body>
</html>
