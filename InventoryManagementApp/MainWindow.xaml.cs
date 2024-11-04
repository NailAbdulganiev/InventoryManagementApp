using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace InventoryManagementApp
{
    public partial class MainWindow : Window
    {
        private Dictionary<string, InventoryItem> receiveDict = new Dictionary<string, InventoryItem>();
        private Dictionary<string, InventoryItem> shipDict = new Dictionary<string, InventoryItem>();

        public MainWindow()
        {
            InitializeComponent();
            UpdateTables();
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
            if (string.IsNullOrWhiteSpace(input))
            {
                return;
            }

            var ids = input.ToUpper().Split(' ');

            foreach (var id in ids)
            {
                if (!IsValidHex(id))
                {
                    MessageBox.Show($"Неверный идентификатор: {id}. Идентификатор должен содержать 24 HEX символа.");
                    continue;
                }

                InventoryItem item;

                if (!addDict.TryGetValue(id, out item))
                {
                    if (removeDict.TryGetValue(id, out InventoryItem removeItem))
                    {
                        item = new InventoryItem
                        {
                            Id = removeItem.Id,
                            Name = removeItem.Name,
                            Quantity = 1
                        };
                        addDict[id] = item;
                    }
                    else
                    {
                        item = new InventoryItem { Id = id, Name = $"Наименование для {id}", Quantity = 1 };
                        addDict[id] = item;
                    }
                }
                else
                {
                    item.Quantity++;
                }

                if (removeDict.TryGetValue(id, out InventoryItem removeItemToUpdate))
                {
                    removeItemToUpdate.Quantity--;
                    if (removeItemToUpdate.Quantity <= 0)
                    {
                        removeDict.Remove(id);
                    }
                }
            }

            UpdateTables();
        }

        private void ReceiveGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Наименование")
            {
                if (e.Row.Item is InventoryItem item)
                {
                    if (receiveDict.ContainsKey(item.Id))
                    {
                        receiveDict[item.Id].Name = ((System.Windows.Controls.TextBox)e.EditingElement).Text;
                    }
                }
            }
            UpdateTables();
        }

        private void ShipGrid_CellEditEnding(object sender, System.Windows.Controls.DataGridCellEditEndingEventArgs e)
        {
            if (e.Column.Header.ToString() == "Наименование")
            {
                if (e.Row.Item is InventoryItem item)
                {
                    if (shipDict.ContainsKey(item.Id))
                    {
                        shipDict[item.Id].Name = ((System.Windows.Controls.TextBox)e.EditingElement).Text;
                    }
                }
            }
            UpdateTables();
        }

        private bool IsValidHex(string id)
        {
            return id.Length == 24 && id.All(c => Uri.IsHexDigit(c));
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
                writer.WriteLine("Прием");
                foreach (var item in receiveDict.Values)
                {
                    writer.WriteLine($"{item.Id},{item.Name},{item.Quantity}");
                }

                writer.WriteLine("Отгрузка");
                foreach (var item in shipDict.Values)
                {
                    writer.WriteLine($"{item.Id},{item.Name},{item.Quantity}");
                }
            }
        }

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
