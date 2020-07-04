// 
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license.
// 
// Microsoft Cognitive Services: https://azure.microsoft.com/en-us/services/cognitive-services
// 
// Microsoft Cognitive Services GitHub:
// https://github.com/Microsoft/Cognitive-CustomVision-Windows
// 
// Copyright (c) Microsoft Corporation
// All rights reserved.
// 
// MIT License:
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED ""AS IS"", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// 


using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Training;
using Microsoft.Azure.CognitiveServices.Vision.CustomVision.Prediction;


namespace CustomVision.Sample
{
    class Program
    {
        private static List<MemoryStream> hemlockImages;

        private static List<MemoryStream> japaneseCherryImages;

        private static MemoryStream testImage;

        static void Main(string[] args)
        {
            // Add your training and prediction key from the settings page of the portal 
            string trainingKey = "38571b510e9144dd961fcf9d8f9e2555";
            string predictionKey = "9e1d7bf1a6bc45d1b6150e5598655f30";

            // <add your prediction key here>
            // Create the Api, passing in the training key

            TrainingApi trainingApi = new TrainingApi() { ApiKey = trainingKey };

            // Create a new project
            Console.WriteLine("Creating new project:");
            var project = trainingApi.CreateProject("My New Project");
            // Make two tags in the new project
            var hemlockTag = trainingApi.CreateTag(project.Id, "Hemlock");
            var japaneseCherryTag = trainingApi.CreateTag(project.Id, "Japanese Cherry");

            // Add some images to the tags
            Console.WriteLine("\tUploading images");
            LoadImagesFromDisk();

            // Images can be uploaded one at a time
            foreach (var image in hemlockImages)
            {
                trainingApi.CreateImagesFromData(project.Id, image, new List<string>() { hemlockTag.Id.ToString() });
            }

            foreach (var image in japaneseCherryImages)
            {
                trainingApi.CreateImagesFromData(project.Id, image, new List<string>() { japaneseCherryTag.Id.ToString() });
            }
            // Now there are images with tags start training the project
            Console.WriteLine("\tTraining");
            var iteration = trainingApi.TrainProject(project.Id);

            // The returned iteration will be in progress, and can be queried periodically to see when it has completed
            while (iteration.Status == "Training")
            {
                Thread.Sleep(1000);

                // Re-query the iteration to get it's updated status
                iteration = trainingApi.GetIteration(project.Id, iteration.Id);
            }

            // The iteration is now trained. Make it the default project endpoint
            iteration.IsDefault = true;
            trainingApi.UpdateIteration(project.Id, iteration.Id, iteration);
            Console.WriteLine("Done!\n");
            // Now there is a trained endpoint, it can be used to make a prediction  
            PredictionEndpoint endpoint = new PredictionEndpoint() { ApiKey = predictionKey };

            // Make a prediction against the new project  
            Console.WriteLine("Making a prediction:");
            var result = endpoint.PredictImage(project.Id, testImage);

            // Loop over each prediction and write out the results  
            foreach (var c in result.Predictions)
            {
                Console.WriteLine($"\t{c.TagName}: {c.Probability:P1}");
            }
            Console.ReadKey();




        }

        private static void LoadImagesFromDisk()
        {
            // this loads the images to be uploaded from disk into memory
            hemlockImages = Directory.GetFiles(@"/Users/chengqun/LearnAI-Bootcamp/lab01.2_customvision01/Resources/Images/Hemlock").Select(f => new MemoryStream(File.ReadAllBytes(f))).ToList();
            japaneseCherryImages = Directory.GetFiles(@"/Users/chengqun/LearnAI-Bootcamp/lab01.2_customvision01/Resources/Images/Japanese Cherry").Select(f => new MemoryStream(File.ReadAllBytes(f))).ToList();
            testImage = new MemoryStream(File.ReadAllBytes(@"/Users/chengqun/LearnAI-Bootcamp/lab01.2_customvision01/Resources/Images/Test/test_image.jpg"));

        }
    }

    internal class PredictionEndpointCredentials
    {
        private string predictionKey;

        public PredictionEndpointCredentials(string predictionKey)
        {
            this.predictionKey = predictionKey;
        }
    }

    internal class TrainingApiCredentials
    {
        private string trainingKey;

        public TrainingApiCredentials(string trainingKey)
        {
            this.trainingKey = trainingKey;
        }
    }
}
