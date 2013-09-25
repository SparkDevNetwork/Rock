<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedValueList.ascx.cs" Inherits="RockWeb.Blocks.Administration.DefinedValueList" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfDefinedTypeId" runat="server" />

        <asp:Panel ID="pnlList" runat="server" Visible="false">

            <div class="row-fluid">
                <h4>Values for Defined Type</h4>
                <asp:Panel ID="pnlValues" runat="server">
                    <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />
                    <Rock:Grid ID="gDefinedValues" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gDefinedValues_Edit" AllowSorting="False">
                        <Columns>
                            <asp:BoundField DataField="Name" HeaderText="Name"/>
                            <asp:BoundField DataField="Description" HeaderText="Description"/>
                            <Rock:ReorderField/>
                            <Rock:DeleteField OnClick="gDefinedValues_Delete" />
                        </Columns>
                    </Rock:Grid>

                </asp:Panel>
            </div>

        </asp:Panel>

        <Rock:ModalDialog ID="modalValue" runat="server">
            <Content>

            <asp:HiddenField ID="hfDefinedValueId" runat="server" />
            <asp:ValidationSummary ID="valSummaryValue" runat="server" CssClass="alert alert-error" />
            <legend>
                <asp:Literal ID="lActionTitleDefinedValue" runat="server" />
            </legend>
            <fieldset>                
                <div class="row-fluid">
                    <div class="span12">
                        <Rock:DataTextBox ID="tbValueName" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Name" />
                        <Rock:DataTextBox ID="tbValueDescription" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    </div>
                </div>
                <div class="attributes">
                    <asp:PlaceHolder ID="phDefinedValueAttributes" runat="server" EnableViewState="false"></asp:PlaceHolder>
                </div>
            </fieldset>

            </Content>
        </Rock:ModalDialog>
        
    </ContentTemplate>
</asp:UpdatePanel>
