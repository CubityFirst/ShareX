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

using ShareX.HelpersLib;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ShareX
{
    public class AvaloniaKeyboardMessageFilter : IMessageFilter
    {
        private const int WM_KEYFIRST = 0x0100;
        private const int WM_KEYLAST = 0x0109;
        private const uint GA_ROOT = 2;

        public bool PreFilterMessage(ref Message m)
        {
            if (m.Msg < WM_KEYFIRST || m.Msg > WM_KEYLAST)
            {
                return false;
            }

            if (Control.FromChildHandle(m.HWnd) != null)
            {
                return false;
            }

            IntPtr root = GetAncestor(m.HWnd, GA_ROOT);

            if (root == IntPtr.Zero || !NativeMethods.GetClassName(root).StartsWith("Avalonia-", StringComparison.Ordinal))
            {
                return false;
            }

            uint pos = GetMessagePos();

            MSG msg = new MSG
            {
                hwnd = m.HWnd,
                message = (uint)m.Msg,
                wParam = m.WParam,
                lParam = m.LParam,
                time = GetMessageTime(),
                pt_x = (short)(pos & 0xFFFF),
                pt_y = (short)((pos >> 16) & 0xFFFF)
            };

            TranslateMessage(ref msg);
            DispatchMessage(ref msg);
            return true;
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
        private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [DllImport("user32.dll")]
        private static extern bool TranslateMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern IntPtr DispatchMessage(ref MSG lpMsg);

        [DllImport("user32.dll")]
        private static extern int GetMessageTime();

        [DllImport("user32.dll")]
        private static extern uint GetMessagePos();
    }
}
