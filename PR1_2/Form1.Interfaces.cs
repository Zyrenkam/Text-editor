using System;
using System.Drawing;
using System.Windows.Forms;

// адаптивное меню и подстройка объектов под параметры экрана

namespace PR1_2
{
    public partial class TextRedactor : Form
    {
        private void TextRedactor_Resize(object sender, EventArgs e)
        {
            // адаптивное изменение объектов приложения

            // Считаем 1% от ширины окна и 10
            int margin1 = (int)(this.ClientSize.Width * 0.1);
            int margin2 = (int)(this.ClientSize.Width * 0.01);

            int topMargin = 150;
            int bottomMargin = 10;

            // Задаем новые координаты и размеры для RichTextBox
            panel1.Left = margin1;
            panel1.Width = this.ClientSize.Width - (margin1 * 2);
            panel1.Top = topMargin;
            panel1.Height = this.ClientSize.Height - topMargin - bottomMargin;


            panel2.Left = margin2;
            panel2.Width = this.ClientSize.Width - (margin2 * 2);
            panel2.Top = 20;
            panel2.Height = 85;

            // Центрирование
            int btnSize = 45;
            int ots = 10; // Отступ между кнопками

            // общ ширина объектов
            int fileGroupWidth = (7 * btnSize) + (6 * ots);
            int searchGroupWidth = 20 + 135;
            int fontGroupWidth = 20 + 170 + 10 + 30;
            int statusWidth = 20 + 30;

            int totalWidth = fileGroupWidth + searchGroupWidth + fontGroupWidth + statusWidth;

            // начало Х координаты
            int startX = (panel2.Width - totalWidth) / 2;
            if (startX < ots) startX = ots;

            int currentX = startX;

            Control[] fileButtons = { btnUndo, btnRedo, button1, button4, button7, button6, button8 };
            foreach (var btn in fileButtons)
            {
                btn.Width = btnSize;
                btn.Height = btnSize;
                btn.Left = currentX;
                btn.Top = (panel2.Height - btnSize) / 2;
                currentX += btnSize + ots;
            }

            // объекты поиск и замена
            currentX += 20; 

            button3.Width = 30;
            button3.Height = 30;
            button3.Left = currentX;
            button3.Top = ots;

            txtBox2.Left = currentX + 35;
            txtBox2.Top = ots;
            txtBox2.Width = 100;

            button2.Width = 30;
            button2.Height = 30;
            button2.Left = currentX;
            button2.Top = ots + 35;

            txtBox1.Left = currentX + 35;
            txtBox1.Top = ots + 35;
            txtBox1.Width = 100;

            currentX += 30 + 5 + 100;

            // объект шрифта
            currentX += 20; 

            ccSize.Left = currentX;
            ccSize.Top = ots;
            ccSize.Width = 60;

            ccFont.Left = currentX;
            ccFont.Top = ots + 35;
            ccFont.Width = 100;

            currentX += 105;

            // объект цвета и статуса
            button9.Width = 30;
            button9.Height = 30;
            button9.Left = currentX;
            button9.Top = ots + 35;

            labelStatus.AutoSize = false;
            labelStatus.Width = 30;
            labelStatus.Height = 30;
            labelStatus.Left = currentX;
            labelStatus.Top = ots;
            labelStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;

            currentX += 30;

        }

        private void SetupPlaceholder(System.Windows.Forms.TextBox tb, string placeholder)
        {
            // подсказка в полях ввода Найти Заменить

            tb.Text = placeholder;
            tb.ForeColor = Color.Gray;

            tb.Enter += (s, e) =>
            {
                if (tb.Text == placeholder)
                {
                    tb.Text = "";
                    tb.ForeColor = Color.Black;
                }
            };
            tb.Leave += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(tb.Text))
                {
                    tb.Text = placeholder;
                    tb.ForeColor = Color.Gray;
                }
            };
        }
    }
}