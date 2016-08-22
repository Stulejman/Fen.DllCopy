using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Fen.DllCopy.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Fen.DllCopy
{
    public partial class SettingsPage
    {
        private List<Solution> Solutions = new List<Solution>();
        public SettingsPage()
        {
            InitializeComponent();
        }

        private void BrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            var result = dialog.ShowDialog();
            var location = dialog.FileNames.FirstOrDefault();
            ProjectDir.Text = location;
            Settings.Default.localPath = location;
            GetAll(location);
            var to = new List<string>();
            var from = new List<string>();
            foreach (var solution in Solutions)
            {
                foreach (var dir in solution.Dirs)
                {
                    if (!string.IsNullOrWhiteSpace(dir.BinPath))
                    {
                        if(Directory.Exists(dir.BinPath))
                            to.Add(dir.BinPath);
                        if (Directory.Exists(dir.LibPath))
                            to.Add(dir.LibPath);
                    }
                    if (dir.Name == "Core" || dir.Name == "Infrastructure")
                    {
                        if (Directory.Exists(dir.Path))
                            from.Add(dir.Path);
                    }
                }
            }
            Settings.Default.DirTo.AddRange(to.ToArray());
            Settings.Default.DirFrom.AddRange(from.ToArray());
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.FirstUse = false;
            Settings.Default.Save();

            var main = new MainWindow();
            main.Show();
            Close();
        }

        public string GetDirName(string s)
        {
            string parent = Path.GetFileName(Directory.GetParent(s).FullName);
            return parent + "\\" + Path.GetFileName(s) ?? "";
        }

        public void GetAll(string start, int depth = 4)
        {
            if (depth == 0) return;

            var dirs = Directory.GetDirectories(start).ToList();
            if (IsMainDir(dirs))
            {
                var solution = new Solution { Name = GetDirName(start), Dirs = GetProjectsDirs(dirs) };
                Solutions.Add(solution);
            }
            else
            {
                foreach (var dir in dirs)
                {
                    GetAll(dir, depth - 1);
                }
            }
        }

        public bool IsMainDir(List<string> dirs)
        {
            return GetProjectsDirs(dirs).Any();
        }

        public List<ProjectDir> GetProjectsDirs(List<string> dirs)
        {
            var list = new List<ProjectDir>();
            foreach (var dir in dirs)
            {
                switch (Path.GetFileName(dir))
                {
                    case "FenergoSolution":
                        list.Add(new ProjectDir
                        {
                            Name = "FenergoSolution",
                            Path = dir,
                            LogPath = dir + "\\Logs",
                            BinPath = dir + "\\FS-WebApp\\UI\\WebUI\\bin",
                            LibPath = dir + "\\FS-WebApp\\Lib"
                        });
                        break;
                    case "PSSolution":
                        list.Add(new ProjectDir { Name = "PSSolution", Path = dir, LogPath = dir + "\\Logs" });
                        break;
                    case "WebApp":
                        list.Add(new ProjectDir
                        {
                            Name = "Core",
                            Path = dir + "\\Src",
                            LogPath = dir + "\\Src\\Logs",
                            BinPath = dir + "\\Src\\UI\\Web\\WebUI\\bin",
                            LibPath = dir + "\\Src\\Lib"
                        });
                        break;
                    case "Logs":
                        list.Add(new ProjectDir { Name = Path.GetFileName(dir.Replace("\\Logs", "")), Path = dir });
                        break;
                    case "Infrastructure":
                        list.Add(new ProjectDir { Name = "Infrastructure", Path = dir });
                        break;
                }
            }
            return list;
        }
    }
}
