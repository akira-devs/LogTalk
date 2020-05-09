using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reactive.Linq;
using System.Text;
using System.Windows;
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
        protected TalkService TalkService { get; }

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
        public MainWindowViewModel(IOpenFileDialogService openFileDialogService, ITextReadService textReadService, TalkService talkService)
        {
            OpenFileDialogService = openFileDialogService;
            TextReadService = textReadService;
            TalkService = talkService;

            TextReadService.Subscribe(x => TalkService.Talker.Speak(x));

            SelectedCast.Value = TalkService.Talker.Cast;
            SelectedCast.Subscribe(x => TalkService.Talker.Cast = x);

            Volume.Value = TalkService.Talker.Volume;
            Volume.Subscribe(x => TalkService.Talker.Volume = x);

            Speed.Value = TalkService.Talker.Speed;
            Speed.Subscribe(x => TalkService.Talker.Speed = x);

            Tone.Value = TalkService.Talker.Tone;
            Tone.Subscribe(x => TalkService.Talker.Tone = x);

            Alpha.Value = TalkService.Talker.Alpha;
            Alpha.Subscribe(x => TalkService.Talker.Alpha = x);

            ToneScale.Value = TalkService.Talker.ToneScale;
            ToneScale.Subscribe(x => TalkService.Talker.ToneScale = x);

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
                TextReadService.Start(InputFile.Value);
            }
        }


    }
}
