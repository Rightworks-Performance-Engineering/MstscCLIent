using AxMSTSCLib;
using Destructurama;
using MSTSCLib;
using RoyalApps.Community.Rdp.WinForms;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Syslog;
using System.Drawing.Imaging;


namespace MstscCLIent
{
    public class SessionAutomation
    {
        private int invokeCount;
        private int maxCount;
        public bool maxCountReached = false;

        public bool actionTaken = false;
        private readonly IMsRdpClientNonScriptable _nonScriptable;
        private RdpControl _rdpControl;
        public SessionAutomation(int retryCount, RdpControl rdpControl)
        {
            invokeCount = 0;
            maxCount = retryCount;

            if (rdpControl.RdpClient == null)
            {
                Log.Error("rdpClient was null");
            }

            _rdpControl = rdpControl;

            object? axRdpClient = null;

            axRdpClient = rdpControl.RdpClient as AxMsRdpClient11NotSafeForScripting;

            if (axRdpClient == null)
            {
                axRdpClient = rdpControl.RdpClient as AxMsRdpClient10NotSafeForScripting;
            }

            if (axRdpClient == null)
            {
                axRdpClient = rdpControl.RdpClient as AxMsRdpClient9NotSafeForScripting;
            }

            if (axRdpClient == null)
            {
                axRdpClient = rdpControl.RdpClient as AxMsRdpClient8NotSafeForScripting;
            }

            _nonScriptable = (IMsRdpClientNonScriptable)((dynamic)axRdpClient!).GetOcx()!;
            if (_nonScriptable == null)
            {
                Log.Error("_nonScriptable was null");
                throw new Exception("Failed to initialize SessionAutomation could not get non-scriptable interface");
            }
        }



        public Bitmap? GetImage()
        {

            if (_rdpControl.RdpClient == null)
            {
                Log.Error("rdpClient was null");
            }

            ++invokeCount;

            Log.Debug("Checking SessionAutomation status {0}.", (invokeCount).ToString());


            //break the loop if max times is reached
            if (invokeCount == maxCount)
            {
                Log.Information("Maximum retries of {maxCount} reached, breaking.", maxCount);
                maxCountReached = true;
                return null;
            }

            // Get screenshot of window

            Bitmap? rdpWindowImage = _rdpControl.SessionCapture;
            if (rdpWindowImage == null)
            {
                Log.Debug("rdpWindowImage was null");
                return null;
            }

            return rdpWindowImage;
        }

        public void SendKeyDown(int keyCode)
        {
            try
            {
                bool keyDown = false;
                _nonScriptable.SendKeys(1, ref keyDown, ref keyCode);
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send key down");
            }
        }


        public void SendKeyUp(int keyCode)
        {
            try
            {
                bool keyUp = true;
                _nonScriptable.SendKeys(1, ref keyUp, ref keyCode);
                Thread.Sleep(50);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to send key up");
            }
        }

        public void SendKeyDown(string key)
        {
            if (!RdpKeyMapper.TryGetKey(key, out var keyStroke))
            {
                throw new ArgumentException($"Unsupported key '{key}'.");
            }

            bool down = false;

            if (keyStroke.RequiresShift)
            {
                int shift = 0x2A;
                _nonScriptable.SendKeys(1, ref down, ref shift);
                Thread.Sleep(50);
            }

            int scanCode = keyStroke.ScanCode;
            _nonScriptable.SendKeys(
                keyStroke.Extended ? 2 : 1,
                ref down,
                ref scanCode);
            Thread.Sleep(50);
        }

        public void SendKeyUp(string key)
        {
            if (!RdpKeyMapper.TryGetKey(key, out var keyStroke))
            {
                throw new ArgumentException($"Unsupported key '{key}'.");
            }

            bool up = true;

            int scanCode = keyStroke.ScanCode;

            _nonScriptable.SendKeys(
                keyStroke.Extended ? 2 : 1,
                ref up,
                ref scanCode);
            Thread.Sleep(50);

            if (keyStroke.RequiresShift)
            {
                int shift = 0x2A;
                _nonScriptable.SendKeys(1, ref up, ref shift);
                Thread.Sleep(50);
            }
        }

        public void SendKey(string key)
        {
            SendKeyDown(key);
            SendKeyUp(key);
        }

        public void SendKey(char c)
        {
            if (!RdpKeyMapper.TryGetKey(c, out var keyStroke))
            {
                throw new ArgumentException($"Unsupported key '{c}'.");
            }

            bool down = false;
            bool up = true;

            if (keyStroke.RequiresShift)
            {
                int shift = 0x2A;
                _nonScriptable.SendKeys(1, ref down, ref shift);
                Thread.Sleep(50);
            }

            int scanCode = keyStroke.ScanCode;

            _nonScriptable.SendKeys(
                keyStroke.Extended ? 2 : 1,
                ref down,
                ref scanCode);
            Thread.Sleep(50);

            _nonScriptable.SendKeys(
                keyStroke.Extended ? 2 : 1,
                ref up,
                ref scanCode);
            Thread.Sleep(50);
            if (keyStroke.RequiresShift)
            {
                int shift = 0x2A;
                _nonScriptable.SendKeys(1, ref up, ref shift);
                Thread.Sleep(50);
            }
        }

        public void SendKeys(string text)
        {
            foreach (char c in text)
            {
                SendKey(c);
            }
        }


