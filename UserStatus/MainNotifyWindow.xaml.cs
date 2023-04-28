namespace SensioUserStatus
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Drawing;
    using System.IO;
    using System.Net.NetworkInformation;
    using System.Reflection;
    using System.Runtime.InteropServices;
    using System.Text.RegularExpressions;
    using System.Timers;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Interop;
    using System.Windows.Media;
    using System.Windows.Media.Animation;
    using System.Windows.Media.Imaging;
    using ExtendedWindowsControls;
    using SensioUserStatus.Classes;

    public partial class MainNotifyWindow : Window
    {
        private static ExtendedNotifyIcon extendedNotifyIcon;

        private Storyboard gridFadeInStoryBoard;
        private Storyboard gridFadeOutStoryBoard;
        //private static MainNotifyWindow _instance;

        public static bool settingChanged = false;
        public static double interval = 10000;
        public static UserName userName = new UserName() { userName = null };
        public static string fullUserName = "";
        public static string user_id = null;

        private static string statusLabelStat = "";
        private static string userNameStat = " ";
        public static ImageSource statusImageSourceStat = new BitmapImage(new Uri("pack://application:,,/Images/UnloggedOrb.ico"));
        public static string userNameColorStat = "";
        private static string userNameStatCursor = "";
        private static string appNameStat = "";
        private static bool radioButtonStat = false;
        private static bool radioButtonVarStat1 = false;
        private static bool radioButtonVarStat2 = false;
        private static bool radioButtonVarStat3 = false;
        private static bool radioButtonVarStat4 = false;
        private static bool updateButtonStat = false;
        private static bool setButtonStat = false;
        private static string runButtonVisibilityStat = "Visible";
        private static string setButtonVisibilityStat = "Hidden";
        public static string setDndValueStat = null;
        public static string setOfflineValueStat = null;

        public static string updateButtonToolTip = " ";
        private static System.Timers.Timer userSendApiTimer = new System.Timers.Timer() { Enabled = false, Interval = interval };

        private static Api_Format watingUserRequest = new Api_Format();
        private static string watingIconPrefix = "";

        public static event EventHandler StatusLabelStatChanged;
        public static event EventHandler UserNameStatChanged;
        public static event EventHandler UserNameColorStatChanged;
        public static event EventHandler UserNameStatCursorChanged;
        public static event EventHandler RadioButtonStatChanged;
        public static event EventHandler RadioButtonVarStat1Changed;
        public static event EventHandler RadioButtonVarStat2Changed;
        public static event EventHandler RadioButtonVarStat3Changed;
        public static event EventHandler RadioButtonVarStat4Changed;
        public static event EventHandler UpdateButtonStatChanged;
        public static event EventHandler UpdateButtonToolTipChanged;
        public static event EventHandler AppNameStatChanged;
        public static event EventHandler StatusImageSourceStatChanged;
        public static event EventHandler SetButtonStatChanged;
        public static event EventHandler SetButtonVisibilityStatChanged;
        public static event EventHandler RunButtonVisibilityStatChanged;
        public static event EventHandler SetDndValueStatChanged;
        public static event EventHandler SetOfflineValueStatChanged;

        public static string StatusLabelStat
        {
            get => statusLabelStat;
            set
            {
                statusLabelStat = value;
                StatusLabelStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string UserNameStat
        {
            get => userNameStat;
            set
            {
                userNameStat = value;
                UserNameStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string UserNameColorStat
        {
            get => userNameColorStat;
            set
            {
                userNameColorStat = value;
                UserNameColorStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string UserNameStatCursor
        {
            get => userNameStatCursor;
            set
            {
                userNameStatCursor = value;
                UserNameStatCursorChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool RadioButtonVarStat1
        {
            get => radioButtonVarStat1;
            set
            {
                radioButtonVarStat1 = value;
                RadioButtonVarStat1Changed?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool RadioButtonVarStat2
        {
            get => radioButtonVarStat2;
            set
            {
                radioButtonVarStat2 = value;
                RadioButtonVarStat2Changed?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool RadioButtonVarStat3
        {
            get => radioButtonVarStat3;
            set
            {
                radioButtonVarStat3 = value;
                RadioButtonVarStat3Changed?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool RadioButtonVarStat4
        {
            get => radioButtonVarStat4;
            set
            {
                radioButtonVarStat4 = value;
                RadioButtonVarStat4Changed?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool RadioButtonStat
        {
            get => radioButtonStat;
            set
            {
                radioButtonStat = value;
                RadioButtonStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }
        public static bool UpdateButtonStat
        {
            get => updateButtonStat;
            set
            {
                updateButtonStat = value;
                UpdateButtonStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static bool SetButtonStat
        {
            get => setButtonStat;
            set
            {
                setButtonStat = value;
                SetButtonStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string RunButtonVisibilityStat
        {
            get => runButtonVisibilityStat;
            set
            {
                runButtonVisibilityStat = value;
                RunButtonVisibilityStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string SetButtonVisibilityStat
        {
            get => setButtonVisibilityStat;
            set
            {
                setButtonVisibilityStat = value;
                SetButtonVisibilityStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string UpdateButtonToolTip
        {
            get => updateButtonToolTip;
            set
            {
                updateButtonToolTip = value;
                UpdateButtonToolTipChanged?.Invoke(null, EventArgs.Empty);
            }
        }


        public static string AppNameStat
        {
            get => appNameStat;
            set
            {
                appNameStat = value;
                AppNameStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static System.Windows.Media.ImageSource StatusImageSourceStat
        {
            get => statusImageSourceStat;
            set
            {
                statusImageSourceStat = value;
                StatusImageSourceStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string SetDndValueStat
        {
            get => setDndValueStat;
            set
            {
                setDndValueStat = value;
                SetDndValueStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public static string SetOfflineValueStat
        {
            get => setOfflineValueStat;
            set
            {
                setOfflineValueStat = value;
                SetOfflineValueStatChanged?.Invoke(null, EventArgs.Empty);
            }
        }

        public MainNotifyWindow()
        {

            userSendApiTimer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            Functions.sendApiTimer.Elapsed += new ElapsedEventHandler(Functions.OnElapsedTime);
            Functions.getApiTimer.Elapsed += new ElapsedEventHandler(Functions.GetElapsedTime);

            InitializeComponent();

            dnd_time.Text = null;
            offline_time.Text = null;

            extendedNotifyIcon = new ExtendedNotifyIcon();
            extendedNotifyIcon.MouseLeave += new ExtendedNotifyIcon.MouseLeaveHandler(extendedNotifyIcon_OnHideWindow);
            extendedNotifyIcon.MouseMove += new ExtendedNotifyIcon.MouseMoveHandler(extendedNotifyIcon_OnShowWindow);
            AppNameStat = extendedNotifyIcon.targetNotifyIcon.Text = "Sensio User Status "+ Assembly.GetEntryAssembly().GetName().Version.ToString().Substring(0, Assembly.GetEntryAssembly().GetName().Version.ToString().Length - 2);
            SetNotifyIcon("Unlogged");

           

            SetWindowToBottomRightOfScreen();
            this.Opacity = 0;
            uiGridMain.Opacity = 0;

            TitleLabel.MouseLeftButtonDown += new MouseButtonEventHandler(title_MouseLeftButtonDown);
            UserName.MouseLeftButtonDown += new MouseButtonEventHandler(OpenBrowser);

            gridFadeOutStoryBoard = (Storyboard)this.TryFindResource("gridFadeOutStoryBoard");
            gridFadeOutStoryBoard.Completed += new EventHandler(gridFadeOutStoryBoard_Completed);
            gridFadeInStoryBoard = (Storyboard)TryFindResource("gridFadeInStoryBoard");
            gridFadeInStoryBoard.Completed += new EventHandler(gridFadeInStoryBoard_Completed);

            Functions.loadSettings();
            Functions.CheckTokenExist();

            //PinButton.IsChecked = true;
            //PinButton_Click(PinButton, e: new RoutedEventArgs());
            //extendedNotifyIcon_OnShowWindow();
        }

      
        public static void enableChangeStatus()
        {

            if (String.IsNullOrEmpty(userName.userName))
            {
                RadioButtonStat = false;
                if (Functions.offlineStatus) {
                    UserNameStat = "Server není dostupný";
                    UserNameColorStat = "Orange";
                    UserNameStatCursor = "Arrow";
                }
                else {
                    UserNameStat = "Neregistrovaný Uživatel";
                    UserNameColorStat = "Red";
                }
                UserNameStatCursor = "Hand";
                StatusLabelStat = "Status nelze mìnit";
            }
            else
            {
                if (fullUserName == "Neregistrovaný Uživatel")
                {
                    RadioButtonStat = false;
                    UserNameStat = fullUserName;
                    UserNameColorStat = "Red";
                    UserNameStatCursor = "Hand"; //"Arrow";
                }
                else
                {

                    if (Functions.offlineStatus)
                    {
                        RadioButtonStat = false;
                        UserNameStat = "Server není dostupný";
                        UserNameColorStat = "Orange";
                        UserNameStatCursor = "Arrow";
                    }
                    else
                    {
                        RadioButtonStat = true;
                        UserNameStat = fullUserName;
                        UserNameStatCursor = "Hand";// "Arrow";
                        StatusLabelStat = "Status lze mìnit";
                        UserNameColorStat = "Blue";
                    }
                }
            }
        }

        private void title_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void OpenBrowser(object sender, MouseButtonEventArgs e)
        {
            if (UserNameColorStat == "Red") {
                Functions.OpenBrowser();
            } else if (UserNameColorStat == "Blue")
            {
                Functions.OpenDasboard();
            }
        }



        public static void SetNotifyIconStat(string iconPrefix, int status)
        {
            if (Functions.offlineStatus) { iconPrefix = "Net"; }

            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico")).Stream;
            extendedNotifyIcon.targetNotifyIcon.Icon = new Icon(iconStream);
            ImageSource changedImage = new BitmapImage(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico"));
            changedImage.Freeze();
            StatusImageSourceStat = changedImage;

            switch (status)
            {
                case 0:
                    RadioButtonVarStat1 = false;
                    RadioButtonVarStat2 = false;
                    RadioButtonVarStat3 = false;
                    RadioButtonVarStat4 = false;
                    break;
                case 1:
                    RadioButtonVarStat1 = true;
                    RadioButtonVarStat2 = false;
                    RadioButtonVarStat3 = false;
                    RadioButtonVarStat4 = false;
                    break;
                case 2:
                    RadioButtonVarStat1 = false;
                    RadioButtonVarStat2 = true;
                    RadioButtonVarStat3 = false;
                    RadioButtonVarStat4 = false;
                    break;
                case 3:
                    RadioButtonVarStat1 = false;
                    RadioButtonVarStat2 = false;
                    RadioButtonVarStat3 = true;
                    RadioButtonVarStat4 = false;
                    break;
                case 4:
                    RadioButtonVarStat1 = false;
                    RadioButtonVarStat2 = false;
                    RadioButtonVarStat3 = false;
                    RadioButtonVarStat4 = true;
                    break;
                default:
                    RadioButtonVarStat1 = false;
                    RadioButtonVarStat2 = false;
                    RadioButtonVarStat3 = false;
                    RadioButtonVarStat4 = false;
                    break;
            }

        }

        private void SetNotifyIcon(string iconPrefix)
        {
            Stream iconStream = Application.GetResourceStream(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico")).Stream;
            extendedNotifyIcon.targetNotifyIcon.Icon = new Icon(iconStream);
            ImageSource changedImage = new BitmapImage(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico"));
            changedImage.Freeze();
            StatusImageSourceStat = changedImage;

            if (!String.IsNullOrEmpty(userName.userName))
            {
                Api_Format sendData = new Api_Format();
                sendData.username = userName.userName;
                sendData.token = userName.Token;
                sendData.pcname = userName.PcName;
                sendData.user_id = userName.user_id;
                switch (iconPrefix)
                {
                    case "Unlogged":
                        sendData.status = 2;
                        Stream iconStreamUnlogged = Application.GetResourceStream(new Uri("pack://application:,,/Images/UnloggedOrb.ico")).Stream;
                        extendedNotifyIcon.targetNotifyIcon.Icon = new Icon(iconStreamUnlogged);
                        ImageSource changedImageUnlogged = new BitmapImage(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico"));
                        changedImageUnlogged.Freeze();
                        StatusImageSourceStat = changedImageUnlogged;
                        break;
                    case "Red":
                        sendData.status = 1;
                        Stream iconStreamRed = Application.GetResourceStream(new Uri("pack://application:,,/Images/RedOrb.ico")).Stream;
                        extendedNotifyIcon.targetNotifyIcon.Icon = new Icon(iconStreamRed);
                        ImageSource changedImageRed = new BitmapImage(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico"));
                        changedImageRed.Freeze();
                        StatusImageSourceStat = changedImageRed;
                        break;
                    case "Green":
                        sendData.status = 2;
                        Stream iconStreamGreen = Application.GetResourceStream(new Uri("pack://application:,,/Images/GreenOrb.ico")).Stream;
                        extendedNotifyIcon.targetNotifyIcon.Icon = new Icon(iconStreamGreen);
                        ImageSource changedImageGreen = new BitmapImage(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico"));
                        changedImageGreen.Freeze();
                        StatusImageSourceStat = changedImageGreen;
                        break;
                    case "Amber":
                        sendData.status = 3;
                        Stream iconStreamAmber = Application.GetResourceStream(new Uri("pack://application:,,/Images/AmberOrb.ico")).Stream;
                        extendedNotifyIcon.targetNotifyIcon.Icon = new Icon(iconStreamAmber);
                        ImageSource changedImageAmber = new BitmapImage(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico"));
                        changedImageAmber.Freeze();
                        StatusImageSourceStat = changedImageAmber;
                        break;
                    case "Purple":
                        sendData.status = 4;
                        Stream iconStreamPurple = Application.GetResourceStream(new Uri("pack://application:,,/Images/PurpleOrb.ico")).Stream;
                        extendedNotifyIcon.targetNotifyIcon.Icon = new Icon(iconStreamPurple);
                        ImageSource changedImagePurple = new BitmapImage(new Uri("pack://application:,,/Images/" + iconPrefix + "Orb.ico"));
                        changedImagePurple.Freeze();
                        StatusImageSourceStat = changedImagePurple;
                        break;
                    default:
                        sendData.status = 0;
                        break;
                } 

                if (!userSendApiTimer.Enabled) {
                    if (Functions.sendAPI(Functions.settings.apiUrl, sendData))
                    {
                        userSendApiTimer.Enabled = false;
                        watingUserRequest = new Api_Format();
                        watingIconPrefix = "";
                        enableChangeStatus();
                        SetNotifyIconStat(iconPrefix, sendData.status);
                    }
                    else
                    {
                        watingUserRequest = sendData;
                        watingIconPrefix = iconPrefix;
                        if (!userSendApiTimer.Enabled) userSendApiTimer.Enabled = true;

                        enableChangeStatus();
                        SetNotifyIconStat("Unlogged", 0);
                    }
                }
             

            }
        }

        public static void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            if (Functions.sendAPI(Functions.settings.apiUrl, watingUserRequest))
            {
                userSendApiTimer.Enabled = false;
                enableChangeStatus();
                SetNotifyIconStat(watingIconPrefix, watingUserRequest.status);
                watingUserRequest = new Api_Format();
                watingIconPrefix = "";
            }
            else
            {
                enableChangeStatus();
                SetNotifyIconStat("Unlogged", 0);
            }

        }

        private void SetWindowToBottomRightOfScreen()
        {
            Left = SystemParameters.WorkArea.Width - Width - 10;
            Top = SystemParameters.WorkArea.Height - Height;
        }

        void extendedNotifyIcon_OnShowWindow()
        {
            gridFadeOutStoryBoard.Stop();
            this.Opacity = 1;
            this.Topmost = true;
            if (uiGridMain.Opacity > 0 && uiGridMain.Opacity < 1)
            {
                uiGridMain.Opacity = 1;
            }
            else if (uiGridMain.Opacity == 0)
            {
                gridFadeInStoryBoard.Begin();
            }
        }

        void extendedNotifyIcon_OnHideWindow()
        {
            if (PinButton.IsChecked == true) return;
            gridFadeInStoryBoard.Stop();
            if (uiGridMain.Opacity == 1 && this.Opacity == 1)
                gridFadeOutStoryBoard.Begin();
            else
            {
                uiGridMain.Opacity = 0;
                this.Opacity = 0;
            }
        }

        private void uiWindowMainNotification_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            extendedNotifyIcon.StopMouseLeaveEventFromFiring();
            gridFadeOutStoryBoard.Stop();
            uiGridMain.Opacity = 1;
        }

        private void uiWindowMainNotification_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            extendedNotifyIcon_OnHideWindow();
        }

        void gridFadeOutStoryBoard_Completed(object sender, EventArgs e)
        {
            this.Opacity = 0;
        }

        void gridFadeInStoryBoard_Completed(object sender, EventArgs e)
        {
            this.Opacity = 1;
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            if (PinButton.IsChecked == true)
                PinImage.Source = new BitmapImage(new Uri("pack://application:,,/Images/Pinned.png"));
            else
                PinImage.Source = new BitmapImage(new Uri("pack://application:,,/Images/Un-Pinned.png"));
        }

        private void colourRadioButton_Click(object sender, RoutedEventArgs e)
        {
            SetNotifyIcon(((RadioButton)sender).Tag.ToString());
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            extendedNotifyIcon.Dispose();
            this.Close();
        }

        private void UpdButton_Click(object sender, RoutedEventArgs e)
        {
            Functions.RunUpdate();
        }

        private void SetButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetButtonVisibilityStat == "Visible")
            {
                SetButtonVisibilityStat = "Hidden";
                RunButtonVisibilityStat = "Visible";
            }
            else {
                SetButtonVisibilityStat = "Visible";
                RunButtonVisibilityStat = "Hidden";
            }

        }
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");

            if (((TextBox)sender).Name == "dnd_time" && !regex.IsMatch(e.Text) && Convert.ToInt32(dnd_time.Text.Insert(((TextBox)e.Source).CaretIndex,e.Text)) >= 1 && Convert.ToInt32(dnd_time.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text)) <= 59)
            {
                setDndValueStat = dnd_time.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text);
                settingChanged = true;
                e.Handled = regex.IsMatch(e.Text);
            }

            else if (((TextBox)sender).Name == "offline_time" && !regex.IsMatch(e.Text) && Convert.ToInt32(offline_time.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text)) >= 1 && Convert.ToInt32(offline_time.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text)) <= 999)
            {
                setOfflineValueStat = offline_time.Text.Insert(((TextBox)e.Source).CaretIndex, e.Text);
                settingChanged = true;
                e.Handled = regex.IsMatch(e.Text);
            }

            else e.Handled = true;
        }

       
    }

}

