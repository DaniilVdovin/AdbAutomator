using System;
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

namespace AdbAutomator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
     
    public partial class MainWindow : Window
    {
        Automator automator;
        public MainWindow()
        {
            InitializeComponent();

            automator = new Automator(this);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            ListBoxItem listBoxItem = new ListBoxItem();
            TextBlock textBlock = new TextBlock();
            textBlock.Text = new TextRange(
                edit_task.Document.ContentStart,
                edit_task.Document.ContentEnd
                ).Text.Trim().ToLower();
            StackPanel stackPanel = new StackPanel();
            stackPanel.Children.Add(textBlock);
            listBoxItem.Content = stackPanel;

            task_list.Items.Add(listBoxItem);
            edit_task.Document.Blocks.Clear();
        }
    }
    public class AutomatorTask
    {
        public string[] comands;
        public AutomatorTask(string[] comands)
        {
            if (comands != null)
                this.comands = comands;
            else
                this.comands = new string[0];
        }
    }
    public class AutomatorDevice
    {
        public string SERIAL,Satus;
        public AutomatorDevice(string SERIAL,string Satus)
        {
            this.SERIAL = SERIAL;
            this.Satus = Satus;
        }
    }
    public static class AutomatorConstryctorTask
    {

    }
    public class Automator
    {
        MainWindow MainWindow;

        string log = "";
        System.Diagnostics.Process process;
        System.Diagnostics.ProcessStartInfo startInfo;
        String adb_exe_path;

        public AutomatorTask automatorTask;
        public List<AutomatorDevice> Devices;
        public Action printLog;


        public Automator(MainWindow mainWindow)
        {
            MainWindow = mainWindow;

            adb_exe_path = $@"D:\AndroidSDK\sdk-tools-windows\platform-tools\adb.exe";
            automatorTask = new AutomatorTask(
                    new string[]
                    {
                        $@"-s [device] shell am start -n com.daniilvdovin.wowmap/com.daniilvdovin.wowmap.MainActivity",
                         "-s [device] shell sleep 1;",
                         "-s [device] shell input tap 550 1190",
                         "-s [device] shell sleep 1;",
                         "-s [device] shell input tap 59 574",
                    }
                );
            process = new System.Diagnostics.Process();
            startInfo = new System.Diagnostics.ProcessStartInfo();

           // startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = $@"Adb.exe";

            startInfo.RedirectStandardOutput = true;
            startInfo.RedirectStandardInput = true;
            startInfo.RedirectStandardError = true;
            startInfo.UseShellExecute = false;
            //startInfo.CreateNoWindow = true;
            process.StartInfo = startInfo;
            printLog = (() => {
                   MainWindow._out.Content = log;
            });

            GetDevices(((l) => {
                Devices = l;
                log += "Devices\n";
                l.ForEach((a) =>
                {
                    log +=$"{a.SERIAL}:{a.Satus}\n";
                });
                log += "\n";
                printLog.Invoke();
            }));

            foreach (AutomatorDevice device in Devices)
            {
                Run(device, automatorTask, printLog);
            }
            
        }
        void GetDevices(Action<List<AutomatorDevice>> events)
        {
            var list = new List<AutomatorDevice>();
            RunPromt("devices", ((s, e) =>
            {
                if(e.Data != null &&
                e.Data !="" &&
                e.Data.Trim().Length!=1 &&
                !e.Data.Trim().Contains("List"))
                    if (e.Data.Split('\t').Length == 2)
                        list.Add(new AutomatorDevice(
                            e.Data.Split('\t')[0].Trim(),
                            e.Data.Split('\t')[1].Trim()
                      ));
            }),
            ((s, e) =>{}));
            events?.Invoke(list);
        }
        public void Run(AutomatorDevice device, AutomatorTask task,Action events)
        {
            //adb 
            log += "+Run\n";
            foreach (string c  in task.comands)
            {
                string cmd = c.Replace("[device]", device.SERIAL);
                log += $"{cmd}\n";
                RunPromt(cmd,((s,e)=>
                {
                    log += $"O:{e.Data}\n";

                }),
                ((s, e) =>
                {
                    log += $"E:{e.Data}\n";

                }));
            } 
            log += "+Close\n";
            events?.Invoke();
        }
        void RunPromt(string cmd,
            System.Diagnostics.DataReceivedEventHandler outputDataReceived,
            System.Diagnostics.DataReceivedEventHandler errorDataReceived)
        {
            startInfo.Arguments = cmd;
            process.OutputDataReceived += outputDataReceived;
            process.ErrorDataReceived += errorDataReceived;
            process.Start();
            process.BeginErrorReadLine();
            process.BeginOutputReadLine();
            process.WaitForExit();
            process.CancelOutputRead();
            process.CancelErrorRead();
            process.OutputDataReceived -= outputDataReceived;
            process.ErrorDataReceived -= errorDataReceived;
            process.Close();
        }
    }
}
