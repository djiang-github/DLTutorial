from __future__ import print_function
import numpy as np
import sys
import os
import argparse


#################################################################################
# Install Keras if it is not avalaible.                                        #
#################################################################################

def install(package_name):
    import importlib
    try:
        importlib.import_module(package_name)
    except ImportError:
        import pip
        pip.main(['install', '--user', package_name])
        import site
        if sys.version_info < (3, 0):
            reload(site)
        elif sys.version_info < (3, 4):
            import imp
            imp.reload(site)
        else:
            importlib.reload(site)


if True:
    install('keras')


#################################################################################
# Keras configs.                                                                #
# Please refer to https://keras.io/backend .                                    #
#################################################################################

import keras
from keras import backend as K

#K.set_floatx('float32')
#String: 'float16', 'float32', or 'float64'.

#K.set_epsilon(1e-05)
#float. Sets the value of the fuzz factor used in numeric expressions.

#K.set_image_data_format('channels_first')
#data_format: string. 'channels_first' or 'channels_last'.


#################################################################################
# Keras imports.                                                                #
#################################################################################

from keras.models import Model
from keras.models import Sequential
from keras.layers import Input
from keras.layers import Lambda
from keras.layers import Layer
from keras.layers import Dense
from keras.layers import Dropout
from keras.layers import Activation
from keras.layers import Flatten
from keras.layers import Conv2D
from keras.layers import MaxPooling2D
from keras.optimizers import SGD
from keras.optimizers import RMSprop


###################################################################
# Variables                                                       #
# When launching project or scripts from Visual Studio,           #
# input_dir and output_dir are passed as arguments.               #
# Users could set them from the project setting page.             #
###################################################################

input_dir = None
output_dir = None
model_dir = None


def main():
    print('no problem!')
    exit(0)


if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--input_dir", help="Input directory where where training dataset and meta data are saved", required=False)
    parser.add_argument("--output_dir", help="Output directory where output such as logs are saved.", required=False)

    args, unknown = parser.parse_known_args()
    input_dir = args.input_dir
    output_dir = args.output_dir
    model_dir = output_dir

    main()
