using System;
using System.Windows.Forms;

namespace PR1_2
{
    internal static class Program
    {
        [STAThread]
        
        static void Main()
        {
            // точка входа
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Создаем и показываем первое окно
            new TextRedactor().Show();

            // Запускаем цикл сообщений, чтобы при закрытии главного окна не завершалось выполнение кода
            Application.Run();
        }
    }
}