# InRule Samples

## Build IrJS Rule APp

This sample allows the irDistribution service to be called from a command line to compile an irJS Rule Application into the javascript package.  It supports the Rule App being sourced either from a Catalog or the filesystem, and outputs the compiled JS to a file on the file system.  An example use case for this application would be as part of a CI/CD pipeline, to automate the process of compiling the irJS Rule Application.

##Sample Execution commands:
    // File-based Rule App
    ./BuildIrJsRuleApp.exe -DistributionKey:00000000-0000-0000-0000-000000000000 -OutputPath:"C:\Working\MortgageCalculator.js" -RuleAppPath:"C:\Working\MortgageCalculator.ruleappx"
    ./BuildIrJsRuleApp.exe -DistributionKey:00000000-0000-0000-0000-000000000000 -OutputPath:".\MortgageCalculator.js" -RuleAppPath:".\MortgageCalculator.ruleappx"

    //Catalog-based Rule App
    ./BuildIrJsRuleApp.exe -DistributionKey:00000000-0000-0000-0000-000000000000 -OutputPath:"C:\Working\MortgageCalculator.js" -CatUri:https://ircatalog.azurewebsites.net/service.svc -CatUsername:username -CatPassword:password -CatRuleAppName:MortgageCalculator -CatLabel:LIVE

