using System;
using System.IO;
using System.Collections.Generic;
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
using System.Threading;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Threading;
using System.Collections.Specialized;
using System.Xml.Serialization;
using System.Windows.Media.Animation;

namespace Copy_Master
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public ObservableCollection<string> CopiedList { get; private set; }
        int m_selectionSetCount = 0;

        //private static object _lock = new object();
     
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
            CopiedList = new ObservableCollection<string>();
            CopiedList.CollectionChanged += ContentCollectionChanged;
          //  BindingOperations.EnableCollectionSynchronization(CopiedList, _lock);

            var dispatcher = Dispatcher.CurrentDispatcher;
            ThreadStart start = () => runner(dispatcher, CopiedList);
            var t = new Thread(start);
            t.SetApartmentState(ApartmentState.STA);
            t.Start(); 
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public static void runner(Dispatcher dispatcher, ObservableCollection<string> List)
        {
            while (true)
            {
                if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C))
                {
                    try
                    {
                        string copied_txt = Clipboard.GetText();
                        string str = List.FirstOrDefault(x => x.Equals(copied_txt));
                        if (str == null && !copied_txt.Equals("")) 
                        {
                            Action add_copied = () => List.Add(copied_txt);
                            dispatcher.Invoke(add_copied);
                        }
                    }
                    catch(Exception e)
                    {
                         Console.Write(e.HResult);
                    }
                }
            }
        }

        //public static void runner(ObservableCollection<string> List)
        //{
        //    while(true)
        //    {
        //        if (Keyboard.IsKeyDown(Key.LeftCtrl) && Keyboard.IsKeyDown(Key.C))
        //        {
        //            try
        //            {
        //                string copied_txt = Clipboard.GetText();
        //                lock (_lock)
        //                {
        //                    //IDataObject data = Clipboard.GetDataObject();
        //                    // Console.Write(data.ToString());
        //                    string str = List.FirstOrDefault(x => x.Equals(copied_txt));
        //                    if (str == null)
        //                        List.Add(copied_txt);
        //                }
                        
        //            }
        //            catch(Exception e)
        //                {
        //                    Console.Write(e.HResult);
        //                }
        //        }
        //    }
        //}

        public int Used
        {
            get { return CopiedList.Count(); }
        }

        public void ContentCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Used");
        }

        private void CopiedItemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionSetCount = CopiedItemList.SelectedItems.Count;
        }

        public int SelectionSetCount
        {
            get { return m_selectionSetCount; }
            set { m_selectionSetCount = value; NotifyPropertyChanged("SelectionSetCount"); }
        }


        private void Button_Clear(object sender, RoutedEventArgs e)
        {
            CopiedList.Clear();
        }

        private void Button_Select_All(object sender, RoutedEventArgs e)
        {
            CopiedItemList.SelectAll();
        }

        private void Button_DeleteSelection(object sender, RoutedEventArgs e)
        {
            var selection = CopiedItemList.SelectedItems.Cast<object>().ToList();
            foreach (var selected in selection)
                CopiedList.Remove(selected.ToString());
        }

        private void Button_CopySelection(object sender, RoutedEventArgs e)
        {
            string res = "";
            bool first = true;
            var selection = CopiedItemList.SelectedItems;
            foreach (var selected in selection)
            {
                if (first)
                    res = selected.ToString() ;
                else
                    res = res + "\n" + selected.ToString();
                first = false;
            }

            Clipboard.SetText(res);
        }

        private void Button_save(object sender, RoutedEventArgs e)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ObservableCollection<string>));
            using (StreamWriter wr = new StreamWriter("customers.xml"))
            {
                xs.Serialize(wr, CopiedList);
            }

            ((Storyboard)FindResource("save_load_animation")).Begin(SaveNotificationAnimation);
        }
    }
}
