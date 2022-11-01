using System;
using System.Collections.Generic;
using System.Text;

namespace V.QueryParser
{
    public enum QueryType
    {
        /// <summary>
        /// 查询所有
        /// </summary>
        All,
        /// <summary>
        /// 基础查询，类似于 key>=value
        /// </summary>
        Base,
        /// <summary>
        /// 组合查询，exp AND exp OR exp
        /// </summary>
        Compound
    }

    public enum Symbol
    {
        Invalid,
        Gt,
        Gte,
        Lt,
        Lte,
        Eq,
        Neq,
        And,
        Or
    }

    public enum ExpType
    {
        Init,
        Key,
        Operator,
        Value,
        SingleQuotes,
        DoubleQuotes,
        Parenthesis,
        Exp
    }
}
