<%@ Control Language="C#" AutoEventWireup="true" CodeFile="AccountPersonList.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.PCOSync.AccountPersonList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfAccountId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-users"></i> People</h1>
            </div>
            <div class="panel-body">
                <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />
                <div class="grid grid-panel">
                    <Rock:Grid ID="gAccountPersons" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gAccountPersons_RowSelected" AllowSorting="False" TooltipField="Id">
                        <Columns>
                            <Rock:PersonField DataField="PersonAlias.Person" HeaderText="Person" />
                            <Rock:RockBoundField DataField="PCOId" HeaderText="PCO Id" />
                            <Rock:RockTemplateField HeaderText="Rock Security">
                                <ItemTemplate><asp:Literal ID="lRockSecurity" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="PCO Security">
                                <ItemTemplate><asp:Literal ID="lPCOSecurity" runat="server" /></ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>
                </div>
            </div>

            <Rock:ModalDialog ID="modalValue" runat="server" Title="Defined Value" ValidationGroup="Value" >
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
