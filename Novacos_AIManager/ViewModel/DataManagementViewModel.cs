using Novacos_AIManager.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacos_AIManager.ViewModel
{
    public class DataManagementViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        void OnPropertyChanged(string name) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public string Title { get; set; }
        public string Column2Name { get; set; }

        private readonly Func<ObservableCollection<dynamic>> _reloadData;

        // 전체 데이터 (폴더 등에서 가져온 것)
        private ObservableCollection<dynamic> _items;
        public ObservableCollection<dynamic> Items
        {
            get => _items;
            private set { _items = value; OnPropertyChanged(nameof(Items)); }
        }

        // 필터링된 데이터 (DataGrid에 표시)
        private ObservableCollection<dynamic> _filteredItems;
        public ObservableCollection<dynamic> FilteredItems
        {
            get => _filteredItems;
            set { _filteredItems = value; OnPropertyChanged(nameof(FilteredItems)); }
        }

        // 날짜 선택
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Now;

        // 조회 버튼 Command
        public RelayCommand SearchCommand { get; set; }

        public DataManagementViewModel(
            string title,
            string col2,
            ObservableCollection<dynamic> data,
            Func<ObservableCollection<dynamic>> reloadData = null)
        {
            Title = title;
            Column2Name = col2;
            _reloadData = reloadData;

            Items = data ?? new ObservableCollection<dynamic>();

            // 조회 버튼 연결
            SearchCommand = new RelayCommand(o => ApplyFilter());

            // 초기에는 전체 데이터 표시
            FilteredItems = new ObservableCollection<dynamic>(Items);

            // 빈 리스트
            //FilteredItems = new ObservableCollection<dynamic>();  
        }

        // 🔥 조회 버튼 누르면 실행되는 필터 함수
        public void ApplyFilter()
        {
            if (_reloadData != null)
            {
                Items = _reloadData() ?? new ObservableCollection<dynamic>();
            }

            var list = new ObservableCollection<dynamic>();

            foreach (var item in Items)
            {
                if (DateTime.TryParseExact(
                    item.Column2.ToString(),
                    "yyyyMMdd",
                    null,
                    System.Globalization.DateTimeStyles.None,
                    out DateTime fileDate
                ))
                {
                    if (fileDate >= StartDate && fileDate <= EndDate)
                        list.Add(item);
                }
            }

            FilteredItems = list;
        }
    }
}
