using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleHttpServer.Enums
{
    enum HttpStatusCode
    {
        Ok = 200,
        NotFound = 404,
        MethodNotAllowed = 405,
        InternalServerError = 500
    }
}
