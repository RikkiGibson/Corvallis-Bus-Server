using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace CorvallisBusDNX.Util
{
    /// <summary>
    /// Taken from here: http://blogs.msdn.com/b/pfxteam/archive/2011/01/15/asynclazy-lt-t-gt.aspx
    /// </summary>
    public class AsyncLazy<T> : Lazy<Task<T>>
    {
        public AsyncLazy(Func<T> valueFactory) 
            : base(() => Task.Factory.StartNew(valueFactory)) { }

        public AsyncLazy(Func<Task<T>> taskFactory) 
            : base(() => Task.Factory.StartNew(() => taskFactory()).Unwrap()) { }

        // This is used so you can implicitly unwrap Value.
        //
        // Example:
        //
        // Without ----> var thingIWant = await MyAsyncLazyType.Value
        // With    ----> var thingIWant = await MyAsyncLazyType
        public TaskAwaiter<T> GetAwaiter() => Value.GetAwaiter();
    }
}
