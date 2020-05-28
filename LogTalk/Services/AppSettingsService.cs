using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LogTalk.Services
{
    /// <summary>
    /// アプリケーション設定サービス
    /// </summary>
    public interface IAppSettingsService : IDictionary<string, string>
    {
        /// <summary>
        /// 設定ファイルの読み込み
        /// </summary>
        void Load();
        /// <summary>
        /// 設定ファイルの書き込み
        /// </summary>
        void Save();
    }
    /// <summary>
    /// アプリケーション設定サービス
    /// </summary>
    public class AppSettingsService : Dictionary<string, string>, IAppSettingsService
    {
        /// <summary>
        /// アプリケーション設定ファイル名
        /// </summary>
        protected const string AppSettingsName = "AppSettings.json";

        /// <summary>
        /// アプリケーション設定ディレクトリ
        /// </summary>
        protected string AppDataDirectory
        {
            get
            {
                // アプリケーション名
                var assembly = Assembly.GetEntryAssembly();
                var name = assembly.GetName().Name;

                // 設定ディレクトリ
                var directory = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                directory = Path.Combine(directory, assembly.GetName().Name);

                return directory;
            }
        }

        /// <summary>
        /// アプリケーション設定ファイル
        /// </summary>
        protected string AppSettingsFile => Path.Combine(AppDataDirectory, AppSettingsName);

        /// <summary>
        /// 設定ファイルの読み込み
        /// </summary>
        public void Load()
        {
            // リセット
            Clear();

            // ファイルの存在チェック
            if (!File.Exists(AppSettingsFile)) return;

            // 読み込み
            using (var stream = new FileStream(AppSettingsFile, FileMode.Open, FileAccess.Read))
            {
                var dictionary = JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream).Result;
                foreach (var entry in dictionary)
                {
                    this[entry.Key] = entry.Value;
                }
            }
        }

        /// <summary>
        /// 設定ファイルの書き込み
        /// </summary>
        public void Save()
        {
            // ディレクトリの存在チェック
            if (!Directory.Exists(AppDataDirectory))
            {
                Directory.CreateDirectory(AppDataDirectory);
            }

            // 書き込み
            using (var stream = new FileStream(AppSettingsFile, FileMode.Create, FileAccess.Write))
            {
                var task = JsonSerializer.SerializeAsync<Dictionary<string, string>>(stream, this);
                task.Wait();
            }

        }
    }
}
