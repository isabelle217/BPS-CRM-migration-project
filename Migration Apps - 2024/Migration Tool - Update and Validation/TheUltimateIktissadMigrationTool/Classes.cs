using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheUltimateIktissadMigrationTool
{
    class Warning
    {
        public Warning(string guid, string exceptionString)
        {
            Guid = guid;
            ExceptionString = exceptionString;
        }
        public string Guid { get; set; }
        public string ExceptionString { get; set; }
    }
}