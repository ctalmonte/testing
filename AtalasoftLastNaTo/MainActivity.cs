using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Atalasoft.FormsProcessing.Omr;
using Atalasoft.Imaging;

namespace AtalasoftLastNaTo
{
    [Activity(Label = "AtalasoftLastNaTo", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        private string imgDocPath = "/mnt/sdcard/tapat/ballots/1.jpg";
        private string tempPath = "/mnt/sdcard/tapat/template/bubbles.template";
        private OmrTemplateDocument template;

        private OmrMark[] recogMark;

        private OmrMark[] OmrRecog()
        {
            OmrEngine engine = new OmrEngine();
            engine.AlignmentConfidenceThreshold = 0.40f;

            using (FileStream tempFS = new FileStream(tempPath, FileMode.Open, FileAccess.Read))
                template = OmrTemplateDocument.Load(tempFS);

            FileSystemImageSource markSrc = new FileSystemImageSource(imgDocPath, true);
            OmrDocument results = engine.RecognizeDocument(markSrc, template);
            return ProcessRecogResults(results);
        }

        private OmrMark[] ProcessRecogResults(OmrDocument doc)
        {
            List<OmrMark> recognizedMarksList = new List<OmrMark>();
            foreach (OmrPage page in doc.Pages)
            {
                foreach (OmrGroup @group in page.Groups)
                {
                    foreach (OmrMark mark in @group.Marks)
                    {
                        recognizedMarksList.Add(mark);
                    }
                }
            }
            return recognizedMarksList.ToArray();
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Get our button from the layout resource,
            // and attach an event to it
            Button button = FindViewById<Button>(Resource.Id.MyButton);

            button.Click += delegate 
            {
                recogMark = OmrRecog();
                using (FileStream mFS = new FileStream("/mnt/sdcard/tapat/votes/1.txt", FileMode.Create, FileAccess.Write))
                {
                    foreach (OmrMark m in recogMark)
                    {
                        string mString = String.Format("Mark {0}: {1} \r\n", m.Template.Name, m.IsMarked);
                        ASCIIEncoding encoding = new ASCIIEncoding();
                        byte[] characters = encoding.GetBytes(mString);
                        foreach (byte character in characters)
                        {
                            mFS.WriteByte(character);
                        }
                    }
                    mFS.Close();
                    //TEST COMMENT
                }
            };
        }
    }
}

