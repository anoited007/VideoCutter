using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using NReco.VideoConverter;
using Microsoft.WindowsAzure.Storage.Blob;


namespace VideoCutter_WebJob
{
    public class Functions
    {
        
        public static void GenerateThumbnail(
        [QueueTrigger("videocutter")] String blobInfo,
        [Blob("photogallery/videos/{queueTrigger}")] CloudBlockBlob inputBlob,
        [Blob("photogallery/videos/{queueTrigger}")] CloudBlockBlob outputBlob, TextWriter logger)
        {
            //use log.WriteLine() rather than Console.WriteLine() for trace output
            logger.WriteLine("GenerateVideo() started...");
            logger.WriteLine("Input blob is: " + blobInfo);

            // Open streams to blobs for reading and writing as appropriate.
            // Pass references to application specific methods
            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                CreateVideoSample(input, output, 5);
                outputBlob.Properties.ContentType = "video/mp4";
            }
            logger.WriteLine("GenerateVideo() completed...");
        }

        private static void CreateVideoSample(Stream input, Stream output, int duration)
        {

            BinaryWriter Writer = null;
            try
            {
                // Create a new stream to write to the file
                Writer = new BinaryWriter(File.Open("temp.mp4", FileMode.Create));
                BinaryReader Reader = new BinaryReader(input);
                byte[] imageBytes = null;
                imageBytes = Reader.ReadBytes((int)input.Length);
                // Writer raw data                
                Writer.Write(imageBytes);
                Writer.Flush();
                Writer.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("*** FileWrite exception: " + e.Message);

            }

            var vid_duration = new ConvertSettings();
            vid_duration.MaxDuration = duration;

            var ffMpeg = new FFMpegConverter();
            ffMpeg.ConvertMedia("temp.mp4", "mp4", output, "mp4", vid_duration);

        }

    }
}
