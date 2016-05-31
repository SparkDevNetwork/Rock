<%@ Control Language="C#" AutoEventWireup="true" CodeFile="MultiPersonSelect.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.MultiPersonSelect" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $('a.btn-checkin-select').click(function () {
            $(this).siblings().attr('onclick', 'return false;');
        });
    });
</script>

<asp:UpdatePanel ID="upContent" runat="server">
<ContentTemplate>


    <Rock:ModalAlert ID="maWarning" runat="server" />

    <div class="checkin-header">
        <h1><asp:Literal ID="lFamilyName" runat="server"></asp:Literal></h1>
    </div>
                
    <div class="checkin-body">
        
        <div class="checkin-scroll-panel">
            <div class="scroller">

                <div class="control-group checkin-body-container">
                    <label class="control-label">Select People</label>
                    <div class="controls checkin-person-list" >
                        <asp:Repeater ID="rSelection" runat="server" >
                            <ItemTemplate>
                                <a person-id='<%# Eval("Person.Id") %>' Class="btn btn-primary btn-checkin-select btn-block js-person-select" style="text-align:left">
                                    <div class="row">
                                        <div class="col-md-2">
                                            <div class="row">
                                                <div class="col-xs-6"><i class='<%# GetCheckboxClass( (bool)Eval("PreSelected") ) %>'></i></div>
                                                <div class="col-xs-6">
                                                    <div class="photo-round photo-round-md pull-left" style="display: block; background-image: url('<%# GetPersonImageTag( Eval("Person") ) %>');"></div>
                                                </div>
                                            </div>
                                        </div>
                                        <div class="col-md-10 family-personselect"><%# Container.DataItem.ToString() %></div>
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
            <asp:LinkButton CssClass="btn btn-default" ID="lbBack" runat="server" OnClick="lbBack_Click" Text="Back" />
            <asp:LinkButton CssClass="btn btn-default" ID="lbCancel" runat="server" OnClick="lbCancel_Click" Text="Cancel" />
            <asp:LinkButton CssClass="btn btn-primary pull-right" ID="lbSelect" runat="server" OnClientClick="return GetPersonSelection();" OnClick="lbSelect_Click" Text="Next" />
        </div>
    </div>

</ContentTemplate>
</asp:UpdatePanel>
