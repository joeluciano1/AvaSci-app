using System;
using System.Runtime.InteropServices;
#if !UNITY_EDITOR
using System.Linq;
#endif
using UnityEngine;

namespace Nrjwolf.Tools
{
    public enum ButtonStyle
    {
        Default,
        Cancel,
        Destructive,
    }

    public class IOSNativeAlert
    {
#if UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_STANDALONE_WIN

        private enum AlertStyle
        {
            Sheet,
            Alert,
        }

        public class AlertButton
        {
            public ButtonStyle Style;
            public string Title;
            public Action Callback;
            internal string m_Id;

            public AlertButton(string title, Action callback, ButtonStyle style = ButtonStyle.Default)
            {
                Title = title;
                Callback = callback;
                Style = style;
            }
        }
#if UNITY_STANDALONE_WIN
        // Windows MessageBox API
        private const int MB_OK = 0x00000000;
        private const int MB_OKCANCEL = 0x00000001;
        private const int MB_YESNO = 0x00000004;
        private const int MB_YESNOCANCEL = 0x00000003;

        private const int IDOK = 1;
        private const int IDCANCEL = 2;
        private const int IDYES = 6;
        private const int IDNO = 7;

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern int MessageBoxW(IntPtr hWnd, string lpText, string lpCaption, int uType);
#endif
        // Native function bindings
#if UNITY_IOS
        private delegate void AlertButtonDelegate(string buttonId);
        [DllImport("__Internal")] private static extern void RegisterMessageHandler(AlertButtonDelegate onButtonClick);
        [DllImport("__Internal")] private static extern void ShowNativeAlert(int alertStyle, string title, string message, string[] buttons, int[] buttonStyles, int buttonCount);
        [DllImport("__Internal")] private static extern void ShowToast(string message, bool isLongDuration);
#elif UNITY_STANDALONE_OSX
        [DllImport("IOSNativeAlert")] private static extern void ShowMacOSAlert(string title, string message, string[] buttons, int buttonCount);
#endif

        private static AlertButton[] m_CurrentAlertButtons;

        [RuntimeInitializeOnLoadMethod]
        public static void Initialize()
        {
#if UNITY_IOS && !UNITY_EDITOR
            RegisterMessageHandler(OnAlertButtonClick);
#endif
        }

#if UNITY_IOS
        [AOT.MonoPInvokeCallback(typeof(AlertButtonDelegate))]
#endif
        public static void OnAlertButtonClick(string buttonId)
        {
            if (m_CurrentAlertButtons == null || m_CurrentAlertButtons.Length == 0) return;

            Debug.Log($"Clicked {buttonId}");
            foreach (var alertButton in m_CurrentAlertButtons)
            {
                if (alertButton.m_Id == buttonId)
                {
                    alertButton.Callback?.Invoke();
                    break;
                }
            }
        }

        public static void ShowSheetMessage(string title, string message) => ShowAlertMessage(title, message, new AlertButton("Ok", null));
        public static void ShowSheetMessage(string title, string message, params AlertButton[] buttons) => CallNativeAlertMessage(AlertStyle.Sheet, title, message, buttons);

        public static void ShowAlertMessage(string title, string message) => ShowAlertMessage(title, message, new AlertButton("Ok", null));
        public static void ShowAlertMessage(string title, string message, params AlertButton[] buttons) => CallNativeAlertMessage(AlertStyle.Alert, title, message, buttons);

        private static void CallNativeAlertMessage(AlertStyle alertStyle, string title, string message, params AlertButton[] buttons)
        {
            // Create unique IDs for buttons
            for (int i = 0; i < buttons.Length; i++)
                buttons[i].m_Id = buttons[i].Title + i;

            // Cache the alert buttons
            m_CurrentAlertButtons = buttons;

#if UNITY_IOS && !UNITY_EDITOR
            ShowNativeAlert((int)alertStyle, title, message, buttons.Select(x => x.Title).ToArray(), buttons.Select(x => (int)x.Style).ToArray(), buttons.Length);

#elif UNITY_STANDALONE_OSX && !UNITY_EDITOR
            ShowMacOSAlert(title, message, buttons.Select(x => x.Title).ToArray(), buttons.Length);

#elif UNITY_STANDALONE_WIN && !UNITY_EDITOR
            ShowWindowsMessageBox(title, message, buttons);

#else
            Debug.Log($"[Editor] Alert: {title}\n{message}");
            foreach (var btn in buttons)
            {
                Debug.Log($"[Editor] Button: {btn.Title}");
            }
#endif
        }

        public static void ShowToast(string text, bool isLongDuration = false)
        {
#if UNITY_IOS && !UNITY_EDITOR
            ShowToast(text, isLongDuration);
#else
            Debug.Log($"[Editor] Toast: {text}");
#endif
        }

#endif
#if UNITY_STANDALONE_WIN
        private static void ShowWindowsMessageBox(string title, string message, AlertButton[] buttons)
        {
            int buttonType = MB_OK; // Default to OK button
            if (buttons.Length == 2 && buttons[0].Title == "OK" && buttons[1].Title == "Cancel")
                buttonType = MB_OKCANCEL;
            else if (buttons.Length == 2 && buttons[0].Title == "Yes" && buttons[1].Title == "No")
                buttonType = MB_YESNO;
            else if (buttons.Length == 3 && buttons[0].Title == "Yes" && buttons[1].Title == "No" && buttons[2].Title == "Cancel")
                buttonType = MB_YESNOCANCEL;

            int result = MessageBoxW(IntPtr.Zero, message, title, buttonType);

            foreach (var button in buttons)
            {
                if ((result == IDOK && button.Title == "OK") ||
                    (result == IDCANCEL && button.Title == "Cancel") ||
                    (result == IDYES && button.Title == "Yes") ||
                    (result == IDNO && button.Title == "No"))
                {
                    button.Callback?.Invoke();
                    break;
                }
            }
        }
#endif
    }
}