        public static class RdpKeyMapper
        {
            private static readonly Dictionary<string, KeyStroke> _namedKeys =
                new(StringComparer.OrdinalIgnoreCase)
                {
                    ["TAB"] = new(0x0F),
                    ["ENTER"] = new(0x1C),
                    ["ESC"] = new(0x01),
                    ["ESCAPE"] = new(0x01),
                    ["BACKSPACE"] = new(0x0E),

                    ["CTRL"] = new(0x1D),
                    ["SHIFT"] = new(0x2A),
                    ["ALT"] = new(0x38),

                    ["UP"] = new(0x48, true),
                    ["DOWN"] = new(0x50, true),
                    ["LEFT"] = new(0x4B, true),
                    ["RIGHT"] = new(0x4D, true),

                    ["HOME"] = new(0x47, true),
                    ["END"] = new(0x4F, true),
                    ["INSERT"] = new(0x52, true),
                    ["DELETE"] = new(0x53, true),

                    ["PAGEUP"] = new(0x49, true),
                    ["PAGEDOWN"] = new(0x51, true),

                    ["F1"] = new(0x3B),
                    ["F2"] = new(0x3C),
                    ["F3"] = new(0x3D),
                    ["F4"] = new(0x3E),
                    ["F5"] = new(0x3F),
                    ["F6"] = new(0x40),
                    ["F7"] = new(0x41),
                    ["F8"] = new(0x42),
                    ["F9"] = new(0x43),
                    ["F10"] = new(0x44),
                    ["F11"] = new(0x57),
                    ["F12"] = new(0x58),

                    ["SPACE"] = new(0x39)
                };

            public sealed record KeyStroke(
            int ScanCode,
            bool Extended = false,
            bool RequiresShift = false);

            private static readonly Dictionary<char, KeyStroke> _characters =
                new()
                {
                    ['a'] = new(0x1E),
                    ['b'] = new(0x30),
                    ['c'] = new(0x2E),
                    ['d'] = new(0x20),
                    ['e'] = new(0x12),
                    ['f'] = new(0x21),
                    ['g'] = new(0x22),
                    ['h'] = new(0x23),
                    ['i'] = new(0x17),
                    ['j'] = new(0x24),
                    ['k'] = new(0x25),
                    ['l'] = new(0x26),
                    ['m'] = new(0x32),
                    ['n'] = new(0x31),
                    ['o'] = new(0x18),
                    ['p'] = new(0x19),
                    ['q'] = new(0x10),
                    ['r'] = new(0x13),
                    ['s'] = new(0x1F),
                    ['t'] = new(0x14),
                    ['u'] = new(0x16),
                    ['v'] = new(0x2F),
                    ['w'] = new(0x11),
                    ['x'] = new(0x2D),
                    ['y'] = new(0x15),
                    ['z'] = new(0x2C),

                    ['0'] = new(0x0B),
                    ['1'] = new(0x02),
                    ['2'] = new(0x03),
                    ['3'] = new(0x04),
                    ['4'] = new(0x05),
                    ['5'] = new(0x06),
                    ['6'] = new(0x07),
                    ['7'] = new(0x08),
                    ['8'] = new(0x09),
                    ['9'] = new(0x0A),

                    [' '] = new(0x39),

                    ['-'] = new(0x0C),
                    ['='] = new(0x0D),
                    ['['] = new(0x1A),
                    [']'] = new(0x1B),
                    [';'] = new(0x27),
                    ['\''] = new(0x28),
                    ['`'] = new(0x29),
                    ['\\'] = new(0x2B),
                    [','] = new(0x33),
                    ['.'] = new(0x34),
                    ['/'] = new(0x35),

                    ['_'] = new(0x0C, false, true),
                    ['+'] = new(0x0D, false, true),
                    ['{'] = new(0x1A, false, true),
                    ['}'] = new(0x1B, false, true),
                    [':'] = new(0x27, false, true),
                    ['"'] = new(0x28, false, true),
                    ['~'] = new(0x29, false, true),
                    ['|'] = new(0x2B, false, true),
                    ['<'] = new(0x33, false, true),
                    ['>'] = new(0x34, false, true),
                    ['?'] = new(0x35, false, true),

                    ['!'] = new(0x02, false, true),
                    ['@'] = new(0x03, false, true),
                    ['#'] = new(0x04, false, true),
                    ['$'] = new(0x05, false, true),
                    ['%'] = new(0x06, false, true),
                    ['^'] = new(0x07, false, true),
                    ['&'] = new(0x08, false, true),
                    ['*'] = new(0x09, false, true),
                    ['('] = new(0x0A, false, true),
                    [')'] = new(0x0B, false, true),
                };

            public static bool TryGetKey(string key, out KeyStroke result)
            {
                if (_namedKeys.TryGetValue(key, out result))
                    return true;

                if (key.Length == 1)
                {
                    return TryGetKey(key[0], out result);
                }

                result = default!;
                return false;
            }

            public static bool TryGetKey(char c, out KeyStroke result)
            {
                if (char.IsUpper(c))
                {
                    if (_characters.TryGetValue(char.ToLowerInvariant(c), out var lower))
                    {
                        result = lower with { RequiresShift = true };
                        return true;
                    }
                }

                return _characters.TryGetValue(c, out result);
            }
        }


    }
}
