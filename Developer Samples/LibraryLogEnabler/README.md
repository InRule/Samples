# InRule Sample LibraryLog Enabler

InRule ships with LibraryLogger included in the IRServer REST Rule Execution Service and irCatalog Repository Service, but with no target logging frameworks activated.  This sample walks through a way to initialize a logging framework during the WCF pipeline startup to give LibLogger a log sink to write out to.

There are a number of updates that need to be made in the app.config file to enable this behavior.  These samples add log4net logging, but LibraryLogger also supports NLog, Serilog, and Loupe.  In addition to the configuration changes, the application will also need to have deployed the custom assembly referenced in the custom WebServer module and the assemblies for whatever logging framework is referenced.

#### Add custom ConfigSections
```HTML
<configSections>
  <section name="log4net" type="log4net.Config.Log4NetConfigurationSectionHandler,log4net" />
  <section name="inrule.logging" type="InRule.Repository.Logging.Configuration.LoggingSectionHandler, InRule.Repository"/>
</configSections>
```

#### Configure InRule.Logging
``` HTML
<inrule.logging>
  <group typeName="InRule.Repository.Logging.Loggers.LoggerGroup, InRule.Repository" level="Debug">
    <logger typeName="InRule.Repository.Logging.Loggers.LibraryLogger, InRule.Repository"/>
  </group>
</inrule.logging>
```

#### Add Custom WebServer Module
``` HTML
<system.webServer>
  <modules>
    <add name="LoggingEnabler" type="LibLogEnabler.LoggingEnabler, LibLogEnabler"/>
  </modules>
</system.webServer>
```

#### Configure your logging framework
This configuration also includes a custom "TraceAppender" log sink that logs to System.Diagnostics.Trace.  This can be useful for Azure App Services, allowing logs to be visible in the Console in the Azure Portal.
``` HTML
<log4net>
  <appender name="RollingFileAppender" type="log4net.Appender.RollingFileAppender">
    <file value="log.txt" />
    <appendToFile value="true" />
    <rollingStyle value="Size" />
    <maxSizeRollBackups value="5" />
    <maximumFileSize value="10MB" />
    <staticLogFileName value="true" />
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date{dd MMM yyyy HH:mm:ss} (%level) %logger %message%newline" />
    </layout>
  </appender>
  <appender name="TraceAppender" type="LibLogEnabler.TraceAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%date{dd MMM yyyy HH:mm:ss} (%level) %logger %message%newline" />
    </layout>
  </appender>
  <appender name="ConsoleAppender" type="log4net.Appender.ConsoleAppender">
    <layout type="log4net.Layout.PatternLayout">
      <conversionPattern value="%message%newline" />
    </layout>
  </appender>
  <root>
    <level value="Warn" />
    <appender-ref ref="RollingFileAppender" />
    <appender-ref ref="TraceAppender" />
    <appender-ref ref="ConsoleAppender" />
  </root>
</log4net>
```