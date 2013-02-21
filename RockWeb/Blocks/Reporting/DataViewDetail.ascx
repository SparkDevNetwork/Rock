<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DataViewDetail" %>
<asp:UpdatePanel ID="upDataView" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfDataViewId" runat="server" />

            <asp:ValidationSummary ID="vsDetails" runat="server" CssClass="alert alert-error" />

            <div id="pnlEditDetails" runat="server" class="well">

                <fieldset>
                    <legend>
                        <asp:Literal ID="lActionTitle" runat="server" />
                    </legend>

                    <div class="row-fluid">
                        <div class="span6">
                            <Rock:DataDropDownList ID="ddlEntityType" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="EntityTypeId" DataTextField="FriendlyName" LabelText="Applies To" DataValueField="Id" />
                            <Rock:DataDropDownList ID="ddlCategory" runat="server" DataTextField="Name" DataValueField="Id" SourceTypeName="Rock.Model.Category, Rock" PropertyName="Name" LabelText="Category" />
                        </div>
                        <div class="span6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Name" CssClass="" />
                            <Rock:DataTextBox  ID="tbDescription" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                        </div>
                    </div>
                </fieldset>

                <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>

                <div class="actions">
                    <asp:LinkButton ID="btnSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                    <asp:LinkButton ID="btnCancel" runat="server" Text="Cancel" CssClass="btn" CausesValidation="false" OnClick="btnCancel_Click" />
                </div>
    
            </div>

            <fieldset id="fieldsetViewDetails" runat="server">
                <legend>
                    <asp:Literal ID="lReadOnlyTitle" runat="server" />
                </legend>
                <div class="well">
                    <div class="row-fluid">
                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />
                    </div>
                    <div class="row-fluid">
                        <asp:Literal ID="lblMainDetails" runat="server" />
                    </div>
                    <div class="actions">
                        <asp:LinkButton ID="btnEdit" runat="server" Text="Edit" CssClass="btn btn-primary btn-mini" OnClick="btnEdit_Click"/>
                        <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-mini" OnClick="btnDelete_Click" />
                    </div>
                </div>
            </fieldset>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
