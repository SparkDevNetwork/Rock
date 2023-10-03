<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AppleTvAppDetail.ascx.cs" Inherits="RockWeb.Blocks.Tv.AppleTvAppDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlBlock" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-tv"></i>
                    <asp:Literal ID="lBlockTitle" runat="server" Text="New Application" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlblInactive" runat="server" LabelType="Danger" Text="Inactive" Visible="false" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbMessages" runat="server" NotificationBoxType="Warning" />

                <asp:Panel ID="pnlView" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <asp:Literal ID="lViewContent" runat="server" />
                        </div>
                    </div>


                    <asp:Button ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server">

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbApplicationName" runat="server" Label="Name" Required="true" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" Checked="true" />
                        </div>
                    </div>

                    <Rock:RockTextBox ID="tbDescription" runat="server" Label="Description" TextMode="MultiLine" Rows="3" />

                    <Rock:CodeEditor ID="ceApplicationJavaScript" runat="server" EditorHeight="600" Label="Application JavaScript" />

                    <Rock:CodeEditor ID="ceApplicationStyles" runat="server" EditorHeight="400" Label="Application Styles" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbEnablePageViews" runat="server" Label="Enable Page Views" Help="Determines if interaction records should be written for page views" AutoPostBack="true" OnCheckedChanged="cbEnablePageViews_CheckedChanged" />
                            <Rock:NumberBox ID="nbPageViewRetentionPeriodDays" runat="server" Label="Page View Retention Period" Help="The number of days to keep page views logged. Leave blank to keep page views logged indefinitely." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="txtApiKey" runat="server" Label="API Key" Required="True" Help="The API key that will be used to secure your TV application." />
                            <Rock:RockCheckBox ID="cbEnablePageViewGeoTracking" runat="server" Label="Enable Page View Geo Tracking" Help="Enabling this feature will allow the PopulateInteractionSessionData job to begin performing geolocation lookup on the IP addresses in the Interaction Session data. This also requires setting up a IP Address Location Service found under System Settings." />                                                                                                
                        </div>
                    </div>

                    <Rock:PagePicker ID="ppLoginPage" runat="server" Label="Authentication Page" Help="The page on your public website that will be used in the authentication process." />

                    <Rock:AttributeValuesContainer ID="avcAttributes" runat="server" />

                    <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                </asp:Panel>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
