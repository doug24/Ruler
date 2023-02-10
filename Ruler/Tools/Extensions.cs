using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;

namespace Ruler
{
    public static class Extensions
    {
        public static Window? FindParentWindow(this DependencyObject child)
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            //Check if this is the end of the tree
            if (parent == null) return null;

            if (parent is Window parentWindow)
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

            if (parent is FrameworkElement parentElement)
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

        public static bool MoveWindow(this Window window, double left, double top, double width, double height)
        {
            IntPtr hWnd = new WindowInteropHelper(window).Handle;
            var matrix = PresentationSource.FromVisual(window).CompositionTarget.TransformToDevice;
            return PInvoke.MoveWindow(new(hWnd), (int)(left * matrix.M11), (int)(top * matrix.M22), (int)(width * matrix.M11), (int)(height * matrix.M22), true) != 0;
        }

        // Based on Rick Strahl's blog:
        // https://weblog.west-wind.com/posts/2020/Oct/12/Window-Activation-Headaches-in-WPF
        unsafe public static void ActivateWindow(this Window window)
        {
            window.Dispatcher.InvokeAsync(() =>
            {
                var hwnd = new WindowInteropHelper(window).Handle;

                var threadId1 = PInvoke.GetWindowThreadProcessId(PInvoke.GetForegroundWindow());
                var threadId2 = PInvoke.GetWindowThreadProcessId(new(hwnd));

                if (threadId1 != threadId2)
                {
                    PInvoke.AttachThreadInput(threadId1, threadId2, true);
                    PInvoke.SetForegroundWindow(new(hwnd));
                    PInvoke.AttachThreadInput(threadId1, threadId2, false);
                }
                else
                {
                    PInvoke.SetForegroundWindow(new(hwnd));
                }
            }, System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        // ******************************************************************
        // Based on Hans Passant Answer on:
        // https://stackoverflow.com/questions/2411392/double-epsilon-for-equality-greater-than-less-than-less-than-or-equal-to-gre

        /// <summary>
        /// Compare two double taking in account the double precision potential error.
        /// Take care: truncation errors accumulate on calculation. More you do, more you should increase the epsilon.
        public static bool AboutEquals(this double value1, double value2)
        {
            double epsilon = Math.Max(Math.Abs(value1), Math.Abs(value2)) * 1E-15;
            return Math.Abs(value1 - value2) <= epsilon;
        }

        /// <summary>
        /// Compare two double taking in account the double precision potential error.
        /// Take care: truncation errors accumulate on calculation. More you do, more you should increase the epsilon.
        /// You get really better performance when you can determine the contextual epsilon first.
        /// </summary>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="precalculatedContextualEpsilon"></param>
        /// <returns></returns>
        public static bool AboutEquals(this double value1, double value2, double precalculatedContextualEpsilon)
        {
            return Math.Abs(value1 - value2) <= precalculatedContextualEpsilon;
        }

        /// <summary>
        /// Calculate a double epsilon based on the size of the comparison values
        /// </summary>
        /// <param name="biggestPossibleContextualValue"></param>
        /// <returns></returns>
        public static double GetContextualEpsilon(this double biggestPossibleContextualValue)
        {
            return biggestPossibleContextualValue * 1E-15;
        }
    }
}
