using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVMaster
{

    static class ClassLocalId
    {
        private static string _globalVar = "";

        public static string GlobalLocalid
        {
            get { return _globalVar; }
            set { _globalVar = value; }
        }
    }
}
