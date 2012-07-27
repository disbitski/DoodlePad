using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

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

namespace SilverlightColorPicker
{
    public partial class ColorPicker : UserControl
    {
        ColorSpace m_colorSpace;
        bool m_sliderMouseDown;
       // bool m_isMouseCaptured;
        bool m_sampleMouseDown;
        float m_selectedHue;
        int m_sampleX;
        int m_sampleY;
        private Color m_selectedColor;
        public delegate void ColorSelectedHandler(Color c);
        public event ColorSelectedHandler ColorSelected;

        public ColorPicker()
        {
            InitializeComponent();
            rectHueMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectHueMonitor_MouseLeftButtonDown);
            rectHueMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectHueMonitor_MouseLeftButtonUp);
            rectHueMonitor.MouseLeave += new MouseEventHandler(rectHueMonitor_MouseLeave);
            rectHueMonitor.MouseMove += new MouseEventHandler(rectHueMonitor_MouseMove);

            rectSampleMonitor.MouseLeftButtonDown += new MouseButtonEventHandler(rectSampleMonitor_MouseLeftButtonDown);
            rectSampleMonitor.MouseLeftButtonUp += new MouseButtonEventHandler(rectSampleMonitor_MouseLeftButtonUp);
            rectSampleMonitor.MouseLeave += new MouseEventHandler(rectSampleMonitor_MouseLeave);
            rectSampleMonitor.MouseMove += new MouseEventHandler(rectSampleMonitor_MouseMove);

            m_colorSpace = new ColorSpace();
            m_selectedHue = 0;
            m_sampleX = (int)rectSampleMonitor.Width/2;
            m_sampleY = (int)rectSampleMonitor.Height/2;
            UpdateSample(m_sampleX, m_sampleY);
        }

        void rectHueMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            //e.Handled = true;
            m_sliderMouseDown = true;
            int yPos = (int)e.GetPosition((UIElement)sender).Y;
            UpdateSelection(yPos);
           // m_isMouseCaptured = CaptureMouse();
        }

        void rectHueMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine("Mouse Up");
            //e.Handled = true;
            m_sliderMouseDown = false;
         //   ReleaseMouseCapture();
        }

        void rectHueMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_sliderMouseDown)
            {
                int yPos = (int)e.GetPosition((UIElement)sender).Y;
                UpdateSelection(yPos);
            }
        }

        void rectHueMonitor_MouseLeave(object sender, EventArgs e)
        {
            m_sliderMouseDown = false;
        }

        void rectSampleMonitor_MouseLeftButtonDown(object sender, MouseEventArgs e)
        {
            m_sampleMouseDown = true;
            Point pos = e.GetPosition((UIElement)sender);
            m_sampleX = (int)pos.X;
            m_sampleY = (int)pos.Y;
            UpdateSample(m_sampleX, m_sampleY);
        }

        void rectSampleMonitor_MouseLeftButtonUp(object sender, MouseEventArgs e)
        {
            m_sampleMouseDown = false;
        }

        void rectSampleMonitor_MouseMove(object sender, MouseEventArgs e)
        {
            if (m_sampleMouseDown)
            {
                Point pos = e.GetPosition((UIElement)sender);
                m_sampleX = (int)pos.X;
                m_sampleY = (int)pos.Y;
                UpdateSample(m_sampleX, m_sampleY);
            }
        }

        void rectSampleMonitor_MouseLeave(object sender, EventArgs e)
        {
            m_sampleMouseDown = false;
        }

        private void UpdateSample(int xPos, int yPos)
        {

            SampleSelector.SetValue(Canvas.LeftProperty, xPos - (SampleSelector.Height / 2));
            SampleSelector.SetValue(Canvas.TopProperty, yPos - (SampleSelector.Height / 2));

            float yComponent = 1 - (float)(yPos / rectSample.Height);
            float xComponent = (float)(xPos / rectSample.Width);

            m_selectedColor = m_colorSpace.ConvertHsvToRgb((float)m_selectedHue, xComponent, yComponent);
            SelectedColor.Fill = new SolidColorBrush(m_selectedColor);
            HexValue.Text = m_colorSpace.GetHexCode(m_selectedColor);

            if (ColorSelected != null)
                ColorSelected(m_selectedColor);
        }

        private void UpdateSelection(int yPos)
        {
            int huePos = (int)(yPos / rectHueMonitor.Height * 255);
            int gradientStops = 6;
            Color c = m_colorSpace.GetColorFromPosition(huePos * gradientStops);
            rectSample.Fill = new SolidColorBrush(c);
            HueSelector.SetValue(Canvas.TopProperty, yPos - (HueSelector.Height / 2));
            m_selectedHue = (float)(yPos / rectHueMonitor.Height) * 360;
            UpdateSample(m_sampleX, m_sampleY);
        }
    }
}
