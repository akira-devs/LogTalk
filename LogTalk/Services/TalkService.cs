using CeVIO.Talk.RemoteService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogTalk.Services
{
    /// <summary>
    /// CeVIO トークサービス
    /// </summary>
    public interface ITalkService
    {
        /// <summary>
        /// CeVIO Creative Studio の起動状態
        /// </summary>
        bool IsHostStarted { get; }
        /// <summary>
        /// 利用可能なキャスト名
        /// </summary>
        string[] AvailableCasts { get; }
        /// <summary>
        /// キャスト
        /// </summary>
        string? Cast { get; set; }
        /// <summary>
        /// 音の大きさ
        /// </summary>
        uint Volume { get; set; }
        /// <summary>
        /// 話す速さ
        /// </summary>
        uint Speed { get; set; }
        /// <summary>
        /// 音の高さ
        /// </summary>
        uint Tone { get; set; }
        /// <summary>
        /// 声質
        /// </summary>
        uint Alpha { get; set; }
        /// <summary>
        /// 抑揚
        /// </summary>
        uint ToneScale { get; set; }
        /// <summary>
        /// 発声
        /// </summary>
        ISpeakingState Speak(string text);
        /// <summary>
        /// 停止
        /// </summary>
        bool Stop();
    }

    /// <summary>
    /// CeVIO トークサービス
    /// </summary>
    public class TalkService : IDisposable, ITalkService
    {
        /// <summary>
        /// CeVIO Creative Studio の起動状態
        /// </summary>
        public bool IsHostStarted => ServiceControl.IsHostStarted;
        /// <summary>
        /// 利用可能なキャスト名
        /// </summary>
        public string[] AvailableCasts => TalkerAgent.AvailableCasts;
        /// <summary>
        /// キャスト
        /// </summary>
        public string? Cast
        {
            get => talker?.Cast;
            set { if (talker != null) talker.Cast = value; }
        }
        /// <summary>
        /// 音の大きさ
        /// </summary>
        public uint Volume
        {
            get => talker?.Volume ?? 0;
            set { if (talker != null) talker.Volume = value; }
        }
        /// <summary>
        /// 話す速さ
        /// </summary>
        public uint Speed
        {
            get => talker?.Speed ?? 0;
            set { if (talker != null) talker.Speed = value; }
        }
        /// <summary>
        /// 音の高さ
        /// </summary>
        public uint Tone
        {
            get => talker?.Tone ?? 0;
            set { if (talker != null) talker.Tone = value; }
        }
        /// <summary>
        /// 声質
        /// </summary>
        public uint Alpha
        {
            get => talker?.Alpha ?? 0;
            set { if (talker != null) talker.Alpha = value; }
        }
        /// <summary>
        /// 抑揚
        /// </summary>
        public uint ToneScale
        {
            get => talker?.ToneScale ?? 0;
            set { if (talker != null) talker.ToneScale = value; }
        }

        /// <summary>
        /// Talker
        /// </summary>
        protected Talker? talker = null;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public TalkService()
        {
            // CeVIO Creative Studio 起動
            ServiceControl.StartHost(false);

            // Talker 生成
            talker = new Talker(TalkerAgent.AvailableCasts.FirstOrDefault());
        }

        /// <summary>
        /// ファイナライザー
        /// </summary>
        ~TalkService()
        {
            Dispose(false);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (talker != null)
            {
                if (disposing)
                {
                    // トーク停止
                    talker.Stop();
                }

                // Talker 開放
                talker = null;

                // CeVIO Creative Studio 終了
                ServiceControl.CloseHost();
            }
        }

        /// <summary>
        /// 発声
        /// </summary>
        public ISpeakingState Speak(string text)
        {
            if (talker == null) throw new ObjectDisposedException(nameof(TalkService));
            return talker.Speak(text);
        }

        /// <summary>
        /// 停止
        /// </summary>
        public bool Stop()
        {
            if (talker == null) throw new ObjectDisposedException(nameof(TalkService));
            return talker.Stop();
        }

    }
}
