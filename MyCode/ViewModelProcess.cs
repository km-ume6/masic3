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
        [ObservableProperty] private ModelProcessStep? _selectedStep;
        [ObservableProperty] int _editStepId;
        [ObservableProperty] string _editStepCommand = string.Empty;
        [ObservableProperty] string _editStepParam = string.Empty;
        [ObservableProperty] TimeSpan _editStepTime;
        [ObservableProperty] bool _isRunning;
        [ObservableProperty] bool _isAdvance;
        [ObservableProperty] string _startProcessDateTimeText = string.Empty;
        [ObservableProperty] string _startStepDateTimeText = string.Empty;
        [ObservableProperty] string _pastProcessDateTimeText = string.Empty;
        [ObservableProperty] string _pastStepDateTimeText = string.Empty;
        [ObservableProperty] string _currentStepIndexText = string.Empty;
        [ObservableProperty] bool _isCheckedTest;
        [ObservableProperty] List<string> _commandSteps;
        [ObservableProperty] string _testParam = string.Empty;

        private ObservableCollection<ModelProcessStep> ProcessStepsCopy;
        ModelProcessStep? currentStep;
        DateTime dateTimeStartProcess;
        DateTime dateTimeStartStep;

        [ObservableProperty] public string _myMessage = string.Empty;
        public ICommand BindSelectionChanged { get; private set; }

        public ViewModelProcess()
        {
            ProcessSteps = new();
            SelectedStep = null;
            currentStep = null;

            dateTimeStartProcess = DateTime.MinValue;
            dateTimeStartStep = DateTime.MinValue;
            ProcessStepsCopy = new();

            CommandSteps = new();
            if (CommandSteps != null)
            {
                CommandSteps.Add("KP-OUT");
                CommandSteps.Add("KP-SEND");
                CommandSteps.Add("KP-WAIT-EVENT");
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
            if (SelectedStep != null)
            {
                EditStepId = SelectedStep.ProcessId;
                EditStepCommand = SelectedStep.ProcessCommand;
                EditStepParam = SelectedStep.ProcessParam;
                EditStepTime = SelectedStep.ProcessTime;

                currentStep = SelectedStep;
                SelectedStep = null;
            }
        }

        [RelayCommand]
        void AddProcessStep()
        {
            switch (EditStepCommand)
            {
                case "KP-OUT":
                    if (string.IsNullOrEmpty(EditStepParam) || !EditStepParam.All(char.IsDigit))
                    {
                        // メッセージウィンドウを表示
                        Application.Current?.MainPage?.DisplayAlert("エラー", "無効なパラメータです。", "OK");
                        return;
                    }                    
                    break;
                default:
                    break;
            }
            currentStep = null;
            ProcessSteps.Add(new ModelProcessStep(ProcessSteps.Count + 1, EditStepCommand, EditStepParam, EditStepTime));
        }
        [RelayCommand]
        void UpdateProcessStep()
        {
            if (currentStep != null)
            {
                int index = ProcessSteps.IndexOf(currentStep);
                if (index > -1)
                {
                    ProcessSteps.RemoveAt(index);
                    ProcessSteps.Insert(index, new ModelProcessStep(index + 1, EditStepCommand, EditStepParam, EditStepTime));
                }
            }
        }

        [RelayCommand]
        void InsertProcessStep()
        {
            if (currentStep != null)
            {
                int index = ProcessSteps.IndexOf(currentStep);
                if (index > -1)
                {
                    ObservableCollection<ModelProcessStep> processItems = new(ProcessSteps);
                    processItems.Insert(index, currentStep);
                    ReNumber(ref processItems);
                }
            }
        }

        [RelayCommand]
        void RemoveProcessStep()
        {
            if (currentStep != null)
            {
                int index = ProcessSteps.IndexOf(currentStep);
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
            EditStepId = 0;
            EditStepCommand = string.Empty;
            EditStepParam = string.Empty;
            EditStepTime = TimeSpan.Zero;
        }

        string TimeSpanToString(TimeSpan ts)
        {
            return $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}";
        }
    }
}
