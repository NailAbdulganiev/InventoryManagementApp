using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, InventoryItem> items = new Dictionary<string, InventoryItem>();
        private Dictionary<string, InventoryItem> receiveDict = new Dictionary<string, InventoryItem>();
        private Dictionary<string, InventoryItem> shipDict = new Dictionary<string, InventoryItem>();

        public MainWindow()
        {
            InitializeComponent();
            LoadData();
            UpdateTables();
        }
        // Справочник номенклатуры
        private void LoadData()
        {
            items["304DB75F196000180001C13A"] = new InventoryItem { Id = "304DB75F196000180001C13A", Name = "Объект 1", Quantity = 0 };
            items["304DB75F196000180001C13B"] = new InventoryItem { Id = "304DB75F196000180001C13B", Name = "Объект 2", Quantity = 0 };
            items["304DB75F196000180001C13C"] = new InventoryItem { Id = "304DB75F196000180001C13C", Name = "Объект 3", Quantity = 0 };
            items["304DB75F196000180001C13D"] = new InventoryItem { Id = "304DB75F196000180001C13D", Name = "Объект 3", Quantity = 0 };
            items["304DB75F196000180001C13E"] = new InventoryItem { Id = "304DB75F196000180001C13E", Name = "Объект 4", Quantity = 0 };
            items["304DB75F196000180001C13F"] = new InventoryItem { Id = "304DB75F196000180001C13F", Name = "Объект 1", Quantity = 0 };
        }

        private void ReceiveInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ProcessInput(ReceiveInput.Text, receiveDict, shipDict);
                ReceiveInput.Clear();
            }
        }

        private void ShipInput_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                ProcessInput(ShipInput.Text, shipDict, receiveDict);
                ShipInput.Clear();
            }
        }

        private void ProcessInput(string input, Dictionary<string, InventoryItem> addDict, Dictionary<string, InventoryItem> removeDict)
        {
            if (string.IsNullOrWhiteSpace(input)) return;

            var ids = input.ToUpper().Split(' ');

            foreach (var id in ids)
            {
                if (!IsValidHex(id))
                {
                    MessageBox.Show($"Неверный идентификатор: {id}. Идентификатор должен содержать 24 HEX символа.");
                    continue;
                }

                if (!items.TryGetValue(id, out InventoryItem item))
                {
                    MessageBox.Show($"Идентификатор {id} отсутствует в справочнике.");
                    continue;
                }

                if (!addDict.TryGetValue(id, out InventoryItem existingItem))
                {
                    addDict[id] = new InventoryItem { Id = item.Id, Name = item.Name, Quantity = 1 };
                }
                else
                {
                    existingItem.Quantity++;
                }

                if (removeDict.TryGetValue(id, out InventoryItem removeItem))
                {
                    removeItem.Quantity--;
                    if (removeItem.Quantity <= 0)
                    {
                        removeDict.Remove(id);
                    }
                }
            }

            UpdateTables();
        }

        private bool IsValidHex(string id)
        {
            return id.Length == 24 && id.All(Uri.IsHexDigit);
        }

        private void UpdateTables()
        {
            var groupedReceiveItems = receiveDict.Values
                .GroupBy(item => item.Name)
                .Select(group => new InventoryItem
                {
                    Id = string.Join(Environment.NewLine, group.Select(g => g.Id)),
                    Name = group.Key,
                    Quantity = group.Sum(g => g.Quantity)
                }).ToList();

            var groupedShipItems = shipDict.Values
                .GroupBy(item => item.Name)
                .Select(group => new InventoryItem
                {
                    Id = string.Join(Environment.NewLine, group.Select(g => g.Id)),
                    Name = group.Key,
                    Quantity = group.Sum(g => g.Quantity)
                }).ToList();

            ReceiveGrid.ItemsSource = groupedReceiveItems;
            ShipGrid.ItemsSource = groupedShipItems;
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            receiveDict.Clear();
            shipDict.Clear();
            UpdateTables();
        }

        private void SaveData(string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath))
            {
                writer.WriteLine("Справочник номенклатуры:");
                foreach (var item in items.Values)
                {
                    writer.WriteLine($"{item.Id},{item.Name},{item.Quantity}");
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.SaveFileDialog saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = "InventoryDictionary",
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
