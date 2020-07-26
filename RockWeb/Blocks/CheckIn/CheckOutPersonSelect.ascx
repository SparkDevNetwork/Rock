<%@ Control Language="C#" AutoEventWireup="true" CodeFile="CheckOutPersonSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.CheckOutPersonSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').on('click', function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>


    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1><asp:Literal ID="lTitle" runat="server"></asp:Literal></h1>
    </div>

    <div class="checkin-body">

        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <label class="control-label"><asp:Literal ID="lCaption" runat="server"></asp:Literal></label>
                    <div class="controls checkin-person-list" >
                        <asp:Repeater ID="rSelection" runat="server" >
                            <ItemTemplate>
                                <a person-id='<%# Eval("Person.Id") %>' Class="btn btn-primary btn-checkin-select btn-block js-person-select <%# GetSelectedClass( (bool)Eval("Selected") ) %>" style="text-align:left">
                                    <div class="row">
                                        <div class="checkbox-container" >
                                            <i class='<%# GetCheckboxClass( (bool)Eval("Selected") ) %>'></i>
                                        </div>
                                        <asp:panel id="pnlPhoto" runat="server" CssClass="photo-container">
                                            <div class="photo-round photo-round-md pull-left" style="display: block; background-image: url('<%# GetPersonImageTag( Eval("Person") ) %>');"></div>
                                        </asp:panel>
                                        <asp:Panel ID="pnlPerson" CssClass="name-container" runat="server"><div class="family-personselect"><%# Container.DataItem.ToString() %></div></asp:Panel>
                                    </div>
                                </a>
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
            <asp:LinkButton CssClass="btn btn-default btn-back" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default btn-cancel" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
            <asp:LinkButton CssClass="btn btn-primary pull-right btn-next" ID="lbSelect" runat="server" OnClientClick="return GetPersonSelection();" OnClick="lbSelect_Click" Text="Next" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
