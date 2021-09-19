using DGVPrinterHelper;
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
    public partial class FormZamestnaci : Form
    {
        private BackgroundWorker worker = null;
        DataTable TB = new DataTable();
        private string _branchToFormZamestnaci = "";
        public FormZamestnaci(string branchToFormZamestnaci)
        {
            InitializeComponent();
            progressBar1.Visible = false;
            worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += worker_DoWork;
            worker.RunWorkerCompleted += worker_RunWorkerCompleted;
            _branchToFormZamestnaci = branchToFormZamestnaci;
            label1.Text = $"Seznam zaměstnaci {_branchToFormZamestnaci} dne {DateTime.Today.ToString("dd.MM.yyyy")}";

        }
        void worker_DoWork(object sender, DoWorkEventArgs e)
        {
            CapNhatLai();
        }
        void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dataGridView1.DataSource = TB;
            for (int i = 0; i < dataGridView1.Columns.Count - 1; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
            }
            dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;

            pictureBox1.Enabled = true;
            pictureBox2.Enabled = true;
            pictureBox3.Enabled = true;
            progressBar1.Visible = false;
            MessageBox.Show("Hotovo!");

        }

        private void FormZamestnaci_Load(object sender, EventArgs e)
        {
            WriteLogToCache.Instance.WriteToCache($"Zamestnaci {_branchToFormZamestnaci}", ClassLocalId.GlobalLocalid);
            this.Text = $"PVMaster Zaměstnaci {_branchToFormZamestnaci}";
            if (_branchToFormZamestnaci == "DC - Morava")
            { 
                textBox1_PC_Name.Text = "DCMO-ANNINH"; 
            }
            else if (_branchToFormZamestnaci == "TD - Brno")
            {
                textBox1_PC_Name.Text = "TAMDA-B-TRACK";
            }
            else
            {
                textBox1_PC_Name.Text = label5.Text= "";
            }


            if (!worker.IsBusy)
            {
                pictureBox1.Enabled = false;
                pictureBox2.Enabled = false;
                pictureBox3.Enabled = false;
                progressBar1.Visible = true;
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = 1;
                worker.RunWorkerAsync();
            }
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            if (!worker.IsBusy)
            {
                pictureBox1.Enabled = false;
                pictureBox2.Enabled = false;
                pictureBox3.Enabled = false;
                progressBar1.Visible = true;
                progressBar1.Style = ProgressBarStyle.Marquee;
                progressBar1.MarqueeAnimationSpeed = 1;
                worker.RunWorkerAsync();
            }


        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            ExportDTGVToExcel.Instance.ExportToExcel(dataGridView1);
            WriteLogToCache.Instance.WriteToCache("Export Excel seznam zamestnaci", ClassLocalId.GlobalLocalid);
        }
        private void CapNhatLai()
        {
            string sql1 = $@"WITH BANG1 AS (SELECT distinct(pwa.[IDParttimeWorker]) as ID_Z  
	                          ,ISNULL(zam.Stt,pwa.[IDParttimeWorker]) as ID_TD ,pwa.[Name] as Jmeno_Prijmeni
	                          ,ISNULL(Firma,Agency) as Firma
	                          ,ISNULL([Pracovní pozice],'Skladník') as Pozice 
                              ,zam.cp as Pobocka
                              --,misto as Misto
                          FROM [HumanResourceManagement].[dbo].[ParttimeWorkerActivity] pwa
                          left join [HumanResourceManagement].[dbo].[ParttimeWorker] pw on pwa.IDParttimeWorker=pw.IDParttimeWorker
                          left join [TamdaSW].dbo.View_PVReport_PamZamStt zam on TRY_CONVERT(INT,pw.CardID)=zam.Stt --collate DATABASE_default
                          where DATEDIFF(Day,[WorkedDate],getdate())=0 and [Druh poměru] is not null )                     
                           SELECT 
						  ROW_NUMBER() OVER( ORDER BY ID_TD) as R#,* 						  
						  FROM BANG1
						  order by  ID_TD";

            string sql2 = $@"WITH BANG1 AS (SELECT distinct(pwa.[IDParttimeWorker]) as ID_Z  
	                          ,ISNULL(zam.Stt,pwa.[IDParttimeWorker]) as ID_TD ,pwa.[Name] as Jmeno_Prijmeni
	                          ,ISNULL(Firma,Agency) as Firma
	                          ,ISNULL([Pracovní pozice],'Skladník') as Pozice 
                              ,case when pc_des='DCMO-ANNINH' then 'DC-MORAVA' ELSE 'TD-BRNO' END AS Pobocka
                          FROM [HumanResourceManagement].[dbo].[ParttimeWorkerActivity] pwa
                          left join [HumanResourceManagement].[dbo].[ParttimeWorker] pw on pwa.IDParttimeWorker=pw.IDParttimeWorker
                          left join [TamdaSW].dbo.View_PVReport_PamZamStt zam on TRY_CONVERT(INT,pw.CardID)=zam.Stt --collate DATABASE_default
                          where DATEDIFF(Day,[WorkedDate],getdate())=0 and [Druh poměru] is not null and pc_des='{textBox1_PC_Name.Text}' )                     
                           SELECT 
						  ROW_NUMBER() OVER( ORDER BY ID_TD) as R#,* 						  
						  FROM BANG1
						  order by  ID_TD";

            try
            {

                if (_branchToFormZamestnaci == "DC - Morava" || _branchToFormZamestnaci == "TD - Brno")
                {
                    DataProvider.SetConnectString = $@"Data Source=192.168.5.100,1434;Initial Catalog=TamdaSW;User ID=admin;Password=c81a57305c570bb51ba0f4a6d048274c;";                  
                    TB = DataProvider.Instance.ExecuteQuery(sql2);                    
                }
                else
                {
                    TB = DataProvider.Instance.ExecuteQuery(sql1);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        private void pictureBox2_Click_1(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCellsExceptHeader;
            }
            WriteLogToCache.Instance.WriteToCache("Print seznam zamestnaci", ClassLocalId.GlobalLocalid);
            DGVPrinter printer = new DGVPrinter();
            printer.Title = $"Seznam zaměstnaců {_branchToFormZamestnaci}";//Header
            printer.SubTitle = string.Format("Dne  {0}", DateTime.Now.ToString("dd.MM.yyyy-HH:mm"));
            printer.SubTitleFormatFlags = StringFormatFlags.LineLimit | StringFormatFlags.NoClip;
            printer.PageNumbers = true;
            printer.PageNumberInHeader = true;
            printer.PrintMargins.Left = 70;
            printer.ShowTotalPageNumber = true;
            printer.PorportionalColumns = true;
            printer.HeaderCellAlignment = StringAlignment.Near;
            printer.SubTitleSpacing = 15;
            printer.FooterSpacing = 10;
            printer.printDocument.DefaultPageSettings.Landscape = false;
            printer.PrintDataGridView(dataGridView1);
        }
    }
}
