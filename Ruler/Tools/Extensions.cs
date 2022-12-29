using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace Ruler
{
    public static class Extensions
    {
        public static Window? FindParentWindow(this DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            //Check if this is the end of the tree
            if (parent == null) return null;

            Window? parentWindow = parent as Window;
            if (parentWindow != null)
            {
                return parentWindow;
            }
            else
            {
                //use recursion until it reaches a Window
                return FindParentWindow(parent);
            }
        }

        public static FrameworkElement? FindParentFrameworkElement(this DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            //Check if this is the end of the tree
            if (parent == null) return null;

            FrameworkElement? parentElement = parent as FrameworkElement;
            if (parentElement != null)
            {
                return parentElement;
            }
            else
            {
                //use recursion until it reaches a FrameworkElement
                return FindParentFrameworkElement(parent);
            }
        }

        public static IEnumerable<T?> FindLogicalChildren<T>(this DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) yield return null;

            var dependencyChildren = LogicalTreeHelper.GetChildren(parent).OfType<DependencyObject>();
            foreach (var child in dependencyChildren)
            {
                if (child is T typedChild)
                    yield return typedChild;

                foreach (T? childOfChild in FindLogicalChildren<T>(child))
                    yield return childOfChild;
            }
        }

        public static T? GetVisualChild<T>(this DependencyObject depObj) where T : Visual
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                    if (child != null && child is T typedChild)
                    {
                        return typedChild;
                    }

                    T? childOfChild = child?.GetVisualChild<T>();
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }
            return null;
        }

        public static T? GetChildOfType<T>(this DependencyObject depObj) where T : DependencyObject
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                var child = VisualTreeHelper.GetChild(depObj, i);

                var result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }
            return null;
        }

    }
}
