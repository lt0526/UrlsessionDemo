using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

using Foundation;
using UIKit;

namespace NSUrlsessionDemo
{
    public class ViewModel
    {
        public delegate void DownloadProgressDelegate(float progress);
        public event DownloadProgressDelegate DownloadEvent;

        
        public void downloadTask()
        {
            NSUrl url = NSUrl.FromString("http://speed.myzone.cn/pc_elive_1.1.rar");

            var config = NSUrlSessionConfiguration.DefaultSessionConfiguration;
            NSUrlSession session = NSUrlSession.FromConfiguration(config, (new SimpleSessionDelegate(DownloadEvent) as INSUrlSessionDelegate), new NSOperationQueue());
            var downloadTask = session.CreateDownloadTask(NSUrlRequest.FromUrl(url));

            downloadTask.Resume();
        }
    }

    public class SimpleSessionDelegate : NSUrlSessionDownloadDelegate
    {
        event ViewModel.DownloadProgressDelegate DownloadEvent;
        public SimpleSessionDelegate(ViewModel.DownloadProgressDelegate downloadEvent)
        {
            DownloadEvent = downloadEvent;
        }
        
        public override void DidFinishDownloading(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, NSUrl location)
        {
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var destinationPath = Path.Combine(documents, "Sample.docx");

            if (File.Exists(location.Path))
            {
                NSFileManager fileManager = NSFileManager.DefaultManager;
                NSError error;

                fileManager.Remove(destinationPath, out error);

                bool success = fileManager.Copy(location.Path, destinationPath, out error);

                if (!success)
                {
                    Console.WriteLine("Error during the copy: {0}", error.LocalizedDescription);
                }
            }
        }
        public override void DidWriteData(NSUrlSession session, NSUrlSessionDownloadTask downloadTask, long bytesWritten, long totalBytesWritten, long totalBytesExpectedToWrite)
        {
            new System.Threading.Thread(new System.Threading.ThreadStart(() =>
            {
                InvokeOnMainThread(() =>
                {
                    //how to access UI?
                    float progress = (float)totalBytesWritten / totalBytesExpectedToWrite;
                    DownloadEvent(progress);
                });
            })).Start();
        }
    }
}