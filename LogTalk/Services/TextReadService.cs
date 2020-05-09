using System;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Text;

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
        /// 区切り文字
        /// </summary>
        protected static readonly char[] SplitChars = new char[]
        {
            '\r',
            '\n',
            '。',
            '．',
            '？',
            '?',
            '！',
            '!',
        };

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
        protected Decoder? decoder = null;
        /// <summary>
        /// テキストバッファ
        /// </summary>
        protected readonly char[] chars = new char[4 << 10];
        /// <summary>
        /// テキスト有効サイズ
        /// </summary>
        protected int available = 0;

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

            // FileSystemWatcher 設定
            watcher.Path = info.DirectoryName;
            watcher.Filter = info.Name;

            // 初期化
            Reset();
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
        /// 内部バッファの初期化
        /// </summary>
        protected void Reset()
        {
            offset = 0;
            decoder?.Reset();
            available = 0;
        }

        /// <summary>
        /// Changed イベント
        /// </summary>
        protected void OnChanged(object sender, FileSystemEventArgs e)
        {
            // ファイル削除
            if (e.ChangeType == WatcherChangeTypes.Deleted)
            {
                // 未通知テキストの通知
                if (available > 0)
                {
                    var text = new string(chars, 0, available).Trim();
                    if (text.Length > 0) subject.OnNext(text);
                }

                // 初期化
                Reset();
                return;
            }

            // ファイルの継続読み込み
            using (var stream = new FileStream(e.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
            {
                // ストリームサイズの確認
                if (stream.Length < offset)
                {
                    // 未通知テキストの通知
                    if (available > 0)
                    {
                        var text = new string(chars, 0, available).Trim();
                        if (text.Length > 0) subject.OnNext(text);
                    }

                    // 初期化
                    Reset();
                    offset = stream.Length;
                    return;
                }

                // 前回の読み込み位置へ移動
                stream.Seek(offset, SeekOrigin.Begin);

                // テキスト読み込み
                ReadText(stream);

                // 今回の読み込み位置を保存
                offset = stream.Position;
            }
        }

        /// <summary>
        /// テキスト読み込み
        /// </summary>
        protected void ReadText(Stream stream)
        {
            // 読み込み
            int count = 0;
            var bytes = new byte[4 << 10];
            while ((count = stream.Read(bytes, 0, bytes.Length)) > 0)
            {
                // テキスト変換
                int decoded = decoder.GetChars(bytes, 0, count, chars, available);
                available += decoded;

                // テキスト分解
                int start = 0;
                int end = 0; ;
                for (int i = 0; i < available; i++)
                {
                    if (SplitChars.Contains(chars[i]))
                    {
                        // 区切り文字の位置を終端位置に設定
                        end = i + 1;
                    }
                    else if (chars[i] == '.' && (i + i >= available || char.IsWhiteSpace(chars[i + 1])))
                    {
                        // 次の文字が終端、空白のピリオドの位置を終端位置に設定
                        end = i + 1;
                    }

                    // テキスト通知
                    if (start < end)
                    {
                        var text = new string(chars, start, end - start).Trim();
                        if (text.Length > 0) subject.OnNext(text);
                        start = end;
                    }
                }

                // 未通知領域を先頭に移動
                if (end < available) Array.Copy(chars, end, chars, 0, available - end);
                available -= end;
            }
        }
    }
}
