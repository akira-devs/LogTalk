using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace LogTalk.UI
{
    /// <summary>
    /// 文字列リソースによるローカライゼーション Extension
    /// </summary>
    public class TExtension : MarkupExtension
    {
        /// <summary>
        /// リソース名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// コンストラクター
        /// </summary>
        public TExtension(string? name)
        {
            Name = name;
        }

        /// <summary>
        /// 値の解決
        /// </summary>
        public override object? ProvideValue(IServiceProvider serviceProvider)
        {
            return Properties.Resources.ResourceManager.GetString(Name) ?? Name;
        }
    }
}
