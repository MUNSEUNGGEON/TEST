using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Novacos_AIManager.ViewModel
{
    public class WebPageViewModel : INotifyPropertyChanged
    {
        public bool IsLoginMode { get; set; } = false;

        public string StartUrl { get; set; }
        public string MainUrl { get; set; }

        public string LoginId { get; set; }
        public string LoginPw { get; set; }

        public string IdElement { get; set; }
        public string PwElement { get; set; }
        public string ButtonElement { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
