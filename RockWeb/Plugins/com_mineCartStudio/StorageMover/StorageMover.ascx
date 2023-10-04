<%@ Control Language="C#" AutoEventWireup="true" CodeFile="StorageMover.ascx.cs" Inherits="RockWeb.Plugins.com_mineCartStudio.StorageMover.StorageMover" %>

<script src="/SignalR/hubs"></script>

<script type="text/javascript">
    $(function () {

        var proxy = $.connection.rockMessageHub;

        proxy.client.receiveNotification = function (name, message, result) {
            if (name == '<%=this.SignalRNotificationKey %>') {
                $('#<%=pnlProgress.ClientID%>').show();
                $('#<%=pnlSelectCriteria.ClientID%>').hide();
                $('#<%=btnMove.ClientID%>').hide();

                if (message) {
                    $('#<%=lMessage.ClientID %>').html(message);
                }

                if (result && result!=='') {
                    $('#<%=divAlert.ClientID %>').show();
                    $('#<%=lProgressResults.ClientID %>').html(result);
                } else {
                    $('#<%=divAlert.ClientID %>').hide();
                }
            }
        }
        proxy.client.updateProgress = function (percentComplete, message) {
            $('#progressBar').css('width', percentComplete + '%').attr('aria-valuenow', percentComplete);
            $('#progressBar').html(percentComplete + '%');
            if (message) {
                $('#<%=lProgressMessage.ClientID %>').html(message);
            }
        };

        proxy.client.showResults = function (name, message) {
            if (name == '<%=this.SignalRNotificationKey %>') {

                if (message) {
                    $('#<%=pnlActions.ClientID%>').hide();
                    $('#<%=pnlProgress.ClientID%>').hide();
                    $('#<%=nbresult.ClientID%>').show();
                    $('#<%=nbresult.ClientID%>').html(message);
                }
            }
        }
        $.connection.hub.start().done(function () {
            // hub started... do stuff here if you want to let the user know something
        });
    })
</script>



<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>
        
        <asp:Panel ID="pnlView" runat="server" CssClass="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title"><i class="fa fa-truck" aria-hidden="true"></i> Storage Mover</h1>
            </div>
            <div class="panel-body">

                <asp:Panel ID="pnlWarning" runat="server">
                    <div class="alert alert-danger">
                        <p>
                            <strong>Important:</strong> Before transitioning storage providers please ensure you have a up-to-date backup of your files and database.
                        </p>
                    </div>

                    <asp:LinkButton ID="btnConfirm" runat="server" OnClick="btnConfirm_Click" CssClass="btn btn-sm btn-primary">Continue</asp:LinkButton>
                </asp:Panel>

                <asp:Panel ID="pnlConfirmed" runat="server" Visible="false">

                    <asp:ValidationSummary ID="valSummaryTop" runat="server" HeaderText="Please Correct the Following" CssClass="alert alert-danger" />
                    <Rock:NotificationBox ID="nbMessages" runat="server"></Rock:NotificationBox>


                    <div class="row">
                        <div class="col-md-4">
                            <Rock:BinaryFileTypePicker ID="btpFileType" runat="server" Label="File Type" OnSelectedIndexChanged="btpFileType_SelectedIndexChanged" Required="true" AutoPostBack="true" CssClass="input-width-xxl" />
                        </div>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlSelectCriteria" Visible="false" runat="server">
                    <div class="row">
                        <div class="col-md-8 margin-t-md margin-b-md">
                            <Rock:Grid ID="gStorageLists" runat="server" ShowFooter="false" ShowActionRow="false" AllowPaging="false" DisplayType="Light">
                                <Columns>
                                    <asp:BoundField DataField="Name" HeaderText="Storage Type" />
                                    <asp:BoundField DataField="Files" HeaderText="Number Of Files" />
                                    <asp:BoundField DataField="StorageFormatted" HeaderText="Storage Size" />
                                </Columns>
                            </Rock:Grid>
                        </div>
                    </div>

                    <asp:Panel ID="pnlAction" Visible="false" runat="server">
                        <div class="row">
                            <div class="col-md-6">
                                <Rock:RockDropDownList ID="ddlSourceStorage" runat="server" Label="Source Storage Type" Required="true" CssClass="input-width-xxl" />
                                <Rock:NumberBox ID="nbMaxFiles" runat="server" CssClass="input-width-sm" Text="1000" Label="Max Files To Move" Help="The maximum files to move at one time. Enter 1 to test the move." />
                            </div>
                            <div class="col-md-6">
                                <Rock:RockLiteral ID="lDestinationStorage" runat="server" Label="Destination Storage" />
                            </div>
                        </div>
                    </asp:Panel>
                </asp:Panel>

                <asp:Panel ID="pnlProgress" Visible="false" runat="server">
                    <div class="row" id="progressContainer">
                        <div class="col-md-12">
                            <Rock:RockLiteral ID="lMessage" runat="server" />
                        </div>
                        <div class="col-md-12">
                            <div class="progress-container ">
                                <div class="progress active" id="progress1">
                                    <div class="progress-bar" id="progressBar" role="progressbar" aria-valuenow="0" aria-valuemin="0" aria-valuemax="100" style="width: 0%"></div>
                                </div>
                            </div>
                            <asp:Label ID="lProgressMessage" runat="server" />
                        </div>
                    </div>
                    <div id="divAlert" runat="server" class="row" style="display:none;">
                        <div class="col-md-12">
                            <strong>Details</strong><br />
                            <div class="alert alert-danger">
                                <pre><asp:Label ID="lProgressResults" CssClass="js-progressResults" runat="server" /></pre>
                            </div>
                        </div>
                    </div>
                </asp:Panel>
                <Rock:RockLiteral ID="nbresult" runat="server"  />

                <asp:Panel ID="pnlActions" class="actions" runat="server" Visible="false">
                    <asp:LinkButton ID="btnMove" runat="server" AccessKey="m" ToolTip="Alt+m" Text="Move" CssClass="btn btn-primary" OnClick="btnMove_Click" />
                </asp:Panel>
            </div>
            
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
