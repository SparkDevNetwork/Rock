<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonEdit.ascx.cs" Inherits="Rock.Web.Blocks.PersonEdit" %>
<%@ Register Assembly="Rock.Framework" Namespace="Rock.Validation" TagPrefix="cc1" %>

<br /><br />
<asp:Label ID="lblFirstName" runat="server" Text="First Name" AssociatedControlID="txtFirstName"></asp:Label> 
<asp:TextBox ID="txtFirstName" runat="server"></asp:TextBox>
<cc1:DataAnnotationValidator ID="valFirstName" runat="server" 
    ControlToValidate="txtFirstName" PropertyName="FirstName"  
    SourceTypeName="Rock.Models.Crm.Person, Rock.Framework" ForeColor="Red" />

<br />
<asp:Label ID="lblNickName" runat="server" Text="Nick Name" AssociatedControlID="txtNickName"></asp:Label> 
<asp:TextBox ID="txtNickName" runat="server"></asp:TextBox>

<br />
<asp:Label ID="lblLastName" runat="server" AssociatedControlID="txtLastName"></asp:Label> 
<asp:TextBox ID="txtLastName" runat="server"></asp:TextBox>

<br />
<asp:Button ID="btnUpdate" runat="server" Text="Update" OnClick="btnUpdate_Click" />
     

