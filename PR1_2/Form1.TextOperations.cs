using System;
using System.Drawing;
using System.Windows.Forms;

// Работа с текстом: замена, поиск, правки в шрифт, цвет, размер текста, назад, повтор

namespace PR1_2
{
    public partial class TextRedactor : Form
    {
        private void btnUndo_Click(object sender, EventArgs e)
        {
            // Кнопка Назад
            if (redactor.CanUndo)
                redactor.Undo();
        }
        private void btnRedo_Click(object sender, EventArgs e)
        {
            // Кнопка Повтор
            if (redactor.CanRedo)
                redactor.Redo();
        }

        private void button3_Click_1(object sender, EventArgs e)
        {
            // Поиск
            string searchText = txtBox2.Text == "Найти" ? "" : txtBox2.Text;

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Введите текст для поиска", "Поиск",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int startFrom = redactor.SelectionStart + redactor.SelectionLength;
            int found = redactor.Find(searchText, startFrom, RichTextBoxFinds.None);

            if (found == -1)
            {
                int total = CountOccurrences(redactor.Text, searchText);

                if (total == 0)
                {
                    MessageBox.Show($"Текст «{searchText}» не найден", "Поиск",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                found = redactor.Find(searchText, 0, RichTextBoxFinds.None);

                MessageBox.Show($"Достигнут конец документа.\nВсего вхождений: {total}",
                    "Поиск завершён", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            if (found != -1)
            {
                redactor.Select(found, searchText.Length);
                redactor.ScrollToCaret();
                redactor.Focus();
            }
        }
        private void button2_Click(object sender, EventArgs e)
        {
            //заменить + игнор
            string searchText = txtBox2.Text == "Найти" ? "" : txtBox2.Text;
            string replacementText = txtBox1.Text == "Заменить" ? "" : txtBox1.Text;

            if (string.IsNullOrEmpty(searchText))
            {
                MessageBox.Show("Введите текст для поиска", "Замена",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (string.IsNullOrEmpty(replacementText))
            {
                MessageBox.Show("Введите текст для замены", "Замена",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // защита от бесконечного цикла, замена не должна содержать искомое
            if (replacementText.Contains(searchText))
            {
                var r = MessageBox.Show(
                    "Текст поиска и замены совпадают. Возможен бесконечный цикл.\nПродолжить?",
                    "Предупреждение", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
                if (r != DialogResult.Yes) return;
            }

            int count = 0;
            int index = 0;
            while ((index = redactor.Find(searchText, index, RichTextBoxFinds.None)) != -1)
            {
                redactor.Select(index, searchText.Length);
                redactor.SelectedText = replacementText;
                index += replacementText.Length; // сдвиг за замененный текст
                count++;
            }

            if (count == 0)
                MessageBox.Show($"Текст «{searchText}» не найден", "Замена",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                MessageBox.Show($"Заменено вхождений: {count}", "Замена",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

            redactor.Focus();
        }

        private void ccFont_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            // меняет шрифт выделенного текста
            if (ccFont.SelectedItem == null) return;

            string fontName = ccFont.SelectedItem.ToString();

            // сохраняем текущий размер и стиль, меняем только имя
            float size = redactor.SelectionFont?.Size ?? redactor.Font.Size;
            FontStyle style = redactor.SelectionFont?.Style ?? redactor.Font.Style;

            try
            {
                Font newFont = new Font(fontName, size, style);
                redactor.SelectionFont = newFont;
                redactor.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось применить шрифт: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void ccSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            // замена размера выделенного текста 
            if (ccSize.SelectedItem == null) return;

            if (!float.TryParse(ccSize.SelectedItem.ToString(), out float size))
                return;

            string fontName = redactor.SelectionFont?.Name ?? redactor.Font.Name;
            FontStyle style = redactor.SelectionFont?.Style ?? redactor.Font.Style;

            try
            {
                Font newFont = new Font(fontName, size, style);
                redactor.SelectionFont = newFont;
                redactor.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Не удалось изменить размер: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void button9_Click_1(object sender, EventArgs e)
        {
            // изменение цвета

            ColorDialog colorDialog = new ColorDialog();
            colorDialog.Color = redactor.SelectionColor;

            if (colorDialog.ShowDialog() == DialogResult.OK && colorDialog.Color != redactor.SelectionColor)
            {
                redactor.SelectionColor = colorDialog.Color;
            }
        }

    }
}