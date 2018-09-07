using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZhaoBi.Controller.Entity
{
    public class RegisInfo : INotifyPropertyChanged
    {
        private int _index;
        public int Index
        {
            get => _index;
            set
            {
                _index = value;
                Invoke(nameof(Index));
            }
        }
        private string _certifiCatePath;
        public string CertifiCatePath
        {
            get => _certifiCatePath;
            set
            {
                _certifiCatePath = value;
                Invoke(nameof(CertifiCatePath));
            }
        }
        public string Phone { get; set; }
        public string AccessToken { get; set; }
        public string Authorization { get; set; }
        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                Invoke(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void Invoke(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
