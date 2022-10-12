<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LinkListLava.ascx.cs" Inherits="RockWeb.Blocks.Cms.LinkListLava" %>

<script type="text/javascript">
    function clearActiveDialog() {
        $('#<%=hfActiveDialog.ClientID %>').val('');
    }
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server">
            <asp:Literal ID="lContent" runat="server"></asp:Literal>
            <asp:Literal ID="lDebug" runat="server" Visible="false"></asp:Literal>
        </asp:Panel>

        <asp:Panel ID="pnlEdit" runat="server" Visible="false">

            <asp:Literal ID="lEditHeader" runat="server" />

            <Rock:ModalAlert ID="mdGridWarningValues" runat="server" />
                        
            <div class="grid">
                <Rock:Grid ID="gLinks" runat="server" AllowPaging="true" RowItemText="Link" OnRowSelected="gLinks_RowSelected" DisplayType="Light" AllowSorting="False" EnableResponsiveTable="false" ShowHeader="false" >
                    <Columns>
                        <Rock:ReorderField/>
                        <Rock:RockTemplateField>
                            <ItemTemplate>
                                <asp:Literal ID="lValue" runat="server" />
                            </ItemTemplate>
                        </Rock:RockTemplateField>
                        <Rock:DeleteField OnClick="gLinks_Delete" ButtonCssClass="btn btn-danger btn-xs grid-delete-button" />
                        <Rock:SecurityField TitleField="Value" ButtonCssClass="btn btn-security btn-xs"  />
                    </Columns>
                </Rock:Grid>
            </div>

            <div class="actions">
                <asp:LinkButton ID="btnDone" runat="server" Text="Done" CssClass="btn btn-link btn-xs" OnClick="btnDone_Click" />
            </div>

            <asp:Literal ID="lEditFooter" runat="server" />

        </asp:Panel>

        <asp:HiddenField ID="hfActiveDialog" runat="server" />

        <Rock:ModalDialog ID="dlgLink" runat="server" Title="Link" ValidationGroup="Value" OnSaveClick="dlgLink_SaveClick" OnCancelScript="clearActiveDialog();" >
            <Content>

                <asp:HiddenField ID="hfDefinedValueId" runat="server" />
                <asp:ValidationSummary ID="valSummaryValue" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="Value" />

                <div class="row">
                    <div class="col-md-6">
                        <Rock:RockTextBox ID="tbTitle" runat="server" ValidationGroup="Value" Label="Title" Required="true"/>
                    </div>
                    <div class="col-md-6">
                        <Rock:RockRadioButtonList ID="rblLinkType" runat="server" Label="Type" RepeatDirection="Horizontal" AutoPostBack="true" OnSelectedIndexChanged="rblLinkType_SelectedIndexChanged" >
                            <asp:ListItem Text="Link" Value="Link"></asp:ListItem>
                            <asp:ListItem Text="Heading" Value="Heading"></asp:ListItem>
                        </Rock:RockRadioButtonList>
                    </div>
                </div>

                <Rock:RockTextBox ID="tbLink" runat="server" ValidationGroup="Value" Label="Link" Required="true" />

            </Content>
        </Rock:ModalDialog>

    </ContentTemplate>
</asp:UpdatePanel>
