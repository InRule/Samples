using InRule.Repository;
using InRule.Repository.RuleElements;

namespace InRule.Runtime.Metrics.SqlServer.IntegrationTests
{
    public static class RepositoryDefExtensions
    {
        public static EntityDef AddEntity(this RuleApplicationDef ruleApplicationDef, string entityName)
        {
            var entityDef = new EntityDef();
            entityDef.Name = entityName;
            ruleApplicationDef.Entities.Add(entityDef);
            return entityDef;
        }

        public static FieldDef AddField(this IContainsFields entityDef, string name, DataType dataType, string defaultValue)
        {
            var field = AddField(entityDef, name, dataType);
            field.DefaultValue = defaultValue;
            return field;
        }

        public static FieldDef AddField(this IContainsFields entityDef, string name, DataType dataType)
        {
            return entityDef.Fields.AddField(name, dataType);
        }

        public static FieldDef AddField(this FieldDefCollection fields, string name, DataType dataType)
        {
            var fieldDef = new FieldDef(name, dataType);
            fields.Add(fieldDef);
            return fieldDef;
        }

        

        public static FieldDef AddCalcField(this IContainsFields container, string name, DataType dataType, string expression)
        {
            var fieldDef = container.AddField(name, dataType);
			fieldDef.IsCalculated = true;
            fieldDef.Calc.FormulaText = expression;
            return fieldDef;
        }

    
        public static FieldDef AddEntityCollection(this IContainsFields fields, string name, string memberEntityName)
        {
            var fieldDef = fields.AddField(name, DataType.Entity);
            fieldDef.IsCollection = true;
            fieldDef.DataTypeEntityName = memberEntityName;
            return fieldDef;
        }

		public static FieldDef AddComplexField(this IContainsFields fields, string name)
		{
			var fieldDef = fields.AddField(name, DataType.Complex);
			return fieldDef;
		}

		public static FieldDef AddComplexCollection(this IContainsFields fields, string name)
		{
			var fieldDef = fields.AddField(name, DataType.Complex);
			fieldDef.IsCollection = true;
			return fieldDef;
		}

		public static AddCollectionMemberActionDef AddAddCollectionMemberAction(this SimpleRuleDef simpleRuleDef,
            string name, string collectionName, params NameExpressionPairDef[] memberValues)
        {
            return simpleRuleDef.SubRules.AddAddCollectionMemberAction(name, collectionName, memberValues);
        }

        public static AddCollectionMemberActionDef AddAddCollectionMemberAction(
            this RuleElementDefCollection ruleDefContainer, string name, string collectionName,
            params NameExpressionPairDef[] memberValues)
        {
            var action = new AddCollectionMemberActionDef(collectionName);
            action.Name = name;
            if (memberValues != null)
            {
                action.MemberValues = new NameExpressionPairDefCollection();
                action.MemberValues.AddRange(memberValues);
            }

            ruleDefContainer.Add(action);
            return action;
        }

        public static AddCollectionMemberActionDef AddAddCollectionMemberAction(
            this RuleElementDefCollection ruleDefContainer, string actionName, string collectionName)
        {
            var actionDef = new AddCollectionMemberActionDef();
            actionDef.Name = actionName;
            actionDef.CollectionName = collectionName;
            ruleDefContainer.Add(actionDef);
            return actionDef;
        }

      
        public static RuleSetDef AddAutoSeqRuleSet(this EntityDef entityDef, string name)
        {
            var ruleSetDef = new RuleSetDef(name);
            ruleSetDef.FireMode = RuleSetFireMode.Auto;
            ruleSetDef.RunMode = RuleSetRunMode.Sequential;
            entityDef.RuleElements.Add(ruleSetDef);
            return ruleSetDef;
        }

        public static SimpleRuleDef AddSimpleRule(this RuleElementDefCollection ruleDefContainer, string name,
            string condition)
        {
            var simpleRule = new SimpleRuleDef(name, condition);
            ruleDefContainer.Add(simpleRule);
            return simpleRule;
        }

        public static SimpleRuleDef AddSimpleRule(this RuleSetDef ruleDefContainer, string name, string condition)
        {
            return ruleDefContainer.Rules.AddSimpleRule(name, condition);
        }
    }
}