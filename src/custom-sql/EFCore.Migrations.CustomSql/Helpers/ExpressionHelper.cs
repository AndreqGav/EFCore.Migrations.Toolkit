using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace EFCore.Migrations.CustomSql.Helpers;

static internal class ExpressionHelper
{
    /// <summary>
    /// Получить наименование свойства из выражения <see cref="Expression"/>
    /// </summary>
    public static string GetMemberName(Expression expression)
    {
        var memberExpression = GetMemberExpression(expression);

        return memberExpression?.Member.Name;
    }

    /// <summary>
    /// Получить <see cref="MemberExpression"/> из выражения <see cref="Expression"/>
    /// </summary>
    public static MemberExpression GetMemberExpression(Expression expression)
    {
        switch (expression)
        {
            case MemberExpression memberExpression:
                return memberExpression;
            case LambdaExpression lambdaExpression:
                switch (lambdaExpression.Body)
                {
                    case MemberExpression body:
                        return body;
                    case UnaryExpression unaryExpression:
                        return (MemberExpression)unaryExpression.Operand;
                }

                break;
        }

        return null;
    }

    /// <summary>
    /// Получить список <see cref="MemberInfo"/> для вложенного обращение к полю <paramref name="expression"/>
    /// https://stackoverflow.com/questions/29084894/how-to-use-an-expressionfunc-to-set-a-nested-property
    /// </summary>
    public static List<MemberInfo> GetDeepMembers<TObject, T>(Expression<Func<TObject, T>> expression)
    {
        var members = new List<MemberInfo>();

        Expression exp = expression;

        // Выражение вида: _ => _
        if (exp is LambdaExpression { Body: ParameterExpression })
        {
            return members;
        }

        while (exp != null)
        {
            var mi = GetMemberExpression(exp);

            if (mi != null)
            {
                members.Add(mi.Member);
                exp = mi.Expression;
            }
            else
            {
                if (exp is not ParameterExpression)
                {
                    throw new NotSupportedException("Выражение для получения MemberExpression не распознано");
                }

                break;
            }
        }

        if (members.Count == 0)
        {
            throw new NotSupportedException("Некорректно распознано выражение");
        }

        members.Reverse();

        return members;
    }

    /// <summary>
    /// Получить значение из цепочки <paramref name="members"/> объекта <paramref name="obj"/>
    /// </summary>
    public static object GetDeepValue(object obj, ICollection<MemberInfo> members)
    {
        var targetObject = obj;
        foreach (var memberInfo in members)
        {
            var pi = memberInfo as PropertyInfo;

            if (targetObject is null)
                break;

            if (pi != null)
            {
                targetObject = pi.GetValue(targetObject);
            }
            else
            {
                var fi = (FieldInfo)memberInfo;
                targetObject = fi.GetValue(targetObject);
            }
        }

        return targetObject;
    }

    /// <summary>
    /// Присвоить значение <paramref name="value"/> цепочке <paramref name="members"/> объекта <paramref name="obj"/>
    /// </summary>
    public static void SetDeepValue(object obj, ICollection<MemberInfo> members, object value)
    {
        var targetObject = obj;
        foreach (var memberInfo in members.Take(members.Count - 1))
        {
            var pi = memberInfo as PropertyInfo;

            if (pi != null)
            {
                targetObject = pi.GetValue(targetObject);
            }
            else
            {
                var fi = (FieldInfo)memberInfo;
                targetObject = fi.GetValue(targetObject);
            }
        }

        {
            var pi = members.Last() as PropertyInfo;

            if (pi != null)
            {
                pi.SetValue(targetObject, value);
            }
            else
            {
                var fi = (FieldInfo)members.Last();
                fi.SetValue(targetObject, value);
            }
        }
    }
}