InRule® Metric logger for the InRule Rule Engine that can log to Microsoft SQL Server.

**Configuring via code:**
```c#
using (var session = new RuleSession(ruleApplicationDef))
{
    session.Settings.MetricLogger = new MetricLogger(DatabaseConnectionString);
    session.Settings.MetricServiceName = "Development"; //Optional name
    
    session.CreateEntity(e1Def.Name);
    session.ApplyRules();
}
```

**Configuring via custom config section:**
```xml
<configuration>

  <configSections>
    <section name="inrule.runtime" type="InRule.Runtime.Configuration.RuntimeConfigSectionHandler, InRule.Runtime" />
  </configSections>
  
  <!-- Other sections ommitted for brevity -->
  
  <appSettings>
    <!-- Connection String to Metrics storage location -->
    <add key="inrule:runtime:metrics:sqlServer:connectionString" value="" />  
  </appSettings>  
    
  <inrule.runtime>
    <ruleEngine 
                metricLoggerAssemblyName="InRule.Runtime.Metrics.SqlServer" 
                metricLoggerTypeName="InRule.Runtime.Metrics.SqlServer.MetricLogger" 
                metricLoggerServiceName="Development"/> <!-- Optional Name -->
  </inrule.runtime>

  <runtime>
    <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
      <dependentAssembly>
        <assemblyIdentity name="Newtonsoft.Json" publicKeyToken="30ad4fe6b2a6aeed" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-12.0.0.0" newVersion="12.0.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.SqlServer.Smo" publicKeyToken="89845dcd8080cc91" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-14.100.0.0" newVersion="14.100.0.0" />
      </dependentAssembly>
      <dependentAssembly>
        <assemblyIdentity name="Microsoft.SqlServer.ConnectionInfo" publicKeyToken="89845dcd8080cc91" culture="neutral" />
        <bindingRedirect oldVersion="0.0.0.0-14.100.0.0" newVersion="14.100.0.0" />
      </dependentAssembly>
    </assemblyBinding>
  </runtime>

</configuration>
```

**Configuring via appSettings:**
```xml
<configuration>
  
  <!-- Other sections ommitted for brevity -->
  
  <appSettings>
    <add key="inrule:runtime:ruleEngine:metricLoggerAssemblyName" value="InRule.Runtime.Metrics.SqlServer" />    
    <add key="inrule:runtime:ruleEngine:metricLoggerTypeName" value="InRule.Runtime.Metrics.SqlServer.MetricLogger" />
    <!-- Optional Name -->
    <add key="inrule:runtime:ruleEngine:metricLoggerServiceName" value="Development" />
    <!-- Connection String to Metrics storage location -->
    <add key="inrule:runtime:metrics:sqlServer:connectionString" value="" />  
  </appSettings>
</configuration>
```
