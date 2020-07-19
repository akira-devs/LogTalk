using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LogTalk.Utils
{
    /// <summary>
    /// ARPAbet 解析処理
    /// </summary>
    public static class Arpabet
    {
        /// <summary>
        /// ARPAbet 記号
        /// </summary>
        public enum Symbol
        {
            AA,
            AE,
            AH,
            AO,
            AW,
            AY,
            B,
            CH,
            D,
            DH,
            EH,
            ER,
            EY,
            F,
            G,
            HH,
            IH,
            IY,
            JH,
            K,
            L,
            M,
            N,
            NG,
            OW,
            OY,
            P,
            R,
            S,
            SH,
            T,
            TH,
            UH,
            UW,
            V,
            W,
            Y,
            Z,
            ZH,
        }

        /// <summary>
        /// ARPAbet 記号列の比較
        /// </summary>
        public class EnumListComparer<T> : IComparer<IReadOnlyList<T>> where T : Enum
        {
            /// <summary>
            /// デフォルト Comparer インスタンス
            /// </summary>
            public static IComparer<IReadOnlyList<T>> Default { get; } = new EnumListComparer<T>();

            /// <summary>
            /// 比較処理
            /// </summary>
            public int Compare(IReadOnlyList<T> x, IReadOnlyList<T> y)
            {
                // 要素同士の比較
                int length = Math.Min(x.Count, y.Count);
                for (int i = 0; i < length; i++)
                {
                    int compared = x[i].CompareTo(y[i]);
                    if (compared != 0) return compared;
                }

                // 配列長の比較
                return x.Count.CompareTo(y.Count);
            }
        }

        /// <summary>
        /// アクセント記号
        /// </summary>
        private static readonly Regex StressSuffix = new Regex(@"\d+$");

        /// <summary>
        /// ARPAbet 記号の文字列 -> Enum 変換
        /// </summary>
        public static Symbol Symbolize(string symbol) => (Symbol)Enum.Parse(typeof(Symbol), StressSuffix.Replace(symbol.Trim(), ""));

        /// <summary>
        /// ARPAbet 記号の文字列 -> Enum 変換
        /// </summary>
        public static IEnumerable<Symbol> Symbolize(IEnumerable<string> symbols) => symbols.Where(x => !string.IsNullOrWhiteSpace(x)).Select(Symbolize);

        /// <summary>
        /// ARPAbet 記号の文字列 -> Enum 変換
        /// </summary>
        public static Symbol? NullableSymbolize(string? symbol) => string.IsNullOrWhiteSpace(symbol) ? (Symbol?)null : Symbolize(symbol);

        /// <summary>
        /// ARPAbet 記号の文字列 -> Enum 変換
        /// </summary>
        public static IEnumerable<Symbol?> NullableSymbolize(IEnumerable<string?> symbols) => symbols.Select(NullableSymbolize);

        /// <summary>
        /// カタカナ変換テーブルの読み込み
        /// </summary>
        public static IReadOnlyList<(Symbol[], string?)> LoadKanaTable(TextReader reader)
        {
            // 子音列数
            const int RowHeaderCount = 2;

            var list = new List<(Symbol[], string?)>();

            // 母音行の読み込み
            string? line = reader.ReadLine();
            if (line == null) return list;

            var vowels = line.Split(',').Skip(RowHeaderCount).Select(NullableSymbolize).ToArray();

            // 子音＋発音行の読み込み
            while ((line = reader.ReadLine()) != null)
            {
                var values = line.Split(',');

                // 子音
                var consonants = values.Take(RowHeaderCount).Select(NullableSymbolize).ToArray();

                // 発音
                var vowel = new Symbol?[1];
                for (int i = 0; i < (values.Length - RowHeaderCount); i++)
                {
                    // ARPAbet 記号列
                    vowel[0] = vowels[i];
                    var symbols = consonants.Concat(vowel).Where(x => x != null).Select(x => x.Value).ToArray();
                    if (symbols.Length == 0) continue;

                    // 変換テーブルに追加
                    var kana = string.IsNullOrWhiteSpace(values[RowHeaderCount + i]) ? null : values[RowHeaderCount + i];
                    list.Add((symbols, kana));
                }
            }

            // ソート
            return list.OrderByDescending(x => x.Item1.Length).ThenBy(x => x.Item1, EnumListComparer<Symbol>.Default).ToList();
        }

        /// <summary>
        /// カタカナ変換テーブルの読み込み
        /// </summary>
        public static IReadOnlyList<(Symbol[], string?)> LoadKanaTable()
        {
            // デフォルトファイル
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var dir = Path.GetDirectoryName(assembly.Location);
            var file = Path.Combine(dir, "arpabet-kana.csv");

            // 読み込み
            using (var reader = new StreamReader(file, Encoding.UTF8))
            {
                return LoadKanaTable(reader);
            }
        }

        /// <summary>
        /// ARPAbet 記号列のカタカナ変換
        /// </summary>
        public static bool TryParse(IReadOnlyList<(Symbol[], string?)> table, IReadOnlyList<Symbol> symbols, out string? kana)
        {
            var builder = new StringBuilder();
            for (int offset = 0; offset < symbols.Count;)
            {
                // 変換テーブルの検索
                int matched = 0;
                foreach (var pattern in table)
                {
                    // シンボル長のチェック
                    if (offset + pattern.Item1.Length > symbols.Count) continue;

                    // マッチング
                    bool mismatch = false;
                    for (int i = 0; i < pattern.Item1.Length; i++)
                    {
                        if (pattern.Item1[i] != symbols[offset + i])
                        {
                            mismatch = true;
                            break;
                        }
                    }
                    if (mismatch) continue;

                    // カナ変換
                    if (string.IsNullOrEmpty(pattern.Item2)) throw new ArgumentException("Not found conversion table: " + string.Join(" ", pattern.Item1.Select(x => x.ToString())));
                    matched = pattern.Item1.Length;
                    builder.Append(pattern.Item2);
                    break;
                }

                // 変換テーブルに一致なし
                if (matched == 0)
                {
                    kana = null;
                    return false;
                }

                // オフセット更新
                offset += matched;
            }

            // 変換
            kana = builder.ToString();
            return true;
        }

        /// <summary>
        /// ARPAbet 記号列のカタカナ変換
        /// </summary>
        public static bool TryParse(IReadOnlyList<(Symbol[], string?)> table, IEnumerable<string> symbols, out string? kana) => TryParse(table, Symbolize(symbols).ToList(), out kana);
    }
}
