using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LogTalk.Services
{
    /// <summary>
    /// 英単語の発声カタカナ 疑似変換サービス
    /// </summary>
    public interface IKatakotoService
    {
        /// <summary>
        /// 変換
        /// </summary>
        string Translate(string text);
    }

    /// <summary>
    /// 英単語の発声カタカナ 疑似変換サービス
    /// </summary>
    public class KatakotoService : IKatakotoService
    {
        /// <summary>
        /// 英単語パターン
        /// </summary>
        protected static readonly Regex WordPattern = new Regex(@"[A-Z]?[a-z]+");
        /// <summary>
        /// 母音パターン
        /// </summary>
        protected static readonly Regex VowelPattern = new Regex(@"^[aiueoy]");
        /// <summary>
        /// 発声ルール
        /// </summary>
        protected static readonly IReadOnlyList<KeyValuePair<string, string>> SpeakingRules = new Dictionary<string, string>
        {
            { "u", "a" },
            { "w", "u" },
            { "x", "ks" },

            { "ai", "ei" },
            { "ay", "ei" },
            { "ea", "i-" },
            { "ee", "i-" },
            { "ey", "i-" },
            { "ie", "ai" },
            { "oa", "o-" },
            { "ue", "u-" },
            { "ui", "u-" },

            { "sh", "s" },
            { "ch", "ch" },
            { "ph", "f" },
            { "wh", "f" },
            { "th", "s" },
            { "ck", "ッk" },
            { "ng", "ng" },
            { "gh", "" },

            { "gg", "ッg" },
            { "ll", "r" },
            { "mm", "m" },
            { "rr", "r" },
            { "ss", "ッs" },

            { "oo", "u-" },
            { "ou", "au" },
            { "ow", "au" },
            { "oi", "oi" },
            { "oy", "oi" },
            { "au", "ou" },
            { "aw", "ou" },

            { "ar", "a-" },
            { "er", "a-" },
            { "ir", "a-" },
            { "or", "o-" },
            { "air", "ea" },
            { "ear", "ia" },
            { "wor", "wa-" },

            { "tch", "ッch" },

            { "ion", "iョン" },
            { "tion", "ション" },
        }.OrderByDescending(x => x.Key.Length).ThenBy(x => x.Key).ToList();
        /// <summary>
        /// カナ変換ルール
        /// </summary>
        protected static readonly IReadOnlyList<KeyValuePair<string, string>> KanaRules = new Dictionary<string, string>
        {
            // 単記号
            { "a", "ア" },
            { "b", "ブ" },
            { "c", "ク" },
            { "d", "ド" },
            { "e", "エ" },
            { "f", "フ" },
            { "g", "グ" },
            { "h", "ハ" },
            { "i", "イ" },
            { "j", "ジ" },
            { "k", "ク" },
            { "l", "ル" },
            { "m", "ム" },
            { "n", "ン" },
            { "o", "オ" },
            { "p", "プ" },
            { "q", "ク" },
            { "r", "ル" },
            { "s", "ス" },
            { "t", "ト" },
            { "u", "ウ" },
            { "v", "ヴ" },
            { "w", "ウ" },
            { "x", "クス" },
            { "y", "イ" },
            { "z", "ズ" },
            { "-", "ー" },

            // 子音 + 母音
            { "ba", "バ" },
            { "bi", "ビ" },
            { "bu", "ブ" },
            { "be", "ベ" },
            { "bo", "ボ" },
            { "by", "ビィ" },
            { "ca", "キャ" },
            { "ci", "キ" },
            { "cu", "ク" },
            { "ce", "セ" },
            { "co", "コ" },
            { "cha", "チャ" },
            { "chi", "チ" },
            { "chu", "チュ" },
            { "che", "チェ" },
            { "cho", "チョ" },
            { "chy", "チ" },
            { "da", "ダ" },
            { "di", "ディ" },
            { "du", "ドゥ" },
            { "de", "デ" },
            { "do", "ド" },
            { "dy", "ディ" },
            { "fa", "ファ" },
            { "fi", "フィ" },
            { "fu", "フ" },
            { "fe", "フェ" },
            { "fo", "フォ" },
            { "fy", "フィ" },
            { "ga", "ガ" },
            { "gi", "ギ" },
            { "gu", "グ" },
            { "ge", "ゲ" },
            { "go", "ゴ" },
            { "gy", "ギィ" },
            { "ha", "ハ" },
            { "hi", "ヒ" },
            { "hu", "フ" },
            { "he", "ヘ" },
            { "ho", "ホ" },
            { "hy", "ヒィ" },
            { "ja", "ジャ" },
            { "ji", "ジ" },
            { "ju", "ジュ" },
            { "je", "ジェ" },
            { "jo", "ジョ" },
            { "jy", "ジィ" },
            { "ka", "カ" },
            { "ki", "キ" },
            { "ku", "ク" },
            { "ke", "ケ" },
            { "ko", "コ" },
            { "ky", "キィ" },
            { "la", "ラ" },
            { "li", "リ" },
            { "lu", "ル" },
            { "le", "レ" },
            { "lo", "ロ" },
            { "ly", "リィ" },
            { "ma", "マ" },
            { "mi", "ミ" },
            { "mu", "ム" },
            { "me", "メ" },
            { "mo", "モ" },
            { "my", "ミィ" },
            { "na", "ナ" },
            { "ni", "ニ" },
            { "nu", "ヌ" },
            { "ne", "ネ" },
            { "no", "ノ" },
            { "ny", "ニィ" },
            { "pa", "パ" },
            { "pi", "ピ" },
            { "pu", "プ" },
            { "pe", "ペ" },
            { "po", "ポ" },
            { "py", "ピィ" },
            { "qa", "カ" },
            { "qi", "キ" },
            { "qu", "ク" },
            { "qe", "ケ" },
            { "qo", "コ" },
            { "qy", "キィ" },
            { "ra", "ラ" },
            { "ri", "リ" },
            { "ru", "ル" },
            { "re", "レ" },
            { "ro", "ロ" },
            { "ry", "リィ" },
            { "sa", "サ" },
            { "si", "シ" },
            { "su", "ス" },
            { "se", "セ" },
            { "so", "ソ" },
            { "sy", "シィ" },
            { "ta", "タ" },
            { "ti", "ティ" },
            { "tu", "トゥ" },
            { "te", "テ" },
            { "to", "ト" },
            { "ty", "ティ" },
            { "va", "バ" },
            { "vi", "ビ" },
            { "vu", "ブ" },
            { "ve", "ベ" },
            { "vo", "ボ" },
            { "vy", "ビィ" },
            { "za", "ザ" },
            { "zi", "ジ" },
            { "zu", "ズ" },
            { "ze", "ゼ" },
            { "zo", "ゾ" },
            { "zy", "ジィ" },
        }.OrderByDescending(x => x.Key.Length).ThenBy(x => x.Key).ToList();

        /// <summary>
        /// 単語発声辞書
        /// </summary>
        protected IDictionary<string, string> Words { get; } = new Dictionary<string, string>();

        /// <summary>
        /// コンストラクター
        /// </summary>
        public KatakotoService() => LoadWords();

        /// <summary>
        /// 単語辞書の読み込み
        /// </summary>
        protected void LoadWords()
        {
            var assembly = Assembly.GetEntryAssembly();

            var dirs = new List<string>();
            dirs.Add(Path.GetDirectoryName(assembly.Location));
            dirs.Add(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), assembly.GetName().Name));

            foreach (var dir in dirs)
            {
                // 単語辞書
                var filename = Path.Combine(dir, "words.json");
                if (!File.Exists(filename)) continue;

                // 読み込み
                using (var stream = new FileStream(filename, FileMode.Open, FileAccess.Read))
                {
                    var dictionary = JsonSerializer.DeserializeAsync<Dictionary<string, string>>(stream).Result;
                    foreach (var entry in dictionary)
                    {
                        Words[entry.Key] = entry.Value;
                    }
                }
            }
        }


        /// <summary>
        /// 変換
        /// </summary>
        public string Translate(string text) => WordPattern.Replace(text, Translate);

        /// <summary>
        /// 変換
        /// </summary>
        protected string Translate(Match match)
        {
            // 単語辞書検索
            if (Words.TryGetValue(match.Value, out var value))
            {
                return value;
            }

            // 小文字変換
            var text = match.Value.ToLower();

            // 発声記号に変換
            var list = TranslateRules(SpeakingRules, text);
            text = string.Join("", list.Select(x => x.Value));

            // カタカナに変換
            list = TranslateRules(KanaRules, text);
            text = string.Join("", list.Select(x => x.Value));

            return text;
        }

        /// <summary>
        /// 記号変換
        /// </summary>
        protected List<KeyValuePair<string, string>> TranslateRules(IEnumerable<KeyValuePair<string, string>> rules, string text)
        {
            // 変換リスト
            var list = new List<KeyValuePair<string, string>>();

            // 変換処理
            for (int offset = 0; offset < text.Length;)
            {
                // パターンマッチング
                KeyValuePair<string, string>? match = null;
                foreach (var rule in rules)
                {
                    if (offset + rule.Key.Length <= text.Length && text.Substring(offset, rule.Key.Length) == rule.Key)
                    {
                        match = rule;
                        break;
                    }
                }

                // マッチング結果の登録
                if (match != null)
                {
                    list.Add(match.Value);
                    offset += match.Value.Key.Length;
                }
                else
                {
                    var value = text[offset].ToString();
                    list.Add(new KeyValuePair<string, string>(value, value));
                    offset += 1;
                }
            }

            return list;
        }
    }
}
