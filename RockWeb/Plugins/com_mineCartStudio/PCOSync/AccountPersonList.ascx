<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountPersonList.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.PCOSync.AccountPersonList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfAccountId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i>People</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">

                    <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />

                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:RockCheckBox ID="cbCurrentOnly" runat="server" Label="Current People Only" />
                        <Rock:RockCheckBox ID="cbBlankPCOId" runat="server" Label="Blank PCO Ids" />
                        <Rock:NumberBox ID="nbPCOId" runat="server" Label="Specific PCO Id" />
                        <Rock:RockCheckBoxList ID="cblRockPermission" runat="server" Label="Rock Permissions" RepeatDirection="Horizontal" />
                        <Rock:RockCheckBoxList ID="cblPCOPermission" runat="server" Label="PCO Permissions" RepeatDirection="Horizontal" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gAccountPersons" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gAccountPersons_RowSelected"
                        RowItemText="Person" AllowSorting="False" TooltipField="Id">
                        <Columns>
                            <Rock:PersonField DataField="Person" HeaderText="Person" SortExpression="LastName,NickName" />
                            <Rock:BoolField DataField="Current" HeaderText="Current" SortExpression="Current" />
                            <Rock:RockBoundField DataField="PCOId" HeaderText="PCO Id" SortExpression="PCOId" />
                            <Rock:RockBoundField DataField="RockPermissionLabel" HeaderText="Rock Permissions" SortExpression="RockPermission" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="PCOPermissionLabel" HeaderText="PCO Permissions" SortExpression="PCOPermission" HtmlEncode="false" />
                        </Columns>
                    </Rock:Grid>

                </div>
            </div>

            <div class="actions">
                <asp:LinkButton ID="btnRefresh" runat="server" Text="Refresh List From Groups" CssClass="btn btn-link" CausesValidation="false" OnClick="btnRefresh_Click" />
            </div>

            <Rock:ModalDialog ID="modalValue" runat="server" Title="Defined Value" ValidationGroup="Value">
                <Content>

                    <asp:HiddenField ID="hfAccountPersonId" runat="server" />
                    <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Value" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:NumberBox ID="tbPcoId" runat="server" NumberType="Integer" ValidationGroup="Value" Label="PCO Id" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lRockValues" runat="server" Label="Current Rock Values" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lPCOValues" runat="server" Label="Current PCO Values" />
                        </div>
                    </div>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
