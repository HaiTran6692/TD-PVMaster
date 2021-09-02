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
        private BackgroundWorker worker_left = null;
        private BackgroundWorker worker_right = null;
        private string _branchToFormSklad = "";
        DataTable TB_left = new DataTable();
        DataTable TB_Prijem = new DataTable();
        DataTable TB_Vydej = new DataTable();

        string _kod_zbozi = "0";

        public FormSklad(string branchToFormSklad)
        {
            InitializeComponent();
            _branchToFormSklad = branchToFormSklad;
            progressBar_left.Visible = false;
            worker_left = new BackgroundWorker();
            worker_left.WorkerReportsProgress = true;
            worker_left.DoWork += worker_left_DoWork;
            worker_left.RunWorkerCompleted += worker_left_RunWorkerCompleted;


            progressBar_right.Visible = false;
            worker_right = new BackgroundWorker();
            worker_right.WorkerReportsProgress = true;
            worker_right.DoWork += worker_right_DoWork;
            worker_right.RunWorkerCompleted += worker_right_RunWorkerCompleted;
        }

        private void FormSklad_Load(object sender, EventArgs e)
        {
            this.Text = $"PVMaster v3c.170721 Sklad {_branchToFormSklad}";
            this.WindowState = FormWindowState.Maximized;

            progressBar_left.Visible = true;
            progressBar_left.Style = ProgressBarStyle.Marquee;
            progressBar_left.MarqueeAnimationSpeed = 1;
            worker_left.RunWorkerAsync();
        }
        void worker_left_DoWork(object sender, DoWorkEventArgs e)
        {
            if (placeHolderTextBox1.Text.Length > 0)
            {
                LoadSkladAllPozice(placeHolderTextBox1.Text);
            }
            else
            {
                LoadSkladAllPozice();
            }
        }
        void worker_left_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dataGridView1.DataSource = null;
            progressBar_left.Visible = false;
            if (TB_left.Rows.Count > 0)
            {
                dataGridView1.Columns.Clear();
                dataGridView1.DataSource = TB_left;
                dataGridView1.Columns[0].HeaderText = "Kod zboží";
                dataGridView1.Columns[1].HeaderText = "Název";
                dataGridView1.Columns[2].HeaderText = "Množství";
                dataGridView1.Columns[3].HeaderText = "Pozice";
                dataGridView1.Columns[4].HeaderText = "Kod paletu";
                for (int i = 0; i < dataGridView1.Columns.Count - 1; i++)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
        }
        void worker_right_DoWork(object sender, DoWorkEventArgs e)
        {
            if (_kod_zbozi.Length > 5)
            {
                Load_PrijemVydej(_kod_zbozi);
            }
        }
        void worker_right_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            dataGridView2.DataSource = null;
            dataGridView3.DataSource = null;
            progressBar_left.Visible = false;

            if (TB_Prijem.Rows.Count > 0)
            {
                dataGridView2.Columns.Clear();
                dataGridView2.DataSource = TB_left;
                dataGridView2.Columns[0].HeaderText = "Kod zboží";
                dataGridView2.Columns[1].HeaderText = "Název";
                dataGridView2.Columns[2].HeaderText = "Množství";
                dataGridView2.Columns[3].HeaderText = "Pozice";
                dataGridView2.Columns[4].HeaderText = "Kod paletu";
                for (int i = 0; i < dataGridView2.Columns.Count - 1; i++)
                {
                    dataGridView2.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }


            if (TB_Vydej.Rows.Count > 0)
            {
                dataGridView3.Columns.Clear();
                dataGridView3.DataSource = TB_left;
                dataGridView3.Columns[0].HeaderText = "Kod zboží";
                dataGridView3.Columns[1].HeaderText = "Název";
                dataGridView3.Columns[2].HeaderText = "Množství";
                dataGridView3.Columns[3].HeaderText = "Pozice";
                dataGridView3.Columns[4].HeaderText = "Kod paletu";
                for (int i = 0; i < dataGridView3.Columns.Count - 1; i++)
                {
                    dataGridView3.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
            }
        }

        private void Load_PrijemVydej(string _mnb)
        {
            string sqlP = $@"with bang_nhap as
                            (select  mr_adrqre as dok
                            ,mr_codrec as user_ 
                            ,mr_qtruvc as slg   
                            ,mr_datmvt as ngay_nhap   
                            ,mr_usscc as palet
                            ,mr_ncdefo as so_don
                            from tb_mvtre
                            where mr_cproin='{_mnb}'
                            union all
                            select hr_adrqre as dok
                            ,hr_codrec as user_                             
                            ,hr_qtruvc as slg
                            ,hr_datmvt as ngay_nhap                                                       
                            ,hr_usscc as palet
                            ,hr_ncdefo as so_don
                            from tb_hmvtre 
                            where hr_cproin='{_mnb}')
                            select * from bang_nhap
                            where   TO_DATE(ngay_nhap,'DD-MM-YYYY') >= TO_DATE(SYSDATE-90, 'DD-MM-YYYY')
                            order by ngay_nhap desc";

            string sqlV = $@"WITH bang1
                            AS (SELECT
                              [date_start],
                              t.[order_number]      --as N'Số đơn'
                              ,
                              p.[product_code] --as N'Mã nội bộ'    
                              ,
                              [human_prepared] --as N'Mã NV'
                              ,
                              [location_id] --as N'Vị trí'
                              ,
                              [quantity_request] --as 'Slg đặt'
                              ,
                              [quantity_prepared] --as 'Slg chốt'
                              ,
                              CASE
                                WHEN p.typ = 1 THEN N'Nhặt đủ'
                                WHEN p.typ = 3 THEN N'Nhặt thiếu'
                                ELSE N'Hủy lệnh'
                              END AS Status_,
                              p.[pallet_number]
                            FROM [TDManagement].[dbo].[Location_Order_Picking] p
                            LEFT JOIN [TDManagement].[dbo].[Location_Order_Tourn] t
                              ON t.pallet_number = p.pallet_number
                            LEFT JOIN [TDManagement].[dbo].[Location_Order_Items] i
                              ON t.[order_number] = i.[order_number]
                              AND p.product_code = i.product_code
                            WHERE p.[product_code] = '{_mnb}'  UNION ALL
                            SELECT
                              [date_start],
                              t.[order_number]      --as N'Số đơn'
                              ,p.[product_code] --as N'Mã nội bộ'	
                              ,[human_prepared] --as N'Mã NV'
                              ,[location_id] --as N'Vị trí'
                              ,[quantity_request] --as 'Slg đặt'
                              ,[quantity_prepared] --as 'Slg chốt'
                              ,CASE
                                WHEN p.typ = 1 THEN N'Nhặt đủ'
                                WHEN p.typ = 3 THEN N'Nhặt thiếu'
                                ELSE N'Hủy lệnh'
                              END AS Status_,
                              p.[pallet_number]
                            FROM [TDArchive].[dbo].[Location_Order_Picking] p
                            LEFT JOIN [TDManagement].[dbo].[Location_Order_Tourn] t
                              ON t.pallet_number = p.pallet_number
                            LEFT JOIN [TDManagement].[dbo].[Location_Order_Items] i
                              ON t.[order_number] = i.[order_number]
                              AND p.product_code = i.product_code
                            WHERE p.[product_code] ='{_mnb}')

                            SELECT TOP 20
                              [human_prepared] as nv_nhat,
                              [quantity_request] as slg_yc,
                              [quantity_prepared] as slg_chot,
                              FORMAT(DATEADD(D, 0, SUBSTRING([date_start], 1, 8)), 'dd-MM-yyyy') + ' ' + SUBSTRING([date_start], 9, 2) + ':' + SUBSTRING([date_start], 11, 2) + ':' + SUBSTRING([date_start], 13, 2) AS N'Thời gian nhặt',
                              [order_number] as so_don,
                              --[product_code],                              
                              --[location_id],                              
                              Status_,
                              [pallet_number] as palet
                            FROM bang1
                            ORDER BY date_start DESC ";

            try
            {
                TB_Prijem = DataProvider.Instance.ExecuteQuery(sqlP);
                TB_Vydej = DataProvider.Instance.ExecuteQuery(sqlV);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }



        }
        private void LoadSkladAllPozice(string _mnb = "")
        {
            string sqlHos = $@"SELECT
                            ul_cproin            AS kod_zbozi
                            , ar_libpro            AS nazev
                             , ue_adrums            AS pozice                             
                               , ul_nqtuvc            AS mnozstvi
                                , ue_usscc as kod_pallet                           
                        FROM
                            tb_lcums
                            INNER JOIN tb_eums ON ue_usscc = ul_usscc
                            INNER JOIN tb_art ON ul_cproin = ar_cproin
                        WHERE ul_nqtuvc is not null and ue_codtsu != 3
                         and substr(ue_adrums, 1, 3) != '10E'
                         and ul_cproin like '%{_mnb}%'
                        ORDER BY  ue_adrums ";


            string sqlDcBrno = $@"SELECT
                            ul_cproin            AS kod_zbozi
                            , ar_libpro            AS nazev
                             , ue_adrums            AS pozice                             
                               , ul_nqtuvc            AS mnozstvi
                                , ue_usscc as kod_pallet                           
                        FROM
                            tb_lcums
                            INNER JOIN tb_eums ON ue_usscc = ul_usscc
                            INNER JOIN tb_art ON ul_cproin = ar_cproin
                        WHERE ul_nqtuvc is not null and ue_codtsu != 3
                         and substr(ue_adrums, 1, 3) != '20E'
                         and ul_cproin like '%{_mnb}%'
                        ORDER BY  ue_adrums ";


            if (_branchToFormSklad == "TK - Hostivice")
            {
                TB_left = DataOracle.Instance.ExecuteQuery(sqlHos);
            }
            else
            {
                TB_left = DataOracle.Instance.ExecuteQuery(sqlDcBrno);
            }

        }

        private void label5_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {

        }

        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow row = new DataGridViewRow();
                row = dataGridView1.Rows[e.RowIndex];
                _kod_zbozi = row.Cells[0].Value.ToString();

            }
            catch (Exception)
            {
                _kod_zbozi = "0";
            }
        }

        private void dataGridView1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                try
                {
                    if (!worker_right.IsBusy)
                    {
                        progressBar_right.Visible = true;
                        progressBar_right.Style = ProgressBarStyle.Marquee;
                        progressBar_right.MarqueeAnimationSpeed = 1;
                        worker_right.RunWorkerAsync();
                    }
                }
                catch (Exception)
                {

                }
            }
            else if (e.KeyCode == Keys.F5)
            {
                try
                {
                    DialogResult dialogResult = MessageBox.Show("Bạn có muốn xuất dữ liệu ra file Excel không?", "Cảnh báo", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information, MessageBoxDefaultButton.Button3);
                    if (dialogResult == DialogResult.Yes)
                    {
                        ExportDTGVToExcel.Instance.ExportToExcel(dataGridView1);
                    }
                }
                catch (Exception)
                {

                }
            }
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (!worker_right.IsBusy)
            {
                progressBar_right.Visible = true;
                progressBar_right.Style = ProgressBarStyle.Marquee;
                progressBar_right.MarqueeAnimationSpeed = 1;
                worker_right.RunWorkerAsync();
            }
        }
    }
}
