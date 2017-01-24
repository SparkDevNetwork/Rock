<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountPersonList.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.PCOSync.AccountPersonList" %>


<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfAccountId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> People</h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel">

                    <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />

                    <Rock:GridFilter ID="rFilter" runat="server">
                        <Rock:RockTextBox ID="tbFirstName" runat="server" Label="First Name" />
                        <Rock:RockTextBox ID="tbLastName" runat="server" Label="Last Name" />
                        <Rock:RockCheckBox ID="cbCurrentOnly" runat="server" Label="Current People Only" Text="Yes" />
                        <Rock:RockCheckBox ID="cbBlankPCOId" runat="server" Label="Blank PCO Ids" Text="Yes" />
                        <Rock:NumberBox ID="nbPCOId" runat="server" Label="Specific PCO Id" />
                        <Rock:RockCheckBoxList ID="cblRockPermission" runat="server" Label="Rock Permissions" RepeatDirection="Horizontal" />
                        <Rock:RockCheckBoxList ID="cblPCOPermission" runat="server" Label="PCO Permissions" RepeatDirection="Horizontal" />
                    </Rock:GridFilter>

                    <Rock:Grid ID="gAccountPersons" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gAccountPersons_RowSelected" CssClass="js-account-people" ShowConfirmDeleteDialog="false"
                        RowItemText="Person" AllowSorting="True" TooltipField="Id">
                        <Columns>
                            <Rock:SelectField Visible="false" />
                            <Rock:RockBoundField DataField="Person" HeaderText="Person" SortExpression="LastName,NickName" />
                            <Rock:BoolField DataField="Current" HeaderText="Current" SortExpression="Current" />
                            <Rock:RockBoundField DataField="PCOId" HeaderText="PCO Id" SortExpression="PCOId" />
                            <Rock:RockBoundField DataField="RockPermissionLabel" HeaderText="Rock Permissions" SortExpression="RockPermission" HtmlEncode="false" />
                            <Rock:RockBoundField DataField="PCOPermissionLabel" HeaderText="PCO Permissions" SortExpression="PCOPermission" HtmlEncode="false" />
                            <asp:HyperLinkField ItemStyle-HorizontalAlign="Center" HeaderStyle-CssClass="grid-columncommand" ItemStyle-CssClass="grid-columncommand"
                                DataNavigateUrlFields="PersonId" DataTextFormatString="<div class='btn btn-default btn-sm'><i class='fa fa-user'></i></div>" DataTextField="PersonId" />
                            <Rock:DeleteField OnClick="gAccountPerson_Delete" />
                        </Columns>
                    </Rock:Grid>

                </div>
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
                            <Rock:RockLiteral ID="lRockValues" runat="server" Label="Rock State" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockLiteral ID="lPCOValues" runat="server" Label="PCO State" />
                        </div>
                    </div>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
