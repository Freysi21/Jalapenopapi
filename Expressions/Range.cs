using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace BaseRestAPI.Expressions
{
    public static class Range
    {

        public static Expression<Func<T, bool>> RangeExpression<T>(Dictionary<string, string[]> dateDict)
        {
            Expression and = null;
            ParameterExpression parameterExpression = Expression.Parameter(typeof(T), "o");
            foreach (var entry in dateDict)
            {
                if (entry.Value.Count() == 2)
                {
                    var from2 = Convert.ToDateTime(entry.Value[0]);
                    var to2 = Convert.ToDateTime(entry.Value[1]);

                    MemberExpression memberExpression1 = Expression.PropertyOrField(parameterExpression, entry.Key);
                    MemberExpression memberExpression2 = Expression.PropertyOrField(parameterExpression, entry.Key);

                    ConstantExpression valueExpression1 = Expression.Constant(to2, typeof(DateTime));
                    ConstantExpression valueExpression2 = Expression.Constant(from2, typeof(DateTime));

                    BinaryExpression binaryExpression1 = Expression.GreaterThanOrEqual(memberExpression1, valueExpression1);
                    BinaryExpression binaryExpression2 = Expression.LessThanOrEqual(memberExpression2, valueExpression2);

                    and = Expression.AndAlso(binaryExpression1, binaryExpression2);
                }
            }
            if(and == null){
                return null;
            }
            return Expression.Lambda<Func<T, bool>>(and, parameterExpression);
        }
    }
}
