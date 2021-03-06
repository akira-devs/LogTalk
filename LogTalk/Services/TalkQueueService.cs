﻿using LogTalk.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogTalk.Services
{
    /// <summary>
    /// トークキューイングサービス
    /// </summary>
    public interface ITalkQueueService
    {
        /// <summary>
        /// キューイング
        /// </summary>
        void Enqueue(string text);
    }

    /// <summary>
    /// トークキューイングサービス
    /// </summary>
    public class TalkQueueService : IDisposable, ITalkQueueService
    {
        /// <summary>
        /// トークサービス
        /// </summary>
        protected ITalkService TalkService { get; }
        /// <summary>
        /// 翻訳処理
        /// </summary>
        protected ITranslator Translator { get; }

        /// <summary>
        /// キュー
        /// </summary>
        protected readonly Queue<string> queue = new Queue<string>();
        /// <summary>
        /// タスク
        /// </summary>
        protected Task? task = null;

        /// <summary>
        /// コンストラクター
        /// </summary>
        public TalkQueueService(ITalkService talkService, ITranslator translator)
        {
            TalkService = talkService;
            Translator = translator;
        }

        /// <summary>
        /// ファイナライザー
        /// </summary>
        ~TalkQueueService()
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
            if (task != null)
            {
                if (disposing)
                {
                    lock (queue)
                    {
                        queue.Clear();
                        task = null;
                    }

                    TalkService.Stop();
                }
            }
        }

        /// <summary>
        /// キューイング
        /// </summary>
        public void Enqueue(string text)
        {
            lock (queue)
            {
                // 翻訳
                text = Translator.Translate(text);

                // キューの追加
                queue.Enqueue(text);

                // タスク生成
                if (task == null)
                {
                    task = Task.Factory.StartNew(Consume, TaskCreationOptions.LongRunning);
                }
            }
        }

        /// <summary>
        /// キューの処理
        /// </summary>
        protected void Consume()
        {
            while (true)
            {
                string text = "";
                lock (queue)
                {
                    // キューが空の場合、タスク終了
                    if (queue.Count == 0)
                    {
                        task = null;
                        return;
                    }

                    // キューの取り出し
                    text = queue.Dequeue();
                    if (string.IsNullOrWhiteSpace(text)) continue;
                }

                // 音声再生
                var state = TalkService.Speak(text);
                state.Wait();
            }
        }
    }
}
