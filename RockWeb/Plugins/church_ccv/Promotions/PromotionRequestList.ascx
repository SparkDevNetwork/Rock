<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PromotionRequestList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Promotions.PromotionRequestList" %>

<asp:UpdatePanel ID="upMarketingCampaigns" runat="server">
    <ContentTemplate>
        <div class ="panel panel-block">
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title">Build a Weekend Promotion</h4>
                    <br />
                    <h5>Pick a weekend, campus and events to promote.</h5>
                </div>
            </div>

            <div class="panel-body">
                <div class="row col-sm-4">
                    <Rock:DatePicker ID="dpTargetPromoDate" runat="server" Label="Which Weekend?" />
                    <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="On which Campus?"/>
                    <Rock:DateRangePicker ID="drpFutureWeeks" runat="server" Label="How far out to show Events?" />
                    <Rock:RockDropDownList ID="ddlPromoType" runat="server" Label="What type of Promotions? (Leave blank to see them all)" />
                    <Rock:BootstrapButton ID="btnApply" runat="server" Text="Apply" OnClick="ApplyDates" CssClass="btn btn-primary" />
                </div>
            </div>
        </div>

        <div class ="panel panel-block">
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title">Events Requesting Promotions</h4>
                    <br />
                    <h5>These are future events that want to be promoted.</h5>
                </div>
            </div>

            <div class="panel-body">
                <div class="grid grid-panel">

                    
                        <Rock:GridFilter ID="rFilter" runat="server" >
                            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" />
                        </Rock:GridFilter>
                    
                
                    <Rock:Grid ID="gPromotions" runat="server">
                        <Columns>
                            <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                            <Rock:RockBoundField DataField="Campus" HeaderText="Campus" SortExpression="Campus" />
                            <Rock:RockBoundField DataField="EventDate" HeaderText="Event Date" SortExpression="EventDate" />
                            <Rock:RockBoundField DataField="PromoType" HeaderText="Promotion Type" SortExpression="PromoType" HtmlEncode="false" />
                            <Rock:EditField IconCssClass="fa fa-check" HeaderText="Approve" OnClick="gPromotions_Approve" />
                        </Columns>
                    </Rock:Grid>
                 </div>
             </div>
        </div>

        <%-- Campus Selection Panel --%>
        <asp:Panel ID="pnlCampusSelect" runat="server" Visible="true">
            <Rock:ModalDialog ID="mdCampusSelect" runat="server" ValidationGroup="vgConfigure">
                <Content>
                    <h5>This event is multi-campus, and the promotion type supports multiple campuses.</h5>
                    <h5>Select the campuses you wish to run this promotion on. (Leave blank for all campuses.)</h5>
                    <div class="margin-v-md" />
                    <asp:Label ID="lbCampusSelectEventInfo" runat="server" />
                    <div class="margin-v-md" />
                    <asp:HiddenField ID="hfPromoRequestId" runat="server" />
                    <asp:PlaceHolder ID="phCampuses" runat="server" />
                </Content>
            </Rock:ModalDialog>
        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
