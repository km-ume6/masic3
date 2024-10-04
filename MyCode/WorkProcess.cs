using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;

namespace masic3.MyCode
{
    internal partial class ViewModelProcess : ObservableObject
    {

        /// <summary>
        /// ワーカスレッド
        /// </summary>
        /// <param name="state"></param>
        private void ProcessThread(object? state)
        {
            Debug.WriteLine("スレッド開始！");
            ModelShare.Working = true;

            DateTime dateTimeNow;
            TimeSpan timeSpan;      // プロセス全体の経過時間
            TimeSpan timeSpanSum;   // 現プロセスまでの設定時間積み上げ
            int currentStepIndex;
            bool gotoNext = false;

            do
            {
                //MyMessage = $"Welcome at {DateTime.Now}";

                if (IsRunning == true)
                {
                    if (ProcessStepsCopy.Count > 0)
                    {
                        dateTimeNow = DateTime.Now;
                        if (dateTimeNow >= dateTimeStartProcess)
                        {
                            // プロセス経過時間を計算する
                            timeSpan = dateTimeNow - dateTimeStartProcess;
                            PastProcessDateTimeText = TimeSpanToString(timeSpan);

                            currentStepIndex = 0;
                            timeSpanSum = ProcessStepsCopy[currentStepIndex].ProcessTime;

                            // ステップ経過処理
                            for (int i = 0; i < ProcessStepsCopy.Count; i++)
                            {
                                if (timeSpan < timeSpanSum)
                                {
                                    gotoNext = Processing(currentStepIndex, dateTimeNow - dateTimeStartStep);
                                    break;
                                }
                                else
                                {
                                    // ステップが終了と判定

                                    // 次のステップに進む準備
                                    dateTimeStartStep = dateTimeStartProcess + timeSpanSum;
                                    if (i + 1 < ProcessStepsCopy.Count)
                                    {
                                        timeSpanSum += ProcessStepsCopy[i+1].ProcessTime;
                                    }
                                    else
                                    {
                                        currentStepIndex++;
                                        break;
                                    }
                                }

                                currentStepIndex++;
                            }

                            //Debug.WriteLine($"Step:{currentStepIndex}, StepTime:{TimeSpanToString(dateTimeNow - dateTimeStartStep)}/{ProcessStepsCopy[currentStepIndex].ProcessTime}, ProcessTime:{timeSpan}/{timeSpanSum}");

                            // ステップを次へ進める処理（Advance）
                            if (IsAdvance == true || gotoNext == true)
                            {
                                ProcessStepsCopy[currentStepIndex].ProcessTime = dateTimeNow - dateTimeStartStep;
                                IsAdvance = false;
                                gotoNext = false;
                            }

                            // コントロール表示更新
                            if (currentStepIndex < ProcessStepsCopy.Count)
                            {
                                CurrentStepIndexText = ProcessStepsCopy[currentStepIndex].ProcessId.ToString();
                            }
                            StartStepDateTimeText = dateTimeStartStep.ToString("HH:mm:ss");
                            PastStepDateTimeText = TimeSpanToString(dateTimeNow - dateTimeStartStep);

                            if (currentStepIndex >= ProcessStepsCopy.Count)
                            {
                                // 制御終了と判定
                                Debug.WriteLine("プロセス終了");

                                IsRunning = false;
                                continue;
                            }
                        }
                    }
                }

                Thread.Sleep(1000);
            } while (ModelShare.EnableThreadLoop == true);

            ModelShare.Working = false;
            Debug.WriteLine("スレッド終了！");
        }

        bool Processing(int index, TimeSpan timeSpan)
        {
            //Debug.WriteLine($"P-ID:{ProcessStepsCopy[index].ProcessId}, P-Time:{timeSpan.TotalSeconds} passed of the {ProcessStepsCopy[index].ProcessTime.TotalSeconds}");

            bool ret = false;
            switch (ProcessStepsCopy[index].ProcessCommand.ToUpper())
            {
                case "KP-OUT":
                    double StartValue = double.Parse((index > 0) ? ProcessStepsCopy[index - 1].ProcessParam : "0.0");
                    double TargetValue = double.Parse(ProcessStepsCopy[index].ProcessParam);
                    double ChangeValue = TargetValue - StartValue;
                    double CurrentValue = StartValue + ChangeValue * (timeSpan.TotalSeconds / ProcessStepsCopy[index].ProcessTime.TotalSeconds);
                    Debug.WriteLine($"KP-OUT:{CurrentValue:0.0}");
                    break;
                case "KP-WAIT-EVENT":
                    if (IsCheckedTest == true)
                    {
                        ret = true;
                    }
                    Debug.WriteLine("KP-WAIT-EVENT");
                    break;
            }

            return ret;
        }
    }
}
