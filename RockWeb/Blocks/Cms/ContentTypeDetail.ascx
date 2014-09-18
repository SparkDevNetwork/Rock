<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ContentTypeDetail.ascx.cs" Inherits="RockWeb.Blocks.Cms.ContentTypeDetail" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >
            <asp:HiddenField ID="hfContentTypeId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-lightbulb-o"></i>
                    <asp:Literal ID="lTitle" runat="server" />
                </h1>
            </div>

            <div class="panel-body">

                <Rock:NotificationBox ID="nbWarningMessage" runat="server" NotificationBoxType="Warning" />

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.ContentType, Rock" PropertyName="Name" />
                            <Rock:RockDropDownList ID="ddlDateRangeType" runat="server" Label="Date Range Type" />
                        </div>
                        <div class="col-md-6">
                            <div class="grid">
                                <Rock:Grid ID="gChannelAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Channel Attribute" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <asp:BoundField DataField="Name" HeaderText="Channel Attributes" />
                                        <Rock:EditField OnClick="gChannelAttributes_Edit" />
                                        <Rock:DeleteField OnClick="gChannelAttributes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                            <div class="grid">
                                <Rock:Grid ID="gItemAttributes" runat="server" AllowPaging="false" DisplayType="Light" RowItemText="Item Attribute" ShowConfirmDeleteDialog="false">
                                    <Columns>
                                        <Rock:ReorderField />
                                        <asp:BoundField DataField="Name" HeaderText="Item Attributes" />
                                        <Rock:EditField OnClick="gItemAttributes_Edit" />
                                        <Rock:DeleteField OnClick="gItemAttributes_Delete" />
                                    </Columns>
                                </Rock:Grid>
                            </div>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                        <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                    </div>

                </div>

                <fieldset id="fieldsetViewSummary" runat="server">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    <div class="row">
                        <div class="col-sm-6">
                            <asp:Literal ID="lDetails" runat="server" />
                        </div>
                        <div class="col-sm-6">
                            <asp:Literal ID="lAttributeDetails" runat="server" />
                        </div>
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="lbEdit" runat="server" Text="Edit" CssClass="btn btn-primary" CausesValidation="false" OnClick="lbEdit_Click" />
                    </div>
                </fieldset>


            </div>

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgChannelAttributes" runat="server" Title="Content Channel Attributes" OnSaveClick="dlgChannelAttributes_SaveClick"  OnCancelScript="clearActiveDialog();" ValidationGroup="ChannelAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtChannelAttributes" runat="server" ShowActions="false" ValidationGroup="ChannelAttributes" />
            </Content>
        </Rock:ModalDialog>

        <Rock:ModalDialog ID="dlgItemAttributes" runat="server" Title="Content Item Attributes" OnSaveClick="dlgItemAttributes_SaveClick"  OnCancelScript="clearActiveDialog();" ValidationGroup="ItemAttributes">
            <Content>
                <Rock:AttributeEditor ID="edtItemAttributes" runat="server" ShowActions="false" ValidationGroup="ItemAttributes" />
            </Content>
        </Rock:ModalDialog>


    </ContentTemplate>
</asp:UpdatePanel>
