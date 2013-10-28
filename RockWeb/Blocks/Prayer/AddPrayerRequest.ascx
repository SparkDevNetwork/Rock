<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AddPrayerRequest.ascx.cs" Inherits="RockWeb.Blocks.Prayer.AddPrayerRequest" %>
<asp:UpdatePanel ID="upAdd" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" ID="pnlForm">

            <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" />
            <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />

                <fieldset>
                    <Rock:DataTextBox ID="tbFirstName" runat="server" Label="First Name" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="FirstName" placeholder="first name"/>
                    <Rock:DataTextBox ID="tbLastName" runat="server" Label="Last Name" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="LastName" placeholder="last name"/>
                    <Rock:DataTextBox ID="tbEmail" runat="server" Label="Email" TextMode="Email" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Email" placeholder="email"/>
                    
                    <Rock:ButtonDropDownList ID="ddlCategory" runat="server" Label="Category"></Rock:ButtonDropDownList>

                    <em ID="lblCount" runat="server" class="pull-right badge"></em>
                    <Rock:DataTextBox ID="txtRequest" runat="server" Label="Request" TextMode="MultiLine" Rows="3" MaxLength="10" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Text" placeholder="Please pray that..."></Rock:DataTextBox>

                    <% if ( enableUrgentFlag ) { %>
                        <Rock:RockCheckBox ID="cbIsUrgent" runat="server" Checked="false" Label="Urgent?" Text="Yes" Help="If 'yes' is checked the request will be flagged as urgent in need of attention quickly." />
                    <% } %>
                    <% if ( enableCommentsFlag )
                       { %>
                        <Rock:RockCheckBox ID="cbAllowComments" runat="server" Checked="false" Label="Allow Encouraging Comments?" Text="Yes" Help="If 'yes' is checked the prayer team can offer encouraging comments on the request." />
                    <% } %>
                    <% if ( enablePublicDisplayFlag )
                       { %>
                        <Rock:RockCheckBox ID="cbAllowPublicDisplay" runat="server" Checked="false" Label="Allow Publication?" Text="Yes" Help="If you check 'yes' you give permission to show the request on the public website." />
                    <% } %>
                </fieldset>

                <asp:LinkButton ID="btnSave" runat="server" Text="Save Request" OnClick="btnSave_Click" CssClass="btn btn-primary" CausesValidation="True"/>

        </asp:Panel>

        <asp:Panel runat="server" ID="pnlReceipt" Visible="False" CssClass="row-fluid">
            <h2>Request Submitted</h2>
            <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Success" ></Rock:NotificationBox>

            <asp:Button ID="btnAddAnother" runat="server" Text="Add Another Request" OnClick="btnAddAnother_Click" CssClass="btn btn-default"/>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

