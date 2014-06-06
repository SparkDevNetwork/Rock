<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ResidentGradeRequest.ascx.cs" Inherits="RockWeb.Plugins.com_ccvonline.Residency.ResidentGradeRequest" %>

<asp:UpdatePanel ID="updatePanel" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfCompetencyPersonProjectId" runat="server" />
        <div class="well">
            <fieldset>
                <legend>
                    <asp:Literal ID="lblLoginTitle" runat="server" Text="Facilitator Login" /></legend>

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" ValidationGroup="vgLogin" />

                <asp:Literal ID="lblLoginInstructions" runat="server" Text="The teacher of this competency or an authorized grader must login to grade this project." />

                <Rock:RockTextBox ID="tbUserName" runat="server" Label="Username" Required="true" ValidationGroup="vgLogin" />
                <Rock:RockTextBox ID="tbPassword" runat="server" Label="Password" Required="true" TextMode="Password" AutoCompleteType="Disabled" ValidationGroup="vgLogin" />

                <Rock:NotificationBox ID="nbWarningMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Warning" />

                <asp:Button ID="btnLogin" runat="server" Text="Login" CssClass="btn btn-primary" OnClick="btnLogin_Click" ValidationGroup="vgLogin" />

            </fieldset>
        </div>

        <fieldset>
            <legend>
                <asp:Literal ID="lblEmailRequestTitle" runat="server" Text="Email Request to Facilitator" /></legend>

            <asp:ValidationSummary ID="ValidationSummary2" runat="server" CssClass="alert alert-error" ValidationGroup="vgEmailRequest" />

            <Rock:DataDropDownList ID="ddlFacilitators" runat="server" Required="true" DataTextField="FullName" DataValueField="Id" SourceTypeName="Rock.Model.Person, Rock" PropertyName="FullName" ValidationGroup="vgEmailRequest" Label="Facilitator" />

            <asp:Button ID="btnSendRequest" runat="server" Text="Send" CssClass="btn btn-primary" OnClick="btnSendRequest_Click" ValidationGroup="vgEmailRequest" />

            <Rock:NotificationBox ID="nbSendMessage" ClientIDMode="Static" runat="server" NotificationBoxType="Info" />

        </fieldset>
    </ContentTemplate>
</asp:UpdatePanel>
