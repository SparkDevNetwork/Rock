<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MultiPersonSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.MultiPersonSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').on('click', function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
    <ContentTemplate>

        <asp:Panel ID="pnlContent" runat="server">

            <Rock:ModalAlert ID="maWarning" runat="server" />

            <asp:Panel ID="pnlSelection" runat="server">
                <div class="checkin-header">
                    <h1><asp:Literal ID="lTitle" runat="server" /></h1>
                </div>

                <div class="checkin-body">

                    <div class="checkin-scroll-panel">
                        <div class="scroller">

                            <div class="control-group checkin-body-container">
                                <label class="control-label"><asp:Literal ID="lCaption" runat="server" /></label>
                                <asp:LinkButton CssClass="btn btn-link pull-right" ID="lbEditFamily" runat="server" OnClick="lbEditFamily_Click" Text="<i class='fa fa-pencil-alt'></i> Edit Family" />
                                <div class="controls checkin-person-list">
                                    <asp:Repeater ID="rSelection" runat="server">
                                        <ItemTemplate>
                                            <div class="row row-no-gutters row-eq-height row-checkin-item">
                                                <asp:Panel ID="pnlPersonButton" runat="server" CssClass="col-xs-12 checkin-person">
                                                    <a data-person-id='<%# Eval("Person.Id") %>' class="btn btn-primary btn-checkin-select btn-block js-person-select <%# GetSelectedClass( (bool)Eval("PreSelected") ) %> <%# GetDisabledClass( (bool)Eval("AnyPossibleSchedules") ) %>">
                                                        <div class="row">
                                                            <div class="checkbox-container">
                                                                <i class='fa fa-3x <%# GetCheckboxClass( (bool)Eval("PreSelected") ) %>'></i>
                                                            </div>
                                                            <asp:Panel ID="pnlPhoto" runat="server" CssClass="photo-container">
                                                                <div class="photo-round photo-round-md pull-left" style="display: block; background-image: url('<%# GetPersonImageTag( Eval("Person") ) %>');"></div>
                                                            </asp:Panel>
                                                            <asp:Panel ID="pnlPerson" CssClass="name-container" runat="server">
                                                                <asp:Literal ID="lPersonButton" runat="server"></asp:Literal>
                                                                <div class='text-light text-left'><asp:Literal ID="lPersonSelectLava" runat="server"></asp:Literal></div>
                                                            </asp:Panel>
                                                        </div>
                                                    </a>
                                                </asp:Panel>
                                                <asp:Panel ID="pnlChangeButton" runat="server" CssClass="col-xs-12 col-sm-3 col-md-2 checkin-change" Visible="false">
                                                    <asp:LinkButton ID="lbChange" runat="server" CssClass="btn btn-default btn-checkin-select btn-block btn-checkin-change" CommandArgument='<%# Eval("Person.Id") %>' CommandName="Change">Change</asp:LinkButton>
                                                </asp:Panel>
                                            </div>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>

                        </div>
                    </div>

                    <asp:HiddenField ID="hfPeople" runat="server"></asp:HiddenField>

                </div>

                <div class="checkin-footer">
                    <div class="checkin-actions">
                        <asp:LinkButton CssClass="btn btn-primary " ID="lbSelect" runat="server" OnClientClick="return GetPersonSelection();" OnClick="lbSelect_Click" Text="Next" data-loading-text="Loading..." />
                        <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
                        <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />

                    </div>
                </div>
            </asp:Panel>

            <asp:Panel ID="pnlOptions" runat="server" Visible="false">

                <div class="checkin-header">
                    <h1>
                        <asp:Literal ID="lOptionTitle" runat="server" /><div class="checkin-sub-title">
                            <asp:Literal ID="lOptionSubTitle" runat="server"></asp:Literal></div>
                    </h1>
                </div>

                <div class="checkin-body">

                    <asp:HiddenField ID="hfPersonId" runat="server" />

                    <div class="checkin-scroll-panel">
                        <div class="scroller">

                            <div class="control-group checkin-body-container">
                                <label class="control-label">Select Options</label>
                                <div class="controls checkin-option-list">
                                    <asp:Repeater ID="rOptions" runat="server">
                                        <ItemTemplate>
                                            <a data-key='<%# Eval("Key") %>' data-schedule-id='<%# Eval("Schedule.Schedule.Id") %>'
                                                class='btn btn-primary btn-checkin-select btn-block js-option-select <%# (bool)Eval("Disabled") ? "btn-dimmed" : "" %>' style="text-align: left">
                                                <div class="row">
                                                    <div class="checkbox-container">
                                                        <i class='fa fa-2x <%# GetCheckboxClass( (bool)Eval("Selected") ) %>'></i>
                                                    </div>
                                                    <asp:Panel ID="pnlOption" CssClass="col" runat="server"><%# GetOptionText( Container.DataItem ) %></asp:Panel>
                                                </div>
                                            </a>
                                        </ItemTemplate>
                                    </asp:Repeater>
                                </div>
                            </div>

                        </div>
                    </div>

                    <asp:HiddenField ID="hfOptions" runat="server" />

                </div>

                <div class="checkin-footer">
                    <div class="checkin-actions">
                        <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbOptionCacncel" runat="server" OnClick="lbOptionCancel_Click" Text="Cancel" />
                        <asp:LinkButton CssClass="btn btn-primary pull-right" ID="lbOptionSelect" runat="server" OnClientClick="return GetOptionSelection();" OnClick="lbOptionSelect_Click" Text="Ok" />


                    </div>
                </div>

            </asp:Panel>

        </asp:Panel>

    </ContentTemplate>
</asp:UpdatePanel>
