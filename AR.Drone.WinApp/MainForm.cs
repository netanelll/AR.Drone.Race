using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using AR.Drone.Client;
using AR.Drone.Client.Command;
using AR.Drone.Client.Configuration;
using AR.Drone.Data;
using AR.Drone.Data.Navigation;
using AR.Drone.Data.Navigation.Native;
using AR.Drone.Media;
using AR.Drone.Video;
using AR.Drone.Avionics;
using AR.Drone.Avionics.Objectives;
using AR.Drone.Avionics.Objectives.IntentObtainers;
using System.Timers;
using XInputDotNetPure;

namespace AR.Drone.WinApp
{
    public partial class MainForm : Form
    {
        private const string ARDroneTrackFileExt = ".ardrone";
        private const string ARDroneTrackFilesFilter = "AR.Drone track files (*.ardrone)|*.ardrone";

        private readonly DroneClient _droneClient;
        private readonly List<PlayerForm> _playerForms;
        private readonly VideoPacketDecoderWorker _videoPacketDecoderWorker;
        private Settings _settings;
        private VideoFrame _frame;
        private Bitmap _frameBitmap;
        private uint _frameNumber;
        private NavigationData _navigationData;
        private NavigationPacket _navigationPacket;
        private PacketRecorder _packetRecorderWorker;
        private FileStream _recorderStream;
        private Autopilot _autopilot;
        private RaceController _raceController;
        private PaintingHelper _paintingHelper;
        private MapConfiguration _mapConf;
        bool drowMiniMap = false;
        bool _isOutOfBoundry = false;

        int counter = 2;
        int ledAnimation = 0;

        bool isVertical = true;

        System.Timers.Timer recoredTimer = new System.Timers.Timer();
        System.Timers.Timer XboxTimer = new System.Timers.Timer();
        XboxHelper xBoxHelper;
        List<float> oldOrders;

        //List<CsvRow> allRaws = new List<CsvRow>(); // stub to load fake nav data to be deleted TODO
        //int count = 0; // stub to load fake nav data to be deleted TODO

        public MainForm()
        {
            InitializeComponent();

            _videoPacketDecoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
            _videoPacketDecoderWorker.Start();

            _raceController = new RaceController();

            _droneClient = new DroneClient("192.168.1.1");
            _droneClient.NavigationPacketAcquired += OnNavigationPacketAcquired;
            _droneClient.VideoPacketAcquired += OnVideoPacketAcquired;
            _droneClient.NavigationDataAcquired += data => _navigationData = data;
            _droneClient.NavigationDataAcquired += _raceController.OnNavigationDataAcquired;

            tmrStateUpdate.Enabled = true;
            tmrVideoUpdate.Enabled = true;

            _playerForms = new List<PlayerForm>();

            _videoPacketDecoderWorker.UnhandledException += UnhandledException;

            this.KeyDown += MainForm_KeyDown;

            RemoteListener(); // Activates the Xbox Remote controller

            _mapConf = new MapConfiguration(1, 850, 250);
            _paintingHelper = new PaintingHelper(_mapConf, this.CreateGraphics()); // Generates class to control all the painting

            //loadFakeDataFromFile(); // stub to load fake nav data to be deleted TODO
        }

