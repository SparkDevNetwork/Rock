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

                <asp:Panel ID="pnlEdit" runat="server" >

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="tbApplicationName" runat="server" Label="Name" />
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
                            <Rock:NumberBox ID="nbPageViewRetentionPeriodDays" runat="server" Label="Page View Retention Period" CssClass="input-width-sm" Help="The number of days to keep page views logged. Leave blank to keep page views logged indefinitely." />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="txtApiKey" runat="server" Label="API Key" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:Button ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" />
                    </div>
                    
                </asp:Panel>
                
            </div>
        
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>