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
        [ObservableProperty] private ObservableCollection<ModelProcessStep> _processSteps;
        [ObservableProperty] private ModelProcessStep? _selectedProcessStep;
        [ObservableProperty] int _editProcessId;
        [ObservableProperty] string _editProcessCommand = string.Empty;
        [ObservableProperty] string _editProcessParam = string.Empty;
        [ObservableProperty] TimeSpan _editProcessTime;
        [ObservableProperty] bool _isRunning;
        [ObservableProperty] bool _isAdvance;
        [ObservableProperty] string _startProcessDateTimeText = string.Empty;
        [ObservableProperty] string _startItemDateTimeText = string.Empty;
        [ObservableProperty] string _pastProcessDateTimeText = string.Empty;
        [ObservableProperty] string _pastItemDateTimeText = string.Empty;
        [ObservableProperty] string _currentProcessIndexText = string.Empty;
        [ObservableProperty] bool _isCheckedTest;
        [ObservableProperty] List<string> _commandItems;
        [ObservableProperty] string _testParam = string.Empty;

        private ObservableCollection<ModelProcessStep> ProcessStepsCopy;
        ModelProcessStep? currentProcessStep;
        DateTime dateTimeStartProcess;
        DateTime dateTimeStartStep;

        [ObservableProperty] public string _myMessage = string.Empty;
        public ICommand BindSelectionChanged { get; private set; }

        public ViewModelProcess()
        {
            ProcessSteps = new();
            SelectedProcessStep = null;
            currentProcessStep = null;

            dateTimeStartProcess = DateTime.MinValue;
            dateTimeStartStep = DateTime.MinValue;
            ProcessStepsCopy = new();

            CommandItems = new();
            if (CommandItems != null)
            {
                CommandItems.Add("KP-OUT");
                CommandItems.Add("KP-SEND");
                CommandItems.Add("KP-WAIT-EVENT");
            }

            // バインディング用のコマンド作成
            BindSelectionChanged = new Command(SelectionChangedCommand);

            TestParam = Preferences.Get("TestParam", "");

            // プロセスデータ読込み
            string xmlFile = MakeXmlName();
            if (System.IO.File.Exists(xmlFile))
            {
                var mySerializer = new XmlSerializer(typeof(ObservableCollection<ModelProcessStep>));
                using var myFileStream = new FileStream(xmlFile, FileMode.Open);
                if (myFileStream != null)
                {
                    var datas = mySerializer.Deserialize(myFileStream);
                    if (datas != null)
                    {
                        ProcessSteps = (ObservableCollection<ModelProcessStep>)datas;
                    }
                }
            }

            ThreadPool.QueueUserWorkItem(ProcessThread);
        }

        void SelectionChangedCommand()
        {
            if (SelectedProcessStep != null)
            {
                EditProcessId = SelectedProcessStep.ProcessId;
                EditProcessCommand = SelectedProcessStep.ProcessCommand;
                EditProcessParam = SelectedProcessStep.ProcessParam;
                EditProcessTime = SelectedProcessStep.ProcessTime;

                currentProcessStep = SelectedProcessStep;
                SelectedProcessStep = null;
            }
        }

        [RelayCommand]
        void AddProcessItem()
        {
            currentProcessStep = null;
            ProcessSteps.Add(new ModelProcessStep(ProcessSteps.Count + 1, EditProcessCommand, EditProcessParam, EditProcessTime));
        }
        [RelayCommand]
        void UpdateProcessItem()
        {
            if (currentProcessStep != null)
            {
                int index = ProcessSteps.IndexOf(currentProcessStep);
                if (index > -1)
                {
                    ProcessSteps.RemoveAt(index);
                    ProcessSteps.Insert(index, new ModelProcessStep(index + 1, EditProcessCommand, EditProcessParam, EditProcessTime));
                }
            }
        }

        [RelayCommand]
        void InsertProcessItem()
        {
            if (currentProcessStep != null)
            {
                int index = ProcessSteps.IndexOf(currentProcessStep);
                if (index > -1)
                {
                    ObservableCollection<ModelProcessStep> processItems = new(ProcessSteps);
                    processItems.Insert(index, currentProcessStep);
                    ReNumber(ref processItems);
                }
            }
        }


        [RelayCommand]
        void RemoveProcessItem()
        {
            if (currentProcessStep != null)
            {
                int index = ProcessSteps.IndexOf(currentProcessStep);
                if (index > -1)
                {
                    ObservableCollection<ModelProcessStep> processItems = new(ProcessSteps);
                    processItems.RemoveAt(index);

                    ReNumber(ref processItems);

                    ClearEditProcessItem();
                }
            }
        }

        void ReNumber(ref ObservableCollection<ModelProcessStep> items)
        {
            ProcessSteps.Clear();

            ModelProcessStep item;
            for (int i = 0; i < items.Count; i++)
            {
                item = items[i];
                item.ProcessId = i + 1;
                ProcessSteps.Add(new ModelProcessStep(item));
            }

            items.Clear();
        }

        /// <summary>
        /// アプリ終了処理
        /// ・CollectionViewの内容をファイルに保存する
        /// </summary>
        [RelayCommand]
        void Finalization()
        {
            // プロセスデータ書込み（保存）
            XmlSerializer mySerializer = new(typeof(ObservableCollection<ModelProcessStep>));
            StreamWriter myWriter = new(MakeXmlName());
            mySerializer.Serialize(myWriter, ProcessSteps);
            myWriter.Close();

            // その他データ書込み
            Preferences.Set("TestParam", TestParam);

            Application.Current?.Quit();
        }

        public void CheckedIsRunning()
        {
            dateTimeStartProcess = DateTime.Now;
            dateTimeStartStep = dateTimeStartProcess;
            if (IsRunning == true)
            {
                StartProcessDateTimeText = dateTimeStartProcess.ToString();
                //StartItemDateTimeText = dateTimeStartStep.ToString("HH:mm:ss");

                // プロセスデータをコピーする
                ProcessStepsCopy.Clear();
                for (int i = 0; i < ProcessSteps.Count; i++)
                {
                    ProcessStepsCopy.Add(new(ProcessSteps[i]));
                }
            }
        }

        public void CheckedIsAdvance()
        {

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
