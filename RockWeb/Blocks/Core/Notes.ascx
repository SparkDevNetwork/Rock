<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Notes.ascx.cs" Inherits="RockWeb.Blocks.Core.Notes" %>

<asp:UpdatePanel ID="upNotes" runat="server">
<ContentTemplate>

<div class="span8 person-notes-container">
    <section id="person-notes" class="person-notes scroll-container">
        <header class="group">
            <h4><asp:Literal ID="lTitle" runat="server"></asp:Literal></h4>
            <a id="note-add" class="note-add btn"><i class="icon-plus"></i></a>

            <script>

                $(document).ready(function () {

                    $('#note-add').click(function () {
                        $('#note-entry').slideToggle("slow");
                    });

                });

            </script>

        </header>
                    
        <div id="note-entry" style="display: none;">
            <label>Note</label>
            <asp:TextBox ID="tbNewNote" runat="server" TextMode="MultiLine"></asp:TextBox>
 
            <div class="row-fluid">
                <div class="span4">
                    <label class="checkbox">
                        <asp:CheckBox ID="cbAlert" runat="server" />
                        Alert
                    </label>
                </div>
                <div class="span4">
                    <label class="checkbox">
                        <asp:CheckBox ID="cbPrivate" runat="server" />
                        Private
                    </label>
                </div>
                <div class="span4">
                    <button class="btn btn-mini" type="button"><i class="icon-lock"></i> Security</button>
                    <asp:Button ID="btnAddNote" runat="server" CssClass="btn btn-mini" Text="Add" />
                </div>
            </div>
        </div>

        <div class="person-notes-details">
            <div class="scrollbar" style="height: 150px;">
                <div class="track" style="height: 150px;">
                    <div class="thumb" style="top: 0px; height: 126.949px;">
                        <div class="end"></div>
                    </div>
                </div>
            </div>
            <div class="viewport">
                <div class="note-container-top"></div>
                <div class="note-container overview" >
                    <asp:PlaceHolder ID="phNotes" runat="server"></asp:PlaceHolder>
                    <asp:Xml ID="xmlNotes" runat="server"></asp:Xml>
                </div>
                <div class="note-container-bottom"></div>
            </div>
        </div>
    </section>
        
    <script>
        $(document).ready(function () {
            $('#person-notes').tinyscrollbar({ size: 150 });
        });
    </script>
        
</div>

</ContentTemplate>
</asp:UpdatePanel>
