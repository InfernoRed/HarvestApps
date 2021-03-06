﻿using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Automation.Peers;
using Windows.UI.Xaml.Automation.Provider;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace HarvestApps.WinStore
{
    /// <summary>
    /// This class adds the IsDefault and IsCancel properties to buttons.
    /// </summary>
    /// <remarks>
    /// From http://www.julmar.com/blog/programming/default-cancel-button-behaviors.
    /// </remarks>
    public static class ButtonBehavior
    {
        private static readonly DependencyProperty DefaultButtonProperty =
            DependencyProperty.RegisterAttached("__DefaultButtonP__", typeof(Button),
                typeof(ButtonBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty IsDefaultProperty =
            DependencyProperty.RegisterAttached("IsDefault", typeof(bool),
                typeof(ButtonBehavior), new PropertyMetadata(false, OnIsDefaultChanged));

        public static bool GetIsDefault(Button button)
        {
            return (bool)button.GetValue(IsDefaultProperty);
        }

        public static void SetIsDefault(Button button, bool value)
        {
            button.SetValue(IsDefaultProperty, value);
        }

        private static void OnIsDefaultChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            // Find the page this button is on.
            Page owner = button.FindVisualParent<Page>();
            if (owner == null)
            {
                RoutedEventHandler eh = null;
                eh = delegate
                {
                    button.Loaded -= eh;
                    InitializeButton(button, true, true);
                };
                button.Loaded += eh;
            }
            else InitializeButton(button, (bool)e.NewValue, true);
        }

        private static readonly DependencyProperty CancelButtonProperty =
            DependencyProperty.RegisterAttached("__CancelButtonP__", typeof(Button),
                typeof(ButtonBehavior), new PropertyMetadata(null));

        public static readonly DependencyProperty IsCancelProperty =
            DependencyProperty.RegisterAttached("IsCancel", typeof(bool),
                typeof(ButtonBehavior), new PropertyMetadata(false, OnIsCancelChanged));

        public static bool GetIsCancel(Button button)
        {
            return (bool)button.GetValue(IsCancelProperty);
        }

        public static void SetIsCancel(Button button, bool value)
        {
            button.SetValue(IsCancelProperty, value);
        }

        private static void OnIsCancelChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var button = sender as Button;
            if (button == null)
                return;

            // Find the page this button is on.
            Page owner = button.FindVisualParent<Page>();
            if (owner == null)
            {
                RoutedEventHandler eh = null;
                eh = delegate
                {
                    button.Loaded -= eh;
                    InitializeButton(button, true, false);
                };
                button.Loaded += eh;
            }
            else InitializeButton(button, (bool)e.NewValue, false);
        }

        private static void InitializeButton(Button button, bool attach, bool isDefault)
        {
            Page owner = button.FindVisualParent<Page>();
            if (owner == null)
            {
                return;
            }

            owner.Unloaded += (_s, _e) =>
            {
                Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= CoreDispatcher_AcceleratorKeyActivated;
            };

            owner.ClearValue(DefaultButtonProperty);
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated -= CoreDispatcher_AcceleratorKeyActivated;
            if (isDefault)
                button.ClearValue(Control.BorderThicknessProperty);

            if (!attach) return;
            
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;
            if (isDefault)
            {
                owner.SetValue(DefaultButtonProperty, button);
                button.BorderThickness = new Thickness(2);
            }
            else
            {
                owner.SetValue(CancelButtonProperty, button);
            }
        }

        private static void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            const CoreVirtualKeyStates downState = CoreVirtualKeyStates.Down;
            var coreWindow = Window.Current.CoreWindow;
            var menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
            var controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
            var shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
            var noModifiers = !menuKey && !controlKey && !shiftKey;

            if (!noModifiers || e.EventType != CoreAcceleratorKeyEventType.KeyDown ||
                (e.VirtualKey != VirtualKey.Enter && e.VirtualKey != VirtualKey.Escape)) return;
            var frame = Window.Current.Content as Frame;
            if (frame == null)
                return;

            var currentPage = frame.Content as Page;
            if (currentPage == null)
                return;

            if (e.VirtualKey == VirtualKey.Enter)
            {
                // Quick check to avoid TextBox with ENTER support
                var tb = FocusManager.GetFocusedElement() as TextBox;
                if (tb != null && tb.AcceptsReturn)
                    return;

                var defaultButton = currentPage.GetValue(DefaultButtonProperty) as Button;
                if (defaultButton == null || !defaultButton.IsEnabled) return;
                var peer = new ButtonAutomationPeer(defaultButton);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                    invokeProv.Invoke();
            }
            else
            {
                var cancelButton = currentPage.GetValue(CancelButtonProperty) as Button;
                if (cancelButton == null || !cancelButton.IsEnabled) return;
                var peer = new ButtonAutomationPeer(cancelButton);
                var invokeProv = peer.GetPattern(PatternInterface.Invoke) as IInvokeProvider;
                if (invokeProv != null)
                    invokeProv.Invoke();
            }
        }
    }
}