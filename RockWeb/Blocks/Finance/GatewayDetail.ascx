<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GatewayDetail.ascx.cs" Inherits="RockWeb.Blocks.Finance.GatewayDetail" %>

<asp:UpdatePanel ID="pnlGatewayListUpdatePanel" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlDetails" CssClass="panel panel-block" runat="server">
           
            <asp:HiddenField ID="hfGatewayId" runat="server" />

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-credit-card"></i> <asp:Literal ID="lActionTitle" runat="server" /></h1>
                
                <div class="panel-labels">
                    <Rock:HighlightLabel ID="hlInactive" runat="server" LabelType="Danger" Text="Inactive" />
                </div>
            </div>
            <div class="panel-body">

                <Rock:NotificationBox ID="nbEditModeMessage" runat="server" NotificationBoxType="Info" />

                <div id="pnlViewDetails" runat="server">
                    <p class="description">
                        <asp:Literal ID="lGatewayDescription" runat="server"></asp:Literal>
                    </p>

                    <div class="row">
                        <div class="col-md-12">
                            <asp:Literal ID="lblMainDetails" runat="server" />
                        </div>
                    </div>

                </div>

                <div id="pnlEditDetails" runat="server">

                    <asp:ValidationSummary ID="valGatewayDetail" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />

                    <div class="row">
                        <div class="col-md-6">
                            <Rock:DataTextBox ID="tbName" runat="server"
                                SourceTypeName="Rock.Model.FinancialGateway, Rock" PropertyName="Name" />
                        </div>
                        <div class="col-md-6">
                            <Rock:RockCheckBox ID="cbIsActive" runat="server" Label="Active" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-12">
                            <Rock:DataTextBox ID="tbDescription" runat="server"
                                SourceTypeName="Rock.Model.FinancialGateway, Rock" PropertyName="Description" TextMode="MultiLine" Rows="3" />
                        </div>
                    </div>

                    <div class="row">
                        <div class="col-md-6"> 
                            <Rock:ComponentPicker ID="cpGatewayType" runat="server" Label="Gateway Type" Required="true" ContainerType="Rock.Financial.GatewayContainer" AutoPostBack="true" OnSelectedIndexChanged="cpGatewayType_SelectedIndexChanged" />
                            <Rock:TimePicker ID="tpBatchTimeOffset" runat="server" Label="Batch Time Offset" 
                                Help="By default online payments will be grouped into batches with a start time 12:00:00 AM. However if the payment gateway groups transactions into batches based on a different time, this offset can specified so that Rock will use the same time when creating batches for online transactions" />
                        </div>
                        <div class="col-md-6">                
                            <asp:PlaceHolder ID="phAttributes" runat="server" EnableViewState="false" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="btnSave" runat="server" AccessKey="s" Text="Save" CssClass="btn btn-primary" OnClick="btnSave_Click" />
                        <asp:LinkButton ID="btnCancel" runat="server" AccessKey="c" Text="Cancel" CssClass="btn btn-link" CausesValidation="false" OnClick="btnCancel_Click" />
                    </div>

            </div>

            </div>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
