using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JiME
{
    public class Title: INotifyPropertyChanged
    {
        int _id;
        public int collection;
        string _dataName;
        public string originalName;

        public Title() { }

        public Title(int id)
        {
            this.id = id;
        }

        public int id
        {
            get => _id;
            set
            {
                if (value != _id)
                {
                    _id = value;
                    NotifyPropertyChanged("id");
                }
            }
        }

        public string dataName
        {
            get => _dataName;
            set
            {
                if (value != _dataName)
                {
                    _dataName = value;
                    NotifyPropertyChanged("dataName");
                }
            }
        }

        [JsonIgnore]
        public string titleName
        {
            get => ("Title " + id + ": " + dataName);
        }


        public override string ToString()
        {
            return dataName;
        }

        public void NotifyPropertyChanged(string propName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
