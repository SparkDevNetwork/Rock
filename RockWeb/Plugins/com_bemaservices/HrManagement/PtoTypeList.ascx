<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PtoTypeList.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.HrManagement.PtoTypeList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-o"></i>PTO Types</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlList" runat="server" Visible="false">

                    <asp:Panel ID="pnlPtoTypes" runat="server">
                        <Rock:ModalAlert ID="mdGridWarningPtoTypes" runat="server" />

                        <div class="grid grid-panel">
                            <Rock:Grid ID="gPtoTypes" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gPtoTypes_Edit" AllowSorting="False" TooltipField="Id">
                                <Columns>
                                    <Rock:RockBoundField DataField="Name" HeaderText="Name" />
                                    <Rock:RockBoundField DataField="Description" HeaderText="Description" />
                                    <Rock:RockBoundField DataField="IsNegativeTimeBalanceAllowed" HeaderText="Negative Balance Allowed?" />
                                    <Rock:RockBoundField DataField="Color" HeaderText="Color" />
                                    <Rock:RockBoundField DataField="WorkflowType.Name" HeaderText="Workflow Type" />
                                    <Rock:RockBoundField DataField="IsActive" HeaderText="Is Active" />
                                </Columns>
                            </Rock:Grid>
                        </div>

                    </asp:Panel>

                </asp:Panel>

            </div>

            <Rock:ModalDialog ID="modalPtoType" runat="server" Title="PTO Type" ValidationGroup="PtoType">
                <Content>
                    <asp:HiddenField ID="hfPtoTypeId" runat="server" />
                    <asp:ValidationSummary ID="valSummary" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="PtoType" />
                    <fieldset>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="rtbName" runat="server" Label="Name" />
                            </div>
                            <div class="col-md-3">
                                <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="IsActive" />
                            </div>
                            <div class="col-md-3">
                                <Rock:RockCheckBox ID="rcbIsNegativeTimeBalanceAllowed" runat="server" Label="Negative Time Balance Allowed?" Visible="false" />
                            </div>
                        </div>
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockTextBox ID="rtbDescription" runat="server" Label="Description" />
                            </div>
                            <div class="col-md-3">
                                <Rock:ColorPicker ID="cpColor" runat="server" Label="Color" />
                            </div>
                            <div class="col-md-3">
                                <Rock:WorkflowTypePicker ID="wtpWorkflowType" runat="server" Label="Workflow Type" Visible="false" />
                            </div>
                        </div>
                    </fieldset>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
