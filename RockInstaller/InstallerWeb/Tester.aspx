<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="Tester.aspx.cs" Inherits="InstallerWeb.Tester" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <form id="form1" runat="server">
    <div>
        
        Server:
        <asp:TextBox ID="txtServer" Text="localhost" runat="server"></asp:TextBox>
        <br />
        
        Database:
        <asp:TextBox ID="txtDatabase" Text="RockJunk" runat="server"></asp:TextBox>
        <br />
        
        Username:
        <asp:TextBox ID="txtUsername" Text="Rockuser" runat="server"></asp:TextBox>
        <br />
        
        Password:
        <asp:TextBox ID="txtPassword" Text="rRUZew6tpsYBhXuZ" runat="server"></asp:TextBox>
        <br />

        <asp:Button ID="btnExecute" Text="Try" runat="server" OnClick="btnExecute_Click" />

        <asp:Literal ID="lMessages" runat="server"></asp:Literal>

    </div>
    </form>
</body>
</html>
