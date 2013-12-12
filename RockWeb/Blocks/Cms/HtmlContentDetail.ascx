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
                        <asp:Panel ID="pnlEdit" runat="server" Visible="false" Height="440">

                            <!-- Approval -->
                            <asp:UpdatePanel ID="upApproval" runat="server">
                                <ContentTemplate>

                                    <div class="alert alert-action">

                                        <asp:Literal ID="lApprovalStatus" runat="server" />

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
                                <asp:LinkButton runat="server" ID="btnShowVersionGrid" Text="History" OnClick="btnShowVersionGrid_Click" />
                            </div>
                            
                            

                            <!-- Edit Html -->

                            
                            <Rock:DateRangePicker ID="pDateRange" runat="server" Label="Display from" />

                            <Rock:HtmlEditor ID="edtHtml" runat="server" ResizeMaxWidth="720" Height="140" />
                            <Rock:RockCheckBox ID="cbOverwriteVersion" runat="server" Text="Don't save as a new version" />

                            

                        </asp:Panel>

                        <asp:Panel ID="pnlVersionGrid" runat="server" Visible="false" Height="440">

                            <div class="scroll-container version-grid-scroll" style="width: 720px">
                                <div class="scrollbar">
                                    <div class="track">
                                        <div class="thumb">
                                            <div class="end"></div>
                                        </div>
                                    </div>
                                </div>
                                <div class="viewport" style="width: 690px">
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
                $('.version-grid-scroll').tinyscrollbar({ size: 150, sizethumb: 20 });
            });
        </script>

        <script type="text/javascript">
            // TODO: change approval status to non-approved if the html content is edited
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
