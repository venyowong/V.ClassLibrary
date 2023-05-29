using System;
using System.Collections.Generic;
using System.Linq;

namespace V.QueryParser
{
    /// <summary>
    /// 查询表达式解析器
    /// <para>key1 > value1 && key2 == value2 || key3 < value3</para>
    /// <para>关键词与符号之间的空格是严格要求的</para>
    /// </summary>
    public class QueryExpression
    {
        /// <summary>
        /// 当 Type 为 Compound 时，有值
        /// </summary>
        public List<QueryExpression> Queries { get; private set; } = new List<QueryExpression>();

        /// <summary>
        /// 当 Type 为 Compound 时，有值
        /// <para>长度比 Queries 少 1，Symbols[i] 左边表达式为 Queries[i] 右边为 Queries[i+1]</para>
        /// </summary>
        public List<Symbol> Symbols { get; private set; } = new List<Symbol>();

        /// <summary>
        /// 当前表达式的查询类型
        /// </summary>
        public QueryType Type { get; private set; } = QueryType.All;

        public string Key { get; private set; }

        public Symbol Ope { get; private set; }

        public string Value { get; private set; }

        public QueryExpression(string key, Symbol ope, string value)
        {
            this.Type = QueryType.Base;
            this.Key = key;
            this.Ope = ope;
            this.Value = value;
        }

        public QueryExpression(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition))
            {
                return;
            }

            var key = string.Empty;
            var ope = Symbol.Invalid;
            var value = string.Empty;
            var exp = string.Empty;
            var expType = ExpType.Init;

