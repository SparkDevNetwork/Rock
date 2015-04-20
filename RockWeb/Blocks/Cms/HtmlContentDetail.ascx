<%@ Control Language="C#" AutoEventWireup="false" CodeFile="HtmlContentDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.HtmlContentDetail" %>

<asp:UpdatePanel runat="server" ID="upnlHtmlContent" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>

        <Rock:NotificationBox ID="nbApprovalRequired" runat="server" NotificationBoxType="Info" Text="Your changes will not be visible until they are reviewed and approved." Visible="false" />
        <asp:Literal ID="lHtmlContent" runat="server" />

        <%-- Edit Panel --%>
        <asp:Panel ID="pnlEditModel" runat="server" Visible="false">
            <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="lbSave_Click" Title="Edit Html">
                <Content>

                    <asp:UpdatePanel runat="server" ID="upnlEdit">
                        <ContentTemplate>
                            <asp:HiddenField ID="hfVersion" runat="server" />
                            <asp:Panel ID="pnlEdit" runat="server" Visible="false">

                                <!-- Approval -->
                                <asp:UpdatePanel ID="upnlApproval" runat="server">
                                    <ContentTemplate>

                                        <div class="alert alert-action">

                                            <asp:Label ID="lblApprovalStatus" runat="server" />

                                            <asp:Label ID="lblApprovalStatusPerson" runat="server" />

                                            <div class="pull-right">
                                                <asp:LinkButton ID="lbApprove" runat="server" OnClick="lbApprove_Click" CssClass="btn btn-primary btn-xs" Text="Approve" />
                                                <asp:LinkButton ID="lbDeny" runat="server" OnClick="lbDeny_Click" CssClass="btn btn-xs btn-link" Text="Deny" />
                                            </div>

                                            <asp:HiddenField ID="hfApprovalStatusPersonId" runat="server" />
                                            <asp:HiddenField ID="hfApprovalStatus" runat="server" />
                                        </div>
                                    </ContentTemplate>
                                </asp:UpdatePanel>

                                <div class="pull-right">
                                    <asp:Literal runat="server" ID="lVersion" Text="Version X | " />
                                    <asp:LinkButton runat="server" ID="lbShowVersionGrid" Text="History" OnClick="lbShowVersionGrid_Click" />
                                </div>

                                <!-- Edit Html -->

                                <Rock:DateRangePicker ID="drpDateRange" runat="server" Label="Display from" />

                                <Rock:HtmlEditor ID="htmlEditor" runat="server" ResizeMaxWidth="720" Height="140" />
                                <Rock:CodeEditor ID="ceHtml" runat="server" EditorHeight="280" />

                                <Rock:RockCheckBox ID="cbOverwriteVersion" runat="server" Text="Don't save as a new version" />

                            </asp:Panel>

                            <asp:Panel ID="pnlVersionGrid" runat="server" Visible="false" >

                                <div class="grid">
                                    <Rock:Grid ID="gVersions" runat="server" DataKeyNames="Id" DisplayType="Full" ShowActionRow="false" AllowPaging="true" PageSize="10">
                                        <Columns>
                                            <Rock:RockBoundField DataField="VersionText" HeaderText="Version" SortExpression="Version" />
                                            <Rock:RockBoundField DataField="ModifiedDateTime" HeaderText="Last Modified" SortExpression="ModifiedDateTime" />
                                            <Rock:RockBoundField DataField="ModifiedByPerson" HeaderText="By User" SortExpression="ModifiedByPerson" />
                                            <Rock:BoolField DataField="Approved" HeaderText="Approved" SortExpression="Approved" />
                                            <Rock:RockBoundField DataField="ApprovedByPerson" HeaderText="By" SortExpression="ApprovedByPerson" />
                                            <Rock:DateField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                                            <Rock:DateField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                                            <Rock:LinkButtonField Text="Select" OnClick="SelectVersion_Click" />
                                        </Columns>
                                    </Rock:Grid>
                                </div>

                                <asp:LinkButton runat="server" ID="lbReturnToEdit" CssClass="btn btn-primary" Text="Back" OnClick="lbReturnToEdit_Click" />

                            </asp:Panel>

                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
