using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Servicios.Singleton
{
    public class ExcepcionesLogIn:Exception
    {
        public ResultadosLogIn Result;
        public ExcepcionesLogIn(ResultadosLogIn result)
        {
            Result = result;
        }
    }
}
