Known Issues
====

# Mix Version Use of irSDK/irAuthor/irCatalog

The majority of Rule Application authoring scenarios should inter-operate between versions 5.3.1-5.4.3 and 5.5.0 of irSDK/irAuthor/irCatalog. However, the following edge cases exist where unexpected behavior may occur:

* Using v5.5.0 irAuthor/irSDK to check-in a Rule Application to a v5.5.0 irCatalog with duplicate RuleSet names under different Entities.

  * Validation with v5.3.1 irAuthor/irSDK will fail with duplicate RuleSet error.

* Using v5.5.0 irAuthor/irSDK to check-in a Rule Application to a v5.3.1 irCatalog with duplicate RuleSet names under different Entities. 

  * Check-in will fail with duplicate RuleSet validation error.

* Using v5.5.0 irAuthor/irSDK to check-in a Rule Application to a v5.5.0 irCatalog with a Decision Input/Output referencing Entity 'Entity1'.

  * irAuthor/irSDK v5.3.1 checks-out the Rule Application, renames 'Entity1' to 'Entity2', and attempts to check-in the Rule Application. 

  * Check-in will fail with error like "Unable to resolve Entity1 to an entity [Decision1.Input1]".

* Using v5.5.0 irAuthor/irSDK to check-in a Rule Application to a v5.3.1 irCatalog with a Decision Input/Output referencing Entity 'Entity1'.

  * irAuthor/irSDK v5.3.1 checks-out the Rule Application, renames 'Entity1' to 'Entity2', and checks-in the Rule Application successfully.

  * irAuthor/irSDK v5.5.0 gets latest Rule Application from irCatalog successfully, but irAuthor/irSDK validation fails with error like "Unable to resolve Entity1 to an entity [Decision1.Input1]".

* Using v5.5.0 irAuthor/irSDK to check-in a Rule Application to a v5.5.0 irCatalog with a Decision Input/Output referencing Entity 'Entity1' and a Decision rule referencing 'Field1' on that Entity.

  * irAuthor/irSDK v5.3.1 checks-out the Rule Application, renames 'Field1' to 'Field2', and attempts to check-in the Rule Application.

  * Check-in will fail with error like "Unable to resolve 'Input1.Field1' in context 'Decision1.DecisionStart.SetValue1' [--> Input1.Field1 <--] [Decision1.DecisionStart.SetValue1]"

* Using v5.5.0 irAuthor/irSDK to check-in a Rule Application to a v5.3.1 irCatalog with a Decision Input/Output referencing Entity 'Entity1' and a Decision rule referencing 'Field1' on that Entity.

  * irAuthor/irSDK v5.3.1 checks-out the Rule Application, renames 'Field1' to 'Field2', and checks-in the Rule Application.

  * irAuthor/irSDK v5.5.0 gets latest Rule Application from irCatalog successfully, but irAuthor/irSDK validation fails with error like "Unable to resolve 'Input1.Field1' in context 'Decision1.DecisionStart.SetValue1' [--> Input1.Field1 <--] [Decision1.DecisionStart.SetValue1]".
