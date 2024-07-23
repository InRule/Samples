using System;
using InRule.Runtime.Engine.State;

namespace InRule.Runtime.Metrics.SqlServer
{
    internal struct MetricSchemaKey : IEquatable<MetricSchemaKey>
    {
        private readonly int _hashCode;
        private readonly string _ruleApplicationName;
        private readonly string _entityName;
        private readonly MetricSchema _metricSchema;

        public MetricSchemaKey(string ruleApplicationName, string entityName, MetricSchema metricSchema)
        {
            _ruleApplicationName = ruleApplicationName;
            _entityName = entityName;
            _metricSchema = metricSchema;
            unchecked
            {
                _hashCode = 397 ^ (_ruleApplicationName?.GetHashCode() ?? 0);
                _hashCode = (_hashCode * 397) ^ (_entityName?.GetHashCode() ?? 0);
                _hashCode = (_hashCode * 397) ^ (_metricSchema?.GetHashCode() ?? 0);
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            return obj is MetricSchemaKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public bool Equals(MetricSchemaKey other)
        {
            return string.Equals(_ruleApplicationName, other._ruleApplicationName) && 
                   string.Equals(_entityName, other._entityName) && 
                   Equals(_metricSchema, other._metricSchema);
        }
    }
}