﻿//#define USE_STUB

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

        private static int numberOfErrorsToDeath = 5;

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
        private HighScore _highScore;
        bool drowMiniMap = false;
        bool _isOutOfBoundry = false;
        int _currentScore = 1000;
        DateTime countDownTime = DateTime.Now;

        int counterToDeath = 0;

        //System.Timers.Timer startingTimer = new System.Timers.Timer(1000);
        int _startingCountdown = 3;

        private DateTime _startingTimeForFlight;

        int counter = 2;
        int ledAnimation = 0;

        bool isVertical = true;

        System.Timers.Timer recoredTimer = new System.Timers.Timer();
        System.Timers.Timer XboxTimer = new System.Timers.Timer();
        XboxHelper xBoxHelper;
        List<float> oldOrders;
        private bool _isGetStarted;

#if USE_STUB
        List<CsvRow> allRaws = new List<CsvRow>(); // stub to load fake nav data to be deleted TODO
        int count = 0; // stub to load fake nav data to be deleted TODO
        float startingYaw = 0; // stub to load fake nav data to be deleted TODO  
#endif

        public MainForm()
        {
            InitializeComponent();

            _videoPacketDecoderWorker = new VideoPacketDecoderWorker(PixelFormat.BGR24, true, OnVideoPacketDecoded);
            _videoPacketDecoderWorker.Start();

            _mapConf = new MapConfiguration(1, 680, 50);
            _paintingHelper = new PaintingHelper(_mapConf, this.CreateGraphics()); // Generates class to control all the painting
            _raceController = new RaceController(_mapConf); // Generates controler for the race

            _highScore = new HighScore();
            _highScore.LoadHighScore();

            btnNewScore.Enabled = false;
            UpdateScoresTables();

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


#if USE_STUB
            loadFakeDataFromFile(); // stub to load fake nav data to be deleted TODO
            startingYaw = float.Parse(allRaws[1][5]); // stub to load fake nav data to be deleted TODO  
#endif
        }

        private void UpdateScoresTables()
        {
            tblScores.Items.Clear();
            foreach (HighScoreRecord recored in _highScore.HighScores)
            {
                tblScores.Items.Add(new ListViewItem(new[] { recored.Name, recored.Score.ToString() }));
            }
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

            //SendSettings();
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

            if (_raceController.IsRacing)
            {
                TimeSpan tsInterval = DateTime.Now - _startingTimeForFlight;
                tbMin.Text = tsInterval.Minutes.ToString();
                tbSec.Text = tsInterval.Seconds.ToString();
            }

            //if (!_mapConf.CheckQuadInSquares(_raceController.X_cord, _raceController.Y_cord, _raceController.Z_cord))
            //{
            //    _paintingHelper.SnakePen = Pens.Red;
            //    _isOutOfBoundry = true;

            //    counterToDeath++;
            //    if (counterToDeath >= numberOfErrorsToDeath)
            //    {
            //        _droneClient.Land();
            //        EndRace();
            //    }
            //}
            //else
            //{
            //    if (_isOutOfBoundry)
            //    {
            //        _paintingHelper.SnakePen = Pens.Green;
            //        _isOutOfBoundry = false;
            //    }
            //}

            //// Changes the rectangle size acording to the quad location
            //_paintingHelper.ChangeVideoRectangleSize(_raceController.X_cord, _raceController.Y_cord, _raceController.GetYawInDegrees());

            //// Draws the quad location on the minimap
            //_paintingHelper.DrawPoint(_raceController.X_cord, _raceController.Y_cord);

            ////// the real code




#if USE_STUB
                        /// stub to get image instead of real bitmap
                        try
                        {
                            _frameBitmap = new Bitmap(@"C:\Users\Pariente\Pictures\IMG_3640.JPG");
                        }
                        catch (Exception)
                        {

                            _frameBitmap = new Bitmap(@"D:\Dev\quad\Desert.jpg");
                        }
#else
            if (_frame == null || _frameNumber == _frame.Number)
                return;
            _frameNumber = _frame.Number;

            if (_frameBitmap == null)
               _frameBitmap = VideoHelper.CreateBitmap(ref _frame);
            else
               VideoHelper.UpdateBitmap(ref _frameBitmap, ref _frame);
#endif


            if (_isGetStarted)
            {
                _droneClient.Takeoff();
                HoverAboveRoundel();
                int sec = (DateTime.Now - countDownTime).Seconds;
                if (sec <= 6)
                {
                    _paintingHelper.DrawNumber(6 - sec, _frameBitmap);
                }
                else
                {
                    _isGetStarted = false;
                    if (_settings == null) _settings = new Settings();
                    Settings settings = _settings;

                    settings.Control.FlyingMode = 0;
                    _droneClient.Send(settings);

                    StartRace();
                }
            }

            //test to paint square on the video image
            if (_paintingHelper.IsGateSeeable)
            {
                _paintingHelper.DrawRectangleOnVideo(_frameBitmap);
            }
            else if (_paintingHelper.IsArrowSeeable)
            {
                _paintingHelper.DrawArrowOnVideo(_frameBitmap);
            }

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
                var ctrl_state = (CTRL_STATES)(navdataBag.demo.ctrl_state >> 0x10);
                node = vativeNode.Nodes.GetOrCreate("ctrl_state");
                node.Text = string.Format("Ctrl State: {0}", ctrl_state);

                var flying_state = (FLYING_STATES)(navdataBag.demo.ctrl_state & 0xffff);
                node = vativeNode.Nodes.GetOrCreate("flying_state");
                node.Text = string.Format("Ctrl State: {0}", flying_state);

                DumpBranch(vativeNode.Nodes, navdataBag);
            }
            tvInfo.EndUpdate();

            //if (_autopilot != null && !_autopilot.Active && btnAutopilot.ForeColor != Color.Black)
            //    btnAutopilot.ForeColor = Color.Black;
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
            EndRace();
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
            configurationTask.ContinueWith(delegate (Task<Settings> task)
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
            SendSettings();
        }

        private void SendSettings()
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
                settings.Detect.Type = 12;
                settings.Detect.DetectionsSelectV = 8;
                //settings.Detect.DetectionsSelectH = 1;
                settings.Video.BitrateCtrlMode = VideoBitrateControlMode.Dynamic;
                settings.Video.Bitrate = 1000;
                settings.Video.MaxBitrate = 2000;
                //settings.Video.Channel = VideoChannelType.Vertical;

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
                //_settings = settings;
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

            using (var dialog = new SaveFileDialog { DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter, FileName = path })
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
            using (var dialog = new OpenFileDialog { DefaultExt = ARDroneTrackFileExt, Filter = ARDroneTrackFilesFilter })
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    StopRecording();

                    var playerForm = new PlayerForm { FileName = dialog.FileName };
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
        //private void btnAutopilot_Click(object sender, EventArgs e)
        //{
        //    if (!_droneClient.IsActive) return;

        //    CreateAutopilot();
        //    if (_autopilot.Active) _autopilot.Active = false;
        //    else
        //    {
        //        CreateAutopilotMission();
        //        _autopilot.Active = true;
        //        btnAutopilot.ForeColor = Color.Red;
        //    }
        //}

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
                    GetStarted();
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
            if (_paintingHelper.IsQuadPassedInAllGates())
            {
                tbMin.ForeColor = Color.Green;
                tbSec.ForeColor = Color.Green;
            }
            else
            {
                tbMin.ForeColor = Color.Red;
                tbSec.ForeColor = Color.Red;
            }
        }

        private void StartRace()
        {
            if (!_raceController.IsRacing)
            {
                _paintingHelper.ResetGates();
                counterToDeath = 0;
                _raceController.startRace();
                _startingTimeForFlight = DateTime.Now;
                //   tmrChangeQuadLocation.Enabled = true;
                drowMiniMap = true;
            }
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
            oldOrders = new List<float> { 0f, 0f, 0f, 0f };
            XboxTimer.Elapsed += new System.Timers.ElapsedEventHandler(CheckAndOperateXbox);
            XboxTimer.Interval = 16;
            XboxTimer.Start();
        }

        private void CheckAndOperateXbox(object sender, ElapsedEventArgs e)
        {
            GamePadState state = GamePad.GetState(PlayerIndex.One);
            if (state.Buttons.A == XInputDotNetPure.ButtonState.Pressed)
            {
                //if (_raceController.IsRacing)
                //{
                //    if (_settings == null) _settings = new Settings();
                //    Settings settings = _settings;

                //    settings.Control.FlyingMode = 0;
                //    _droneClient.Send(settings);
                //    return;
                //}


                //StartRace();
                _isGetStarted = true;
                countDownTime = DateTime.Now;
            }
            else if (state.Buttons.B == XInputDotNetPure.ButtonState.Pressed)
            {
                if (_paintingHelper.IsQuadPassedInAllGates())
                {
                    EndRace();

                    TimeSpan tsInterval = DateTime.Now - _startingTimeForFlight;
                    _currentScore = (int)tsInterval.TotalSeconds;

                    if (_currentScore < _highScore.HighScores[0].Score)
                    {
                        while (_raceController.Z_cord < 0.8)
                        {
                            _droneClient.Progress(FlightMode.Progressive, 0f, 0f, 0f, 0.5f);
                        }

                        CreateFlightAnimation(FlightAnimationType.Wave);

                        Thread.Sleep(5000);
                    }

                    _droneClient.Land();
                   // btnNewScore.Enabled = true;
                }
                else
                {
                    _droneClient.Land();
                    EndRace();
                }
            }
            else if (state.Buttons.X == XInputDotNetPure.ButtonState.Pressed)
            {
                if (_raceController.IsRacing)
                {
                    CreateFlightAnimation(FlightAnimationType.FlipRight);
                }
            }
            else if (_raceController.IsRacing)
            {
                // on the thumbs: left right is x, up down is y
                //state.DPad.Up, state.DPad.Right, state.DPad.Down, state.DPad.Left
                List<float> navOrdersr = xBoxHelper.getNavOrders(state.DPad.Left, state.DPad.Right, state.DPad.Up, state.DPad.Down, state.ThumbSticks.Right.X,
                    state.ThumbSticks.Right.Y);

                if (oldOrders[0] != navOrdersr[0] || oldOrders[1] != navOrdersr[1] ||
                    oldOrders[2] != navOrdersr[2] || oldOrders[3] != navOrdersr[3])
                {
                    // lets the race controller know if there is suppose to be a turn
                    if (oldOrders[2] != 0 || navOrdersr[2] != 0)
                    {
                        _raceController.IsSupposeToTurn = true;
                    }
                    else
                    {
                        _raceController.IsSupposeToTurn = false;
                    }

                    // _droneClient.Progress(FlightMode.Progressive, roll, pitch, yaw, gaz);
                    _droneClient.Progress(FlightMode.Progressive, navOrdersr[0], navOrdersr[1], navOrdersr[2], navOrdersr[3]);

                    // Saves all the orders that are being sent to the drone
                    xBoxHelper.allNavOrders.Add(new navOrder()
                    { time = DateTime.Now.ToLongTimeString() + ":" + DateTime.Now.Millisecond, orders = navOrdersr });

                    oldOrders = navOrdersr;
                }
                else if (navOrdersr[0] == 0.0f && navOrdersr[1] == 0.0f &&
                        navOrdersr[2] == 0.0f && navOrdersr[3] == 0.0f)
                {
                    _droneClient.Progress(FlightMode.Hover, navOrdersr[0], navOrdersr[1], navOrdersr[2], navOrdersr[3]);
                }
            }
        }

        private void CreateFlightAnimation(FlightAnimationType animation)
        {
            if (_settings == null) _settings = new Settings();
            Settings settings = _settings;

            settings.Control.FlightAnimation = new FlightAnimation(animation);
            _droneClient.Send(settings);
        }

        private void GetStarted()
        {
            //_startingCountdown = 3;

            //startingTimer.Elapsed += countDown;
            //startingTimer.Enabled = true;
            
        }

        private void HoverAboveRoundel()
        {
            if (_raceController.IsRacing)
            {
                if (_settings == null) _settings = new Settings();
                Settings settings = _settings;

                settings.Control.FlyingMode = 2;
                _droneClient.Send(settings);
            }
        }

        //private void countDown(object sender, ElapsedEventArgs e)
        //{
        //    _startingCountdown--;
            
        //    if (_startingCountdown <= 0)
        //    {
        //        startingTimer.Enabled = false;
        //        if (_settings == null) _settings = new Settings();
        //        Settings settings = _settings;

        //        settings.Control.FlyingMode = 0;
        //        _droneClient.Send(settings);

        //        StartRace();
        //    }
        //}

        /// <summary>
        /// Runs every x milliseconds to paint to current location of the quad
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeQuadLocation_Tick(object sender, EventArgs e)
        {
#if !USE_STUB
            // Checks if the quad is inside the allowed area
            if (!_mapConf.CheckQuadInSquares(_raceController.X_cord, _raceController.Y_cord, _raceController.Z_cord))
            {
                _paintingHelper.SnakePen = Pens.Red;
                _isOutOfBoundry = true;

                counterToDeath++;
                if (counterToDeath >= numberOfErrorsToDeath)
                {
                    _droneClient.Land();
                    EndRace();
                }
            }
            else
            {
                if (_isOutOfBoundry)
                {
                    _paintingHelper.SnakePen = Pens.Green;
                    _isOutOfBoundry = false;
                }
            }

            // Changes the rectangle size acording to the quad location
            _paintingHelper.ChangeVideoRectangleSize(_raceController.X_cord, _raceController.Y_cord, _raceController.GetYawInDegrees());

            // Draws the quad location on the minimap
            _paintingHelper.DrawPoint(_raceController.X_cord, _raceController.Y_cord);
#endif

#if USE_STUB
            /////////////////////////////////// stub to load fake nav data to be deleted TODO
            if (allRaws.Count > count)
            {
                if (!_mapConf.CheckQuadInSquares(float.Parse(allRaws[count][0]), float.Parse(allRaws[count][1]),
                    float.Parse(allRaws[count][2])))
                {
                    _paintingHelper.SnakePen = new Pen(Color.Red, _paintingHelper.SnakeSize);
                    _isOutOfBoundry = true;

                    counterToDeath++;
                    if (counterToDeath >= numberOfErrorsToDeath)
                    {
                        _droneClient.Land();
                        EndRace();
                    }
                }
                else
                {
                    if (_isOutOfBoundry)
                    {
                        _paintingHelper.SnakePen = new Pen(Color.Green, _paintingHelper.SnakeSize);
                        _isOutOfBoundry = false;
                    }
                }
            }

            if (allRaws.Count > count)
            {
                float x = float.Parse(allRaws[count][0]);
                float y = float.Parse(allRaws[count][1]);

                _paintingHelper.DrawPoint(x, y);

                // Changes the rectangle size acording to the quad location
                _paintingHelper.ChangeVideoRectangleSize(float.Parse(allRaws[count][0]), float.Parse(allRaws[count][1]),
                    (float.Parse(allRaws[count][5]) - startingYaw) * (180 / Math.PI));

            }
            else
            {
                tmrChangeQuadLocation.Enabled = false;
            }

            count += 100;

            /////////////////////////////////// stub to load fake nav data to be deleted TODO  
#endif
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

        private void btnNewScore_Click(object sender, EventArgs e)
        {
            if (tbNewScore.Text != null && tbNewScore.Text != "")
            {
                _highScore.AddToHighScore(tbNewScore.Text, _currentScore);
                UpdateScoresTables();
                _currentScore = 1000;
                btnNewScore.Enabled = false;
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            _highScore.SaveHighScore();
        }

#if USE_STUB
        /// <summary>
        /// Stub to be deleted TODO
        /// </summary>
        private void loadFakeDataFromFile()
        {
            CsvFileReader csvReader;
            try
            {
                csvReader = new CsvFileReader(@"C:\Users\Pariente\Desktop\mahanet 2016\out9.csv");
            }
            catch (Exception)
            {

                csvReader = new CsvFileReader(@"D:\Dev\quad\out.csv");
            }
            CsvRow csvRaw = new CsvRow();
            while (csvReader.ReadRow(csvRaw))
            {
                CsvRow csvRaw1 = new CsvRow();
                foreach (string item in csvRaw)
                {
                    csvRaw1.Add(item);
                }

                allRaws.Add(csvRaw1);
            }
        }

#endif
    }
}