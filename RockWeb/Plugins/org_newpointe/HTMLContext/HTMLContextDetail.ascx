<%@ Control Language="C#" AutoEventWireup="true" CodeFile="HTMLContextDetail.ascx.cs" Inherits="RockWeb.Plugins.org_newpointe.HTMLContext.HTMLContextDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-code"></i> HTML Contexts
                </h1>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlList" runat="server" CssClass="grid grid-panel">

                    <Rock:Grid runat="server" ID="rgHtmlFragments" AllowSorting="true" OnGridRebind="rgHtmlFragments_GridRebind">
                        <Columns>
                            <asp:BoundField HeaderText="Block Id" DataField="BlockId" SortExpression="BlockId" />
                            <asp:BoundField HeaderText="Context Name" DataField="ContextName" SortExpression="ContextName" />
                            <asp:BoundField HeaderText="Context Parameters" DataField="ContextParameters" SortExpression="ContextParameters" />
                            <asp:BoundField HeaderText="Version" DataField="Version" SortExpression="Version" />
                            <Rock:BoolField HeaderText="Is Approved" DataField="IsApproved" SortExpression="IsApproved" />
                            <Rock:DateTimeField HeaderText="Time Approved" DataField="ApprovedDateTime" SortExpression="ApprovedDateTime" />
                            <Rock:DateTimeField HeaderText="Start Time" DataField="StartDateTime" SortExpression="StartDateTime" />
                            <Rock:DateTimeField HeaderText="Expire Time" DataField="ExpireDateTime" SortExpression="ExpireDateTime" />
                            <%--<Rock:EditField IconCssClass="fa fa-pencil" ToolTip="Edit in HTML Editor" OnClick="rgHtmlFragments_EditHTML" />--%>
                            <Rock:EditField IconCssClass="fa fa-code" ToolTip="Edit in Code Editor"  OnClick="rgHtmlFragments_EditCode" />
                        </Columns>
                    </Rock:Grid>

                </asp:Panel>
                <asp:Panel ID="pnlEdit" runat="server" Visible="false">

                    <div class="row">
                        <div class="col-md-1">
                            <Rock:RockTextBox ID="rtbBlockId" runat="server" Label="BlockId"></Rock:RockTextBox>
                        </div>
                        <div class="col-md-4">
                            <Rock:RockTextBox ID="rtbContextName" runat="server" Label="Context Name"></Rock:RockTextBox>
                        </div>
                        <div class="col-md-6">
                            <Rock:RockTextBox ID="rtbContextParameter" runat="server" Label="Context Parameter"></Rock:RockTextBox>
                        </div>
                        <div class="col-md-1">
                            <Rock:RockTextBox ID="rtbVersion" runat="server" Label="Version"></Rock:RockTextBox>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:DateTimePicker ID="dtpStartTime" runat="server" Label="Start Time" />
                        </div>
                        <div class="col-md-4">
                            <Rock:DateTimePicker ID="dtpExpire" runat="server" Label="Expire Time" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="rcbApproved" runat="server" Label="Approved" />
                        </div>
                    </div>

                    <asp:HiddenField ID="hfHTMLContentGUID" runat="server" />

                    <Rock:HtmlEditor ID="htmlEditor" runat="server" ResizeMaxWidth="720" Height="550" />
                    <Rock:CodeEditor ID="ceHtml" runat="server" EditorHeight="500" />

                    <asp:LinkButton ID="lbCancel" runat="server" CssClass="btn btn-default" Text="Cancel" OnClick="lbCancel_Click"></asp:LinkButton>
                    <asp:LinkButton ID="lbSave" runat="server" CssClass="btn btn-primary" Text="Save" OnClick="lbSave_Click"></asp:LinkButton>
                    <asp:LinkButton ID="lbSaveAs" runat="server" CssClass="btn btn-primary" Text="Save As" OnClick="lbSaveAs_Click"></asp:LinkButton>

                </asp:Panel>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
