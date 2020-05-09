using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reactive.Linq;
using LogTalk.Services;

namespace LogTalk.ViewModels
{
    /// <summary>
    /// メインウィンドウ
    /// </summary>
    public class MainWindowViewModel : BindableBase
    {
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
        public MainWindowViewModel(IOpenFileDialogService openFileDialogService, ITextReadService textReadService, ITalkService talkService, ITalkQueueService talkQueueService)
        {
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

            OpenCommand.Subscribe(Open);

            ToggleWatchCommand.Subscribe(ToggleWatch);
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
