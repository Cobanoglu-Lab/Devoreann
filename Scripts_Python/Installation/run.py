from subprocess import Popen, CREATE_NEW_CONSOLE
import subprocess
import os

os.chdir('VR_DL_Data')
Popen(['python', 'pyclient.py'], creationflags=CREATE_NEW_CONSOLE)
os.chdir('Server')
subprocess.call(['python', 'vrdl.py'])
