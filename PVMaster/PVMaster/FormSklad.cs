using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PVMaster
{
    public partial class FormSklad : Form
    {
        private string _branchToFormSklad = "";
        public FormSklad(string branchToFormSklad)
        {
            InitializeComponent();
            _branchToFormSklad = branchToFormSklad;
        }
    }
}
