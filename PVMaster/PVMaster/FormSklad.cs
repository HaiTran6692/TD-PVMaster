using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
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
            this.Text = $"PVMaster Sklad {_branchToFormSklad}";
            //this.WindowState = FormWindowState.Maximized;

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
                dataGridView1.Columns[2].HeaderText = "Pozice";
                dataGridView1.Columns[3].HeaderText = "Množství";
                dataGridView1.Columns[4].HeaderText = "Total";
                dataGridView1.Columns[5].HeaderText = "Kod paletu";
               

                for (int i = 0; i < dataGridView1.Columns.Count - 1; i++)
                {
                    dataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                }
                dataGridView1.Columns[dataGridView1.Columns.Count - 1].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
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
            progressBar_right.Visible = false;
            decimal prijem = 0;
            decimal vydej =0;


            if (TB_Prijem.Rows.Count > 0)
            {
                dataGridView2.Columns.Clear();
                dataGridView2.DataSource = TB_Prijem;
                dataGridView2.Columns[0].HeaderText = "Kod zboží";
                dataGridView2.Columns[1].HeaderText = "Množství";
                dataGridView2.Columns[2].HeaderText = "Rampa";
                dataGridView2.Columns[3].HeaderText = "Datum";
                dataGridView2.Columns[4].HeaderText = "Číslo objednávky";
                dataGridView2.Columns[5].HeaderText = "Kod paletu";                
                for (int i = 0; i < dataGridView2.Columns.Count - 1; i++)
                {
                    dataGridView2.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                }
                for (int j = 0; j < TB_Prijem.Rows.Count; j++)
                {
                    prijem += decimal.Parse(TB_Prijem.Rows[j][1].ToString());
                }
            }
            
            label5Prijem.Text = $"Příjem( do 90 dnů): {prijem.ToString("00")} ks";


            if (TB_Vydej.Rows.Count > 0)
            {
                dataGridView3.Columns.Clear();
                dataGridView3.DataSource = TB_Vydej;
                dataGridView3.Columns[0].HeaderText = "Kod zboží";
                dataGridView3.Columns[1].HeaderText = "Objednáno";
                dataGridView3.Columns[2].HeaderText = "Vydáno";
                dataGridView3.Columns[3].HeaderText = "Datum";
                dataGridView3.Columns[4].HeaderText = "Číslo objednávky";
                dataGridView3.Columns[5].HeaderText = "Kod paletu";
              
                for (int i = 0; i < dataGridView3.Columns.Count - 1; i++)
                {
                    dataGridView3.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.DisplayedCells;
                }
                for (int j = 0; j < TB_Vydej.Rows.Count; j++)
                {
                    vydej += decimal.Parse(TB_Vydej.Rows[j][2].ToString());
                }
            }
            label1Vydej.Text = $"Výdej( do 90 dnů): {vydej.ToString("00")} ks";
        }
        private void Load_PrijemVydej(string _mnb)
        {
            string sqlP = $@"with bang_nhap as
                            (select mr_cproin as kz, mr_adrqre as Dok
                            ,mr_codrec as User_ 
                            ,mr_qtruvc as Mnozstvi   
                            ,mr_datmvt as Datum   
                            ,mr_usscc as Kod_palet
                            ,mr_ncdefo as Cislo_obj
                            from tb_mvtre
                            where mr_cproin='{_mnb}'
                            union all
                            select hr_cproin as kz, hr_adrqre as Dok
                            ,hr_codrec as User_                             
                            ,hr_qtruvc as Mnozstvi
                            ,hr_datmvt as Datum                                                       
                            ,hr_usscc as Kod_palet
                            ,hr_ncdefo as Cislo_obj
                            from tb_hmvtre 
                            where hr_cproin='{_mnb}')
                            select kz,Mnozstvi,Dok,Datum,Cislo_obj,Kod_palet from bang_nhap
                            where   TO_DATE(Datum,'DD-MM-YYYY') >= TO_DATE(SYSDATE-90, 'DD-MM-YYYY') 
                            order by Datum desc";


            string sqlV = $@"WITH bang1
                            AS (SELECT p.[product_code] as kz
                              ,[date_start],t.created as created
                              ,t.[order_number]      --as N'Số đơn'
                              , p.[product_code] --as N'Mã nội bộ'    
                              ,[human_prepared] --as N'Mã NV'
                              , [location_id] --as N'Vị trí'
                              , [quantity_request] --as 'Slg đặt'
                              , [quantity_prepared] --as 'Slg chốt'
                              , CASE WHEN p.typ = 1 THEN N'Nhặt đủ'
                                WHEN p.typ = 3 THEN N'Nhặt thiếu'
                                ELSE N'Hủy lệnh' END AS Status_
                              ,p.[pallet_number]
                            FROM [TDManagement].[dbo].[Location_Order_Picking] p
                            LEFT JOIN [TDManagement].[dbo].[Location_Order_Tourn] t
                              ON t.pallet_number = p.pallet_number
                            LEFT JOIN [TDManagement].[dbo].[Location_Order_Items] i
                              ON t.[order_number] = i.[order_number]
                              AND p.product_code = i.product_code
                            WHERE p.[product_code] = '{_mnb}'  )               

                            SELECT 
                              kz,
                              isnull([quantity_request],[quantity_prepared]) as Objednano,
                              [quantity_prepared] as Dodano,
                              FORMAT(DATEADD(D, 0, SUBSTRING([date_start], 1, 8)), 'dd-MM-yyyy') + ' ' + SUBSTRING([date_start], 9, 2) + ':' + SUBSTRING([date_start], 11, 2) + ':' + SUBSTRING([date_start], 13, 2) AS N'Datum',
                              [order_number] as Objednavka,                           
                              [pallet_number] as Palet_kod
                            FROM bang1 where datediff(day,getdate(),created)>=-30
                            ORDER BY date_start DESC ";
            //UNION ALL
            //    SELECT p.[product_code] as kz
            //      ,[date_start]
            //      ,t.[order_number]      --as N'Số đơn'
            //      ,p.[product_code] --as N'Mã nội bộ'	
            //      ,[human_prepared] --as N'Mã NV'
            //      ,[location_id] --as N'Vị trí'
            //      ,[quantity_request] --as 'Slg đặt'
            //      ,[quantity_prepared] --as 'Slg chốt'
            //      ,CASE
            //        WHEN p.typ = 1 THEN N'Nhặt đủ'
            //        WHEN p.typ = 3 THEN N'Nhặt thiếu'
            //        ELSE N'Hủy lệnh'
            //      END AS Status_
            //      ,p.[pallet_number]
            //    FROM [TDArchive].[dbo].[Location_Order_Picking] p
            //    LEFT JOIN [TDManagement].[dbo].[Location_Order_Tourn] t
            //      ON t.pallet_number = p.pallet_number
            //    LEFT JOIN [TDManagement].[dbo].[Location_Order_Items] i
            //      ON t.[order_number] = i.[order_number]
            //      AND p.product_code = i.product_code
            //    WHERE p.[product_code] ='{_mnb}')

            try
            {
                TB_Vydej = DataProvider.Instance.ExecuteQuery(sqlV);
                if (_branchToFormSklad == "TK - Hostivice")
                {
                    TB_Prijem = DataOracle.Instance.ExecuteQuery(sqlP);
                }
                else
                {

                    TB_Prijem = DataOracleMorava.Instance.ExecuteQuery(sqlP);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void LoadSkladAllPozice(string _mnb = "")
        {
            string sqlHos = $@"with bangtotal as (
                              SELECT
                              ul_cproin           AS kz
                            , sum(ul_nqtuvc)      AS total                                                 
                        FROM
                            tb_lcums
                            INNER JOIN tb_eums ON ue_usscc = ul_usscc
                            INNER JOIN tb_art ON ul_cproin = ar_cproin
                        WHERE ul_nqtuvc is not null and ue_codtsu != 3
                         and substr(ue_adrums, 1, 3) != '10E' group by ul_cproin)
                        SELECT
                              ul_cproin            AS kod_zbozi
                            , ar_libpro            AS nazev
                            , ue_adrums            AS pozice                             
                            , ul_nqtuvc            AS mnozstvi
                            ,total
                            , ue_usscc as kod_pallet                           
                        FROM
                            tb_lcums
                            INNER JOIN tb_eums ON ue_usscc = ul_usscc
                            INNER JOIN tb_art ON ul_cproin = ar_cproin
                            INNER JOIN bangtotal on ul_cproin=kz                    
                        WHERE ul_nqtuvc is not null and ue_codtsu != 3
                         and substr(ue_adrums, 1, 3) != '10E'
                         and ul_cproin like '%{_mnb}%' and Rownum<=200
                        ORDER BY  ue_adrums ";

            string sqlDcBrno = $@"with bangtotal as (
                              SELECT
                              ul_cproin           AS kz
                            , sum(ul_nqtuvc)      AS total                                                 
                        FROM
                            tb_lcums
                            INNER JOIN tb_eums ON ue_usscc = ul_usscc
                            INNER JOIN tb_art ON ul_cproin = ar_cproin
                        WHERE ul_nqtuvc is not null and ue_codtsu != 3
                         and substr(ue_adrums, 1, 3) != '10E' group by ul_cproin)
                        SELECT
                              ul_cproin            AS kod_zbozi
                            , ar_libpro            AS nazev
                            , ue_adrums            AS pozice                             
                            , ul_nqtuvc            AS mnozstvi
                            ,total
                            , ue_usscc as kod_pallet                           
                        FROM
                            tb_lcums
                            INNER JOIN tb_eums ON ue_usscc = ul_usscc
                            INNER JOIN tb_art ON ul_cproin = ar_cproin
                            INNER JOIN bangtotal on ul_cproin=kz  
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
                TB_left = DataOracleMorava.Instance.ExecuteQuery(sqlDcBrno);
            }

        }
        private void pictureBox1_Click(object sender, EventArgs e) //cap nhat lai
        {
            if (!worker_left.IsBusy)
            {
                placeHolderTextBox1.Text = "";
                progressBar_left.Visible = true;
                progressBar_left.Style = ProgressBarStyle.Marquee;
                progressBar_left.MarqueeAnimationSpeed = 1;
                worker_left.RunWorkerAsync();
            }
        }
       
        private void Find()
        {
            if (!worker_left.IsBusy)
            {
                _kod_zbozi = placeHolderTextBox1.Text;
                progressBar_left.Visible = true;
                progressBar_left.Style = ProgressBarStyle.Marquee;
                progressBar_left.MarqueeAnimationSpeed = 1;
                worker_left.RunWorkerAsync();                
                if (!worker_right.IsBusy)
                {
                    progressBar_right.Visible = true;
                    progressBar_right.Style = ProgressBarStyle.Marquee;
                    progressBar_right.MarqueeAnimationSpeed = 1;
                    worker_right.RunWorkerAsync();
                }
            }
        }
        private void dataGridView1_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                DataGridViewRow row = new DataGridViewRow();
                row = dataGridView1.Rows[e.RowIndex];
                _kod_zbozi = row.Cells[0].Value.ToString();
                label3Celkem.Text = "";
                label3Celkem.Text = $"Konečný stav: {row.Cells[4].Value.ToString()} ks";
                this.Text = $"PVMaster Sklad {_branchToFormSklad} --- zboží: {row.Cells[0].Value.ToString()} {row.Cells[1].Value.ToString()}";
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
                    DialogResult dialogResult = MessageBox.Show("Export to file Excel?", "Warning", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question, MessageBoxDefaultButton.Button3);
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
        private void pictureBox4_Click(object sender, EventArgs e)
        {
            string filePath = "";
            SaveFileDialog dialog = new SaveFileDialog(); // tạo SaveFileDialog để lưu file excel 
            dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";  // chỉ lọc ra các file có định dạng Excel
            dialog.FileName = "Report " + DateTime.Now.ToString("ddMMyyHHmmss");

            if (dialog.ShowDialog() == DialogResult.OK)            // Nếu mở file và chọn nơi lưu file thành công sẽ lưu đường dẫn lại dùng
            {
                filePath = dialog.FileName;
            }

            if (string.IsNullOrEmpty(filePath))// nếu đường dẫn null hoặc rỗng thì báo không hợp lệ và return hàm
            {
                MessageBox.Show("Path not found!");
                return;
            }

            try
            {
                using (ExcelPackage p = new ExcelPackage())
                {
                    p.Workbook.Properties.Author = "Tran Ngoc Hai";// đặt tên người tạo file                    
                    p.Workbook.Properties.Title = "NV";// đặt tiêu đề cho file 

                    #region Sheet1
                    p.Workbook.Worksheets.Add("sheet1");  //Tạo một sheet để làm việc trên đó                   
                    ExcelWorksheet ws1 = p.Workbook.Worksheets[1]; // lấy sheet vừa add ra để thao tác
                    ws1.Name = "Konečný stav";  // đặt tên cho sheet                 
                    ws1.Cells.Style.Font.Size = 11;   // fontsize mặc định cho cả sheet                
                    ws1.Cells.Style.Font.Name = "Calibri";    // font family mặc định cho cả sheet
                    ws1.Row(1).Height = 3 * ws1.Row(1).Height;
                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            if (dataGridView1.Rows[i].Cells[j].Value != null)
                            {
                                ws1.Cells[i + 2, j + 1].Value = dataGridView1.Rows[i].Cells[j].Value.ToString();
                                ws1.Cells[i + 2, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws1.Cells[i + 2, j + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            }
                        }
                    }
                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        ws1.Cells[1, i + 1].Value = dataGridView1.Columns[i].HeaderText;
                        ws1.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws1.Cells[1, i + 1].Style.WrapText = true;
                        ws1.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws1.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        ws1.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        ws1.Column(i + 1).AutoFit();
                    }
                    #endregion

                    #region Sheet2
                    p.Workbook.Worksheets.Add("sheet2");  //Tạo một sheet để làm việc trên đó                   
                    ExcelWorksheet ws2 = p.Workbook.Worksheets[2]; // lấy sheet vừa add ra để thao tác
                    ws2.Name = "Příjem (do 90 dnů)";  // đặt tên cho sheet                 
                    ws2.Cells.Style.Font.Size = 11;   // fontsize mặc định cho cả sheet                
                    ws2.Cells.Style.Font.Name = "Calibri";    // font family mặc định cho cả sheet
                    ws2.Row(1).Height = 3 * ws2.Row(1).Height;
                    for (int i = 0; i < dataGridView2.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView2.Columns.Count; j++)
                        {
                            if (dataGridView2.Rows[i].Cells[j].Value != null)
                            {
                                ws2.Cells[i + 2, j + 1].Value = dataGridView2.Rows[i].Cells[j].Value.ToString();
                                ws2.Cells[i + 2, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws2.Cells[i + 2, j + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            }
                        }
                    }
                    for (int i = 0; i < dataGridView2.Columns.Count; i++)
                    {
                        ws2.Cells[1, i + 1].Value = dataGridView2.Columns[i].HeaderText;
                        ws2.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws2.Cells[1, i + 1].Style.WrapText = true;
                        ws2.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws2.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        ws2.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        ws2.Column(i + 1).AutoFit();
                    }
                    #endregion

                    #region Sheet3
                    p.Workbook.Worksheets.Add("sheet3");  //Tạo một sheet để làm việc trên đó                   
                    ExcelWorksheet ws3 = p.Workbook.Worksheets[3]; // lấy sheet vừa add ra để thao tác
                    ws3.Name = "Výdej (do 90 dnů)";  // đặt tên cho sheet                 
                    ws3.Cells.Style.Font.Size = 11;   // fontsize mặc định cho cả sheet                
                    ws3.Cells.Style.Font.Name = "Calibri";    // font family mặc định cho cả sheet
                    ws3.Row(1).Height = 3 * ws3.Row(1).Height;
                    for (int i = 0; i < dataGridView3.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView3.Columns.Count; j++)
                        {
                            if (dataGridView3.Rows[i].Cells[j].Value != null)
                            {
                                ws3.Cells[i + 2, j + 1].Value = dataGridView3.Rows[i].Cells[j].Value.ToString();
                                ws3.Cells[i + 2, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws3.Cells[i + 2, j + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            }
                        }
                    }
                    for (int i = 0; i < dataGridView3.Columns.Count; i++)
                    {
                        ws3.Cells[1, i + 1].Value = dataGridView3.Columns[i].HeaderText;
                        ws3.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        ws3.Cells[1, i + 1].Style.WrapText = true;
                        ws3.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws3.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        ws3.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                        ws3.Column(i + 1).AutoFit();
                    }
                    #endregion



                    Byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
                Process.Start(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Có lỗi khi lưu file!");
            }
        }

        private void placeHolderTextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode==Keys.Enter)
            {
                Find();          
            }
        }
        private void pictureBox2_Click(object sender, EventArgs e)
        {
            Find();
        }
       
    }
}
