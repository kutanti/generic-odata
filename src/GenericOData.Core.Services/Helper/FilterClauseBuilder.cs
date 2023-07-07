using Microsoft.OData.UriParser;
using System.Globalization;
using System.Text;

namespace GenericOData.Core.Services.Helper
{
    /// <summary>
    /// Create query string based on $filter '==' equal with and clause.
    /// </summary>
    public class FilterClauseBuilder : QueryNodeVisitor<StringBuilder>
    {
        private const DateTimeStyles DATETIMESTYLES = DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal | DateTimeStyles.AllowWhiteSpaces;
        private StringBuilder _query;

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterClauseBuilder"/> class.
        /// </summary>
        /// <param name="query">query.</param>
        public FilterClauseBuilder(StringBuilder query)
        {
            _query = query;
        }

        /// <inheritdoc/>
        public override StringBuilder Visit(BinaryOperatorNode nodeIn)
        {
            var left = nodeIn.Left;
            if (left.Kind == QueryNodeKind.Convert)
            {
                left = (left as ConvertNode).Source;
            }
            var right = nodeIn.Right;
            if (right.Kind == QueryNodeKind.Convert)
            {
                right = (right as ConvertNode).Source;
            }

            switch (nodeIn.OperatorKind)
            {
                case BinaryOperatorKind.And:
                        var lb = new FilterClauseBuilder(_query);
                        var lq = left.Accept(lb);
                        //if (nodeIn.OperatorKind == BinaryOperatorKind.Or)
                        //{
                        //    lq = lq.Or();
                        //}
                        var rb = new FilterClauseBuilder(lq);
                        return right.Accept(rb);

                case BinaryOperatorKind.Equal:
                    string op = GetOperatorString(nodeIn.OperatorKind);
                    
                    if (right.Kind == QueryNodeKind.Constant)
                    {
                        var value = GetConstantValue(right);

                        if(value.GetType() == typeof(string))
                        {
                            value= Uri.EscapeDataString(value?.ToString()?? string.Empty);
                        }
                        if (left.Kind == QueryNodeKind.SingleValueFunctionCall)
                        {
                            var functionNode = left as SingleValueFunctionCallNode;
                            _query.Append($"{functionNode}{op}{value}");
                        }
                        else
                        {
                            string column = GetColumnName(left).ToLowerInvariant();
                            _query.Append($"{column}{op}{value}&");
                        }
                    }
                    break;

                default:
                    return _query;
            }

            return _query;
        }

        /// <inheritdoc/>
        public override StringBuilder Visit(SingleValueFunctionCallNode nodeIn)
        {
            if (nodeIn is null)
            {
                throw new ArgumentNullException(nameof(nodeIn));
            }

            return _query;
        }

        /// <inheritdoc/>
        public override StringBuilder Visit(UnaryOperatorNode nodeIn)
        {
            return _query;
        }

        private static StringBuilder ApplyFunction(StringBuilder query, SingleValueFunctionCallNode leftNode, string operand, object rightValue)
        {
            var columnName = GetColumnName(leftNode.Parameters.FirstOrDefault());
            switch (leftNode.Name.ToUpperInvariant())
            {
                case "YEAR":
                case "MONTH":
                case "DAY":
                case "HOUR":
                case "MINUTE":
                    query = query.Append($"{leftNode.Name}, {columnName}, {operand}, {rightValue}");
                    break;
                case "DATE":
                    query = query.Append(string.Concat(columnName, operand, rightValue is DateTime date ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture.DateTimeFormat) : rightValue));
                    break;
                case "TIME":
                    query = query.Append(string.Concat(columnName, operand, rightValue is DateTime time ? time.ToString("HH:mm", CultureInfo.InvariantCulture.DateTimeFormat) : rightValue));
                    break;
                default:
                    break;
            }

            return query;
        }

        private static bool ConvertToDateTimeUTC(string dateTimeString, out DateTime? dateTime)
        {
            if (DateTime.TryParse(dateTimeString, CultureInfo.InvariantCulture.DateTimeFormat, DATETIMESTYLES, out var dateTimeValue))
            {
                dateTime = dateTimeValue;
                return true;
            }
            else
            {
                dateTime = null;
                return false;
            }
        }

        private static string GetColumnName(QueryNode node)
        {
            var column = string.Empty;
            if (node.Kind == QueryNodeKind.Convert)
            {
                node = (node as ConvertNode).Source;
            }

            if (node.Kind == QueryNodeKind.SingleValuePropertyAccess)
            {
                column = (node as SingleValuePropertyAccessNode).Property.Name.Trim();
            }

            if (node.Kind == QueryNodeKind.SingleValueOpenPropertyAccess)
            {
                column = (node as SingleValueOpenPropertyAccessNode).Name.Trim();
            }

            return column;
        }

        private static object GetConstantValue(QueryNode node)
        {
            if (node.Kind == QueryNodeKind.Convert)
            {
                return GetConstantValue((node as ConvertNode).Source);
            }
            else if (node.Kind == QueryNodeKind.Constant)
            {
                var value = (node as ConstantNode).Value;
                if (value is string)
                {
                    var trimedValue = value.ToString().Trim();
                    if (ConvertToDateTimeUTC(trimedValue, out var dateTime))
                    {
                        return dateTime.Value;
                    }

                    return trimedValue;
                }

                return value;
            }
            else if (node.Kind == QueryNodeKind.CollectionConstant)
            {
                return (node as CollectionConstantNode).Collection.Select(cn => GetConstantValue(cn));
            }

            return null;
        }

        private static string GetOperatorString(BinaryOperatorKind operatorKind)
        {
            switch (operatorKind)
            {
                case BinaryOperatorKind.Equal:
                    return "=";

                case BinaryOperatorKind.NotEqual:
                    return "<>";

                case BinaryOperatorKind.GreaterThan:
                    return ">";

                case BinaryOperatorKind.GreaterThanOrEqual:
                    return ">=";

                case BinaryOperatorKind.LessThan:
                    return "<";

                case BinaryOperatorKind.LessThanOrEqual:
                    return "<=";

                case BinaryOperatorKind.Or:
                    return "or";

                case BinaryOperatorKind.And:
                    return "and";

                case BinaryOperatorKind.Add:
                    return "+";

                case BinaryOperatorKind.Subtract:
                    return "-";

                case BinaryOperatorKind.Multiply:
                    return "*";

                case BinaryOperatorKind.Divide:
                    return "/";

                case BinaryOperatorKind.Modulo:
                    return "%";

                default:
                    return string.Empty;
            }
        }
    }
}
