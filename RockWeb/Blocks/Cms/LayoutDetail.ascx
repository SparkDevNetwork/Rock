<%@ Control Language="C#" AutoEventWireup="true" CodeFile="LayoutDetail.ascx.cs" Inherits="RockWeb.Blocks.Crm.LayoutDetail" %>

<asp:UpdatePanel ID="upDetail" runat="server">
    <ContentTemplate>

        <asp:HiddenField ID="hfSiteId" runat="server" />
        <asp:HiddenField ID="hfLayoutId" runat="server" />

        <div class="banner">
            <h1><asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
        </div>

        <asp:ValidationSummary ID="ValidationSummary1" runat="server" CssClass="alert alert-danger" ValidationGroup="LayoutDetail" />

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
                <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" ValidationGroup="LayoutDetail" ></asp:LinkButton>
                <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn btn-default" OnClick="btnCancel_Click" CausesValidation="false"></asp:LinkButton>
            </div>

        </div>

    </ContentTemplate>
</asp:UpdatePanel>
