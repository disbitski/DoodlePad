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
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Ink;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Marketplace;
using System.Xml.Linq;
using Microsoft.Phone.Shell;


/**************************************************************
/      DoodlePad
/      Version 1.2 Full
/
/	   OS Support: 7.0, 7.5
/      Author: David Isbitski (DaveDev Productions)
/	   Blog: http://blogs.msdn.com/davedev
/      Twitter: http://twitter.com/theDaveDev
/      Web: http://about.me/davedev
/      Git: http://github.com/disbitski
/      Email: disbitski@hotmail.com
/
/      Special thanks to:
/
/           Cricket Font: http://www.fonts4free.net/cricket-font.html
/           WriteableBitMapEx: http://writeablebitmapex.codeplex.com/	
/
/      DISCLAIMER: 
/
/      Copyright 2012 DaveDev Productions
/
/      Licensed under the Apache License, Version 2.0 (the "License");
/      you may not use this file except in compliance with the License.
/      You may obtain a copy of the License at
/
/      http://www.apache.org/licenses/LICENSE-2.0
/
/      Unless required by applicable law or agreed to in writing, software
/      distributed under the License is distributed on an "AS IS" BASIS,
/      WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
/      See the License for the specific language governing permissions and
/      limitations under the License.
**************************************************************/

namespace DoodlePad
{
    public partial class MainPage : PhoneApplicationPage
    {
      
        CameraCaptureTask cameraTask;
        PhotoChooserTask photoTask;
        MarketplaceDetailTask marketTask;
        public InkCollector ink;
        Stack<Stroke> removedStrokes; 
        LicenseInformation licenseInfo;

        //Custom brushes
        private Color brushColor;
        private int brushSize;
        private bool customColor;
        private int customColorCount;
        private bool customBrush1 = false;
        private bool customBrush2 = false;
        private bool customBrush3 = false;
        private bool customBrush4 = false;
        private bool customBrush5 = false;
        private bool customBrush6 = false;


	
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            //Init
            removedStrokes=new Stack<Stroke>();
            licenseInfo = new LicenseInformation();
            
            //Only support Portrait mode for now
            SupportedOrientations = SupportedPageOrientation.Portrait;

            //Camera Event
            cameraTask = new CameraCaptureTask();
            cameraTask.Completed += new EventHandler<PhotoResult>(cameraTask_Completed);

            //Photo Event
            photoTask = new PhotoChooserTask();
            photoTask.Completed += new EventHandler<PhotoResult>(photoTask_Completed);
			
			//About Event
			spAboutOpen.Completed+=new System.EventHandler(spAboutOpen_Completed);

            //Set Up Ink
            App.inkStorage = inkSketch;
            App.inkStorage.Visibility = Visibility.Visible;
            if (App.inkStrokes.Strokes.Count > 0)
            {
                App.inkStorage.Strokes = App.inkStrokes.Strokes;
            }
            ink = new InkCollector(App.inkStorage);

            //Custom Color Handler
            colorPicker.ColorSelected += new SilverlightColorPicker.ColorPicker.ColorSelectedHandler(colorPicker_ColorSelected);
                        
            //Set default brush properties
            SetBrushDefaults();

            //Load any custom colors that have been created
            LoadCustomGlobalBrushes();
            
            spAbout.Visibility = Visibility.Collapsed;
            btnPurchase.Visibility=Visibility.Collapsed;
            imgEraser.Visibility = Visibility.Collapsed;
            spSettings.Visibility = Visibility.Collapsed;

          
            this.BackKeyPress += new EventHandler<System.ComponentModel.CancelEventArgs>(MainPage_BackKeyPress);

            if (App.customBG)
            {
                imgBG.Source = App.WBMP;
                App.imgTemp = imgBG;
            }

            //Dynamically create AppBar
            LoadAppBar();

        }

