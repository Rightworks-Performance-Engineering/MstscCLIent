
using RoyalApps.Community.Rdp.WinForms;
using RoyalApps.Community.Rdp.WinForms.Configuration;
using RoyalApps.Community.Rdp.WinForms.Configuration.Connection;
using RoyalApps.Community.Rdp.WinForms.Configuration.Display;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Extensions.Logging;


namespace MstscCLIent
{
    public partial class UI : Form
    {
        private readonly System.ComponentModel.IContainer? components = null;
        private readonly RdpControl _rdpControl;
        private readonly string msrdcexLogPath;
        private readonly Options options;

        private readonly System.Windows.Forms.Timer _timer = new() { Interval = 1000 };

        private void Form1_Shown(Object? sender, EventArgs e)
        {
            try
            {
                _rdpControl.Connect();
                this.Refresh();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "An exception occurred while trying to connect to the remote desktop. Please check that the connection and account details are correct and that the remote host is reachable. The application will now exit.");
                Log.CloseAndFlush();
                Application.Exit();
            }
        }
        public UI(Options incomingOptions)
        {
            using ILoggerFactory factory = new SerilogLoggerFactory(Log.Logger);
            Microsoft.Extensions.Logging.ILogger msLogger = factory.CreateLogger("MstscCLIent");

            options = incomingOptions;
            string systemPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "mstscax.dll");
            if (File.Exists(systemPath))
            {
                Icon = Icon.ExtractAssociatedIcon(systemPath) ?? SystemIcons.Application;
            }
            else
            {
                Icon = SystemIcons.Application;
            }
            string logPath = Path.GetTempPath();
            string dateTime = DateTime.Now.ToString("yyyy-MM-dd--HH-mm-ss");
            string sam = options.Username.Split('@')[0];
            Directory.CreateDirectory(@$"{logPath + "MstscCLIent\\"}");
            msrdcexLogPath = @$"{logPath + "MstscCLIent\\" + dateTime + "-" + sam + "-"}MstscCLIent-MsRdpEx.log";

            MaximizedBounds = Screen.FromHandle(Handle).WorkingArea;
            WindowState = FormWindowState.Maximized;
            _rdpControl = new RdpControl
            {
                Dock = DockStyle.Fill,
                Parent = this,
                Logger = msLogger,
                EnableSessionCapture = true
            };
            _rdpControl.RdpConfiguration.Display.DesktopWidth = 1920;
            _rdpControl.RdpConfiguration.Display.DesktopHeight = 1080;
            _rdpControl.RdpConfiguration.Input.AllowBackgroundInput = true;
            _rdpControl.RdpConfiguration.Display.ResizeBehavior = ResizeBehavior.Scrollbars;
            _rdpControl.RdpConfiguration.Input.GrabFocusOnConnect = false;
            _rdpControl.RdpConfiguration.Input.DisableClickDetection = true;
            _rdpControl.RdpConfiguration.LogLevel = "DEBUG";
            _rdpControl.RdpConfiguration.LogEnabled = true;
            _rdpControl.RdpConfiguration.LogFilePath = msrdcexLogPath;
            _rdpControl.RdpConfiguration.Display.UseLocalScaling = true;
            _rdpControl.RdpConfiguration.Display.ColorDepth = RoyalApps.Community.Rdp.WinForms.Configuration.Display.ColorDepth.ColorDepth8Bpp;
            _rdpControl.RdpConfiguration.Connection.Compression = false;
            _rdpControl.RdpConfiguration.Performance.EnableFontSmoothing = true;
            _rdpControl.RdpConfiguration.Performance.EnableEnhancedGraphics = true;
            var ConnectingText = new Label()
            {
                Text = "Connecting...",
                AutoSize = false,
                Font = new Font("Arial", 60),
                TextAlign = ContentAlignment.MiddleCenter,
                Dock = DockStyle.Fill,
                Left = 10,
                Width = this.Width - 10,
                Parent = this,
            };
            ConnectingText.BringToFront();
            var disconnect = new ToolStripMenuItem("Disconnect");
            disconnect.Click += (_, _) => _rdpControl.Disconnect();
            var InputButton = new ToolStripMenuItem("Ignore Input");
            InputButton.Checked = true;
            _rdpControl.Enabled = false;
            ActiveControl = null;
            InputButton.CheckOnClick = true;
            InputButton.CheckedChanged += InputButton_CheckedChanged;
            void InputButton_CheckedChanged(object? sender, EventArgs e)
            {
                if (InputButton.Checked)
                {
                    _rdpControl.Enabled = false;
                    ActiveControl = null;
                    Refresh();
                }
                else
                {
                    _rdpControl.Enabled = true;
                    ActiveControl = null;
                    Refresh();
                }
            }
            var connectionInfo = new ToolStripMenuItem("Show Connection Info");
            connectionInfo.Click += (_, _) =>
            {
                _rdpControl.ShowConnectionInfo();
            };
            var remoteDesktopMenu = new ToolStripMenuItem("Remote Desktop")
            {
                DropDownItems =
            {
                disconnect,
                new ToolStripSeparator(),
                connectionInfo,
                new ToolStripSeparator(),
                InputButton,
            }
            };
            var menuStrip = new MenuStrip
            {
                Dock = DockStyle.Top,
                Parent = this,
                Items = { remoteDesktopMenu }
            };
            _rdpControl.OnDisconnected += (_, e) =>
            {
                Log.Information("Session disconnected.");
                Log.Information("{e.Description}", e.Description);
                _rdpControl.Dispose();
                Log.CloseAndFlush();
                Application.Exit();
            };
            _rdpControl.OnConnected += (_, _) =>
            {
                ConnectingText.Hide();
                ConnectingText.Enabled = false;

                SessionAutomation exampleSessionAutomation = new(retryCount: 30, _rdpControl);

                _timer.Tick += (_, _) =>
                {
                    Application.DoEvents();
                    if (!(exampleSessionAutomation.actionTaken || exampleSessionAutomation.maxCountReached))
                    {
                        Bitmap? image = exampleSessionAutomation.GetImage();
                        if (image != null)
                        {
                            // Do things with the image Here
                            // Send text here too

                            //Send Ctrl+Alt+Del to the remote session
                            //exampleSessionAutomation.SendKeyDown("CTRL");
                            //exampleSessionAutomation.SendKeyDown("ALT");
                            //exampleSessionAutomation.SendKeyDown("DELETE");
                            //exampleSessionAutomation.SendKeyUp("CTRL");
                            //exampleSessionAutomation.SendKeyUp("ALT");
                            //exampleSessionAutomation.SendKeyUp("DELETE");

                            //exampleSessionAutomation.SendKeys("Hello, this is an automated message sent over RDP.");

                        }
                    }
                };
                _timer.Start();
            };

            _rdpControl.RdpConfiguration.Server = options.Host;
            Text = @$"RDP: {options.Host} as {options.Username}";
            _rdpControl.RdpConfiguration.Credentials.Domain = options.Domain;
            _rdpControl.RdpConfiguration.Credentials.Username = options.Username;
            _rdpControl.RdpConfiguration.Credentials.Password = new SensitiveString(options.Password);
            _rdpControl.RdpConfiguration.Input.DisableClickDetection = true;
            _rdpControl.RdpConfiguration.Display.ContainerHandledFullScreen = true;
            Shown += Form1_Shown;
        }

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
