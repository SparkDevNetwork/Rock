<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedValueList.ascx.cs" Inherits="RockWeb.Blocks.Core.DefinedValueList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfDefinedTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-o"></i> Values</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlList" runat="server" Visible="false">

                        <asp:Panel ID="pnlValues" runat="server">
                            <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />
                        
                            <div class="grid grid-panel">
                                <Rock:Grid ID="gDefinedValues" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gDefinedValues_Edit" AllowSorting="False" TooltipField="Id">
                                    <Columns>
                                        <Rock:ReorderField/>
                                        <Rock:RockBoundField DataField="Value" HeaderText="Value"/>
                                        <Rock:RockBoundField DataField="Description" HeaderText="Description"/>
                                    </Columns>
                                </Rock:Grid>
                            </div>

                        </asp:Panel>

                </asp:Panel>

            </div>

            

            <Rock:ModalDialog ID="modalValue" runat="server" Title="Defined Value" ValidationGroup="Value" >
                <Content>

                <asp:HiddenField ID="hfDefinedValueId" runat="server" />
                <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="Value" />
                <legend>
                    <asp:Literal ID="lActionTitleDefinedValue" runat="server" />
                </legend>
                <fieldset>                
                    <div class="row-fluid">
                        <div class="span12">
                            <Rock:DataTextBox ID="tbValueName" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Value" ValidationGroup="Value" Label="Value"/>
                            <Rock:DataTextBox ID="tbValueDescription" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" ValidationGroup="Value" ValidateRequestMode="Disabled"/>
                        </div>
                    </div>
                    <div class="attributes">
                        <asp:PlaceHolder ID="phDefinedValueAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                </fieldset>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
        
    </ContentTemplate>
</asp:UpdatePanel>
