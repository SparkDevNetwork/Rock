<!DOCTYPE html>
<html>
	<head>
		<title>Rock Diagnose - Rock Pill</title>
		<link rel='stylesheet' href='//fonts.googleapis.com/css?family=Open+Sans:300,400,600,700' type='text/css' />
		<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/twitter-bootstrap/3.4.1/css/bootstrap.min.css" integrity="sha256-bZLfwXAP04zRMK2BjiO8iu9pf4FbLqX6zitd+tIvLhE=" crossorigin="anonymous" />
		<link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css" integrity="sha256-eZrrJcwDc/3uDhsdt61sL2oOBY362qM3lon1gyExkL0=" crossorigin="anonymous" />

		<script src="https://cdnjs.cloudflare.com/ajax/libs/jquery/1.12.4/jquery.min.js" integrity="sha256-ZosEbRLbNQzLpnKIkEdrPv7lOy9C27hHQ+Xp8a4MxAQ=" crossorigin="anonymous"></script>

		<link href="//rockrms.blob.core.windows.net/pills/Styles/rock-pills.css" rel="stylesheet" />
		<link rel="shortcut icon" href="//rockrms.blob.core.windows.net/pills/Images/favicon.ico" />

	</head>
	<body>

		<div id="content">
            <h1 id="logo"></h1>

			<h1>Rock Pill</h1>

			<div id="console">
				<ul>
					<%
						dsn = "Provider='sqloledb';" & ImportConnectionString("web.ConnectionStrings.config", "RockContext", false)

						sql = "SELECT TOP 50 * FROM [ExceptionLog]"
						Set oConn = Server.CreateObject("ADODB.Connection")
						Set oRS = Server.CreateObject("ADODB.RecordSet")
						oConn.Open dsn
						oRS.Open sql, oConn

						If NOT oRS.EOF Then
						   oRS.MoveFirst
						   Do
							  Response.Write("<li><span class='date'>" & oRS("CreatedDateTime") & "</span> Id:" & oRS("Id") & " - " & oRS("ExceptionType") & "<br>")
							  Response.Write(oRS("Description") & "</li>")
							  oRS.MoveNext
						   Loop Until oRS.EOF
						End If
					%>
				</ul>
			</div>


		</div>
	</body>
</html>

<%

Function ImportConnectionString(webConfig, attrName, reformatDSN)
    Dim oXML, oNode, oChild, oAttr, dsn
    Set oXML=Server.CreateObject("Microsoft.XMLDOM")
    oXML.Async = "false"
    oXML.Load(Server.MapPath(webConfig))
    Set oNode = oXML.GetElementsByTagName("connectionStrings").Item(0)
    Set oChild = oNode.GetElementsByTagName("add")
    ' Get the first match
    For Each oAttr in oChild
        If  oAttr.getAttribute("name") = attrName then
            dsn = oAttr.getAttribute("connectionString")
            If reformatDSN Then
                ' Optionally reformat the connection string (adjust as needed)
                dsn = Replace(dsn, "User ID=", "UID=")
                dsn = Replace(dsn, "Password=", "PWD=")
                dsn = Replace(dsn, "Data Source=", "Server=")
                dsn = Replace(dsn, "Initial Catalog=", "Database=")
                dsn = Replace(dsn, "Persist Security Info=True;", "")
                dsn = "Provider=MSDASQL;Driver={SQL Server};" & dsn
            End If
            ImportConnectionString = dsn
            Exit Function
        End If
    Next
End Function

%>