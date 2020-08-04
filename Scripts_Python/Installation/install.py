from subprocess import Popen, CREATE_NEW_CONSOLE
import subprocess
subprocess.call(['conda', 'env', 'create', '-f', 'VR_DL_Data/environment.yml'])
subprocess.run('conda activate VR_Env_GPU && python download_data.py', shell=True)
