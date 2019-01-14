<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">

  <xsl:template match="/dhcppackets">
    <html>
    <head>
    <title>DHCP Server package capture</title>
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
         td.dhcplabel { border:1px solid #000; vertical-align:top; overflow:hidden; padding:3px; background-color:#D8D8D8; }
         td.dhcpclass { border:1px solid #000; vertical-align:top; overflow:hidden; padding:3px; }
         th.dhcpclass { border:1px solid #000; vertical-align:top; overflow:hidden; background-color:#D8D8D8; text-align:left; padding:3px; }

         h3 {margin:0px; } 
     </style>
     </head>
    <BODY>
    <div id="frame">
        <div id="r1"><img src="RJ45_5.JPG"/></div>
        <div id="r2"><img src="RJ45_52.JPG"/></div>
        <div id="r3"><h2 style="padding-left:50px; margin:0px; ">DHCP Server package capture</h2></div>
    </div>
    
    <div id="pbody">
    
    <xsl:apply-templates select="dhcppacket"/>
    
    <br /><br /> <a href="http://www.dhcpserver.de">Visit the DHCP Server homepage</a>

    </div>

    </BODY>
    <head>
    <META HTTP-EQUIV="Pragma" CONTENT="no-cache" />
    <META HTTP-EQUIV="Expires" CONTENT="-1" />
    </head>
    </html>
  </xsl:template>    
    
    
  <xsl:template match="dhcppacket">

    <br/>
    <br/>

    <table class="dhcpclass">

     <colgroup>
       <col width="18%" />
       <col width="18%" />
       <col width="14%" />
       <col width="22%" />
       <col width="13%" />
       <col width="15%" />
     </colgroup>

     <tr>
     <td class="dhcpclass" colspan="6">
      <h3>
        <div style="float:left;">
        <xsl:choose>
           <xsl:when test="op=1">Boot request</xsl:when>
           <xsl:when test="op=2">Boot Reply</xsl:when>
           <xsl:otherwise>Unknown Request</xsl:otherwise>
        </xsl:choose>
        (<xsl:value-of select="op"/>) from/to <xsl:value-of select="partner_ipaddr"/></div>
       <div style="float:right;"><xsl:value-of select="timestamp_utc"/></div>
      </h3>
     </td>
     </tr>

     <tr>
       <td class="dhcplabel">Client MAC address:</td> 
       <td class="dhcpclass"><xsl:value-of select="chaddr"/></td>
       <td class="dhcplabel">HW addr len:</td> 
       <td class="dhcpclass"><xsl:value-of select="hlen"/></td>
       <td class="dhcplabel">HW type:</td> 
       <td class="dhcpclass">
        <xsl:choose>
           <xsl:when test="htype=1">Ethernet</xsl:when>
           <xsl:otherwise>Unknown</xsl:otherwise>
        </xsl:choose>
        (<xsl:value-of select="htype"/>)
       </td>
     </tr>

     <tr>
       <td class="dhcplabel">Your (client) IP address:</td> 
       <td class="dhcpclass"><xsl:value-of select="yiaddr"/></td>
       <td class="dhcplabel">Server host name:</td>
       <td class="dhcpclass"><xsl:value-of select="sname"/></td>
       <td class="dhcplabel">Hops:</td>
       <td class="dhcpclass"><xsl:value-of select="hops"/></td>
     </tr>

     <tr>
       <td class="dhcplabel">Next server IP address:</td> 
       <td class="dhcpclass"><xsl:value-of select="siaddr"/></td>
       <td class="dhcplabel">Boot file name:</td>
       <td class="dhcpclass"><xsl:value-of select="file"/></td>
       <td class="dhcplabel">Transaction ID:</td>
       <td class="dhcpclass">0x<xsl:value-of select="xid"/></td>
     </tr>

     <tr>
       <td class="dhcplabel">Relay agent IP address:</td> 
       <td class="dhcpclass"><xsl:value-of select="giaddr"/></td>
       <td class="dhcplabel">Magic cookie:</td>
       <td class="dhcpclass"><xsl:value-of select="magiccookie"/></td>
       <td class="dhcplabel">Seconds elapsed:</td>
       <td class="dhcpclass"><xsl:value-of select="secs"/></td>
     </tr>

     <tr>
       <td class="dhcplabel"></td> 
       <td class="dhcpclass"></td>
       <td class="dhcplabel">Padding:</td>
       <td class="dhcpclass"><xsl:value-of select="padding"/></td>
       <td class="dhcplabel">Bootp flags:</td>
       <td class="dhcpclass">
            0x<xsl:value-of select="flags"/>
           <xsl:choose>
              <xsl:when test="flags=0"> (unicast)</xsl:when>
              <xsl:otherwise> (broadcast)</xsl:otherwise>
           </xsl:choose>
       </td>
     </tr>

     <tr>
       <td class="dhcplabel"><b>Option tag</b></td> 
       <td class="dhcplabel"><b>Len</b></td>
       <td class="dhcplabel" colspan="4"><b>Data</b></td>
     </tr>

     <xsl:apply-templates select="options/option"/>

     </table>
  </xsl:template>



  <xsl:template match="options/option">

     <tr>
       <td class="dhcpclass"><xsl:value-of select="tag"/></td> 
       <td class="dhcpclass"><xsl:value-of select="len"/></td>

       <td class="dhcpclass" colspan="4">
         <xsl:choose>
           <xsl:when test="desc_text"><xsl:value-of select="desc_text"/>
              <xsl:if test="value_text">: <xsl:value-of select="value_text"/></xsl:if>
           </xsl:when>
           <xsl:otherwise><xsl:value-of select="hex"/></xsl:otherwise>
         </xsl:choose>
       </td>
     </tr>

  </xsl:template>

</xsl:stylesheet>
