using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace PR1_2
{
    public partial class TextRedactor : Form
    {
        // Импорт функции для работы с сообщениями Windows
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        // Константы для управления Undo
        private const int WM_SETREDRAW = 0x000B;
        private const int EM_SETUNDOLIMIT = 0x00C0; // Устанавливает лимит шагов отмены
        private const int EM_STOPGROUPTYPING = 0x0448; // Останавливает группировку ввода

        private static int _openCount = 0;
        private string _currentFilePath = null;

        private bool _isModified = false;
        private bool _isLoading = false;
        private Timer _autoSaveTimer;
        private bool _autoSaveEnabled = false;

        private int printLineIndex = 0;

        public TextRedactor()
        {
            // задание шрифтов, отступов, цвета, инициализация таймера, автосохранение, подсказки
            
            InitializeComponent();

            // Увеличиваем лимит шагов отмены (чтобы хватило на весь текст)
            // wParam = 1000 (максимальное количество шагов), lParam = 0
            SendMessage(redactor.Handle, EM_SETUNDOLIMIT, (IntPtr)1000, IntPtr.Zero);

            // границы панели для текста
            panel1.BackColor = Color.White;
            panel1.Padding = new Padding(0, 20, 0, 20);

            //настройки richtextbox
            redactor.Dock = DockStyle.Fill;
            redactor.BorderStyle = BorderStyle.None;
            redactor.BackColor = Color.White;
            redactor.SelectionIndent = 75;         // Отступ всего абзаца слева
            redactor.SelectionRightIndent = 75;    // Отступ справа
            redactor.SelectionHangingIndent = -25; // Реализация красной строки
            redactor.Font = new Font("Times New Roman", 14);
            redactor.ForeColor = Color.Black;

            panel1.BorderStyle = BorderStyle.FixedSingle;

            panel2.BackColor = ColorTranslator.FromHtml("#ffffff");
            panel2.BorderStyle = BorderStyle.FixedSingle;

            this.Resize += TextRedactor_Resize; // адаптивное расположение redactor
            this.BackColor = ColorTranslator.FromHtml("#eceaea");
            this.MinimumSize = new Size(800, 600);

            _autoSaveTimer = new System.Windows.Forms.Timer();
            _autoSaveTimer.Interval = 10000; // 10 секунд
            _autoSaveTimer.Tick += AutoSaveTimer_Tick;

            _openCount++;
            SetupPlaceholder(txtBox2, "Найти");
            SetupPlaceholder(txtBox1, "Заменить");

            redactor.TextChanged += redactor_TextChanged;

            TextRedactor_Resize(this, EventArgs.Empty);

            //наведение курсора
            object[,] helpMsg = {
            { btnUndo, "Отменить" },
            { btnUndo, "Отменить" },
            { btnRedo, "Повторить" },
            { button1, "Открыть новое окно" },
            { button4, "Создать новый документ" },
            { button7, "Открыть файл" },
            { button6, "Сохранить файл" },
            { button8, "Печать документа" },
            { button3, "Найти текст" },
            { button2, "Заменить текст" },
            { button9, "Выбрать цвет текста" },
            { ccFont, "Выбрать шрифт" },
            { ccSize, "Выбрать размер шрифта" }
            };

            for (int i = 0; i < helpMsg.GetLength(0); i++)
            {
                toolTip1.SetToolTip((Control)helpMsg[i, 0], (string)helpMsg[i, 1]);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // загрузка окна, заполнение выпадающих списков

            TextRedactor_Resize(this, EventArgs.Empty);

            ccFont.Items.Clear();

            // Все установленные шрифты
            foreach (var family in FontFamily.Families)
                ccFont.Items.Add(family.Name);
            ccFont.Text = redactor.Font.Name;

            // Размеры текстов
            ccSize.Items.Clear();
            int[] sizes = { 8, 9, 10, 11, 12, 14, 16, 18, 20, 24, 28, 32, 36, 48, 72 };
            foreach (var s in sizes)
                ccSize.Items.Add(s.ToString());
            ccSize.Text = ((int)redactor.Font.Size).ToString();

            ccFont.Text = redactor.Font.Name;
            ccSize.Text = redactor.Font.Size.ToString();
        }

        private void UpdateTitle()
        {
            // обновляет заголовок окна, меняет статус галочка\крестик  

            // Формируем имя файла для заголовка
            string fileName = _currentFilePath != null ? System.IO.Path.GetFileName(_currentFilePath) : "Безымянный";

            // Обновляем заголовок окна 
            this.Text = (_isModified ? "*" : "") + fileName + " — Текстовый редактор";

            // Обновляем Label со статусом
            if (_isModified)
            {
                labelStatus.Text = "✖";   
                labelStatus.ForeColor = Color.Red;
            }
            else
            {
                labelStatus.Text = "✔";   
                labelStatus.ForeColor = Color.Green;
            }
        }

        // Подсчет количества непересекающихся вхождений
        private int CountOccurrences(string text, string pattern)
        {
            if (string.IsNullOrEmpty(pattern)) return 0;

            int count = 0;
            int index = 0;
            while ((index = text.IndexOf(pattern, index, StringComparison.Ordinal)) != -1)
            {
                count++;
                index += pattern.Length; // сдвиг за найденное (непересекающиеся)
            }
            return count;
        }

        private void redactor_TextChanged(object sender, EventArgs e)
        {
            // Разрываем группировку ввода, чтобы Undo отменял по одному символу
            SendMessage(redactor.Handle, EM_STOPGROUPTYPING, IntPtr.Zero, IntPtr.Zero);

            // поле редактирования текста
            if (!_isLoading)
            {
                _isModified = true;  
                UpdateTitle();   
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            // закрытие окон и завершение выполения кода
            _openCount--;

            if (_autoSaveTimer != null)
            {
                _autoSaveTimer.Stop();
                _autoSaveTimer.Dispose();
            }

            base.OnFormClosed(e);

            // Если закрыто последнее окно — принудительно завершаем процесс
            if (_openCount <= 0)
            {
                Environment.Exit(0);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // Создание новых окон
            TextRedactor newWindow = new TextRedactor();

            newWindow.Show();
        }
        private void openFileDialog1_FileOk(object sender, CancelEventArgs e) { } //открытие файла

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e) { } //сохранение

        private void panel1_Paint(object sender, PaintEventArgs e) { } // панель для richtextbox

        private void panel2_Paint(object sender, PaintEventArgs e) { } // панель инструментов

        private void labelStatus_Click(object sender, EventArgs e) { } // Индикатор сохранения файла

        private void txtBox1_TextChanged(object sender, EventArgs e) { } // Найти

        private void txtBox2_TextChanged(object sender, EventArgs e) { } //Заменить

        private void toolTip1_Popup(object sender, PopupEventArgs e) { } // комментарий при навдеении

    }
}
