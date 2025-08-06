using AForge.Video.DirectShow;
using AForge.Video;
using System.Drawing;

namespace LPGxWebApp.Class
{
    public class WebcamCapture
    {
        private FilterInfoCollection videoDevices;  // List of available video devices
        private VideoCaptureDevice videoSource;     // Webcam video source
        private Bitmap currentFrame;                // Captured frame

        public WebcamCapture()
        {
            // Initialize webcam device
            videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            if (videoDevices.Count == 0)
            {
                throw new Exception("No video devices found.");
            }

            // Select the first available webcam
            videoSource = new VideoCaptureDevice(videoDevices[0].MonikerString);

            // Set up the event for capturing the video frame
            videoSource.NewFrame += new NewFrameEventHandler(videoSource_NewFrame);
        }

        // Event handler for capturing a new frame
        private void videoSource_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            // Save the current frame to a bitmap object
            currentFrame = (Bitmap)eventArgs.Frame.Clone();
        }

        // Start the webcam capture
        public void StartCapture()
        {
            videoSource.Start();
        }

        // Stop the webcam capture
        public void StopCapture()
        {
            if (videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource.WaitForStop();
            }
        }

        // Convert captured image to byte array
        public byte[] ConvertToByteArray()
        {
            if (currentFrame == null)
            {
                throw new Exception("No image captured.");
            }

            using (MemoryStream ms = new MemoryStream())
            {
                // Save the current frame to the memory stream in PNG format
                currentFrame.Save(ms, System.Drawing.Imaging.ImageFormat.Png);

                // Return the byte array from the memory stream
                return ms.ToArray();
            }
        }
    }
}
