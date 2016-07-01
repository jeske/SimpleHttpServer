using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer
{
    static class ExceptionHandler
    {
        public static void Handle(ILog log, Exception ex)
        {
            log.Error(ex);

            if (ConfigurationDefaults.ThrowExceptions)
                throw ex;
        }
    }
}
