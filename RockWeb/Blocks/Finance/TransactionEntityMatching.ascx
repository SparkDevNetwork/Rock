<%@ Control Language="C#" AutoEventWireup="true" CodeFile="TransactionEntityMatching.ascx.cs" Inherits="RockWeb.Blocks.Finance.TransactionEntityMatching" %>

<asp:UpdatePanel ID="upnlContent" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-star"></i>Blank List Block</h1>
            </div>
            <div class="panel-body">
                <div class="row">
                        <div class="col-md-4">
                            <Rock:RockDropDownList ID="ddlBatch" runat="server" Label="Open Batches" AutoPostBack="true" OnSelectedIndexChanged="ddlBatch_SelectedIndexChanged" />
                        </div>
                    </div>
                <div class="grid grid-panel">
                    
                    <Rock:Grid ID="gTransactionDetails" runat="server" OnRowDataBound="gTransactionDetails_RowDataBound" OnRowCommand="gTransactionDetails_RowCommand" >
                        <Columns>
                            <Rock:RockLiteralField ID="lPersonName" HeaderText="Person" />
                            <Rock:RockLiteralField ID="lAmount" HeaderText="Amount" />
                            <Rock:RockLiteralField ID="lAccount" HeaderText="Account" />
                            <Rock:RockTemplateField HeaderText="#TODO#" >
                                <ItemTemplate>
                                    <asp:UpdatePanel ID="upEntity" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false" >
                                        <ContentTemplate>
                                                                    
                                        </ContentTemplate>
                                    </asp:UpdatePanel>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

        </asp:Panel>

        <asp:Panel ID="pnlSettings" runat="server">
            <Rock:ModalDialog ID="mdSettings" runat="server" OnSaveClick="mdSettings_SaveClick">
                <Content>
                    <asp:UpdatePanel ID="upnlSettings" runat="server">
                        <ContentTemplate>
                            <div class="row">
                                <div class="col-md-6">
                                    <Rock:DefinedValuePicker ID="ddlTransactionType" runat="server" Label="Transaction Type" Help="The Transaction Type that the transaction should be set to. Leave blank to get the original Transaction Type." />
                                    <Rock:EntityTypePicker ID="etpEntityType" runat="server" AutoPostBack="true" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierColumn" runat="server" Label="Entity Type Qualifier Column" />
                                    <Rock:RockTextBox ID="tbEntityTypeQualifierValue" runat="server" Label="Entity Type Qualifier Value" />
                                    <Rock:RockDropDownList ID="ddlDefinedTypePicker" runat="server" Visible="false" Label="Defined Type" />
                                    <Rock:GroupTypePicker ID="gtpGroupType" runat="server" Visible="false" Label="Group Type" />
                                </div>
                                <div class="col-md-6">
                                </div>
                            </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </Content>

            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
