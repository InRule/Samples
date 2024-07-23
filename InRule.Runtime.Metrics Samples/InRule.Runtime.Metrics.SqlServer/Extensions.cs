using InRule.Runtime.Engine.State;

namespace InRule.Runtime.Metrics.SqlServer
{
    public static class Extensions
    {
        public static string GetMetricColumnName(this MetricProperty property)
        {
            return $"{property.Name}_{property.DataType}";
        }
    }
}