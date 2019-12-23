using iSpeak.Models;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Web;

namespace iSpeak.Common
{
    public class ExportExcel
    {
        #region SupportCode ExportToExcel
        private static void SetWorkbookProperties(ExcelPackage p)
        {
            //Here setting some document properties
            p.Workbook.Properties.Author = "Harry Agung S";
            p.Workbook.Properties.Title = "Export to Excel";
        }

        private static ExcelWorksheet CreateSheet(ExcelPackage p, string sheetName)
        {
            p.Workbook.Worksheets.Add(sheetName);
            ExcelWorksheet ws = p.Workbook.Worksheets[1];
            ws.Name = sheetName; //Setting Sheet's name
            ws.Cells.Style.Font.Size = 11; //Default font size for whole sheet
            ws.Cells.Style.Font.Name = "Calibri"; //Default Font name for whole sheet

            return ws;
        }
        #endregion

        public ExcelPackage StockOnHand(string branch, List<StockOnHandViewModels> models)
        {
            ExcelPackage p = new ExcelPackage();

            //set the workbook properties and add a default sheet in it
            SetWorkbookProperties(p);
            //Create a sheet
            ExcelWorksheet ws = CreateSheet(p, "Sheet1");

            //setting width column
            ws.Column(1).Width = 10;
            ws.Column(2).Width = 70;
            ws.Column(3).Width = 20;
            ws.Column(4).Width = 20;

            ws.Cells[1, 1].Value = "INVENTORY STOCK ON HAND";
            ws.Cells[1, 1, 1, 4].Merge = true;
            ws.Cells[1, 1].Style.Font.Size = 18;
            ws.Cells[1, 1].Style.Font.Bold = true;
            ws.Cells[1, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            ws.Cells[3, 1].Value = "Branch";
            ws.Cells[3, 2].Value = branch;
            ws.Cells[3, 2].Style.Font.Bold = true;

            ws.Cells[4, 1].Value = "NO.";
            ws.Cells[4, 2].Value = "PRODUCT";
            ws.Cells[4, 3].Value = "QTY";
            ws.Cells[4, 4].Value = "UNIT";
            ws.Cells[4, 1, 4, 4].Style.Font.Bold = true;
            ws.Cells[4, 1, 4, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            var headerBorder = ws.Cells[4, 1, 4, 4].Style.Border;
            headerBorder.Left.Style = headerBorder.Top.Style = headerBorder.Right.Style = headerBorder.Bottom.Style = ExcelBorderStyle.Thin;
            var headerFill = ws.Cells[4, 1, 4, 4].Style.Fill;
            headerFill.PatternType = ExcelFillStyle.Solid;
            headerFill.BackgroundColor.SetColor(Color.LightGreen);

            ws.View.FreezePanes(5, 1);
            int rowIndex = 5;
            if (models.Count > 0)
            {
                int rowCount = 1;
                foreach (var item in models.OrderBy(x => x.Product))
                {
                    ws.Cells[rowIndex, 1].Value = rowCount;
                    ws.Cells[rowIndex, 2].Value = item.Product;
                    ws.Cells[rowIndex, 3].Value = item.Qty;
                    ws.Cells[rowIndex, 4].Value = item.Unit;

                    var cellBorder = ws.Cells[rowIndex, 1, rowIndex, 4].Style.Border;
                    cellBorder.Left.Style = cellBorder.Top.Style = cellBorder.Right.Style = cellBorder.Bottom.Style = ExcelBorderStyle.Thin;

                    rowIndex++; rowCount++;
                }
            }
            else
            {
                ws.Cells[rowIndex, 1].Value = "None of Data Available";
                ws.Cells[rowIndex, 1, rowIndex, 4].Merge = true;
                var cellBorder = ws.Cells[rowIndex, 1, rowIndex, 4].Style.Border;
                cellBorder.Left.Style = cellBorder.Top.Style = cellBorder.Right.Style = cellBorder.Bottom.Style = ExcelBorderStyle.Thin;
                rowIndex++;
            }

            return p;
        }
    }
}