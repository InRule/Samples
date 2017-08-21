<?xml version="1.0" encoding="iso-8859-1" ?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance">
	<xsl:param name="ruleAppName" />
	<xsl:param name="sectionName" />
	<xsl:param name="reportDateTime" />
	<xsl:param name="messages" />
	<xsl:param name="showReportHead" />
	<xsl:param name="showSectionHead" />
  <xsl:param name="showImages" />
	<xsl:param name="disableOutputEscaping" />
	<xsl:output method="xml" indent="yes" />
  <xsl:template match="/">
    <div>
    <style type="text/css">
      table.rulereportdetail tr th {
      font-weight: bold;
      background:#B0E0E6;
      }
      table.rulereportdetail tr td {
      font-size: 8pt;
      text-align: left;
      background: #fff;
      }
      h2.rulereport {
      margin: 3px;
      }
      h3.rulereport {
      margin: 3px;
      }
      div.rulereportsection {
      border: 1px solid #ddd;
      margin-bottom: 1em;
      }
      div.rulereportelement {
      border: 1px solid tan;
      margin-bottom: 1em;
      }
      div.def {
      border-bottom: 1px solid #ddd;
      }
      div.rulereportdetail {
      border-top: 1px solid #ddd;
      }
      table.rulereportnoborder
      {
      border: solid 0px;
      border-collapse: collapse;
      }
      table.rulereportnoborder td
      {
      border: solid 0px;
      }
    </style>
    <xsl:choose>
				<xsl:when test="$showReportHead='true'">
				<h2 class="rulereport">Rule Execution Report</h2>
				<table width="100%" class="rulereportnoborder" >
					<tr>
						<td>
							RuleApp Name:
							<xsl:value-of select="$ruleAppName" />
						</td>
						<td>
							Report Date:
							<xsl:value-of select="$reportDateTime" />
						</td>
					</tr>
            <tr>
              <td></td>
              <td>
                <xsl:value-of disable-output-escaping="yes" select="$messages" />
              </td>
            </tr>
         	</table>
				<br/>
			</xsl:when>
			</xsl:choose>
				
				<br/>
				<div class="rulereportsection">
          <xsl:choose>
            <xsl:when test="$showSectionHead='true'">
					  <h3 class="rulereport">
              <xsl:value-of select="$sectionName" /> 
            </h3>
            </xsl:when>
          </xsl:choose>
					<table class="rulereportdetail" cellpadding="3" cellspacing="2" width="100%">
						<col width="30%" />
						<col width="70%" />
						<tr>
							<th>Element</th>
							<th>Value</th>
						</tr>
						<!--Call LogItem Template -->
						<xsl:apply-templates select="/*/*">
						</xsl:apply-templates>
					</table>
				</div>
    </div>
	</xsl:template>
				
	<!--Element Template -->
	<xsl:template match="*">
		
		<xsl:choose>
			<xsl:when test="count(./*)&gt;0">
				<tr>
					<td colspan="2">
						<span class="colx">
							<xsl:value-of select="name(.)"/>
						</span>
					</td>
				</tr>
				<tr>
					<td colspan="2">
						<div class="rulereportelement">
						<table class="rulereportdetail" cellpadding="3" cellspacing="2" width="100%">
							<col width="30%" />
							<col width="70%" />
							<xsl:apply-templates select="./*">
							</xsl:apply-templates>
						</table>
						</div>
					</td>
				</tr>
			</xsl:when>
			<xsl:otherwise>
				<tr>
					<td>
						<span class="colx">
							<xsl:value-of select="name(.)" />
						</span>
					</td>
					<td>
						<span class="colx">
              <xsl:choose>
				<xsl:when test="$showImages&gt;0 and (contains(.,'.jpg') or contains(.,'.gif'))">
				  <img>
					<xsl:attribute name="src">
					  <xsl:value-of select="." />
					</xsl:attribute>
				  </img>
				</xsl:when>
				  <xsl:when test="$disableOutputEscaping&gt;0">
					  <xsl:value-of disable-output-escaping="yes" select="." />
				  </xsl:when>  
                <xsl:otherwise>
                  <xsl:value-of disable-output-escaping="no" select="." />
                </xsl:otherwise>
              </xsl:choose>
						</span>
					</td>
				</tr>
			</xsl:otherwise>								
		</xsl:choose>		
	</xsl:template>
</xsl:stylesheet>