        private void UnhandledException(object sender, Exception exception)
        {
            MessageBox.Show(exception.ToString(), "Unhandled Exception (Ctrl+C)", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Text += Environment.Is64BitProcess ? " [64-bit]" : " [32-bit]";
        }

        protected override void OnClosed(EventArgs e)
        {
            if (_autopilot != null)
            {
                _autopilot.UnbindFromClient();
                _autopilot.Stop();
            }

            StopRecording();

            _droneClient.Dispose();
            _videoPacketDecoderWorker.Dispose();

            base.OnClosed(e);
        }

        private void OnNavigationPacketAcquired(NavigationPacket packet)
        {
            if (_packetRecorderWorker != null && _packetRecorderWorker.IsAlive)
                _packetRecorderWorker.EnqueuePacket(packet);

            _navigationPacket = packet;
        }

        private void OnVideoPacketAcquired(VideoPacket packet)
        {
            if (_packetRecorderWorker != null && _packetRecorderWorker.IsAlive)
                _packetRecorderWorker.EnqueuePacket(packet);
            if (_videoPacketDecoderWorker.IsAlive)
                _videoPacketDecoderWorker.EnqueuePacket(packet);
        }

        private void OnVideoPacketDecoded(VideoFrame frame)
        {
            _frame = frame;
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            _droneClient.Start();

            _paintingHelper.DrawTrack();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            _droneClient.Stop();
        }

        private void tmrVideoUpdate_Tick(object sender, EventArgs e)
        {

            tmrChangeQuadLocation.Enabled = drowMiniMap; // fix to mini map drow using xbox remute. should not here. TODO 

            if (_navigationData != null)
            {
                // Updates the battery field
                tbBattery.Text = _navigationData.Battery.Percentage.ToString();

                if (_navigationData.Battery.Low == true)
                {
                    tbBattery.ForeColor = Color.Red;

                } 
            }

            if (_frame == null || _frameNumber == _frame.Number)
                return;
            _frameNumber = _frame.Number;

            if (_frameBitmap == null)
                _frameBitmap = VideoHelper.CreateBitmap(ref _frame);
            else
                VideoHelper.UpdateBitmap(ref _frameBitmap, ref _frame);

            pbVideo.Image = _frameBitmap;
        }

        private void tmrStateUpdate_Tick(object sender, EventArgs e)
        {
            tvInfo.BeginUpdate();

            TreeNode node = tvInfo.Nodes.GetOrCreate("ClientActive");
            node.Text = string.Format("Client Active: {0}", _droneClient.IsActive);

            node = tvInfo.Nodes.GetOrCreate("Navigation Data");
            if (_navigationData != null) DumpBranch(node.Nodes, _navigationData);

            node = tvInfo.Nodes.GetOrCreate("Configuration");
            if (_settings != null) DumpBranch(node.Nodes, _settings);

            TreeNode vativeNode = tvInfo.Nodes.GetOrCreate("Native");

            NavdataBag navdataBag;
            if (_navigationPacket.Data != null && NavdataBagParser.TryParse(ref _navigationPacket, out navdataBag))
            {
                var ctrl_state = (CTRL_STATES) (navdataBag.demo.ctrl_state >> 0x10);
                node = vativeNode.Nodes.GetOrCreate("ctrl_state");
                node.Text = string.Format("Ctrl State: {0}", ctrl_state);

                var flying_state = (FLYING_STATES) (navdataBag.demo.ctrl_state & 0xffff);
                node = vativeNode.Nodes.GetOrCreate("flying_state");
                node.Text = string.Format("Ctrl State: {0}", flying_state);

                DumpBranch(vativeNode.Nodes, navdataBag);
            }
            tvInfo.EndUpdate();

            if (_autopilot != null && !_autopilot.Active && btnAutopilot.ForeColor != Color.Black)
                btnAutopilot.ForeColor = Color.Black;
        }

        private void DumpBranch(TreeNodeCollection nodes, object o)
        {
            Type type = o.GetType();
         
            foreach (FieldInfo fieldInfo in type.GetFields())
            {
                TreeNode node = nodes.GetOrCreate(fieldInfo.Name);
                object value = fieldInfo.GetValue(o);

                DumpValue(fieldInfo.FieldType, node, value);
            }

            foreach (PropertyInfo propertyInfo in type.GetProperties())
            {
                TreeNode node = nodes.GetOrCreate(propertyInfo.Name);
                object value = propertyInfo.GetValue(o, null);

                DumpValue(propertyInfo.PropertyType, node, value);
            }
        }

        private void DumpValue(Type type, TreeNode node, object value)
        {
            if (value == null)
                node.Text = node.Name + ": null";
            else
            {
                if (type.Namespace.StartsWith("System") || type.IsEnum)
                    node.Text = node.Name + ": " + value;
                else
                    DumpBranch(node.Nodes, value);
            }
        }

        private void btnFlatTrim_Click(object sender, EventArgs e)
        {
            _droneClient.FlatTrim();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            _droneClient.Takeoff();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            _droneClient.Land();
        }

        private void btnEmergency_Click(object sender, EventArgs e)
        {
            _droneClient.Emergency();
        }

        private void btnReset_Click(object sender, EventArgs e)
        {
            _droneClient.ResetEmergency();
        }

        //private void btnSwitchCam_Click(object sender, EventArgs e)
        //{
        //    var configuration = new Settings();
        //    configuration.Video.Channel = VideoChannelType.Next;
        //    _droneClient.Send(configuration);
        //}

        private void btnSwitchCam_Click(object sender, EventArgs e)
        {
            var configuration = new Settings();
            if (isVertical)
            {
                configuration.Video.Channel = VideoChannelType.Horizontal;
                isVertical = false;
            }
            else
            {
                configuration.Video.Channel = VideoChannelType.Vertical;
                isVertical = true;
            }
            _droneClient.Send(configuration);
        }

        private void btnHover_Click(object sender, EventArgs e)
        {
            _droneClient.Hover();
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f);
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f);
        }

