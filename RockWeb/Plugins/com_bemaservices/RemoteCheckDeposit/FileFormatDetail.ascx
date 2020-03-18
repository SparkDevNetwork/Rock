<%@ Control Language="C#" AutoEventWireup="true" CodeFile="FileFormatDetail.ascx.cs" Inherits="RockWeb.Plugins.com_bemaservices.RemoteCheckDeposit.FileFormatDetail" %>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlEdit" CssClass="panel panel-block" runat="server">
            <asp:HiddenField ID="hfId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title">
                    <i class="fa fa-file-o"></i>
                    <asp:Literal ID="lActionTitle" runat="server" />
                </h1>

                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>

            <Rock:PanelDrawer ID="pdAuditDetails" runat="server"></Rock:PanelDrawer>

            <div class="panel-body">
                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <asp:Panel ID="pnlViewDetails" runat="server">
                     <p class="description">
                        <asp:Literal ID="lFileFormatDescription" runat="server"></asp:Literal>
                     </p>

                     <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:PlaceHolder ID="phAttributes" runat="server" />
                        </div>
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEditDetails" runat="server">
                    <asp:ValidationSummary ID="vsEditDetails" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server" SourceTypeName="com.bemaservices.RemoteCheckDeposit.Model.ImageCashLetterFileFormat, com.bemaservices.RemoteCheckDeposit" PropertyName="Name" />
                        </div>

                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <div class="form-group">
                                <Rock:DataTextBox ID="tbDescription" runat="server" SourceTypeName="com.bemaservices.RemoteCheckDeposit.Model.ImageCashLetterFileFormat, com.bemaservices.RemoteCheckDeposit" PropertyName="Description" TextMode="MultiLine" Rows="3" ValidateRequestMode="Disabled" />
                            </div>
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbFileNameTemplate" runat="server" SourceTypeName="com.bemaservices.RemoteCheckDeposit.Model.ImageCashLetterFileFormat, com.bemaservices.RemoteCheckDeposit" PropertyName="FileNameTemplate" Help="The exported file name will be formatted according to these specifications. <span class='tip tip-lava'></span>" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:ComponentPicker ID="cpFileFormatType" runat="server" Label="File Format Type" Required="true" ContainerType="com.bemaservices.RemoteCheckDeposit.FileFormatTypeContainer, com.bemaservices.RemoteCheckDeposit" AutoPostBack="true" OnSelectedIndexChanged="cpFileFormatType_SelectedIndexChanged" />
                        </div>

                        <div class="col-md-6">
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:PlaceHolder ID="phEditAttributes" runat="server"></asp:PlaceHolder>
                        </div>
                    </div>
                </asp:Panel>

                <div class="actions">
                    <asp:LinkButton ID="lbSave" runat="server" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" />
                    <asp:LinkButton ID="lbCancel" runat="server" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="lbCancel_Click" />
                </div>
            </div>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
