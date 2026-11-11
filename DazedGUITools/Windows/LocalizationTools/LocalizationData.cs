using Dazed.Vitals;
using Dazed.Vitals.Localization;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace DazedGUITools.Windows.LocalizationTools
{
    public static class LocalizationData
    {
        private static SerializedMainStringTable _mainStringTable;
        private static readonly Dictionary<string, string> _localizationsPath = new();
        private static string[]? _availableLocalizations;
        private static SerializedLanguageInfo[] _languageInfo;

        private static readonly List<string> _identifiers = new();
        private static readonly List<string> _entries = new();
        private static int _currentLanguageIndex = 0;
        public static int CurrentLanguageIndex { get => _currentLanguageIndex; set => _currentLanguageIndex = value; }

        public static Dictionary<string, string> LocalizationPath => _localizationsPath;
        public static string[]? AvailableLocalizations => _availableLocalizations;
        public static bool MainStringTableAssigned { get; private set; }
        internal static bool LocalizationsExist => _availableLocalizations != null && _availableLocalizations.Length > 0;
        internal static string MainStringTablePath => MainWindow.ProjectFolderCombine(Locals.MainStringTablePath);
        internal static string LocalizationFolderPath => MainWindow.ProjectFolderCombine(CommonPaths.LocalizationFolderName);

        internal static int ItemsCount => _identifiers.Count;

        private static int _itemQueuedForDeletion = -1;

        private static SerializedLanguageInfo CurrentLanguageInfo => _languageInfo[_currentLanguageIndex];
        internal static string CurrentStringTablePath => Path.Combine(LocalizationFolderPath, $"{CurrentLanguageInfo.ShortName}{Locals.StringTableFileExtension}");

        static LocalizationData()
        {
            if (!Path.Exists(MainStringTablePath))
                return;
            FindLocalizations();
            if (LocalizationsExist)
                _currentLanguageIndex = 0;
        }

        internal static void FindLocalizations()
        {
            List<string> filesPaths = [];
            DirectoryInfo directoryInfo = new(LocalizationFolderPath);
            filesPaths.AddRange(from file in directoryInfo.GetFiles() where file.Name.EndsWith(Locals.LocalInfoExtension) select file.FullName);

            _localizationsPath.Clear();
            var info = new SerializedLanguageInfo[filesPaths.Count];
            for (var i = 0; i < filesPaths.Count; i++)
            {
                var content = File.ReadAllText(filesPaths[i]);
                var languageInfo = JsonConvert.DeserializeObject<SerializedLanguageInfo>(content);
                info[i] = languageInfo;
                _localizationsPath.Add(languageInfo.ShortName, filesPaths[i]);
            }
            _availableLocalizations = [.. _localizationsPath.Keys];
            _languageInfo = info;
        }

        internal static void LoadLocalization()
        {
            var path = Path.Combine(LocalizationFolderPath, $"{CurrentLanguageInfo.ShortName}{Locals.StringTableFileExtension}");
            var content = File.ReadAllText(path);
            if (string.IsNullOrEmpty(content))
                return;
            var stringTableData = JsonConvert.DeserializeObject<SerializedStringTable>(content);
            _entries.Clear();
            _entries.AddRange(stringTableData.Content);
            LoadMainStringTable();
            while (_identifiers.Count > _entries.Count)
            {
                _entries.Add("TO DEFINE");
            }
        }

        internal static void LoadMainStringTable()
        {
            var path = MainStringTablePath;
            var content = File.ReadAllText(path);
            if (string.IsNullOrEmpty(content))
            {
                _mainStringTable = new SerializedMainStringTable();
                return;
            }
            _mainStringTable = JsonConvert.DeserializeObject<SerializedMainStringTable>(content);
            _identifiers.Clear();
            _identifiers.AddRange(_mainStringTable.Identifier);
            MainStringTableAssigned = true;
        }

        public static string[] GetIdentifiers() => _identifiers.ToArray();
        public static string GetIdentifier(int index) => _identifiers[index];
        internal static string GetEntry(int index) => _entries[index];
        public static void SetIdentifierValue(int index, string value) => _identifiers[index] = value;
        public static void SetEntryValue(int index, string value) => _entries[index] = value;

        public static void AddEntry(string val) => _entries.Add(val);
        public static void AddIdentifier(string val) => _identifiers.Add(val);

        internal static void CreateEmptyMainStringTable()
        {
            _mainStringTable = new();
            MainStringTableAssigned = true;
        }

        internal static void SaveTable()
        {
            SerializedStringTable stringTable = new([.. _entries]);
            _mainStringTable = new([.. _identifiers]);

            File.WriteAllText(MainStringTablePath, JsonConvert.SerializeObject(_mainStringTable));
            File.WriteAllText(CurrentStringTablePath, JsonConvert.SerializeObject(stringTable));
        }

        internal static void QueueItemDeletion(int i) => _itemQueuedForDeletion = i;

        internal static void DeleteQueuedItem()
        {
            if (_itemQueuedForDeletion == -1)
                return;
            _identifiers.RemoveAt(_itemQueuedForDeletion);
            _entries.RemoveAt(_itemQueuedForDeletion);
            _itemQueuedForDeletion = -1;
        }
    }
}
