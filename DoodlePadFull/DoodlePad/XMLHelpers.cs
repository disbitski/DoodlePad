using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Linq;
using System.Xml.Linq;


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
    //Handle serialization of Ink Propeties to phone storage
    public class XMLHelpers
    {
        public static XElement StrokestoXML(StrokeCollection mystrokes)
        {
            string xmlnsString = "http://schemas.microsoft.com/client/2007";

            XNamespace xmlns = xmlnsString;
            XElement XMLStrokes = new XElement(xmlns + "StrokeCollection",
                new XAttribute("xmlns", xmlnsString));

            //create stroke, then add to collection element      
            XElement mystroke;
            foreach (Stroke s in mystrokes)
            {
                mystroke = new XElement(xmlns + "Stroke",
                  new XElement(xmlns + "Stroke.DrawingAttributes",
                    new XElement(xmlns + "DrawingAttributes",
                       new XElement("Color",
                                new XAttribute("A", s.DrawingAttributes.Color.A),
                                new XAttribute("R", s.DrawingAttributes.Color.R),
                                new XAttribute("G", s.DrawingAttributes.Color.G),
                                new XAttribute("B", s.DrawingAttributes.Color.B)
                            ),
                       new XElement("OutlineColor",
                                new XAttribute("A", s.DrawingAttributes.OutlineColor.A),
                                new XAttribute("R", s.DrawingAttributes.OutlineColor.R),
                                new XAttribute("G", s.DrawingAttributes.OutlineColor.G),
                                new XAttribute("B", s.DrawingAttributes.OutlineColor.B)
                            ),
                       new XAttribute("Width", s.DrawingAttributes.Width),
                       new XAttribute("Height", s.DrawingAttributes.Height))));

                //create points separately then add to mystroke XElement
                XElement myPoints = new XElement(xmlns + "Stroke.StylusPoints");
                foreach (StylusPoint sp in s.StylusPoints)
                {
                    XElement mypoint = new XElement(xmlns + "StylusPoint",
                      new XAttribute("X", sp.X.ToString()),
                      new XAttribute("Y", sp.Y.ToString()));
                    //add the new point to the points collection of the stroke
                    myPoints.Add(mypoint);
                }
                //add the new points collection to the stroke
                mystroke.Add(myPoints);
                //add the stroke to the collection
                XMLStrokes.Add(mystroke);
            }
            return XMLStrokes;
        }

      

        public static StrokeCollection XMLtoSrokes(XElement xml)
        {
            var xmlElem = xml;
            XNamespace xmlns = xmlElem.GetDefaultNamespace();
            StrokeCollection objStrokes = new StrokeCollection();
            double _width=7;
            double _height=7;
            byte _colorA=255;
            byte _outerColorA=255;
            byte _colorB=0;
            byte _colorG=0;
            byte _colorR=0;
            byte _outerColorB=0;
            byte _outerColorG=0;
            byte _outerColorR=0;

            
            //Query the XAML to extract the Strokes
            var strokes = from s in xmlElem.Descendants(xmlns+ "Stroke") select s;
            foreach (XElement strokeNodeElement in strokes)
            {
                //query the stroke to extract the drawingattributes
           
                var da = from d
                   in strokeNodeElement.Descendants(xmlns + "DrawingAttributes")
                             select d;
                foreach (XElement daElement in da)
                {
                    //Grab Brush Sizes
                    _width=Convert.ToDouble(daElement.Attribute("Width").Value);
                    _height = Convert.ToDouble(daElement.Attribute("Height").Value);

                    //Grab Brush Colors
                    var colors = from c
                        in da.Descendants("Color")
                                 select c;
                    foreach (XElement colorElement in colors)
                    {
                        _colorA = Convert.ToByte(colorElement.Attribute("A").Value);
                        _colorR = Convert.ToByte(colorElement.Attribute("R").Value);
                        _colorG = Convert.ToByte(colorElement.Attribute("G").Value);
                        _colorB = Convert.ToByte(colorElement.Attribute("B").Value);
                    }

                    //Grab Outline Brush Colors
                    var outlineColors = from c
                       in da.Descendants("OutlineColor")
                                 select c;
                    foreach (XElement colorElement in outlineColors)
                    {
                        _outerColorA = Convert.ToByte(colorElement.Attribute("A").Value);
                        _outerColorR = Convert.ToByte(colorElement.Attribute("R").Value);
                        _outerColorG = Convert.ToByte(colorElement.Attribute("G").Value);
                        _outerColorB = Convert.ToByte(colorElement.Attribute("B").Value);
                    }

                }
                
                //query the stroke to extract its StylusPoints
                var points = from p 
                    in strokeNodeElement.Descendants(xmlns + "StylusPoint") select p;

                //create Stylus points collection from point element values
                StylusPointCollection pointData = new System.Windows.Input.StylusPointCollection();
                foreach (XElement pointElement in points)
                {
                    double Xpoint = Convert.ToDouble(pointElement.Attribute("X").Value);
                    double Ypoint = Convert.ToDouble(pointElement.Attribute("Y").Value);
                    pointData.Add(new StylusPoint(Xpoint, Ypoint));
                }
            
                //create a new Stroke from the StylusPointCollection
                System.Windows.Ink.Stroke newstroke = new System.Windows.Ink.Stroke(pointData);
                newstroke.DrawingAttributes.Width = _width;
                newstroke.DrawingAttributes.Height=_height;
                newstroke.DrawingAttributes.Color = Color.FromArgb(_colorA,_colorR,_colorG,_colorB);
                newstroke.DrawingAttributes.OutlineColor = Color.FromArgb(_outerColorA, _outerColorR, _outerColorG, _outerColorB);
            

                //add the new stroke to the StrokeCollection
                objStrokes.Add(newstroke);
           }

            return objStrokes;
        }
    
    }
}
