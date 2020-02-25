using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using NReco.VideoConverter;
using Microsoft.WindowsAzure.Storage.Blob;


namespace VideoCutter_WebJob
{
    public class Functions
    {
        
        public static void GenerateVideo(
        [QueueTrigger("videocutter")] String blobInfo,
        [Blob("videocutter/videos/{queueTrigger}")] CloudBlockBlob inputBlob,
        [Blob("videocutter/videostrim/{queueTrigger}")] CloudBlockBlob outputBlob, TextWriter logger)
        {
            //use log.WriteLine() rather than Console.WriteLine() for trace output
            logger.WriteLine("GenerateVideo() started...");
            logger.WriteLine("Input blob is: " + blobInfo);

            //Add debugging lines to check if blob exists and if it's in the right containter
            logger.WriteLine(inputBlob.Exists());
            logger.WriteLine(inputBlob.Uri);
            

            // Open streams to blobs for reading and writing as appropriate.
            // Pass references to application specific methods
            using (Stream input = inputBlob.OpenRead())
            using (Stream output = outputBlob.OpenWrite())
            {
                
                CreateVideoSample(input, output);
                outputBlob.Properties.ContentType = "video/mp4";

                if (inputBlob.Metadata.ContainsKey("Title"))
                {
                    outputBlob.Metadata["Title"] = inputBlob.Metadata["Title"];
                }
                else
                {
                    outputBlob.Metadata["Title"] = " ";
                }
            }
            logger.WriteLine("GenerateVideo() completed...");
        }

        // Since the spec doc only mentioned trimming to 5 secs, the method will use 5 as default
        private static void CreateVideoSample(Stream input, Stream output, int duration = 5)
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
