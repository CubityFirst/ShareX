#region License Information (GPL v3)

/*
    ShareX - A program that allows you to take screenshots and share any file type
    Copyright (c) 2007-2026 ShareX Team

    This program is free software; you can redistribute it and/or
    modify it under the terms of the GNU General Public License
    as published by the Free Software Foundation; either version 2
    of the License, or (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program; if not, write to the Free Software
    Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

    Optionally you can also view the license at <http://www.gnu.org/licenses/>.
*/

#endregion License Information (GPL v3)

using Avalonia.Controls;
using Avalonia.Threading;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace ShareX.ImageEditor.Hosting
{
    public static class Win32KeyboardInterop
    {
        private const uint WM_GETDLGCODE = 0x0087;
        private const uint WM_KEYDOWN = 0x0100;
        private const uint WM_KEYUP = 0x0101;
        private const uint WM_CHAR = 0x0102;
        private const uint WM_DEADCHAR = 0x0103;
        private const uint WM_SYSKEYDOWN = 0x0104;
        private const uint WM_SYSKEYUP = 0x0105;
        private const uint WM_SYSCHAR = 0x0106;
        private const uint WM_SYSDEADCHAR = 0x0107;

        private const int DLGC_WANTARROWS = 0x0001;
        private const int DLGC_WANTTAB = 0x0002;
        private const int DLGC_WANTALLKEYS = 0x0004;
        private const int DLGC_WANTCHARS = 0x0080;

        private const uint PM_REMOVE = 0x0001;

        public static void Attach(TopLevel topLevel)
        {
            if (OperatingSystem.IsWindows())
            {
                Win32Properties.AddWndProcHookCallback(topLevel, WndProcHook);
            }
        }

        [SupportedOSPlatform("windows")]
        private static IntPtr WndProcHook(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_GETDLGCODE)
            {
                handled = true;
                return DLGC_WANTALLKEYS | DLGC_WANTCHARS | DLGC_WANTARROWS | DLGC_WANTTAB;
            }

            if (msg is WM_KEYDOWN or WM_KEYUP or WM_SYSKEYDOWN or WM_SYSKEYUP)
            {
                while (PeekMessage(out MSG charMsg, hWnd, WM_CHAR, WM_DEADCHAR, PM_REMOVE) ||
                    PeekMessage(out charMsg, hWnd, WM_SYSCHAR, WM_SYSDEADCHAR, PM_REMOVE))
                {
                    MSG captured = charMsg;

                    Dispatcher.UIThread.Post(() =>
                    {
                        SendMessage(captured.hwnd, captured.message, captured.wParam, captured.lParam);
                    }, DispatcherPriority.Send);
                }
            }

            return IntPtr.Zero;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MSG
        {
            public IntPtr hwnd;
            public uint message;
            public IntPtr wParam;
            public IntPtr lParam;
            public int time;
            public int pt_x;
            public int pt_y;
        }

        [DllImport("user32.dll")]
        private static extern bool PeekMessage(out MSG lpMsg, IntPtr hWnd, uint wMsgFilterMin, uint wMsgFilterMax, uint wRemoveMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);
    }
}