        private void btnTurnLeft_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, yaw: 0.25f);
        }

        private void btnTurnRight_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, yaw: -0.25f);
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, roll: -0.05f);
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, roll: 0.05f);
        }

        private void btnForward_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f);
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f);
        }

        private void btnReadConfig_Click(object sender, EventArgs e)
        {
            Task<Settings> configurationTask = _droneClient.GetConfigurationTask();
            configurationTask.ContinueWith(delegate(Task<Settings> task)
                {
                    if (task.Exception != null)
                    {
                        Trace.TraceWarning("Get configuration task is faulted with exception: {0}", task.Exception.InnerException.Message);
                        return;
                    }

                    _settings = task.Result;
                });
            configurationTask.Start();
        }

        private void btnSendConfig_Click(object sender, EventArgs e)
        {
            var sendConfigTask = new Task(() =>
                {
                    if (_settings == null) _settings = new Settings();
                    Settings settings = _settings;

                    if (string.IsNullOrEmpty(settings.Custom.SessionId) ||
                        settings.Custom.SessionId == "00000000")
                    {
                        // set new session, application and profile
                        _droneClient.AckControlAndWaitForConfirmation(); // wait for the control confirmation

                        settings.Custom.SessionId = Settings.NewId();
                        _droneClient.Send(settings);
                        
                        _droneClient.AckControlAndWaitForConfirmation();

                        settings.Custom.ProfileId = Settings.NewId();
                        _droneClient.Send(settings);
                        
                        _droneClient.AckControlAndWaitForConfirmation();

                        settings.Custom.ApplicationId = Settings.NewId();
                        _droneClient.Send(settings);
                        
                        _droneClient.AckControlAndWaitForConfirmation();
                    }

                    settings.General.NavdataDemo = false;
                    settings.General.NavdataOptions = NavdataOptions.All;

                    settings.Video.BitrateCtrlMode = VideoBitrateControlMode.Dynamic;
                    settings.Video.Bitrate = 1000;
                    settings.Video.MaxBitrate = 2000;

                    //settings.Leds.LedAnimation = new LedAnimation(LedAnimationType.BlinkGreenRed, 2.0f, 2);
                    //settings.Control.FlightAnimation = new FlightAnimation(FlightAnimationType.Wave);

                    // record video to usb
                    //settings.Video.OnUsb = true;
                    // usage of MP4_360P_H264_720P codec is a requirement for video recording to usb
                    //settings.Video.Codec = VideoCodecType.MP4_360P_H264_720P;
                    // start
                    //settings.Userbox.Command = new UserboxCommand(UserboxCommandType.Start);
                    // stop
                    //settings.Userbox.Command = new UserboxCommand(UserboxCommandType.Stop);


                    //send all changes in one pice
                    _droneClient.Send(settings);
                });
            sendConfigTask.Start();
        }

        private void StopRecording()
        {
            if (_packetRecorderWorker != null)
            {
                _packetRecorderWorker.Stop();
                _packetRecorderWorker.Join();
                _packetRecorderWorker = null;
            }
            if (_recorderStream != null)
            {
                _recorderStream.Dispose();
                _recorderStream = null;
            }
        }

        private void btnStartRecording_Click(object sender, EventArgs e)
        {
            string path = string.Format("flight_{0:yyyy_MM_dd_HH_mm}" + ARDroneTrackFileExt, DateTime.Now);

            using (var dialog = new SaveFileDialog {DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter, FileName = path})
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    StopRecording();

                    _recorderStream = new FileStream(dialog.FileName, FileMode.OpenOrCreate);
                    _packetRecorderWorker = new PacketRecorder(_recorderStream);
                    _packetRecorderWorker.Start();
                }
            }
        }

        private void btnStopRecording_Click(object sender, EventArgs e)
        {
            StopRecording();
        }

        private void btnReplay_Click(object sender, EventArgs e)
        {
            using (var dialog = new OpenFileDialog {DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter})
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    StopRecording();

                    var playerForm = new PlayerForm {FileName = dialog.FileName};
                    playerForm.Closed += (o, args) => _playerForms.Remove(o as PlayerForm);
                    _playerForms.Add(playerForm);
                    playerForm.Show(this);
                }
            }
        }

        // Make sure '_autopilot' variable is initialized with an object
        private void CreateAutopilot()
        {
            if (_autopilot != null) return;

            _autopilot = new Autopilot(_droneClient);
            _autopilot.OnOutOfObjectives += Autopilot_OnOutOfObjectives;
            _autopilot.BindToClient();
            _autopilot.Start();
        }

        // Event that occurs when no objectives are waiting in the autopilot queue
        private void Autopilot_OnOutOfObjectives()
        {
            _autopilot.Active = false;
        }

        // Create a simple mission for autopilot
        private void CreateAutopilotMission()
        {
            _autopilot.ClearObjectives();

            // Do two 36 degrees turns left and right if the drone is already flying
            if (_droneClient.NavigationData.State.HasFlag(NavigationState.Flying))
            {
                const float turn = (float)(Math.PI / 5);
                float heading = _droneClient.NavigationData.Yaw;

                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading + turn, aCanBeObtained: true)));
                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading - turn, aCanBeObtained: true)));
                _autopilot.EnqueueObjective(Objective.Create(2000, new Heading(heading, aCanBeObtained: true)));
            }
            else // Just take off if the drone is on the ground
            {
                _autopilot.EnqueueObjective(new FlatTrim(1000));
                _autopilot.EnqueueObjective(new Takeoff(3500));
            }

            // One could use hover, but the method below, allows to gain/lose/maintain desired altitude
            _autopilot.EnqueueObjective(
                Objective.Create(3000,
                    new VelocityX(0.0f),
                    new VelocityY(0.0f),
                    new Altitude(1.0f)
                )
            );

            _autopilot.EnqueueObjective(new Land(5000));
        }

        // Activate/deactive autopilot
        private void btnAutopilot_Click(object sender, EventArgs e)
        {
            if (!_droneClient.IsActive) return;

            CreateAutopilot();
            if (_autopilot.Active) _autopilot.Active = false;
            else
            {
                CreateAutopilotMission();
                _autopilot.Active = true;
                btnAutopilot.ForeColor = Color.Red;
            }
        }

        // Starts recording the navigation data
        private void RecordData_Click(object sender, EventArgs e)
        {
            StartRace();
        }

        private void TakeFramesFromVerticalCamera()
        {
            counter++;

            if (counter == 50)
            {
                var configuration = new Settings();
                configuration.Video.Channel = VideoChannelType.Vertical;
                _droneClient.Send(configuration);

                counter = 0;
            }

            if (counter == 1)
            {
                var configuration = new Settings();
                configuration.Video.Channel = VideoChannelType.Horizontal;
                _droneClient.Send(configuration);
            }
        }

        // stops recording the navigation data, and saves it to csv file
        private void StopRecordData_Click(object sender, EventArgs e)
        {
            EndRace();

        }

        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            //MessageBox.Show(e.KeyValue.ToString());

            switch (e.KeyValue)
            {
                case 32:
                    //           _droneClient.Takeoff();
                    StartRace();
                    break;
                case 67:
                    _droneClient.Hover();
                    break;
                case 86:
                    //               _droneClient.Land();
                    EndRace();
                    break;
                // go up
                case 87:
                    _droneClient.Progress(FlightMode.Progressive, gaz: 0.25f);
                    break;
                    //go down
                case 83:
                    _droneClient.Progress(FlightMode.Progressive, gaz: -0.25f);
                    break;
                    // turn right
                case 68:
                    _droneClient.Progress(FlightMode.Progressive, yaw: 0.25f);
                    break;
                    //turn left
                case 65:
                    _droneClient.Progress(FlightMode.Progressive, yaw: -0.25f);
                    break;
                    //go right
                case 74:
                    _droneClient.Progress(FlightMode.Progressive, roll: -0.05f);
                    break;
                    //go left
                case 76:
                    _droneClient.Progress(FlightMode.Progressive, roll: 0.05f);
                    break;
                    // go forward
                case 73:
                    _droneClient.Progress(FlightMode.Progressive, pitch: -0.05f);
                    break;
                    //go backward
                case 75:
                    _droneClient.Progress(FlightMode.Progressive, pitch: 0.05f);
                    break;
                default:
                    break;
            }
        }

        private void EndRace()
        {
            _raceController.endRace();
            //   tmrChangeQuadLocation.Enabled = false;
            drowMiniMap = false;
        }

        private void StartRace()
        {
            _raceController.startRace();
            //   tmrChangeQuadLocation.Enabled = true;
            drowMiniMap = true;
        }

        private void LedsShow_Click(object sender, EventArgs e)
        {
            ConfigCommand cm = new ConfigCommand("leds:leds_anim", ledAnimation.ToString() + ",1073741824,2");
            _droneClient.Send(cm);

            ledAnimation++;

            if (ledAnimation == 20)
                ledAnimation = 0;
        }

        /// <summary>
        /// Start listening to remote controll - Xbox
        /// </summary>
        private void RemoteListener()
        {
            xBoxHelper = new XboxHelper();
            oldOrders = new List<float>{ 0f, 0f, 0f, 0f };
            XboxTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckAndOperateXbox);
            XboxTimer.Interval = 16;
            XboxTimer.Start();
        }

        private void CheckAndOperateXbox(object sender, ElapsedEventArgs e)
        {
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            if (state.Buttons.A == XInputDotNetPure.ButtonState.Pressed)
            {
                StartRace();
                _droneClient.Takeoff();
            }
            else if (state.Buttons.B == XInputDotNetPure.ButtonState.Pressed)
            {
                _droneClient.Land();
                EndRace();
            }
            else
            {
                // on the thumbs: left right is x, up down is y
                List<float> navOrdersr = xBoxHelper.getNavOrders(state.ThumbSticks.Left.X, state.ThumbSticks.Left.Y, state.ThumbSticks.Right.X,
                    state.ThumbSticks.Right.Y);

                if (oldOrders[0] != navOrdersr[0] || oldOrders[1] != navOrdersr[1] ||
                    oldOrders[2] != navOrdersr[2] || oldOrders[3] != navOrdersr[3])
                {
                    // _droneClient.Progress(FlightMode.Progressive, roll, pitch, yaw, gaz);
                    _droneClient.Progress(FlightMode.Progressive, navOrdersr[0], navOrdersr[1], navOrdersr[2], navOrdersr[3]);

                    // Saves all the orders that are being sent to the drone
                    xBoxHelper.allNavOrders.Add(new navOrder()
                    { time = DateTime.Now.ToLongTimeString() + ":" + DateTime.Now.Millisecond, orders = navOrdersr });

                    oldOrders = navOrdersr;
                }
            }
        }

        /// <summary>
        /// Runs every x milliseconds to paint to current location of the quad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeQuadLocation_Tick(object sender, EventArgs e)
        {
            if (!_mapConf.CheckQuadInSquares(_raceController.X_cord, _raceController.Y_cord))
            {
                _paintingHelper.SnakePen = Pens.Red;
                _isOutOfBoundry = true;
            }
            else
            {
                if (_isOutOfBoundry)
                {
                    _paintingHelper.SnakePen = Pens.Green;
                    _isOutOfBoundry = false;
                }
            }

            _paintingHelper.DrawPoint(_raceController.X_cord, _raceController.Y_cord);

            /////////////////////////////////// stub to load fake nav data to be deleted TODO
            //if (!_mapConf.CheckQuadInSquares(float.Parse(allRaws[count][0]), float.Parse(allRaws[count][1])))
            //{
            //    _paintingHelper.SnakePen = Pens.Red;
            //    _isOutOfBoundry = true;
            //}
            //else
            //{
            //    if (_isOutOfBoundry)
            //    {
            //        _paintingHelper.SnakePen = Pens.Green;
            //        _isOutOfBoundry = false;
            //    }
            //}

            //if (allRaws.Count > count)
            //{
            //    float x = float.Parse(allRaws[count][0]);
            //    float y = float.Parse(allRaws[count][1]);

            //    _paintingHelper.DrawPoint(x, y);
            //}
            //else
            //{
            //    tmrChangeQuadLocation.Enabled = false;
            //}

            //count += 100;
            /////////////////////////////////// stub to load fake nav data to be deleted TODO
        }

        /// <summary>
        /// When the screen is done Painting itself, draw a ractangle for the quad corse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            _paintingHelper.DrawRectangle();
        }

        private void btnCleanMap_Click(object sender, EventArgs e)
        {
            _paintingHelper.CleanMap(this);
        }

        private void btnDrawTrack_Click(object sender, EventArgs e)
        {
            _paintingHelper.DrawTrack();
        }

        /// <summary>
        /// Stub to be deleted TODO
        /// </summary>
        //private void loadFakeDataFromFile()
        //{
        //    CsvFileReader csvReader = new CsvFileReader(@"C:\Users\Pariente\Desktop\mahanet 2016\out3.csv");
        //    CsvRow csvRaw = new CsvRow();
        //    while (csvReader.ReadRow(csvRaw))
        //    {
        //        CsvRow csvRaw1 = new CsvRow();
        //        foreach (string item in csvRaw)
        //        {
        //            csvRaw1.Add(item);
        //        }

        //        allRaws.Add(csvRaw1);
        //    }
        //}
    }
}