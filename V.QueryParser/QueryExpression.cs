using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

            var cursorType = ExpType.Init;
            var expectType = ExpType.Init;

            #region parse
            for (int i = 0; i < condition.Length; i++)
            {
                switch (expectType)
                {
                    case ExpType.Init:
                    case ExpType.Exp:
                        switch (condition[i])
                        {
                            case '(':
                                cursorType = ExpType.Parenthesis;
                                expectType = ExpType.Parenthesis;
                                exp = string.Empty;
                                continue;
                            case ' ':
                                continue;
                            default:
                                cursorType = ExpType.Key;
                                expectType = ExpType.Key;
                                exp = condition[i].ToString();
                                continue;
                        }
                    case ExpType.Parenthesis:
                        switch (condition[i])
                        {
                            case ')':
                                cursorType = ExpType.Exp;
                                expectType = ExpType.Operator;
                                this.Queries.Add(new QueryExpression(exp));
                                exp = string.Empty;
                                continue;
                            default:
                                cursorType = ExpType.Parenthesis;
                                expectType = ExpType.Parenthesis;
                                exp += condition[i];
                                continue;
                        }
                    case ExpType.Key:
                        switch (condition[i])
                        {
                            case ' ':
                                cursorType = ExpType.Key;
                                expectType = ExpType.Operator;
                                key = exp;
                                exp = string.Empty;
                                continue;
                            default:
                                cursorType = ExpType.Key;
                                expectType = ExpType.Key;
                                exp += condition[i];
                                continue;
                        }
                    case ExpType.Operator:
                        switch (condition[i])
                        {
                            case ' ':
                                if (string.IsNullOrEmpty(exp))
                                {
                                    continue;
                                }
                                else
                                {
                                    cursorType = ExpType.Operator;
                                    switch (exp.ToLower())
                                    {
                                        case ">":
                                            if (string.IsNullOrEmpty(key))
                                            {
                                                throw new ExpressionException(condition, i, "比较运算符之前应该要有关键词");
                                            }

                                            expectType = ExpType.Value;
                                            exp = string.Empty;
                                            ope = Symbol.Gt;
                                            continue;
                                        case ">=":
                                            if (string.IsNullOrEmpty(key))
                                            {
                                                throw new ExpressionException(condition, i, "比较运算符之前应该要有关键词");
                                            }

                                            expectType = ExpType.Value;
                                            exp = string.Empty;
                                            ope = Symbol.Gte;
                                            continue;
                                        case "<":
                                            if (string.IsNullOrEmpty(key))
                                            {
                                                throw new ExpressionException(condition, i, "比较运算符之前应该要有关键词");
                                            }

                                            expectType = ExpType.Value;
                                            exp = string.Empty;
                                            ope = Symbol.Lt;
                                            continue;
                                        case "<=":
                                            if (string.IsNullOrEmpty(key))
                                            {
                                                throw new ExpressionException(condition, i, "比较运算符之前应该要有关键词");
                                            }

                                            expectType = ExpType.Value;
                                            exp = string.Empty;
                                            ope = Symbol.Lte;
                                            continue;
                                        case "==":
                                            if (string.IsNullOrEmpty(key))
                                            {
                                                throw new ExpressionException(condition, i, "比较运算符之前应该要有关键词");
                                            }

                                            expectType = ExpType.Value;
                                            exp = string.Empty;
                                            ope = Symbol.Eq;
                                            continue;
                                        case "!=":
                                            if (string.IsNullOrEmpty(key))
                                            {
                                                throw new ExpressionException(condition, i, "比较运算符之前应该要有关键词");
                                            }

                                            expectType = ExpType.Value;
                                            exp = string.Empty;
                                            ope = Symbol.Neq;
                                            continue;
                                        case "like":
                                            if (string.IsNullOrEmpty(key))
                                            {
                                                throw new ExpressionException(condition, i, "like 运算符之前应该要有关键词");
                                            }

                                            expectType = ExpType.Value;
                                            exp = string.Empty;
                                            ope = Symbol.Like;
                                            continue;
                                        case "&&":
                                            if (!string.IsNullOrEmpty(key) && ope != Symbol.Invalid && !string.IsNullOrEmpty(value))
                                            {
                                                this.Queries.Add(new QueryExpression(key, ope, value));
                                                key = string.Empty;
                                                ope = Symbol.Invalid;
                                                value = string.Empty;
                                            }
                                            if (!this.Queries.Any())
                                            {
                                                throw new ExpressionException(condition, i, "逻辑运算符之前应该要有表达式");
                                            }

                                            expectType = ExpType.Exp;
                                            exp = string.Empty;
                                            this.Symbols.Add(Symbol.And);
                                            continue;
                                        case "||":
                                            if (!string.IsNullOrEmpty(key) && ope != Symbol.Invalid && !string.IsNullOrEmpty(value))
                                            {
                                                this.Queries.Add(new QueryExpression(key, ope, value));
                                                key = string.Empty;
                                                ope = Symbol.Invalid;
                                                value = string.Empty;
                                            }
                                            if (!this.Queries.Any())
                                            {
                                                throw new ExpressionException(condition, i, "逻辑运算符之前应该要有表达式");
                                            }

                                            expectType = ExpType.Exp;
                                            exp = string.Empty;
                                            this.Symbols.Add(Symbol.Or);
                                            continue;
                                        default:
                                            throw new ExpressionException(condition, i, "无法识别的运算符");
                                    }
                                }
                            default:
                                cursorType = ExpType.Operator;
                                expectType = ExpType.Operator;
                                exp += condition[i];
                                continue;
                        }
                    case ExpType.Value:
                        switch (condition[i])
                        {
                            case ' ':
                                if (string.IsNullOrEmpty(exp))
                                {
                                    continue;
                                }
                                else
                                {
                                    if (string.IsNullOrEmpty(key) || ope == Symbol.Invalid)
                                    {
                                        throw new ExpressionException(condition, i, "不合法的表达式");
                                    }

                                    cursorType = ExpType.Exp;
                                    expectType = ExpType.Operator;
                                    value = exp;
                                    exp = string.Empty;
                                }
                                break;
                            case '\'':
                                if (string.IsNullOrEmpty(exp))
                                {
                                    cursorType = ExpType.SingleQuotes;
                                    expectType = ExpType.SingleQuotes;
                                }
                                else
                                {
                                    cursorType = ExpType.Value;
                                    expectType = ExpType.Value;
                                    exp += condition[i];
                                    continue;
                                }
                                break;
                            case '"':
                                if (string.IsNullOrEmpty(exp))
                                {
                                    cursorType = ExpType.DoubleQuotes;
                                    expectType = ExpType.DoubleQuotes;
                                }
                                else
                                {
                                    cursorType = ExpType.Value;
                                    expectType = ExpType.Value;
                                    exp += condition[i];
                                    continue;
                                }
                                break;
                            default:
                                cursorType = ExpType.Value;
                                expectType = ExpType.Value;
                                exp += condition[i];
                                continue;
                        }
                        break;
                    case ExpType.SingleQuotes:
                        switch (condition[i])
                        {
                            case '\\':
                                cursorType = ExpType.SingleQuotes;
                                expectType = ExpType.SingleQuotes;

                                if (condition.Length > i + 1 && condition[i + 1] == '\'')
                                {
                                    exp = exp + '\'';
                                    i++;
                                }
                                else
                                {
                                    exp += condition[i];
                                }
                                continue;
                            case '\'':
                                if (string.IsNullOrEmpty(key) || ope == Symbol.Invalid)
                                {
                                    throw new ExpressionException(condition, i, "不合法的表达式");
                                }

                                cursorType = ExpType.Exp;
                                expectType = ExpType.Operator;
                                value = exp;
                                exp = string.Empty;
                                continue;
                            default:
                                cursorType = ExpType.SingleQuotes;
                                expectType = ExpType.SingleQuotes;
                                exp += condition[i];
                                continue;
                        }
                    case ExpType.DoubleQuotes:
                        switch (condition[i])
                        {
                            case '\\':
                                cursorType = ExpType.DoubleQuotes;
                                expectType = ExpType.DoubleQuotes;

                                if (condition.Length > i + 1 && condition[i + 1] == '"')
                                {
                                    exp = exp + '"';
                                    i++;
                                }
                                else
                                {
                                    exp += condition[i];
                                }
                                continue;
                            case '"':
                                if (string.IsNullOrEmpty(key) || ope == Symbol.Invalid)
                                {
                                    throw new ExpressionException(condition, i, "不合法的表达式");
                                }

                                cursorType = ExpType.Exp;
                                expectType = ExpType.Operator;
                                value = exp;
                                exp = string.Empty;
                                continue;
                            default:
                                cursorType = ExpType.DoubleQuotes;
                                expectType = ExpType.DoubleQuotes;
                                exp += condition[i];
                                continue;
                        }
                }
            }

            switch (cursorType)
            {
                case ExpType.DoubleQuotes:
                    throw new ExpressionException(condition, condition.Length - 1, "缺失 \"");
                case ExpType.Exp:
                    if (this.Queries.Any() && !string.IsNullOrEmpty(key) && ope != Symbol.Invalid && !string.IsNullOrEmpty(value))
                    {
                        this.Queries.Add(new QueryExpression(key, ope, value));
                    }
                    else if (this.Queries.Count == 1)
                    {
                        var query = this.Queries.First();
                        this.Queries = query.Queries;
                        this.Value = query.Value;
                        this.Symbols = query.Symbols;
                        this.Key = query.Key;
                        this.Ope = query.Ope;
                        this.Type = query.Type;
                    }
                    break;
                case ExpType.Key:
                case ExpType.Operator:
                    throw new ExpressionException(condition, condition.Length - 1, "不完整的表达式");
                case ExpType.Parenthesis:
                    throw new ExpressionException(condition, condition.Length - 1, "缺失 )");
                case ExpType.SingleQuotes:
                    throw new ExpressionException(condition, condition.Length - 1, "缺失 '");
                case ExpType.Value:
                    if (string.IsNullOrEmpty(exp))
                    {
                        throw new ExpressionException(condition, condition.Length - 1, "不完整的表达式");
                    }

                    value = exp;
                    if (this.Queries.Any() && !string.IsNullOrEmpty(key) && ope != Symbol.Invalid && !string.IsNullOrEmpty(value))
                    {
                        this.Queries.Add(new QueryExpression(key, ope, value));
                    }
                    break;
            }
            #endregion parse

            if (this.Queries.Any())
            {
                if (this.Queries.Count != this.Symbols.Count + 1)
                {
                    throw new ExpressionException(condition, condition.Length - 1, "不完整的表达式");
                }

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
