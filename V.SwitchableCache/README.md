# V.SwitchableCache

```
/// <summary>
/// 添加可切换的缓存服务
/// <para>通过注入 <seealso cref="ICacheService"/> 来引用</para>
/// </summary>
/// <param name="services"></param>
/// <param name="redisConnectionString">若不为空，则启用 redis 缓存，并且可直接注入 <seealso cref="ConnectionMultiplexer"/>，因为该方法将连接池对象添加到容器中了</param>
/// <param name="redisDb"></param>
/// <returns></returns>
public static IServiceCollection AddSwitchableCache(this IServiceCollection services, string redisConnectionString = null, int redisDb = 0);
```