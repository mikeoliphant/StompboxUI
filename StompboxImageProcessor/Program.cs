using System.Drawing;

namespace StompboxImageProcessor
{
    class StompboxImageProcessor : ImageSheetProcessor.ImageSheetProcessor
    {
        public void RenderImages(string destPath)
        {
            BeginRenderImages(destPath);
            Render();

            EndRenderImages();
        }

        public void Render()
        {
            BeginSpriteSheetGroup("UISheet");

            AddFont("MainFont", "Calibri", FontStyle.Bold, 36);
            AddFont("SmallFont", "Calibri", 28);

            PushDirectory("UserInterface");

            Add("SingleWhitePixel");

            //CreateRotations("DialPointer", "DialPointerRotations", 32);

            //DoSquaredSprites = true;
            //processor.Scale("DialPointerRotations", 128, 128);
            //processor.DoSquaredSprites = false;

            Add("StompboxLogo");

            AddWithShadow("HoverTextOutline");

            Add("DialBackground");
            Add("DialPointer");

            Add("PowerOff");
            Add("PowerOn");

            Add("PopupBackground");
            Add("PluginBackground");
            Add("ButtonPressed");
            Add("ButtonUnpressed");

            Add("LevelDisplay");

            Add("TunerPoint");

            Add("VerticalSlider");

            Add("MoreButton");

            Add("Play");
            Add("Stop");
            Add("Restart");
            Add("Record");

            PopDirectory();

            EndSpriteSheetGroup();
        }

        void CreateRotations(string srcImage, string destImage, int numRotations)
        {
            string srcPath = GetSourcePath(srcImage);
            string destPath = GetSourcePath(destImage);

            Bitmap srcBitmap = (Bitmap)Bitmap.FromFile(srcPath);
            srcBitmap.SetResolution(96, 96);

            Bitmap destBitmap = new Bitmap(srcBitmap.Width * numRotations, srcBitmap.Height);

            Bitmap tmpBitmap = new Bitmap(srcBitmap.Width, srcBitmap.Height);

            for (int i = 0; i < numRotations; i++)
            {
                float angle = ((float)i / (float)numRotations) * 360f;

                Rectangle srcRect = new Rectangle(0, 0, srcBitmap.Width, srcBitmap.Height);

                using (Graphics gt = Graphics.FromImage(tmpBitmap))
                {
                    using (Graphics g = Graphics.FromImage(destBitmap))
                    {
                        gt.Clear(Color.Transparent);

                        //move rotation point to center of image
                        gt.TranslateTransform((float)srcBitmap.Width / 2, (float)srcBitmap.Height / 2);
                        //rotate
                        gt.RotateTransform(angle);
                        //move image back
                        gt.TranslateTransform(-(float)srcBitmap.Width / 2, -(float)srcBitmap.Height / 2);
                        //draw passed in image onto graphics object
                        gt.DrawImage(srcBitmap, 0, 0, srcRect, GraphicsUnit.Pixel);

                        g.DrawImage(tmpBitmap, i * srcBitmap.Width, 0, srcRect, GraphicsUnit.Pixel);
                    }
                }
            }

            destBitmap.Save(destPath, System.Drawing.Imaging.ImageFormat.Png);
        }
    }


    class Program
    {
        static void Main(string[] args)
        {
            var processor = new StompboxImageProcessor();

            processor.SrcPath = @"C:\Code\StompboxUI\SrcTextures";
            processor.ForceRegen = false;

            processor.RenderImages(@"C:\Code\StompboxUI\StompboxShared\Content\Textures");
        }

    }
}
