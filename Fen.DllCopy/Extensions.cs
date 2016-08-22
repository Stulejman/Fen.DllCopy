using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace Fen.DllCopy
{
    public class BinFolder
    {
        [Bindable(true)]
        public bool Checked { get; set; }
        public string FullPath { get; set; }
        public string ShortPath { get; set; }
        public string LibPath { get; set; }
        [Bindable(true)]
        public string ProjectName { get; set; }
    }
    public static class Extensions
    {
        public static bool Contains(this string source, string toCheck, StringComparison comp)
        {
            return source.IndexOf(toCheck, comp) >= 0;
        }
        public static int AddNew(this StringCollection list, string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return -1;
            var index = list.IndexOf(s);
            if (index != -1)
            {
                return index;
            }
            list.Add(s);
            return list.IndexOf(s);
        }
        public static IEnumerable<BinFolder> GetAllBinFolders(string rootPath)
        {
            if (!Directory.Exists(rootPath)) yield break;
            foreach (var subdir in Directory.GetDirectories(rootPath))
            {
                if ((subdir.EndsWith(@"bin\Debug") ||
                     (subdir.EndsWith(@"bin") && !Directory.GetDirectories(subdir).Any(s => s.EndsWith("Debug"))))
                    && !subdir.Contains("Test") && !subdir.Contains("Mock"))
                {
                    yield return new BinFolder
                    {
                        FullPath = subdir,
                        ShortPath = subdir.Replace(rootPath, ""),
                        ProjectName = GetProjectName(subdir)
                    };
                }
                else
                {
                    foreach (var subdir2 in GetAllBinFolders(subdir))
                    {
                        yield return subdir2;
                    }
                }
            }
        }
        private static string GetProjectName(string subdir)
        {
            var splited = subdir.Split(Path.DirectorySeparatorChar).ToList();
            var projectName = splited[splited.IndexOf("bin") - 1];
            return NonStandardProjectNames.ContainsKey(projectName) ? NonStandardProjectNames[projectName] : projectName;
        }

        private static readonly Dictionary<string, string> NonStandardProjectNames = new Dictionary<string, string>
        {
            {"Ergo.ExpertBanker.Domain.LegalEntityModule.Strategies", "Ergo.ExpertBanker.Domain.LegalEntityModule.UIControlGroupStrategy"},
            {"CovenantModuleServiceImplementation","Ergo.ExpertBanker.CovenantModuleServiceImplementation" },
            {"CovenantService","Ergo.ExpertBanker.CovenantModuleService" },
            {"FenergoQueueServiceImplementation", "Fenergo.QueueServiceImplementation"},
            {"Ergo.ExpertBanker.ServiceImplementation","Ergo.ExpertBanker.MainModuleServiceImplementation" }
        };

    }
    public class Solution
    {
        public string Name { get; set; }
        public List<ProjectDir> Dirs { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }
    public class ProjectDir
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public string LogPath { get; set; }
        public string LibPath { get; set; }
        public string BinPath { get; set; }
        public override string ToString()
        {
            return Name;
        }
    }

    public enum Projects
    {
        FenergoSolution,
        PSSolution,
        WebApp
    }
}
