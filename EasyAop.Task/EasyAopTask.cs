using System;
using System.Collections.Generic;
using System.Text;

namespace EasyAop
{
  public  class EasyAopTask : Microsoft.Build.Utilities.Task
    {
        public string OutputPath
        {
            get; set;
        }

        public string TargetPath { get; set; }
        public override bool Execute()
        {
            try
            {
                EasyAop.Core.EasyAop.Work(TargetPath, OutputPath);
                return true;
            }
            catch (Exception e)
            {
                throw e;
            }

        }
    }
}
