﻿using Parser;
using Sprache;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace ODataParser
{
    public class WhereClauseEx<T>
    {
        public WhereClauseEx()
        {
            this.Property = (from name in Parse.Letter.AtLeastOnce().Text()
                             select Expression.Property(this.predicateInputParamater, name)).Named(nameof(Property));

            this.Function = (from name in Parse.Letter.AtLeastOnce().Text()
                             from lparen in Parse.Char('(')
                             from expr in Parse.Ref(() => this.BooleanTerm).DelimitedBy(Parse.Char(',').Token())
                             from rparen in Parse.Char(')')
                             select CallFunction(name, expr.ToArray())).Named(nameof(Function));

            this.Constant = ScalarValues.Number.Named(nameof(Constant));
            //this.Constant = ScalarValues.Number.XOr(ScalarValues.StringConstant).Named(nameof(Constant));
            //old//this.Constant = Parse.Decimal.Select(x => Expression.Constant(int.Parse(x))).Named(nameof(Constant));

            this.Factor = (from lparen in Parse.Char('(')
                           from expr in Parse.Ref(() => this.BooleanTerm)
                           from rparen in Parse.Char(')')
                           select expr).Named("expression").XOr(this.Constant).XOr(this.Function.Or(this.Property));

            this.Operand = ((from sign in Parse.Char('-')
                             from factor in Factor
                             select Expression.Negate(factor)).XOr(Factor)).Token();

            // terms by priority:
            this.MutiplicativeTerm = Parse.ChainOperator(Operators.MultiplicativeArithmeticOperators, this.Operand, Expression.MakeBinary);
            this.AdditiveTerm = Parse.ChainOperator(Operators.AditiveArithmeticOperators, this.MutiplicativeTerm, Expression.MakeBinary);
            this.ComparativeTerm = Parse.ChainOperator(Operators.ComparisionOperators, this.AdditiveTerm, Expression.MakeBinary);
            this.BooleanTerm = Parse.ChainOperator(Operators.BinaryBoolean, this.ComparativeTerm, Expression.MakeBinary);
        }

        private static Expression CallFunction(string name, Expression[] parameters)
        {
            if (name == "startswith")
                return Expression.Call(typeof(string).GetMethod(nameof(string.StartsWith), parameters.Select(e => e.Type).ToArray()));
            else if (name == "endswith")
                return Expression.Call(typeof(string).GetMethod(nameof(string.EndsWith), parameters.Select(e => e.Type).ToArray()));
            throw new ParseException(string.Format("Function '{0}({1})' does not exist.", name, string.Join(",", parameters.Select(e => e.Type.Name))));
        }

        public Parser<Expression> MutiplicativeTerm { get; private set; }
        public Parser<Expression> AdditiveTerm { get; private set; }
        public Parser<Expression> Operand { get; private set; }
        public Parser<Expression> Factor { get; private set; }
        public Parser<ConstantExpression> Constant { get; private set; }
        public Parser<Expression> Function { get; private set; }
        public Parser<Expression> ComparativeTerm { get; private set; }
        public Parser<Expression> BooleanTerm { get; private set; }
        public Parser<Expression> Property { get; private set; }

        /// <summary>
        ///  a where clause is a delegate like: (T t) => t.property == 1.
        ///  <see cref="predicateInputParamater"/> is the placeholder of the instance of T which is matched by the
        ///  buildt predicate.
        /// </summary>
        private readonly ParameterExpression predicateInputParamater = Expression.Parameter(typeof(T));

        public Expression<Func<T, bool>> Of(string whereClause)
        {
            return Expression.Lambda<Func<T, bool>>(
                body: this.BooleanTerm.Parse(whereClause),
                parameters: new[] { this.predicateInputParamater }
            );
        }

        public Expression<Func<T, outT>> GetProperty<outT>(string whereClause)
        {
            return Expression.Lambda<Func<T, outT>>(
                body: this.BooleanTerm.Parse(whereClause),
                parameters: new[] { this.predicateInputParamater }
            );
        }
    }
}