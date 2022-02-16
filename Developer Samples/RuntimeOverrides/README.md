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

The 2 samples illustrate how the REST operation may be overridden by both irSDK and .config `<appSettings/>`:
- [RuntimeOverridesViaSdk](RuntimeOverridesViaSdk/)
- [RuntimeOverridesViaConfig](RuntimeOverridesViaConfig/)