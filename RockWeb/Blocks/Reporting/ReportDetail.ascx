<%@ Control Language="C#" AutoEventWireup="true" CodeFile="ReportDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.ReportDetail" %>
<asp:UpdatePanel ID="upReport" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfReportId" runat="server" />

            <asp:ValidationSummary ID="vsDetails" runat="server" CssClass="alert alert-error" />

            <div id="pnlEditDetails" runat="server" class="well">

                <fieldset>
                    <h1 class="banner">
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </h1>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:CategoryPicker ID="cpCategory" runat="server" Required="true" EntityTypeName="Rock.Model.Report" LabelText="Category" />
                            <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="EntityTypeId" DataTextField="FriendlyName" LabelText="Applies To" DataValueField="Id" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                            <Rock:DataDropDownList ID="ddlDataView" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Name" LabelText="Data View" />
                        </div>
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Name" CssClass="" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.Report, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>
                </fieldset>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>

            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <h1 class="banner">
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </h1>

                <div class="row-fluid">
                    <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                </div>
                <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                </div>
                <div class="actions">
                    <asp:LinkButton ID="btnEdit"   runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                    <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-mini" OnClick="btnDelete_Click" />
                    <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-mini pull-right" />
                </div>

                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" />

            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
