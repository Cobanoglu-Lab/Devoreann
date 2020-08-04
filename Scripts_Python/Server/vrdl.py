"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

"""
vrdl.py (Virtual Reality Deep Learning)

Hub program that spawns and handles communication between a server and evaluator.
"""
import os
os.environ["CUDA_DEVICE_ORDER"] = "PCI_BUS_ID"
os.environ['CUDA_VISIBLE_DEVICES'] = ""#"-1"

import grpc
import evaluator_pb2
import evaluator_pb2_grpc
from concurrent import futures
_ONE_DAY_IN_SECONDS = 60 * 60 * 24
import time

from evaluator import main as evaluator_main
from evaluator import GPUSpawnerPersistent as spawner
from data_manager import DataManager
from server import Evaluator
from multiprocessing import Process, Queue

from pychannel import main as channel_main


HOST = 'localhost'
PORT = 50052
PORT_UNITY = 50053
NUM_FOLDS = 4  # Number of times to test the model for k-fold stratification.
NUM_GPUS = 1   # Spawns k persistent threads (1 per GPU).

def serve(eval, channel):
    _server = grpc.server(futures.ThreadPoolExecutor(max_workers=10))
    evaluator_pb2_grpc.add_EvaluatorServicer_to_server(Evaluator(dm, eval, commandq, ucommandq), _server)
    _server.add_insecure_port('localhost:50051')
    _server.start()
    print("server started")
    try:
        while True:
            time.sleep(_ONE_DAY_IN_SECONDS)
    except KeyboardInterrupt:
        _server.stop(0)
        eval.on_exit()

if __name__ == '__main__':
    # Spawn a server and evaluator & pychannel to VRPC
    commandq = Queue()
    ucommandq = Queue() # Unity command queue
    
    channel = Process(target=channel_main, args=(commandq, HOST, PORT,))
    channel.start()
    
    uchannel = Process(target=channel_main, args=(ucommandq, HOST, PORT_UNITY))
    uchannel.start()

    dm = DataManager(NUM_FOLDS)
    eval = evaluator_main(NUM_GPUS, NUM_FOLDS, dm, ucommandq, ucommandq, ucommandq)
    serve(eval, channel)
    channel.join()
