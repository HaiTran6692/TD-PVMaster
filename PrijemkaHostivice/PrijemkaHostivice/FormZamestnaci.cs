using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrijemkaHostivice
{
    public partial class FormZamestnaci : Form
    {
        private string _branchToFormZamestnaci = "";
        public FormZamestnaci(string branchToFormZamestnaci)
        {
            InitializeComponent();
            _branchToFormZamestnaci = branchToFormZamestnaci;
            CapNhatLai();
        }
      

        private void FormZamestnaci_Load(object sender, EventArgs e)
        {
            this.Text = $"PV_Report v3c.170721 Zaměstnaci {_branchToFormZamestnaci}";
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            CapNhatLai();

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }
        private void CapNhatLai()
        {
            string sql1 = $@"SELECT distinct(pwa.[IDParttimeWorker]) as ID
                              ,pwa.[Name]
	                          ,ISNULL(zam.Stt,pwa.[IDParttimeWorker]) as ID_Tamda
	                          ,ISNULL(Firma,Agency) as Firma
	                          ,ISNULL([Pracovní pozice],'Skladník') as Pozice ,Isnull([druh Poměru],'agency') as  Druh
                          FROM [HumanResourceManagement].[dbo].[ParttimeWorkerActivity] pwa
                          left join [HumanResourceManagement].[dbo].[ParttimeWorker] pw on pwa.IDParttimeWorker=pw.IDParttimeWorker
                          left join [TamdaSW].dbo.View_PVReport_PamZamStt zam on TRY_CONVERT(INT,pw.CardID)=zam.Stt --collate DATABASE_default
                          where DATEDIFF(Day,[WorkedDate],getdate())=0
                          order by Isnull([druh Poměru],'agency') desc, ID_Tamda";
            try
            {
                DataTable TB = DataProvider.Instance.ExecuteQuery(sql1);
                dataGridView1.DataSource = TB;
                for (int i = 0; i < dataGridView1.Columns.Count - 1; i++)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                }
                dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
