﻿using System.Linq.Expressions;

namespace Light.DependencyInjection.TypeResolving
{
    public interface IOptimizeLifetimeExpression
    {
        Expression Optimize(Expression createInstanceExpression, ResolveExpressionContext context);
    }
}