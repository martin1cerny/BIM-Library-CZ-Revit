using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Diagnostics;

namespace BimLibraryAddin.Extensions
{
    class RevitHandle : IWin32Window
    {
        IntPtr _hwnd;

        public RevitHandle()
        {
            Process process = Process.GetCurrentProcess();
            IntPtr h = process.MainWindowHandle;
            Debug.Assert(IntPtr.Zero != h, "expected non-null window handle");
            _hwnd = h;
        }

        public IntPtr Handle
        {
            get
            {
                return _hwnd;
            }
        }

        public void SetAsOwnerTo(System.Windows.Window win)
        {
            WindowInteropHelper helper = new WindowInteropHelper(win);
            helper.Owner = Handle;
        }
    }
}