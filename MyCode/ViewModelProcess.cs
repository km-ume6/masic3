using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics.Text;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Xml.Serialization;

namespace masic3.MyCode
{
    internal partial class ViewModelProcess : ObservableObject
    {
        // これらは入力用コントロールへバインドされる
        [ObservableProperty] private ObservableCollection<ModelProcessItem> _processItems;
        [ObservableProperty] private ModelProcessItem? _selectedProcessItem;
        [ObservableProperty] int _editProcessId;
        [ObservableProperty] string _editProcessCommand = string.Empty;
        [ObservableProperty] string _editProcessParam = string.Empty;
        [ObservableProperty] TimeSpan _editProcessTime;
        [ObservableProperty] bool _isRunning;
        [ObservableProperty] string _startProcessDateTimeText = string.Empty;
        [ObservableProperty] string _startItemDateTimeText = string.Empty;
        [ObservableProperty] string _pastProcessDateTimeText = string.Empty;
        [ObservableProperty] string _pastItemDateTimeText = string.Empty;
        [ObservableProperty] string _currentProcessIndexText = string.Empty;
        [ObservableProperty] bool _isCheckedTest;
        [ObservableProperty] List<string> _commandItems;

        ModelProcessItem? currentProcessItem;
        DateTime dateTimeStartProcess;
        DateTime dateTimeStartItem;

        [ObservableProperty] public string _myMessage = string.Empty;
        public ICommand BindSelectionChanged { get; private set; }

        public ViewModelProcess()
        {
            ProcessItems = new();
            SelectedProcessItem = null;
            currentProcessItem = null;

            dateTimeStartProcess = DateTime.MinValue;
            dateTimeStartItem = DateTime.MinValue;

            CommandItems = new();
            if(CommandItems != null)
            {
                CommandItems.Add("KP-OUT");
                CommandItems.Add("KP-SEND");
                CommandItems.Add("KP-WAIT-EVENT");
            }

            // バインディング用のコマンド作成
            BindSelectionChanged = new Command(SelectionChangedCommand);

            // プロセスデータ読込み
            string xmlFile = MakeXmlName();
            if (System.IO.File.Exists(xmlFile))
            {
                var mySerializer = new XmlSerializer(typeof(ObservableCollection<ModelProcessItem>));
                using var myFileStream = new FileStream(xmlFile, FileMode.Open);
                if (myFileStream != null)
                {
                    var datas = mySerializer.Deserialize(myFileStream);
                    if (datas != null)
                    {
                        ProcessItems = (ObservableCollection<ModelProcessItem>)datas;
                    }
                }
            }

            ThreadPool.QueueUserWorkItem(ProcessThread);
        }

        void SelectionChangedCommand()
        {
            if (SelectedProcessItem != null)
            {
                EditProcessId = SelectedProcessItem.ProcessId;
                EditProcessCommand = SelectedProcessItem.ProcessCommand;
                EditProcessParam = SelectedProcessItem.ProcessParam;
                EditProcessTime = SelectedProcessItem.ProcessTime;

                currentProcessItem = SelectedProcessItem;
                SelectedProcessItem = null;
            }
        }

        [RelayCommand]
        void AddProcessItem()
        {
            currentProcessItem = null;
            ProcessItems.Add(new ModelProcessItem(ProcessItems.Count + 1, EditProcessCommand, EditProcessParam, EditProcessTime));
        }

        [RelayCommand]
        void InsertProcessItem()
        {
            if (currentProcessItem != null)
            {
                int index = ProcessItems.IndexOf(currentProcessItem);
                if (index > -1)
                {
                    ProcessItems.Insert(index, new ModelProcessItem(index, EditProcessCommand, EditProcessParam, EditProcessTime));
                }
            }
        }

        [RelayCommand]
        void UpdateProcessItem()
        {
            if (currentProcessItem != null)
            {
                int index = ProcessItems.IndexOf(currentProcessItem);
                if (index > -1)
                {
                    ProcessItems.RemoveAt(index);
                    ProcessItems.Insert(index, new ModelProcessItem(index + 1, EditProcessCommand, EditProcessParam, EditProcessTime));
                }
            }
        }

        [RelayCommand]
        void RemoveProcessItem()
        {
            if (currentProcessItem != null)
            {
                int index = ProcessItems.IndexOf(currentProcessItem);
                if (index > -1)
                {
                    ProcessItems.RemoveAt(index);

                    ObservableCollection<ModelProcessItem> processItems = new(ProcessItems);
                    ProcessItems.Clear();

                    ModelProcessItem item;
                    for (int i = 0; i < processItems.Count; i++)
                    {
                        item = processItems[i];

                        if (i >= index)
                        {
                            item.ProcessId--;
                        }

                        ProcessItems.Add(new ModelProcessItem(item));
                    }

                    processItems.Clear();
                    ClearEditProcessItem();
                }
            }
        }

        /// <summary>
        /// アプリ終了処理
        /// ・CollectionViewの内容をファイルに保存する
        /// </summary>
        [RelayCommand]
        void Finalization()
        {
            // プロセスデータ書込み（保存）
            XmlSerializer mySerializer = new (typeof(ObservableCollection<ModelProcessItem>));
            StreamWriter myWriter = new(MakeXmlName());
            mySerializer.Serialize(myWriter, ProcessItems);
            myWriter.Close();

            // その他データ書込み
            Preferences.Set("SendParam", window.X);

            Application.Current?.Quit();
        }

        public void CheckedIsRunning()
        {
            dateTimeStartProcess = DateTime.Now;
            dateTimeStartItem = dateTimeStartProcess;
            if (IsRunning == true)
            {
                StartProcessDateTimeText = dateTimeStartProcess.ToString();
                //StartItemDateTimeText = dateTimeStartItem.ToString("HH:mm:ss");
            }
        }

        /// <summary>
        /// 実行ファイル名の拡張子をxmlに変えた文字列を返す
        /// </summary>
        /// <returns></returns>
        private string MakeXmlName()
        {
            string exeName = System.Reflection.Assembly.GetExecutingAssembly().Location;
            return Path.ChangeExtension(exeName, "xml");
        }

        void ClearEditProcessItem()
        {
            EditProcessId = 0;
            EditProcessCommand = string.Empty;
            EditProcessParam = string.Empty;
            EditProcessTime = TimeSpan.Zero;
        }

        string TimeSpanToString(TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }
    }
}
