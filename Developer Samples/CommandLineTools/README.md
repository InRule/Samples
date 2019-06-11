# InRule Sample Command Line Utilities

These projects are designed to demonstrate various tasks that administrators may want to automate, such as during a CI/CD pipeline process.



## Build IrJS Rule App

This sample allows the irDistribution service to be called from a command line to compile an irJS Rule Application into the javascript package.  It supports the Rule App being sourced either from a Catalog or the filesystem, and outputs the compiled JS to a file on the file system.

### Sample Execution commands:

    // File-based Rule App
    ./BuildIrJsRuleApp.exe -DistributionKey:00000000-0000-0000-0000-000000000000 -OutputPath:"C:\Working\MortgageCalculator.js" -RuleAppPath:"C:\Working\MortgageCalculator.ruleappx"
    ./BuildIrJsRuleApp.exe -DistributionKey:00000000-0000-0000-0000-000000000000 -OutputPath:".\MortgageCalculator.js" -RuleAppPath:".\MortgageCalculator.ruleappx"

    //Catalog-based Rule App
    ./BuildIrJsRuleApp.exe -DistributionKey:00000000-0000-0000-0000-000000000000 -OutputPath:"C:\Working\MortgageCalculator.js" -CatUri:https://ircatalog.azurewebsites.net/service.svc/core -CatUsername:username -CatPassword:password -CatRuleAppName:MortgageCalculator -CatLabel:LIVE



## Promote Rule App

This sample allows a Rule Application to be promoted from one Catalog instance to another from a command line.  It supports the Rule App being sourced either from a Catalog or the filesystem, and outputs the compiled JS to a file on the file system.

### Sample Execution command:

    ./PromoteRuleApp.exe -RuleAppName:"MortgageCalculator" -Label:LIVE -Comment:"Publish from command line tool" -SrcCatUri:"https://road-dgardiner-dev-envt1-ircatalog.azurewebsites.net/service.svc/core" -SrcCatUser:"admin" -SrcCatPass:"password" -DestCatUri:"http://127.0.0.1/InRuleCatalogService/Service.svc/core" -DestCatUser:"admin" -DestCatPass:"password"



## Solution Notes

### To build any project's .NET Core dll assembly into a .NET Framework executable, run the following command as a post-build step:

    dotnet publish -c Debug -r win10-x64