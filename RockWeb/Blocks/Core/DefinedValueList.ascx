<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedValueList.ascx.cs" Inherits="RockWeb.Blocks.Core.DefinedValueList" %>

<asp:UpdatePanel ID="upnlSettings" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" CssClass="panel panel-block" runat="server">

            <asp:HiddenField ID="hfDefinedTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-file-o"></i> <asp:Literal ID="lTitle" runat="server" /></h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlList" runat="server" Visible="false">

                        <asp:Panel ID="pnlValues" runat="server">
                            <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />

                            <div class="grid grid-panel">
                                <Rock:Grid ID="gDefinedValues" runat="server" AllowPaging="true" DisplayType="Full" OnRowSelected="gDefinedValues_Edit" AllowSorting="False">
                                    <Columns>
                                        <Rock:ReorderField/>
                                        <Rock:RockBoundField DataField="Value" HeaderText="Value"/>
                                        <Rock:RockBoundField DataField="Category" HeaderText="Category"/>
                                        <Rock:RockBoundField DataField="Description" HeaderText="Description"/>
                                        <Rock:BoolField DataField="IsActive" HeaderText="Active" />
                                    </Columns>
                                </Rock:Grid>
                            </div>

                        </asp:Panel>

                </asp:Panel>

            </div>



            <Rock:ModalDialog ID="modalValue" runat="server" Title="Defined Value" ValidationGroup="Value" >
                <Content>

                <asp:HiddenField ID="hfDefinedValueId" runat="server" />
                <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Value" />
                <legend>
                    <asp:Literal ID="lActionTitleDefinedValue" runat="server" />
                </legend>
                <fieldset>
                    <Rock:DataTextBox ID="tbValueName" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Value" ValidationGroup="Value" Label="Value"/>
                    <Rock:DataTextBox ID="tbValueDescription" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" ValidationGroup="Value" ValidateRequestMode="Disabled"/>
                    <Rock:RockCheckBox ID="cbValueActive" runat="server" Label="Active" />
                    <Rock:CategoryPicker ID="cpCategory" runat="server" Required="false" Label="Category" EntityTypeName="Rock.Model.DefinedValue"
                        Help="The category for the defined value. This will be used for the selection of the defined value." />
                    <div class="attributes">
                        <Rock:AttributeValuesContainer ID="avcDefinedValueAttributes" runat="server" />
                    </div>
                </fieldset>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
