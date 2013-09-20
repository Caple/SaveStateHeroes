<%@ Page Language=VB Debug=true %>
<script runat=server>
Sub Page_Load(ByVal Sender as Object, ByVal E as EventArgs)
    lblMessage.Text = "www.java2s.com"
End Sub
</SCRIPT>
<HTML>
<HEAD>
<TITLE>Test page</TITLE>
</HEAD>
<BODY>
    <form runat="server">
    <Font Face="Tahoma">
    <asp:Label id="lblTitle" Width="90%" Font-Size="25pt" Font-Name="Arial"  Text="Test Page"
        runat="server"
    />
    <asp:Label
        id="lblMessage"
        runat="Server"
        Font-Bold="True"
    />
</Font>
</Form>
</BODY>
</HTML>
