using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Backup
{
    public class ListLabel
    {
        public string Path { get; set; }
        public string Status { get; set; }
    }
    public partial class MainWindow : Window
    {
        List<string> PathsToBackup;
        string DestPath;
        int copiers = 0;
        int copiersDone = 0;
        public MainWindow()
        {
            InitializeComponent();
            PathsToBackup = GetBackupPaths();
            DestPath = GetDestPath();

            if (DestPath != "")
                DestIsSet();
            else
                DestNotSet();

            foreach(string path in PathsToBackup)
            {
                PathsList.Items.Add(new ListLabel { Path = path, Status = "Ready" });
            }
        }

        void DestIsSet()
        {
            SelectedPathBox.Text = "Selected Backup Path:\n" + DestPath;
            BackupButton.IsEnabled = true;
        }

        void DestNotSet()
        {
            BackupButton.IsEnabled = false;
            SelectedPathBox.Text = "Select Backup Path";
        }

        string SelectFolder()
        {
            Process process = new Process
            {
                StartInfo =
                {
                FileName = "cmd.exe",
                Arguments = "/c SelectFile.bat",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true,
                }
            };

            process.Start();
            process.WaitForExit();
            string selectedFolder = process.StandardOutput.ReadToEnd().Replace("\n", "").Replace("\r", "");

            if (selectedFolder.Contains("ECHO"))
                return "";

            return selectedFolder;
        }

        List<string> GetBackupPaths()
        {
            var paths = File.ReadAllText("ToBackupPaths.txt").Split('\r').ToList();

            for (int i = 0; i < paths.Count; i++)
                paths[i] = paths[i].Replace("\n", "");

            if (paths[0] == "")
                return new();

            return paths;
        }

        string GetDestPath()
        {
            return File.ReadAllText("BackupPath.txt");
        }

        private void SelectNewBackupPath(object sender, RoutedEventArgs e)
        {
            string selectedFolder = SelectFolder();

            if (selectedFolder == "")
                return;

            DestPath = selectedFolder;
            File.WriteAllText("BackupPath.txt", selectedFolder + "\n");
            DestIsSet();
        }

        void UpdateBackupPathsFile()
        {
            string toWrite = "";
            foreach (string path in PathsToBackup)
            {
                toWrite += path + "\n";
            }
            File.WriteAllText("ToBackupPaths.txt", toWrite);
        }

        private void AddPathClick(object sender, RoutedEventArgs e)
        {
            string selectedFolder = SelectFolder();

            if (selectedFolder == "")
                return;

            PathsToBackup.Add(selectedFolder);
            PathsList.Items.Add(new ListLabel { Path = selectedFolder, Status = "Ready"});

            UpdateBackupPathsFile();
        }

        private void RemovePathClick(object sender, RoutedEventArgs e)
        {
            ListLabel selectedItem = (ListLabel)PathsList.SelectedItem;
            if (selectedItem == null)
                return;
            string path = selectedItem.Path;
            PathsToBackup.Remove(path);
            PathsList.Items.Remove(selectedItem);
            RmButton.IsEnabled = false;
            OpenButton.IsEnabled = false;
            UpdateBackupPathsFile();
        }

        private void BackupFiles()
        {
            for (int i = 0; i < PathsToBackup.Count; i++)
            {
                copiers += 1;
                string source = PathsToBackup[i];
                string dest = DestPath + source.Split("\\").Last();
                Process p = new Process
                {
                    StartInfo =
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c robocopy \""+source+"\" \""+dest+"\" /s /MIR",
                        UseShellExecute = false,
                        RedirectStandardOutput = false,
                        CreateNoWindow = true,
                    }
                };
                p.EnableRaisingEvents = true;
                p.Exited += new EventHandler(RoboCopyExit);
                p.Start();

                foreach (ListLabel item in PathsList.Items)
                {
                    if (source == item.Path)
                    {
                        item.Status = "Copying...";
                        PathsList.Items.Refresh();
                    }
                }
            }
        }

        private void RoboCopyExit(object sender, EventArgs e)
        {
            copiersDone += 1;
            Process p = (Process)sender;
            string pSource = p.StartInfo.Arguments.Split('"', 3)[1];
            foreach (ListLabel item in PathsList.Items)
            {
                Dispatcher.Invoke(() =>
                {
                    string itemSource = item.Path;
                    if (pSource == itemSource)
                    {
                        item.Status = "Done";
                        PathsList.Items.Refresh();
                    }
                });
            }

            if (copiers == copiersDone)
            {
                Dispatcher.Invoke(() =>
                {
                    BackupButton.IsEnabled = true;
                });
            }
        }

        private void StartBackupClick(object sender, RoutedEventArgs e)
        {
            BackupButton.IsEnabled = false;
            BackupFiles();
        }

        private void OpenPathClick(object sender, RoutedEventArgs e)
        {
            ListLabel selectedItem = (ListLabel)PathsList.SelectedItem;

            if (selectedItem == null)
                return;

            Process process = new Process
            {
                StartInfo =
                {
                FileName = "cmd.exe",
                Arguments = "/c start " + selectedItem.Path,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                CreateNoWindow = true,
                }
            };

            process.Start();
        }

        private void ListSelctionChange(object sender, SelectionChangedEventArgs e)
        {
            RmButton.IsEnabled = true;
            OpenButton.IsEnabled = true;
        }
    }
}
