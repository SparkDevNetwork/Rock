<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PrayerRequestEntry.ascx.cs" Inherits="RockWeb.Blocks.Prayer.PrayerRequestEntry" %>
<asp:UpdatePanel ID="upAdd" runat="server">
    <ContentTemplate>
        <asp:Panel runat="server" CssClass="panel panel-block" ID="pnlForm">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-praying-hands"></i> Add Prayer Request</h1>
            </div>
            <div class="panel-body">

                <asp:ValidationSummary ID="valValidation" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" Title="Warning" Visible="false" />

                <fieldset>
                    <asp:Panel ID="pnlRequester" CssClass="prayer-requester" runat="server">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" Required="true" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" Required="false" />
                        <Rock:EmailBox ID="tbEmail" runat="server" Label="Email" Required="false" />
                        <Rock:PhoneNumberBox ID="pnbPhone" runat="server" Label="Mobile Phone" />
                        <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                    </asp:Panel>

                    <Rock:ButtonDropDownList ID="bddlCategory" runat="server" Label="Category"></Rock:ButtonDropDownList>

                    <em ID="lblCount" runat="server" class="pull-right badge"></em>
                    <Rock:DataTextBox ID="dtbRequest" runat="server" Label="Request" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" SourceTypeName="Rock.Model.PrayerRequest, Rock" PropertyName="Text" placeholder="Please pray that..."></Rock:DataTextBox>

                    <Rock:AttributeValuesContainer ID="avcEditAttributes" runat="server"/>

                    <% if ( EnableUrgentFlag ) { %>
                        <Rock:RockCheckBox ID="cbIsUrgent" runat="server" Checked="false" Label="Urgent?" Text="Yes" Help="If 'yes' is checked the request will be flagged as urgent in need of attention quickly." />
                    <% } %>
                    <% if ( EnableCommentsFlag )
                        { %>
                        <Rock:RockCheckBox ID="cbAllowComments" runat="server" Checked="false" Label="Allow Encouraging Comments?" Text="Yes" Help="If 'yes' is checked the prayer team can offer encouraging comments on the request." />
                    <% } %>
                    <% if ( EnablePublicDisplayFlag )
                        { %>
                        <Rock:RockCheckBox ID="cbAllowPublicDisplay" runat="server" Checked="false" Label="Allow Publication?" Text="Yes" Help="If you check 'yes' you give permission to show the request on the public website." />
                    <% } %>
                </fieldset>

                <Rock:BootstrapButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save Request" DataLoadingText="Saving..." OnClick="btnSave_Click" CssClass="btn btn-primary" CausesValidation="True"/>

            </div>



        </asp:Panel>

        <asp:Panel runat="server" ID="pnlReceipt" Visible="False" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-praying-hands"></i> Add Prayer Request</h1>
            </div>
            <div class="panel-body">

                <h2>Request Submitted</h2>
                <Rock:NotificationBox ID="nbMessage" runat="server" NotificationBoxType="Success" ></Rock:NotificationBox>

                <asp:Button ID="btnAddAnother" runat="server" Text="Add Another Request" OnClick="btnAddAnother_Click" CssClass="btn btn-link"/>

            </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>

