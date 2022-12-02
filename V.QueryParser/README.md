# V.QueryParser

木叉一人工作室封装的表达式字符串解析类库，该类库只需要遍历一次字符串即可解析出对应的表达式结构，但是目前对于表达式的格式要求比较严格，在两个关键词之间必须加上空格，否则会解析失败。

```
/// <summary>
/// 查询表达式解析器
/// <para>key1 > value1 && key2 == value2 || key3 like value3</para>
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

    public QueryExpression(string condition){}
}
```

当初始化表达式为空时，Type 为 QueryType.All，表示查询全部；当 Type 为 QueryType.Base 时，表示表达式为基础查询，直接看 Key、Ope、Value 三个字段，Ope 支持 Gt、Gte、Lt、Lte、Eq、Neq、Like，当 value 部分为字符串时，可以用英文单引号或双引号包围起来，使表达式更加清晰可读，当然也可以不使用，但是这种情况下，value 不可以包含空格。

当 Type 为 QueryType.Compound 时，表示表达式为组合查询，需要使用到 Queries、Symbols 两个字段，Queries 依次存储从左到右的表达式，Symbols 依次存储从左到右的逻辑运算符，Queries 的长度等于 Symbols 长度 + 1，在解析组合查询时，应该遍历 Symbols，先把 Ope.And 左右两边的表达式先进行运算(即，若 Symbols[i] == Ope.And，则先运算 Queries[i] and Queries[i + 1])，最后再运算 Ope.Or。若表达式不存在括号，则 Queries 中全部都是基础查询，若存在括号，则 Queries 可能包含组合查询。

```
// 使用以下代码测试表达式，并查看解析后的表达式数据格式
var query = new QueryExpression("key1 > value1 && key2 == value2 || key3 like value3");
```

若出现表达式解析异常，或需要支持的特性，欢迎提交 issue 以及 PR