        void MainPage_BackKeyPress(object sender, System.ComponentModel.CancelEventArgs e)
        {
            //Check for Back Button - else exit
            if (spAbout.Visibility == Visibility.Visible)
            {
                spAbout.Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }
            else if (spSettings.Visibility == Visibility.Visible)
            {
                spSettings.Visibility = Visibility.Collapsed;
                e.Cancel = true;
            }
            else
            {
                var result = MessageBox.Show("Are you sure you want to exit?  You will lose all unsaved work.", "Exit DoodlePad", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    //Exit App
                }
                else
                {
                    e.Cancel = true;
                }
            }
          
           
        }

     
        //Photo Chooser Event
        void photoTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                ChangeBackgroundCamera(bmp);

            }
        }

        //Camera Chooser Event
        void cameraTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(e.ChosenPhoto);
                ChangeBackgroundCamera(bmp);
                
            }
        }
         
        //Default Brush and Ink Mode
        private void SetBrushDefaults()
        {
            inkSketch = App.inkStrokes;
            ink.DefaultDrawingAttributes = App.inkDA;
            if (App.BMP != null)
            {
                ChangeBackgroundCamera(App.BMP);
            }
            brushSize = App.BrushSize;
            brushColor = App.BrushColor;
            customColor = App.CustomColor;
            ink.DefaultDrawingAttributes = App.inkDA;


            ShowCurrentSelections(brushColor, brushSize);

          
            

        }

         //Change Background Picture to Camera Shot
        private void ChangeBackgroundCamera(BitmapImage bmp)
        {
            ImageBrush img = new ImageBrush();
            img.Stretch = Stretch.UniformToFill;
            img.ImageSource = bmp;
            gridBackground.Background = img;
            App.BMP = bmp;
            imgBG.Source = bmp;
            App.imgTemp = imgBG;
            App.inkStorage.Width = gridBackground.Width;
            App.inkStorage.Height = gridBackground.Height;

        }

        //Change global brush color to newly selected color
        public void ChangeBrushColor(Color color)
        {
            DrawingAttributes da = new DrawingAttributes();
            da.Height = ink.DefaultDrawingAttributes.Height;
            da.Width = ink.DefaultDrawingAttributes.Width;
            da.Color = color;
            da.OutlineColor = color;
            ink.DefaultDrawingAttributes = da;
            App.inkDA = da;

            if (customBrush1) App.CustomBrush1 = color;
            if (customBrush2) App.CustomBrush2 = color;
            if (customBrush3) App.CustomBrush3 = color;
            if (customBrush4) App.CustomBrush4 = color;
            if (customBrush5) App.CustomBrush5 = color;
            if (customBrush6) App.CustomBrush6 = color;
            
        }

        //Change global brush size to newly selected size
        public void ChangeBrushSize(int width, int height)
        {
            DrawingAttributes da = new DrawingAttributes();
            da.Color = ink.DefaultDrawingAttributes.Color;
            da.OutlineColor = ink.DefaultDrawingAttributes.OutlineColor;
            da.Height = height;
            da.Width = width;
            ink.DefaultDrawingAttributes = da;
            App.inkDA = da;
        }


        //Show the Brush Pallete
        public void SwapScreens()
        {
            if (spSettings.Visibility == Visibility.Collapsed)
            {
                spAbout.Visibility = Visibility.Collapsed;
                ShowCurrentSelections(ink.DefaultDrawingAttributes.Color, ink.DefaultDrawingAttributes.Height);
                spSettings.Visibility = Visibility.Visible;
                ink.Mode = InkMode.Ink;
            }
            else
            {
                App.BrushColor = brushColor;
                App.BrushSize = brushSize;
                App.CustomColor = customColor;
              
            }
            
        }

        

        //This will load our Color/Brush Pallete
        private void appbar_colors_Click(object sender, EventArgs e)
        {
            if (spSettings.Visibility == Visibility.Visible)
            {
                spSettings.Visibility = Visibility.Collapsed;
            }
            else
            {
                spSettings.Visibility = Visibility.Visible;

            }
        }

        //Camera Task will run if full version
        private void appbar_camera_Click(object sender, EventArgs e)
        {
            if (licenseInfo.IsTrial())
            {
                MessageBox.Show("Camera is unavailable in Trial Mode.");
            }
            else
            {
                cameraTask.Show();
            }
        }

        //Photochooser Task will run if full version
        private void appbar_photos_Click(object sender, EventArgs e)
        {
            if (licenseInfo.IsTrial())
            {
                MessageBox.Show("Photos are unavailable in Trial Mode.");
            }
            else
            {
                photoTask.Show();
            }
        }

        //User can save doodle if full version
        private void appbar_save_Click(object sender, EventArgs e)
        {

            if (licenseInfo.IsTrial())
            {
                MessageBox.Show("Saving is unavailable in Trial Mode.");
            }
            else
            {
                WriteableBitmap bitmap = new WriteableBitmap(LayoutRoot, null);
                var name = String.Format("DoodlePad_{0:yyyy-MM-dd_hh-mm-ss-tt}.jpg", DateTime.Now);
                try
                {
                    bitmap.SaveToMediaLibrary(name);
                    MessageBox.Show("Doodle successfully saved as '" + name + "'!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Sorry, unable to save your Doodle at this time. Make sure the phone is disconnected from your PC.");
                }
            }
        }

        //Show our About Page
        private void About_Click(object sender, EventArgs e)
        {
            if (spAbout.Visibility == Visibility.Collapsed)
            {
                spSettings.Visibility = Visibility.Collapsed;
				spAbout.Visibility = Visibility.Visible;
			    spAboutOpen.Begin();
               
                
            }
            else if(spAbout.Visibility == Visibility.Visible)
            {			
                spAbout.Visibility = Visibility.Collapsed;
            }

        }

        
        //Email Task if users wishes to contact us
        private void ContactUs_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();
            emailComposeTask.To = "disbitski@hotmail.com";
            emailComposeTask.Subject = "DoodlePad 1.1";
            emailComposeTask.Show();
        }


        //Clear current drawing
        private void Clear_All_Click(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Erase all of your Doodle?", "Erase All", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                App.inkStorage.Strokes.Clear();
            }
            
        }

        //Undo Last stroke
        private void Undo_Click(object sender, EventArgs e)
        {
            if (App.inkStorage.Strokes != null && App.inkStorage.Strokes.Count > 0)
            {
                removedStrokes.Push(App.inkStorage.Strokes.Last());
                App.inkStorage.Strokes.RemoveAt(App.inkStorage.Strokes.Count - 1);
            }  
 
        }

        //Redo Last removed stroke
        private void Redo_Click(object sender, EventArgs e)
        {
            if (removedStrokes != null && removedStrokes.Count > 0)
            {
                App.inkStorage.Strokes.Add(removedStrokes.Pop());
            }  
        }

          
        //Display purchase option for Trial Mode on About menu
        private void spAboutOpen_Completed(object sender, System.EventArgs e)
        {
            if (licenseInfo.IsTrial())
            {
                btnPurchase.Content = "Buy Full Version";
                btnPurchase.Visibility = Visibility.Visible;
                btnPurchaseShake.Begin();
            }
            else
            {
                btnPurchase.Content = "Leave a Review!";
                btnPurchase.Visibility = Visibility.Visible;
                btnPurchaseShake.Begin();
            }
        }

        //Purchase Task if user clicked on purchase button in trial mode
        private void btnPurchase_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (licenseInfo.IsTrial())
            {
                marketTask = new MarketplaceDetailTask();
                marketTask.Show();
            }
            else
            {

                MarketplaceReviewTask reviewTask = new MarketplaceReviewTask();
                reviewTask.Show();

            }
        }
        
    
        //Toggle Eraser
        private void Erase_Click(object sender, EventArgs e)
        {
            if (ink.Mode == InkMode.PointErase)
            {
                ink.Mode = InkMode.Ink;
                imgEraser.Visibility = Visibility.Collapsed;
            }
            else
            {
                
                DrawingAttributes da = new DrawingAttributes();
                da.Height = 15;
                da.Width = 15;
                da.Color = ink.DefaultDrawingAttributes.Color;
                da.OutlineColor = ink.DefaultDrawingAttributes.OutlineColor;
                ink.DefaultDrawingAttributes = da;
                ink.Mode = InkMode.PointErase;
                imgEraser.Visibility = Visibility.Visible;
            }
        }

        private void ResetBG_Click(object sender, EventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Reset Background to Notebook Paper?", "Reset Background", MessageBoxButton.OKCancel);

            if (result == MessageBoxResult.OK)
            {
                ImageBrush img = new ImageBrush();
                img.Stretch = Stretch.UniformToFill;
                img.ImageSource = new BitmapImage(new Uri(@"Assets\Paper.png", UriKind.Relative));
                gridBackground.Background = img;
                App.BMP = new BitmapImage(new Uri(@"Assets\Paper.png", UriKind.Relative));
                App.customBG = false;
                imgBG.Source = App.BMP;
                App.imgTemp = imgBG;
                     
            }
             
        }

 

        //Custom Color Selected
        private void rectCustomColor_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            //deprecated
        }

        private void ChangeColorFromCustomBrush(Brush brush)
        {

            Color newColor = new Color();
            SolidColorBrush oldBrush = (SolidColorBrush)brush;

            newColor.A = oldBrush.Color.A;
            newColor.R = oldBrush.Color.R;
            newColor.G = oldBrush.Color.G;
            newColor.B = oldBrush.Color.B;
            brushColor = newColor;
            ChangeBrushColor(newColor);
            customColor = true;
            
        }

        //Change selectecd color to custom color from colorpicker
        void colorPicker_ColorSelected(Color c)
        {
            Color newColor = new Color();
            newColor.A = c.A;
            newColor.R = c.R;
            newColor.G = c.G;
            newColor.B = c.B;

            SetCustomColor(new SolidColorBrush(newColor));
                        
            ChangeBrushColor(newColor);
            brushColor = newColor;
            customColor = true;
           
           
        }

        private void SetCustomColor(SolidColorBrush newColor)
        {
            bool filled=false;
            if (customBrush1)
            {
                rectCustom1.Fill = newColor;
                filled=true;
            }
            if (customBrush2)
            {
                rectCustom2.Fill = newColor;
                filled = true;
            }
            if (customBrush3)
            {
                rectCustom3.Fill = newColor;
                filled = true;
            }
            if (customBrush4)
            {
                rectCustom4.Fill = newColor;
                filled = true;
            }
            if (customBrush5)
            {
                rectCustom5.Fill = newColor;
                filled = true;
            }
            if (customBrush6)
            {
                rectCustom6.Fill = newColor;
                filled = true;
            }

        
        }

        //This will highlight the user selected brush colors and size in the settings screen
        private void ShowCurrentSelections(Color color, double height)
        {
            SolidColorBrush brush = new SolidColorBrush();
            SolidColorBrush defaultBrush = new SolidColorBrush();
            brush.Color = Colors.White;
            defaultBrush.Color = Colors.Black;

            //Brush Color Checks
            if (color == Colors.Blue)
            {
                rectBlue.Stroke = brush;
            }
            else
            {
                rectBlue.Stroke = defaultBrush;
            }
            if (color == Colors.Red)
            {
                rectRed.Stroke = brush;
            }
            else
            {
                rectRed.Stroke = defaultBrush;
            }
            if (color == Colors.Yellow)
            {
                rectYellow.Stroke = brush;
            }
            else
            {
                rectYellow.Stroke = defaultBrush;
            }
            if (color == Colors.Green)
            {
                rectGreen.Stroke = brush;
            }
            else
            {
                rectGreen.Stroke = defaultBrush;
            }
            if (color == Colors.White)
            {
                rectWhite.Stroke = brush;
            }
            else
            {
                rectWhite.Stroke = defaultBrush;
            }
            if (color == Colors.Black)
            {
                rectBlack.Stroke = brush;
            }
            else
            {
                rectBlack.Stroke = defaultBrush;
            }

            if (customColor)
            {
                //rectCustomColor.Stroke = brush;
                //rectCustomColor.Fill = new SolidColorBrush(App.inkDA.Color);
               
            }
            else
            {
                //rectCustomColor.Stroke = defaultBrush;
            }

            //Brush Size Checks
            if (height == 7)
            {
                brushSize7.Stroke = brush;
            }
            else
            {
                brushSize7.Stroke = defaultBrush;
            }
            if (height == 15)
            {
                brushSize15.Stroke = brush;
            }
            else
            {
                brushSize15.Stroke = defaultBrush;
            }
            if (height == 25)
            {
                brushSize25.Stroke = brush;
            }
            else
            {
                brushSize25.Stroke = defaultBrush;
            }
            if (height == 30)
            {
                brushSize30.Stroke = brush;
            }
            else
            {
                brushSize30.Stroke = defaultBrush;
            }

            if (height == 45)
            {
                brushSize45.Stroke = brush;
            }
            else
            {
                brushSize45.Stroke = defaultBrush;
            }

        }

        #region Settings Menu - Color, Brushes, Erase
        private void rectBlue_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            customColor = false;
            brushColor = Colors.Blue;
            ChangeBrushColor(Colors.Blue);
            rectBlue.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void rectRed_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            customColor = false;
            brushColor = Colors.Red;
            ChangeBrushColor(Colors.Red);
            rectRed.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void rectYellow_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            customColor = false;
            brushColor = Colors.Yellow;
            ChangeBrushColor(Colors.Yellow);
            rectYellow.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void rectGreen_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            customColor = false;
            brushColor = Colors.Green;
            ChangeBrushColor(Colors.Green);
            rectGreen.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void rectWhite_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {

            resetCustomSelections(); 
            customColor = false;
            brushColor = Colors.White;
            ChangeBrushColor(Colors.White);
            rectWhite.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void rectBlack_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            customColor = false;
            brushColor = Colors.Black;
            ChangeBrushColor(Colors.Black);
            rectBlack.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void brushSize7_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetBrushSelctions();
            brushSize = 7;
            ChangeBrushSize(7, 7);
            brushSize7.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void brushSize15_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetBrushSelctions();
            brushSize = 15;
            ChangeBrushSize(15, 15);
            brushSize15.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void brushSize25_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetBrushSelctions();
            brushSize = 25;
            ChangeBrushSize(25, 25);
            brushSize25.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void brushSize30_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetBrushSelctions();
            brushSize = 30;
            ChangeBrushSize(30, 30);
            brushSize30.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void brushSize35_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetBrushSelctions();
            brushSize = 35;
            ChangeBrushSize(35, 35);
            //brushstroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void brushSize45_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetBrushSelctions();
            brushSize = 45;
            ChangeBrushSize(45, 45);
            brushSize45.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void brushSize50_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetBrushSelctions();
            brushSize = 50;
            ChangeBrushSize(50, 50);
            //brushSize50.Stroke = new SolidColorBrush(Colors.White);
            SwapScreens();
        }

        private void rectCustom1_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            rectCustom1.Stroke = new SolidColorBrush(Colors.White);
            customBrush1 = true;
            customColor = true;
            
            string image=rectCustom1.Fill.ToString();
            if (image!= "System.Windows.Media.ImageBrush")
            {
                ChangeColorFromCustomBrush(rectCustom1.Fill);
                SolidColorBrush brush = (SolidColorBrush)rectCustom1.Fill;
                App.CustomBrush1 = brush.Color;
            }
                       
        }

        private void rectCustom2_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            rectCustom2.Stroke = new SolidColorBrush(Colors.White);
            customBrush2 = true;
            customColor = true;

            string image = rectCustom2.Fill.ToString();
            if (image != "System.Windows.Media.ImageBrush")
            {
                ChangeColorFromCustomBrush(rectCustom2.Fill);
                SolidColorBrush brush = (SolidColorBrush)rectCustom2.Fill;
                App.CustomBrush2 = brush.Color;
            }
        }


        private void rectCustom3_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            rectCustom3.Stroke = new SolidColorBrush(Colors.White);
            customBrush3 = true;
            customColor = true;

            string image = rectCustom3.Fill.ToString();
            if (image != "System.Windows.Media.ImageBrush")
            {
                ChangeColorFromCustomBrush(rectCustom3.Fill);
                SolidColorBrush brush = (SolidColorBrush)rectCustom3.Fill;
                App.CustomBrush3 = brush.Color;
            }
        }


        private void rectCustom4_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            rectCustom4.Stroke = new SolidColorBrush(Colors.White);
            customBrush4 = true;
            customColor = true;

            string image = rectCustom4.Fill.ToString();
            if (image != "System.Windows.Media.ImageBrush")
            {
                ChangeColorFromCustomBrush(rectCustom4.Fill);
                SolidColorBrush brush = (SolidColorBrush)rectCustom4.Fill;
                App.CustomBrush4 = brush.Color;
            }
        }

        private void rectCustom5_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            rectCustom5.Stroke = new SolidColorBrush(Colors.White);
            customBrush5 = true;
            customColor = true;

            string image = rectCustom5.Fill.ToString();
            if (image != "System.Windows.Media.ImageBrush")
            {
                ChangeColorFromCustomBrush(rectCustom5.Fill);
                SolidColorBrush brush = (SolidColorBrush)rectCustom5.Fill;
                App.CustomBrush5 = brush.Color;
            }
        }

        private void rectCustom6_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            resetCustomSelections();
            rectCustom6.Stroke = new SolidColorBrush(Colors.White);
            customBrush6 = true;
            customColor = true;

            string image = rectCustom6.Fill.ToString();
            if (image != "System.Windows.Media.ImageBrush")
            {
                ChangeColorFromCustomBrush(rectCustom6.Fill);
                SolidColorBrush brush = (SolidColorBrush)rectCustom6.Fill;
                App.CustomBrush6 = brush.Color;
            }
        }

        #endregion

        private void resetBrushSelctions()
        {
            brushSize7.Stroke = new SolidColorBrush(Colors.Black);
            brushSize15.Stroke = new SolidColorBrush(Colors.Black);
            brushSize25.Stroke = new SolidColorBrush(Colors.Black);
            brushSize30.Stroke = new SolidColorBrush(Colors.Black);
            brushSize45.Stroke = new SolidColorBrush(Colors.Black);
       }
        
        private void resetCustomSelections()
        {
            customBrush1 = false;
            customBrush2 = false;
            customBrush3 = false;
            customBrush4 = false;
            customBrush5 = false;
            customBrush6 = false;

            rectCustom1.Stroke = new SolidColorBrush(Colors.Black);
            rectCustom2.Stroke = new SolidColorBrush(Colors.Black);
            rectCustom3.Stroke = new SolidColorBrush(Colors.Black);
            rectCustom4.Stroke = new SolidColorBrush(Colors.Black);
            rectCustom5.Stroke = new SolidColorBrush(Colors.Black);
            rectCustom6.Stroke = new SolidColorBrush(Colors.Black);

            rectBlack.Stroke = new SolidColorBrush(Colors.Black);
            rectBlue.Stroke = new SolidColorBrush(Colors.Black);
            rectRed.Stroke = new SolidColorBrush(Colors.Black);
            rectWhite.Stroke = new SolidColorBrush(Colors.Black);
            rectYellow.Stroke = new SolidColorBrush(Colors.Black);
            rectGreen.Stroke = new SolidColorBrush(Colors.Black);

        }

        private void LoadCustomGlobalBrushes()
        {
            string image;

            if (App.CustomBrush1 != null)
            {
                image = App.CustomBrush1.ToString();
                if (image != "System.Windows.Media.ImageBrush" && image !="#00000000")
                {
                    rectCustom1.Fill=new SolidColorBrush(App.CustomBrush1);
                }
            }

            if (App.CustomBrush2 != null)
            {
                image = App.CustomBrush2.ToString();
                if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
                {
                    rectCustom2.Fill = new SolidColorBrush(App.CustomBrush2);
                }
            }

            if (App.CustomBrush3 != null)
            {
                image = App.CustomBrush3.ToString();
                if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
                {
                    rectCustom3.Fill = new SolidColorBrush(App.CustomBrush3);
                }
            }

            if (App.CustomBrush4 != null)
            {
                image = App.CustomBrush4.ToString();
                if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
                {
                    rectCustom4.Fill = new SolidColorBrush(App.CustomBrush4);
                }
            }

            if (App.CustomBrush5 != null)
            {
                image = App.CustomBrush5.ToString();
                if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
                {
                    rectCustom5.Fill = new SolidColorBrush(App.CustomBrush5);
                }
            }

            if (App.CustomBrush6 != null)
            {
                image = App.CustomBrush6.ToString();
                if (image != "System.Windows.Media.ImageBrush" && image != "#00000000")
                {
                    rectCustom6.Fill = new SolidColorBrush(App.CustomBrush6);
                }
            }


        }

        private void LoadAppBar()
        {
            ApplicationBar = new ApplicationBar();

            ApplicationBarIconButton appbar_button1 = new ApplicationBarIconButton(new Uri("/Assets/eraser.png", UriKind.Relative));
            appbar_button1.Text = "Erase Mode";
            appbar_button1.Click+=new EventHandler(Erase_Click);

            ApplicationBarIconButton appbar_button2 = new ApplicationBarIconButton(new Uri("/Assets/undo.png", UriKind.Relative));
            appbar_button2.Text = "Undo";
            appbar_button2.Click += new EventHandler(Undo_Click);

            ApplicationBarIconButton appbar_button3 = new ApplicationBarIconButton(new Uri("/Assets/redo.png", UriKind.Relative));
            appbar_button3.Text = "Redo";
            appbar_button3.Click += new EventHandler(Redo_Click);

            ApplicationBarIconButton appbar_button4 = new ApplicationBarIconButton(new Uri("/Assets/appbar.colors.png", UriKind.Relative));
            appbar_button4.Text = "Colors";
            appbar_button4.Click += new EventHandler(appbar_colors_Click);

            ApplicationBarMenuItem menuItem1 = new ApplicationBarMenuItem();
            menuItem1.Text = "About";
            menuItem1.Click += new EventHandler(About_Click);

            ApplicationBarMenuItem menuItemFree = new ApplicationBarMenuItem();
            menuItemFree.Text = "Get FREE version of DoodlePad";
            menuItemFree.Click += new EventHandler(menuItemFree_Click);
            
            ApplicationBarMenuItem menuItem2 = new ApplicationBarMenuItem();
            menuItem2.Text = "Erase All";
            menuItem2.Click += new EventHandler(Clear_All_Click);

            ApplicationBarMenuItem menuItem3 = new ApplicationBarMenuItem();
            menuItem3.Text = "Reset Background";
            menuItem3.Click += new EventHandler(ResetBG_Click);

            ApplicationBarMenuItem menuItem4 = new ApplicationBarMenuItem();
            menuItem4.Text = "Use Camera for Background";
            menuItem4.Click += new EventHandler(appbar_camera_Click);

            ApplicationBarMenuItem menuItem5 = new ApplicationBarMenuItem();
            menuItem5.Text = "Choose a Background";
            menuItem5.Click += new EventHandler(appbar_photos_Click);

            ApplicationBarMenuItem menuItem6 = new ApplicationBarMenuItem();
            menuItem6.Text = "Save Doodle to Pictures Hub";
            menuItem6.Click += new EventHandler(appbar_save_Click);


            ApplicationBar.Buttons.Add(appbar_button1);
            ApplicationBar.Buttons.Add(appbar_button2);
            ApplicationBar.Buttons.Add(appbar_button3);
            ApplicationBar.Buttons.Add(appbar_button4);
            ApplicationBar.MenuItems.Add(menuItem1);
            if (licenseInfo.IsTrial()) ApplicationBar.MenuItems.Add(menuItemFree);
            ApplicationBar.MenuItems.Add(menuItem2);
            ApplicationBar.MenuItems.Add(menuItem3);
            ApplicationBar.MenuItems.Add(menuItem4);
            ApplicationBar.MenuItems.Add(menuItem5);
            ApplicationBar.MenuItems.Add(menuItem6);
        }

        void menuItemFree_Click(object sender, EventArgs e)
        {
            MarketplaceSearchTask searchTask = new MarketplaceSearchTask();
            searchTask.SearchTerms = "DoodlePad Free";
            searchTask.Show();
        }
        
    }
}
