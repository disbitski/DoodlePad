using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Xml;


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
    public enum InkMode
    {
        Ink,
        StrokeErase,
        PointErase
    }


    public class StrokeSegment
    {
        public Stroke Stroke
        {
            get { return _stroke; }
            set { _stroke = value; }
        }

        public int BeginIndex;
        public int EndIndex;

        private Stroke _stroke = null;
    }

   
    public class InkCollector : IDisposable
    {
        public InkCollector(InkPresenter presenter)
        {
            _presenter = presenter;
            _presenter.Cursor = Cursors.Stylus;
            _presenter.MouseLeftButtonDown += new MouseButtonEventHandler(_presenter_MouseLeftButtonDown);
            _presenter.MouseMove += new MouseEventHandler(_presenter_MouseMove);
            _presenter.MouseLeftButtonUp += new MouseButtonEventHandler(_presenter_MouseLeftButtonUp);
        }

        void _presenter_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _presenter.CaptureMouse();
            if (_mode == InkMode.Ink)
            {
                _stroke = new Stroke(e.StylusDevice.GetStylusPoints(_presenter));
                _stroke.DrawingAttributes = _drawingAttributes;
                _presenter.Strokes.Add(_stroke);
                //App.inkStorage.Strokes.Add(_stroke);
            }

            //Collect the stylus points and store them in a StylusPointCollection object.
            if (_mode == InkMode.StrokeErase)
            {
                _erasePoints = e.StylusDevice.GetStylusPoints(_presenter);
            }
            if (_mode == InkMode.PointErase)
            {
                //Collect the stylus points and store them in a StylusPointCollection object.
                StylusPointCollection pointErasePoints = e.StylusDevice.GetStylusPoints(_presenter);

                //Store the last stylus point in this stylus point collection separately. Storing the last 
                //stylus point ensures that the point at which the mouse left button down event was called is 
                //also processed in the subsequent mouse move handler.
                _lastPoint = pointErasePoints[pointErasePoints.Count-1];
            }
        }

        void _presenter_MouseMove(object sender, MouseEventArgs e)
        {
            if (_mode == InkMode.Ink && _stroke != null)
            {
                _stroke.StylusPoints.Add(e.StylusDevice.GetStylusPoints(_presenter));
            }
            if (_mode == InkMode.StrokeErase && _erasePoints != null)
            {

                //Add the stylus points to the StylusPointCollection object.
                _erasePoints.Add(e.StylusDevice.GetStylusPoints(_presenter));

                // Compare the collected stylus points with the stroke collection of the ink presenter.
                StrokeCollection hitStrokes = _presenter.Strokes.HitTest(_erasePoints);
                if (hitStrokes.Count > 0)
                {
                    foreach (Stroke hitStroke in hitStrokes)
                    {
                        //Delete the strokes that intersect with the collected stylus points.
                        _presenter.Strokes.Remove(hitStroke);
                        //App.inkStorage.Strokes.Remove(hitStroke);
                    }
                }
            }
            if (_mode == InkMode.PointErase && _lastPoint != null)
            {
                StylusPointCollection pointErasePoints = e.StylusDevice.GetStylusPoints(_presenter);
                pointErasePoints.Insert(0, _lastPoint.Value);
                //Compare collected stylus points with the ink presenter strokes and store the intersecting strokes.
                StrokeCollection hitStrokes = _presenter.Strokes.HitTest(pointErasePoints);
                if (hitStrokes.Count > 0)
                {
                    foreach (Stroke hitStroke in hitStrokes)
                    {

                        ////For each intersecting stroke, split the stroke into two while removing the intersecting points.
                        ProcessPointErase(hitStroke, pointErasePoints);
                    }
                }
                _lastPoint = pointErasePoints[pointErasePoints.Count - 1];
            }
        }

        void _presenter_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _presenter.ReleaseMouseCapture();
            if (_mode == InkMode.Ink && _stroke != null)
            {
                _stroke.StylusPoints.Add(e.StylusDevice.GetStylusPoints(_presenter));
            }
            _stroke = null;
            _erasePoints = null;
            _lastPoint = null;
        }

        void ProcessPointErase(Stroke stroke, StylusPointCollection pointErasePoints)
        {
            Stroke splitStroke1, splitStroke2, hitTestStroke;            

            // Determine first split stroke.
            splitStroke1 = new Stroke();
            hitTestStroke = new Stroke();
            hitTestStroke.StylusPoints.Add(stroke.StylusPoints);
            hitTestStroke.DrawingAttributes = stroke.DrawingAttributes;

            //Iterate through the stroke from index 0 and add each stylus point to splitstroke1 until 
            //a stylus point that intersects with the input stylus point collection is reached.
            while (true)
            {
                StylusPoint sp = hitTestStroke.StylusPoints[0];
                hitTestStroke.StylusPoints.RemoveAt(0);
                if (!hitTestStroke.HitTest(pointErasePoints)) break;
                splitStroke1.StylusPoints.Add(sp);
            }

            //Determine second split stroke.
            splitStroke2 = new Stroke();
            hitTestStroke = new Stroke();
            hitTestStroke.StylusPoints.Add(stroke.StylusPoints);
            hitTestStroke.DrawingAttributes = stroke.DrawingAttributes;
            while (true)
            {
                StylusPoint sp = hitTestStroke.StylusPoints[hitTestStroke.StylusPoints.Count - 1];
                hitTestStroke.StylusPoints.RemoveAt(hitTestStroke.StylusPoints.Count - 1);
                if (!hitTestStroke.HitTest(pointErasePoints)) break;
                splitStroke2.StylusPoints.Insert(0, sp);
            }

            // Replace stroke with splitstroke1 and splitstroke2.
            if (splitStroke1.StylusPoints.Count > 1)
            {
                splitStroke1.DrawingAttributes = stroke.DrawingAttributes;
                _presenter.Strokes.Add(splitStroke1);
                //App.inkStorage.Strokes.Add(splitStroke1);
            }
            if (splitStroke2.StylusPoints.Count > 1)
            {
                splitStroke2.DrawingAttributes = stroke.DrawingAttributes;
                _presenter.Strokes.Add(splitStroke2);
                 //App.inkStorage.Strokes.Add(splitStroke2);
            }
            _presenter.Strokes.Remove(stroke);
            // App.inkStorage.Strokes.Remove(stroke);
        }

        public InkMode Mode
        {
            get { return _mode; }
            set
            {
                _mode = value;
                switch (_mode)
                {
                    case InkMode.Ink:
                        _presenter.Cursor = Cursors.Stylus;
                        break;
                    case InkMode.StrokeErase:
                        _presenter.Cursor = Cursors.Eraser;
                        break;
                    case InkMode.PointErase:
                        _presenter.Cursor = Cursors.Hand;
                        break;
                }
            }
        }

        public DrawingAttributes DefaultDrawingAttributes
        {
            get { return _drawingAttributes; }
            set { _drawingAttributes = value; }
        }

        public void Dispose()
        {
            _presenter.MouseLeftButtonDown -= new MouseButtonEventHandler(_presenter_MouseLeftButtonDown);
            _presenter.MouseMove -= new MouseEventHandler(_presenter_MouseMove);
            _presenter.MouseLeftButtonUp -= new MouseButtonEventHandler(_presenter_MouseLeftButtonUp);
            _presenter = null;
        }

        private InkPresenter _presenter = null;
        private InkMode _mode = InkMode.Ink;
        private Stroke _stroke = null;
        private Nullable<StylusPoint> _lastPoint = null;
        private StylusPointCollection _erasePoints = null;
        private DrawingAttributes _drawingAttributes = new DrawingAttributes();
        private CultureInfo invCult = CultureInfo.InvariantCulture;
    }
}
