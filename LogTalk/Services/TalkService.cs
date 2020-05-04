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
    public class TalkService : IDisposable
    {
        /// <summary>
        /// 利用可能なキャスト名
        /// </summary>
        public string[] AvailableCasts => TalkerAgent.AvailableCasts;

        /// <summary>
        /// Talker
        /// </summary>
        public Talker? Talker { get; protected set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public TalkService()
        {
            // CeVIO Creative Studio 起動
            ServiceControl.StartHost(true);

            // Talker 生成
            Talker = new Talker(TalkerAgent.AvailableCasts.FirstOrDefault());
            Talker.Volume = 100;
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
            if (Talker != null)
            {
                if (disposing)
                {
                    // トーク停止
                    Talker.Stop();
                }

                // Talker 開放
                Talker = null;

                // CeVIO Creative Studio 終了
                ServiceControl.CloseHost();
            }
        }

    }
}
