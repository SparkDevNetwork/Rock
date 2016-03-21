<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PromotionList.ascx.cs" Inherits="RockWeb.Plugins.church_ccv.Promotions.PromotionList" %>

<asp:UpdatePanel ID="UpdatePanel1" runat="server">
    <ContentTemplate>
        <div class ="panel panel-block">
            <div class="panel-heading">
                <div class="row col-sm-4">
                    <h4 class="panel-title">Live Promotions</h4>
                    <br />
                    <h5>These are the live promotions for the selected weekend and campus.</h5>
                </div>
            </div>

            <div class="panel-body">
                <div class="row col-sm-4">
                    <Rock:DatePicker ID="dpTargetPromoDate" runat="server" Label="Which Weekend?" />
                    <Rock:RockDropDownList ID="ddlCampus" runat="server" Label="Which Campus?"/>
                    <Rock:RockDropDownList ID="ddlPromoType" runat="server" Label="What type of Promotions? (Leave blank to see them all)" />
                    <Rock:BootstrapButton ID="btnApply" runat="server" Text="Apply" OnClick="ApplyDates" CssClass="btn btn-primary" />
                </div>
            </div>
        </div>
        <Rock:GridFilter ID="rPromotionsFilter" runat="server" >
            <Rock:RockTextBox ID="tbTitle" runat="server" Label="Title" />
        </Rock:GridFilter>
        <Rock:Grid ID="gPromotionOccurrencesGrid" runat="server" OnRowSelected="PromotionOccurrencesGrid_RowSelected">
            <Columns>
                <Rock:RockBoundField DataField="Title" HeaderText="Title" SortExpression="Title" />
                <Rock:RockBoundField DataField="EventDate" HeaderText="Event Date" SortExpression="EventDate" />
                <Rock:RockBoundField DataField="PromoDate" HeaderText="Promotion Date(s)" SortExpression="PromoDate" />
                <Rock:RockBoundField DataField="Campus" HeaderText="Promoting On Campus(s)" SortExpression="Campus" />
                <Rock:RockBoundField DataField="Priority" HeaderText="Priority" SortExpression="Priority" />
                <Rock:RockBoundField DataField="PromoType" HeaderText="Promotion Type" SortExpression="PromoType" HtmlEncode="false" />
                <Rock:DeleteField HeaderText="Remove" OnClick="PromotionOccurrencesGrid_Remove" />
            </Columns>
        </Rock:Grid>
    </ContentTemplate>
</asp:UpdatePanel>
