using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace EasyAop.Core
{
    public class ExceEventArg
    {
        public Exception Error { get; set; }
        public MethodBase MethodInfo { get; set; }
        public object ReturnValue { get; set; }
        private List<object> _parameters = new List<object>();
        public List<object> Parameters { get { return _parameters; } set { _parameters = value; } }
    }
}
