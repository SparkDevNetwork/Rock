<%@ Control Language="C#" AutoEventWireup="true" CodeFile="BookViewer.ascx.cs" Inherits="RockWeb.Plugins.com_centralaz.Prayerbook.BookViewer" %>
<asp:UpdatePanel ID="upBookViewer" runat="server">
    <ContentTemplate>
        <Rock:RockDropDownList ID="ddlBooks" runat="server" Label="View entries for book: "
            DataTextField="Name" DataValueField="Id"
            OnSelectedIndexChanged="ddlBooks_SelectedIndexChanged" AutoPostBack="true" />
        <asp:HiddenField ID="hfEntryIndex" runat="server" />
        <div class="row">
            <div class="col-md-3" id="entriesList">
                <asp:GridView runat="server" ID="gBookEntries" EnableModelValidation="True" AutoGenerateColumns="False"
                    GridLines="none" BorderColor="LightGray" CssClass="list" RowStyle-CssClass="listItem"
                    AlternatingRowStyle-CssClass="listAltItem" OnRowDataBound="gBookEntries_OnRowDataBound">
                    <Columns>
                        <asp:TemplateField HeaderText="" ShowHeader="false" HeaderStyle-HorizontalAlign="Left"
                            ItemStyle-Width="35%">
                            <ItemTemplate>
                                <asp:LinkButton ID="LinkButton1" runat="server" Text='<%#Eval("Person.FullName") %>'
                                    OnCommand="gBookEntries_Command" CommandArgument='<%#Eval("Id")%>' />
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
            <div class="col-md-9" id="divViewEntry" runat="server" visible="false">
                <div id="viewEntry">
                    <h2>
                        <asp:Literal runat="server" ID="lContributor" />
                        <small runat="server" id="dtSpouse">spouse:
                            <asp:Literal runat="server" ID="lSpouseName" /></small></h2>
                    <div class="row">
                        <div class="col-md-6">
                            <dl>
                                <dt>Ministry</dt>
                                <dd>
                                    <asp:Literal runat="server" ID="lMinistry" /></dd>
                            </dl>
                        </div>
                        <div class="col-md-6">
                            <dl>
                                <dt>Sub-Ministry</dt>
                                <dd>
                                    <asp:Literal runat="server" ID="lSubministry" /></dd>
                            </dl>
                        </div>
                    </div>
                    <dl>
                        <asp:Literal runat="server" ID="lPraise1" />
                        <asp:Literal runat="server" ID="lMinistryNeed1" />
                        <asp:Literal runat="server" ID="lMinistryNeed2" />
                        <asp:Literal runat="server" ID="lMinistryNeed3" />
                        <asp:Literal runat="server" ID="lPersonalRequest1" />
                        <asp:Literal runat="server" ID="lPersonalRequest2" />
                    </dl>
                    <a href="#" class="btn btn-sm btn-default" onclick="scroller('#entriesList'); return false;"><i class="fa fa-arrow-up"></i>top </a>
                    <asp:LinkButton ID="lbNext" TabIndex="1" runat="server" CssClass="btn btn-primary pull-right" OnClick="lbNext_Click">Next <i class="fa fa-chevron-right"></i></asp:LinkButton>
                </div>
            </div>
        </div>
    </ContentTemplate>
</asp:UpdatePanel>
<script type="text/javascript">
    $(function () {
        scroller("#viewEntry");
    });

    function scroller(objID) {
        var $item = $(objID);
        if ($item.length > 0) {
            $('html, body').animate({
                scrollTop: $item.offset().top
            }, 700);
        }
    }

    Sys.WebForms.PageRequestManager.getInstance().add_endRequest(function () {
        scroller("#viewEntry");
    });
</script>
