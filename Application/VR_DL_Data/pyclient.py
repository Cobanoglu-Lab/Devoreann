"""  
Copyright © 2020, University of Texas Southwestern Medical Center. All rights reserved.
Contributors: Kevin VanHorn, Meyer Zinn, Murat Can Cobanoglu
Department: Lyda Hill Department of Bioinformatics.
This software and any related documentation constitutes published and/or unpublished works and may contain valuable trade secrets and proprietary information belonging to The University of Texas Southwestern Medical Center (UT SOUTHWESTERN).  None of the foregoing material may be copied, duplicated or disclosed without the express written permission of UT SOUTHWESTERN.  IN NO EVENT SHALL UT SOUTHWESTERN BE LIABLE TO ANY PARTY FOR DIRECT, INDIRECT, SPECIAL, INCIDENTAL, OR CONSEQUENTIAL DAMAGES, INCLUDING LOST PROFITS, ARISING OUT OF THE USE OF THIS SOFTWARE AND ITS DOCUMENTATION, EVEN IF UT SOUTHWESTERN HAS BEEN ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.  UT SOUTHWESTERN SPECIFICALLY DISCLAIMS ANY WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE. THE SOFTWARE AND ACCOMPANYING DOCUMENTATION, IF ANY, PROVIDED HEREUNDER IS PROVIDED "AS IS". UT SOUTHWESTERN HAS NO OBLIGATION TO PROVIDE MAINTENANCE, SUPPORT, UPDATES, ENHANCEMENTS, OR MODIFICATIONS.
This software contains copyrighted materials from Oculus, Unity Technologies, Keras, TensorFlow, gRPC, NumPy, Matplotlib, OpenCV, Pyprind, Nvidia CUDA, wiki.unity3d.com, PyCharm, Visual Studio Community, and Google. Corresponding terms and conditions apply.
"""

import socket
import time, pickle
from image_utils import plot_save_all, clear_texture_cache

HOST = '198.215.56.140'  # The server's hostname or IP address
PORT = 50052        # The port used by the server

#HOST = '127.0.0.1'
#PORT = 65432

def main():
    while True:
        try:
            run_client()
        except KeyboardInterrupt:
            print('Client Ended')
            return
        except:
            print('Server Closed, attempting to reconnect...')
            continue


def parse_command(data, conn):
    commands = []
    data = data.decode('ASCII')
    commands = data.split('#') # delimeter for args on functions
    try:
        print('Received command %s' % str(commands[0]))
        if(commands[0] == 'refresh_server'):
            return refresh_server_files()
        elif commands[0] == 'clear_texture_cache':
            return clear_texture_cache_internal()
        elif commands[0] == 'plot_save_all':
            if not commands[1] == 'pickle':
                print(commands[1])
            print('Waiting for pickled data...')
            data = get_pickled_data(conn)
            return plot_save_all_internal(data)
        elif commands[0] == 'stream':
            return get_data_stream(conn)
    except Exception as e:
        print(e)
        print('Command invalid - skipped.')
        return 'end'
    return 'end'


def run_client():
    with socket.socket(socket.AF_INET, socket.SOCK_STREAM) as s:
        bConnected = False
        while not bConnected:
            try:
                s.connect((HOST, PORT))
                bConnected = True
            except:
                print("Waiting for server...")
                time.sleep(10)

        print("Connected to server.")

        while(True):
            try:
                # Wait for commands
                #s.sendall(b'Hello, world')
                data = s.recv(1024) # waits for commands
                if not data: continue
                print('Received', repr(data))
                s.sendall(str.encode(parse_command(data, s)))
                print("Response sent.")
                time.sleep(0.2)
            except KeyboardInterrupt:
                break
            except Exception as e:
                print(e)
                break

def get_data_stream(conn):
    conn.sendall(str.encode(('streamoutput')))

    data = b""
    while True:
        packet = conn.recv(4096)
        print(packet)

    return 'end'

def refresh_server_files():
    input_str = ''
    try:
        file = open('input.ini', mode='r')
        input_str = file.read()
        print(input_str)
    except:
        return 'end'

    command_str = 'input.ini#' + input_str + '#' + 'end'
    return command_str

def clear_texture_cache_internal():
    clear_texture_cache()
    return 'end'

def plot_save_all_internal(activations):
    print('plot save all')
    plot_save_all(activations, len(activations)-1)
    return 'end'

def get_pickled_data(conn):
    conn.sendall(str.encode('end'))
    #data = conn.recv(4096)

    data = b""
    while True:
        packet = conn.recv(4096)
        #print(packet)
        if not packet: break
        if packet[-4:] == b'None':
            print('bNone found.')
            data += packet[:-4]
            break
        data += packet

    conn.sendall(str.encode('end'))
    print('loaded all')

    _data = pickle.loads(data)

    print('transformed pickled data.')
    return _data

if __name__ == '__main__':
    main()
