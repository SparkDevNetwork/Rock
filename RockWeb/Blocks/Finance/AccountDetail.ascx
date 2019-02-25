﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.AccountDetail" %>

<asp:UpdatePanel ID="pnlAccountListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
           
            <asp:HiddenField ID="hfAccountId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-building-o"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lAccountDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                          <div class="col-md-6">
                            <asp:Literal ID="lLeftDetails" runat="server" />
                        </div>
                        <div class="col-md-6">
                            <asp:PlaceHolder ID="phAttributesView" runat="server" />
                        </div>
                    </div>
                       <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" CausesValidation="false" />
                    </div>
                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valAccountDetail" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" />
                    <asp:CustomValidator ID="cvAccount" runat="server" Display="None" />

                    <div class="row">
                        <div class="col-md-4">
                            <Rock:DataTextBox ID="tbName" runat="server"
                                SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                        <div class="col-md-4">
                            <Rock:RockCheckBox ID="cbIsPublic" runat="server" Label="Is Public" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server"
                                SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:CodeEditor ID="cePublicDescription" runat="server" Label="Public description HTML" EditorMode="Lava" EditorTheme="Rock" EditorHeight="250"
                                Help="Additional HTML content to include with the account." />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6"> 
                            <Rock:AccountPicker ID="apParentAccount" runat="server" Label="Parent Account" />
                            <Rock:DefinedValuePicker ID="dvpAccountType" runat="server" Label="Account Type" />
                            <Rock:DataTextBox ID="tbPublicName" runat="server"
                                SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="PublicName" />
                            <Rock:CampusPicker ID="cpCampus" runat="server" Label="Campus" />
                            <Rock:DataTextBox ID="tbUrl" runat="server"
                                SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="Url" />
                        </div>
                        <div class="col-md-6">                
                            <Rock:DataTextBox ID="tbGLCode" runat="server"
                                SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="GlCode" Label="GL Code" />
                            <Rock:DatePicker ID="dtpStartDate" runat="server" SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="StartDate" Label="Start Date" />
                            <Rock:DatePicker ID="dtpEndDate" runat="server" SourceTypeName="Rock.Model.FinancialAccount, Rock" PropertyName="EndDate" Label="End Date" />
                            <Rock:RockCheckBox ID="cbIsTaxDeductible" runat="server" Label="Tax Deductible" />           
                        </div>
                    </div>
                    
                    <div class="row">
                        <div class="col-md-6">
                            <div class="attributes">
                                <Rock:DynamicPlaceHolder ID="phAttributes" runat="server" />
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

            </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
