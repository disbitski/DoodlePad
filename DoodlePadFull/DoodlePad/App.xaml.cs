using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.IO.IsolatedStorage;
using System.Xml.Linq;
using System.Windows.Ink;
using System.Xml;
using System.Windows.Media.Imaging;
using System.IO;


/**************************************************************
/      DoodlePad
/      Version 1.2 Full
/      Last Mod: January 17, 2010 
/	  
/      Author: David Isbitski (DaveDev Productions)
/	   Blog: http://blogs.msdn.com/davedev
/      Twitter: http://twitter.com/theDaveDev
/      Web: http://about.me/davedev
/      Git: http://github.com/disbitski
/      Email: disbitski@hotmail.com
/
**************************************************************/
namespace DoodlePad
{
    public partial class App : Application
    {

        public static InkPresenter inkStorage;
        public static InkPresenter inkStrokes=new InkPresenter();
        public static DrawingAttributes inkDA;
        public static bool customColorExists=false;
        public static BitmapImage BMP;
        public static Color BrushColor=Colors.Black;
        public static int BrushSize=15;
        public static bool CustomColor;
        public const string IMGID= "SavedImage.bmp";
        public static WriteableBitmap WBMP;
        public static bool customBG=false;
        public static Image imgTemp;
        public static Color CustomBrush1;
        public static Color CustomBrush2;
        public static Color CustomBrush3;
        public static Color CustomBrush4;
        public static Color CustomBrush5;
        public static Color CustomBrush6;
        

        // Easy access to the root frame
        public PhoneApplicationFrame RootFrame { get; private set; }

    
        // Constructor
        public App()
        {
            // Global handler for uncaught exceptions. 
            // Note that exceptions thrown by ApplicationBarItem.Click will not get caught here.
            UnhandledException += Application_UnhandledException;

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();

            inkDA = new DrawingAttributes();
            inkDA.Color = Colors.Black;
            inkDA.OutlineColor = Colors.Black;
            inkDA.Width = inkDA.Height = 15d;

            
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {

            BMP = new BitmapImage(new Uri(@"Assets\Paper.png", UriKind.Relative));
            imgTemp = new Image();
            imgTemp.Source = BMP;

            IsolatedStorageSettings loadSettings = IsolatedStorageSettings.ApplicationSettings;

            try
            {
                Color customBrush;

                try
                {
                    loadSettings.TryGetValue("CustomBrush1", out customBrush);
                    if (customBrush != null) CustomBrush1 = customBrush;
                }
                catch
                {
                    //
                }

                try
                {
                    loadSettings.TryGetValue("CustomBrush2", out customBrush);
                    if (customBrush != null) CustomBrush2 = customBrush;
                }
                catch
                {
                    //
                }

                try
                {
                    loadSettings.TryGetValue("CustomBrush3", out customBrush);
                    if (customBrush != null) CustomBrush3 = customBrush;
                }
                catch
                {
                    //
                }

                try
                {
                    loadSettings.TryGetValue("CustomBrush4", out customBrush);
                    if (customBrush != null) CustomBrush4 = customBrush;
                }
                catch
                {
                    //
                }

                try
                {
                    loadSettings.TryGetValue("CustomBrush5", out customBrush);
                    if (customBrush != null) CustomBrush5 = customBrush;
                }
                catch
                {
                    //
                }

                try
                {
                    loadSettings.TryGetValue("CustomBrush6", out customBrush);
                    if (customBrush != null) CustomBrush5 = customBrush;
                }
                catch
                {
                    //
                }
            }
            catch
            {
                //
            }
            
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            if (PhoneApplicationService.Current.State.ContainsKey("inkXML"))
            {
                XElement inkXML;
                inkStrokes = new InkPresenter();
                inkXML= PhoneApplicationService.Current.State["inkXML"] as XElement;
                if (inkXML != null)
                {
                    inkStrokes.Strokes = XMLHelpers.XMLtoSrokes(inkXML);
                }
                PhoneApplicationService.Current.State.Remove("inkXML");
             }

            if (PhoneApplicationService.Current.State.ContainsKey("inkColor"))
            {
                inkDA.Color = (Color)PhoneApplicationService.Current.State["inkColor"];
                PhoneApplicationService.Current.State.Remove("inkColor");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("inkOutlineColor"))
            {
                inkDA.OutlineColor = (Color)PhoneApplicationService.Current.State["inkOutlineColor"];
                PhoneApplicationService.Current.State.Remove("inkOutlineColor");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("inkWidth"))
            {
                inkDA.Width = (double)PhoneApplicationService.Current.State["inkWidth"];
                PhoneApplicationService.Current.State.Remove("inkWidth");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("inkHeight"))
            {
                inkDA.Height = (double)PhoneApplicationService.Current.State["inkHeight"];
                PhoneApplicationService.Current.State.Remove("inkHeight");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("CustomColor"))
            {
                CustomColor = (bool)PhoneApplicationService.Current.State["CustomColor"];
                PhoneApplicationService.Current.State.Remove("CustomColor");
            }
            
            if (PhoneApplicationService.Current.State.ContainsKey("CustomBrush1"))
            {
                CustomBrush1 = (Color)PhoneApplicationService.Current.State["CustomBrush1"];
                PhoneApplicationService.Current.State.Remove("CustomBrush1");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("CustomBrush2"))
            {
                CustomBrush2 = (Color)PhoneApplicationService.Current.State["CustomBrush2"];
                PhoneApplicationService.Current.State.Remove("CustomBrush2");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("CustomBrush3"))
            {
                CustomBrush3 = (Color)PhoneApplicationService.Current.State["CustomBrush3"];
                PhoneApplicationService.Current.State.Remove("CustomBrush3");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("CustomBrush4"))
            {
                CustomBrush4 = (Color)PhoneApplicationService.Current.State["CustomBrush4"];
                PhoneApplicationService.Current.State.Remove("CustomBrush4");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("CustomBrush5"))
            {
                CustomBrush5 = (Color)PhoneApplicationService.Current.State["CustomBrush5"];
                PhoneApplicationService.Current.State.Remove("CustomBrush5");
            }

            if (PhoneApplicationService.Current.State.ContainsKey("CustomBrush6"))
            {
                CustomBrush6 = (Color)PhoneApplicationService.Current.State["CustomBrush6"];
                PhoneApplicationService.Current.State.Remove("CustomBrush6");
            }

            try
            {
                WBMP = IsolatedStorageHelper.GetImage(IsolatedStorageHelper.LoadIfExists(IMGID));
                customBG=true;
            }
            catch
            {
                customBG=false;
            }

            BrushSize = (int)inkDA.Width;
            BrushColor = inkDA.Color;
          
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            PhoneApplicationService.Current.State.Add("inkXML", XMLHelpers.StrokestoXML(inkStorage.Strokes) as XElement);
            PhoneApplicationService.Current.State.Add("inkColor", inkDA.Color);
            PhoneApplicationService.Current.State.Add("inkOutlineColor", inkDA.OutlineColor);
            PhoneApplicationService.Current.State.Add("inkWidth", inkDA.Width);
            PhoneApplicationService.Current.State.Add("inkHeight", inkDA.Height);
            
            String image = CustomBrush1.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                PhoneApplicationService.Current.State.Add("CustomBrush1", CustomBrush1);
            }

            image = CustomBrush2.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                PhoneApplicationService.Current.State.Add("CustomBrush2", CustomBrush2);
            }

