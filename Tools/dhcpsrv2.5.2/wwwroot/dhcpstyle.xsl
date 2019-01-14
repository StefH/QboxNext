<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:template match="/servers">
    <html>
    <head>
    <title>DHCP Server Status</title>
    <META HTTP-EQUIV="Pragma" CONTENT="no-cache" />
    <META HTTP-EQUIV="Expires" CONTENT="-1" />
     <style type="text/css">
         body { margin:20px; padding:0; }
         #frame { position:absolute; float:left; width:876px; height:196px; border:0px; }
         #r1 { width:876px; height:146px; position:absolute; top:0px; left:0px; border:0px;}
         #r2 { width:123px; height:50px; position:absolute; top:146px; left:0px; border:0px; }
         #r3 { width:753px; height:50px; position:absolute; top:146px; left:123px; line-height:50px;  border:0px solid white; padding:0px; background-color:Black; color:white; overflow:hidden; }
         #pbody { width:876px; position:absolute; top:240px; }

         table.mytable { width:876px; border-collapse:collapse; }
         td.mytable { border:0px solid #000; vertical-align:middle; overflow:hidden; margin:0px; padding:3px; }
         h2.mytable { vertical-align:middle; overflow:hidden; margin:0px; padding:3px; }

         table.dhcpclass { width:876px; border-collapse:collapse; }
         td.dhcpclass { border:1px solid #000; vertical-align:top; overflow:hidden; padding:3px; }
         th.dhcpclass { border:1px solid #000; vertical-align:top; overflow:hidden; background-color:#D8D8D8; text-align:left; padding:3px; }

     </style>
     </head>
    <BODY>
    <div id="frame">
        <div id="r1"><img src="RJ45_5.JPG"/></div>
        <div id="r2"><img src="RJ45_52.JPG"/></div>
        <div id="r3"><h2 style="padding-left:50px; margin:0px; ">DHCP Server Status</h2></div>
    </div>
    
    <div id="pbody">
    
    <xsl:apply-templates select="server"/>
    
    <br /><br /> <a href="http://www.dhcpserver.de">Visit the DHCP Server homepage</a>

    </div>

    </BODY>
    <head>
    <META HTTP-EQUIV="Pragma" CONTENT="no-cache" />
    <META HTTP-EQUIV="Expires" CONTENT="-1" />
    </head>
    </html>
  </xsl:template>    
    
    
  <xsl:template match="server">
        
    <table class="mytable">
     <tr>
       <td class="mytable">
         <h2 class="mytable"><xsl:value-of select="name"/> Connections</h2>             
       </td>
       <td class="mytable" style="text-align:right;">
             <a>
               <xsl:attribute name="href">
                 dhcptrace.txt?s=<xsl:value-of select="name"/>
               </xsl:attribute>Show Trace for <xsl:value-of select="name"/>
             </a>
       </td>
     </tr>
    </table>


     <table class="dhcpclass"><tr>
       <th class="dhcpclass">Index</th>
       <th class="dhcpclass">Bind</th>
       <th class="dhcpclass">IP Address</th>
       <th class="dhcpclass">Type</th>
       <th class="dhcpclass">Port</th>
       <th class="dhcpclass">Status</th>
     </tr>

     <xsl:apply-templates select="connections/connection">
         <xsl:with-param name="server_index"> <xsl:value-of select="index"/> </xsl:with-param>
     </xsl:apply-templates>
     </table>
     
    <br />
    <table class="mytable">
     <tr>
       <td class="mytable">
         <h2 class="mytable"><xsl:value-of select="name"/> DHCP Clients</h2>             
       </td>
       <td class="mytable" style="text-align:right;">
       </td>
     </tr>
    </table>

     <table class="dhcpclass"><tr>
       <th class="dhcpclass">Id</th>
       <th class="dhcpclass">IP Address</th>
       <th class="dhcpclass">Hostname</th>
       <th class="dhcpclass">AutoConfig</th>
       <th class="dhcpclass">Lease ends</th>
     </tr>
     <xsl:apply-templates select="clients/client"/>
     </table>
  </xsl:template>

  <xsl:template match="connection">
     <xsl:param name="server_index" />

     <tr>
       <td class="dhcpclass"><xsl:value-of select="index"/></td>
       <td class="dhcpclass"><xsl:value-of select="bind_index"/>=<xsl:value-of select="bind"/></td>
       <td class="dhcpclass"><xsl:value-of select="ipaddr"/></td>

       <td class="dhcpclass">
                 <div style="float:left;"><xsl:value-of select="type"/></div>

                 <xsl:if test="dhcppackets">
                  <div style="float:right;"><a>
                     <xsl:attribute name="href">
                       dhcpstatus.xml?cidx=<xsl:value-of select="index"/>&amp;sidx=<xsl:value-of select="$server_index"/>&amp;mode=pkgtrace
                     </xsl:attribute>capture
                  </a></div>
                 </xsl:if>
       </td>

       <td class="dhcpclass"><xsl:value-of select="port"/></td>
       <td class="dhcpclass"><xsl:value-of select="status"/></td>
     </tr>
  </xsl:template>

  <xsl:template match="client">
     <tr>
       <td class="dhcpclass"><xsl:value-of select="id"/></td>
       <td class="dhcpclass"><xsl:value-of select="ipaddr"/></td>
       <td class="dhcpclass"><xsl:value-of select="hostname"/></td>
       <td class="dhcpclass"><xsl:value-of select="autoconfig"/></td>
       <td class="dhcpclass"><xsl:value-of select="lease_end_utc"/></td>
    </tr>
  </xsl:template>

</xsl:stylesheet>
