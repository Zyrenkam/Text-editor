using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;

namespace PR1_2
{
    public partial class TextRedactor : Form
    {
        private void button8_Click_1(object sender, EventArgs e)
        {
            printLineIndex = 0; // сброс счетчика перед печатью

            PrintPreviewDialog preview = new PrintPreviewDialog();
            preview.Document = printDocument1;
            preview.ShowDialog();

            PrintDialog printDialog = new PrintDialog();
            printDialog.Document = printDocument1;

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                try { printDocument1.Print(); }
                catch (Exception ex) { MessageBox.Show($"Ошибка печати: {ex.Message}"); }
            }
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            int x = e.MarginBounds.Left;
            int y = e.MarginBounds.Top;
            Font printFont = redactor.Font;
            Brush brush = new SolidBrush(redactor.ForeColor);

            float lineHeight = printFont.GetHeight(e.Graphics);
            int linesPerPage = (int)(e.MarginBounds.Height / lineHeight);

            string[] lines = redactor.Text.Split(new[] { '\n' }, StringSplitOptions.None);

            int count = 0;
            // Начинаем с того места, где остановились
            for (int i = printLineIndex; i < lines.Length; i++)
            {
                if (count >= linesPerPage)
                {
                    printLineIndex = i; // Запоминаем, на какой строке остановились
                    e.HasMorePages = true;
                    return;
                }
                e.Graphics.DrawString(lines[i], printFont, brush, x, y);
                y += (int)lineHeight;
                count++;
            }
            e.HasMorePages = false;
            printLineIndex = 0;
        }
    }
}