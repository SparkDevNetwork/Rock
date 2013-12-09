<%@ Control Language="C#" AutoEventWireup="false" CodeFile="HtmlContentDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.HtmlContentDetail" %>

<asp:UpdatePanel runat="server" ID="upPanel" ChildrenAsTriggers="false" UpdateMode="Conditional">
    <ContentTemplate>
        <%-- View Panel --%>
        <asp:Panel ID="pnlView" runat="server">
            <asp:Literal ID="lPreText" runat="server" />
            <asp:Literal ID="lHtmlContent" runat="server" />
            <asp:Literal ID="lPostText" runat="server" />
        </asp:Panel>

        <%-- Edit Panel --%>
        <Rock:ModalDialog ID="mdEdit" runat="server" OnSaveClick="btnSave_Click" Title="Edit Html" PopupDragHandleControlID="edtHtml">
            <Content>
                
                <asp:UpdatePanel runat="server" ID="upEdit">
                    <ContentTemplate>
                        <asp:HiddenField ID="hfVersion" runat="server" />
                        <asp:Panel ID="pnlEdit" runat="server" Visible="false" Height="420">
                            <div class="pull-right">
                                <asp:Literal runat="server" ID="lVersion" Text="Version X | " />
                                <asp:LinkButton runat="server" ID="btnShowVersionGrid" Text="History" OnClick="btnShowVersionGrid_Click" />
                            </div>
                            <Rock:DateRangePicker ID="pDateRange" runat="server" Label="Display Date Range" />

                            <Rock:HtmlEditor ID="edtHtml" runat="server" ResizeMaxWidth="720" Height="140" />
                            <Rock:RockCheckBox ID="cbOverwriteVersion" runat="server" Text="Don't save as a new version" />
                            <Rock:RockCheckBox ID="chkApproved" runat="server" Text="Approve" />

                        </asp:Panel>

                        <asp:Panel ID="pnlVersionGrid" runat="server" Visible="false" Height="420">

                            <div class="scroll-container version-grid-scroll" style="width:720px">
                                <div class="scrollbar">
                                    <div class="track">
                                        <div class="thumb">
                                            <div class="end"></div>
                                        </div>
                                    </div>
                                </div>
                                <div class="viewport" style="width:690px">
                                    <div class="overview">
                                        <Rock:Grid ID="gVersions" runat="server" DataKeyNames="Id" DisplayType="Light" ShowActionRow="false">
                                            <Columns>
                                                <asp:BoundField DataField="VersionText" HeaderText="Version" SortExpression="Version" />
                                                <asp:BoundField DataField="ModifiedDateTime" HeaderText="Last Modified" SortExpression="ModifiedDateTime" />
                                                <asp:BoundField DataField="ModifiedByPerson" HeaderText="By User" SortExpression="ModifiedByPerson" />
                                                <Rock:BoolField DataField="Approved" HeaderText="Approved" SortExpression="Approved" />
                                                <asp:BoundField DataField="ApprovedByPerson" HeaderText="By" SortExpression="ApprovedByPerson" />
                                                <Rock:DateField DataField="StartDateTime" HeaderText="Start" SortExpression="StartDateTime" />
                                                <Rock:DateField DataField="ExpireDateTime" HeaderText="Expire" SortExpression="ExpireDateTime" />
                                                <Rock:LinkButtonField Text="Select" OnClick="SelectVersion_Click" />
                                            </Columns>
                                        </Rock:Grid>
                                    </div>
                                </div>
                            </div>

                            <asp:LinkButton runat="server" ID="btnReturnToEdit" CssClass="btn btn-primary" Text="Back" OnClick="btnReturnToEdit_Click" />

                        </asp:Panel>

                    </ContentTemplate>
                </asp:UpdatePanel>
            </Content>
        </Rock:ModalDialog>

        <script type="text/javascript">
            Sys.Application.add_load(function () {
                $('.version-grid-scroll').tinyscrollbar({ size: 150 });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>