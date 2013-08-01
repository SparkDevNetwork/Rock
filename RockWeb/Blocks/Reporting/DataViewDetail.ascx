<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DataViewDetail" %>
<asp:UpdatePanel ID="upDataView" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfDataViewId" runat="server" />

            <asp:ValidationSummary ID="vsDetails" runat="server" CssClass="alert alert-error" />

            <div id="pnlEditDetails" runat="server" class="well">

                <fieldset>
                    <h1 class="banner">
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </h1>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Name" CssClass="" />
                            <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                            <Rock:LabeledDropDownList ID="ddlTransform" runat="server" LabelText="Post-filter Transformation" />
                        </div>
                        <div class="span6">
                            <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="EntityTypeId"
                                LabelText="Applies To" DataTextField="FriendlyName" DataValueField="Id" AutoPostBack="true" OnSelectedIndexChanged="ddlEntityType_SelectedIndexChanged" />
                            <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.DataView" LabelText="Category" Required="true" />
                        </div>
                    </div>
                </fieldset>

                <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                    <asp:LinkButton ID="btnPreview" runat="server" Text="Preview" CssClass="btn pull-right" CausesValidation="false" OnClick="btnPreview_Click" />
                </div>

            </div>

            <div id="pnlViewDetails" runat="server">

                <fieldset>
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
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-mini" OnClick="btnDelete_Click" />
                        <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-mini pull-right" />
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                    </div>

                </fieldset>

                <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" />

            </div>

            <Rock:ModalDialog ID="modalPreview" runat="server" Title="Preview">
                <Content>
                    <Rock:Grid ID="gPreview" runat="server" AllowSorting="true" EmptyDataText="No Results" ShowActionRow="false" />
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
