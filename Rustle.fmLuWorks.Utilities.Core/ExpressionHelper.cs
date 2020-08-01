using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Rustle.fmLuWorks.Utilities.Core
{
    public static class ExpressionHelper
    {
        public static T GetPropertyValue<T>(this Expression<Func<T>> expression)
        {
            return expression.Compile().Invoke();
        }

        public static void SetPropertyValue<T>(this Expression<Func<T>> expression, T value)
        {
            if (!(expression.Body is MemberExpression memberExpression))
            {
                throw new ArgumentException(nameof(memberExpression));
            }

            if (!(memberExpression.Member is PropertyInfo propertyInfo))
            {
                throw new ArgumentException(nameof(propertyInfo));
            }

            var target = Expression.Lambda(memberExpression.Expression)
                .Compile()
                .DynamicInvoke();

            propertyInfo.SetValue(target, value);
        }

        public static Expression<T> CreateExpression<T>(Expression<T> expression)
        {
            return expression;
        }
    }
}
