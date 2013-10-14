<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TagReport.ascx.cs" Inherits="RockWeb.Blocks.Administration.TagReport" %>

<h4><asp:Literal ID="lTaggedTitle" runat="server"></asp:Literal></h4>

<Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" OnRowSelected="gReport_RowSelected" />
