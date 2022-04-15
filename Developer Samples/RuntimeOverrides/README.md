# Runtime Overrides

Runtime overrides may override End Point and Data Element settings that differ from what was originally authored in a Rule Application.
This can be useful when deploying an application to different environments.

Overrides may be specified either using irSDK from the `RuleSession.Overrides` property, or via .config file `<appSettings/>`.

The following End Points may be overridden:
- Database Connection
- Rest Service
- Web Service
- SendMail Service
- XML Schema

The following Data Elements may be overridden:
- Inline Value List
- Inline Table
- Inline XML Document
- SQL Query
- XPath Query
- REST Operation

---
## irSDK Override Mechanism
When creating a RuleSession for executing rules, overrides may be specified prior to execution.

For example:

```
using (RuleSession session = new RuleSession(ruleApp))
{
    session.Overrides.OverrideRestOperationUriTemplate("operation1", "api/v2/invoice");
    session.CreateEntity("Invoice");
    session.ApplyRules();
}
```

---
## Configuration Override Mechanism
Using the `<appSettings/>` in the application's .config file, overrides may be applied declaratively instead of adding irSDK code.

These `<appSettings/>` may be extended using [Configuration Builders](https://github.com/aspnet/MicrosoftConfigurationBuilders) to read configuration from external sources such as Environment Variables, JSON files, or Azure KeyVault.

The `<appSettings/>` follow a key/value pattern. The key is the identifier of the override, and the value is the new value for the Runtime to use.

For example:

```
<configuration>
    <appSettings>
        <add key="inrule:runtime:overrides:operation1:RestOperation:uritemplate" value="api/v2/test" />
    </appSettings>
</configuration>
```

This illustrates the equivalent of the irSDK override example listed above.

### Key Format
The appSetting key follows the following format:

`inrule:runtime:overrides:<name>:<type>:<property>`

- `<name>` is the name of the End Point or Data Element
- `<type>` is the type of End Point or Data Element (see list below)
- `<property>` is the property of the type being overridden (see list below)

**Note: These overrides will apply to End Points and Data Elements with the same name across different Rule Applications.**

The `<name>` component is **case-sensitive**.

The `<type>` and `<property>` components are **not case-sensitive**. The combination of type/property may be selected from the following:

### End Points
| Type | Property |
| ---- | -------- |
| DatabaseConnection | ConnectionString |
| SendMailServer | ServerAddress |
| XmlDocument | XmlPath |
| XmlSchema | XsdPath |
| | EnableXsdValidation |
| WebService | WsdlUrl |
| | ServiceUriOverride |
| | MaxReceivedMessageSize |
| RestService | RootUrl |
| | AllowUntrustedCertificates |
| | AuthenticationType |
| | Username |
| | Password |
| | Domain |
| | X509CertificatePath |
| | X509CertificatePassword |

### Data Elements

| Name | Property |
| ---- | -------- |
| ValueList | ValueListItems* |
| Table | TableSettings* |
| XPathDocument | InlineXml |
| SqlQuery | Query |
| RestOperation | UriTemplate |
| | Body |
| | Headers* |

_*see special handling below_

**Note: All appSetting values must be HTML-safe, so any values containing characters such as '&', '<', '>', '\\"' should first be passed through an HTML encoder.**

For example:

```
const string unencodedValue = @"<InlineXml><Value1>This is a \"test\" & should be encoded</Value1></InlineXml>";

Console.WriteLine("Unencoded Value: " + unencodedValue);
Console.WriteLine("Encoded Value: " + System.Net.WebUtility.HtmlEncode(unencodedValue));
```

The above should output the following:

```
Unencoded Value: <InlineXml><Value1>This is a "test" & should be encoded</Value1></InlineXml>
Encoded Value: &lt;InlineXml&gt;&lt;Value1&gt;This is a &quot;test&quot; &amp; should be encoded&lt;/Value1&gt;&lt;/InlineXml&gt;
```

#### ValueListItems Serialization
The value for ValueListItems uses a custom XML format, which must be HTML encoded for use as the value.

For example:

```
const string valueListItems =
@"<ValueListItems>
    <ValueListItem>
        <Value>Value1</Value>
        <DisplayText>Value One</DisplayText>
    </ValueListItem>
    <ValueListItem>
        <Value>Value2</Value>
    </ValueListItem>
    <ValueListItem>
        <Value>Value3</Value>
    </ValueListItem>
</ValueListItems>";

var value = System.Net.WebUtility.HtmlEncode(valueListItems);
```

#### TableSettings Serialization
The value of TableSettings configuration must use irSDK to XML Serialize an instance of `InRule.Repository.TableSettings`. This must then be HTML encoded for use as the value.

For example:

```
var tableSettings = ruleAppDef.DataElements["table1"].TableSettings;
var xs = new XmlSerializer(typeof("InRule.Repository.TableSettings));
var sb = new StringBuilder();
using (var writer = new StringWriter(sb))
{
    xs.Serialize(writer, tableSettings);
}

var value = System.Net.WebUtility.HtmlEncode(sb.ToString());
```

#### REST Operation Headers
The Headers are a collection of name/value pairs. Any number of headers may be applied. To simulate a collection, the appSetting key is extended to include the header name.

For example:
```
<configuration>
    <appSettings>
        <add key="inrule:runtime:overrides:operation1:RestOperation:Headers:Header1" value="value1" />
        <add key="inrule:runtime:overrides:operation1:RestOperation:Headers:Header2" value="value2" />
        <add key="inrule:runtime:overrides:operation1:RestOperation:Headers:Header3" value="value3" />
        <add key="inrule:runtime:overrides:operation1:RestOperation:Headers:Connection" value="keep-alive" />
        <add key="inrule:runtime:overrides:operation1:RestOperation:Headers:Accept-Language" value="en-US" />
    </appSettings>
</configuration>
```

---
## Samples
The 2 samples illustrate how the REST operation may be overridden by both irSDK and .config `<appSettings/>`:
- [RuntimeOverridesViaSdk](RuntimeOverridesViaSdk/)
- [RuntimeOverridesViaConfig](RuntimeOverridesViaConfig/)
