using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogTalk.Utils
{
    /// <summary>
    /// カーネギーメロン大学 発音辞書
    /// </summary>
    public class CmuPronouncingDictionary
    {
        /// <summary>
        /// コメントパターン
        /// </summary>
        protected static readonly Regex Comment = new Regex(@"#.*$");
        /// <summary>
        /// 空白文字パターン
        /// </summary>
        protected static readonly Regex Whitespaces = new Regex(@"\s+");
        /// <summary>
        /// 英単語パターン
        /// </summary>
        protected static readonly Regex Word = new Regex(@"^[a-z]", RegexOptions.IgnoreCase);
        /// <summary>
        /// 重複単語サフィックス
        /// </summary>
        protected static readonly Regex WordSuffix = new Regex(@"\(\d+\)$");

        /// <summary>
        /// 発音辞書の読み込み
        /// </summary>
        public static IDictionary<string, string> Load(TextReader reader, IReadOnlyList<(Arpabet.Symbol[], string?)> arpabet)
        {
            var dictionary = new Dictionary<string, string>();

            string line;
            while ((line = reader.ReadLine()) != null)
            {
                ParseLine(dictionary, arpabet, line);
            }

            return dictionary;
        }

        /// <summary>
        /// 発音辞書の読み込み
        /// </summary>
        public static IDictionary<string, string> Load()
        {
            // ARPAbet 辞書の読み込み
            var arpabet = Arpabet.LoadKanaTable();

            // デフォルトファイル
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.Location);
            var file = Path.Combine(dir, "cmudict.dict");

            // 読み込み
            using (var reader = new StreamReader(file, Encoding.UTF8))
            {
                return Load(reader, arpabet);
            }
        }

        /// <summary>
        /// 行の解析
        /// </summary>
        protected static void ParseLine(IDictionary<string, string> dictionary, IReadOnlyList<(Arpabet.Symbol[], string?)> arpabet, string line)
        {
            // コメント除去
            line = Comment.Replace(line, "").Trim();
            if (string.IsNullOrWhiteSpace(line)) return;

            // 空白で分割
            var values = Whitespaces.Split(line);
            if (values.Length < 2) return;

            // 英単語の取得
            var word = WordSuffix.Replace(values[0], "").ToLower();
            if (!Word.IsMatch(word)) return;
            if (dictionary.ContainsKey(word)) return;

            // ARPAbet の変換
            if (Arpabet.TryParse(arpabet, values.Skip(1), out var pronunciation))
            {
                // 単語辞書登録
                dictionary.Add(word, pronunciation);
            }
            else
            {
                throw new ArgumentException("Can't parse ARPAbet: " + line);
            }
        }
    }
}
