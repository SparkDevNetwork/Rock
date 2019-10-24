<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AssessmentList.ascx.cs" Inherits="Rockweb.Blocks.Crm.AssessmentList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        <asp:Literal ID="lAssessments" runat="server"></asp:Literal>

    </ContentTemplate>
</asp:UpdatePanel>