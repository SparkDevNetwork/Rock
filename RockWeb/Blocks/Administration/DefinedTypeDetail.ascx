<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DefinedTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Administration.DefinedTypeDetail" %>

<asp:UpdatePanel ID="upSettings" runat="server">
    <ContentTemplate>
        <asp:HiddenField ID="hfIdType" runat="server" />

        

        <asp:Panel ID="pnlTypeDetails" runat="server" Visible="false">

            <div id="pnlEditDetails" runat="server" class="well">

                <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-error" />

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbTypeName" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Name" />
                            <Rock:DataTextBox ID="tbTypeDescription" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                            <Rock:DataTextBox ID="tbTypeCategory" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="Category" />
                            <Rock:FieldTypeList ID="ddlTypeFieldType" runat="server" SourceTypeName="Rock.Model.DefinedType, Rock" PropertyName="FieldType" />
                        </div>

                        <div class="span6">
                            <div class="control-group">
                                <h5>Defined Type Attributes
                                </h5>
                                <Rock:Grid ID="gDefinedTypeAttributes" runat="server" AllowPaging="false" DisplayType="Light">
                                    <Columns>
                                        <asp:BoundField DataField="Name" />
                                        <Rock:EditField OnClick="gDefinedTypeAttributes_Edit" />
                                        <Rock:DeleteField OnClick="gDefinedTypeAttributes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSaveType" runat="server" Text="Save" CssClass="btn primary" OnClick="btnSaveType_Click" />
                    <asp:LinkButton ID="btnCancelType" runat="server" Text="Cancel" CssClass="btn secondary" CausesValidation="false" OnClick="btnCancelType_Click" />
                </div>
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <div class="well">
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="attributes">
                        <asp:PlaceHolder ID="phDefinedTypeAttributesReadOnly" runat="server" EnableViewState="false"></asp:PlaceHolder>
                    </div>
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" CausesValidation="false" OnClick="btnEdit_Click" />
                    </div>
                </div>
            </fieldset>

            <asp:Panel ID="pnlValues" runat="server">
                <h4>Values</h4>

                <Rock:Grid ID="rGridValue" runat="server" OnRowSelected="rGridValue_Edit" AllowSorting="true">
                    <Columns>
                        <asp:BoundField DataField="Name" HeaderText="Name" SortExpression="Name" />
                        <asp:BoundField DataField="Description" HeaderText="Description" SortExpression="Description" />
                        <Rock:DeleteField OnClick="rGridValue_Delete" />
                    </Columns>
                </Rock:Grid>

            </asp:Panel>

        </asp:Panel>

        <asp:Panel ID="pnlDefinedTypeAttributes" runat="server" Visible="false">
            <RockWeb:RockAttributeEditor ID="edtDefinedTypeAttributes" runat="server" OnSaveClick="btnSaveDefinedTypeAttribute_Click" OnCancelClick="btnCancelDefinedTypeAttribute_Click" />
        </asp:Panel>

        <Rock:ModalDialog ID="modalValues" runat="server" Title="Type Values">
            <Content>
                <asp:HiddenField ID="hfIdValue" runat="server" />
                <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-error block-message error" />

                <fieldset>
                    <Rock:DataTextBox ID="tbValueName" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Name" />
                    <Rock:DataTextBox ID="tbValueDescription" runat="server" SourceTypeName="Rock.Model.DefinedValue, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                    <div class="control-group">
                        <label class="control-label">
                            <asp:Literal ID="attrCaption" runat="server"></asp:Literal></label>
                        <div class="controls">
                            <asp:PlaceHolder ID="phAttrControl" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                </fieldset>
            </Content>
        </Rock:ModalDialog>


        <Rock:NotificationBox ID="nbMessage" runat="server" Title="Error" NotificationBoxType="Error" Visible="false" />

    </ContentTemplate>
</asp:UpdatePanel>
