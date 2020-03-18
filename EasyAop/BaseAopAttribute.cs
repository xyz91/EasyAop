using System;
using System.Collections.Generic;
using System.Text;

namespace EasyAop.Core
{
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Constructor, AllowMultiple = false)]
    public abstract class BaseAopAttribute : Attribute
    {
        public int Order { get; set; }
        public AopType Type { get; set; }
        public abstract void Before(ExceEventArg method);
        public abstract void After(ExceEventArg method);
        public abstract void Exception(ExceEventArg method);

    }
}
