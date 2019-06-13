/* Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 *  
 *  Copyright © 2019, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
 */

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

public class CEvaluator : MonoBehaviour
{
    float accuracy;

    const string PYTHON = "C:/Users/VR_Demo/Anaconda3/envs/VR_Env_GPU/python.exe";

    CLayerManager manager;

    private bool bIsCalculating = false;

    // Use this for initialization
    void Start()
    {
        manager = FindObjectOfType<CLayerManager>();
    }

    public IEnumerator Evaluate(CLayerType[] simpleLayers)
    {
        yield return null;
        string args = "";

        bool flat = false;
        for (int i = 0; i < simpleLayers.Length; i++)
        {
            CLayerType l = simpleLayers[i];
            switch (l)
            {
                case CLayerType.Conv:
                    if (flat)
                    {
                        throw new System.ArgumentException("tried to add multidimensional layer after flatten");
                    }
                    args += "Conv2D:"+"32,"; // :filters
                    break;
                case CLayerType.Full:
                case CLayerType.Dense:
                    if (!flat)
                    {
                        args += "Flatten,";
                        flat = true;
                    }
                    args += "Dense:128,";
                    break;
                case CLayerType.Pool:
                    if (flat)
                    {
                        throw new System.ArgumentException("tried to add multidimensional layer after flatten");
                    }
                    args += "Pool,";
                    break;
                case CLayerType.Dropout:
                    args += "Dropout:0.5,";
                    break;
                default:
                    throw new System.ArgumentException("unknown layer type: " + l);
            }
        }
        if (!flat)
        {
            args += "Flatten,";
        }
        args += "$"; // End of stream

        // Send to python as string
        bIsCalculating = true;


        RunPythonScript("python evaluator.py",args);

        /*using (var call = client.EvaluateAsync(req))
        {
            StartCoroutine(UpdateProgress());
            while (!call.ResponseAsync.IsCompleted)
            {
                //accuracy = call.ResponseAsync.Result.Accuracy;
                yield return new WaitForSeconds(0.5f);
            }
            bIsCalculating = false;
            accuracy = call.ResponseAsync.Result.Accuracy;
            if (manager) manager.CompleteComputation(accuracy);
        }*/
    }

    public IEnumerator UpdateProgress()
    {
        if (!manager) yield return null;

        Regex regex = new Regex(@"\d+/7000");

        FileStream logFileStream = new FileStream(manager.logpath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        StreamReader logFileReader = new StreamReader(logFileStream);

        bool bLocalWaiting = true;

        while (bIsCalculating && bLocalWaiting)
        {
            if (!logFileReader.EndOfStream)
            {
                string line = logFileReader.ReadLine();

                if (line.Length > 4 && line.Substring(0, 4) == "7000")
                {
                    manager.UpdateProgress("Writing activation layers...");
                    bLocalWaiting = false; // Don't read anymore lines
                }
                else
                {
                    Match match = regex.Match(line);
                    if (match.Success)
                    {
                        manager.UpdateProgress(match.Value + " complete.");
                    }
                }
            }

            yield return new WaitForSeconds(0.12f);
        }

        // Clean up
        logFileReader.Close();
        logFileStream.Close();
    }


    public string RunPythonScript(string cmd, string args)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = PYTHON;
        start.Arguments = string.Format("\"{0}\" \"{1}\"", cmd, args);
        start.UseShellExecute = false;// Do not use OS shell
        start.CreateNoWindow = true; // We don't need new window
        start.RedirectStandardOutput = true;// Any output, generated by application will be redirected back
        start.RedirectStandardError = true; // Any error in standard output will be redirected back (for example exceptions)
        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string stderr = process.StandardError.ReadToEnd(); // Here are the exceptions from our Python script
                string result = reader.ReadToEnd(); // Here is the result of StdOut(for example: print "test")
                return result;
            }
        }
    }
}