<%@ Control Language="C#" AutoEventWireup="true" CodeFile="NamelessPersonList.ascx.cs" Inherits="RockWeb.Blocks.Crm.NamelessPersonList" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title ">
                    <i class="fa fa-question-circle"></i>
                    Nameless People
                </h1>
            </div>
            <div class="panel-body">

                <div class="grid grid-panel ">
                    <Rock:Grid ID="gNamelessPersonList" runat="server" RowItemText="Nameless Person" DataKeyNames="Id">
                        <Columns>
                            <Rock:RockLiteralField ID="lUnmatchedPerson" HeaderText="Phone Number" OnDataBound="lUnmatchedPerson_DataBound" />
                            <Rock:LinkButtonField ID="btnLinkToPerson" Text="<i class='fa fa-user'></i>" ToolTip="Link to Person" CssClass="btn btn-default btn-sm" OnClick="btnLinkToPerson_Click" />
                        </Columns>
                    </Rock:Grid>
                </div>

            </div>

            <%-- Link to Person --%>
            <Rock:ModalDialog ID="mdLinkToPerson" runat="server" Title="Link Phone Number to Person" OnSaveClick="mdLinkToPerson_SaveClick" ValidationGroup="vgLinkToPerson" OnCancelScript="clearActiveDialog();">
                <Content>
                    <asp:HiddenField ID="hfNamelessPersonId" runat="server" />

                    <Rock:NotificationBox ID="nbMergeRequestCreated" runat="server" Heading="To prevent data loss and to ensure the highest level of security, a merge request will be created upon pressing Save." NotificationBoxType="Info" Visible="true" />
                    <Rock:NotificationBox ID="nbAddPerson" runat="server" Heading="Please correct the following:" NotificationBoxType="Danger" Visible="false" />
                    <asp:ValidationSummary ID="valSummaryAddPerson" runat="server" HeaderText="Please correct the following:" CssClass="alert alert-validation" ValidationGroup="vgLinkToPerson" />

                    <Rock:Toggle ID="tglLinkPersonMode" runat="server" OnText="Link Existing Person" CssClass="margin-b-md" OffText="Add New Person" ActiveButtonCssClass="btn-primary" OnCheckedChanged="tglLinkPersonMode_CheckedChanged" />

                    <asp:Panel ID="pnlLinkToNewPerson" runat="server">
                        <Rock:PersonBasicEditor ID="newPersonEditor" runat="server" ValidationGroup="vgLinkToPerson" />
                    </asp:Panel>

                    <asp:Panel ID="pnlLinkToExistingPerson" runat="server" Visible="false">
                        <fieldset>
                            <Rock:PersonPicker ID="ppPerson" runat="server" Label="Person" Required="true" ValidationGroup="vgLinkToPerson" />
                        </fieldset>
                    </asp:Panel>

                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
