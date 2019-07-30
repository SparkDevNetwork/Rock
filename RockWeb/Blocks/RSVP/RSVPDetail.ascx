<%@ Control Language="C#" AutoEventWireup="true" CodeFile="RSVPDetail.ascx.cs" Inherits="RockWeb.Blocks.Event.RSVPDetail" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <div class="panel panel-block">

            <div class="panel-heading">
                <h1 class="panel-title pull-left">
                    <i class="fa fa-user-check"></i>
                    <asp:Literal ID="lHeading" runat="server" Text="RSVP Detail" />
                </h1>
            </div>

            <div class="panel-body">
                <asp:Panel ID="pnlDetails" runat="server">

                    <div class="row">
                        <div class="col-sm-6">
                            <div class="row">
                                <div class="col-sm-12">
                                    <Rock:RockLiteral ID="lOccurrenceDate" runat="server" Label="Date" />
                                </div>
                                <div class="col-sm-12">
                                    <Rock:RockLiteral ID="lLocation" runat="server" Label="Location" />
                                </div>
                                <div class="col-sm-12">
                                    <Rock:RockLiteral ID="lSchedule" runat="server" Label="Schedule" />
                                </div>
                            </div>
                        </div>
                        <div class="col-sm-6">
                            <!-- chart -->
                            <canvas id="doughnutChartCanvas" runat="server"></canvas>
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbEditOccurrence" runat="server" AccessKey="e" ToolTip="Alt+e" Text="Edit" CssClass="btn btn-primary" OnClick="lbEditOccurrence_Click" CausesValidation="false" />
                    </div>
                </asp:Panel>

                <asp:Panel ID="pnlEdit" runat="server">
                    <div class="row">
                        <div class="col-sm-6">
                            <Rock:DatePicker ID="dpOccurrenceDate" runat="server" Label="Date" Required="true" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:SchedulePicker ID="spSchedule" runat="server" Label="Schedule" />
                        </div>
                        <div class="col-sm-6">
                            <Rock:RockDropDownList ID="rddlLocation" runat="server" Label="Location" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:HtmlEditor ID="heAcceptMessage" runat="server" Toolbar="Light" Label="Custom Accept Message" />
                        </div>
                    </div>
                    <div class="row">
                        <div class="col-sm-12">
                            <Rock:HtmlEditor ID="heDeclineMessage" runat="server" Toolbar="Light" Label="Custom Decline Message" />
                        </div>
                    </div>

                    <div class="actions">
                        <asp:LinkButton ID="lbSaveOccurrence" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSaveOccurrence_Click" />
                        <asp:LinkButton ID="lbCancelOccurrence" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancelOccurrence_Click" CausesValidation="false" />
                    </div>

                </asp:Panel>

            </div>

        </div>

        <asp:Panel ID="pnlAttendees" runat="server" CssClass="panel panel-block">
            <div class="panel-body">

                    <Rock:Grid ID="gAttendees" runat="server" DisplayType="Light" ExportSource="ColumnOutput" OnRowDataBound="gAttendees_RowDataBound" DataKeyNames="PersonId">
                        <Columns>
                            <Rock:RockBoundField DataField="FullName" HeaderText="Invitees" />
                            <Rock:RockTemplateField HeaderText="Status" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="grid-select-field">
                                <ItemTemplate>
                                    <Rock:RockRadioButtonList ID="rrblRSVPStatus" runat="server" RepeatDirection="Horizontal">
                                        <asp:ListItem Text="Accept" Value="Accept" />
                                        <asp:ListItem Text="Decline" Value="Decline" />
                                    </Rock:RockRadioButtonList>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Decline Reason" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="grid-select-field">
                                <ItemTemplate>
                                    <Rock:DataDropDownList ID="rddlDeclineReason" runat="server" SourceTypeName="Rock.Model.DefinedValue" DataTextField="Value" DataValueField="Id">
                                        <asp:ListItem Text="" Value="" />
                                    </Rock:DataDropDownList>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Decline Note" HeaderStyle-HorizontalAlign="Center" ItemStyle-HorizontalAlign="Center" ItemStyle-CssClass="grid-select-field">
                                <ItemTemplate>
                                    <Rock:RockTextBox ID="tbDeclineNote" runat="server" Text='<%# Eval("DeclineNote") %>' />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                        </Columns>
                    </Rock:Grid>

                    <div class="actions">
                        <asp:LinkButton ID="lbSave" runat="server" AccessKey="s" ToolTip="Alt+s" Text="Save" CssClass="btn btn-primary" OnClick="lbSave_Click" CausesValidation="false" />
                        <asp:LinkButton ID="lbCancel" runat="server" AccessKey="c" ToolTip="Alt+c" Text="Cancel" CssClass="btn btn-link" OnClick="lbCancel_Click" CausesValidation="false"></asp:LinkButton>
                    </div>

            </div>
        </asp:Panel>
    </ContentTemplate>
</asp:UpdatePanel>
