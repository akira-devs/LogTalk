using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace LogTalk.UI
{
    /// <summary>
    /// DataContext の Dispose Action
    /// </summary>
    public class DataContextDisposeAction : TriggerAction<FrameworkElement>
    {
        /// <summary>
        /// Action 実行
        /// </summary>
        protected override void Invoke(object parameter)
        {
            // DataContext の Dispose
            if (AssociatedObject?.DataContext is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}
