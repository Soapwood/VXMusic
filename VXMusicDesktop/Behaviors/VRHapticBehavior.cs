using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VXMusicDesktop.Core;

namespace VXMusicDesktop.Behaviors
{
    public static class VRHapticBehavior
    {
        #region EnableHapticFeedback Attached Property

        public static readonly DependencyProperty EnableHapticFeedbackProperty =
            DependencyProperty.RegisterAttached(
                "EnableHapticFeedback",
                typeof(bool),
                typeof(VRHapticBehavior),
                new PropertyMetadata(false, OnEnableHapticFeedbackChanged));

        public static bool GetEnableHapticFeedback(DependencyObject obj)
        {
            return (bool)obj.GetValue(EnableHapticFeedbackProperty);
        }

        public static void SetEnableHapticFeedback(DependencyObject obj, bool value)
        {
            obj.SetValue(EnableHapticFeedbackProperty, value);
        }

        #endregion

        #region HapticDuration Attached Property

        public static readonly DependencyProperty HapticDurationProperty =
            DependencyProperty.RegisterAttached(
                "HapticDuration",
                typeof(ushort),
                typeof(VRHapticBehavior),
                new PropertyMetadata((ushort)2000));

        public static ushort GetHapticDuration(DependencyObject obj)
        {
            return (ushort)obj.GetValue(HapticDurationProperty);
        }

        public static void SetHapticDuration(DependencyObject obj, ushort value)
        {
            obj.SetValue(HapticDurationProperty, value);
        }

        #endregion

        private static void OnEnableHapticFeedbackChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                bool enableHaptic = (bool)e.NewValue;
                
                if (enableHaptic)
                {
                    element.MouseEnter += OnElementMouseEnter;
                    element.Unloaded += OnElementUnloaded;
                }
                else
                {
                    element.MouseEnter -= OnElementMouseEnter;
                    element.Unloaded -= OnElementUnloaded;
                }
            }
        }

        private static void OnElementMouseEnter(object sender, MouseEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                // Only trigger haptic feedback for elements that can be interacted with
                if (IsInteractiveElement(element))
                {
                    TriggerHapticFeedback(element);
                }
            }
        }

        private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                element.MouseEnter -= OnElementMouseEnter;
                element.Unloaded -= OnElementUnloaded;
            }
        }

        private static bool IsInteractiveElement(FrameworkElement element)
        {
            // Check if element is enabled and visible
            if (!element.IsEnabled || element.Visibility != Visibility.Visible)
                return false;

            // Check for common interactive elements
            switch (element)
            {
                case Button button:
                    return button.IsEnabled && button.Command?.CanExecute(button.CommandParameter) != false;
                case ComboBox comboBox:
                    return comboBox.IsEnabled;
                case RadioButton radioButton:
                    return radioButton.IsEnabled;
                case CheckBox checkBox:
                    return checkBox.IsEnabled;
                case TextBox textBox:
                    return textBox.IsEnabled && !textBox.IsReadOnly;
                case ListBox listBox:
                    return listBox.IsEnabled;
                case MenuItem menuItem:
                    return menuItem.IsEnabled;
                case Slider slider:
                    return slider.IsEnabled;
                default:
                    // For other elements, check if they have a cursor indicating interactivity
                    return element.Cursor == Cursors.Hand || element.Cursor == Cursors.Arrow;
            }
        }

        private static void TriggerHapticFeedback(FrameworkElement element)
        {
            try
            {
                // Get the haptic feedback service from the DI container
                var hapticService = App.ServiceProvider?.GetService<IVRHapticFeedbackService>();
                var loggerFactory = App.ServiceProvider?.GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger("VRHapticBehavior");
                
                if (hapticService != null)
                {
                    ushort duration = GetHapticDuration(element);
                    logger?.LogDebug($"Triggering haptic feedback for {element.GetType().Name} with duration {duration}");
                    
                    // Log VR system status
                    logger?.LogDebug($"VR System Available: {hapticService.IsVRSystemAvailable()}");
                    logger?.LogDebug($"Has Connected Controllers: {hapticService.HasConnectedControllers()}");
                    
                    hapticService.TriggerHapticFeedback(duration);
                }
                else
                {
                    logger?.LogWarning("VRHapticFeedbackService not available from DI container");
                }
            }
            catch (System.Exception ex)
            {
                // Log the exception if we have access to a logger
                try
                {
                    // Try to get a logger factory and create a logger for haptic feedback
                    var loggerFactory = App.ServiceProvider?.GetService<ILoggerFactory>();
                    var logger = loggerFactory?.CreateLogger("VRHapticBehavior");
                    logger?.LogWarning(ex, "Failed to trigger haptic feedback from VRHapticBehavior");
                }
                catch
                {
                    // If we can't even log, just silently fail to avoid breaking the UI
                }
            }
        }
    }
}