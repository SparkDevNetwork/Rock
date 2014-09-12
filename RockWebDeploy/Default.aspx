<%@ Page Language="C#" AutoEventWireup="true" Inherits="System.Web.UI.Page" %>

<script runat="server">

    protected override void OnLoad( EventArgs e )
    {
        base.OnLoad( e );

        Context.Session["RockExceptionCode"] = "66";
        throw new Exception( @"
URL routing has not been configured correctly. This is usually due to an exception occurring during 
application start-up. Please check the exception log for any recent exceptions (you may have to 
check the 'ExceptionLog' database table directly if you are not able to view exceptions in Rock). In 
cases where Rock was not able to access the database, it logs errors to the 
/App_Data/Logs/RockExceptions.csv file. Make sure to check this file also for any recent exceptions." );
        
    }

</script>

