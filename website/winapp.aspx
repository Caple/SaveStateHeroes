<%@ Page Language="VB" %>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">

<html xmlns="http://www.w3.org/1999/xhtml">
    <head runat="server">
        <title>ssH Windows App</title>
        <link href="images/site.ico" rel="shortcut icon" type="image/x-icon" />
        <style type="text/css">
        
            html {
                height: 100%;
            }
        
            body {
                height: 100%;
                margin: 0;
                color: #FFF;
                background: rgb(132,137,145);
                background: -moz-radial-gradient(center, ellipse cover,  rgba(132,137,145,1) 0%, rgba(31,40,45,1) 100%);
                background: -webkit-gradient(radial, center center, 0px, center center, 100%, color-stop(0%,rgba(132,137,145,1)), color-stop(100%,rgba(31,40,45,1)));
                background: -webkit-radial-gradient(center, ellipse cover,  rgba(132,137,145,1) 0%,rgba(31,40,45,1) 100%);
                background: -o-radial-gradient(center, ellipse cover,  rgba(132,137,145,1) 0%,rgba(31,40,45,1) 100%);
                background: -ms-radial-gradient(center, ellipse cover,  rgba(132,137,145,1) 0%,rgba(31,40,45,1) 100%);
                background: radial-gradient(ellipse at center,  rgba(132,137,145,1) 0%,rgba(31,40,45,1) 100%);
                filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#848991', endColorstr='#1f282d',GradientType=1 );
            }

            a {
                color: #ddd;
            }

            a:hover {
                color: #bbb;
            }

            a:active {
                color: #888;
            }
       
            #content  {
                width: 800px;
                height: auto;
                position: absolute;
                left: 50%;
                top: 10%; 
                padding: 20px;
                margin-left: -400px;
                border-style:solid;
                border-width:medium;
                font-size: 120%;
                font-size: 90%;
                font-family: Verdana;

                background: rgb(103,107,114);
                background: -moz-radial-gradient(center, ellipse cover,  rgba(103,107,114,1) 0%, rgba(22,22,22,1) 100%);
                background: -webkit-gradient(radial, center center, 0px, center center, 100%, color-stop(0%,rgba(103,107,114,1)), color-stop(100%,rgba(22,22,22,1)));
                background: -webkit-radial-gradient(center, ellipse cover,  rgba(103,107,114,1) 0%,rgba(22,22,22,1) 100%);
                background: -o-radial-gradient(center, ellipse cover,  rgba(103,107,114,1) 0%,rgba(22,22,22,1) 100%);
                background: -ms-radial-gradient(center, ellipse cover,  rgba(103,107,114,1) 0%,rgba(22,22,22,1) 100%);
                background: radial-gradient(ellipse at center,  rgba(103,107,114,1) 0%,rgba(22,22,22,1) 100%);
                filter: progid:DXImageTransform.Microsoft.gradient( startColorstr='#676b72', endColorstr='#161616',GradientType=1 );


            }

            #screenshot {
                float:left;
                margin-bottom: 10px;
            }

            #description {
                margin-left: 5px;
                margin-right: 20px;
                margin-bottom: 10px; 
                margin-top: 20px;
                width: 450px;
                text-indent : 20px;
            }

            #downloadLink {
                float: right;
                margin-right: 20px;
                margin-top: 3px;
                padding-left: 50px;
                font-size: 200%;
                height: 100%;
                border-left: 1px solid #fff;
            }

            #verHeader {
                float: right;
                margin-right: 20px;
                margin-top: 15px;
                padding-left: 50px;
                font-size: 200%;
                height: 100%;
                font-size: 65%
            }

        </style>

    </head>
    <body>
        <div id="content">
            <img id="screenshot" src="client_windows/screenshot.png"/>
            <div style="margin-bottom: 160px;"></div><hr />
            <table>
                <tr>
                    <td>
                        <div id="description">
                            Want to know when we're streaming next, but don't want to sit around on the website all day? 
                            Grab our windows client and be notified about all streams right from your desktop.
                        </div>
                    </td>
                    <td>
                        <div id="verHeader">installer version: v04</div>
                        <div id="downloadLink">  
                            <a href="/client_windows/installers/ssh_install_v04.exe">Download Now</a>
                        </div>
                    </td>
                </tr>
            </table>


        </div>
    </body>
</html>
