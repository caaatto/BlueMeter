namespace BlueMeter.WinForm.Plugin;

public static class KeyExtension
{
    public static string KeysToString(this Keys key)
    {
        if (key == Keys.None)
            return string.Empty;

        var result = new List<string>();

        // Check for modifier keys
        if ((key & Keys.Control) == Keys.Control)
            result.Add("Ctrl");
        if ((key & Keys.Alt) == Keys.Alt)
            result.Add("Alt");
        if ((key & Keys.Shift) == Keys.Shift)
            result.Add("Shift");

        // Get the main key (without modifiers)
        var mainKey = key & Keys.KeyCode;
        // exclude ControlKey
        if (mainKey != Keys.None && mainKey != Keys.ControlKey && mainKey != Keys.ShiftKey)
            result.Add(mainKey.ToString());

        return string.Join("+", result);
    }

    /// <summary>
    /// 转换整数到 Keys，若主键不合法则使用默认键，若控制键不合法则清除
    /// </summary>
    /// <param name="value"></param>
    /// <param name="defaultKey"></param>
    /// <param name="allowedKeys"></param>
    /// <returns></returns>
    public static Keys IntToKeys(this int value, Keys defaultKey, Keys allowedKeys = (Keys.Shift | Keys.Control | Keys.Alt))
    {
        var code = value & (int)Keys.KeyCode;
        var modifiers = value & (int)Keys.Modifiers;

        // 列出合法的控制键
        var allowedModifiers = (int)(allowedKeys & Keys.Modifiers);

        // 若主键不合法则使用默认键
        if (!Enum.IsDefined(typeof(Keys), code))
        {
            code = (int)defaultKey;
        }

        // 若控制键不合法则清除
        if ((modifiers & ~allowedModifiers) != 0)
        {
            modifiers = 0;
        }
        return (Keys)(code | modifiers);
    }
}