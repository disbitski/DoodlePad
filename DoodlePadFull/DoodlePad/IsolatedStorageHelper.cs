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
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.IO;
using System.Windows.Resources;

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
    public class IsolatedStorageHelper
    {

        
        public static BitmapImage ImgFromBase64(byte[] byteArray)
         {
             if (byteArray.Length != 0)
             {
                 //byte[] byteArray = Convert.FromBase64String(sBase64);
                 MemoryStream ms = new MemoryStream(byteArray);
                 BitmapImage imageSource = new BitmapImage();
                 imageSource.SetSource(ms);
                 //ms.Close();
                 //ms.Dispose();
                 return imageSource;
             }
             else
             {
                 return null;
             }
         }

        public static byte[] LoadIfExists(string fileName)
        {
            byte[] retVal;

            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                if (iso.FileExists(fileName))
                {
                    using (IsolatedStorageFileStream stream = iso.OpenFile(fileName, FileMode.Open))
                    {
                        retVal = new byte[stream.Length];
                        stream.Read(retVal, 0, retVal.Length);
                    }
                }
                else
                {
                    retVal = new byte[0];
                }
            }
            return retVal;
        }

        /// <summary>
        ///     Saves to isolated storage
        /// </summary>
        /// <param name="buffer">The buffer</param>
        /// <param name="fileName"></param>
        public static void SaveToDisk(byte[] buffer, string fileName)
        {
            using (IsolatedStorageFile iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (
                    IsolatedStorageFileStream stream = new IsolatedStorageFileStream(fileName, FileMode.OpenOrCreate, iso))
                {
                    stream.Write(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        ///     Gets an image from storage
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns>The bitmap</returns>
        public static WriteableBitmap GetImage(byte[] buffer)
        {
            int width = buffer[0] * 256 + buffer[1];
            int height = buffer[2] * 256 + buffer[3];

            long matrixSize = width * height;

            WriteableBitmap retVal = new WriteableBitmap(width, height);

            int bufferPos = 4;

            for (int matrixPos = 0; matrixPos < matrixSize; matrixPos++)
            {
                int pixel = buffer[bufferPos++];
                pixel = pixel << 8 | buffer[bufferPos++];
                pixel = pixel << 8 | buffer[bufferPos++];
                pixel = pixel << 8 | buffer[bufferPos++];
                retVal.Pixels[matrixPos] = pixel;
            }

            return retVal;
        }

        /// <summary>
        ///     Gets the buffer to save to disk from the writeable bitmap
        /// </summary>
        /// <param name="bitmap">The bitmap image</param>
        /// <returns>The buffer of bytes</returns>
        public static byte[] GetSaveBuffer(WriteableBitmap bitmap)
        {
            long matrixSize = bitmap.PixelWidth * bitmap.PixelHeight;

            long byteSize = matrixSize * 4 + 4;

            byte[] retVal = new byte[byteSize];

            long bufferPos = 0;

            retVal[bufferPos++] = (byte)((bitmap.PixelWidth / 256) & 0xff);
            retVal[bufferPos++] = (byte)((bitmap.PixelWidth % 256) & 0xff);
            retVal[bufferPos++] = (byte)((bitmap.PixelHeight / 256) & 0xff);
            retVal[bufferPos++] = (byte)((bitmap.PixelHeight % 256) & 0xff);

            for (int matrixPos = 0; matrixPos < matrixSize; matrixPos++)
            {
                retVal[bufferPos++] = (byte)((bitmap.Pixels[matrixPos] >> 24) & 0xff);
                retVal[bufferPos++] = (byte)((bitmap.Pixels[matrixPos] >> 16) & 0xff);
                retVal[bufferPos++] = (byte)((bitmap.Pixels[matrixPos] >> 8) & 0xff);
                retVal[bufferPos++] = (byte)((bitmap.Pixels[matrixPos]) & 0xff);
            }

            return retVal;
        }

    
    }


    

   
 




   }

