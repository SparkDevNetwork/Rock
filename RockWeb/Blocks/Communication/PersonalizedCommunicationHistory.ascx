<%@ Control Language="C#" AutoEventWireup="true" CodeFile="PersonalizedCommunicationHistory.ascx.cs" Inherits="RockWeb.Blocks.Communication.PersonalizedCommunicationHistory" %>

<style>
    .avatar-lg.avatar-icon {
        display: inline-flex;
        justify-content: center;
        align-items: center;
        font-size: 22px;
        padding: 0;
        background: #EDEDED;
        color: #7D7D7D;
        border-color: #CDCDCD;
    }

        .avatar-lg.avatar-icon::after {
            display: block;
        }

    .communication-preview {
        box-shadow: 0px 6px 8px rgba(0, 0, 0, 0.05);
        border: 1px solid #DBDBDB;
        font-size: 14px;
        margin: 2px 0 8px;
    }

        .communication-preview + .communication-preview {
            margin-top: 24px;
        }

        .communication-preview .card-heading {
            padding: 8px 12px;
            border-bottom: 1px solid #DBDBDB;
            line-height: 1.4;
            overflow: hidden;
        }

        .communication-preview .card-title {
            font-weight: 500;
        }

        .communication-preview .card-body {
            padding: 16px;
        }

    .sms-bubble {
        background: #0084FF;
        border-radius: 20px;
        color: #fff;
        font-size: 16px;
        text-align: right;
        padding: 12px 24px 16px;
        max-width: 75%;
        margin-left: auto;
        margin-bottom: 12px;
    }

    .sms-image {
        max-width: 30%;
        overflow: hidden;
        border-radius: 20px;
        margin-left: auto;
        max-height: 400px;
        margin-bottom: 12px;
    }

    .push-msg {
        border: 1px solid #DBDBDB;
        border-radius: 4px;
        box-shadow: 0px 6px 8px rgba(0, 0, 0, 0.1);
        font-size: 14px;
        margin-bottom: 12px;
    }

    .push-msg-header {
        padding: 8px 12px 6px;
        display: flex;
        align-items: center;
    }

    .push-msg-body {
        padding: 2px 12px 12px;
        display: flex;
        flex-direction: column;
    }

    .push-msg-icon {
        width: 16px;
        height: 16px;
        background: #EE7725;
        border-radius: 3px;
        margin-right: 8px;
    }

    .push-msg-app-name {
        font-weight: 600;
        color: #737475;
    }

    .push-msg-title {
        font-weight: 500;
    }
</style>
<asp:UpdatePanel ID="upPanel" runat="server" UpdateMode="Conditional" ChildrenAsTriggers="false">
    <%-- Block updating is set to conditional and automatic triggering disabled to prevent unwanted page refreshes during postback when fetching a communication detail panel. --%>
    <ContentTemplate>
        <div class="panel panel-block">
            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-comments-o"></i><asp:Literal ID="lBlockTitle" runat="server"></asp:Literal></h1>
            </div>
            <div class="panel-body">
                <div class="grid grid-panel">
                    <%-- The filter is enclosed in an UpdatePanel to prevent unwanted updates during postback from the Communications List. --%>
                    <asp:UpdatePanel ID="upFilter" runat="server">
                        <ContentTemplate>
                            <Rock:GridFilter ID="rFilter" runat="server" FieldLayout="Custom">
                                <div class="row">
                                    <div class="col-md-8">
                                        <Rock:RockTextBox ID="tbSubject" runat="server" Label="Subject" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlMedium" runat="server" Label="Medium" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:PersonPicker ID="ppCreatedBy" runat="server" Label="Created By" EnableSelfSelection="true" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:DateRangePicker ID="drpDates" runat="server" Label="Sent Date Range" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlSystemCommunicationType" runat="server" Label="System Communication Type" EnhanceForLongLists="true" />
                                    </div>
                                </div>
                                <div class="row">
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlTemplate" runat="server" Label="Communication Template" EnhanceForLongLists="true" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlStatus" runat="server" Label="Status" />
                                    </div>
                                    <div class="col-md-4">
                                        <Rock:RockDropDownList ID="ddlBulk" runat="server" Label="Bulk Message" />
                                    </div>
                                </div>
                            </Rock:GridFilter>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                    <div class="communication-grid">
                        <Rock:Grid CssClass="communication-grid" ID="gCommunication" runat="server" AllowSorting="false" PageSizes="50,100"
                            RowClickEnabled="false" TableStriped="false">
                            <Columns>
                                <Rock:RockTemplateField ItemStyle-CssClass="py-0">
                                    <ItemTemplate>
                                        <asp:UpdatePanel runat="server" ID="upCommunicationItem" UpdateMode="Conditional" ChildrenAsTriggers="false">
                                            <ContentTemplate>
                                                <asp:Literal ID="lCommunicationDetailRow" runat="server"></asp:Literal>
                                            </ContentTemplate>
                                        </asp:UpdatePanel>
                                    </ItemTemplate>
                                </Rock:RockTemplateField>
                            </Columns>
                        </Rock:Grid>
                    </div>
                </div>
            </div>
        </div>
        <script>
            <%-- Toggles the visibility of the detail panel for an entry in the Communication List. --%>
            function toggleCommunicationDetail ( ctlToggle, rowId, closedStateButtonHtml, openStateButtonHtml )
            {
                html = $( '#details-' + rowId ).is( ':visible' ) ? closedStateButtonHtml : openStateButtonHtml;
                $( ctlToggle ).html( html );
                $( '#details-' + rowId ).slideToggle();
            }
        </script>
    </ContentTemplate>
</asp:UpdatePanel>