using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogTalk.Services.Extensions
{
    /// <summary>
    /// アプリケーション設定 拡張メソッド
    /// </summary>
    public static class AppSettingsExtension
    {
        /// <summary>
        /// string 型プロパティの取得
        /// </summary>
        public static bool TryGetProperty(this IDictionary<string, string> dictionary, string key, IReactiveProperty<string> property)
        {
            if (dictionary.TryGetValue(key, out var value))
            {
                property.Value = value;
                return true;
            }

            return false;
        }

        /// <summary>
        /// uint 型プロパティの取得
        /// </summary>
        public static bool TryGetProperty(this IDictionary<string, string> dictionary, string key, IReactiveProperty<uint> property)
        {
            if (dictionary.TryGetValue(key, out var value) && uint.TryParse(value, out var number))
            {
                property.Value = number;
                return true;
            }

            return false;
        }
    }
}
