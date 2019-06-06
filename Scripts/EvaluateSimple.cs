/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using System.Threading.Tasks;
using System.Threading;
using Evaluator;
using Google.Protobuf.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class EvaluateSimple : MonoBehaviour
{
    Channel channel;
    Evaluator.Evaluator.EvaluatorClient client;
    float accuracy;

    CLayerManager manager;

    private bool bIsCalculating = false;
    private string conf;

    private int numClasses = -1;
    private int numDataPoints = -1;
    private int numTraining;

    // Use this for initialization
    void Start()
    {
        if (Application.isEditor)
        {
            conf = "C:/Users/VR_Demo/Desktop/VR_DevelopmentIDE/VR_DevelopmentIDE/VR_DL/Package/config.ini";
        }
        else
        {
            conf = Application.dataPath + "/config.ini";
        }
        SetConfValues();
        /*string localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect("8.8.8.8", 50051);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address.ToString();
        }*/
        //string localIP = "172.18.227.36";

        string localIP = "localhost";

        channel = new Channel(localIP + ":50051", ChannelCredentials.Insecure);
        Debug.Log("Created channel.");
        client = new Evaluator.Evaluator.EvaluatorClient(channel);
        Debug.Log("Created client.");

        manager = FindObjectOfType<CLayerManager>();
    }

    void SetConfValues()
    {
        string conf_str = File.ReadAllText(conf);

        Regex r_classes = new Regex(@"num_classes\s=\s\d+");
        Regex r_datapoints = new Regex(@"num_datapoints\s=\s\d+");
        Regex r_digits = new Regex(@"\d+");

        Match match = r_classes.Match(conf_str);
        if (match.Success)
        {
            match = r_digits.Match(match.Value);
            if (match.Success)
            {
                int.TryParse(match.Value, out numClasses);
            }
        }

        match = r_datapoints.Match(conf_str);
        if (match.Success)
        {
            match = r_digits.Match(match.Value);
            if (match.Success)
            {
                int.TryParse(match.Value, out numDataPoints);
            }
        }

        if (numDataPoints == -1 || numClasses == -1) { numTraining = 7000; }
        else numTraining = (int)(numClasses * .8 * .8 * numDataPoints);
    }

    public IEnumerator Evaluate(CLayerType[] simpleLayers)
    {
        EvaluateRequest req = new EvaluateRequest { };
        bool flat = false;
        for (int i = 0; i < simpleLayers.Length; i++)
        {
            CLayerType l = simpleLayers[i];
            switch (l)
            {
                case CLayerType.Conv:
                    if (flat) {
                        throw new System.ArgumentException("tried to add multidimensional layer after flatten");
                    }
                    req.Layers.Add(new Evaluator.Layer { Convolution = new Evaluator.ConvolutionLayer { Filters = 32 } });
                    break;
                case CLayerType.Full:
                case CLayerType.Dense:
                    if (!flat) {
                        req.Layers.Add(new Evaluator.Layer{Flatten = new Evaluator.FlattenLayer{}});
                        flat = true;
                    }
                    req.Layers.Add(new Evaluator.Layer { Dense = new Evaluator.DenseLayer { Neurons = 128 } });
                    break;
                case CLayerType.Pool:
                    if (flat) {
                        throw new System.ArgumentException("tried to add multidimensional layer after flatten");
                    }
                    req.Layers.Add(new Evaluator.Layer { Maxpooling = new Evaluator.MaxpoolingLayer { } });
                    break;
                case CLayerType.Dropout:
                    req.Layers.Add(new Evaluator.Layer { Dropout = new Evaluator.DropoutLayer { Dimension = 0.5f } });
                    break;
                default:
                    throw new System.ArgumentException("unknown layer type: " + l);
            }
        }
        if (!flat) {
            req.Layers.Add(new Evaluator.Layer { Flatten = new Evaluator.FlattenLayer { } });
        }
        bIsCalculating = true;
        using (var call = client.EvaluateAsync(req)) {
            StartCoroutine(UpdateProgress());
            while (!call.ResponseAsync.IsCompleted) {
                //accuracy = call.ResponseAsync.Result.Accuracy;
                yield return new WaitForSeconds(0.5f);
            }
            bIsCalculating = false;
            accuracy = call.ResponseAsync.Result.Accuracy;
            if (manager) manager.CompleteComputation(accuracy);
        }
    }

    public IEnumerator UpdateProgress()
    {
        if (!manager) yield return null;

        string reg = @"\d+/" + numTraining.ToString();
        Regex regex = new Regex(@reg);

        FileStream logFileStream = new FileStream(manager.logpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        StreamReader logFileReader = new StreamReader(logFileStream);

        bool bLocalWaiting = true;
        int epochNum = 1;

        while (bIsCalculating && bLocalWaiting)
        {
            if (!logFileReader.EndOfStream)
            {
                string line = logFileReader.ReadLine();

                if(line.Length > 4 && line.Substring(0,4) == numTraining.ToString())
                {
                    epochNum++;
                }
                else if (line.Contains("Test"))
                {
                    manager.UpdateProgress("Writing activation layers...");
                    bLocalWaiting = false; // Don't read anymore lines
                }
                else
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        // manager.UpdateProgress("Epoch #" + (3-numEpochs) + ": " + match.Value + " complete.");
                        manager.UpdateProgress("Epoch #" + epochNum +" " + match.Value + " complete.");
                    }
                }
            }

            yield return new WaitForSeconds(0.12f);
        }

        // Clean up
        logFileReader.Close();
        logFileStream.Close();
    }
}