<%@ Page Language="C#" AutoEventWireup="true" CodeFile="RESTHelp.aspx.cs" Inherits="RESTHelp" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head id="Head1" runat="server">
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <style type="text/css">
        BODY
        {
            background-color: white;
            color: #000000;
            font-family: Verdana;
            margin-left: 0;
            margin-top: 0;
        }
        #content
        {
            font-size: 0.7em;
            margin-left: 30px;
            padding-bottom: 2em;
        }
        A:link
        {
            color: #336699;
            font-weight: bold;
            text-decoration: underline;
        }
        A:visited
        {
            color: #6699CC;
            font-weight: bold;
            text-decoration: underline;
        }
        A:active
        {
            color: #336699;
            font-weight: bold;
            text-decoration: underline;
        }
        .heading1
        {
            background-color: #003366;
            border-bottom: 6px solid #336699;
            color: #FFFFFF;
            font-family: Tahoma;
            font-size: 26px;
            font-weight: normal;
            margin: 0 0 10px -20px;
            padding-bottom: 8px;
            padding-left: 30px;
            padding-top: 16px;
        }
        pre
        {
            background-color: #E5E5CC;
            border: 1px solid #F0F0E0;
            font-family: Courier New;
            font-size: small;
            margin-top: 0;
            padding: 5px;
            white-space: pre-wrap;
            word-wrap: break-word;
        }
        table
        {
            border-collapse: collapse;
            border-spacing: 0;
            font-family: Verdana;
        }
        table th
        {
            background-color: #CECF9C;
            border-bottom: 2px solid white;
            border-right: 2px solid white;
            font-weight: bold;
        }
        table td
        {
            background-color: #E5E5CC;
            border-bottom: 2px solid white;
            border-right: 2px solid white;
        }
    </style>
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div id="content">
        <p class="heading1">Rock Web API Descriptions</p>
        <asp:GridView ID="gvRoutes" runat="server" AutoGenerateColumns="false" BorderWidth="0">
            <Columns>
                <asp:BoundField DataField="RelativePath" HeaderText="RelativePath" />
                <asp:BoundField DataField="HttpMethod" HeaderText="HttpMethod" HtmlEncode="false"/>
            </Columns>
        </asp:GridView>
    </div>
    </form>
</body>
</html>
