using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Data;
using System.IO;
using System.Globalization;
// can cai dat them nuget EPPlus 4.5.3.2

namespace PrijemkaHostivice
{
    class ExportDTGVToExcel
    {
        private static ExportDTGVToExcel instance;
        public static ExportDTGVToExcel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ExportDTGVToExcel();
                }
                return ExportDTGVToExcel.instance;
            }

            private set { ExportDTGVToExcel.instance = value; }
        }
        public void ExportToExcel(DataGridView dataGridView1)
        {

            string filePath = "";
            // tạo SaveFileDialog để lưu file excel
            SaveFileDialog dialog = new SaveFileDialog();

            // chỉ lọc ra các file có định dạng Excel
            dialog.Filter = "Excel | *.xlsx | Excel 2003 | *.xls";
            dialog.FileName = "PV Report " + DateTime.Now.ToString("dd.MM.yy HH.mm");


            // Nếu mở file và chọn nơi lưu file thành công sẽ lưu đường dẫn lại dùng
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                filePath = dialog.FileName;
            }

            // nếu đường dẫn null hoặc rỗng thì báo không hợp lệ và return hàm
            if (string.IsNullOrEmpty(filePath))
            {
                MessageBox.Show("Đường dẫn báo cáo không hợp lệ");
                return;
            }

            try
            {
                using (ExcelPackage p = new ExcelPackage())
                {
                    // đặt tên người tạo file
                    p.Workbook.Properties.Author = "Tran Ngoc Hai";

                    // đặt tiêu đề cho file
                    p.Workbook.Properties.Title = "NV";

                    //Tạo một sheet để làm việc trên đó
                    p.Workbook.Worksheets.Add("sheet1");

                    // lấy sheet vừa add ra để thao tác
                    ExcelWorksheet ws = p.Workbook.Worksheets[1];

                    // đặt tên cho sheet
                    ws.Name = "Report " + DateTime.Today.ToString("dd.MM.yy");
                    // fontsize mặc định cho cả sheet
                    ws.Cells.Style.Font.Size = 11;
                    // font family mặc định cho cả sheet
                    ws.Cells.Style.Font.Name = "Calibri";



                    ws.Row(1).Height = 4 * ws.Row(1).Height;
                    //ws.Column(1).Style.Numberformat.Format = "0";
                    //ws.Column(2).Style.Numberformat.Format = "0";

                    for (int i = 0; i < dataGridView1.Rows.Count; i++)
                    {
                        for (int j = 0; j < dataGridView1.Columns.Count; j++)
                        {
                            if (dataGridView1.Rows[i].Cells[j].Value != null&& dataGridView1.Columns[j].Visible)
                            {
                                ws.Cells[i + 2, j + 1].Value = dataGridView1.Rows[i].Cells[j].Value.ToString();
                                ws.Cells[i + 2, j + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                                ws.Cells[i + 2, j + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            }
                            
                        }
                    }

                    for (int i = 0; i < dataGridView1.Columns.Count; i++)
                    {
                        if (dataGridView1.Columns[i].Visible)
                        {
                            ws.Cells[1, i + 1].Value = dataGridView1.Columns[i].HeaderText;
                            ws.Cells[1, i + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                            ws.Cells[1, i + 1].Style.WrapText = true;
                            ws.Cells[1, i + 1].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                            ws.Cells[1, i + 1].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                            ws.Cells[1, i + 1].Style.Border.BorderAround(ExcelBorderStyle.Thin, System.Drawing.Color.Black);
                            ws.Column(i + 1).AutoFit();
                        }
                       

                    }

                    Byte[] bin = p.GetAsByteArray();
                    File.WriteAllBytes(filePath, bin);
                }
                MessageBox.Show("Xuất excel thành công!");
                System.Diagnostics.Process.Start(filePath); 
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString(), "Có lỗi khi lưu file!");
            }
        }

    }
}
