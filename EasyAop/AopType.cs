using System;
using System.Collections.Generic;
using System.Text;

namespace EasyAop.Core
{
    /// <summary>
    /// use in class or property, it don't work in method
    /// </summary>
    public enum AopType
    {
        /// <summary>
        /// if on class then method ,if on property then get and set
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// only method 
        /// </summary>
        Method = 1,
        /// <summary>
        /// only ctor
        /// </summary>
        Ctor = 2,
        /// <summary>
        /// only get
        /// </summary>
        Get = 4,
        /// <summary>
        /// only set
        /// </summary>
        Set = 8,
        /// <summary>
        /// get or set 
        /// </summary>
        Property = 12,
        /// <summary>
        /// all
        /// </summary>
        All = 15
    }
}
