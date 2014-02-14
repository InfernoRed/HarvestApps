using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace HarvestApps.WinStore
{
    public static class XamlExtensions
    {
        // http://social.msdn.microsoft.com/Forums/windowsapps/en-US/2e646a4e-0053-4fb9-8d6a-a24ee765312f/exception-thrown-in-visualtreehelpergetparent-in-windows-81-and-vs-2013-rc?forum=winappswithcsharp
        public static T FindVisualParent<T>(this DependencyObject dobj) where T : DependencyObject
        {
            while (true)
            {
                var parent = VisualTreeHelper.GetParent(dobj);
                if (parent == null && dobj is FrameworkElement)
                {
                    parent = (dobj as FrameworkElement).Parent;
                }
                if (parent is T)
                {
                    return parent as T;
                }
                if (parent == null) return null;
                dobj = parent;
            }
        }
    }
}