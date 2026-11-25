using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

// Работа с файлами: открытие, создание, сохранение, автосохранение

namespace PR1_2
{
    public partial class TextRedactor : Form
    {
        private void button4_Click_1(object sender, EventArgs e) // Создать
        {
            // создание пустого документа

            // защита
            _isLoading = true;

            redactor.Clear();
            redactor.Font = new Font("Times New Roman", 14);
            redactor.Update();

            _isLoading = false;

            _currentFilePath = null;
            _autoSaveEnabled = false;
            _autoSaveTimer.Stop();

            _isModified = false;
            UpdateTitle(); // ставим галочку
        }

        private void button7_Click_1(object sender, EventArgs e) // Открыть
        {
            // открытие нового файла + диалоговое окно

            openFileDialog1.Filter = "Текстовые файлы (*.rtf)|*.rtf|Все файлы (*.*)|*.*";
            openFileDialog1.Title = "Выберите текстовый файл";
            openFileDialog1.CheckFileExists = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    _isLoading = true;

                    redactor.LoadFile(openFileDialog1.FileName, RichTextBoxStreamType.RichText);

                    _isLoading = false;
                    _currentFilePath = openFileDialog1.FileName;
                    _isModified = false;
                    UpdateTitle();

                    _autoSaveEnabled = true;
                    _autoSaveTimer.Start();

                }
                catch (Exception ex)
                {
                    _isLoading = false;

                    MessageBox.Show("Ошибка при чтении файла: " + ex.Message, "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button6_Click_1(object sender, EventArgs e) 
        {
            //  Сохранение документа

            if (SaveDocument(true))
            {
                // Показываем сообщение только если это ручное сохранение с новым файлом
                if (string.IsNullOrEmpty(_currentFilePath) == false)
                    MessageBox.Show("Файл сохранён", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private bool SaveDocument(bool showDialogIfNeeded)
        {
            // Пособник сохранения файла

            // Если путь неизвестен — нужен диалог
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                if (!showDialogIfNeeded) return false; // при автосохранении без имени файла пропускаем

                saveFileDialog1.Filter = "RTF файлы (*.rtf)|*.rtf|Все файлы (*.*)|*.*";
                saveFileDialog1.Title = "Сохранить файл";
                saveFileDialog1.DefaultExt = "rtf";

                if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    _currentFilePath = saveFileDialog1.FileName;
                }
                else { return false; }
            }

            // сохраняем без диалога
            try
            {
                redactor.SaveFile(_currentFilePath, RichTextBoxStreamType.RichText);
                _isModified = false;
                UpdateTitle();
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void AutoSaveTimer_Tick(object sender, EventArgs e)
        {
            // Прикол автосохранения

            // Сохраняем только если файл был изменён и у него есть путь
            if (_autoSaveEnabled && _isModified && !string.IsNullOrEmpty(_currentFilePath))
                SaveDocument(false);
        }
    }
}