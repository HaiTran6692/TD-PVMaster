﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PVMaster
{
    class InvoiceDetail
    {
        public string kod_zbozi { get; set; }
        public string nazev { get; set; } //nazev_zbozi
        public string objednano { get; set; }
        public string prjato { get; set; }
        public string cena_bez { get; set; }
    }
}
