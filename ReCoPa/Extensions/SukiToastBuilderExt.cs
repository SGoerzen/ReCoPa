using System;
using SukiUI.Toasts;

namespace ReCoPa.Extensions;

public static class SukiToastBuilderExt
{
    public static SukiToastBuilder WithDismiss(this SukiToastBuilder toast, bool dismiss = true)
    {
        toast.SetCanDismissByClicking(dismiss);
        return toast;
    }

    public static void QuickShow(this SukiToastBuilder toast, double seconds = 4)
    {
        toast.SetCanDismissByClicking(true);
        var dismiss = toast.Dismiss();
        var delaySeconds = seconds <= 0 ? 4 : seconds;
        dismiss.After(TimeSpan.FromSeconds(delaySeconds));
        toast.Queue();
    }
}
