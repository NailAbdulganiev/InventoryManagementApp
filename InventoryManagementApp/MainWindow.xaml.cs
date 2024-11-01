using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, int> receiveDict = new Dictionary<string, int>(); // Словарь для приема
        private Dictionary<string, int> shipDict = new Dictionary<string, int>(); // Словарь для отгрузки

        public MainWindow()
        {
            InitializeComponent();
            UpdateTables();
        }

        // Метод обработки ввода для приема
        private void ReceiveInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ProcessInput(ReceiveInput.Text, receiveDict, shipDict);
                ReceiveInput.Clear();
            }
        }

        // Метод обработки ввода для отгрузки
        private void ShipInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ProcessInput(ShipInput.Text, shipDict, receiveDict);
                ShipInput.Clear();
            }
        }

        // Обработка введенных идентификаторов
        private void ProcessInput(string input, Dictionary<string, int> addDict, Dictionary<string, int> removeDict)
        {
            if (input == "")
            {
                return;
            }

            var ids = input.ToUpper().Split(' ');

            foreach (var id in ids)
            {
                if (addDict.ContainsKey(id))
                {
                    addDict[id]++;
                }
                else
                {
                    addDict[id] = 1;
                }

                if (removeDict.ContainsKey(id))
                {
                    removeDict[id]--;
                    if (removeDict[id] == 0)
                    {
                        removeDict.Remove(id);
                    }
                }
            }

            UpdateTables();
        }

        // Обновление данных
        private void UpdateTables()
        {
            ReceiveGrid.ItemsSource = null;
            ReceiveGrid.ItemsSource = receiveDict.Select(x => new { Name = x.Key, Quantity = x.Value }).ToList();

            ShipGrid.ItemsSource = null;
            ShipGrid.ItemsSource = shipDict.Select(x => new { Name = x.Key, Quantity = x.Value }).ToList();
        }

        // Очистка таблиц
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            receiveDict.Clear();
            shipDict.Clear();
            UpdateTables();
        }

        // Сохранение данных в файл
        private void SaveData(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Прием");
                foreach (var item in receiveDict)
                {
                    writer.WriteLine($"{item.Key},{item.Value}");
                }

                writer.WriteLine("Отгрузка");
                foreach (var item in shipDict)
                {
                    writer.WriteLine($"{item.Key},{item.Value}");
                }
            }
        }

        // Обработчик события нажатия кнопки "Сохранить"
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "InventoryData",
                DefaultExt = ".txt",
                Filter = "Text documents (.txt)|*.txt"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                SaveData(saveFileDialog.FileName);
            }
        }
    }
}

