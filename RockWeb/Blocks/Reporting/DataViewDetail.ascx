<%@ Control Language="C#" AutoEventWireup="true" CodeFile="DataViewDetail.ascx.cs" Inherits="RockWeb.Blocks.Reporting.DataViewDetail" %>
<asp:UpdatePanel ID="upDataView" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" runat="server" Visible="false">
            <asp:HiddenField ID="hfDataViewId" runat="server" />

            <div id="pnlEditDetails" class="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-filter"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

                    <asp:ValidationSummary ID="vsDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <fieldset>

                        <div class="row">
                            <div class="col-md-6">
                                <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Name" CssClass="" />
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="Rock.Model.DataView, Rock" PropertyName="Description" TextMode="MultiLine" Rows="4" />
                                <Rock:EntityTypePicker ID="etpEntityType" runat="server" Label="Applies To" OnSelectedIndexChanged="etpEntityType_SelectedIndexChanged" AutoPostBack="true" />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlTransform" runat="server" Label="Post-filter Transformation" />
                                <Rock:CategoryPicker ID="cpCategory" runat="server" EntityTypeName="Rock.Model.DataView" Label="Category" Required="true" />
                            </div>
                        </div>
                    </fieldset>

                    <asp:PlaceHolder ID="phFilters" runat="server"></asp:PlaceHolder>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                        <asp:LinkButton ID="btnPreview" runat="server" Text="Preview" CssClass="btn btn-default pull-right" CausesValidation="false" OnClick="btnPreview_Click" />
                    </div>

                </div>

            </div>

            <div id="pnlViewDetails" class="panel panel-block" runat="server">

                <div class="panel-heading">
                    <h1 class="panel-title"><i class="fa fa-filter"></i> <asp:Literal ID="lReadOnlyTitle" runat="server" /></h1>
                </div>
                <div class="panel-body">

                    <fieldset>

                        <p class="description">
                            <asp:Literal ID="lDescription" runat="server"></asp:Literal>
                        </p>

                        <div class="row">
                            <div class="col-md-6">
                                <asp:Literal ID="lblMainDetails" runat="server" />
                            </div>
                            <div class="col-md-6">
                                <asp:Literal ID="lFilters" runat="server" />
                            </div>
                        </div>

                        <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                        <div class="actions">
                            <asp:LinkButton ID="btnEdit" runat="server" AccessKey="e" Text="Edit" CssClass="btn btn-primary" OnClick="btnEdit_Click" />
                            <asp:LinkButton ID="btnDelete" runat="server" Text="Delete" CssClass="btn btn-link" OnClick="btnDelete_Click" />
                            <Rock:SecurityButton ID="btnSecurity" runat="server" class="btn btn-sm btn-security pull-right" />
                            <Rock:ModalAlert ID="mdDeleteWarning" runat="server" />
                        </div>

                    </fieldset>

                    <h4>Results</h4>

                    <div class="grid">
                        <Rock:Grid ID="gReport" runat="server" AllowSorting="true" EmptyDataText="No Results" />
                    </div>

                </div>

            </div>

            <Rock:ModalDialog ID="modalPreview" runat="server" Title="Preview (top 15 rows )" ValidationGroup="Preview">
                <Content>
                    <div class="grid"><Rock:Grid ID="gPreview" runat="server" AllowSorting="true" EmptyDataText="No Results" ShowActionRow="false" DisplayType="Light" /></div>
                </Content>
            </Rock:ModalDialog>

        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
