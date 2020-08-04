from subprocess import Popen, CREATE_NEW_CONSOLE
import subprocess
import os
import gzip
import requests
import shutil
import pickle
from keras.utils import HDF5Matrix

# Source = https://github.com/nsadawi/Download-Large-File-From-Google-Drive-Using-Python
def download_file_from_google_drive(id, destination):
	URL = "https://docs.google.com/uc?export=download"

	session = requests.Session()

	response = session.get(URL, params = { 'id' : id }, stream = True)
	token = get_confirm_token(response)

	if token:
		params = { 'id' : id, 'confirm' : token }
		response = session.get(URL, params = params, stream = True)

	save_response_content(response, destination)    

def get_confirm_token(response):
	for key, value in response.cookies.items():
		if key.startswith('download_warning'):
			return value

	return None

def save_response_content(response, destination):
	CHUNK_SIZE = 32768

	with open(destination, "wb") as f:
		for chunk in response.iter_content(CHUNK_SIZE):
			if chunk: # filter out keep-alive new chunks
				f.write(chunk)


#Download PCAM Data

try:
	os.mkdir('Data/')
except:
	pass

print('Downloading [7+ GB] into Data/...')	
download_file_from_google_drive('1Ka0XfEMiwgCYPdTI-vv6eUElOBnKFKQ2', 'Data/camelyonpatch_level_2_split_train_x.h5.gz')
download_file_from_google_drive('1269yhu3pZDP8UYFQs-NYs3FPwuK-nGSG', 'Data/camelyonpatch_level_2_split_train_y.h5.gz')
download_file_from_google_drive('1qV65ZqZvWzuIVthK8eVDhIwrbnsJdbg_', 'Data/camelyonpatch_level_2_split_test_x.h5.gz')
download_file_from_google_drive('17BHrSrwWKjYsOgTMmoqrIjDy6Fa2o_gP', 'Data/camelyonpatch_level_2_split_test_y.h5.gz')

print('Decompressing Data files.')
for f in os.listdir('Data'):
	with gzip.open('Data/'+f, 'r') as f_in, open('Data/'+f[:-3], 'wb') as f_out:
			shutil.copyfileobj(f_in, f_out)

#Format data into pickle files:

x_train = HDF5Matrix('Data/camelyonpatch_level_2_split_train_x.h5', 'x')
y_train = HDF5Matrix('Data/camelyonpatch_level_2_split_train_y.h5', 'y')
x_test = HDF5Matrix('Data/camelyonpatch_level_2_split_test_x.h5', 'x')
y_test = HDF5Matrix('Data/camelyonpatch_level_2_split_test_y.h5', 'y')

num_datapoints = 3000 # Training & Validation (.25 split)
X = x_train[0:num_datapoints]
Y = [row[0][0][0] for row in y_train.data[0:num_datapoints]]

num_datapoints = 2000 # Testing
X_final = x_test[0:num_datapoints]
Y_final = [row[0][0][0] for row in y_test.data[0:num_datapoints]]

pickle_out = open("VR_DL_Data/Server/X_train.pickle", "wb")
pickle.dump(X, pickle_out)
pickle_out.close()

pickle_out = open("VR_DL_Data/Server/X_test.pickle", "wb")
pickle.dump(X_final, pickle_out)
pickle_out.close()

pickle_out = open("VR_DL_Data/Server/y_train.pickle", "wb")
pickle.dump(Y, pickle_out)
pickle_out.close()

pickle_out = open("VR_DL_Data/Server/y_test.pickle", "wb")
pickle.dump(Y_final, pickle_out)
pickle_out.close()

