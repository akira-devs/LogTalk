using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LogTalk.Services
{
    /// <summary>
    /// ファイル選択ダイアログサービス
    /// </summary>
    public interface IOpenFileDialogService
    {
        /// <summary>
        /// ダイアログ表示
        /// </summary>
        string? ShowDialog();
    }

    /// <summary>
    /// ファイル選択ダイアログサービス
    /// </summary>
    public class OpenFileDialogService : IOpenFileDialogService
    {
        /// <summary>
        /// ダイアログ表示
        /// </summary>
        public string? ShowDialog()
        {
            var dialog = new OpenFileDialog();

            // ダイアログ表示
            var result = dialog.ShowDialog() ?? false;
            if (!result) return null;

            return dialog.FileName;
        }
    }
}