            image = CustomBrush3.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                PhoneApplicationService.Current.State.Add("CustomBrush3", CustomBrush3);
            }

            image = CustomBrush4.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                PhoneApplicationService.Current.State.Add("CustomBrush4", CustomBrush4);
            }

            image = CustomBrush5.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                PhoneApplicationService.Current.State.Add("CustomBrush5", CustomBrush5);
            }

            image = CustomBrush6.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                PhoneApplicationService.Current.State.Add("CustomBrush6", CustomBrush6);
            }
            
            PhoneApplicationService.Current.State.Add("CustomColor", CustomColor);
           // IsolatedStorageHelper.SaveToDisk(IsolatedStorageHelper.GetSaveBuffer(new WriteableBitmap(BMP)), IMGID);
            WriteableBitmap newWBMP = new WriteableBitmap(imgTemp, null);
            IsolatedStorageHelper.SaveToDisk(IsolatedStorageHelper.GetSaveBuffer(newWBMP), IMGID);

           
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            
            String image = CustomBrush1.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                if(settings.Contains("CustomBrush1"))
                {
                    settings["CustomBrush1"]=CustomBrush1;
                }
                else
                {
                    settings.Add("CustomBrush1", CustomBrush1);
                }
            }

            
            image = CustomBrush2.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                if (settings.Contains("CustomBrush2"))
                {
                    settings["CustomBrush2"] = CustomBrush2;
                }
                else
                {
                    settings.Add("CustomBrush2", CustomBrush2);
                }
            }

            image = CustomBrush3.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                if (settings.Contains("CustomBrush3"))
                {
                    settings["CustomBrush3"] = CustomBrush3;
                }
                else
                {
                    settings.Add("CustomBrush3", CustomBrush3);
                }
            }

            image = CustomBrush4.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                if (settings.Contains("CustomBrush4"))
                {
                    settings["CustomBrush4"] = CustomBrush4;
                }
                else
                {
                    settings.Add("CustomBrush4", CustomBrush4);
                }
            }

            image = CustomBrush5.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                if (settings.Contains("CustomBrush5"))
                {
                    settings["CustomBrush5"] = CustomBrush5;
                }
                else
                {
                    settings.Add("CustomBrush5", CustomBrush5);
                }
            }
            
            image = CustomBrush6.ToString();
            if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
            {
                if (settings.Contains("CustomBrush6"))
                {
                    settings["CustomBrush6"] = CustomBrush6;
                }
                else
                {
                    settings.Add("CustomBrush6", CustomBrush6);
                }
            }


            settings.Save();
            
        }

        // Code to execute if a navigation fails
        void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();

                if (e.Handled == true)
                {
                    //
                }
            }

            try
            {
               //Handle Errors
              
            }
            catch(Exception ex)
            {
                if (ex.Message == "AppExit")
                {
                    //Exit Gracefuly
                }
            }
        }

      
        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}
