// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using EchoBotML.Model.DataModels;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;
using Microsoft.ML;

namespace Microsoft.BotBuilderSamples.Bots
{
    public class EchoBot : ActivityHandler
    {
        private const string MODEL_FILEPATH = @"MLModel.zip";
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            MLContext mlContext = new MLContext();

            ITransformer mlModel = mlContext.Model.Load(GetAbsolutePath(MODEL_FILEPATH), out _);
            var predEngine = mlContext.Model.CreatePredictionEngine<ModelInput, ModelOutput>(mlModel);
            ModelInput sampleData = new ModelInput { SentimentText = turnContext.Activity.Text };
            ModelOutput predictionResult = predEngine.Predict(sampleData);
            var predictionBool = predictionResult.Prediction;
            //var predictionBool = predictionResult.Score > 0.7 ? predictionResult.Prediction : !predictionResult.Prediction;
            await turnContext.SendActivityAsync(MessageFactory.Text($"Text: {turnContext.Activity.Text} | Prediction: {(Convert.ToBoolean(predictionBool) ? "Toxic" : "Non Toxic")} sentiment"), cancellationToken);
        }

        public static string GetAbsolutePath(string relativePath)
        {
            FileInfo _dataRoot = new FileInfo(typeof(Program).Assembly.Location);
            string assemblyFolderPath = _dataRoot.Directory.FullName + @"\EchoBotML.Model\";

            string fullPath = Path.Combine(assemblyFolderPath, relativePath);

            return fullPath;
        }

        protected override async Task OnMembersAddedAsync(IList<ChannelAccount> membersAdded, ITurnContext<IConversationUpdateActivity> turnContext, CancellationToken cancellationToken)
        {
            foreach (var member in membersAdded)
            {
                if (member.Id != turnContext.Activity.Recipient.Id)
                {
                    await turnContext.SendActivityAsync(MessageFactory.Text($"Hello and welcome!"), cancellationToken);
                }
            }
        }
    }
}
