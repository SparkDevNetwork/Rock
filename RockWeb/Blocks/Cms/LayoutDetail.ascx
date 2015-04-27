<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LayoutDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.LayoutDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server" >

        <asp:HiddenField ID="hfSiteId" runat="server" />
        <asp:HiddenField ID="hfLayoutId" runat="server" />

        <div class="panel-heading">
            <h1 class="panel-title"><i class="fa fa-th"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
        </div>
        <div class="panel-body">
            <asp:ValidationSummary ID="ValidationSummary1" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" ValidationGroup="LayoutDetail" />

            <div id="pnlEditDetails" runat="server">

                <fieldset>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbLayoutName" runat="server" SourceTypeName="Rock.Model.Layout, Rock" PropertyName="Name" ValidationGroup="LayoutDetail" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Layout, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" ValidationGroup="LayoutDetail" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:RockDropDownList ID="ddlLayout" runat="server" Label="Layout File" Help="The layout file that this layout should use" ValidationGroup="LayoutDetail" />
                        </div>
                        <div class="col-md-6">
                        </div>
                    </div>

                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="LayoutDetail" ></asp:LinkButton>
                    <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
                </div>

            </div>

                <fieldset id="fieldsetViewDetails" runat="server">

                    <p class="description"><asp:Literal ID="lLayoutDescription" runat="server"></asp:Literal></p>

                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" AccessKey="m" Text="Edit" CssClass="btn btn-primary btn-sm" CausesValidation="false" OnClick="btnEdit_Click" />
    <%--                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link btn-sm" CausesValidation="false" OnClick="btnDelete_Click" />--%>
                    </div>

                </fieldset>
        </div>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