            #region parse
            for (int i = 0; i < condition.Length; i++)
            {
                switch (expType)
                {
                    case ExpType.Init:
                        switch (condition[i])
                        {
                            case '(':
                                expType = ExpType.Parenthesis;
                                exp = string.Empty;
                                continue;
                            case ' ':
                                continue;
                            default:
                                expType = ExpType.Key;
                                exp = condition[i].ToString();
                                continue;
                        }
                    case ExpType.Parenthesis:
                        switch (condition[i])
                        {
                            case ')':
                                expType = ExpType.Exp;
                                this.Queries.Add(new QueryExpression(exp));
                                exp = string.Empty;
                                continue;
                            default:
                                exp = exp + condition[i];
                                continue;
                        }
                    case ExpType.Key:
                        switch (condition[i])
                        {
                            case ' ':
                                expType = ExpType.Operator;
                                key = exp;
                                exp = string.Empty;
                                continue;
                            default:
                                exp = exp + condition[i];
                                continue;
                        }
                    case ExpType.Exp:
                        switch (condition[i])
                        {
                            case ' ':
                                continue;
                            case '&':
                            case '|':
                                if (!string.IsNullOrWhiteSpace(key))
                                {
                                    this.Queries.Add(new QueryExpression(key, ope, value));
                                    key = string.Empty;
                                    ope = Symbol.Invalid;
                                    value = string.Empty;
                                }
                                expType = ExpType.Operator;
                                exp = condition[i].ToString();
                                continue;
                            case '(':
                                expType = ExpType.Parenthesis;
                                exp = string.Empty;
                                continue;
                            default:
                                expType = ExpType.Key;
                                exp = condition[i].ToString();
                                continue;
                        }
                    case ExpType.Operator:
                        switch (condition[i])
                        {
                            case ' ':
                                switch (exp.ToLower())
                                {
                                    case ">":
                                        expType = ExpType.Value;
                                        exp = string.Empty;
                                        ope = Symbol.Gt;
                                        continue;
                                    case ">=":
                                        expType = ExpType.Value;
                                        exp = string.Empty;
                                        ope = Symbol.Gte;
                                        continue;
                                    case "<":
                                        expType = ExpType.Value;
                                        exp = string.Empty;
                                        ope = Symbol.Lt;
                                        continue;
                                    case "<=":
                                        expType = ExpType.Value;
                                        exp = string.Empty;
                                        ope = Symbol.Lte;
                                        continue;
                                    case "==":
                                        expType = ExpType.Value;
                                        exp = string.Empty;
                                        ope = Symbol.Eq;
                                        continue;
                                    case "!=":
                                        expType = ExpType.Value;
                                        exp = string.Empty;
                                        ope = Symbol.Neq;
                                        continue;
                                    case "like":
                                        expType = ExpType.Value;
                                        exp = string.Empty;
                                        ope = Symbol.Like;
                                        continue;
                                    case "&&":
                                        expType = ExpType.Exp;
                                        exp = string.Empty;
                                        this.Symbols.Add(Symbol.And);
                                        continue;
                                    case "||":
                                        expType = ExpType.Exp;
                                        exp = string.Empty;
                                        this.Symbols.Add(Symbol.Or);
                                        continue;
                                    default:
                                        throw new ExpressionException(condition, i, "invalid operator, {>, >=, <, <=, ==, &&, ||} are supported");
                                }
                            default:
                                exp = exp + condition[i];
                                continue;
                        }
                    case ExpType.Value:
                        switch (condition[i])
                        {
                            case '\'':
                                expType = ExpType.SingleQuotes;
                                exp = string.Empty;
                                continue;
                            case '"':
                                expType = ExpType.DoubleQuotes;
                                exp = string.Empty;
                                continue;
                            case ' ':
                                expType = ExpType.Exp;
                                value = exp;
                                exp = string.Empty;
                                continue;
                            default:
                                exp = exp + condition[i];
                                continue;
                        }
                    case ExpType.SingleQuotes:
                        switch (condition[i])
                        {
                            case '\\':
                                if (condition.Length > i + 1 && condition[i + 1] == '\'')
                                {
                                    exp = exp + '\'';
                                    i++;
                                }
                                else
                                {
                                    exp = exp + condition[i];
                                }
                                continue;
                            case '\'':
                                expType = ExpType.Exp;
                                value = exp;
                                exp = string.Empty;
                                continue;
                            default:
                                exp = exp + condition[i];
                                continue;
                        }
                    case ExpType.DoubleQuotes:
                        switch (condition[i])
                        {
                            case '\\':
                                if (condition.Length > i + 1 && condition[i + 1] == '"')
                                {
                                    exp = exp + '"';
                                    i++;
                                }
                                else
                                {
                                    exp = exp + condition[i];
                                }
                                continue;
                            case '"':
                                expType = ExpType.Exp;
                                value = exp;
                                exp = string.Empty;
                                continue;
                            default:
                                exp = exp + condition[i];
                                continue;
                        }
                }
            }

            switch (expType)
            {
                case ExpType.DoubleQuotes:
                case ExpType.SingleQuotes:
                case ExpType.Value:
                    if (this.Queries.Any() && !string.IsNullOrEmpty(key) && ope != Symbol.Invalid && !string.IsNullOrEmpty(exp))
                    {
                        this.Queries.Add(new QueryExpression(key, ope, exp));
                    }
                    else
                    {
                        value = exp;
                    }
                    break;
                case ExpType.Exp:
                    if (this.Queries.Any() && !string.IsNullOrEmpty(key) && ope != Symbol.Invalid && !string.IsNullOrEmpty(value))
                    {
                        this.Queries.Add(new QueryExpression(key, ope, value));
                    }
                    break;
                case ExpType.Key:
                case ExpType.Operator:
                    throw new ExpressionException(condition, condition.Length - 1, "Incomplete expression");
                case ExpType.Parenthesis:
                    throw new ExpressionException(condition, condition.Length - 1, "')' excepted");
            }
            #endregion parse

            if (this.Queries.Any())
            {
                this.Type = QueryType.Compound;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(key) && ope != Symbol.Invalid && !string.IsNullOrWhiteSpace(value))
                {
                    this.Type = QueryType.Base;
                    this.Key = key;
                    this.Ope = ope;
                    this.Value = value;
                }
            }
        }
    }
}
