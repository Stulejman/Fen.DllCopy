using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Fen.DllCopy.Properties;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace Fen.DllCopy
{
    public partial class MainWindow
    {
        private int _count;
        public List<BinFolder> AllBinFolders = new List<BinFolder>();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            ReloadCombo();
            GetAllModules();
            ModulesList.ItemsSource = AllBinFolders;
        }

        private void GetAllModules()
        {
            var item = Settings.Default.DirFrom[DirFrom.SelectedIndex];
            AllBinFolders = Extensions.GetAllBinFolders(item).OrderBy(x => x.ProjectName).ToList();
            Filter();
        }

        public int SelectedDirFrom
        {
            get { return Settings.Default.SelectedFrom; }
            set { Settings.Default.SelectedFrom = value; }
        }

        public int SelectedDirTo
        {
            get { return Settings.Default.SelectedTo; }
            set { Settings.Default.SelectedTo = value; }
        }

        public string SelectedDirToText => DirTo.SelectedItem as string;
        
        private void OnClosing(object sender, CancelEventArgs e)
        {
            Settings.Default.Save();
        }

        private void ReloadCombo()
        {
            ReloadFrom();
            ReloadTo();
        }

        private void ReloadTo()
        {
            var selectedTo = Settings.Default.SelectedTo;
            DirTo.ItemsSource = null;
            DirTo.ItemsSource = Settings.Default.DirTo;
            DirTo.SelectedIndex = selectedTo;
        }

        private void ReloadFrom()
        {
            var selectedFrom = Settings.Default.SelectedFrom;
            DirFrom.ItemsSource = null;
            DirFrom.ItemsSource = Settings.Default.DirFrom;
            DirFrom.SelectedIndex = selectedFrom;
        }


        private async void CopyClick(object sender, RoutedEventArgs e)
        {
            var toCopy = AllBinFolders.Where(x => x.Checked).ToList();
            Output.Items.Clear();
            Progress.Maximum = toCopy.Count;
            if (!string.IsNullOrWhiteSpace(SelectedDirToText))
                await Task.Run(() => CopyFilesAsync(this, toCopy));
        }

        internal void CopyFilesAsync(MainWindow gui, List<BinFolder> list)
        {
            foreach (var folder in list)
            {
                Dispatcher.Invoke(() =>
                {
                    var pdb = Copy(folder, SelectedDirToText, "pdb");
                    var dll = Copy(folder, SelectedDirToText, "dll");
                    if (pdb && dll)
                    {
                        Log("Copied " + folder.ProjectName);
                    }
                    Progress.Value = ++_count;
                });
            }
        }

        private void FilterChange(object sender, TextChangedEventArgs e)
        {
            Filter();
        }

        private void Filter()
        {
            ModulesList.ItemsSource = null;
            ModulesList.ItemsSource = string.IsNullOrWhiteSpace(FilterBox.Text)
                ? AllBinFolders
                : AllBinFolders.Where(x => x.ProjectName.Contains(FilterBox.Text, StringComparison.OrdinalIgnoreCase));
        }

        private void BrowseToClick(object sender, RoutedEventArgs e)
        {
            var path = BrowseDir();
            var idx = Settings.Default.DirTo.AddNew(path);
            SelectedDirTo = idx;
            ReloadTo();
        }

        private void BrowseFromClick(object sender, RoutedEventArgs e)
        {
            var path = BrowseDir();
            var idx = Settings.Default.DirFrom.AddNew(path);
            SelectedDirTo = idx;
            ReloadFrom();
        }

        private static string BrowseDir()
        {
            var dialog = new CommonOpenFileDialog {IsFolderPicker = true};
            var result = dialog.ShowDialog();
            return result == CommonFileDialogResult.Ok ? dialog.FileNames.FirstOrDefault() : string.Empty;
        }


        private void DeselectClick(object sender, RoutedEventArgs e)
        {
            AllBinFolders.ForEach(x => x.Checked = false);

            ModulesList.ItemsSource = null;
            ModulesList.ItemsSource = AllBinFolders;
        }

        private void SelectAllClick(object sender, RoutedEventArgs e)
        {
            AllBinFolders.ForEach(x => x.Checked = true);

            ModulesList.ItemsSource = null;
            ModulesList.ItemsSource = AllBinFolders;
        }

        private bool Copy(BinFolder selectedFolder, string toPath, string extension)
        {
            try
            {
                var fileFrom = Path.Combine(selectedFolder.FullPath, selectedFolder.ProjectName) + "." + extension;
                if (Path.GetFileName(toPath) == "Lib")
                {
                    var directories = Directory.GetDirectories(toPath);
                    foreach (var dir in directories)
                    {
                        var files = Directory.GetFiles(dir).Select(Path.GetFileName);
                        if (files.Contains(selectedFolder.ProjectName + ".dll"))
                        {
                            return CopyFile(selectedFolder, fileFrom, dir, extension);
                        }
                    }
                }
                else
                {
                    return CopyFile(selectedFolder, fileFrom, toPath, extension);
                }
                return false;
            }
            catch (Exception e)
            {
                Log(e.ToString());
                return false;
            }
        }

        private bool CopyFile(BinFolder selectedFolder,string fileFrom, string dir, string extension)
        {
            var to = Path.Combine(dir, selectedFolder.ProjectName) + "." + extension;

            if (File.Exists(fileFrom))
            {
                if (File.Exists(to))
                {
                    var fileName = Path.GetFileName(to);
                    if (new FileInfo(to).IsReadOnly)
                    {
                        Log($"Exception: {fileName} is in readonly mode!");
                        return false;
                    }
                    else
                    {
                        File.Copy(fileFrom, to, true);
                        return true;
                    }
                }
                return false;
            }
            else
            {
                Log("File not found:" + fileFrom);
                return false;
            }
        }

        private void Log(string s)
        {
            Output.Items.Insert(0, s);
        }

        private void FromChanged(object sender, SelectionChangedEventArgs e)
        {
            GetAllModules();
        }












        
        
    }
}