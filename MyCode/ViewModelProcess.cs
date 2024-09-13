using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using System.Xml.Serialization;

namespace masic3.MyCode
{
    internal partial class ViewModelProcess : ObservableObject
    {
        [ObservableProperty] private ObservableCollection<ModelProcessItem> _processItems;
        [ObservableProperty] private ModelProcessItem? _selectedProcessItem;
        ModelProcessItem? currentProcessItem;

        // これらは入力用コントロールへバインドされる
        [ObservableProperty] int _editProcessId;
        [ObservableProperty] TimeSpan _editProcessTime;
        [ObservableProperty] string _editProcessName = string.Empty;
        [ObservableProperty] int _editProcessData;
        [ObservableProperty] bool _isRunning;
        [ObservableProperty] string _startProcessDateTimeText = string.Empty;
        [ObservableProperty] string _startItemDateTimeText = string.Empty;
        [ObservableProperty] string _pastProcessDateTimeText = string.Empty;
        [ObservableProperty] string _pastItemDateTimeText = string.Empty;

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
                EditProcessTime = SelectedProcessItem.ProcessTime;
                EditProcessName = SelectedProcessItem.ProcessCommand;
                EditProcessData = SelectedProcessItem.ProcessData;

                currentProcessItem = SelectedProcessItem;
                SelectedProcessItem = null;
            }
        }

        [RelayCommand]
        void AddProcessItem()
        {
            currentProcessItem = null;
            ProcessItems.Add(new ModelProcessItem(ProcessItems.Count + 1, EditProcessTime, EditProcessName, EditProcessData));
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
                    ProcessItems.Insert(index, new ModelProcessItem(index + 1, EditProcessTime, EditProcessName, EditProcessData));
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
            // データ書込み（保存）
            XmlSerializer mySerializer = new XmlSerializer(typeof(ObservableCollection<ModelProcessItem>));
            StreamWriter myWriter = new(MakeXmlName());
            mySerializer.Serialize(myWriter, ProcessItems);
            myWriter.Close();

            Application.Current?.Quit();
        }

        public void CheckedIsRunning()
        {
            dateTimeStartProcess = DateTime.Now;
            dateTimeStartItem = dateTimeStartProcess;
            if (IsRunning == true)
            {
                StartProcessDateTimeText = dateTimeStartProcess.ToString();
                StartItemDateTimeText = StartProcessDateTimeText;
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
            EditProcessTime = TimeSpan.Zero;
            EditProcessName = string.Empty;
            EditProcessData = 0;
        }

        string TimeSpanToString(TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }

        /// <summary>
        /// ワーカースレッド
        /// </summary>
        /// <param name="state"></param>
        private void ProcessThread(object? state)
        {
            Debug.WriteLine("スレッド開始！");
            masic3.MyCode.ModelShare.Working = true;

            DateTime dateTimeNow;
            TimeSpan timeSpan;
            TimeSpan timeSpanSum = new(0, 0, 0);
            int currentProcessIndex;

            do
            {
                //MyMessage = $"Welcome at {DateTime.Now}";

                if (IsRunning == true)
                {
                    if (ProcessItems.Count > 0)
                    {
                        dateTimeNow = DateTime.Now;
                        if (dateTimeNow >= dateTimeStartProcess)
                        {
                            // 制御経過時間を計算する
                            timeSpan = dateTimeNow - dateTimeStartProcess;
                            PastProcessDateTimeText = TimeSpanToString(timeSpan);

                            currentProcessIndex = 0;
                            timeSpanSum = ProcessItems[0].ProcessTime;

                            // プロセス経過処理
                            foreach (ModelProcessItem item in ProcessItems)
                            {
                                // プロセス経過時間文字列を作る
                                PastItemDateTimeText = TimeSpanToString(dateTimeNow - dateTimeStartItem);

                                if (timeSpan < timeSpanSum)
                                {
                                    break;
                                }
                                else
                                {
                                    // プロセス終了と判定

                                    dateTimeStartItem = dateTimeStartProcess + timeSpanSum;
                                    StartItemDateTimeText = dateTimeStartItem.ToString();
                                    timeSpanSum += item.ProcessTime;
                                }

                                currentProcessIndex++;
                            }

                            if (currentProcessIndex >= ProcessItems.Count)
                            {
                                // 制御終了と判定

                                IsRunning = false;
                                continue;
                            }
                        }
                    }
                }

                Thread.Sleep(1000);
            } while (masic3.MyCode.ModelShare.EnableThreadLoop == true);

            masic3.MyCode.ModelShare.Working = false;
            Debug.WriteLine("スレッド終了！");
        }
    }
}
