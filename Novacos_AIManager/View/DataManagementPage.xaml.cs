using Novacos_AIManager.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Novacos_AIManager.View
{
    /// <summary>
    /// DataManagementPage.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class DataManagementPage : UserControl
    {
        public DataManagementPage(
            string title,
            string column2Name,
            ObservableCollection<dynamic> items,
            Func<ObservableCollection<dynamic>> reloadData = null)
        {
            InitializeComponent();
            DataContext = new DataManagementViewModel(title, column2Name, items, reloadData);
        }

        private void DG_Loaded(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as DataManagementViewModel;
            if (vm == null) return;

            var dg = sender as DataGrid;
            dg.Columns.Clear();

            // Num
            dg.Columns.Add(new DataGridTextColumn
            {
                Header = "Num",
                Binding = new Binding("Num"),
                Width = new DataGridLength(60)
            });

            // Date
            dg.Columns.Add(new DataGridTextColumn
            {
                Header = "Date",
                Binding = new Binding("Date"),
                Width = new DataGridLength(120)
            });

            // FileName
            dg.Columns.Add(new DataGridTextColumn
            {
                Header = "FileName",
                Binding = new Binding("FileName"),
                Width = new DataGridLength(1, DataGridLengthUnitType.Star)
            });

            // Column2 (동적 이름)
            dg.Columns.Add(new DataGridTextColumn
            {
                Header = vm.Column2Name,   // ViewModel에서 받음
                Binding = new Binding("Column2"),
                Width = new DataGridLength(150)
            });

            // CameraType
            dg.Columns.Add(new DataGridTextColumn
            {
                Header = "CameraType",
                Binding = new Binding("CameraType"),
                Width = new DataGridLength(120)
            });
        }
    }
}
