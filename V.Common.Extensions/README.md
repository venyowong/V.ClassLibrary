# V.Common.Extensions

木叉一人工作室常用扩展方法封装

## Nuget

[V.Common.Extensions](https://www.nuget.org/packages/V.Common.Extensions)

## HttpRequestExtension

### ReadBody() 方法
从 HttpRequest 读取 String，读完后将 Stream Position 设置为 0

### GetAbsoluteUrl() 方法
获取 HttpRequest 所对应的当前请求链接

### GetAbsoluteUrl(string path) 方法
将 path 转换为绝对路径 url，/ 开头，不支持 ../

## HttpResponseMessageExtension

### ReadAsObj<T>() 方法
将 HttpResponseMessage 的内容读取为 T 对象

### ReadAsString() 方法
将 HttpResponseMessage 的内容读取为 String

### ReadAsDecompressedString() 方法
将 HttpResponseMessage 的 GZip 压缩内容读取为 String

## ListExtension

### IsNullOrEmpty() 方法
判断 IEnumerable<T> 是否为 null 或者长度为 0

## ServiceCollectionExtension

### AddTransientBothTypes 方法
为父类型和子类型同时添加服务实例，避免通过父类注入的时候，无法通过子类类型查找子类对象，反之亦然。可以避免通过父类注入时，通过 GetServices 获取所有对象再查找子类时导致的内存浪费

## StringExtension

### ToJson() 方法
使用 Newtonsoft.Json 将对象转换为 json 字符串

### ToObj<T>() 方法
使用 Newtonsoft.Json 将 json 字符串转换为 T 对象

### Md5() 方法
md5 hash

### MaskMobile() 方法
将手机号中间四位转换为 * 得到脱敏手机号

### IsValidMobile() 方法
使用正则表达式判断字符串是否为合法手机号

### Sha1() 方法
SHA1 hash

### DESEncrypt(byte[] key = null, byte[] iv = null) 方法
DES 加密，默认 key 为 Encoding.ASCII.GetBytes("670851ad")，默认 iv 为 Encoding.ASCII.GetBytes("89532a19")

### DESDecrypt(byte[] key = null, byte[] iv = null) 方法
DES 解密，默认 key 为 Encoding.ASCII.GetBytes("670851ad")，默认 iv 为 Encoding.ASCII.GetBytes("89532a19")