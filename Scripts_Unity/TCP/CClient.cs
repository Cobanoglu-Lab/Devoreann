/* Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
 *  Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
 *  Department: Lyda Hill Department of Bioinformatics 
 *  
 *  Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class CClient : MonoBehaviour
{
    public Text ModelText;
    public Text OutputText;
    public Text P1, P2, P3;
    private static readonly object strlock = new object();
    private Queue<string> DataQueue = new Queue<string>();
    private Queue<string> q1 = new Queue<string>(); // updates for each gpu
    private Queue<string> q2 = new Queue<string>(); // updates for each gpu
    private Queue<string> q3 = new Queue<string>(); // updates for each gpu
    public bool bRefreshDataStream = true;

    private float waitTime = 0.1f;
    private static Socket sender;

    private CModelHandler modelHandler;

    // Use this for initialization
    void Start()
    {
        modelHandler = FindObjectOfType<CModelHandler>();
        Debug.Log("Starting CClient");
        StartCoroutine(StartClient());
        StartCoroutine(RefreshDataStream());
        StartCoroutine(Refresh1());
        StartCoroutine(Refresh2());
        StartCoroutine(Refresh3());
    }

    // TODO: consolodate
    private IEnumerator Refresh1()
    {
        if (!P1) yield return null;
        while (true)
        {
            if(q1.Count > 0)
            {
                string data = q1.Dequeue();
                /*if (data.Contains("Epoch"))
                {
                    modelHandler.IncrementEpoch(0, data);
                    continue;
                }*/
                if (data.Contains("FOLD"))
                {
                    Debug.LogError(data);
                    modelHandler.SetFold(0, data);
                    continue;
                }
                P1.text = data;
            }
            yield return new WaitForSeconds(waitTime);
        }
    }
    private IEnumerator Refresh2()
    {
        if (!P2) yield return null;
        while (true)
        {
            if (q2.Count > 0)
            {
                string data = q2.Dequeue();
                /*if (data.Contains("Epoch"))
                {
                    modelHandler.IncrementEpoch(1, data);
                    continue;
                }*/
                if (data.Contains("FOLD"))
                {
                    Debug.LogError(data);
                    modelHandler.SetFold(1, data);
                    continue;
                }
                P2.text = data;
            }
            yield return new WaitForSeconds(waitTime);
        }
    }
    private IEnumerator Refresh3()
    {
        if (!P3) yield return null;
        while (true)
        {
            if (q3.Count > 0)
            {
                string data = q3.Dequeue();
                /*if (data.Contains("Epoch"))
                {
                    modelHandler.IncrementEpoch(2, data);
                    continue;
                }*/
                if (data.Contains("FOLD"))
                {
                    Debug.LogError(data);
                    modelHandler.SetFold(2, data);
                    continue;
                }

                P3.text = data;
            }
            yield return new WaitForSeconds(waitTime);
        }
    }

    public void ResetGPUView()
    {
        if (P1) P1.text = "";
        if (P2) P2.text = "";
        if (P3) P3.text = "";
    }

    private IEnumerator RefreshDataStream()
    {
        while (true)
        {
            if (bRefreshDataStream && DataQueue.Count > 0)
            {
                string str = DataQueue.Dequeue();
                str = str.Trim('\r', '\n');
                str = Regex.Replace(str, @"\[=*>.*\]*ETA*:*.{6}", "");
                Debug.LogWarning(str);
                int gpu = -1;
                try{
                    int.TryParse(str[str.Length-1].ToString(), out gpu);
                } catch{ continue; }
                //Debug.LogError(str);

                if (str.Contains("ETA") || str.Contains("=") || str.Contains("Epoch") || str.Contains("val_loss"))
                    continue;

                if (str.Contains("accs"))
                {
                    modelHandler.ProcessCI(str);
                    continue;
                }

                if (gpu == 0 && P1) { q1.Enqueue(str.Remove(str.Length - 1, 1)); } 
                else if (gpu == 1 && P2) { q2.Enqueue(str.Remove(str.Length - 1, 1)); } 
                else if (gpu == 2 && P3) { q3.Enqueue(str.Remove(str.Length - 1, 1)); } 
            }
            yield return new WaitForSeconds(0.01f);
        }
    }

    private void AppendToDataQueue(string data)
    {
        string[] outputs = data.Split('$');
        for(int i = 0; i < outputs.Length; ++i)
        {
            if (outputs[i].Length > 1)
            {
                //Debug.LogWarning(outputs[i]);
                DataQueue.Enqueue(outputs[i]);
            }
        }
    }

    private IEnumerator StartClient()
    {
        Thread thread = new Thread(() => SynchronousSocketClient.Main("198.215.56.140", "50053"));
        thread.Start();
        //SynchronousSocketClient.Main("127.0.0.1", "65432");
        while (true)
        {
            bool __lockWasTaken = false;
            try
            {
                System.Threading.Monitor.TryEnter(strlock, ref __lockWasTaken);
                if (__lockWasTaken)
                {
                    String data = SynchronousSocketClient.dataStream.ToString();
                    if (data.Length > 0)
                    {
                        //Debug.LogWarning(data);
                        AppendToDataQueue(data);
                        //if (OutputText) OutputText.text = data;
                    }
                    SynchronousSocketClient.dataStream.Remove(0, SynchronousSocketClient.dataStream.Length);

                    String model = SynchronousSocketClient.modelStream.ToString();
                    if (model.Length > 0)
                    {
                        Debug.LogWarning(model);
                        SynchronousSocketClient.modelStream.Clear();
                        model = model.Replace("\n", "\n");
                        model = model.Replace("summary#", "");
                        model = model.Replace("end#","");

                        if (ModelText) ModelText.text = model;
                    }
                }
            }
            finally
            {
                if (__lockWasTaken) System.Threading.Monitor.Exit(strlock);
            }

            yield return new WaitForSeconds(.5f);
        }
    }

    public class SynchronousSocketClient
    {
        public static StringBuilder dataStream = new StringBuilder("");
        public static StringBuilder modelStream = new StringBuilder("");

        public static bool ProcessModelString(ref String str, ref Socket sender)
        {
            if (str.Substring(0, 8).Equals("summary#"))
            {
                byte[] bytes = new byte[1024];
                String modelstr = "";
                //lock (strlock) { modelStream.Clear(); } // Clear any existing models.
                modelstr += str;
                while (!modelstr.EndsWith("end#"))
                {
                    Thread.Sleep(10);
                    int bytesRec = sender.Receive(bytes);
                    modelstr += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                }
                lock (strlock)
                {
                    modelStream.Clear();
                    modelStream.Append(modelstr);
                }
                byte[] msg = Encoding.ASCII.GetBytes("end#");
                int bytesSent = sender.Send(msg);
                return true;
            }
            else return false; // Not a model summary string
        }

        public static bool ProcessStreamString(ref String str, ref Socket sender)
        {
            if (str.Substring(0, 5).Equals("start"))
            {
                int idx = int.Parse(str.Substring(5));
                byte[] bytes = new byte[1024];
                String data = "";
                while (!data.EndsWith("finished#"))
                {
                    int bytesRec = sender.Receive(bytes);
                    data = Encoding.ASCII.GetString(bytes, 0, bytesRec);
                    lock (strlock)
                    {
                        dataStream.Append(data);
                    }
                }
                byte[] msg = Encoding.ASCII.GetBytes("end#");
                int bytesSent = sender.Send(msg);
                return true;
            }
            else return false;
        }

        private static bool ProcessAccuracyString(ref string str, ref Socket sender)
        {
            if (str.Substring(0, 4).Equals("accs"))
            {
                lock (strlock)
                {
                    dataStream.Append(str);
                }
                return true;
            }
            return false;
        }

        public static void ProcessString(ref String str, ref Socket sender)
        {
            if(str.Length > 9) { 
                //String mine = str.Substring(0, 8);
                if (ProcessModelString(ref str, ref sender)) return;
            }
            if(str.Length > 5)
            {
                //String mine = str.Substring(0, 5);
                if (ProcessStreamString(ref str, ref sender)) return;
            }
            if(str.Length > 4)
            {
                if (ProcessAccuracyString(ref str, ref sender)) return;
            }

            lock (strlock)
            {
                dataStream.Append(str);
            }

            byte[] msg = Encoding.ASCII.GetBytes("end#");
            int bytesSent = sender.Send(msg);
        }

        

        public static void StartClient(string IP, int PORT)
        {
            // Data buffer for incoming data.  
            byte[] bytes = new byte[1024];

            // Connect to a remote device.  
            try
            {
                // Establish the remote endpoint for the socket.  
                // This example uses port 11000 on the local computer.  
                //IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                //IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPAddress ipAddress = IPAddress.Parse(IP);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, PORT);

                // Create a TCP/IP  socket.  
                sender = new Socket(ipAddress.AddressFamily,
                    SocketType.Stream, ProtocolType.Tcp);

                // Connect the socket to the remote endpoint. Catch any errors.  
                try
                {
                    sender.Connect(remoteEP);

                    try
                    {
                        while (true)
                        {
                            Console.WriteLine("Socket connected to {0}",
                            sender.RemoteEndPoint.ToString());
                                
                            int bytesRec = sender.Receive(bytes);
                            String resultStr = Encoding.ASCII.GetString(bytes, 0, bytesRec);

                            ProcessString(ref resultStr, ref sender);
                        }
                    }
                    finally
                    {
                        //byte[] msg = Encoding.ASCII.GetBytes("shutdown#");
                        //int bytesSent = sender.Send(msg);
                        // Release the socket.
                        sender.Shutdown(SocketShutdown.Both);
                        sender.Close();
                    }

                    /*
                    // Encode the data string into a byte array.  
                    byte[] msg = Encoding.ASCII.GetBytes("This is a test<EOF>");

                    // Send the data through the socket.  
                    int bytesSent = sender.Send(msg);

                    // Receive the response from the remote device.  
                    int bytesRec = sender.Receive(bytes);
                    Console.WriteLine("Echoed test = {0}",
                        Encoding.ASCII.GetString(bytes, 0, bytesRec));
                    */
                }
                catch (ArgumentNullException ane)
                {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                }
                catch (SocketException se)
                {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                }
                catch (Exception e)
                {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static int Main(String IP, String PORT)
        {
            int intPORT;
            int.TryParse(PORT, out intPORT);
            StartClient(IP, intPORT);
            return 0;
        }

        public void OnApplicationQuit()
        {
            sender.Shutdown(SocketShutdown.Both);
            sender.Close();
        }
    }
}
