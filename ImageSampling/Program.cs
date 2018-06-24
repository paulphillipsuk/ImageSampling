using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;

namespace ImageSampling
{
    class Program
    {
        public static readonly List<string> ImageExtensions = new List<string> { ".JPG", ".JPE", ".BMP", ".GIF", ".PNG" };

        private const string WATERMARK = "Sample Image © 1st Safari Day Nurseries Ltd";

        static void Main(string[] args)
        {
            if (!args.Any())
            {
                Console.WriteLine("Use this via command line, ImageSampling.exe [directory to images]");
                return;
            }
            var directoryPath = args[0].ToString();

            var stopWatch = new Stopwatch();

            stopWatch.Start();

            Task.Run(() => IterateDirectories(directoryPath)).Wait();

            stopWatch.Stop();

            Console.WriteLine($"Completed - {stopWatch.Elapsed.Seconds} second(s).");
        }

        static void IterateDirectories(string path)
        {

            if (!Directory.Exists(path))
                return;

            var directoriesAndFiles = new DirectoryInfo(path);

            var loop1 = Parallel.ForEach(directoriesAndFiles.GetFiles(), new ParallelOptions() { MaxDegreeOfParallelism = 10 }, file =>
              {
                  if (file.Name.StartsWith("watermark-"))
                      return;
                  Console.WriteLine($"Processing - {file.FullName}");
                  try
                  {
                      if (IsAnImage(file.FullName))
                          AddWaterMark(file.FullName, file.Directory.FullName, $"watermark-{file.Name}");
                  }
                  catch (Exception e)
                  {
                      Console.WriteLine($"An unknown error occurred - {e.Message}.");
                  }

              });

            var loop2 = Parallel.ForEach(directoriesAndFiles.GetDirectories(), new ParallelOptions() { MaxDegreeOfParallelism = 10 }, directory =>
           {
               IterateDirectories(directory.FullName);
           });
        }

        static void AddWaterMark(string path, string saveDirectory, string newFileName)
        {
            using (Bitmap bmp = new Bitmap(path))
            {
                using (Graphics grp = Graphics.FromImage(bmp))
                {
                    Brush brush = new SolidBrush(Color.FromArgb(110, Color.DimGray));



                    //Set the Font and its size.
                    Font font = new Font("Comic Sans MS", 60, FontStyle.Bold, GraphicsUnit.Pixel);
                    //Determine the size of the Watermark text.
                    SizeF textSize = new SizeF();
                    textSize = grp.MeasureString(WATERMARK, font);

                    //Position the text and draw it on the image.
                    //Point position = new Point((bmp.Width - ((int)textSize.Width + 10)), (bmp.Height - ((int)textSize.Height + 10)));
                    Point position = new Point((bmp.Width - ((int)textSize.Width + 10)), (bmp.Height - ((int)textSize.Height + 10)));
                    grp.DrawString(WATERMARK, font, brush, position);

                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        //Save the Watermarked image to the MemoryStream.
                        var newPath = $"{saveDirectory}/{newFileName}";
                        bmp.Save(newPath, ImageFormat.Png);

                    }
                }
            }
        }


        private static bool IsAnImage(string path)
        {
            if (ImageExtensions.Contains(Path.GetExtension(path).ToUpperInvariant()))
            {
                return true;
            }
            return false;
        }
    }
}
