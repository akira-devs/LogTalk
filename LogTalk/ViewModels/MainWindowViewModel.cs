using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reactive.Linq;
using LogTalk.Services;
using LogTalk.Services.Extensions;

namespace LogTalk.ViewModels
{
    /// <summary>
    /// メインウィンドウ
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
        /// <summary>
        /// アプリケーション設定サービス
        /// </summary>
        protected IAppSettingsService AppSettingsService { get; }
        /// <summary>
        /// ファイル選択ダイアログサービス
        /// </summary>
        protected IOpenFileDialogService OpenFileDialogService { get; }
        /// <summary>
        /// トークテキスト読み込みサービス
        /// </summary>
        protected ITextReadService TextReadService { get; }
        /// <summary>
        /// CeVIO トークサービス
        /// </summary>
        protected ITalkService TalkService { get; }
        /// <summary>
        /// トークキューイングサービス
        /// </summary>
        protected ITalkQueueService TalkQueueService { get; }

        /// <summary>
        /// 入力ファイル
        /// </summary>
        public ReactiveProperty<string?> InputFile { get; } = new ReactiveProperty<string?>();
        /// <summary>
        /// 利用可能なキャスト名
        /// </summary>
        public string[] Casts => TalkService.AvailableCasts;
        /// <summary>
        /// 選択されたキャスト名
        /// </summary>
        public ReactiveProperty<string?> SelectedCast { get; } = new ReactiveProperty<string?>();
        /// <summary>
        /// 音の大きさ
        /// </summary>
        [Range(0, 100)]
        public ReactiveProperty<uint> Volume { get; } = new ReactiveProperty<uint>();
        /// <summary>
        /// 話す速さ
        /// </summary>
        [Range(0, 100)]
        public ReactiveProperty<uint> Speed { get; } = new ReactiveProperty<uint>();
        /// <summary>
        /// 音の高さ
        /// </summary>
        [Range(0, 100)]
        public ReactiveProperty<uint> Tone { get; } = new ReactiveProperty<uint>();
        /// <summary>
        /// 声質
        /// </summary>
        [Range(0, 100)]
        public ReactiveProperty<uint> Alpha { get; } = new ReactiveProperty<uint>();
        /// <summary>
        /// 抑揚
        /// </summary>
        [Range(0, 100)]
        public ReactiveProperty<uint> ToneScale { get; } = new ReactiveProperty<uint>();
        /// <summary>
        /// 設定ファイルの読み込みコマンド
        /// </summary>
        public ReactiveCommand LoadSettingsCommand { get; } = new ReactiveCommand();
        /// <summary>
        /// 設定ファイルの保存コマンド
        /// </summary>
        public ReactiveCommand SaveSettingsCommand { get; } = new ReactiveCommand();
        /// <summary>
        /// 開くコマンド
        /// </summary>
        public ReactiveCommand OpenCommand { get; } = new ReactiveCommand();
        /// <summary>
        /// 監視切り替えコマンド
        /// </summary>
        public ReactiveCommand ToggleWatchCommand { get; } = new ReactiveCommand();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public MainWindowViewModel(IAppSettingsService appSettingsService, IOpenFileDialogService openFileDialogService, ITextReadService textReadService, ITalkService talkService, ITalkQueueService talkQueueService)
        {
            AppSettingsService = appSettingsService;
            OpenFileDialogService = openFileDialogService;
            TextReadService = textReadService;
            TalkService = talkService;
            TalkQueueService = talkQueueService;

            TextReadService.Subscribe(TalkQueueService.Enqueue);

            SelectedCast.Value = TalkService.Cast;
            SelectedCast.Subscribe(x => TalkService.Cast = x);

            Volume.Value = TalkService.Volume;
            Volume.Subscribe(x => TalkService.Volume = x);

            Speed.Value = TalkService.Speed;
            Speed.Subscribe(x => TalkService.Speed = x);

            Tone.Value = TalkService.Tone;
            Tone.Subscribe(x => TalkService.Tone = x);

            Alpha.Value = TalkService.Alpha;
            Alpha.Subscribe(x => TalkService.Alpha = x);

            ToneScale.Value = TalkService.ToneScale;
            ToneScale.Subscribe(x => TalkService.ToneScale = x);

            LoadSettingsCommand.Subscribe(LoadSettings);
            SaveSettingsCommand.Subscribe(SaveSettings);

            OpenCommand.Subscribe(Open);

            ToggleWatchCommand.Subscribe(ToggleWatch);
        }

        /// <summary>
        /// 設定の読み込み
        /// </summary>
        protected void LoadSettings()
        {
            // サービス利用可能チェック
            if (!TalkService.IsHostStarted) return;

            // 読み込み
            AppSettingsService.Load();

            // 設定反映
            AppSettingsService.TryGetProperty(nameof(InputFile), InputFile);
            AppSettingsService.TryGetProperty(nameof(SelectedCast), SelectedCast);
            AppSettingsService.TryGetProperty(nameof(Volume), Volume);
            AppSettingsService.TryGetProperty(nameof(Speed), Speed);
            AppSettingsService.TryGetProperty(nameof(Tone), Tone);
            AppSettingsService.TryGetProperty(nameof(Alpha), Alpha);
            AppSettingsService.TryGetProperty(nameof(ToneScale), ToneScale);
        }

        /// <summary>
        /// 設定の書き込み
        /// </summary>
        protected void SaveSettings()
        {
            // サービス利用可能チェック
            if (!TalkService.IsHostStarted) return;

            // 設定反映
            AppSettingsService[nameof(InputFile)] = InputFile.Value;
            AppSettingsService[nameof(SelectedCast)] = SelectedCast.Value;
            AppSettingsService[nameof(Volume)] = Volume.Value.ToString();
            AppSettingsService[nameof(Speed)] = Speed.Value.ToString();
            AppSettingsService[nameof(Tone)] = Tone.Value.ToString();
            AppSettingsService[nameof(Alpha)] = Alpha.Value.ToString();
            AppSettingsService[nameof(ToneScale)] = ToneScale.Value.ToString();

            // 書き込み
            AppSettingsService.Save();
        }

        protected void Open()
        {
            var filename = OpenFileDialogService.ShowDialog();
            if (filename != null) InputFile.Value = filename;
        }

        protected void ToggleWatch()
        {
            if (TextReadService.IsRunning)
            {
                TextReadService.Stop();
            }
            else
            {
                var filename = InputFile.Value;
                if (filename != null && File.Exists(filename)) TextReadService.Start(filename);
            }
        }


    }
}
