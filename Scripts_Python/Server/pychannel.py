"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

""" pyserver: communicates with the local machine to send/receive TCP messages
"""
import socket
import time
from multiprocessing import Queue
from queue import Empty

#HOST = '127.0.0.1'  # Standard loopback interface address (localhost)
#PORT = 65432        # Port to listen on (non-privileged ports are > 1023)

command_dict = {
    "refresh server" : "refresh_server#", # Refreshes server ini files
    "clear textures" : "clear_texture_cache#",
    "save images" : "plot_save_all#"
    }

def main(commandq, HOST, PORT):
    #commandq = Queue()
    #commandq.put(command_dict["refresh server"])
    while True:
        try:
            run_server(commandq, HOST, PORT)
        except KeyboardInterrupt:
            print("Connection server closed.")
            break
        except Exception as e:
            print(e)
            break
            #time.sleep(.5)
            #continue
    
def stream_data(command_queue, conn):
    """ Stream queue contents until finished# command
    """
    print('\n\t Streaming data...\n')
    while True:
        try:
            command = command_queue.get(block = True, timeout = 0.05)
            if type(command) == str:
                if(command == 'finished#'):
                    print('\n\t Streaming finished. Waiting for response...')
                    conn.recv(1024)
                    print('Success: response received.')
                    return
                elif(command[:5] == 'start'):
                    continue
                command = str.encode(command)
            conn.send(command)
        except Empty:
            continue
        except Exception as e:
            print('ERROR:' + e)
        

def run_server(command_queue, HOST, PORT):
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        # Initialize server
        s.setsockopt(socket.SOL_SOCKET, socket.SO_REUSEADDR, 1)
        s.bind((HOST, PORT))
        s.listen()

        bRunning = True
        # Listen for clients, if client fails wait for a new client.
        # Establish client connection:
        conn, addr = s.accept()
        with conn:
            print('Connected by', addr)
            # Serve a connection & wait for commands from Queue:
            while True:
                try:
                    command = command_queue.get(block=True, timeout=0.05)
                    if type(command) == str:
                        if(command[:5] == "start"):
                            stream_data(command_queue, conn)
                            print("success: finished streaming data.")
                            continue
                        command = str.encode(command)
                        print("\tCommand received %s" % str(command))
                    else:
                        conn.send(command)
                        print("Sent pickle data.")
                        conn.send(str.encode('None'))
                        print("Sent terminator.")
                        conn.recv(1024)
                        print("Success: Image model sent.")
                        continue
                    
                    conn.send(command)
                    print("Command sent success.")
                    print("Waiting on receive...")
                    response = conn.recv(1024)
                    print("received")
                    process_response(response, conn)
                except Empty:
                    continue
                except KeyboardInterrupt:
                    conn.close()
                    bRunning = False
                    break
                except Exception as e:
                    print(e)
                    conn.close()
                    break
        print('Connection with %s ended.' % str(addr))
        s.close()

def process_response(response, conn):
    print("Processed Response: %s" % str(response))
    response = response.decode('ASCII')
    commands = response.split('#')  # delimeter for args on functions
    try:
        print('Received command %s' % str(commands[0]))
        if commands[0] == 'end':
            return
        elif (commands[0] == 'input.ini'):
            return refresh_ini(commands[1])
        elif (commands[0] == 'streamoutput'):
            return stream_output(conn)
    except:
        return 'end'
    return 'end'

def refresh_ini(contents):
    try:
        file = open('input.ini', 'w')
        file.write(contents)
        print("Wrote input.ini file on server.")
    except:
        return

#if __name__ == '__main__':
#    main()
