using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LogTalk.Services
{
    /// <summary>
    /// トークテキスト読み込みサービス
    /// </summary>
    public interface ITextReadService : IObservable<string>
    {
        /// <summary>
        /// テキストファイルのエンコーディング
        /// </summary>
        Encoding Encoding { get; set; }
        /// <summary>
        /// 実行状態
        /// </summary>
        bool IsRunning { get; }
        /// <summary>
        /// トークテキスト読み込み開始
        /// </summary>
        void Start(string filename);
        /// <summary>
        /// トークテキスト読み込み停止
        /// </summary>
        void Stop();
    }

    /// <summary>
    /// トークテキスト読み込みサービス
    /// </summary>
    public class TextReadService : IDisposable, ITextReadService
    {
        /// <summary>
        /// テキストファイルのエンコーディング
        /// </summary>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Observable ソース
        /// </summary>
        protected readonly Subject<string> subject = new Subject<string>();
        /// <summary>
        /// ファイルシステム監視
        /// </summary>
        protected FileSystemWatcher? watcher = new FileSystemWatcher();
        /// <summary>
        /// 前回の読み込み位置
        /// </summary>
        protected long offset = 0;
        /// <summary>
        /// テキストデコーダー
        /// </summary>
        protected Decoder decoder = null;

        /// <summary>
        /// 実行状態
        /// </summary>
        public bool IsRunning => watcher?.EnableRaisingEvents ?? false;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public TextReadService()
        {
            // FileSystemWatcher 初期化
            watcher.IncludeSubdirectories = false;
            watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.Size | NotifyFilters.LastWrite | NotifyFilters.LastAccess;
            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Deleted += OnChanged;
            watcher.Renamed += OnChanged;
        }

        /// <summary>
        /// ファイナライザー
        /// </summary>
        ~TextReadService() => Dispose(false);

        /// <summary>
        /// Dispose 実装
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose 実装
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (watcher != null)
            {
                if (disposing)
                {
                    watcher.Dispose();
                }

                watcher = null;
            }
        }

        /// <summary>
        /// Subscribe 実装
        /// </summary>
        public IDisposable Subscribe(IObserver<string> observer) => subject.Subscribe(observer);

        /// <summary>
        /// トークテキスト読み込み開始
        /// </summary>
        public void Start(string filename)
        {
            if (watcher == null) throw new ObjectDisposedException(nameof(TextReadService));

            var info = new FileInfo(filename);

            watcher.Path = info.DirectoryName;
            watcher.Filter = info.Name;
            offset = info.Length;
            decoder = Encoding.GetDecoder();

            watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// トークテキスト読み込み停止
        /// </summary>
        public void Stop()
        {
            if (watcher == null) throw new ObjectDisposedException(nameof(TextReadService));
            watcher.EnableRaisingEvents = false;
        }

        /// <summary>
        /// Changed イベント
        /// </summary>
        protected void OnChanged(object sender, FileSystemEventArgs e)
        {
            using (var stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                stream.Seek(offset, SeekOrigin.Begin);

                int count = 0;
                var bytes = new byte[4 << 10];
                var chars = new char[4 << 10];
                while ((count = stream.Read(bytes, 0, bytes.Length)) > 0)
                {
                    int decoded = decoder.GetChars(bytes, 0, count, chars, 0);
                    subject.OnNext(new string(chars, 0, decoded));
                }

                offset = stream.Position;
            }
        }
    }
}
